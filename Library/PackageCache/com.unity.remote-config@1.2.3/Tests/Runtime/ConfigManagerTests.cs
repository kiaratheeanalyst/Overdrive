using System.Collections;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Reflection;
using System;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.Linq;
using System.IO;


namespace Unity.RemoteConfig.Tests
{
    internal class ConfigManagerTests
    {
        [UnityTest]
        public IEnumerator SetCustomUserID_SetsCustomUserID()
        {
            ConfigManagerTestUtils.SetAppIdOnCommonPayload();
            var monoTest = new MonoBehaviourTest<SetCustomUserID_MonobehaviorTest>(false);
            
            monoTest.component.StartTest();
            yield return monoTest;
            FieldInfo deliveryFieldInfo = typeof(ConfigManager).GetField("deliveryPayload", BindingFlags.Static | BindingFlags.NonPublic);
            FieldInfo customUserIdFieldInfo = deliveryFieldInfo.FieldType.GetField("customUserId");
            Assert.That(string.Equals(customUserIdFieldInfo.GetValue(deliveryFieldInfo.GetValue(null)), ConfigManagerTestUtils.userId));
        }

        [UnityTest]
        public IEnumerator SetEnvironmentID_SetsEnvironmentID()
        {
            ConfigManagerTestUtils.SetAppIdOnCommonPayload();
            var monoTest = new MonoBehaviourTest<SetEnvironmentID_MonobehaviorTest>(false);

            monoTest.component.StartTest();
            yield return monoTest;
            FieldInfo deliveryFieldInfo = typeof(ConfigManager).GetField("deliveryPayload", BindingFlags.Static | BindingFlags.NonPublic);
            FieldInfo environmentIdFieldInfo = deliveryFieldInfo.FieldType.GetField("environmentId");
            Assert.That(string.Equals(environmentIdFieldInfo.GetValue(deliveryFieldInfo.GetValue(null)), ConfigManagerTestUtils.environmentId));
        }

        [UnityTest]
        public IEnumerator ConfigFieldInitializedAsEmpty()
        {
            yield return null;
            var emptyJObject = new JObject();
            Assert.That(ConfigManager.appConfig != null);
            Assert.That(ConfigManager.appConfig.config != null);
            Assert.AreEqual(ConfigManager.appConfig.config.GetType(), emptyJObject.GetType());
        }

    }

    internal class RuntimeConfigTests
    {
        [UnityTest]
        public IEnumerator ResponseParsedEventHanlder_ProperlySetsAssignmentId()
        {
            ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadString);

            yield return null;
            Assert.That(ConfigManager.appConfig.assignmentID == "a04fb7ec-26e4-4247-b8b4-70dd6967a858");
        }
        [UnityTest]
        public IEnumerator ResponseParsedEventHanlder_ReturnsNullAssignmentIdWhenBadResponse()
        {
            var assignmentIdBeforeRequest = ConfigManager.appConfig.assignmentID;
            ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadStringNoRCSection);

            yield return null;
            Assert.That(ConfigManager.appConfig.assignmentID == assignmentIdBeforeRequest);
        }

        [UnityTest]
        public IEnumerator ResponseParsedEventHandler_ProperlySetsEnvironmentId()
        {
            ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadString);

            yield return null;
            Assert.That(ConfigManager.appConfig.environmentID == "7d2e0e2d-4bcd-4b6e-8d5d-65d17a708db2");
        }

        [UnityTest]
        public IEnumerator ResponseParsedEventHandler_ReturnsNullEnvironmentIdWhenBadResponse()
        {
            var environmentIdBeforeRequest = ConfigManager.appConfig.environmentID;
            ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadStringNoRCSection);

            yield return null;
            Assert.That(ConfigManager.appConfig.environmentID == environmentIdBeforeRequest);
        }

        [UnityTest]
        public IEnumerator ResponseParsedEventHandler_ProperlySetsConfig()
        {
            ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadString);

            yield return null;
            Assert.That(ConfigManager.appConfig.config.ToString() == ((JObject)JObject.Parse(ConfigManagerTestUtils.jsonPayloadString)["settings"]).ToString());
        }

        [UnityTest]
        public IEnumerator ResponseParsedEventHandler_ProperlySetsConfigWhenBadResponse()
        {
            var configBeforeRequest = ConfigManager.appConfig.config;
            ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadStringNoRCSection);

            yield return null;
            Assert.That(ConfigManager.appConfig.config.ToString() == configBeforeRequest.ToString());
        }

        [UnityTest]
        public IEnumerator GetBool_ReturnsRightValue()
        {
            ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadString);
            yield return null;
            Assert.That(ConfigManager.appConfig.GetBool("bool") == true);
        }

        [UnityTest]
        public IEnumerator GetBool_ReturnsRightValueWhenBadResponse()
        {
            var boolBeforeRequest = ConfigManager.appConfig.GetBool("bool");
            ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadStringNoRCSection);
            yield return null;
            Assert.That(ConfigManager.appConfig.GetBool("bool") == boolBeforeRequest);
        }

        [UnityTest]
        public IEnumerator GetFloat_ReturnsRightValue()
        {
            ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadString);
            yield return null;
            Assert.That(ConfigManager.appConfig.GetFloat("helloe") == 0.12999999523162842);
        }
       [UnityTest]
        public IEnumerator GetFloat_ReturnsRightValueWhenBadResponse()
        {
            var floatBeforeRequest = ConfigManager.appConfig.GetFloat("helloe");
            ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadStringNoRCSection);
            yield return null;
            Assert.That(ConfigManager.appConfig.GetFloat("helloe") == floatBeforeRequest);
        }

        [UnityTest]
        public IEnumerator GetInt_ReturnsRightValue()
        {
            ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadString);
            yield return null;
            Assert.That(ConfigManager.appConfig.GetInt("someInt") == 12);
        }
       [UnityTest]
        public IEnumerator GetInt_ReturnsRightValueWhenBadResponse()
        {
            var intBeforeRequest = ConfigManager.appConfig.GetFloat("someInt");
            ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadStringNoRCSection);
            yield return null;
            Assert.That(ConfigManager.appConfig.GetInt("someInt") == intBeforeRequest);
        }

        [UnityTest]
        public IEnumerator GetString_ReturnsRightValue()
        {
            ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadString);
            yield return null;
            Assert.That(ConfigManager.appConfig.GetString("madBro") == "madAF");
        }

        [UnityTest]
        public IEnumerator GetString_ReturnsRightValueWhenBadResponse()
        {
            var stringBeforeRequest = ConfigManager.appConfig.GetString("madBro");
            ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadStringNoRCSection);
            yield return null;
            Assert.That(ConfigManager.appConfig.GetString("madBro") == stringBeforeRequest);
        }

        [UnityTest]
        public IEnumerator GetLong_ReturnsRightValue()
        {
            ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadString);
            yield return null;
            Assert.That(ConfigManager.appConfig.GetLong("longSomething") == 9223372036854775806);
        }

        [UnityTest]
        public IEnumerator GetLong_ReturnsRightValueWhenBadResponse()
        {
            var longBeforeRequest = ConfigManager.appConfig.GetLong("longSomething");
            ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadStringNoRCSection);
            yield return null;
            Assert.That(ConfigManager.appConfig.GetLong("longSomething") == longBeforeRequest);
        }

        [UnityTest]
        public IEnumerator HasKey_ReturnsRightValue()
        {
            ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadString);
            yield return null;
            Assert.That(ConfigManager.appConfig.HasKey("longSomething"));
        }

        [UnityTest]
        public IEnumerator HasKey_ReturnsRightValueWhenBadResponse()
        {
            var hasKeylongBeforeRequest = ConfigManager.appConfig.HasKey("longSomething");
            ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadStringNoRCSection);
            yield return null;
            Assert.That(ConfigManager.appConfig.HasKey("longSomething") == hasKeylongBeforeRequest);
        }

        [UnityTest]
        public IEnumerator GetKeys_ReturnsRightValue()
        {
            ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadString);
            yield return null;
            Assert.That(ConfigManager.appConfig.GetKeys().Length == ((JObject)JObject.Parse(ConfigManagerTestUtils.jsonPayloadString)["settings"]).Properties().Select(prop => prop.Name).ToArray<string>().Length);
        }

        [UnityTest]
        public IEnumerator GetKeys_ReturnsRightValueWhenBadResponse()
        {
            var keyLengthBeforeRequest = ConfigManager.appConfig.GetKeys().Length;
            ConfigManagerTestUtils.SendPayloadToConfigManager(ConfigManagerTestUtils.jsonPayloadStringNoRCSection);
            yield return null;
            Assert.That(ConfigManager.appConfig.GetKeys().Length == keyLengthBeforeRequest);
        }
   }

    internal static class ConfigManagerTestUtils
    {
        public const string userId = "some-user-id";
        public const string environmentId = "7d2e0e2d-4bcd-4b6e-8d5d-65d17a708db2";

        public struct UserAttributes
        {

        }

        public struct AppAttributes
        {

        }

        public static string jsonPayloadString =
            @"{
                ""prefs"": {
                    ""testInt"": 1,
                    ""boolField"": ""false"",
                    ""dwadwd"": ""test""
                },
                ""analytics"": {
                    ""enabled"": true
                },
                ""connect"": {
                    ""limit_user_tracking"": false,
                    ""player_opted_out"": false,
                    ""enabled"": true
                },
                ""performance"": {
                    ""enabled"": true
                },
                ""settings"": {
                    ""raywagagwawd"": """",
                    ""settingsConfig"": ""{\""hello\"":2.0,\""someInt\"":32,\""madBro\"":\""fdsfadsf\""}"",
                    ""fghfg"": ""ffgf"",
                    ""longSomething"": 9223372036854775806,
                    ""someInt"": 12,
                    ""helloe"": 0.12999999523162842,
                    ""blah"": ""blahd"",
                    ""bool"": true,
                    ""madBro"": ""madAF"",
                    ""jsonKeys"": ""settingsConfig,gameConfig"",
                    ""skiwdafsdwas"": ""hello""
                },
                ""settingsMetadata"": {
                    ""assignmentId"": ""a04fb7ec-26e4-4247-b8b4-70dd6967a858"",
                    ""environmentId"": ""7d2e0e2d-4bcd-4b6e-8d5d-65d17a708db2"",
                }
            }";

        public static string jsonPayloadStringNoRCSection =
            @"{
                ""prefs"": {
                    ""testInt"": 1,
                    ""boolField"": ""false"",
                    ""dwadwd"": ""test""
                },
                ""analytics"": {
                    ""enabled"": true
                },
                ""connect"": {
                    ""limit_user_tracking"": false,
                    ""player_opted_out"": false,
                    ""enabled"": true
                },
                ""performance"": {
                    ""enabled"": true
                }
            }";

        public static void SendPayloadToConfigManager(string payload)
        {
            var eventDelegate = (MulticastDelegate)typeof(ConfigManager).GetField("ResponseParsed", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            if (eventDelegate != null)
            {
                foreach (var handler in eventDelegate.GetInvocationList())
                {
                    handler.Method.Invoke(handler.Target, new object[] { new ConfigResponse { requestOrigin = ConfigOrigin.Remote, status = ConfigRequestStatus.Success }, JObject.Parse(payload) });
                }
            }
        }

        public static void SetAppIdOnCommonPayload()
        {
            FieldInfo commonFieldInfo = typeof(ConfigManager).GetField("commonPayload", BindingFlags.Static | BindingFlags.NonPublic);
            var common = commonFieldInfo.GetValue(null);
            FieldInfo appIdFieldInfo = common.GetType().GetField("appid");
            appIdFieldInfo.SetValue(common, "de2c88ca-80fc-448f-bfa9-ab598bf7a9e4");
            commonFieldInfo.SetValue(null, common);
        }
    }

    internal interface IRCTest
    {
        void StartTest();
    }
}