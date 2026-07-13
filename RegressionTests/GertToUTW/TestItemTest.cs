
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
namespace RegressionTests.GertToUTW;

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
        Assert.AreEqual(string.Empty, item.Result.Value);
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
    }