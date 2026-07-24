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
// Ignore Spelling: Routestep
using GertToUTW;
namespace RegressionTests.GertToUTW;

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
    [DataRow("cat")]
    [DataRow("0000000000000000000")] // Length greater than 18 digits
    [DataRow("-12.5")]
    [DataRow("AB34 7")]
    public void MaterialNumberPropertyInvalidInputThrowsTest( string invalid_material )
        {
        TestRun run = new();
        _ = Assert.ThrowsExactly<ArgumentException>(
            () =>
            {
                return run.MaterialNumber = invalid_material;
            });
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
        _ = Assert.ThrowsExactly<ArgumentException>(
            () =>
            {
                return run.MaterialRevision = invalid_revision;
            });
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
        _ = Assert.ThrowsExactly<ArgumentException>(
            () =>
            {
                return run.OperatingMode = invalid_mode;
            });
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

/** @class      LotNumberCalculatorTests

    @ingroup    REF_GertToUTWEngine_RegressionTest_GertToUTW_TestRunTest

    @brief      Unit tests for the 'generate_lot_number' method of the `TestRun` class.

    @details    Test the generation of lot numbers based on material number and revision, ensuring that the output adheres to the expected format and value.*/
[TestClass]
public class LotNumberCalculatorTests
    {
    /** @brief Tests that the 'GenerateLotNumber' method throws an ArgumentNullException when provided with a null or empty argument */
    [TestMethod]
    [DataRow(null, "B001")]
    [DataRow("", "B001")]
    [DataRow("12345678", null)]
    [DataRow("12345678", "")]
    public void GenerateLotNumber_Invalid( string? mat_num, string? rev_num )
        {
        _ = Assert.ThrowsExactly<ArgumentException>(
            () =>
            {
                return TestRun.generate_lot_number(mat_num, rev_num);
            });
        }

    /** @brief Tests that empty or non-numeric strings throw a FormatException */
    [TestMethod]
    [DataRow("not_a_number", "B001")]
    [DataRow("12345678", "not_hex")]
    public void GenerateLotNumber_InvalidFormat_ThrowsFormatException( string mat_num, string rev_num )
        {
        _ = Assert.ThrowsExactly<FormatException>(
            () =>
            {
                return TestRun.generate_lot_number(mat_num, rev_num);
            });
        }

    /** @brief Tests that the 'GenerateLotNumber' method correctly computes the lot number from given material number and revision. */
    [TestMethod]
    //  MaterialNumber | MaterialRevision | ExpectedLotNumber
    [DataRow("12345678", "B001", "390735")]   // 12345678 + 45057 = 12390735 -> Modulo 1M -> 390735
    [DataRow("87654321", "0000", "654321")]   // Let's verify: 87654321 % 1000000 = 654321
    [DataRow("00000001", "C003", "049156")]   // 1 + 49155 = 49156           -> Padded   -> 049156
    [DataRow("0", "0000", "000000")]
    public void GenerateLotNumber_Valid(
        string mat_num, string mat_rev, string expected )
        {
        string actual_result = TestRun.generate_lot_number(mat_num, mat_rev);
        Assert.AreEqual(expected, actual_result);
        }

    /** @brief Verifies that try_auto_generate_lot handles FormatException silently without updating Lot */
    [TestMethod]
    public void TryAutoGenerateLot_FormatException()
        {
        TestRun test_run = new()
            {
            MaterialRevision = "XXXX",

            MaterialNumber = "0000"
            };

        Assert.AreEqual("000000", test_run.Lot);
        }

    /** @brief Verifies that try_auto_generate_lot handles OverflowException silently without updating Lot */
    [TestMethod]
    public void TryAutoGenerateLot_OverflowException()
        {
        TestRun test_run = new()
            {
            MaterialRevision = "B001",
            MaterialNumber = "99999999999999999999"
            };
        Assert.AreEqual("000000", test_run.Lot);
        }
    }

/** @class      FindLinkPHandleStepTests
    @ingroup    REF_GertToUTWEngine_RegressionTest_GertToUTW_TestRunTest
    @brief      Unit tests for the 'Find_link_phandle_step' method of the `TestRun` class.
    @details    Verifies that the `Find_link_phandle_step` method correctly parses the "Link PHandle" 
                test step output and updates properties under various test run execution scenarios.*/
[TestClass]
public class FindLinkPHandleStepTests
    {
    /** @brief Verifies that the `Find_link_phandle_step` method handles scenarios where the PHandle step is missing, empty, or unpassed. */
    [TestMethod]
    public void FindLinkPHandle_EmptyOrInvalidOutput()
        {
        // Case 1: Empty list of test items
        TestRun run1 = new()
            { TestItem = [] };
        run1 = run1.Find_link_phandle_step();
        Assert.IsTrue(string.IsNullOrEmpty(run1.Routestep));
        Assert.IsEmpty(run1.SerialNumberAttributes);

        // Case 2: List with no "Link PHandle" step
        TestRun run2 = new()
            {
            TestItem =
                [
                new TestItem { Name = "Step 1: [Init]", Stdout = "INFO::ActionSteps\nResult: PASS" },
                new TestItem { Name = "Step 2: [Rush]", Stdout = "INFO::FillVariables\nResult: FAIL" }
                ]
            };
        run2 =run2.Find_link_phandle_step();
        Assert.IsTrue(string.IsNullOrEmpty(run2.Routestep));
        Assert.IsEmpty(run2.SerialNumberAttributes);

        // Case 3: List with a "Link PHandle" step but Stdout is empty
        TestRun run3 = new()
            {
            TestItem =
                [
                new TestItem { Name = "Step 1: [Init]", Stdout = "INFO::ActionSteps\nResult: PASS" },
                new TestItem { Name = "Link PHandle", Stdout = "", Result = new Result { Value = "PASSED" } }
                ]
            };
        run3 =run3.Find_link_phandle_step();
        Assert.IsTrue(string.IsNullOrEmpty(run3.Routestep));
        Assert.IsEmpty(run3.SerialNumberAttributes);

        // Case 4: List with a "Link PHandle" step but Result is not "PASSED" (e.g. skipped/failed)
        TestRun run4 = new()
            {
            TestItem =
                [
                new TestItem
                    {
                    Name = "Link PHandle",
                    Stdout = """
                    INFO::FillVariables(0): SET [LinkPHandle]
                    INFO::FillVariables(0): SET [FT_FUNCTION]
                    INFO::FillVariables(0): SET [1022000000]
                    INFO::FillVariables(0): SET [F60287RA]
                    """,
                    Result = new Result { Value = "SKIPPED" } // Step was not PASSED
                    }
                ]
            };
        run4 =run4.Find_link_phandle_step();
        Assert.AreEqual("FT_FUNCTION", run4.Routestep);
        Assert.IsEmpty(run4.SerialNumberAttributes);
        }

    /** @brief Verifies that `Find_link_phandle_step` successfully extracts RouteStep and PHandle properties on a valid PASSED run. */
    [TestMethod]
    public void FindLinkPHandle_ValidOutput()
        {
        TestRun run = new()
            {
            TestItem =
                [
                new TestItem { Name = "Step 1: [Init]", Stdout = "INFO::ActionSteps\nResult: PASS" },
                new TestItem
                    {
                    Name = "Step 11: [L.999.02. Link PHandle]",
                    Stdout = """
                    INFO::FillVariables(0): SET [LinkPHandle]
                    INFO::FillVariables(0): SET [FT_FUNCTION]
                    INFO::FillVariables(0): SET [1022000000]
                    INFO::FillVariables(0): SET [F60287RA]
                    """,
                    Result = new Result { Value = "PASSED" }
                    },
                new TestItem { Name = "Step 3: [Rush]", Stdout = "INFO::FillVariables\nResult: FAIL" }
                ]
            };

        // Act
        run=run.Find_link_phandle_step();

        // Assert
        Assert.AreEqual("FT_FUNCTION", run.Routestep);

        Assert.HasCount(1, run.SerialNumberAttributes);
        Assert.AreEqual("PHandle", run.SerialNumberAttributes[0].Name);
        Assert.AreEqual("F60287RA", run.SerialNumberAttributes[0].Value);
        }
    }
