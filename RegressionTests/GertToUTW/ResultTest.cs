
using GertToUTW;

/** @file

    @copyright  &copy; 2024, Tria Technologies GmbH
                SPDX-License-Identifier: (GPL-2.0-or-later OR LGPL-2.1-or-later)

    @date       09.07.2026

    @author     Mathilde Needham (Mathilde.Needham@tria-technologies.com)

    @defgroup   REF_GertToUTWEngine_RegressionTest_GertToUTW_ResultTest   ResultTest
    @{
    @ingroup    REF_GertToUTWEngine_RegressionTest_GertToUTW

    @brief      Regression tests for the `Result` class 

    @}
*/
namespace RegressionTests.GertToUTW;

/** @class      ResultTest
    @ingroup    REF_GertToUTWEngine_RegressionTest_GertToUTW_ResultTest

    @brief      Unit tests for the `Result` class.

    @details    Contains validation methods targeting edge-cases, exceptions, and proper runtime handling 
                of properties within the `TestItem` class structure.
*/
[TestClass]
public sealed class ResultTests
    {
    /** @brief Verifies that the default constructor sets accurate baseline states for the object. */
    [TestMethod]
    public void ConstructorDefaultInitializationTest()
        {
        Result result = new();
        Assert.AreEqual(string.Empty, result.Value);
        }

    /** @brief Verifies that assigning a null reference value resets the execution status to an empty string. */
    [TestMethod]
    public void ValuePropertyNullInputResetsToEmptyTest()
        {
        Result result = new()
            {
            Value = null!
            };
        Assert.AreEqual(string.Empty, result.Value);
        }

    /** @brief      Validates strict string normalization and direct invariant uppercase matching.
     
        @param[in]  input_verdict    The raw variance token to parse.
        @param[in]  expected_verdict The compliant destination uppercase string value.
    */
    [TestMethod]
    [DataRow("passed", "PASSED")]
    [DataRow("FAILED", "FAILED")]
    [DataRow("Skipped", "SKIPPED")]
    [DataRow("incomplete", "INCOMPLETE")]
    [DataRow("error", "ERROR")]
    public void ValuePropertyExactInvariantsNormalizeTest( string input_verdict, string expected_verdict )
        {
        Result result = new()
            {
            Value = input_verdict
            };
        Assert.AreEqual(expected_verdict, result.Value);
        }

    /** @brief      Confirms that unrecognized text triggers an ArgumentException.
     
        @param[in]  invalid_verdict   An unexpected outcome label text.
    */
    [TestMethod]
    [DataRow("UNKNOWN")]
    [DataRow("invalid_state")]
    [DataRow("PASS_YET_INVALID")]
    public void ValuePropertyInvalidInputThrowsTest( string invalid_verdict )
        {
        Result result = new();

        _ = Assert.ThrowsExactly<ArgumentException>(() =>
        {
            result.Value = invalid_verdict;
        });
        }
    }

/** @class      TestMapTests
    @ingroup    REF_GertToUTWEngine_RegressionTest_GertToUTW_ResultTest

    @brief      Unit tests for the 'map_result' method of the `Result` class.

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
    [DataRow(null,null)]
    public void MapResult( string raw_result, string expected_result )
        {
        string result = Result.map_result(raw_result);
        Assert.AreEqual(expected_result, result);
        }
    }