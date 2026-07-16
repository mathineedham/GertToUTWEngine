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

namespace RegressionTests.GertToUTW;


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
         _ = Assert.ThrowsExactly<FormatException>(() =>GertLogParser.parse_date(malformed_date_text));
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
        Assert.AreEqual(expected_value, result);
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
    [DataRow("[LogData = Start]\nStep 1: [Init]\nINFO::ActionSteps\nResult: PASS\n--------------------\n[LogData = End]", 1)]
    [DataRow("[LogData = Start]\nStep 1: [Init]\nINFO::ActionSteps\nResult: PASS\nSome Interstitial Trash Text\n--------------------\nStep 2: [Teardown]\n INFO::FillVariables\nResult: FAIL\n--------------------\n[LogData = End]", 2)]
    [DataRow("""
        [LogData = Start]
        Step  1: [I.1.1. Prepare]

        ##### WaitOnReply
        send command : [cd /cygdrive/c/DMS_Testenvironment_HW/Scripts4Gert]
        received data:
        | cd /cygdrive/c/DMS_Testenvironment_HW/Scripts4Gert]0;/cygdrive/c/DMS_Testenvironment_HW/Scripts4Gert[32madmtsg@DESTSM3WK07176N [33m/cygdrive/c/DMS_Testenvironment_HW/Scripts4Gert[0m$ 
        Time elapsed= 0.10
        Result: PASS
        --------------------
        Step  2: [I.1.3. Anschliessen]
        INFO::AskUserImage(0): 
        #####  Internal Cmd AskUserImage:
        args=[['\n CFAST Karte gesteckt und alles angeschlossen ?', 'ALL_CONNECTED1.JPG']]
        Result: PASS
        --------------------
        [LogData = End]
        """, 2)]
    public void ParseTestItems_EvaluatesLogStructures_AndReturnsExpectedCount( string raw_content, int expected_count )
        {
        List<TestItem> result = GertLogParser.parse_test_items(raw_content);
        Assert.IsNotNull(result);
        Assert.HasCount(expected_count, result);
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
    [DataRow("GertToUTW\\LogTestFiles\\Valid\\valid_emptystring.log", 1)]
    [DataRow("GertToUTW\\LogTestFiles\\Valid\\valid_doublerun.log", 2)]
    [DataRow("GertToUTW\\LogTestFiles\\Valid\\valid_1022000000.log", 2)]
    public void GertLogParserTest_Valid( string relative_file_name, int expected_run_count )
        {
        string absolute_path = Path.Combine(theBaseFilesDir, relative_file_name);
        Assert.IsTrue(File.Exists(absolute_path));
        List<TestRun> result = GertLogParser.ParseGertLog(relative_file_name);
        Assert.IsNotNull(result);
        Assert.HasCount(expected_run_count, result);
        }

    [TestMethod]
    [DataRow("")]
    [DataRow(null)]
    public void GertLogParserTest_EmptyOrNull( string relative_file_name )
        {
        _ = Assert.Throws<ArgumentException>(() => GertLogParser.ParseGertLog(relative_file_name));
        }

    [TestMethod]
    public void GertLogParserTest_NonExistentFile()
        {
        string non_existent_file = "GertToUTW\\LogTestFiles\\Invalid\\nonexistent.log";
        _ = Assert.Throws<FileNotFoundException>(() => GertLogParser.ParseGertLog(non_existent_file));
        }
    }

