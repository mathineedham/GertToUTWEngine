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
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

using GertToUTW;

[assembly: InternalsVisibleTo("RegressionTest")]
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
    public void Parse_date_ValidFormat_ReturnsExpectedDateTime()
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
    public void Parse_date_InvalidFormat_ThrowsFormatExceptionWithRawValueAsMessage( string malformed_date_text )
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
    [TestMethod]
    [DataRow("PASS", "PASSED")]
    [DataRow("FAIL", "FAILED")]
    [DataRow("SKIP", "SKIPPED")]
    [DataRow("", "")]
    [DataRow(" d  ", " d  ")]
    [DataRow(null, null)]
    public void MapResult_WhenCalled_ReturnsMappedValueOrOriginalRawInput( string raw_result, string expected_result )
        {
        string result = GertLogParser.map_result(raw_result);
        Assert.AreEqual(expected_result, result, $"Mapping outcome did not match expectations for input: '{raw_result}'");
        }
    }