
using GertToUTW;

/** @file

    @copyright  &copy; 2024, Tria Technologies GmbH
                SPDX-License-Identifier: (GPL-2.0-or-later OR LGPL-2.1-or-later)

    @date       09.07.2026

    @author     Mathilde Needham (Mathilde.Needham@tria-technologies.com)

    @defgroup   REF_GertToUTWEngine_RegressionTest_GertToUTW_TestRunTest   TestRunTest
    @{
    @ingroup    REF_GertToUTWEngine_RegressionTest_GertToUTW

    @brief      Regression tests for the `TestRun` and `SerialNumberAttributes` class 

    @details    Validates the behavior of the `TestRun` class, including default construction,
                handling of various Type configurations and expected exception handling in error scenarios.
    @}
*/
namespace RegressionTest.GertToUTW;
/** @class      TestRunTest
    @ingroup    REF_GertToUTWEngine_RegressionTest_GertToUTW_TestRunTest

    @brief      Unit tests for the `TestRun` class.
*/
[TestClass]
public sealed class TestRunTests
    {
    /** @brief Verifies that the default constructor accurately configures structural default values. */
    [TestMethod]
    public void ConstructorDefaultInitializationTest()
        {
        TestRun run = new();

        Assert.AreEqual(1, run.TestRun_Key);
        Assert.IsNotNull(run.Result);
        Assert.AreEqual(string.Empty, run.Result.Value);
        Assert.AreEqual("000000", run.Lot);
        Assert.AreEqual("0.0.0", run.SoftwareVersion);
        Assert.AreEqual("OPERATING", run.OperatingMode);
        Assert.IsNotNull(run.TestItem);
        Assert.IsNotNull(run.SerialNumberAttributes);
        Assert.IsNull(run.Comment);
        Assert.IsNull(run.Routestep);
        Assert.IsNull(run.Station);

        }

    /** @brief Validates safe extraction and storage of correct structural Material Numbers. */
    [TestMethod]
    [DataRow("12345678")]
    [DataRow("9")]
    [DataRow("")]
    [DataRow(null)]
    public void MaterialNumberPropertyValidInputPassesTest( string valid_material )
        {
        TestRun run = new()
            {
            MaterialNumber = valid_material
            };
        Assert.AreEqual(valid_material, run.MaterialNumber);
        }

    /** @brief Confirms that malformed Material Numbers trigger an ArgumentException. */
    [TestMethod]
    [DataRow("abc")]
    [DataRow("0000000000000000000")] // Length greater than 18 digits
    [DataRow("-12.5")]
    [DataRow("AB34 7")]
    public void MaterialNumberPropertyInvalidInputThrowsTest( string invalid_material )
        {
        TestRun run = new();
        _ = Assert.ThrowsExactly<ArgumentException>(() => run.MaterialNumber = invalid_material);
        }

    /** @brief Validates compliant material engineering revisions match expectations. */
    [TestMethod]
    [DataRow("A1B2")]
    [DataRow("0000")]
    [DataRow("")]
    public void MaterialRevisionPropertyValidInputPassesTest( string valid_revision )
        {
        TestRun run = new()
            {
            MaterialRevision = valid_revision
            };
        Assert.AreEqual(valid_revision, run.MaterialRevision);
        }

    /** @brief Confirms that a poorly formatted material engineering revision throws an Exception. */
    [TestMethod]
    [DataRow("A1B")]  // Length under 4 characters
    [DataRow("A1B2C")] // Length over 4 characters
    [DataRow("A1-3")] // Invalid characters
    public void MaterialRevisionPropertyInvalidInputThrowsTest( string invalid_revision )
        {
        TestRun run = new();
        _ = Assert.ThrowsExactly<ArgumentException>(() => run.MaterialRevision = invalid_revision);
        }

    /** @brief Validates that properly masked batch sequence lots assign cleanly. */
    [TestMethod]
    [DataRow("123456")]
    [DataRow("1234567")]
    [DataRow("")]
    public void LotPropertyValidInputPassesTest( string valid_lot )
        {
        TestRun run = new()
            {
            Lot = valid_lot
            };
        Assert.AreEqual(valid_lot, run.Lot);
        }

    /** @brief Confirms that assigning an out-of-bounds or character-broken lot throws. */
    [TestMethod]
    [DataRow("12345")]   // Too short
    [DataRow("12345678")] // Too long
    [DataRow("123A56")]   // Letters
    public void LotPropertyInvalidInputThrowsTest( string invalid_lot )
        {
        TestRun run = new();
        _ = Assert.ThrowsExactly<ArgumentException>(() => run.Lot = invalid_lot);
        }


    /** @brief Validates explicit string normalization and upper-case invariants on Operating Modes. */
    [TestMethod]
    [DataRow("operating", "OPERATING")]
    [DataRow("Engineering", "ENGINEERING")]
    [DataRow("repair", "REPAIR")]
    [DataRow("DEVELOPMENT", "DEVELOPMENT")]
    [DataRow("Rma", "RMA")]
    [DataRow(null,null)]
    public void OperatingModePropertyNormalizesInvariantsTest( string input_mode, string expected_mode )
        {
        TestRun run = new()
            {
            OperatingMode = input_mode
            };
        Assert.AreEqual(expected_mode, run.OperatingMode);
        }

    /** @brief Confirms unmapped execution environments throw an exception safely. */
    [TestMethod]
    [DataRow("PRODUCTION")]
    [DataRow("DEBUG")]
    public void OperatingModePropertyInvalidInputThrowsTest( string invalid_mode )
        {
        TestRun run = new();
        _ = Assert.ThrowsExactly<ArgumentException>(() => run.OperatingMode = invalid_mode);
        }


    /** @brief VFalidates set and get for Routestep,Station and Comment properties*/
    [TestMethod]
    public void RoutestepStationCommentPropertySetGetTest()
        {
        TestRun run = new()
            {
            Routestep = "Step1",
            Station = "StationA",
            Comment = "This is a test comment."
            };
        Assert.AreEqual("Step1", run.Routestep);
        Assert.AreEqual("StationA", run.Station);
        Assert.AreEqual("This is a test comment.", run.Comment);
        }
    }