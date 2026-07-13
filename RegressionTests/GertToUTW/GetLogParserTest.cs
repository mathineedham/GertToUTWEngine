/** @file       GertLogParserTests.cs

    @copyright  &copy; 2026, Tria Technologies GmbH
                SPDX-License-Identifier: (GPL-2.0-or-later OR LGPL-2.1-or-later)

    @date       09.07.2026

    @author     Mathilde Needham (Mathilde.Needham@tria-technologies.com)
 
    @defgroup   REF_GertToUTWEngine_RegressionTest_GertToUTW_GerLogParserTest   GerLogParserTest
    @{
    @ingroup    REF_GertToUTWEngine_RegressionTest_GertToUTW

    @brief      Provides utility mechanisms to read, interpret, and parse execution log files.

    @details    The `GertLogParser` class splits raw file contents into individual historical test data chunks, 
                extracting metadata attributes, tracking evaluation criteria, and translating execution timelines 
                into serializable structural objects.
    @}
*/
using System.Text.RegularExpressions;

using GertToUTW;

namespace RegressionTest.GertToUTW;


/** @class      TestParseDateTests
    @ingroup    REF_GertToUTWEngine_RegressionTest_GertToUTW_GerLogParserTest

    @brief      Unit tests for the 'parse_date' method of the `GertLogParser` class.

    @details    Verifies that the `parse_date` method correctly interprets various date string formats, 
                handles edge cases, and throws appropriate exceptions for invalid inputs.
*/
[TestClass]
public sealed class ParseDateTests
    {
    /** @brief Verifies that a correctly formatted date string parses into the exact expected DateTime object. */
    [TestMethod]
    public void Parse_date_ValidFormat()
        {
        string valid_date_text = "09.07.2026 at 14:30:25";
        DateTime expected = new(2026, 7, 9, 14, 30, 25);
        DateTime result = GertLogParser.parse_date(valid_date_text);

        Assert.AreEqual(expected, result);
        }

    /** @brief Verifies that any format mismatch causes a FormatException with the raw string embedded as the message. */
    [TestMethod]
    [DataRow("2026-07-09 14:30:25")]
    [DataRow("09/07/2026 at 14h30m25s")]
    public void Parse_date_InvalidFormat( string malformed_date_text )
        {
        try
            {
            _ = GertLogParser.parse_date(malformed_date_text);
            Assert.Fail("Expected parse_date to throw a FormatException, but it completed successfully.");
            }
        catch( FormatException ex )
            {
            Assert.AreEqual(malformed_date_text, ex.Message, "The exception message must exactly equal the invalid input string.");
            }
        }
    }

/** @class      TestExtractDateTests
    @ingroup    REF_GertToUTWEngine_RegressionTest_GertToUTW_GerLogParserTest

    @brief      Unit tests for the 'extract_field' method of the `GertLogParser` class.

    @details    Verifies that the `extract_field` method correctly identifies and extracts date fields from log text, 
                handles edge cases, and throws appropriate exceptions for invalid inputs.
*/
[TestClass]
public sealed partial class ExtractFieldTests
    {
    /** @brief Verifies that a correctly formatted date field is extracted from the log text. 
     * param[in]  expr A regex pattern to identify the date field in the log text.
     * param[in]  log_text   A string containing a valid date field in the expected format.
     * param[in]  expected_date   The DateTime object that should result from parsing the extracted field.
     * param[in]  explicit_default A string representing the default value to use if the field is not found; should be null if not given
     */
    [TestMethod]
    // Success cases with single and multiple values
    [DataRow(@"Key\s*=\s*(.*)", "Some header info\nKey =   SRNUM-999X   \nSome footer info", "SRNUM-999X", null)]
    [DataRow(@"Key\s*=\s*(.*),\s*(.*)", "Header\nKey =   Value-1,   Value-2   \nFooter", "Value-1", null)]
    // No match cases with and without explicit default values
    [DataRow(@"Key\s*=\s*(.*)", "Missing the target key entirely", "UNKNOWN_PRODUCT", "UNKNOWN_PRODUCT")]
    [DataRow(@"Key\s*=\s*(.*)", "Missing the target key entirely", "", null)]
    public void Extract_Field_Tests( string expr, string log_text, string expected_value, string explicit_default)
        {
        Regex r = new(expr);
        string result;
        if( explicit_default != null )
            {
            result = GertLogParser.extract_field(r, log_text, explicit_default);
            }
        else
            {
            result = GertLogParser.extract_field(r, log_text);
            }
        Assert.AreEqual(expected_value, result, $"Failed for test case: {r} with input '{log_text}'");
        }
    }

/** @class      TestMapTests
    @ingroup    REF_GertToUTWEngine_RegressionTest_GertToUTW_GerLogParserTest

    @brief      Unit tests for the 'map_result' method of the `GertLogParser` class.

    @details    Verifies that the `map_result` method correctly translates raw result strings into standardized outcome categories, 
                handles edge cases, and throws appropriate exceptions for invalid inputs.
*/
[TestClass]
public class TestMapTests
    {
    /** @brief Verifies that a correctly formatted result string is mapped to the expected standardized outcome. 
     * param[in]  raw_result A string representing the raw result from the log.
     * param[in]  expected_result The expected standardized outcome after mapping.
     */
    [TestMethod]
    [DataRow("PASS", "PASSED")]
    [DataRow("FAIL", "FAILED")]
    [DataRow("SKIP", "SKIPPED")]
    [DataRow("", "")]
    [DataRow(" d  ", " d  ")]
    public void MapResult( string raw_result, string expected_result )
        {
        string result = GertLogParser.map_result(raw_result);
        Assert.AreEqual(expected_result, result, $"Mapping outcome did not match expectations for input: '{raw_result}'");
        }
    }

/** @class      SerialAttributesTests
    @ingroup    REF_GertToUTWEngine_RegressionTest_GertToUTW_GerLogParserTest

    @brief      Unit tests for the 'build_serial_attributes' method of the `GertLogParser` class.

    @details    Verifies that the `build_serial_attributes` method correctly constructs a dictionary of serializable attributes from the log text, 
                handles edge cases.
*/
[TestClass]
public class SerialAttributesTests
    {
    /** @brief Verifies that the `build_serial_attributes` method correctly extracts and serializes attributes from log text. */
    [TestMethod]
    public void BuildSerialAttributes_att()
        {
        List<(string, string)> input_attributes =
            [
                ("MACAddress", "00:1A:2B:3C:4D:5E"),
                ("SerialNumber", "SRNUM-999X"),
                ("FirmwareVersion", "v2.1.4")
            ];
        List<SerialNumberAttributes> result = GertLogParser.build_serial_attributes(input_attributes);
        Assert.IsNotNull(result, "The returned list should never be null.");
        Assert.HasCount(input_attributes.Count, result, "The result list count should match the input count.");
        Assert.AreEqual(1, result[0].SerialNumberAttributes_Key, "Key should currently default to 1.");
        Assert.AreEqual("MACAddress", result[0].Name, "Name should match the first tuple element.");
        Assert.AreEqual("00:1A:2B:3C:4D:5E", result[0].Value, "Value should match the second tuple element.");
        Assert.AreEqual(1, result[1].SerialNumberAttributes_Key, "Key remains 1 for subsequent items in current implementation.");
        Assert.AreEqual("SerialNumber", result[1].Name);
        Assert.AreEqual("SRNUM-999X", result[1].Value);
        }

    /** @brief Verifies that the `serial_attributes` method correctly handles an empty input list. */
    [TestMethod]
    public void BuildSerialAttributes_empty()
        {
        List<(string, string)> empty_input = [];
        List<SerialNumberAttributes> result = GertLogParser.build_serial_attributes(empty_input);
        Assert.IsNotNull(result, "Should return an instantiated list, not null.");
        Assert.IsEmpty(result, "An empty input list should produce an empty output list.");
        }
    }

/** @class      BuildTestItemTest
    @ingroup    REF_GertToUTWEngine_RegressionTest_GertToUTW_GerLogParserTest

    @brief      Unit tests for the 'build_test_item' method of the `GertLogParser` class.

    @details    Verifies that the `build_test_item` method correctly constructs a `TestItem` object, and saves the
                test output to the correct property
*/
[TestClass]
public class BuildTestItemTest
    {
    [TestClass]
    public class GertLogParserTestItemRoutingTests
        {
        [TestMethod]
        [DataRow("  Stack trace exception details  ", "FAIL", null, "Stack trace exception details")]
        [DataRow("  Standard informational output  ", "PASS", "Standard informational output", null)]
        [DataRow("", "PASS", null, null)]
        [DataRow("   ", "FAIL", null, null)]
        public void BuildTestItem_ValidatesOutputRoutingLogicOnly(
            string middle_string,
            string raw_result_input,
            string expected_stdout,
            string expected_stderr )
            {
            string target_log_block =
                $"Step 12: [StepName] INFO::ActionSteps details\n" +
                $"{middle_string}" +
                $"Result: {raw_result_input}";

            Match raw_match = GertLogParser.step_item_regex().Match(target_log_block);
            Assert.IsTrue(raw_match.Success, " The production step_item_regex failed to match the test logblock layout structure.");
            TestItem result = GertLogParser.build_test_item(raw_match);
            Assert.AreEqual(expected_stdout, result.Stdout, "The value assigned to Stdout did not match routing expectations.");
            Assert.AreEqual(expected_stderr, result.Stderr, "The value assigned to Stderr did not match routing expectations.");
            }
        }
    }

/** @class      ParseStepItemTests
    @ingroup    REF_GertToUTWEngine_RegressionTest_GertToUTW_GerLogParserTest
    @brief      Unit tests for the 'parse_test_items' method of the `GertLogParser` class.
    @details    Verifies that the `parse_test_items` method correctly recognises all elements and all test steps
*/
[TestClass]
public class ParseStepItemTests
    {
    /** @brief Verifies that the `parse_test_items` throws an exception when provided with a invalid input*/
    [TestMethod]
    public void ParseTestItems_NoTopLevelEnvelopeMatch_ThrowsFormatException()
        {
        string invalid_content = "Random invalid file context without log envelopes";
        _ = Assert.Throws<FormatException>(() => GertLogParser.parse_test_items(invalid_content));
        }

    /** @brief Verifies that the `parse_test_items` correctly parses a valid log block and returns the expected number of test items. */
    [TestMethod]
    [DataRow("[LogData = Start]\nSome unrelated body noise\n[LogData = End]", 0)]
    [DataRow("[LogData = Start]\nJunkBlock 1: [Noise] text\n[LogData = End]", 0)]
    [DataRow("[LogData = Start]\nStep 1: [Init] INFO::ActionSteps\nResult: PASS\n--------------------\n[LogData = End]", 1)]
    [DataRow("[LogData = Start]\nStep 1: [Init] INFO::ActionSteps\nResult: PASS\nSome Interstitial Trash Text\n--------------------\nStep 2: [Teardown] INFO::FillVariables\nResult: FAIL\n--------------------\n[LogData = End]", 2)]
    public void ParseTestItems_EvaluatesLogStructures_AndReturnsExpectedCount( string raw_content, int expected_count )
        {
        List<TestItem> result = GertLogParser.parse_test_items(raw_content);
        Assert.IsNotNull(result, "The method should always return a valid list instance, never null.");
        Assert.HasCount(expected_count, result, $"Expected to extract exactly {expected_count} items from the payload string.");
        }
    }

/** @class      ParseGertLog
    @ingroup    REF_GertToUTWEngine_RegressionTest_GertToUTW_GerLogParserTest
    @brief      Unit tests for the 'ParseGertLog' method of the `GertLogParser` class.
    @details    Uses testfiles to verify that the `ParseGertLog` method correctly parses entire log files and produces the expected number of test runs
*/
[TestClass]
public class GertLogParserFlowControlTests
    {
    private static readonly string theBaseFilesDir = AppDomain.CurrentDomain.BaseDirectory;

    [TestMethod]
    [DataRow("GertToUTW\\LogTestFiles\\Valid\\valid_singlerun.log", 1)]
    [DataRow("GertToUTW\\LogTestFiles\\Valid\\valid_doublerun.log", 2)]
    [DataRow("GertToUTW\\LogTestFiles\\Valid\\valid_1022000000.log", 2)]
    public void GertLogParserTest_Valid( string relative_file_name, int expected_run_count )
        {
        string absolute_path = Path.Combine(theBaseFilesDir, relative_file_name);
        Assert.IsTrue(File.Exists(absolute_path), $"Test setup configuration failure: File not found at target location: {absolute_path}");
        List<TestRun> result = GertLogParser.ParseGertLog(relative_file_name);
        Assert.IsNotNull(result);
        Assert.HasCount(expected_run_count, result, $"Expected file '{relative_file_name}' to extract exactly {expected_run_count} compiled run records.");
        }

    [TestMethod]
    [DataRow("Invalid/invalid_singlerun.log")] // doesnt have testrun rresult
    [DataRow("Invalid/invalid_doublerun.log")] // doesnt have macadress
    [DataRow("Invalid/invalid3.log")] //
    [DataRow("Invalid/invalid4.log")] //
    public void GertLogParserTest_Invalid( string relative_file_name )
        {
        //todo
        }
    }