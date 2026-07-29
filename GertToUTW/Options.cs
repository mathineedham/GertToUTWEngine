using CommandLine;
namespace GertToUTW;

public class Options
    {
    [Value(0, Required = true, HelpText = "Input log file path.")]
    public string InputPath { get; set; } = string.Empty;

    [Value(1, Required = true, HelpText = "Output directory path.")]
    public string OutputDirectory { get; set; } = string.Empty;

    [Option('l', "lot", Required = false, HelpText = "Given lot number")]
    public string LotNumber { get; set; } = string.Empty;
    }
