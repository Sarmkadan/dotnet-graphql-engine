#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;

namespace GraphQLEngine.Configuration;

/// <summary>
/// Parses command-line arguments for the GraphQL engine
/// Supports long and short options with validation
/// </summary>
sealed public class CliArgumentParser
{
    private readonly ILogger<CliArgumentParser> _logger;
    private readonly Dictionary<string, CliOption> _options;

    public CliArgumentParser(ILogger<CliArgumentParser> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = new Dictionary<string, CliOption>();
        InitializeDefaultOptions();
    }

    /// <summary>
    /// Parses command-line arguments
    /// </summary>
    public CliArguments Parse(string[] args)
    {
        _logger.LogInformation("Parsing CLI arguments: {ArgCount} arguments", args.Length);

        var result = new CliArguments();
        var i = 0;

        while (i < args.Length)
        {
            var arg = args[i];

            if (arg.StartsWith("--"))
            {
                // Long option
                var optionName = arg.Substring(2);
                i = ParseLongOption(optionName, args, i, result);
            }
            else if (arg.StartsWith("-") && arg.Length == 2)
            {
                // Short option
                var optionChar = arg[1].ToString();
                i = ParseShortOption(optionChar, args, i, result);
            }
            else
            {
                // Positional argument
                result.PositionalArgs.Add(arg);
                i++;
            }
        }

        ValidateArguments(result);
        _logger.LogInformation("CLI arguments parsed successfully");

        return result;
    }

    /// <summary>
    /// Parses a long option
    /// </summary>
    private int ParseLongOption(string optionName, string[] args, int index, CliArguments result)
    {
        var parts = optionName.Split('=');
        var name = parts[0];
        var value = parts.Length > 1 ? parts[1] : null;

        if (!_options.ContainsKey(name))
        {
            _logger.LogWarning("Unknown option: --{OptionName}", name);
            return index + 1;
        }

        var option = _options[name];

        if (option.RequiresValue && value is null)
        {
            if (index + 1 < args.Length && !args[index + 1].StartsWith("-"))
            {
                value = args[index + 1];
                index++;
            }
            else
            {
                throw new InvalidOperationException($"Option --{name} requires a value");
            }
        }

        result.Options[name] = new CliOptionValue { Name = name, Value = value };
        _logger.LogDebug("Parsed option: --{OptionName} = {Value}", name, value ?? "(flag)");

        return index + 1;
    }

    /// <summary>
    /// Parses a short option
    /// </summary>
    private int ParseShortOption(string shortName, string[] args, int index, CliArguments result)
    {
        var longName = FindLongOptionName(shortName);

        if (longName is null)
        {
            _logger.LogWarning("Unknown short option: -{ShortName}", shortName);
            return index + 1;
        }

        var option = _options[longName];
        string? value = null;

        if (option.RequiresValue)
        {
            if (index + 1 < args.Length && !args[index + 1].StartsWith("-"))
            {
                value = args[index + 1];
                index++;
            }
            else
            {
                throw new InvalidOperationException($"Option -{shortName} requires a value");
            }
        }

        result.Options[longName] = new CliOptionValue { Name = longName, Value = value };
        _logger.LogDebug("Parsed option: -{ShortName} ({LongName}) = {Value}", shortName, longName, value ?? "(flag)");

        return index + 1;
    }

    /// <summary>
    /// Finds the long option name for a short option
    /// </summary>
    private string? FindLongOptionName(string shortName)
    {
        return _options.FirstOrDefault(x => x.Value.ShortName == shortName).Key;
    }

    /// <summary>
    /// Validates parsed arguments
    /// </summary>
    private void ValidateArguments(CliArguments args)
    {
        foreach (var option in _options.Values)
        {
            if (option.IsRequired && !args.Options.ContainsKey(option.Name))
            {
                throw new InvalidOperationException($"Required option {option.Name} is missing");
            }
        }
    }

    /// <summary>
    /// Initializes default options
    /// </summary>
    private void InitializeDefaultOptions()
    {
        AddOption("help", "h", "Show help message", requiresValue: false, required: false);
        AddOption("version", "v", "Show version information", requiresValue: false, required: false);
        AddOption("config", "c", "Configuration file path", requiresValue: true, required: false);
        AddOption("port", "p", "Server port", requiresValue: true, required: false);
        AddOption("host", null, "Server host", requiresValue: true, required: false);
        AddOption("loglevel", "l", "Log level", requiresValue: true, required: false);
        AddOption("environment", "e", "Environment (dev/prod)", requiresValue: true, required: false);
        AddOption("schema", "s", "Schema file path", requiresValue: true, required: false);
    }

    /// <summary>
    /// Adds a custom option
    /// </summary>
    public void AddOption(string name, string? shortName, string description, bool requiresValue = false, bool required = false)
    {
        _options[name] = new CliOption
        {
            Name = name,
            ShortName = shortName,
            Description = description,
            RequiresValue = requiresValue,
            IsRequired = required
        };
    }

    /// <summary>
    /// Gets help text
    /// </summary>
    public string GetHelpText()
    {
        var sb = new System.Text.StringBuilder();

        sb.AppendLine("Usage: dotnet-graphql-engine [options]");
        sb.AppendLine();
        sb.AppendLine("Options:");

        foreach (var option in _options.Values.OrderBy(x => x.Name))
        {
            var optionStr = $"  --{option.Name}";
            if (option.ShortName is not null)
                optionStr += $", -{option.ShortName}";

            if (option.RequiresValue)
                optionStr += " <value>";

            sb.AppendLine(optionStr);
            sb.AppendLine($"    {option.Description}");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Gets version information
    /// </summary>
    public string GetVersionInfo()
    {
        return "dotnet-graphql-engine v1.0.0";
    }
}

/// <summary>
/// CLI option definition
/// </summary>
sealed public class CliOption
{
    public string Name { get; set; } = string.Empty;
    public string? ShortName { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool RequiresValue { get; set; }
    public bool IsRequired { get; set; }
}

/// <summary>
/// Parsed CLI arguments
/// </summary>
sealed public class CliArguments
{
    public Dictionary<string, CliOptionValue> Options { get; set; } = new();
    public List<string> PositionalArgs { get; set; } = new();

    public string? GetOptionValue(string optionName)
    {
        return Options.TryGetValue(optionName, out var value) ? value.Value : null;
    }

    public bool HasOption(string optionName)
    {
        return Options.ContainsKey(optionName);
    }

    public int GetPort(int defaultPort = 5000)
    {
        var port = GetOptionValue("port");
        return int.TryParse(port, out var p) ? p : defaultPort;
    }

    public string GetHost(string defaultHost = "localhost")
    {
        return GetOptionValue("host") ?? defaultHost;
    }

    public string GetEnvironment(string defaultEnv = "development")
    {
        return GetOptionValue("environment") ?? defaultEnv;
    }

    public string GetLogLevel(string defaultLevel = "Information")
    {
        return GetOptionValue("loglevel") ?? defaultLevel;
    }
}

/// <summary>
/// CLI option value
/// </summary>
sealed public class CliOptionValue
{
    public string Name { get; set; } = string.Empty;
    public string? Value { get; set; }
}
