
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace GertToUTW;

public static partial class GertLogParser
    {
    // Regex Rules
    [GeneratedRegex(@"GERT started on host:\s*(\S+)")]
    private static partial Regex computer_name_regex();

    [GeneratedRegex(@"(?=GERT started on host:)")]
    private static partial Regex report_split_regex();

    [GeneratedRegex(@"\[TestRunStart =\s*([^\]]+)\]")]
    private static partial Regex start_time_regex();

    [GeneratedRegex(@"\[OPER=\s*([^\]]+)\]")]
    private static partial Regex operator_name_regex();

    [GeneratedRegex(@"Script\s*=\s*(\d+)")]
    private static partial Regex material_number_regex();

    [GeneratedRegex(@"\[Product =\s*([^\]]+)\]")]
    private static partial Regex material_text_regex();

    [GeneratedRegex(@"\[Revision =\s*([^\]]+)\]")]
    private static partial Regex material_revision_regex();

    [GeneratedRegex(@"\[BoardID =\s*([^\]]+)\]")]
    private static partial Regex serial_number_regex();

    [GeneratedRegex(@"\[ScriptResult =\s*([^\]]+)\]")]
    private static partial Regex result_regex();

    [GeneratedRegex(@"\[TestRunEnd =\s*([^\]]+)\]")]
    private static partial Regex end_time_regex();

    [GeneratedRegex(@"MACAddress1:\s*(\S+)")]
    private static partial Regex mac_address_regex();

    [GeneratedRegex(@"\[LogData = Start\](.*?)\[LogData = End\]", RegexOptions.Singleline)]
    private static partial Regex log_data_regex();

    [GeneratedRegex(@"-{20,}\s*(?=Step\s+\d+:|INFO::CloseTestLog)")]
    private static partial Regex steps_split_regex();

    [GeneratedRegex(@"Step\s+(\d+):\s*\[(.*?)\]\s*((?:INFO::FillVariables|INFO::ActionSteps).*?)\s*\n(?:.*?INFO::FillVariables.*?: SET .*?\n)*\s*(.*?)Result:\s*(\S+)", RegexOptions.Singleline)]
    private static partial Regex step_item_regex();

    private static readonly Dictionary<string, string> theResultRules = new(StringComparer.Ordinal)
    {
        { "PASS", "PASSED" },
        { "FAIL", "FAILED" },
        { "SKIP", "SKIPPED" }
    };

    public static List<TestRun> ParseGertLog( string filepath )
        {
        string content = File.ReadAllText(filepath, Encoding.UTF8);
        List<TestRun> test_runs = [];

        // Split the file contents by individual log sessions
        string[] report_chunks = report_split_regex().Split(content);

        foreach( string chunk in report_chunks )
            {
            if( string.IsNullOrWhiteSpace(chunk) )
                {
                continue;
                }
            if( !chunk.Contains("GERT started on host:") )
                {
                continue;
                }

            Console.WriteLine( chunk.Length ); 

            string result_raw = extract_field(result_regex(), chunk);
            List<TestItem> test_items = parse_test_items(chunk);

            test_runs.Add(new TestRun
                {
                TestRun_Key = 1,
                ComputerName = extract_field(computer_name_regex(), chunk),
                OperatorName = extract_field(operator_name_regex(), chunk),
                MaterialNumber = extract_field(material_number_regex(), chunk),
                MaterialText = extract_field(material_text_regex(), chunk),
                MaterialRevision = extract_field(material_revision_regex(), chunk),
                SerialNumber = extract_field(serial_number_regex(), chunk),
                Result = map_result(result_raw),
                SequencerId = "GERT",
                StartTime = parse_date(extract_field(start_time_regex(), chunk)),
                EndTime = parse_date(extract_field(end_time_regex(), chunk)),
                SerialNumberAttributes = build_serial_attributes(extract_field(mac_address_regex(), chunk)),
                TestItem = test_items
                });
            }

        return test_runs;
        }

    private static string extract_field( Regex regex, string text, string @default = "" )
        {
        Match match = regex.Match(text);
        return match.Success ? match.Groups[1].Value.Trim() : @default;
        }

    private static string map_result( string raw_result )
        {
        return theResultRules.TryGetValue(raw_result, out string? transformed) ? transformed : raw_result;
        }

    private static DateTime parse_date( string date_str )
        {
        if( DateTime.TryParseExact(date_str, "dd.MM.yyyy 'at' HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsed_date) )
            {
            return parsed_date;
            }
        return DateTime.Now;
        }

    private static List<SerialNumberAttributes> build_serial_attributes( string mac_address )
        {
        return [new() { SerialNumberAttributes_Key = 1, Name = "MACAdress", Value = mac_address }];
        }

    private static List<TestItem> parse_test_items( string content )
        {
        List<TestItem> test_items = [];
        Match log_data_match = log_data_regex().Match(content);

        if( !log_data_match.Success )
            {
            return test_items;
            }

        string log_body = log_data_match.Groups[1].Value;
        string[] steps_raw = steps_split_regex().Split(log_body);

        foreach( string raw_chunk in steps_raw )
            {
            string step_chunk = raw_chunk.Trim();
            if( !step_chunk.StartsWith("Step", StringComparison.Ordinal) )
                {
                continue;
                }

            Match match = step_item_regex().Match(step_chunk);
            if( match.Success )
                {
                test_items.Add(build_test_item(match));
                }
            }

        return test_items;
        }

    private static TestItem build_test_item( Match match )
        {
        string result_raw = map_result(match.Groups[5].Value.Trim());
        string middle_string = match.Groups[4].Value.Trim();

        string? stdout_val = null;
        string? stderr_val = null;

        if( !string.IsNullOrEmpty(middle_string) )
            {
            if( result_raw == "FAILED" )
                {
                stderr_val = middle_string;
                }
            else
                {
                stdout_val = middle_string;
                }
            }

        return new TestItem
            {
            TestItem_Key = 1,
            Idx = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture),
            Name = match.Groups[2].Value.Trim(),
            Description = match.Groups[3].Value.Trim(),
            Result = result_raw,
            Stdout = stdout_val,
            Stderr = stderr_val
            };
        }
    }