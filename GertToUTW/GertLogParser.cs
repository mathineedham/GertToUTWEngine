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

[assembly: InternalsVisibleTo("RegressionTests")]
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
    [GeneratedRegex(@"Step\s+(\d+):\s*\[(.*?)\]\s*\n([^\n\r]*)(?:\s*\n(.*?))?\s*\nResult:\s*(\S+)", RegexOptions.Singleline)]
    internal static partial Regex step_item_regex();

    /** @brief      Reads, isolates, maps, and structures complete session groups from a log payload.

        @param[in]  filepath   The explicit layout system access path to the target document.

        @return     A collection containing valid structural execution models.
    */
    public static List<TestRun> ParseGertLog( string filepath )
        {
        if( string.IsNullOrEmpty(filepath) )
            {
            throw new ArgumentNullException(nameof(filepath), "The provided file path cannot be null or empty.");
            }
        if (!File.Exists(filepath) )
            {
            throw new FileNotFoundException(null, filepath);
            }
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
                Result = new Result(result_raw) ,
                SequencerId = "GERT",
                StartTime = parse_date(extract_field(start_time_regex(), chunk)),
                EndTime = parse_date(extract_field(end_time_regex(), chunk)),
                SerialNumberAttributes = [new SerialNumberAttributes { SerialNumberAttributes_Key = 1, Name = "MACAdress", Value = mac_adress }],
                TestItem = test_items
                }.Find_link_phandle_step());
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
            throw new FormatException();
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
                test_items.Add(new TestItem(match));
                }
            }

        return test_items;
        }
    }