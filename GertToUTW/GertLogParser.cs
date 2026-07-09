/** @file

    @copyright  &copy; 2024, Tria Technologies GmbH
                SPDX-License-Identifier: (GPL-2.0-or-later OR LGPL-2.1-or-later)

    @date       08.07.2026

    @author     Mathilde Needham (Mathilde.Needham@tria-technologies.com)

    @defgroup   REF_GertToUTWEngine_GertToUTW_GertLogParser   GertLogParser 
    @{
    @ingroup    REF_GertToUTWEngine_GertToUTW

    @brief      Provides utility mechanisms to read, interpret, and parse execution log files.

    @details    The `GertLogParser` class splits raw file contents into individual historical test data chunks, 
                extracting metadata attributes, tracking evaluation criteria, and translating execution timelines 
                into serializable structural objects.
    @}
*/
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

[assembly: InternalsVisibleTo("RegressionTest")]
namespace GertToUTW;

/** @ingroup    REF_GertToUTWEngine_GertToUTW_GertLogParser

    @class      GertLogParser

    @brief      Static parsing utility wrapper for unstructured textual logs.

    @details    Exposes specialized optimization rules driven by source-generated regular expressions to match 
                and parse custom log variables, handling step iterations, standard error captures, and layout mappings.
*/
public static partial class GertLogParser
    {
    /** @brief      Matches the starting environmental network host identifier. */
    [GeneratedRegex(@"GERT started on host:\s*(\S+)")]
    internal static partial Regex computer_name_regex();
    /** @brief      Splits individual log report execution records safely. */
    [GeneratedRegex(@"(?=GERT started on host:)")]
    internal static partial Regex report_split_regex();
    /** @brief      Matches the execution initialization timeline start flag. */
    [GeneratedRegex(@"\[TestRunStart =\s*([^\]]+)\]")]
    internal static partial Regex start_time_regex();
    /** @brief      Matches the associated operator validation credential marker. */
    [GeneratedRegex(@"\[OPER=\s*([^\]]+)\]")]
    internal static partial Regex operator_name_regex();
    /** @brief      Extracts numerical product schematic indices. */
    [GeneratedRegex(@"Script\s*=\s*(\d+)")]
    internal static partial Regex material_number_regex();
    /** @brief      Matches plain-text descriptive product information strings. */
    [GeneratedRegex(@"\[Product =\s*([^\]]+)\]")]
    internal static partial Regex material_text_regex();
    /** @brief      Matches alpha-numeric engineering design index modifications. */
    [GeneratedRegex(@"\[Revision =\s*([^\]]+)\]")]
    internal static partial Regex material_revision_regex();
    /** @brief      Matches individual target physical asset serial markings. */
    [GeneratedRegex(@"\[BoardID =\s*([^\]]+)\]")]
    internal static partial Regex serial_number_regex();
    /** @brief      Extracts final evaluation context verdict indicators. */
    [GeneratedRegex(@"\[ScriptResult =\s*([^\]]+)\]")]
    internal static partial Regex result_regex();
    /** @brief      Matches the absolute structural termination timestamp metadata flag. */
    [GeneratedRegex(@"\[TestRunEnd =\s*([^\]]+)\]")]
    internal static partial Regex end_time_regex();
    /** @brief      Extracts peripheral hardware network adapter identifier codes. */
    [GeneratedRegex(@"MACAddress1:\s*(\S+)")]
    internal static partial Regex mac_address_regex();
    /** @brief      Captures core body segments containing isolated step-by-step arrays. */
    [GeneratedRegex(@"\[LogData = Start\](.*?)\[LogData = End\]", RegexOptions.Singleline)]
    internal static partial Regex log_data_regex();
    /** @brief      Splits raw execution blocks into discrete functional milestones. */
    [GeneratedRegex(@"-{20,}\s*(?=Step\s+\d+:|INFO::CloseTestLog)")]
    internal static partial Regex steps_split_regex();
    /** @brief      Validates internal parameters, descriptions, variables, and output scopes. */
    [GeneratedRegex(@"Step\s+(\d+):\s*\[(.*?)\]\s*((?:INFO::FillVariables|INFO::ActionSteps).*?)\s*\n(?:.*?INFO::FillVariables.*?: SET .*?\n)*\s*(.*?)Result:\s*(\S+)", RegexOptions.Singleline)]
    internal static partial Regex step_item_regex();
    /** @brief      Internal static key translation reference map for input abbreviations. */
    internal static readonly Dictionary<string, string> theResultRules = new(StringComparer.Ordinal)
    {
        { "PASS", "PASSED" },
        { "FAIL", "FAILED" },
        { "SKIP", "SKIPPED" }
    };

    /** @brief      Reads, isolates, maps, and structures complete session groups from a log payload.

        @param[in]  filepath   The explicit layout system access path to the target document.

        @return     A collection containing valid structural execution models.
    */
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

            string mac_adress = extract_field(mac_address_regex(), chunk);

            test_runs.Add(new TestRun
                {
                TestRun_Key = 1,
                ComputerName = extract_field(computer_name_regex(), chunk),
                OperatorName = extract_field(operator_name_regex(), chunk),
                MaterialNumber = extract_field(material_number_regex(), chunk),
                MaterialText = extract_field(material_text_regex(), chunk),
                MaterialRevision = extract_field(material_revision_regex(), chunk),
                SerialNumber = extract_field(serial_number_regex(), chunk),
                Result = new Result { Value = map_result(result_raw) },
                SequencerId = "GERT",
                StartTime = parse_date(extract_field(start_time_regex(), chunk)),
                EndTime = parse_date(extract_field(end_time_regex(), chunk)),
                SerialNumberAttributes = build_serial_attributes([("MACAdress",mac_adress)]),
                TestItem = test_items
                });
            }

        return test_runs;
        }

    /** @brief      Safely evaluates and captures targeted matching capturing groups.

        @param[in]  regex     The specific analytical expression rule mapping context.
        @param[in]  text      The raw source text segment to scan.
        @param[in]  default   Fallback context value used when matching groups fail.

        @return     The extracted matched string chunk value or the requested default.
    */
    internal static string extract_field( Regex regex, string text, string @default = "" )
        {
        Match match = regex.Match(text);
        return match.Success ? match.Groups[1].Value.Trim() : @default;
        }

    /** @brief      Normalizes incoming status variations into standard validation strings.

        @param[in]  raw_result   The target unstructured log string status entry.

        @return     The corresponding upper-case tracking state outcome token.
    */
    internal static string map_result( string raw_result )
        {
        return theResultRules.TryGetValue(raw_result, out string? transformed) ? transformed : raw_result;
        }

    /** @brief      Parses specific standard logging calendar strings into concrete DateTime instances.

        @param[in]  date_str   The raw text configuration segment tracking time.

        @return     The converted object timestamp data snapshot, or system runtime fallback `Now`.
    */
    internal static DateTime parse_date( string date_str )
        {
        if( DateTime.TryParseExact(date_str, "dd.MM.yyyy 'at' HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsed_date) )
            {
            return parsed_date;
            }

        throw new FormatException(date_str);
        }

    /** @brief      Constructs peripheral identifier containers around physical device logs.

    @param[in]  attributes   A dictionary containing the identifier names and their corresponding values.

    @return     An explicit relational layout tracking entry list.
*/
    internal static List<SerialNumberAttributes> build_serial_attributes( List<(string, string)> attributes )
        {
        return [.. attributes.Select(attr => new SerialNumberAttributes { SerialNumberAttributes_Key = 1,Name = attr.Item1, Value = attr.Item2 })];
        }

    /** @brief      Isolates step records embedded directly inside structural file body payloads.

        @param[in]  content   The overall multi-line log container layout block.

        @return     An isolated array listing sub-milestone object definitions.
    */
    internal static List<TestItem> parse_test_items( string content )
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

    /** @brief      Transforms an internal regex tracking capture match into a structural object instance.

        @param[in]  match   The analytical capturing map tracker containing parsed indices.

        @return     An initialized structural item definition.
    */
    internal static TestItem build_test_item( Match match )
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
            Result = new Result { Value = result_raw },
            Stdout = stdout_val,
            Stderr = stderr_val
            };
        }
    }