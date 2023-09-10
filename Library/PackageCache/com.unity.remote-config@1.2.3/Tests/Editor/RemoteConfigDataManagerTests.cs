using NUnit.Framework;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEditor;
using System;

namespace Unity.RemoteConfig.Editor.Tests
{
    internal class RemoteConfigDataManagerTests
    {
        [TearDown]
        public void TearDown()
        {
            var path = typeof(RemoteConfigDataManager)
                .GetField("k_PathToDataStore", BindingFlags.Static | BindingFlags.NonPublic)
                .GetValue(null) as string;
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.SaveAssets();
        }

        [Test]
        public void CheckAndCreateDataStore_ReturnsDataStore()
        {
            RemoteConfigDataManager dataManager = new RemoteConfigDataManager();
            Assert.That(dataManager.CheckAndCreateDataStore().GetType() == typeof(RemoteConfigDataStore));
        }

        [Test]
        public void InitDataStore_InitsAll()
        {
            var dataStore = RCTestUtils.GetDataStore();
            RemoteConfigDataManager dataManager = new RemoteConfigDataManager();

            var initDataStoreMethod =
                typeof(RemoteConfigDataManager).GetMethod("InitDataStore", BindingFlags.NonPublic |
                    BindingFlags.Instance);
            initDataStoreMethod?.Invoke(dataManager, new object[] { });

            Assert.That(dataStore.rsKeyList != null);
            Assert.That(dataStore.rsLastCachedKeyList != null);
            Assert.That(Equals(dataStore.currentEnvironment, "Release"));
            Assert.That(string.IsNullOrEmpty(dataStore.currentEnvironmentId));
            Assert.That(dataStore.environments != null);
            Assert.That(dataStore.rulesList != null);
            Assert.That(dataStore.lastCachedRulesList != null);
            Assert.That(dataStore.addedRulesIDs != null);
            Assert.That(dataStore.updatedRulesIDs != null);
            Assert.That(dataStore.deletedRulesIDs != null);

            Assert.That(dataStore._rsKeyList != null);
            Assert.That(dataStore._rsLastCachedKeyList != null);
            Assert.That(dataStore._rulesList != null);
            Assert.That(dataStore._lastCachedRulesList != null);
            Assert.That(dataStore._environments != null);
        }

        [Test]
        public void CheckAndCreateAssetFolder_CreatesAssetFolder()
        {
            RemoteConfigDataManager dataManager = new RemoteConfigDataManager();

            var path = typeof(RemoteConfigDataManager)
                .GetField("k_PathToDataStore", BindingFlags.Static | BindingFlags.NonPublic)
                .GetValue(dataManager) as string;
            Directory.Delete(path, true);
            Assert.That(!Directory.Exists(path));

            var checkAndCreateAssetFolderMethod =
                typeof(RemoteConfigDataManager).GetMethod("CheckAndCreateAssetFolder",
                    BindingFlags.NonPublic | BindingFlags.Instance);
            checkAndCreateAssetFolderMethod?.Invoke(dataManager, new object[] { path });
            Assert.That(Directory.Exists(path));
        }

        [Test]
        public void GetRSList_ReturnsRSList()
        {
            var dataStore = RCTestUtils.GetDataStore();
            dataStore.rsKeyList = new JArray(RCTestUtils.rsListWithMetadata);
            RemoteConfigDataManager dataManager = new RemoteConfigDataManager();
            Assert.That(Equals(dataManager.GetRSList(), dataStore.rsKeyList));
        }

        [Test]
        public void RSList_ReturnsCorrectKeysTypesAndValues()
        {
            var dataStore = RCTestUtils.GetDataStore();
            dataStore.rsKeyList = new JArray(RCTestUtils.rsListWithMetadata);
            foreach(var settingWithMetadata in dataStore.rsKeyList)
            {
                Assert.That(!string.IsNullOrEmpty(settingWithMetadata["metadata"]["entityId"].Value<string>()));
                switch (settingWithMetadata["rs"]["type"].Value<string>())
                {
                    case "bool":
                        Assert.That(settingWithMetadata["rs"]["key"].Value<string>().Equals(RCTestUtils.settingBool["key"].Value<string>()));
                        Assert.That(settingWithMetadata["rs"]["type"].Value<string>().Equals(RCTestUtils.settingBool["type"].Value<string>()));
                        Assert.That(settingWithMetadata["rs"]["value"].Value<string>().Equals(RCTestUtils.settingBool["value"].Value<string>()));
                        break;
                    case "int":
                        Assert.That(settingWithMetadata["rs"]["key"].Value<string>().Equals(RCTestUtils.settingInt["key"].Value<string>()));
                        Assert.That(settingWithMetadata["rs"]["type"].Value<string>().Equals(RCTestUtils.settingInt["type"].Value<string>()));
                        Assert.That(settingWithMetadata["rs"]["value"].Value<string>().Equals(RCTestUtils.settingInt["value"].Value<string>()));
                        break;
                    case "float":
                        Assert.That(settingWithMetadata["rs"]["key"].Value<string>().Equals(RCTestUtils.settingFloat["key"].Value<string>()));
                        Assert.That(settingWithMetadata["rs"]["type"].Value<string>().Equals(RCTestUtils.settingFloat["type"].Value<string>()));
                        Assert.That(settingWithMetadata["rs"]["value"].Value<string>().Equals(RCTestUtils.settingFloat["value"].Value<string>()));
                        break;
                    case "long":
                        Assert.That(settingWithMetadata["rs"]["key"].Value<string>().Equals(RCTestUtils.settingLong["key"].Value<string>()));
                        Assert.That(settingWithMetadata["rs"]["type"].Value<string>().Equals(RCTestUtils.settingLong["type"].Value<string>()));
                        Assert.That(settingWithMetadata["rs"]["value"].Value<string>().Equals(RCTestUtils.settingLong["value"].Value<string>()));
                        break;
                    case "string":
                        Assert.That(settingWithMetadata["rs"]["key"].Value<string>().Equals(RCTestUtils.settingString["key"].Value<string>()));
                        Assert.That(settingWithMetadata["rs"]["type"].Value<string>().Equals(RCTestUtils.settingString["type"].Value<string>()));
                        Assert.That(settingWithMetadata["rs"]["value"].Value<string>().Equals(RCTestUtils.settingString["value"].Value<string>()));
                        break;
                }
            }
        }

        [Test]
        public void GetCurrentEnvironmentName_ReturnsEnvironmentName()
        {
            var dataStore = RCTestUtils.GetDataStore();
            dataStore.currentEnvironment = RCTestUtils.currentEnvironment;
            RemoteConfigDataManager dataManager = new RemoteConfigDataManager();
            Assert.That(Equals(dataManager.GetCurrentEnvironmentName(), dataStore.currentEnvironment));
        }

        [Test]
        public void GetCurrentEnvironmentId_ReturnsCurrentEnvironmentId()
        {
            var dataStore = RCTestUtils.GetDataStore();
            dataStore.currentEnvironmentId = RCTestUtils.currentEnvironmentId;
            RemoteConfigDataManager dataManager = new RemoteConfigDataManager();
            Assert.That(Equals(dataManager.GetCurrentEnvironmentId(), dataStore.currentEnvironmentId));
        }

        [Test]
        public void GetCurrentEnvironmentIsDefault_ReturnsTrueForFirstEnvironment()
        {
            var dataStore = RCTestUtils.GetDataStore();
            var currentEnvironment = RCTestUtils.environment1;
            dataStore.currentEnvironmentIsDefault = currentEnvironment["isDefault"].Value<bool>();
            RemoteConfigDataManager dataManager = new RemoteConfigDataManager();
            Assert.That(Equals(dataManager.GetCurrentEnvironmentIsDefault(), dataStore.currentEnvironmentIsDefault));
        }

        [Test]
        public void SetDefaultEnvironment_ShouldSetDefaultEnvironment()
        {
            var dataStore = RCTestUtils.GetDataStore();
            var newDefaultEnvironment = RCTestUtils.environment2;
            dataStore.environments = RCTestUtils.environments;
            RemoteConfigDataManager dataManager = new RemoteConfigDataManager();
            dataManager.SetDefaultEnvironment(newDefaultEnvironment["id"].Value<string>());
            Assert.That(Equals(dataStore.environments[1]["isDefault"].Value<bool>(), true));
        }

        [Test]
        public void GetEnvironments_ReturnsListOfEnvironments()
        {
            var dataStore = RCTestUtils.GetDataStore();
            dataStore.environments = RCTestUtils.environments;
            RemoteConfigDataManager dataManager = new RemoteConfigDataManager();
            Assert.That(Equals(dataManager.GetEnvironments(), dataStore.environments));
        }

        [Test]
        public void GetRulesList_ReturnsRulesList()
        {
            var dataStore = RCTestUtils.GetDataStore();
            RemoteConfigDataManager dataManager = new RemoteConfigDataManager();
            dataManager.SetRulesDataStore(RCTestUtils.rulesList);
            Assert.That(Equals(dataManager.GetRulesList(), dataStore.rulesList));
        }

        [Test]
        public void SetRulesDataStore_SetsEntityIdOnKeys()
        {
            RemoteConfigDataManager dataManager = new RemoteConfigDataManager();
            dataManager.SetRulesDataStore(RCTestUtils.rulesList);
            var rulesList = dataManager.GetRulesList();
            foreach(var rule in rulesList)
            {
                foreach(var setting in rule["value"])
                {
                    Assert.That(!string.IsNullOrEmpty(setting["metadata"]["entityId"].Value<string>()));
                    switch (setting["rs"]["type"].Value<string>())
                    {
                        case "bool":
                            Assert.That(setting["rs"]["key"].Value<string>().Equals(RCTestUtils.settingBool["key"].Value<string>()));
                            break;
                        case "int":
                            Assert.That(setting["rs"]["key"].Value<string>().Equals(RCTestUtils.settingInt["key"].Value<string>()));
                            break;
                        case "float":
                            Assert.That(setting["rs"]["key"].Value<string>().Equals(RCTestUtils.settingFloat["key"].Value<string>()));
                            break;
                        case "long":
                            Assert.That(setting["rs"]["key"].Value<string>().Equals(RCTestUtils.settingLong["key"].Value<string>()));
                            break;
                        case "string":
                            Assert.That(setting["rs"]["key"].Value<string>().Equals(RCTestUtils.settingString["key"].Value<string>()));
                            break;
                    }
                }
            }
        }

        [Test]
        public void GetAddedRulesIDs_ReturnsAddedRuleIDList()
        {
            var dataStore = RCTestUtils.GetDataStore();
            dataStore.addedRulesIDs = new List<string>(RCTestUtils.addedRuleIDs);
            RemoteConfigDataManager dataManager = new RemoteConfigDataManager();
            Assert.That(Equals(dataManager.GetAddedRulesIDs(), dataStore.addedRulesIDs));
        }

        [Test]
        public void GetUpdatedRulesIDs_ReturnsUpdatedRuleIDList()
        {
            var dataStore = RCTestUtils.GetDataStore();
            dataStore.updatedRulesIDs = new List<string>(RCTestUtils.updatedRuleIDs);
            RemoteConfigDataManager dataManager = new RemoteConfigDataManager();
            Assert.That(Equals(dataManager.GetUpdatedRulesIDs(), dataStore.updatedRulesIDs));
        }

        [Test]
        public void GetDeletedRulesIDs_ReturnsUpdatedRuleIDList()
        {
            var dataStore = RCTestUtils.GetDataStore();
            dataStore.deletedRulesIDs = new List<string>(RCTestUtils.deletedRuleIDs);
            RemoteConfigDataManager dataManager = new RemoteConfigDataManager();
            Assert.That(Equals(dataManager.GetDeletedRulesIDs(), dataStore.deletedRulesIDs));
        }

        [Test]
        public void GetRuleAtIndex_ReturnsRuleAtIndex()
        {
            var dataStore = RCTestUtils.GetDataStore();
            RemoteConfigDataManager dataManager = new RemoteConfigDataManager();
            dataManager.SetRulesDataStore(RCTestUtils.rulesList);
            for (int i = 0; i < dataStore.rulesList.Count; i++)
            {
                Assert.That(Equals(dataManager.GetRuleAtIndex(i), dataStore.rulesList[i]));
            }
        }

        [Test]
        public void GetRuleByID_ReturnsRuleWithGivenID()
        {
            var dataStore = RCTestUtils.GetDataStore();
            RemoteConfigDataManager dataManager = new RemoteConfigDataManager();
            dataManager.SetRulesDataStore(RCTestUtils.rulesList);
            for (int i = 0; i < dataStore.rulesList.Count; i++)
            {
                Assert.That(Equals(dataManager.GetRuleByID(dataStore.rulesList[i]["id"].Value<string>()), dataStore.rulesList[i]));
            }
        }

        [Test]
        public void SetCurrentEnvironment_SetsCurrentEnvironment()
        {
            var dataStore = RCTestUtils.GetDataStore();
            RemoteConfigDataManager dataManager = new RemoteConfigDataManager();

            var currentEnvironment = new JObject();
            currentEnvironment["name"] = RCTestUtils.currentEnvironment;
            currentEnvironment["id"] = RCTestUtils.currentEnvironmentId;

            dataManager.SetCurrentEnvironment(currentEnvironment);

            Assert.That(Equals(dataStore.currentEnvironment, currentEnvironment["name"].Value<string>()));
            Assert.That(Equals(dataStore.currentEnvironmentId, currentEnvironment["id"].Value<string>()));
        }

        [Test]
        public void SetEnvironmentsList_SetsEnvironmentsList()
        {
            var dataStore = RCTestUtils.GetDataStore();
            RemoteConfigDataManager dataManager = new RemoteConfigDataManager();
            dataManager.SetEnvironmentsList(RCTestUtils.environments);

            Assert.That(Equals(dataStore.environments, RCTestUtils.environments));
        }

        [Test]
        public void SetRSDataStore_SetsRSDataStoreWhenAListIsPassedIn()
        {
            var dataStore = RCTestUtils.GetDataStore();
            RemoteConfigDataManager dataManager = new RemoteConfigDataManager();
            var config = new JObject();
            config["type"] = "settings";
            config["id"] = "someId";
            config["value"] = RCTestUtils.rsListNoMetadata;
            dataManager.SetRSDataStore(config);
            Assert.That(RCTestUtils.rsListNoMetadata.Count.Equals(dataStore.rsKeyList.Count));
            for(int i = 0; i < RCTestUtils.rsListNoMetadata.Count; i++)
            {
                Assert.That(!string.IsNullOrEmpty(dataStore.rsKeyList[i]["metadata"]["entityId"].Value<string>()));
                Assert.That(dataStore.rsKeyList[i]["rs"]["key"].Equals(RCTestUtils.rsListNoMetadata[i]["key"]));
                Assert.That(dataStore.rsKeyList[i]["rs"]["value"].Equals(RCTestUtils.rsListNoMetadata[i]["value"]));
            }
        }

        [Test]
        public void SetRulesDataStore_SetsRulesStoreWhenAListIsPassedInWithoutSettings()
        {
            var dataStore = RCTestUtils.GetDataStore();
            RemoteConfigDataManager dataManager = new RemoteConfigDataManager();
            dataStore.rulesList = new JArray();
            dataManager.SetRulesDataStore(RCTestUtils.rulesWithNoSettingsList);
            Assert.That(Equals(dataManager.GetRulesList(), dataStore.rulesList));
            for(int i = 0; i < RCTestUtils.rulesWithNoSettingsList.Count; i++)
            {
                Assert.That(dataStore.rulesList[i]["id"].Value<string>().Equals(RCTestUtils.rulesWithNoSettingsList[i]["id"].Value<string>()));
                Assert.That(dataStore.rulesList[i]["name"].Value<string>().Equals(RCTestUtils.rulesWithNoSettingsList[i]["name"].Value<string>()));
            }
        }

        [Test]
        public void SetRulesDataStore_SetsRulesStoreWhenAListIsPassedInWithSettings()
        {
            var dataStore = RCTestUtils.GetDataStore();
            RemoteConfigDataManager dataManager = new RemoteConfigDataManager();
            dataStore.rulesList = new JArray();
            dataManager.SetRulesDataStore(RCTestUtils.rulesWithSettingsList);
            for(int i = 0; i < RCTestUtils.rulesWithSettingsList.Count; i++)
            {
                Assert.That(dataStore.rulesList[i]["id"].Value<string>().Equals(RCTestUtils.rulesWithSettingsList[i]["id"].Value<string>()));
                Assert.That(dataStore.rulesList[i]["name"].Value<string>().Equals(RCTestUtils.rulesWithSettingsList[i]["name"].Value<string>()));
            }
        }

        [Test]
        public void SetRulesDataStore_SetsRulesStoreWhenAListIsPassedInWithAndWithoutSettings()
        {
            var dataStore = RCTestUtils.GetDataStore();
            RemoteConfigDataManager dataManager = new RemoteConfigDataManager();
            dataStore.rulesList = new JArray();
            dataManager.SetRulesDataStore(RCTestUtils.rulesList);
            Assert.That(Equals(dataManager.GetRulesList(), dataStore.rulesList));
            for(int i = 0; i < RCTestUtils.rulesList.Count; i++)
            {
                Assert.That(dataStore.rulesList[i]["id"].Value<string>().Equals(RCTestUtils.rulesList[i]["id"].Value<string>()));
                Assert.That(dataStore.rulesList[i]["name"].Value<string>().Equals(RCTestUtils.rulesList[i]["name"].Value<string>()));
            }
        }

        [Test]
        public void SetRulesDataStore_SetsRulesStoreWhenAListIsPassedInWithFormattedMetadata()
        {
            var dataStore = RCTestUtils.GetDataStore();
            RemoteConfigDataManager dataManager = new RemoteConfigDataManager();
            dataStore.rulesList = new JArray();
            dataManager.SetRulesDataStore(RCTestUtils.rulesWithSettingsMetadata);
            Assert.That(Equals(dataManager.GetRulesList(), dataStore.rulesList));
            for(int i = 0; i < RCTestUtils.rulesWithSettingsMetadata.Count; i++)
            {
                Assert.That(dataStore.rulesList[i]["id"].Value<string>().Equals(RCTestUtils.rulesWithSettingsMetadata[i]["id"].Value<string>()));
                Assert.That(dataStore.rulesList[i]["name"].Value<string>().Equals(RCTestUtils.rulesWithSettingsMetadata[i]["name"].Value<string>()));
            }
        }

        [Test]
        public void SetRulesDataStore_SetsRulesStoreWhenAListIsPassedInWithAndWithoutFormattedMetadata()
        {
            var dataStore = RCTestUtils.GetDataStore();
            RemoteConfigDataManager dataManager = new RemoteConfigDataManager();
            dataStore.rulesList = new JArray();
            dataManager.SetRulesDataStore(RCTestUtils.rulesWithAndWithoutSettingsMetadata);
            Assert.That(Equals(dataManager.GetRulesList(), dataStore.rulesList));
            for(int i = 0; i < RCTestUtils.rulesWithAndWithoutSettingsMetadata.Count; i++)
            {
                Assert.That(dataStore.rulesList[i]["id"].Value<string>().Equals(RCTestUtils.rulesWithAndWithoutSettingsMetadata[i]["id"].Value<string>()));
                Assert.That(dataStore.rulesList[i]["name"].Value<string>().Equals(RCTestUtils.rulesWithAndWithoutSettingsMetadata[i]["name"].Value<string>()));
            }
        }

       [Test]
        public void SetRulesDataStore_SetsCorrectEntityIdOnSettingsInRule()
        {
            var dataStore = RCTestUtils.GetDataStore();
            RemoteConfigDataManager dataManager = new RemoteConfigDataManager();
            var config = new JObject();
            config["type"] = "settings";
            config["id"] = "someId";
            config["value"] = RCTestUtils.rsListNoMetadata;
            dataManager.SetRSDataStore(config);
            dataManager.SetRulesDataStore(RCTestUtils.rulesWithSettingsList);

            var rulesFromDataStore = dataStore.rulesList;
            var settingsFromDataStore = dataStore.rsKeyList;

            for(int i = 0; i < rulesFromDataStore.Count; i++)
            {
                var settingsForCurrentRule = rulesFromDataStore[i]["value"];
                for(int j = 0; j < settingsForCurrentRule.Count(); j++)
                {
                    Assert.That(settingsForCurrentRule[j]["rs"]["key"].Equals(settingsFromDataStore[j]["rs"]["key"]));
                    Assert.That(!string.IsNullOrEmpty(settingsForCurrentRule[j]["metadata"]["entityId"].Value<string>()));
                }
            }
        }

        [Test]
        public void DeleteRule_DeletesRuleFromRulesListAndAddsRuleToDeletedRuleIDs()
        {
            var dataStore = RCTestUtils.GetDataStore();
            RemoteConfigDataManager dataManager = new RemoteConfigDataManager();
            dataStore.rulesList = new JArray();
            dataManager.SetRulesDataStore(RCTestUtils.rulesWithSettingsList);
            var deletedRule = RCTestUtils.rulesWithSettingsList[0];

            dataManager.DeleteRule(deletedRule["id"].Value<string>());

            Assert.That(!dataStore.rulesList.Contains(deletedRule));
            Assert.That(dataStore.deletedRulesIDs.Contains(deletedRule["id"].Value<string>()));
            Assert.That(!dataStore.rulesList.Contains(deletedRule));
        }

        [Test]
        public void UpdateRuleAttributes_ShouldUpdateRule()
        {
            var dataStore = RCTestUtils.GetDataStore();
            dataStore.rulesList = new JArray();
            RemoteConfigDataManager dataManager = new RemoteConfigDataManager();
            dataManager.SetRulesDataStore(RCTestUtils.rulesWithNoSettingsList);
            var oldRule = (JObject)RCTestUtils.rulesWithNoSettingsList[0];
            var newRule = (JObject)RCTestUtils.rulesWithNoSettingsList[1];
            newRule["id"] = "very-new-id";
            newRule["name"] = "very-new-name";

            dataManager.UpdateRuleAttributes(oldRule["id"].Value<string>(), newRule);
            Assert.That(dataStore.rulesList.Any(r => r["id"].Value<string>() == newRule["id"].Value<string>()));
            Assert.That(!dataStore.rulesList.Any(r => r["id"].Value<string>() == oldRule["id"].Value<string>()));
            var rulesFromList = new JArray(dataStore.rulesList.Where(r => r["id"].Value<string>() == newRule["id"].Value<string>()));
            Assert.That(rulesFromList[0]["name"].Value<string>() == newRule["name"].Value<string>());
            Assert.That(rulesFromList[0]["enabled"].Value<bool>() == newRule["enabled"].Value<bool>());
        }

        [Test]
        public void EnableOrDisableRule_UpdatesEnabledFieldOfRule()
        {
            var dataStore = RCTestUtils.GetDataStore();
            dataStore.rulesList = new JArray();
            RemoteConfigDataManager dataManager = new RemoteConfigDataManager();
            dataManager.SetRulesDataStore(RCTestUtils.rulesWithNoSettingsList);
            var currentRule = (JObject)RCTestUtils.rulesWithNoSettingsList[0];
            var currentRuleId = currentRule["id"].Value<string>();
            var currentRuleEnabled = currentRule["enabled"].Value<bool>();

            dataManager.EnableOrDisableRule(currentRuleId, !currentRuleEnabled);
            var rulesFromList = new JArray(dataStore.rulesList.Where(r => r["id"].Value<string>() == currentRuleId));
            Assert.That(rulesFromList[0]["enabled"].Value<bool>() == !currentRuleEnabled);
        }

        [Test]
        public void AddSettingToRule_ShouldAddRightSettingToRightRule()
        {
            var dataStore = RCTestUtils.GetDataStore();
            dataStore.rulesList = new JArray();
            RemoteConfigDataManager dataManager = new RemoteConfigDataManager();
            dataManager.SetRulesDataStore(RCTestUtils.rulesWithNoSettingsList);
            dataStore.rsKeyList = new JArray(RCTestUtils.rsListWithMetadata);
            var currentRule = (JObject)RCTestUtils.rulesWithNoSettingsList[0];
            var currentRuleId = currentRule["id"].Value<string>();

            dataManager.AddSettingToRule(currentRuleId, RCTestUtils.EntityIdString);
            var rulesFromList = new JArray(dataStore.rulesList.Where(r => r["id"].Value<string>() == currentRuleId));
            Assert.That(rulesFromList[0]["value"][0]["rs"]["key"].Value<string>() == RCTestUtils.settingString["key"].Value<string>());
        }

        [Test]
        public void DeleteSettingFromRule_ShouldRemoveRightSettingsFromRightRule()
        {
            var dataStore = RCTestUtils.GetDataStore();
            dataStore.rulesList = new JArray();
            RemoteConfigDataManager dataManager = new RemoteConfigDataManager();
            dataManager.SetRulesDataStore(RCTestUtils.rulesWithSettingsList);
            var currentRule = (JObject)RCTestUtils.rulesWithSettingsList[0];
            var currentRuleId = currentRule["id"].Value<string>();
            var rulesFromList = new JArray(dataStore.rulesList.Where(r => r["id"].Value<string>() == currentRuleId));
            var currentSettings = rulesFromList[0]["value"];

            dataManager.DeleteSettingFromRule(currentRuleId, currentSettings[0]["metadata"]["entityId"].Value<string>());
            Assert.That(dataStore.updatedRulesIDs[0].Equals(currentRuleId));

            var rulesFromListAfter = new JArray(dataStore.rulesList.Where(r => r["id"].Value<string>() == currentRuleId));
            var currentSettingsAfter = rulesFromListAfter[0]["value"];
            var hasBoolkey = false;
            for (int i = 0; i < currentSettingsAfter.Count(); i++)
            {
                if (currentSettingsAfter[i]["rs"]["key"].Value<string>() == RCTestUtils.settingBool["key"].Value<string>())
                {
                    hasBoolkey = true;
                }
            }
            Assert.That(hasBoolkey.Equals(false));
        }

        [Test]
        public void UpdateSettingForRule_ShouldUpdateSettingOnRightRule()
        {
            var dataStore = RCTestUtils.GetDataStore();
            dataStore.rulesList = new JArray();
            RemoteConfigDataManager dataManager = new RemoteConfigDataManager();
            dataManager.SetRulesDataStore(RCTestUtils.rulesWithSettingsList);
            var currentRule = (JObject)RCTestUtils.rulesWithSettingsList[0];
            var currentRuleId = currentRule["id"].Value<string>();
            var rulesFromList = new JArray(dataStore.rulesList.Where(r => r["id"].Value<string>() == currentRuleId));
            var currentSettings = rulesFromList[0]["value"];

            var newSetting = currentSettings[0].DeepClone();
            newSetting["rs"]["key"] = "SettingUpdated";
            dataManager.UpdateSettingForRule(currentRuleId, (JObject)newSetting);
            Assert.That(dataStore.updatedRulesIDs[0].Equals(currentRuleId));

            var rulesFromListAfter = new JArray(dataStore.rulesList.Where(r => r["id"].Value<string>() == currentRuleId));
            var currentSettingsAfter = rulesFromListAfter[0]["value"];
            var settingAfter = currentSettingsAfter[0];
            Assert.That(Equals(settingAfter["rs"]["key"], newSetting["rs"]["key"]));
        }

        [Test]
        public void RemoveRuleFromAddedRuleIDs_RemovesRuleFromAddedRules()
        {
            var dataStore = RCTestUtils.GetDataStore();
            dataStore.addedRulesIDs = new List<string>(RCTestUtils.updatedRuleIDs);
            RemoteConfigDataManager dataManager = new RemoteConfigDataManager();
            dataManager.RemoveRuleFromAddedRuleIDs(RCTestUtils.updatedRuleIDs[0]);
            Assert.That(dataStore.addedRulesIDs.Count == 2);
            Assert.That(!dataStore.addedRulesIDs.Contains(RCTestUtils.updatedRuleIDs[0]));
        }

        [Test]
        public void RemoveRuleFromUpdatedRuleIDs_RemovesRuleFromUpdatedRules()
        {
            var dataStore = RCTestUtils.GetDataStore();
            dataStore.updatedRulesIDs = new List<string>(RCTestUtils.updatedRuleIDs);
            RemoteConfigDataManager dataManager = new RemoteConfigDataManager();
            dataManager.RemoveRuleFromUpdatedRuleIDs(RCTestUtils.updatedRuleIDs[0]);
            Assert.That(dataStore.updatedRulesIDs.Count == 2);
            Assert.That(!dataStore.updatedRulesIDs.Contains(RCTestUtils.updatedRuleIDs[0]));
        }

        [Test]
        public void RemoveRuleFromDeletedRuleIDs_RemovesRuleFromDeletedRules()
        {
            var dataStore = RCTestUtils.GetDataStore();
            dataStore.deletedRulesIDs = new List<string>(RCTestUtils.updatedRuleIDs);
            RemoteConfigDataManager dataManager = new RemoteConfigDataManager();
            dataManager.RemoveRuleFromDeletedRuleIDs(RCTestUtils.updatedRuleIDs[0]);
            Assert.That(dataStore.deletedRulesIDs.Count == 2);
            Assert.That(!dataStore.deletedRulesIDs.Contains(RCTestUtils.updatedRuleIDs[0]));
        }

        [Test]
        public void ClearRuleIDs_ClearsAllLists()
        {
            var dataStore = RCTestUtils.GetDataStore();
            dataStore.updatedRulesIDs = new List<string>(RCTestUtils.updatedRuleIDs);
            dataStore.addedRulesIDs = new List<string>(RCTestUtils.addedRuleIDs);
            dataStore.deletedRulesIDs = new List<string>(RCTestUtils.deletedRuleIDs);
            RemoteConfigDataManager dataManager = new RemoteConfigDataManager();
            dataManager.ClearRuleIDs();
            Assert.That(dataStore.updatedRulesIDs.Count == 0);
            Assert.That(dataStore.addedRulesIDs.Count == 0);
            Assert.That(dataStore.deletedRulesIDs.Count == 0);
        }

        [Test]
        public void AddSetting_AddsSettingToListAndDict()
        {
            var dataStore = RCTestUtils.GetDataStore();
            RemoteConfigDataManager dataManager = new RemoteConfigDataManager();
            dataStore.rsKeyList = new JArray(RCTestUtils.rsListWithMetadata);

            var newSetting = new JObject();
            newSetting["metadata"] = new JObject();
            newSetting["metadata"]["entityId"] = "new-setting-entitity-id";
            newSetting["rs"] = new JObject();
            newSetting["rs"]["key"] = "NewSetting";
            newSetting["rs"]["value"] = "NewValue";
            newSetting["rs"]["type"] = "string";

            dataManager.AddSetting(newSetting);
            var settingsFromList = new JArray(dataStore.rsKeyList.Where(s => s["rs"]["key"].Value<string>() == newSetting["rs"]["key"].Value<string>()));
            var addedSetting = settingsFromList[0];
            Assert.That(Equals(addedSetting["rs"]["key"], newSetting["rs"]["key"]));
            Assert.That(Equals(addedSetting["rs"]["value"], newSetting["rs"]["value"]));
            Assert.That(Equals(addedSetting["rs"]["type"], newSetting["rs"]["type"]));
        }

        [Test]
        public void DeleteSetting_DeletesSettingFromListAndDict()
        {
            var dataStore = RCTestUtils.GetDataStore();
            RemoteConfigDataManager dataManager = new RemoteConfigDataManager();
            dataStore.rsKeyList = new JArray(RCTestUtils.rsListWithMetadata);

            dataManager.DeleteSetting(RCTestUtils.rsListWithMetadata[0]["metadata"]["entityId"].Value<string>());
            Assert.That(!dataStore.rsKeyList.Contains(RCTestUtils.rsListWithMetadata[0]));
        }

        [Test]
        public void UpdateSetting_UpdatesCorrectSetting()
        {
            var dataStore = RCTestUtils.GetDataStore();
            RemoteConfigDataManager dataManager = new RemoteConfigDataManager();
            dataStore.rsKeyList = new JArray(RCTestUtils.rsListWithMetadata);

            var newSetting = new JObject();
            newSetting["metadata"] = new JObject();
            newSetting["metadata"]["entityId"] = "new-setting-entitity-id";
            newSetting["rs"] = new JObject();
            newSetting["rs"]["key"] = "NewSetting";
            newSetting["rs"]["value"] = "NewValue";
            newSetting["rs"]["type"] = "string";

            var oldSetting = (JObject)dataStore.rsKeyList[0];

            dataManager.UpdateSetting(oldSetting, newSetting);
            Assert.That(!dataStore.rsKeyList.Contains(oldSetting));
            Assert.That(dataStore.rsKeyList.Contains(newSetting));

            var settingsFromList = new JArray(dataStore.rsKeyList.Where(s => s["rs"]["key"].Value<string>() == newSetting["rs"]["key"].Value<string>()));
            var updatedSetting = settingsFromList[0];
            Assert.That(Equals(updatedSetting["rs"]["key"], newSetting["rs"]["key"]));
            Assert.That(Equals(updatedSetting["rs"]["value"], newSetting["rs"]["value"]));
            Assert.That(Equals(updatedSetting["rs"]["type"], newSetting["rs"]["type"]));
        }

        [Test]
        public void HasRules_CorrectlyReturnsTrueIfThereAreRules()
        {
            var dataStore = RCTestUtils.GetDataStore();
            dataStore.rulesList = new JArray();
            RemoteConfigDataManager dataManager = new RemoteConfigDataManager();
            dataManager.SetRulesDataStore(RCTestUtils.rulesWithNoSettingsList);
            Assert.That(dataManager.HasRules());
        }

        [Test]
        public void HasRules_CorrectlyReturnsFalseIfThereAreNoRules()
        {
            var dataStore = RCTestUtils.GetDataStore();
            dataStore.rulesList = new JArray();
            RemoteConfigDataManager dataManager = new RemoteConfigDataManager();
            Assert.That(dataManager.HasRules() == false);
        }

        [Test]
        public void IsSettingInRule_ReturnsTrueWhenSettingIsInRule()
        {
            var dataStore = RCTestUtils.GetDataStore();
            dataStore.rulesList = new JArray();
            RemoteConfigDataManager dataManager = new RemoteConfigDataManager();
            dataManager.SetRulesDataStore(RCTestUtils.rulesWithSettingsList);

            var currentRule = dataStore.rulesList[0];
            var currentRuleId = currentRule["id"].Value<string>();
            var currentSettingEntityId = currentRule["value"][0]["metadata"]["entityId"].Value<string>();
            Assert.That(dataManager.IsSettingInRule(currentRuleId, currentSettingEntityId));
        }

        [Test]
        public void IsSettingInRule_ReturnsFalseWhenSettingIsNotInRule()
        {
            var dataStore = RCTestUtils.GetDataStore();
            dataStore.rulesList = new JArray();
            RemoteConfigDataManager dataManager = new RemoteConfigDataManager();
            dataManager.SetRulesDataStore(RCTestUtils.rulesWithSettingsList);

            var currentRule = dataStore.rulesList[0];
            var currentRuleId = currentRule["id"].Value<string>();
            Assert.That(!dataManager.IsSettingInRule(currentRuleId, "non-existing-entityId"));
        }

        [Test]
        public void ValidateRule_ShouldReturnTrueForValidRule()
        {
            RemoteConfigDataManager dataManager = new RemoteConfigDataManager();
            Assert.That(dataManager.ValidateRule(RCTestUtils.ruleNoValues1));
        }

        [Test]
        public void ValidateRule_ShouldReturnFalseForInvalidRule()
        {
            RemoteConfigDataManager dataManager = new RemoteConfigDataManager();
            var rule = (JObject)RCTestUtils.ruleNoValues1.DeepClone();
            rule["priority"] = 1001;
            Assert.That(dataManager.ValidateRule(rule) == false);
            rule["priority"] = -1;
            Assert.That(dataManager.ValidateRule(rule) == false);
        }

        [Test]
        public void ConfigID_ShouldReturnConfigIDFromDataStore()
        {
            var dataStore = RCTestUtils.GetDataStore();
            RemoteConfigDataManager dataManager = new RemoteConfigDataManager();
            Assert.That(string.IsNullOrEmpty(dataManager.configId));
            dataStore.configId = "someId";
            Assert.That(string.Equals(dataManager.configId, "someId"));
        }

        [Test]
        public void ValidateRule_ShouldReturnFalseForAddingDuplicateRuleName()
        {
            var dataStore = RCTestUtils.GetDataStore();
            RemoteConfigDataManager dataManager = new RemoteConfigDataManager();
            dataStore.rulesList = new JArray();
            var rule1 = (JObject)RCTestUtils.ruleNoValues1.DeepClone();
            dataManager.AddRule(rule1);
            var rule2 = (JObject)RCTestUtils.ruleNoValues2.DeepClone();
            dataManager.AddRule(rule2);
            rule2["name"] = rule1["name"].Value<string>();
            Assert.That(dataManager.ValidateRule(rule2) == false);
        }

        [Test]
        public void UpdateRuleType_ShouldReturnRuleOfNewTypeVariant()
        {
            var dataStore = RCTestUtils.GetDataStore();
            dataStore.rulesList = new JArray();
            RemoteConfigDataManager dataManager = new RemoteConfigDataManager();
            dataManager.SetRulesDataStore(RCTestUtils.rulesWithNoSettingsList);
            var currRule = (JObject)RCTestUtils.rulesWithNoSettingsList[0];

            var newType = "variant";
            dataManager.UpdateRuleType(currRule["id"].Value<string>(), newType);

            Assert.That(dataStore.rulesList.Any(r => r["id"].Value<string>() == currRule["id"].Value<string>()));
            var rulesFromList = new JArray(dataStore.rulesList.Where(r => r["id"].Value<string>() == currRule["id"].Value<string>()));
            Assert.That(rulesFromList[0]["name"].Value<string>() == currRule["name"].Value<string>());
            Assert.That(rulesFromList[0]["type"].Value<string>() == newType);
        }

//        this test is commented as we have confirmation dialog when switching rule type from 'variant' to 'segmentation'
//        [Test]
//        public void UpdateRuleType_ShouldReturnRuleOfNewTypeSegmentation()
//        {
//            var dataStore = RCTestUtils.GetDataStore();
//            dataStore.rulesList = new JArray();
//            RemoteConfigDataManager dataManager = new RemoteConfigDataManager();
//            dataManager.SetRulesDataStore(RCTestUtils.rulesWithNoSettingsList);
//            var currRule = (JObject)RCTestUtils.rulesWithNoSettingsList[0];
//
//            var newType = "variant";
//            dataManager.UpdateRuleType(currRule["id"].Value<string>(), newType);
//
//            Assert.That(dataStore.rulesList.Any(r => r["id"].Value<string>() == currRule["id"].Value<string>()));
//            var rulesFromList = new JArray(dataStore.rulesList.Where(r => r["id"].Value<string>() == currRule["id"].Value<string>()));
//            Assert.That(rulesFromList[0]["name"].Value<string>() == currRule["name"].Value<string>());
//            Assert.That(rulesFromList[0]["type"].Value<string>() == newType);
//
//            newType = "segmentation";
//            dataManager.UpdateRuleType(currRule["id"].Value<string>(), newType);
//
//            Assert.That(dataStore.rulesList.Any(r => r["id"].Value<string>() == currRule["id"].Value<string>()));
//            rulesFromList = new JArray(dataStore.rulesList.Where(r => r["id"].Value<string>() == currRule["id"].Value<string>()));
//            Assert.That(rulesFromList[0]["name"].Value<string>() == currRule["name"].Value<string>());
//            Assert.That(rulesFromList[0]["type"].Value<string>() == newType);
//        }

        [Test]
        public void UpdateRuleTypeForSameType_ShouldReturnSameRuleTypeSegmentation()
        {
            var dataStore = RCTestUtils.GetDataStore();
            dataStore.rulesList = new JArray();
            RemoteConfigDataManager dataManager = new RemoteConfigDataManager();
            dataManager.SetRulesDataStore(RCTestUtils.rulesWithNoSettingsList);
            var currRule = (JObject)RCTestUtils.rulesWithNoSettingsList[0];

            var newType = "segmentation";
            dataManager.UpdateRuleType(currRule["id"].Value<string>(), newType);

            Assert.That(dataStore.rulesList.Any(r => r["id"].Value<string>() == currRule["id"].Value<string>()));
            var rulesFromList = new JArray(dataStore.rulesList.Where(r => r["id"].Value<string>() == currRule["id"].Value<string>()));
            Assert.That(rulesFromList[0]["name"].Value<string>() == currRule["name"].Value<string>());
            Assert.That(rulesFromList[0]["type"].Value<string>() == newType);
        }

        [Test]
        public void UpdateRuleTypeForSameType_ShouldReturnSameRuleTypeVariant()
        {
            var dataStore = RCTestUtils.GetDataStore();
            dataStore.rulesList = new JArray();
            RemoteConfigDataManager dataManager = new RemoteConfigDataManager();
            dataManager.SetRulesDataStore(RCTestUtils.rulesWithNoSettingsList);
            var currRule = (JObject)RCTestUtils.rulesWithNoSettingsList[0];

            // segmentation to variant
            var newType = "variant";
            dataManager.UpdateRuleType(currRule["id"].Value<string>(), newType);

            Assert.That(dataStore.rulesList.Any(r => r["id"].Value<string>() == currRule["id"].Value<string>()));
            var rulesFromList = new JArray(dataStore.rulesList.Where(r => r["id"].Value<string>() == currRule["id"].Value<string>()));
            Assert.That(rulesFromList[0]["name"].Value<string>() == currRule["name"].Value<string>());
            Assert.That(rulesFromList[0]["type"].Value<string>() == newType);

            // variant to variant
            newType = "variant";
            dataManager.UpdateRuleType(currRule["id"].Value<string>(), newType);

            Assert.That(dataStore.rulesList.Any(r => r["id"].Value<string>() == currRule["id"].Value<string>()));
            rulesFromList = new JArray(dataStore.rulesList.Where(r => r["id"].Value<string>() == currRule["id"].Value<string>()));
            Assert.That(rulesFromList[0]["name"].Value<string>() == currRule["name"].Value<string>());
            Assert.That(rulesFromList[0]["type"].Value<string>() == newType);
        }

        [Test]
        public void AddingRuleOfVariantTypeInDataStore_ShouldAddVariantTypeToRulesList()
        {
            var dataStore = RCTestUtils.GetDataStore();
            dataStore.rulesList = new JArray();
            RemoteConfigDataManager dataManager = new RemoteConfigDataManager();
            dataManager.SetRulesDataStore(RCTestUtils.rulesWithNoSettingsList);

            var mixedTypeRulesList = new JArray();
            var currRule = (JObject)RCTestUtils.rulesWithNoSettingsList[0];
            var newType = "variant";
            dataManager.UpdateRuleType(currRule["id"].Value<string>(), newType);

            var rulesFromList = new JArray(dataStore.rulesList.Where(r => r["id"].Value<string>() == currRule["id"].Value<string>()));
            var variantRule = rulesFromList[0];

            mixedTypeRulesList.Add((JObject)RCTestUtils.rulesWithNoSettingsList[1]); // type segmentation
            mixedTypeRulesList.Add(variantRule); // type variant

            dataManager.SetRulesDataStore(mixedTypeRulesList);
            Assert.That(mixedTypeRulesList.Count == dataStore.rulesList.Count);
            Assert.That(dataStore.rulesList[0]["type"].Value<string>() == "segmentation");
            Assert.That(dataStore.rulesList[1]["type"].Value<string>() == newType);
        }
    }

    internal static class RCTestUtils
    {
        public const string EntityIdBool = "bool0000-0000-0000-0000-000000000000";
        public const string EntityIdInt = "int00000-0000-0000-0000-000000000000";
        public const string EntityIdLong = "long0000-0000-0000-0000-000000000000";
        public const string EntityIdFloat = "float000-0000-0000-0000-000000000000";
        public const string EntityIdString = "string00-0000-0000-0000-000000000000";
        public const int IntValue = 1;
        public const float FloatValue = 1.0f;
        public const bool BoolValue = true;
        public const long LongValue = 32L;
        public const string StringValue = "test-string";

        public const string currentEnvironment = "test-environment";
        public const string currentEnvironmentId = "test-environment-id";

        public static JObject settingBool = new JObject
        {
            {"key", "SettingBool"},
            {"type", "bool"},
            {"value", BoolValue}
        };

        public static JObject settingInt = new JObject
        {
            {"key", "SettingInt"},
            {"type", "int"},
            {"value", IntValue}
        };

        public static JObject settingLong = new JObject
        {
            {"key", "settingLong"},
            {"type", "long"},
            {"value", LongValue}
        };

        public static JObject settingFloat = new JObject
        {
            {"key", "SettingFloat"},
            {"type", "float"},
            {"value", FloatValue}
        };

        public static JObject settingString = new JObject
        {
            {"key", "SettingString"},
            {"type", "string"},
            {"value", StringValue}
        };

        public static JObject settingBoolWithMetadata = new JObject
        {
            {"metadata", new JObject {{"entityId", EntityIdBool}}},
            {"rs", settingBool}
        };

        public static JObject settingIntWithMetadata = new JObject
        {
            {"metadata", new JObject {{"entityId", EntityIdInt}}},
            {"rs", settingInt}
        };

        public static JObject settingLongWithMetadata = new JObject
        {
            {"metadata", new JObject {{"entityId", EntityIdLong}}},
            {"rs", settingLong}
        };

        public static JObject settingFloatWithMetadata = new JObject
        {
            {"metadata", new JObject {{"entityId", EntityIdFloat}}},
            {"rs", settingFloat}
        };

        public static JObject settingStringWithMetadata = new JObject
        {
            {"metadata", new JObject {{"entityId", EntityIdString}}},
            {"rs", settingString}
        };


        // settings
        public static JArray rsListWithMetadata = new JArray
        {
            settingBoolWithMetadata,
            settingIntWithMetadata,
            settingLongWithMetadata,
            settingFloatWithMetadata,
            settingStringWithMetadata
        };

        public static JArray rsListNoMetadata = new JArray {
            settingBool,
            settingInt,
            settingLong,
            settingFloat,
            settingString
        };

        // rules
        public static JObject rule1 = new JObject
        {
            {"id", "rule-id-1"},
            {"name", "rule-name-1"},
            {"enabled", true},
            {"priority", 1000},
            {"condition", "true"},
            {"rolloutPercentage", 100},
            {"startDate", "2019-07-10T23:15:14.000-0700"},
            {"endDate", "2019-08-12T08:15:14.000+0430"},
            {"value", new JObject { }}
        };

        public static JObject rule2 = new JObject
        {
            {"id", "rule-id-2"},
            {"name", "rule-name-2"},
            {"enabled", true},
            {"priority", 1000},
            {"condition", "true"},
            {"rolloutPercentage", 100},
            {"startDate", "2019-07-10T23:15:14.000-0700"},
            {"endDate", "2019-08-12T08:15:14.000+0430"},
            {"value", new JObject { }}
        };

        public static JObject rule3 = new JObject
        {
            {"id", "rule-id-3"},
            {"name", "rule-name-3"},
            {"enabled", true},
            {"priority", 1000},
            {"condition", "true"},
            {"rolloutPercentage", 100},
            {"startDate", "2019-07-10T23:15:14.000-0700"},
            {"endDate", "2019-08-12T08:15:14.000+0430"},
            {"value", new JObject { }}
        };

        public static JObject rule4 = new JObject
        {
            {"id", "rule-id-4"},
            {"name", "rule-name-4"},
            {"enabled", true},
            {"priority", 1000},
            {"condition", "true"},
            {"rolloutPercentage", 100},
            {"startDate", "2019-07-10T23:15:14.000-0700"},
            {"endDate", "2019-08-12T08:15:14.000+0430"},
            {"value", new JObject { }}
        };

        public static JObject ruleNoValues1 = AddTypeAndSettingsToRule(JObject.FromObject(rule1), "segmentation", null);
        public static JObject ruleNoValues2 = AddTypeAndSettingsToRule(JObject.FromObject(rule2), "segmentation", null);
        public static JObject ruleValuesWithMetadata1 = AddTypeAndSettingsToRule(JObject.FromObject(rule3), "segmentation", rsListWithMetadata);
        public static JObject ruleValuesNoMetadata1 = AddTypeAndSettingsToRule(JObject.FromObject(rule4), "segmentation", rsListNoMetadata);

        public static Dictionary<string, JObject> rulesDict = new Dictionary<string, JObject>()
        {
            {
                ruleNoValues1["name"].Value<string>(), ruleNoValues1
            },
            {
                ruleNoValues2["name"].Value<string>(), ruleNoValues2
            },
            {
                ruleValuesNoMetadata1["name"].Value<string>(), ruleValuesNoMetadata1
            }
        };

        public static Dictionary<string, JObject> rulesDictWithSettings = new Dictionary<string, JObject>()
        {
            {
                ruleValuesNoMetadata1["name"].Value<string>(), ruleValuesNoMetadata1
            }
        };

        public static Dictionary<string, JObject> rulesDictWithNoSettings = new Dictionary<string, JObject>()
        {
            {
                ruleNoValues1["name"].Value<string>(), ruleNoValues1
            },
            {
                ruleNoValues2["name"].Value<string>(), ruleNoValues2
            }
        };

        public static Dictionary<string, JObject> rulesDictWithSettingsMetadata = new Dictionary<string, JObject>()
        {
            {
                ruleValuesWithMetadata1["name"].Value<string>(), ruleValuesWithMetadata1
            }
        };

        public static Dictionary<string, JObject> rulesDictWithAndWithoutSettingsMetadata = new Dictionary<string, JObject>()
        {
            {
                ruleNoValues1["name"].Value<string>(), ruleNoValues1
            },
            {
                ruleValuesWithMetadata1["name"].Value<string>(), ruleValuesWithMetadata1
            }
        };

        public static JObject AddTypeAndSettingsToRule(JObject jRule, string type, JArray values)
        {
            var newJRule = new JObject();
            newJRule["id"] = jRule["id"].Value<string>();
            newJRule["name"] = jRule["name"].Value<string>();
            newJRule["enabled"] = jRule["enabled"].Value<bool>();
            newJRule["priority"] = jRule["priority"].Value<int>();
            newJRule["condition"] = jRule["condition"].Value<string>();
            newJRule["rolloutPercentage"] = jRule["rolloutPercentage"].Value<int>();
            newJRule["startDate"] = jRule["startDate"].Value<string>();
            newJRule["endDate"] = jRule["endDate"].Value<string>();
            newJRule["type"] = type;
            newJRule["value"] = values;
            return newJRule;
        }

        public static JArray rulesList = new JArray(rulesDict.Values.ToList());
        public static JArray rulesWithSettingsList = new JArray(rulesDictWithSettings.Values.ToList());
        public static JArray rulesWithNoSettingsList = new JArray(rulesDictWithNoSettings.Values.ToList());
        public static JArray rulesWithSettingsMetadata = new JArray(rulesDictWithSettingsMetadata.Values.ToList());
        public static JArray rulesWithAndWithoutSettingsMetadata = new JArray(rulesDictWithAndWithoutSettingsMetadata.Values.ToList());

        public static List<string> addedRuleIDs = new List<string>()
        {
            "added-rule-id-1",
            "added-rule-id-2",
            "added-rule-id-3"
        };

        public static List<string> updatedRuleIDs = new List<string>()
        {
            "updated-rule-id-1",
            "updated-rule-id-2",
            "updated-rule-id-3"
        };

        public static List<string> deletedRuleIDs = new List<string>()
        {
            "deleted-rule-id-1",
            "deleted-rule-id-2",
            "deleted-rule-id-3"
        };

        // environments
        public static JObject environment1 = new JObject
        {
            {"id", "env-id-1"},
            {"project_id", "app-id-1"},
            {"name", "env-name-1"},
            {"description", "env-description-1"},
            {"created-at", "2019-07-10T23:15:14.000-0700"},
            {"updated-at", "2019-08-12T08:15:14.000+0430"},
            {"isDefault", true}
        };

        public static JObject environment2 = new JObject
        {
            {"id", "env-id-2"},
            {"project_id", "app-id-2"},
            {"name", "env-name-2"},
            {"description", "env-description-1"},
            {"created-at", "2019-07-10T23:15:14.000-0700"},
            {"updated-at", "2019-08-12T08:15:14.000+0430"},
            {"isDefault", false}
        };

        public static JArray environments = new JArray
        {
            environment1,
            environment2
        };

        public static RemoteConfigDataStore GetDataStore()
        {
            var pathToDataStore = typeof(RemoteConfigDataManager)
            .GetField("k_PathToDataStore", BindingFlags.Static | BindingFlags.NonPublic)
            .GetValue(null) as string;

            var dataStoreAssetFileName = typeof(RemoteConfigDataManager)
                .GetField("k_DataStoreAssetFileName", BindingFlags.Static | BindingFlags.NonPublic)
                .GetValue(null) as string;

            var dataStoreName = typeof(RemoteConfigDataManager)
                .GetField("k_DataStoreName", BindingFlags.Static | BindingFlags.NonPublic)
                .GetValue(null) as string;

            string formattedPath = Path.Combine(pathToDataStore, string.Format(dataStoreAssetFileName, dataStoreName));
            RemoteConfigDataStore asset = InitDataStore();
            CheckAndCreateAssetFolder(pathToDataStore);
            AssetDatabase.CreateAsset(asset, formattedPath);
            AssetDatabase.SaveAssets();

            return AssetDatabase.LoadAssetAtPath(formattedPath, typeof(RemoteConfigDataStore)) as RemoteConfigDataStore;
        }

        private static RemoteConfigDataStore InitDataStore()
        {
            RemoteConfigDataStore asset = ScriptableObject.CreateInstance<RemoteConfigDataStore>();
            asset.rsKeyList = new JArray();
            asset.rsLastCachedKeyList = new JArray();
            asset.currentEnvironment = "Release";
            asset.currentEnvironmentId = null;
            asset.environments = new JArray();
            asset.rulesList = new JArray();
            asset.lastCachedRulesList = new JArray();
            asset.addedRulesIDs = new List<string>();
            asset.updatedRulesIDs = new List<string>();
            asset.deletedRulesIDs = new List<string>();

            return asset;
        }

        private static void CheckAndCreateAssetFolder(string dataStorePath)
        {
            string[] folders = dataStorePath.Split('/');
            string assetPath = null;
            foreach (string folder in folders)
            {
                if (assetPath == null)
                {
                    assetPath = folder;
                }
                else
                {
                    string folderPath = Path.Combine(assetPath, folder);
                    if (!Directory.Exists(folderPath))
                    {
                        AssetDatabase.CreateFolder(assetPath, folder);
                    }
                    assetPath = folderPath;
                }
            }
        }
    }
}