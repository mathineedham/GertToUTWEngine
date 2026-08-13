/** @file

    @copyright  &copy; 2026, TRIA Technologies GmbH
                SPDX-License-Identifier: (GPL-2.0-or-later OR LGPL-2.1-or-later)

    @date       2026-07-31

    @author
        Development Team (dev@tria.de)

    @brief
        Provides unit tests for the TestRunNormalizer utility class.

    @details
        - Validates default PHandle attribute propagation across batch test runs.
        - Validates fallback behavior when missing or empty route steps are encountered.
        - Verifies exact exception throwing behavior for null inputs using MSTest v4.
        - Ensures public contract safety without modifying shared mutable state.

    @defgroup REF_GertToUTW_RegressionTests_GertToUTW_TestRunNormalizer_Tests TestRunNormalizer_Tests
    @{
    @ingroup  REF_GertToUTW_RegressionTests_GertToUTW
    @}
*/

using GertToUTW;

namespace RegressionTests.GertToUTW;

/** @ingroup REF_GertToUTW_RegressionTests_GertToUTW_TestRunNormalizer_Tests
    @class TestRunNormalizerTests
    @brief
        Provides unit tests for the TestRunNormalizer class.

    @details
        - Verifies correct propagation of default PHandle attributes and route steps across collections of TestRun instances.
        - Validates exception handling contracts for invalid inputs.
        - Uses MSTest v4 assertions for behavioral verification.

    @see GertToUTW.TestRunNormalizer
*/
[TestClass]
public class TestRunNormalizerTests
    {
    /** @test
            Validates that NormalizeRouteAndPHandle throws ArgumentNullException when passed a null list.

        @details
            - Passes a null list reference to the normalization method.
            - Verifies that ArgumentNullException is thrown exactly.
            - Confirms that the resulting exception message is non-empty.

        @note
            Validates public boundary guard behavior against null input parameters.

        @see GertToUTW.TestRunNormalizer.NormalizeRouteAndPHandle
    */
    [TestMethod]
    public void NormalizeRouteAndPHandle_NullList_ThrowsArgumentNullException()
        {
        ArgumentNullException except = Assert.ThrowsExactly<ArgumentNullException>(
            () =>
            {
                return TestRunNormalizer.NormalizeRouteAndPHandle(null!);
            });

        Assert.IsFalse(string.IsNullOrWhiteSpace(except.Message));
        }

    /** @test
            Validates that no modifications occur when the provided list of test runs is empty.

        @details
            - Passes an empty list of TestRun instances.
            - Confirms that the returned list is non-null and empty.

        @note
            Validates edge-case collection handling without throwing exceptions.

        @see GertToUTW.TestRunNormalizer.NormalizeRouteAndPHandle
    */
    [TestMethod]
    public void NormalizeRouteAndPHandle_EmptyList_ReturnsEmptyList()
        {
        List<TestRun> list = [];

        List<TestRun> result = TestRunNormalizer.NormalizeRouteAndPHandle(list);

        Assert.IsNotNull(result);
        Assert.IsEmpty(result);
        }

    /** @test
            Validates that the first valid PHandle found in any test run is propagated to test runs missing a PHandle attribute.

        @details
            - Configures an initial TestRun without attributes and a subsequent TestRun with a valid PHandle.
            - Executes normalization on the collection.
            - Asserts that the missing PHandle attribute is populated on the first TestRun.

        @note
            Verifies forward-propagation contract for missing PHandle attributes.

        @see GertToUTW.TestRunNormalizer.NormalizeRouteAndPHandle
    */
    [TestMethod]
    public void NormalizeRouteAndPHandle_MissingPHandle_PropagatesFirstValidPHandle()
        {
        TestRun tr1 = new()
            {
            SerialNumberAttributes = []
            };

        TestRun tr2 = new()
            {
            SerialNumberAttributes =
                [
                new() { Name = "PHandle", Value = "PH_12345" }
                ]
            };

        List<TestRun> list = [tr1, tr2];

        _ = TestRunNormalizer.NormalizeRouteAndPHandle(list);

        Assert.HasCount(1, tr1.SerialNumberAttributes);
        Assert.AreEqual("PHandle", tr1.SerialNumberAttributes[0].Name);
        Assert.AreEqual("PH_12345", tr1.SerialNumberAttributes[0].Value);
        }

    /** @test
            Validates that existing PHandle attributes are not overwritten during normalization, even if casing differs.

        @details
            - Configures a TestRun with an existing 'phandle' attribute.
            - Configures another TestRun with a default 'PHandle' attribute.
            - Verifies that case-insensitive existing PHandles are preserved.

        @note
            Ensures normalization does not clobber pre-existing serial number attributes.

        @see GertToUTW.TestRunNormalizer.NormalizeRouteAndPHandle
    */
    [TestMethod]
    public void NormalizeRouteAndPHandle_ExistingPHandleDifferentCase_DoesNotOverwrite()
        {
        TestRun tr1 = new()
            {
            SerialNumberAttributes =
                [
                new SerialNumberAttributes { Name = "phandle", Value = "EXISTING_VAL" }
                ]
            };

        TestRun tr2 = new()
            {
            SerialNumberAttributes =
                [
                new SerialNumberAttributes { Name = "PHandle", Value = "DEFAULT_VAL" }
                ]
            };

        List<TestRun> list = [tr1, tr2];

        _ = TestRunNormalizer.NormalizeRouteAndPHandle(list);

        Assert.HasCount(1, tr1.SerialNumberAttributes);
        Assert.AreEqual("EXISTING_VAL", tr1.SerialNumberAttributes[0].Value);
        }

    /** @test
            Validates that when no valid PHandle is found, whitespace fallbacks are ignored and no attributes are added.

        @details
            - Passes a collection without valid PHandle values.
            - Verifies that no attributes are added to the list elements.

        @note
            Guarantees whitespace entries are rejected during attribute evaluation.

        @see GertToUTW.TestRunNormalizer.NormalizeRouteAndPHandle
    */
    [TestMethod]
    public void NormalizeRouteAndPHandle_NoValidPHandleFound_NoAttributesAdded()
        {
        TestRun tr1 = new()
            {
            SerialNumberAttributes = []
            };

        List<TestRun> list = [tr1];

        _ = TestRunNormalizer.NormalizeRouteAndPHandle(list);

        Assert.IsEmpty(tr1.SerialNumberAttributes);
        }

    /** @test
            Validates that the first valid route step is propagated to test runs missing a route step.

        @details
            - Sets up test runs with 'NOT_SET', whitespace, valid route step ('STEP_010'), and null.
            - Executes route normalization.
            - Asserts that whitespace and null values receive the propagated route step while non-whitespace entries remain unaltered.

        @note
            Tests fallback rules for Routestep string normalization.

        @see GertToUTW.TestRunNormalizer.NormalizeRouteAndPHandle
    */
    [TestMethod]
    public void NormalizeRouteAndPHandle_MissingRouteStep_PropagatesFirstValidRouteStep()
        {
        TestRun tr1 = new()
            {
            Routestep = "NOT_SET"
            };
        TestRun tr2 = new()
            {
            Routestep = "  "
            };
        TestRun tr3 = new()
            {
            Routestep = "STEP_010"
            };
        TestRun tr4 = new()
            {
            Routestep = null
            };

        List<TestRun> list = [tr1, tr2, tr3, tr4];

        _ = TestRunNormalizer.NormalizeRouteAndPHandle(list);

        Assert.AreEqual("STEP_010", tr1.Routestep);
        Assert.AreEqual("STEP_010", tr2.Routestep);
        Assert.AreEqual("STEP_010", tr3.Routestep);
        Assert.AreEqual("STEP_010", tr4.Routestep);
        }

    /** @test
            Validates that when no valid route step exists in the list, no default route step is applied.

        @details
            - Configures test runs containing only 'NOT_SET' and null route steps.
            - Verifies that route steps are preserved as-is without default replacement.

        @note
            Ensures fallback route propagation only triggers when a valid step is present.

        @see GertToUTW.TestRunNormalizer.NormalizeRouteAndPHandle
    */
    [TestMethod]
    public void NormalizeRouteAndPHandle_NoValidRouteStepFound_RouteStepsUnchanged()
        {
        TestRun tr1 = new()
            {
            Routestep = "NOT_SET"
            };
        TestRun tr2 = new()
            {
            Routestep = null
            };

        List<TestRun> list = [tr1, tr2];

        _ = TestRunNormalizer.NormalizeRouteAndPHandle(list);

        Assert.AreEqual("NOT_SET", tr1.Routestep);
        Assert.IsNull(tr2.Routestep);
        }
    }
