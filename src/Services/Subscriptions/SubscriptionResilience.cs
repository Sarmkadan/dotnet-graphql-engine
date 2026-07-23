#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using GraphQLEngine.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace GraphQLEngine.Services.Subscriptions;

/// <summary>
/// Determines what happens when a subscriber's bounded buffer is full
/// and the producer publishes another item.
/// </summary>
public enum SubscriptionOverflowPolicy
{
    /// <summary>Drop the oldest buffered item to make room for the new one.</summary>
    DropOldest = 0,

    /// <summary>Drop the incoming item and keep the buffered ones.</summary>
    DropNewest = 1,

    /// <summary>Terminate the stream with a GraphQL error signalling the client is too slow.</summary>
    TerminateWithError = 2
}

/// <summary>
/// Configuration key names for subscription resilience settings.
/// </summary>
public static class ConfigurationKeys
{
    /// <summary>Key for the per-subscriber buffer capacity (int, items).</summary>
    public const string SubscriptionBufferCapacity = "GraphQLEngine:Subscriptions:BufferCapacity";

    /// <summary>Key for the buffer overflow policy (<see cref="SubscriptionOverflowPolicy"/> name).</summary>
    public const string SubscriptionOverflowPolicy = "GraphQLEngine:Subscriptions:OverflowPolicy";

    /// <summary>Key for the maximum number of event-source reconnect attempts (int).</summary>
    public const string SubscriptionMaxRetryAttempts = "GraphQLEngine:Subscriptions:MaxRetryAttempts";

    /// <summary>Key for the initial retry delay in milliseconds (int).</summary>
    public const string SubscriptionBaseRetryDelayMs = "GraphQLEngine:Subscriptions:BaseRetryDelayMs";

    /// <summary>Key for the maximum retry delay in milliseconds (int).</summary>
    public const string SubscriptionMaxRetryDelayMs = "GraphQLEngine:Subscriptions:MaxRetryDelayMs";

    /// <summary>Key for the retry jitter factor, 0..1 (double).</summary>
    public const string SubscriptionRetryJitterFactor = "GraphQLEngine:Subscriptions:RetryJitterFactor";
}

/// <summary>
/// Settings that control per-subscriber buffering and event-source retry behavior.
/// </summary>
sealed public class SubscriptionResilienceOptions
{
    /// <summary>Maximum number of items buffered per subscriber before the overflow policy applies.</summary>
    public int BufferCapacity { get; set; } = 256;

    /// <summary>Policy applied when the buffer is full.</summary>
    public SubscriptionOverflowPolicy OverflowPolicy { get; set; } = SubscriptionOverflowPolicy.DropOldest;

    /// <summary>Maximum reconnect attempts before the stream terminates with a GraphQL error.</summary>
    public int MaxRetryAttempts { get; set; } = 5;

    /// <summary>Delay before the first retry; doubles on each subsequent attempt.</summary>
    public TimeSpan BaseRetryDelay { get; set; } = TimeSpan.FromMilliseconds(250);

    /// <summary>Upper bound for the exponential backoff delay.</summary>
    public TimeSpan MaxRetryDelay { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>Fraction (0..1) of the computed delay randomized as jitter.</summary>
    public double JitterFactor { get; set; } = 0.2;

    /// <summary>
    /// Validates the option values.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when any value is outside its valid range.</exception>
    public void Validate()
    {
        if (BufferCapacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(BufferCapacity), BufferCapacity, "Buffer capacity must be greater than zero.");
        if (MaxRetryAttempts < 0)
            throw new ArgumentOutOfRangeException(nameof(MaxRetryAttempts), MaxRetryAttempts, "Max retry attempts cannot be negative.");
        if (BaseRetryDelay <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(BaseRetryDelay), BaseRetryDelay, "Base retry delay must be positive.");
        if (MaxRetryDelay < BaseRetryDelay)
            throw new ArgumentOutOfRangeException(nameof(MaxRetryDelay), MaxRetryDelay, "Max retry delay must be at least the base retry delay.");
        if (JitterFactor is < 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(JitterFactor), JitterFactor, "Jitter factor must be between 0 and 1.");
    }

    /// <summary>
    /// Binds options from an <see cref="IConfiguration"/> using the <see cref="ConfigurationKeys"/> names.
    /// Missing keys keep their defaults.
    /// </summary>
    /// <param name="configuration">Configuration source to read from.</param>
    /// <returns>A validated options instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="configuration"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a bound value is outside its valid range.</exception>
    /// <exception cref="FormatException">Thrown when a configuration value cannot be parsed.</exception>
    public static SubscriptionResilienceOptions FromConfiguration(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var options = new SubscriptionResilienceOptions();

        if (configuration[ConfigurationKeys.SubscriptionBufferCapacity] is { } capacity)
            options.BufferCapacity = int.Parse(capacity);
        if (configuration[ConfigurationKeys.SubscriptionOverflowPolicy] is { } policy)
            options.OverflowPolicy = Enum.Parse<SubscriptionOverflowPolicy>(policy, ignoreCase: true);
        if (configuration[ConfigurationKeys.SubscriptionMaxRetryAttempts] is { } attempts)
            options.MaxRetryAttempts = int.Parse(attempts);
        if (configuration[ConfigurationKeys.SubscriptionBaseRetryDelayMs] is { } baseDelay)
            options.BaseRetryDelay = TimeSpan.FromMilliseconds(int.Parse(baseDelay));
        if (configuration[ConfigurationKeys.SubscriptionMaxRetryDelayMs] is { } maxDelay)
            options.MaxRetryDelay = TimeSpan.FromMilliseconds(int.Parse(maxDelay));
        if (configuration[ConfigurationKeys.SubscriptionRetryJitterFactor] is { } jitter)
            options.JitterFactor = double.Parse(jitter, System.Globalization.CultureInfo.InvariantCulture);

        options.Validate();
        return options;
    }
}

/// <summary>
/// Exception raised when a subscription stream is terminated by the engine,
/// either because a slow consumer overflowed its buffer or because the
/// event source failed permanently after exhausting retries.
/// </summary>
sealed public class SubscriptionTerminatedException : GraphQLException
{
    /// <summary>Error code for a buffer overflow termination.</summary>
    public const string BufferOverflowCode = "SUBSCRIPTION_BUFFER_OVERFLOW";

    /// <summary>Error code for an event-source failure termination.</summary>
    public const string EventSourceFailedCode = "SUBSCRIPTION_EVENT_SOURCE_FAILED";

    /// <summary>
    /// Initializes the exception with a message and error code.
    /// </summary>
    /// <param name="message">Human-readable description of the termination.</param>
    /// <param name="errorCode">Machine-readable error code.</param>
    public SubscriptionTerminatedException(string message, string errorCode)
        : base(message, errorCode)
    {
    }

    /// <summary>
    /// Initializes the exception with a message, error code and the causing exception.
    /// </summary>
    /// <param name="message">Human-readable description of the termination.</param>
    /// <param name="errorCode">Machine-readable error code.</param>
    /// <param name="innerException">The underlying failure.</param>
    public SubscriptionTerminatedException(string message, string errorCode, Exception innerException)
        : base(message, innerException)
        => ErrorCode = errorCode;
}

/// <summary>
/// A bounded per-subscriber buffer backed by a <see cref="Channel{T}"/>.
/// Guarantees that a slow consumer never holds more than
/// <see cref="SubscriptionResilienceOptions.BufferCapacity"/> items in memory.
/// </summary>
/// <typeparam name="T">The type of items delivered to the subscriber.</typeparam>
sealed public class BoundedSubscriberBuffer<T> : IDisposable
{
    private readonly Channel<T> _channel;
    private readonly SubscriptionOverflowPolicy _policy;
    private readonly int _capacity;
    private long _droppedCount;
    private volatile bool _terminated;

    /// <summary>
    /// Creates a bounded buffer.
    /// </summary>
    /// <param name="capacity">Maximum number of buffered items.</param>
    /// <param name="policy">Policy applied when the buffer is full.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="capacity"/> is not positive.</exception>
    public BoundedSubscriberBuffer(int capacity, SubscriptionOverflowPolicy policy)
    {
        if (capacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "Capacity must be greater than zero.");

        _capacity = capacity;
        _policy = policy;
        _channel = Channel.CreateBounded<T>(new BoundedChannelOptions(capacity)
        {
            FullMode = policy switch
            {
                SubscriptionOverflowPolicy.DropOldest => BoundedChannelFullMode.DropOldest,
                SubscriptionOverflowPolicy.DropNewest => BoundedChannelFullMode.DropWrite,
                _ => BoundedChannelFullMode.Wait
            },
            SingleReader = true,
            SingleWriter = false
        });
    }

    /// <summary>Maximum number of items this buffer can hold.</summary>
    public int Capacity => _capacity;

    /// <summary>Number of items currently buffered; never exceeds <see cref="Capacity"/>.</summary>
    public int Count => _channel.Reader.Count;

    /// <summary>Total number of items dropped due to overflow.</summary>
    public long DroppedCount => Interlocked.Read(ref _droppedCount);

    /// <summary>Whether the buffer has been terminated and accepts no more items.</summary>
    public bool IsTerminated => _terminated;

    /// <summary>
    /// Offers an item to the buffer, applying the configured overflow policy when full.
    /// </summary>
    /// <param name="item">The item to buffer.</param>
    /// <returns>
    /// True when the item was accepted (possibly displacing an older one);
    /// false when it was dropped or the buffer is terminated.
    /// </returns>
    public bool TryPublish(T item)
    {
        if (_terminated)
            return false;

        if (_policy == SubscriptionOverflowPolicy.TerminateWithError && _channel.Reader.Count >= _capacity)
        {
            Terminate(new SubscriptionTerminatedException(
                $"Subscriber buffer overflow: consumer fell more than {_capacity} items behind the producer.",
                SubscriptionTerminatedException.BufferOverflowCode));
            return false;
        }

        var written = _channel.Writer.TryWrite(item);
        if (!written || _policy == SubscriptionOverflowPolicy.DropOldest && _channel.Reader.Count >= _capacity)
        {
            // DropWrite rejects the incoming item; DropOldest silently displaced one.
            if (!written)
                Interlocked.Increment(ref _droppedCount);
        }

        return written;
    }

    /// <summary>
    /// Completes the buffer normally: buffered items remain readable, then the stream ends.
    /// </summary>
    public void Complete()
    {
        _terminated = true;
        _channel.Writer.TryComplete();
    }

    /// <summary>
    /// Terminates the buffer with an error that the consumer observes after draining buffered items.
    /// </summary>
    /// <param name="error">The terminal error.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="error"/> is null.</exception>
    public void Terminate(Exception error)
    {
        ArgumentNullException.ThrowIfNull(error);
        _terminated = true;
        _channel.Writer.TryComplete(error);
    }

    /// <summary>
    /// Reads all items as an async stream until the buffer completes or terminates.
    /// </summary>
    /// <param name="cancellationToken">Cancels the enumeration.</param>
    /// <returns>The buffered items in order.</returns>
    /// <exception cref="SubscriptionTerminatedException">
    /// Thrown by the enumeration when the buffer was terminated with an error.
    /// </exception>
    public IAsyncEnumerable<T> ReadAllAsync(CancellationToken cancellationToken = default)
        => _channel.Reader.ReadAllAsync(cancellationToken);

    /// <inheritdoc />
    public void Dispose() => Complete();
}

/// <summary>
/// Runs a subscription event source with exponential backoff and jitter,
/// reconnecting on failure up to a configurable number of attempts before
/// terminating the subscriber's stream with a GraphQL error.
/// </summary>
sealed public class ResilientEventSourceRunner
{
    private readonly SubscriptionResilienceOptions _options;
    private readonly ILogger _logger;
    private readonly Func<int, TimeSpan, CancellationToken, Task> _delay;

    /// <summary>
    /// Creates a runner with the given options.
    /// </summary>
    /// <param name="options">Retry configuration.</param>
    /// <param name="logger">Logger for retry diagnostics.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="options"/> or <paramref name="logger"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="options"/> contains invalid values.</exception>
    public ResilientEventSourceRunner(SubscriptionResilienceOptions options, ILogger logger)
        : this(options, logger, static (_, delay, ct) => Task.Delay(delay, ct))
    {
    }

    internal ResilientEventSourceRunner(
        SubscriptionResilienceOptions options,
        ILogger logger,
        Func<int, TimeSpan, CancellationToken, Task> delay)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(delay);
        options.Validate();

        _options = options;
        _logger = logger;
        _delay = delay;
    }

    /// <summary>
    /// Computes the backoff delay for a given retry attempt (1-based),
    /// applying exponential growth, the configured cap, and random jitter.
    /// </summary>
    /// <param name="attempt">The 1-based retry attempt number.</param>
    /// <returns>The delay to wait before the attempt.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="attempt"/> is less than 1.</exception>
    public TimeSpan ComputeDelay(int attempt)
    {
        if (attempt < 1)
            throw new ArgumentOutOfRangeException(nameof(attempt), attempt, "Attempt must be at least 1.");

        var exponentialMs = _options.BaseRetryDelay.TotalMilliseconds * Math.Pow(2, attempt - 1);
        var cappedMs = Math.Min(exponentialMs, _options.MaxRetryDelay.TotalMilliseconds);
        var jitterSpanMs = cappedMs * _options.JitterFactor;
        var jitteredMs = cappedMs - jitterSpanMs + Random.Shared.NextDouble() * jitterSpanMs * 2;
        return TimeSpan.FromMilliseconds(Math.Min(jitteredMs, _options.MaxRetryDelay.TotalMilliseconds));
    }

    /// <summary>
    /// Runs <paramref name="connectAndPump"/> until it completes successfully,
    /// retrying failures with exponential backoff and jitter. When retries are
    /// exhausted the failure is wrapped in a <see cref="SubscriptionTerminatedException"/>
    /// and thrown so the caller can terminate the subscriber's stream.
    /// </summary>
    /// <param name="connectAndPump">
    /// Delegate that connects to the event source and pumps events; it should
    /// return normally when the stream ends and throw on transport failure.
    /// </param>
    /// <param name="cancellationToken">Cancels the run, including backoff waits.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="connectAndPump"/> is null.</exception>
    /// <exception cref="OperationCanceledException">Thrown when <paramref name="cancellationToken"/> is cancelled.</exception>
    /// <exception cref="SubscriptionTerminatedException">Thrown when all retry attempts are exhausted.</exception>
    public async Task RunAsync(Func<CancellationToken, Task> connectAndPump, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(connectAndPump);

        var attempt = 0;
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                await connectAndPump(cancellationToken).ConfigureAwait(false);
                return;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                attempt++;
                if (attempt > _options.MaxRetryAttempts)
                {
                    _logger.LogError(ex,
                        "Subscription event source failed permanently after {Attempts} attempts", attempt);
                    throw new SubscriptionTerminatedException(
                        $"Subscription event source failed after {attempt} attempts.",
                        SubscriptionTerminatedException.EventSourceFailedCode,
                        ex);
                }

                var delay = ComputeDelay(attempt);
                _logger.LogWarning(ex,
                    "Subscription event source failed (attempt {Attempt}/{Max}); retrying in {Delay}ms",
                    attempt, _options.MaxRetryAttempts, (int)delay.TotalMilliseconds);
                await _delay(attempt, delay, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
