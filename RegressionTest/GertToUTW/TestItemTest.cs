
using GertToUTW;

/** @file

    @copyright  &copy; 2024, Tria Technologies GmbH
                SPDX-License-Identifier: (GPL-2.0-or-later OR LGPL-2.1-or-later)

    @date       08.07.2026

    @author     Mathilde Needham (Mathilde.Needham@tria-technologies.com)

    @defgroup   REF_GertToUTWEngine_RegressionTest_GertToUTW_TestItemTest   TestItemTest
    @{
    @ingroup    REF_GertToUTWEngine_RegressionTest_GertToUTW

    @brief      Regression tests for the `TestItem` class 

    @details    Validates the behavior of the `TestItem` class, including default construction,
                handling of various Type configurations and expected exception handling in error scenarios.
    @}
*/
namespace RegressionTest.GertToUTW;

/** @class      TestItemTests
    @ingroup    REF_GertToUTWEngine_RegressionTest_GertToUTW_TestItemTest

    @brief      Unit tests for the `TestItem` class.

    @details    Contains validation methods targeting edge-cases, exceptions, and proper runtime handling 
                of properties within the `TestItem` class structure.
*/
[TestClass]
public sealed class TestItemTests
    {
    /** @brief Verifies that the default constructor sets accurate baseline states for the object. */
    [TestMethod]
    public void ConstructorDefaultInitializationTest()
        {
        TestItem item = new();
        Assert.AreEqual(1, item.TestItem_Key);
        Assert.AreEqual(string.Empty, item.Name);
        Assert.IsNull(item.Description);
        Assert.IsNull(item.Stdout);
        Assert.IsNull(item.Stderr);
        Assert.IsNull(item.Idx);
        Assert.AreEqual(string.Empty, item.Result);
        }

    /** @brief      Validates that legal sequence indexing values correctly persist to the property.
     
        @param[in]  valid_idx   A boundary integer within the structural xsd:short limits.
    */
    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(32767)]
    public void IdxPropertyValidInputTest( int valid_idx )
        {
        TestItem item = new()
            {
            Idx = valid_idx
            };
        Assert.AreEqual(valid_idx, item.Idx);
        }

    /** @brief      Ensures that setting an index outside valid bounds throws an ArgumentOutOfRangeException.
     
        @param[in]  invalid_idx   An integer breaking sequence boundaries.
    */
    [TestMethod]
    [DataRow(-1)]
    [DataRow(32768)]
    public void IdxPropertyBoundaryViolationThrowsTest( int invalid_idx )
        {
        TestItem item = new();

        _ = Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
        {
            item.Idx = invalid_idx;
        });
        }

    /** @brief Verifies that assigning a null reference value resets the execution status to an empty string. */
    [TestMethod]
    public void ResultPropertyNullInputResetsToEmptyTest()
        {
        TestItem item = new()
            {
            Result = null!
            };
        Assert.AreEqual(string.Empty, item.Result);
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
    public void ResultPropertyExactInvariantsNormalizeTest( string input_verdict, string expected_verdict )
        {
        TestItem item = new()
            {
            Result = input_verdict
            };
        Assert.AreEqual(expected_verdict, item.Result);
        }

    /** @brief      Confirms that unrecognized text triggers an ArgumentException.
     
        @param[in]  invalid_verdict   An unexpected outcome label text.
    */
    [TestMethod]
    [DataRow("UNKNOWN")]
    [DataRow("invalid_state")]
    [DataRow("PASS_YET_INVALID")]
    public void ResultPropertyInvalidInputThrowsTest( string invalid_verdict )
        {
        TestItem item = new();

        _ = Assert.ThrowsExactly<ArgumentException>(() =>
        {
            item.Result = invalid_verdict;
        });
        }
    }