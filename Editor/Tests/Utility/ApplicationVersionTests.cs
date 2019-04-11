using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BGC.Utility;
using LightJson;
using NUnit.Framework;

namespace BGC.Tests
{
    public class ApplicationVersionTests
    {
        private delegate bool VersionComparison(in ApplicationVersion lhs, in ApplicationVersion rhs);
        private delegate bool VersionUnitary(in ApplicationVersion value);

        [Test]
        public void TestApplicationVersionFeatures()
        {
            VersionComparison greaterThan = (in ApplicationVersion lhs, in ApplicationVersion rhs) => lhs > rhs;
            VersionComparison greaterOrEq = (in ApplicationVersion lhs, in ApplicationVersion rhs) => lhs >= rhs;
            VersionComparison lessThan = (in ApplicationVersion lhs, in ApplicationVersion rhs) => lhs < rhs;
            VersionComparison lessOrEq = (in ApplicationVersion lhs, in ApplicationVersion rhs) => lhs <= rhs;

            VersionComparison equal = (in ApplicationVersion lhs, in ApplicationVersion rhs) => lhs == rhs;
            VersionComparison notEqual = (in ApplicationVersion lhs, in ApplicationVersion rhs) => lhs != rhs;

            VersionUnitary isNull = (in ApplicationVersion value) => value.IsNull();

            //Test some implicitly created ApplicationVersions
            //Test >
            {
                Assert.IsTrue(greaterThan("1.0.1", "0.999.123"));
                Assert.IsFalse(greaterThan("9.99.999", "9.100.9999"));
                Assert.IsTrue(greaterThan("10.0.0.1", "10"));
                //Equality
                Assert.IsFalse(greaterThan("123.456.7890", "123.456.7890"));
            }

            //Test >=
            {
                Assert.IsTrue(greaterOrEq("1.0.1", "0.999.123"));
                Assert.IsFalse(greaterOrEq("9.99.999", "9.100.9999"));
                Assert.IsTrue(greaterOrEq("10.0.0.1", "10"));
                //Equality
                Assert.IsTrue(greaterOrEq("123.456.7890", "123.456.7890"));
            }

            //Test <
            {
                Assert.IsFalse(lessThan("1.0.1", "0.999.123"));
                Assert.IsTrue(lessThan("9.99.999", "9.100.9999"));
                Assert.IsTrue(lessThan("10", "10.0.0.1"));
                //Equality
                Assert.IsFalse(lessThan("44.536.0", "44.536"));
            }

            //Test <=
            {
                Assert.IsFalse(lessOrEq("1.0.1", "0.999.123"));
                Assert.IsTrue(lessOrEq("9.99.999", "9.100.9999"));
                Assert.IsTrue(lessOrEq("10", "10.0.0.1"));
                //Equality
                Assert.IsTrue(lessOrEq("44.536.0", "44.536"));
            }

            //Test ==
            {
                Assert.IsFalse(equal("1.0.1", "0.999.123"));
                Assert.IsFalse(equal("9.99.999", "9.100.9999"));
                Assert.IsFalse(equal("10", "10.0.0.1"));
                //Equality
                Assert.IsTrue(equal("44.536.0", "44.536"));
            }

            //Test !=
            {
                Assert.IsTrue(notEqual("1.0.1", "0.999.123"));
                Assert.IsTrue(notEqual("9.99.999", "9.100.9999"));
                Assert.IsTrue(notEqual("10.0.0.1", "10"));
                //Equality
                Assert.IsFalse(notEqual("123.456.7890", "123.456.7890"));
            }

            //Test IsNull
            {
                Assert.IsFalse(isNull("1"));
                Assert.IsFalse(isNull("9999.9999.9999.9999"));
                Assert.IsFalse(isNull("3.2.1.1"));
                Assert.IsTrue(isNull("0"));
                Assert.IsTrue(isNull("0.0.0.0"));

                //Bad attempts

                string[] unparsableAttempts = new string[]
                {
                    "asdf",
                    "asdf.asdfas.gasdfga.*",
                    ""
                };

                string[] truncatingAttempts = new string[] { "0.0.0.0.9" };

                //Error Handling
                foreach (string version in unparsableAttempts)
                {
                    UnityEngine.TestTools.LogAssert.Expect(LogType.Error, $"Failed to parse version string: {version}");
                    Assert.IsTrue(isNull(version));
                }

                //Error Handling
                foreach (string version in truncatingAttempts)
                {
                    UnityEngine.TestTools.LogAssert.Expect(LogType.Error, $"ApplicationVersion constructed by string with more than 4 fields: \"{version}\"");
                    Assert.IsTrue(isNull(version));
                }
                
            }

            //Test constructors
            {
                Assert.IsTrue(equal("44.536.0", new ApplicationVersion(44, 536)));
                Assert.IsTrue(greaterOrEq("123.456.7890.1", new ApplicationVersion(123, 456, 7890)));
            }

            //Test explicit fields
            {
                ApplicationVersion testVer = "10.0.1";

                Assert.IsTrue(testVer.Major == 10);
                Assert.IsTrue(testVer.Minor == 0);
                Assert.IsTrue(testVer.Build == 1);
                Assert.IsTrue(testVer.Revision == 0);

                Assert.IsTrue(testVer.ToString() == "10.0.1");
            }

        }
    }
}