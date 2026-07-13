/** @file       SerialNumberAttributesTests.cs

    @brief      Provides automated verification scenarios testing the SerialNumberAttributes data model.

    @details    This file establishes simple execution scenarios verifying structural default states
                and accurate property assignment handling within the SerialNumberAttributes entity.
                Verification boundaries are constructed using the MSTest framework infrastructure.

    @author     Mathilde Needham (Mathilde.Needham@tria-technologies.com)
    @date       09.07.2026

    @defgroup   REF_GertToUTWEngine_RegressionTest_GertToUTW_SerialNumberAttributesTest   SerialNumberAttributesTest
    @{
    @ingroup    REF_GertToUTWEngine_RegressionTest_GertToUTW
*/
using GertToUTW;
namespace RegressionTest.GertToUTW;

/** @class      SerialNumberAttributesTests
    @ingroup    REF_GertToUTWEngine_RegressionTest_GertToUTW_SerialNumberAttributesTest

    @brief      Unit tests for the `SerialNumberAttributes` class.
*/
[TestClass]
public sealed class SerialNumberAttributesTests
    {
    /** @brief Verifies that the default constructor initializes accurate baseline states. */
    [TestMethod]
    public void ConstructorDefaultInitializationTest()
        {
        SerialNumberAttributes attribute = new();

        Assert.AreEqual(1, attribute.SerialNumberAttributes_Key);
        Assert.AreEqual(string.Empty, attribute.Name);
        Assert.AreEqual(string.Empty, attribute.Value);
        }

    /** @brief Verifies that explicit data assignments flow through all properties cleanly. */
    [TestMethod]
    public void PropertiesReadWriteDataFlowTest()
        {
        SerialNumberAttributes attribute = new()
            {
            SerialNumberAttributes_Key = 42,
            Name = "MACAddress",
            Value = "00:1A:2B:3C:4D:5E"
            };

        Assert.AreEqual(42, attribute.SerialNumberAttributes_Key);
        Assert.AreEqual("MACAddress", attribute.Name);
        Assert.AreEqual("00:1A:2B:3C:4D:5E", attribute.Value);
        }
    }