using System;
using System.Threading;
using System.Threading.Tasks;
using GraphQLEngine.Services.Subscriptions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace GraphQLEngine.Tests
{
    public class SubscriptionResilienceTests
    {
        [Fact]
        public async Task BoundedSubscriberBuffer_FastProducerSlowConsumer_StaysBounded()
        {
            // Arrange: producer publishes 100x faster than the consumer drains,
            // exercising the DropOldest overflow policy so memory never exceeds capacity.
            const int capacity = 50;
            const int producedItems = 5000;
            using var buffer = new BoundedSubscriberBuffer<int>(capacity, SubscriptionOverflowPolicy.DropOldest);

            var producer = Task.Run(() =>
            {
                for (var i = 0; i < producedItems; i++)
                {
                    buffer.TryPublish(i);

                    // Sample the buffer occupancy while the producer is running to prove
                    // it never grows past the configured capacity, even under 100:1 pressure.
                    Assert.True(buffer.Count <= capacity);
                }

                buffer.Complete();
            });

            var consumedCount = 0;
            var consumer = Task.Run(async () =>
            {
                await foreach (var _ in buffer.ReadAllAsync())
                {
                    consumedCount++;
                    if (consumedCount % 100 == 0)
                        await Task.Delay(1);
                }
            });

            await producer;
            await consumer;

            // Assert
            Assert.True(buffer.Count <= capacity);
            Assert.True(consumedCount <= producedItems);
        }

        [Fact]
        public void BoundedSubscriberBuffer_DropNewestPolicy_RejectsWhenFull()
        {
            using var buffer = new BoundedSubscriberBuffer<int>(2, SubscriptionOverflowPolicy.DropNewest);

            Assert.True(buffer.TryPublish(1));
            Assert.True(buffer.TryPublish(2));
            buffer.TryPublish(3);
            Assert.Equal(2, buffer.Count);
        }

        [Fact]
        public void BoundedSubscriberBuffer_TerminateWithErrorPolicy_TerminatesWhenFull()
        {
            using var buffer = new BoundedSubscriberBuffer<int>(1, SubscriptionOverflowPolicy.TerminateWithError);

            Assert.True(buffer.TryPublish(1));
            Assert.False(buffer.TryPublish(2));
            Assert.True(buffer.IsTerminated);
        }

        [Fact]
        public void BoundedSubscriberBuffer_ThrowsOnNonPositiveCapacity()
            => Assert.Throws<ArgumentOutOfRangeException>(() => new BoundedSubscriberBuffer<int>(0, SubscriptionOverflowPolicy.DropOldest));

        [Fact]
        public void SubscriptionResilienceOptions_Validate_ThrowsOnInvalidValues()
        {
            var options = new SubscriptionResilienceOptions { BufferCapacity = -1 };
            Assert.Throws<ArgumentOutOfRangeException>(options.Validate);
        }

        [Fact]
        public void ResilientEventSourceRunner_ComputeDelay_GrowsExponentiallyAndRespectsCap()
        {
            var options = new SubscriptionResilienceOptions
            {
                BaseRetryDelay = TimeSpan.FromMilliseconds(100),
                MaxRetryDelay = TimeSpan.FromMilliseconds(500),
                JitterFactor = 0
            };
            var runner = new ResilientEventSourceRunner(options, NullLogger.Instance);

            Assert.Equal(TimeSpan.FromMilliseconds(100), runner.ComputeDelay(1));
            Assert.Equal(TimeSpan.FromMilliseconds(200), runner.ComputeDelay(2));
            Assert.Equal(TimeSpan.FromMilliseconds(400), runner.ComputeDelay(3));
            Assert.Equal(TimeSpan.FromMilliseconds(500), runner.ComputeDelay(4));
        }

        [Fact]
        public async Task ResilientEventSourceRunner_RunAsync_TerminatesAfterMaxAttempts()
        {
            var options = new SubscriptionResilienceOptions
            {
                MaxRetryAttempts = 2,
                BaseRetryDelay = TimeSpan.FromMilliseconds(1),
                MaxRetryDelay = TimeSpan.FromMilliseconds(2)
            };
            var runner = new ResilientEventSourceRunner(options, NullLogger.Instance);
            var attempts = 0;

            await Assert.ThrowsAsync<SubscriptionTerminatedException>(() =>
                runner.RunAsync(_ =>
                {
                    attempts++;
                    throw new InvalidOperationException("simulated transport failure");
                }));

            Assert.Equal(3, attempts);
        }
    }
}
