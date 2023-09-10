using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json.Linq;

namespace Unity.RemoteConfig.Editor
{
    /// <summary>
    /// This class contains all methods needed to perform CRUD operations on the data objects.
    /// No other classes should ever interact with the data objects directly.
    /// </summary>
    internal class RemoteConfigDataManager
    {
        public event Action RulesDataStoreChanged;
        public event Action RemoteSettingDataStoreChanged;
        public event Action EnvironmentChanged;
        public event Action RulesDeleted;
        
        const string k_DataStoreAssetFileName = "{0}.asset";
        const string k_DataStoreName = "RemoteConfigDataStoreAsset";
        const string k_PathToDataStore = "Assets/Editor/RemoteConfig/Data";
        const string k_CurrentEnvironment = "UnityRemoteConfigEditorEnvironment";
        
        RemoteConfigDataStore m_DataStore;
        
        public enum m_DataStoreStatus {
            Init = 0,
            UnSynchronized = 1,
            Synchronized = 2,
            Pending = 3,
            Error = 4
        };

        public const int defaultRulePriority = 1000;
        private const int maxRulePriority = defaultRulePriority;
        private const int minRulePriority = 0;
        
        public string configId { get { return m_DataStore.configId; } set { m_DataStore.configId = value; } }
      
        public static readonly List<string> rsTypes = new List<string> { "string", "bool", "float", "int", "long" };
        public m_DataStoreStatus dataStoreStatus { get; set; }

        public int settingsCount
        {
            get
            {
                if (m_DataStore.rsKeyList == null)
                {
                    return 0;
                }
                else
                {
                    return m_DataStore.rsKeyList.Count;
                }
            }
        }

        /// <summary>
        /// Constructor: creates amd initalizes the Remote Config data store and restores the last selected environment.
        /// </summary>
        public RemoteConfigDataManager()
        {
            m_DataStore = CheckAndCreateDataStore();
            RestoreLastSelectedEnvironment(m_DataStore.currentEnvironment);
            dataStoreStatus = m_DataStoreStatus.Init;
        }

        public void SetDirty()
        {
            if(m_DataStore != null)
            {
                EditorUtility.SetDirty(m_DataStore);
            }
        }

        /// <summary>
        /// Returns the name of the last selected environment that is stored in EditorPrefs.
        /// </summary>
        /// <param name="defaultEnvironment"> The default environment name to be returned if last selected environment is not found</param>
        /// <returns> Name of last selected environment or defaultEnvironment if last selected is not found</returns>
        public string RestoreLastSelectedEnvironment(string defaultEnvironment)
        {
            return EditorPrefs.GetString(k_CurrentEnvironment + Application.cloudProjectId, defaultEnvironment);
        }
        
        /// <summary>
        /// Sets the name of the last selected environment and stores it in EditorPrefs.
        /// </summary>
        /// <param name="environmentName"> Name of environment to be stored</param>
        public void SetLastSelectedEnvironment (string environmentName)
        {
            EditorPrefs.SetString(k_CurrentEnvironment + Application.cloudProjectId, environmentName);
        }
        
        /// <summary>
        /// Checks for the existence of the Remote Config data store. Creates a new data store if one doesn't already exist
        /// and saves it to the AssetDatabase.
        /// </summary>
        /// <returns>Remote Config data store object</returns>
        public RemoteConfigDataStore CheckAndCreateDataStore()
        {
            string formattedPath = Path.Combine(k_PathToDataStore, string.Format(k_DataStoreAssetFileName, k_DataStoreName));
            if (AssetDatabase.FindAssets(k_DataStoreName).Length > 0)
            {
                if (AssetDatabase.LoadAssetAtPath(formattedPath, typeof(RemoteConfigDataStore)) == null)
                {
                    AssetDatabase.DeleteAsset(formattedPath);
                }
            }
            if (AssetDatabase.FindAssets(k_DataStoreName).Length == 0)
            {
                RemoteConfigDataStore asset = InitDataStore();
                CheckAndCreateAssetFolder(k_PathToDataStore);
                AssetDatabase.CreateAsset(asset, formattedPath);
                AssetDatabase.SaveAssets();
            }
            return AssetDatabase.LoadAssetAtPath(formattedPath, typeof(RemoteConfigDataStore)) as RemoteConfigDataStore;
        }

        private RemoteConfigDataStore InitDataStore()
        {
            RemoteConfigDataStore asset = ScriptableObject.CreateInstance<RemoteConfigDataStore>();
            asset.rsKeyList = new JArray();
            asset.currentEnvironment = "Please create an environment.";
            asset.currentEnvironmentId = null;
            asset.environments = new JArray();
            asset.rulesList = new JArray();
            asset.lastCachedRulesList = new JArray();
            asset.addedRulesIDs = new List<string>();
            asset.updatedRulesIDs = new List<string>();
            asset.deletedRulesIDs = new List<string>();

            return asset;
        }
        
        private void CheckAndCreateAssetFolder(string dataStorePath)
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

        /// <summary>
        /// Gets the Remote Settings list.
        /// </summary>
        /// <returns> List of JObjects</returns>
        public JArray GetRSList()
        {
            return m_DataStore.rsKeyList;
        }

        /// <summary>
        /// Gets the current environment name.
        /// </summary>
        /// <returns> Name of the current environment</returns>
        public string GetCurrentEnvironmentName()
        {
            return m_DataStore.currentEnvironment;
        }

        /// <summary>
        /// Gets the current environment ID.
        /// </summary>
        /// <returns> ID of the current environment</returns>
        public string GetCurrentEnvironmentId()
        {
            return m_DataStore.currentEnvironmentId;
        }

        /// <summary>
        /// Gets the current environment isDefault flag.
        /// </summary>
        /// <returns> isDefault of the current environment</returns>
        public bool GetCurrentEnvironmentIsDefault()
        {
            return m_DataStore.currentEnvironmentIsDefault;
        }

        /// <summary>
        /// Gets a list of all the environments for the current working project.
        /// </summary>
        /// <returns> List of Environment objects containing the name and ID</returns>
        public JArray GetEnvironments()
        {
            if(m_DataStore.environments == null)
            {
                m_DataStore.environments = new JArray();
            }
            return m_DataStore.environments;
        }

        /// <summary>
        /// Gets the Rules list.
        /// </summary>
        /// <returns> List of RuleWithSettingsMetadata objects</returns>
        public JArray GetRulesList()
        {
            if(m_DataStore.rulesList == null)
            {
                m_DataStore.rulesList = new JArray();
            }
            return m_DataStore.rulesList;
        }

        /// <summary>
        /// Copies last list of rules from the Remote Config Data Store
        /// </summary>
        public void SetLastCachedRulesList()
        {
            //TODO: Add variant rules list
            m_DataStore.lastCachedRulesList = new JArray(m_DataStore.rulesList);
        }

        /// <summary>
        /// Gets last list of rules from the Remote Config Data Store
        /// </summary>
        public JArray GetLastCachedRulesList()
        {
            return m_DataStore.lastCachedRulesList;
        }

        /// <summary>
        /// Gets the list of added Rule ID's.
        /// </summary>
        /// <returns> List of Rule ID's for new rules that were added since the last push</returns>
        public List<string> GetAddedRulesIDs()
        {
            return m_DataStore.addedRulesIDs;
        }
        
        /// <summary>
        /// Gets the list of updated Rule ID's.
        /// </summary>
        /// <returns> List of Rule ID's for rules that were updated since the last push</returns>
        public List<string> GetUpdatedRulesIDs()
        {
            return m_DataStore.updatedRulesIDs;
        }
        
        /// <summary>
        /// Gets the list of deleted Rule ID's.
        /// </summary>
        /// <returns> List of Rule ID's for rules that were deleted since the last push</returns>
        public List<string> GetDeletedRulesIDs()
        {
            return m_DataStore.deletedRulesIDs;
        }

        /// <summary>
        /// Gets the RuleWithSettingsMetadata at the given index in the rulesList.
        /// </summary>
        /// <param name="selectedRuleIndex">The index of the RuleWithSettingsMetadata we are getting from the rulesList</param>
        /// <returns>The RuleWithSettingsMetadata from the rulesList at the given index</returns>
        public JObject GetRuleAtIndex(int selectedRuleIndex)
        {
            return (JObject)m_DataStore.rulesList[selectedRuleIndex];
        }

        /// <summary>
        /// Gets the RuleWithSettingsMetadata for the given Rule ID.
        /// </summary>
        /// <param name="ruleId">The ID of the RuleWithSettingsMetadata that that we want to get</param>
        /// <returns>The RuleWithSettingsMetadata from the rulesList for the given index</returns>
        public JObject GetRuleByID(string ruleId)
        {
            //return m_DataStore.rulesList.Find(rule => rule.id == ruleId);
            for(int i = 0; i < m_DataStore.rulesList.Count; i++)
            {
                if(m_DataStore.rulesList[i]["id"].Value<string>() == ruleId)
                {
                    return (JObject)m_DataStore.rulesList[i];
                }
            }
            return new JObject();
        }

        /// <summary>
        /// Sets the the current environment ID name.
        /// </summary>
        /// <param name="currentEnvironment">Current Environment object containing the current environment name and ID</param>
        public void SetCurrentEnvironment(JObject currentEnvironment)
        {
            m_DataStore.currentEnvironment = currentEnvironment["name"].Value<string>();
            m_DataStore.currentEnvironmentId = currentEnvironment["id"].Value<string>();
            m_DataStore.currentEnvironmentIsDefault = false;
            if(currentEnvironment["isDefault"] != null)
            {
                m_DataStore.currentEnvironmentIsDefault = currentEnvironment["isDefault"].Value<bool>();
            }
            EnvironmentChanged?.Invoke();
        }

        /// <summary>
        /// Sets the default environment.
        /// </summary>
        /// <param name="defaultEnvironmentId">default Environment ID</param>
        public void SetDefaultEnvironment(string defaultEnvironmentId)
        {
            for (var i=0; i < m_DataStore.environments.Count; i++)
            {
                ((JObject)m_DataStore.environments[i])["isDefault"] = m_DataStore.environments[i]["id"].Value<string>() == defaultEnvironmentId;
            }

            // if current environment became default, update the isDefault flag
            if (GetCurrentEnvironmentId() == defaultEnvironmentId)
            {
                m_DataStore.currentEnvironmentIsDefault = true;
            }

            CheckEnvironmentsValid();
        }

        /// <summary>
        /// Checks if set of environments is valid. There must be exactly one default environment.
        /// </summary>
        public void CheckEnvironmentsValid()
        {
            if (m_DataStore.environments.Count < 1)
            {
                Debug.Log("Please create at least one environment");
            }

            var defaultEnvironmentsCount = m_DataStore.environments.Count((e) => {
                if (((JObject)e)["isDefault"] != null)
                {
                    return e["isDefault"].Value<bool>();
                }
                return false;
            });
            if (defaultEnvironmentsCount < 1)
            {
                Debug.Log("Please set environment as default");
            }
            if (defaultEnvironmentsCount > 1)
            {
                Debug.LogWarning("Something went wrong. More than one default environment");
            }

            var environmentsWithNoName = m_DataStore.environments.Where(e => e["name"].Value<string>() == "");
            for (var i = 0; i < environmentsWithNoName.Count(); i++)
            {
                Debug.LogWarning($"Environment with id: {m_DataStore.environments.ElementAt(i)["id"].Value<string>()} has an empty name");
            }
        }

        /// <summary>
        /// Sets the list of Environment objects containing the name and ID.
        /// </summary>
        /// <param name="environments">List of Environment objects containing the name and ID</param>
        public void SetEnvironmentsList(JArray environments)
        {
            m_DataStore.environments = environments;
        }

        /// <summary>
        /// Sets the config object on the Remote Config Data Store
        /// </summary>
        /// <param name="config">A config object representing the new config</param>
        public void SetRSDataStore(JObject config)
        {
            m_DataStore.rsKeyList = new JArray();
            if (config.HasValues)
            {
                foreach(var val in config["value"])
                {
                    var newSetting = new JObject();
                    newSetting["metadata"] = new JObject();
                    newSetting["metadata"]["entityId"] = Guid.NewGuid().ToString();
                    newSetting["rs"] = val;
                    m_DataStore.rsKeyList.Add(newSetting);
                }
                m_DataStore.configId = config["id"].Value<string>();
            }
            else
            {
                m_DataStore.configId = null;
            }
        }

        /// <summary>
        /// Copies last key list of settings from the Remote Config Data Store
        /// </summary>
        public void SetLastCachedKeyList()
        {
            m_DataStore.rsLastCachedKeyList = new JArray(m_DataStore.rsKeyList);
        }

        /// <summary>
        /// Gets last key list of settings from the Remote Config Data Store
        /// </summary>
        public JArray GetLastCachedKeyList()
        {
            return m_DataStore.rsLastCachedKeyList;
        }

        bool IsSegmentationRule(JObject rule)
        {
            return rule["type"].Value<string>() == "segmentation";
        }

        bool IsVariantRule(JObject rule)
        {
            return rule["type"].Value<string>() == "variant";
        }

        public void UpdateRule(JObject oldRule, JObject newRule)
        {
            if (IsVariantRule(oldRule))
            {
                for(int i = 0; i < m_DataStore.rulesList.Count; i++)
                {
                    if(m_DataStore.rulesList[i]["id"].Value<string>() == oldRule["id"].Value<string>())
                    {
                        m_DataStore.rulesList[i] = newRule;
                    }
                }
            }
            AddRuleToUpdatedRuleIDs(newRule["id"].Value<string>());
            RulesDataStoreChanged?.Invoke();
        }

        /// <summary>
        /// Sets the Rules data store using a list of Rules.
        /// </summary>
        /// <param name="newRulesDataStore">A list of Rule objects</param>
        public void SetRulesDataStore(JArray newRulesDataStore)
        {
            m_DataStore.rulesList = new JArray();
            var defaultSettings = m_DataStore.rsKeyList;
            foreach(var rule in newRulesDataStore)
            {
                if(rule["type"].Value<string>() == "segmentation")
                {
                    var settingsInRule = new JArray();
                    foreach (var setting in rule["value"])
                    {
                        var newSetting = new JObject();

                        // if rule is already formatted, with ["rs"] key present in the setting, leave setting as is:
                        if (setting["rs"] != null)
                        {
                            newSetting = (JObject)setting;
                        }
                        else
                        {
                            string entityId = null;
                            var defaultSettingIndex = -1;
                            for (int i = 0; i < defaultSettings.Count; i++)
                            {
                                if (defaultSettings[i]["rs"]["key"].Value<string>() == setting["key"].Value<string>() && defaultSettings[i]["rs"]["type"].Value<string>() == setting["type"].Value<string>())
                                {
                                    defaultSettingIndex = i;
                                }
                            }
                            if (defaultSettingIndex == -1)
                            {
                                entityId = Guid.NewGuid().ToString();
                            }
                            else
                            {
                                entityId = defaultSettings[defaultSettingIndex]["metadata"]["entityId"].Value<string>();
                            }

                            newSetting["metadata"] = new JObject();
                            newSetting["metadata"]["entityId"] = entityId;
                            newSetting["rs"] = setting;
                        }

                        settingsInRule.Add(newSetting);
                    }
                    var newRule = rule;
                    newRule["value"] = settingsInRule;
                    m_DataStore.rulesList.Add(newRule);
                }
                else if(rule["type"].Value<string>() == "variant")
                {
                    foreach (var variant in rule["value"])
                    {
                        var variantsJArray = (JArray)variant["values"];
                        var settingsInVariant = new JArray();
                        for(int i = 0; i < variantsJArray.Count; i++)
                        {
                            var newSetting = new JObject();

                            // if rule is already formatted, with ["rs"] key present in the setting, leave setting as is:
                            if (variantsJArray[i]["rs"] != null)
                            {
                                newSetting = (JObject)variantsJArray[i];
                            }
                            else
                            {
                                string entityId = null;
                                var defaultSettingIndex = -1;
                                for (int j = 0; j < defaultSettings.Count; j++)
                                {
                                    if (defaultSettings[j]["rs"]["key"].Value<string>() == variantsJArray[i]["key"].Value<string>() && defaultSettings[j]["rs"]["type"].Value<string>() == variantsJArray[i]["type"].Value<string>())
                                    {
                                        defaultSettingIndex = j;
                                    }
                                }
                                if (defaultSettingIndex == -1)
                                {
                                    entityId = Guid.NewGuid().ToString();
                                }
                                else
                                {
                                    entityId = defaultSettings[defaultSettingIndex]["metadata"]["entityId"].Value<string>();
                                }

                                newSetting["metadata"] = new JObject();
                                newSetting["metadata"]["entityId"] = entityId;
                                newSetting["rs"] = variantsJArray[i];
                            }

                            settingsInVariant.Add(newSetting);
                        }
                        variant["values"] = settingsInVariant;
                    }
                    m_DataStore.rulesList.Add(rule);
                }
            }
            RulesDataStoreChanged?.Invoke();
            RemoteSettingDataStoreChanged?.Invoke();
        }

        /// <summary>
        /// Adds a rule to the Rules data store. This will add it to the rulesList.
        /// </summary>
        /// <param name="newRule">The RuleWithSettingsMetadata to be added</param>
        public void AddRule(JObject newRule)
        {
            m_DataStore.rulesList.Add(newRule);
            RulesDataStoreChanged?.Invoke();
            RemoteSettingDataStoreChanged?.Invoke();
            AddRuleToAddedRuleIDs(newRule);
        }

        private void AddRuleToAddedRuleIDs(JObject newRule)
        {
            m_DataStore.addedRulesIDs.Add(newRule["id"].Value<string>());
        }

        /// <summary>
        /// Deletes a rule from the Rules data store. This will delete it from the rulesList.
        /// </summary>
        /// <param name="deletedRuleID">ID of the Rule to be deleted</param>
        public void DeleteRule(string deletedRuleID)
        {
            AddRuleToDeletedRuleIDs(deletedRuleID);
            for(int i = 0; i < m_DataStore.rulesList.Count; i++)
            {
                if(m_DataStore.rulesList[i]["id"].Value<string>() == deletedRuleID)
                {
                    m_DataStore.rulesList.RemoveAt(i);
                    break;
                }
            }
            //m_DataStore.rulesList.Remove(m_DataStore.rulesList.Find(rule => rule.id == deletedRuleID));
            RulesDataStoreChanged?.Invoke();
            RemoteSettingDataStoreChanged?.Invoke();
        }
        
        /// <summary>
        /// Deletes all the rules in the current environment's config.
        /// </summary>
        public void DeleteRulesForCurrentEnvironment()
        {
            if (m_DataStore.rulesList != null)
            {
                foreach (var rule in m_DataStore.rulesList.ToList())
                {
                    DeleteRule(rule["id"].Value<string>());
                }
            }
            RulesDeleted?.Invoke();
        }

        private void AddRuleToDeletedRuleIDs(string deletedRuleId)
        {
            bool ruleAdded = false;
            if (m_DataStore.addedRulesIDs.Contains(deletedRuleId))
            {
                m_DataStore.addedRulesIDs.Remove(deletedRuleId);
                ruleAdded = true;
            }

            if (m_DataStore.updatedRulesIDs.Contains(deletedRuleId))
            {
                m_DataStore.updatedRulesIDs.Remove(deletedRuleId);
            }

            if (!ruleAdded)
            {
                m_DataStore.deletedRulesIDs.Add(deletedRuleId);
            }
        }

        /// <summary>
        /// Checks to see if the given Rule's attributes are within the accepted range.
        /// </summary>
        /// <param name="rule">RuleWithSettingsMetadata object to be validated</param>
        /// <returns>true if the rule is valid and false if the rule is not valid</returns>
        public bool ValidateRule(JObject rule)
        {
            if (ValidateRulePriority(rule) && ValidateRuleName(rule))
            {
                dataStoreStatus = m_DataStoreStatus.UnSynchronized;
                return true;
            }
            else
            {
                dataStoreStatus = m_DataStoreStatus.Error;
                return false;
            }
        }

        /// <summary>
        /// Checks to see if the given rule's name is valid.
        /// </summary>
        /// <param name="rule">RuleWithSettingsMetadata object to be validated</param>
        /// <returns>true if the rule's name is valid, false if it is not valid</returns>
        public bool ValidateRuleName(JObject rule)
        {
            for(int i = 0; i < m_DataStore.rulesList.Count; i++)
            {
                if (m_DataStore.rulesList[i]["id"].Value<string>() != rule["id"].Value<string>() && m_DataStore.rulesList[i]["name"].Value<string>() == rule["name"].Value<string>())
                {
                    Debug.LogWarning(m_DataStore.rulesList[i]["name"].Value<string>() + " already exists. Rule names must be unique.");
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Checks to see if the given rule's priority is valid.
        /// </summary>
        /// <param name="rule">RuleWithSettingsMetadata object to be validated</param>
        /// <returns>true if the rule's priority is valid, false if it is not valid</returns>
        public bool ValidateRulePriority(JObject rule)
        {	        
            if (rule["priority"].Value<int>() < 0 || rule["priority"].Value<int>() > 1000)
            {
                Debug.LogWarning("Rule: " + rule["name"].Value<string>() + " has an invalid priority. The set priority is " + rule["priority"].Value<int>() + ". The values for priority must be between " + minRulePriority + " and " + maxRulePriority);
                return false;
            }
            else
            {
                return true;	            
            }
        }

        /// <summary>
        /// Updates the attributes for a given rule. This will update the rule in the rulesList.
        /// </summary>
        /// <param name="ruleId">ID of the rule being updated</param>
        /// <param name="newRule">RuleWithSettingsMetadata object containing the new attributes</param>
        public void UpdateRuleAttributes(string ruleId, JObject newRule)
        {
            if (ValidateRule(newRule))
            {
                for(int i = 0; i < m_DataStore.rulesList.Count; i++)
                {
                    if(m_DataStore.rulesList[i]["id"].Value<string>() == ruleId)
                    {
                        m_DataStore.rulesList[i] = newRule;
                        break;
                    }
                }

                RulesDataStoreChanged?.Invoke();
                AddRuleToUpdatedRuleIDs(ruleId);
            }
        }

        /// <summary>
        /// Updates the type of a given rule.
        /// </summary>
        /// <param name="ruleId">ID of the rule being updated</param>
        /// <param name="newType">New type for the rule</param>
        public void UpdateRuleType(string ruleId, string newType)
        {
            // see which type is being changed to.
            // if going from segmentation to variant, need to change datastructure
            // if going from variant to segmentation, need to blow away variant structure and move to old structure

            // find the rule first

            for(int i = 0; i < m_DataStore.rulesList.Count; i++)
            {
                if(m_DataStore.rulesList[i]["id"].Value<string>() == ruleId)
                {
                    var ruleObj = m_DataStore.rulesList[i];
                    var oldType = ruleObj["type"].Value<string>();

                    if(oldType == "segmentation" && newType == "variant")
                    {
                        var oldValue = (JArray)ruleObj["value"];
                        oldValue.Parent.Remove();
                        var newVariant = new JObject();
                        newVariant["id"] = "variant-1";
                        newVariant["type"] = newType;
                        newVariant["weight"] = null;
                        newVariant["values"] = oldValue;
                        var ruleValueJArray = new JArray();
                        ruleValueJArray.Add(newVariant);
                        ruleObj["value"] = ruleValueJArray;
                        ruleObj["type"] = newType;
                    }
                    else if(oldType == "variant" && newType == "segmentation")
                    {
                        if (!EditorUtility.DisplayDialog("Switching rule type from 'variant' to 'segmentation' will remove your variants",
                            "Do you want to proceed?", "Yes", "No"))
                        {
                            return;
                        }
                        var oldValue = (JArray)ruleObj["value"];
                        oldValue.Parent.Remove();
                        var newValues = (JArray)oldValue[0]["values"];
                        ruleObj["type"] = newType;
                        ruleObj["value"] = newValues;
                    }

                    RulesDataStoreChanged?.Invoke();
                    AddRuleToUpdatedRuleIDs(ruleId);
                }
            }
        }

        /// <summary>
        /// Enables or disables the given rule.
        /// </summary>
        /// <param name="ruleId">ID of Rule to be enabled or disabled</param>
        /// <param name="enabled">true = enabled, false = disabled</param>
        public void EnableOrDisableRule(string ruleId, bool enabled)
        {
            for(int i = 0; i < m_DataStore.rulesList.Count; i++)
            {
                if(m_DataStore.rulesList[i]["id"].Value<string>() == ruleId)
                {
                    m_DataStore.rulesList[i]["enabled"] = enabled;
                    break;
                }
            }
            AddRuleToUpdatedRuleIDs(ruleId);
            RulesDataStoreChanged?.Invoke();
        }

        /// <summary>
        /// Adds the given setting to the given rule.
        /// </summary>
        /// <param name="selectedRuleId">ID of the rule that the setting should be added to</param>
        /// <param name="entityId">EntityId of the setting to be added to the given rule</param>
        public void AddSettingToRule(string selectedRuleId, string entityId)
        {
            if(IsSettingInRule(selectedRuleId, entityId))
            {
                return;
            }

            for (int i = 0; i < m_DataStore.rulesList.Count; i++)
            {
                if (m_DataStore.rulesList[i]["id"].Value<string>() == selectedRuleId)
                {
                    var currentRuleSettings = (JArray)m_DataStore.rulesList[i]["value"];
                    for (int j = 0; j < m_DataStore.rsKeyList.Count; j++)
                    {
                        if (m_DataStore.rsKeyList[j]["metadata"]["entityId"].Value<string>() == entityId)
                        {
                            currentRuleSettings.Add(m_DataStore.rsKeyList[j]);
                        }
                    }
                }
            }
            RemoteSettingDataStoreChanged?.Invoke();
            AddRuleToUpdatedRuleIDs(selectedRuleId);
        }

        /// <summary>
        /// Deletes the given setting to the given Rule.
        /// </summary>
        /// <param name="ruleId">ID of the rule that the setting should be deleted from</param>
        /// <param name="entityId">EntityId of the setting to be deleted from the given rule</param>
        public void DeleteSettingFromRule(string ruleId, string entityId)
        {
            if(!IsSettingInRule(ruleId, entityId))
            {
                return;
            }

            for (int i = 0; i < m_DataStore.rulesList.Count; i++)
            {
                if (m_DataStore.rulesList[i]["id"].Value<string>() == ruleId)
                {
                    var currentRuleSettings = (JArray)m_DataStore.rulesList[i]["value"];
                    for (int j = 0; j < currentRuleSettings.Count; j++)
                    {
                        if (currentRuleSettings[j]["metadata"]["entityId"].Value<string>() == entityId)
                        {
                            currentRuleSettings.Remove(currentRuleSettings[j]);
                        }
                    }
                }
            }
            RemoteSettingDataStoreChanged?.Invoke();
            AddRuleToUpdatedRuleIDs(ruleId);
        }

        /// <summary>
        /// Updates the value of the given setting for the given rule.
        /// </summary>
        /// <param name="ruleId">ID of the rule that the updated setting belong to</param>
        /// <param name="updatedSetting">A RsKvtData containing the updated value</param>
        public void UpdateSettingForRule(string ruleId, JObject updatedSetting)
        {
            for(int i = 0; i < m_DataStore.rulesList.Count; i++)
            {
                if(m_DataStore.rulesList[i]["id"].Value<string>() == ruleId)
                {
                    var tempArr = (JArray)m_DataStore.rulesList[i]["value"];
                    for(int j = 0; j < tempArr.Count; j++)
                    {
                        if(tempArr[j]["metadata"]["entityId"].Value<string>() == updatedSetting["metadata"]["entityId"].Value<string>())
                        {
                            tempArr[j] = updatedSetting;
                        }
                    }
                }
            }
            RemoteSettingDataStoreChanged?.Invoke();
            AddRuleToUpdatedRuleIDs(ruleId);
        }

        private void AddRuleToUpdatedRuleIDs(string updatedRule)
        {
            //this is a new rule, do nothing - the changes will get picked up the add rule request
            if (!m_DataStore.addedRulesIDs.Contains(updatedRule) && !m_DataStore.updatedRulesIDs.Contains(updatedRule))
            {
                m_DataStore.updatedRulesIDs.Add(updatedRule);
            }
        }

        /// <summary>
        /// Removes the given rule ID from the list of added rules ID's.
        /// </summary>
        /// <param name="ruleId">ID of the rule to be removed from the list of added rule ID's</param>
        public void RemoveRuleFromAddedRuleIDs(string ruleId)
        {
            m_DataStore.addedRulesIDs.Remove(ruleId);
        }
        
        /// <summary>
        /// Removes the given rule ID from the list of updated rule ID's.
        /// </summary>
        /// <param name="ruleId">ID of the rule to be removed from the list of updated rule ID's</param>
        public void RemoveRuleFromUpdatedRuleIDs(string ruleId)
        {
            m_DataStore.updatedRulesIDs.Remove(ruleId);
        }
        
        /// <summary>
        /// Removes the given rule ID from the list of deleted rule ID's.
        /// </summary>
        /// <param name="ruleId">ID of the rule to be remove from the list of deleted rule ID's</param>
        public void RemoveRuleFromDeletedRuleIDs(string ruleId)
        {
            m_DataStore.deletedRulesIDs.Remove(ruleId);
        }
        
        /// <summary>
        /// Clears the list of added rule ID's, list of updated rule ID's, and the list of deleted rule ID's.
        /// </summary>
        public void ClearRuleIDs()
        {
            m_DataStore.addedRulesIDs.Clear();
            m_DataStore.updatedRulesIDs.Clear();
            m_DataStore.deletedRulesIDs.Clear();
        }

        /// <summary>
        /// Adds a setting to the Remote Settings data store. This will add the setting to the rsKeyList.
        /// </summary>
        /// <param name="newSetting">The setting to be added</param>
        public void AddSetting(JObject newSetting)
        {
            m_DataStore.rsKeyList.Add(newSetting);
            RemoteSettingDataStoreChanged?.Invoke();
        }

        /// <summary>
        /// Deletes a setting from the Remote Settings data store. This will delete the setting from the rsKeyList.
        /// </summary>
        /// <param name="entityId">The EntityId of the setting to be deleted</param>
        public void DeleteSetting(string entityId)
        {
            for(int i = 0; i < m_DataStore.rsKeyList.Count; i++)
            {
                if (m_DataStore.rsKeyList[i]["metadata"]["entityId"].Value<string>() == entityId)
                {
                    m_DataStore.rsKeyList.RemoveAt(i);
                    break;
                }
            }
            //m_DataStore.rsKeyList.Remove(m_DataStore.rsKeyList.Find(s => s.metadata.entityId == entityId));
            RemoteSettingDataStoreChanged?.Invoke();
        }

        /// <summary>
        /// Updates a setting in the Remote Settings data store. This will update the setting in the rsKeyList.
        /// </summary>
        /// <param name="oldSetting">The RsKvtData of the setting to be updated</param>
        /// <param name="newSetting">The new setting with the updated fields</param>
        public void UpdateSetting(JObject oldSetting, JObject newSetting)
        {
            var isError = false;
            if (newSetting["rs"]["key"].Value<string>().Length >= 255)
            {
                Debug.LogWarning(newSetting["rs"]["key"].Value<string>() + " is at the maximum length of 255 characters.");
                isError = true;
            }
            if (!isError)
            {
                for (int i = 0; i < m_DataStore.rsKeyList.Count; i++)
                {
                    if (m_DataStore.rsKeyList[i]["metadata"]["entityId"].Value<string>() == oldSetting["metadata"]["entityId"].Value<string>())
                    {
                        m_DataStore.rsKeyList[i] = newSetting;
                    }
                }
                RemoteSettingDataStoreChanged?.Invoke();
            }
        }

        /// <summary>
        /// Checks to see if any rules exist
        /// </summary>
        /// <returns>true if there is at leave one rule and false if there are no rules</returns>
        public bool HasRules()
        {
            if(m_DataStore.rulesList == null)
            {
                return false;
            }
            return m_DataStore.rulesList.Count > 0;
        }

        /// <summary>
        /// Checks if the given setting is being used by the given rule
        /// </summary>
        /// <param name="ruleId">ID of the rule that needs to be checked</param>
        /// <param name="rsEntityId">EntityId of the setting that needs to be checked</param>
        /// <returns>true if the given setting is being used by the given rule</returns>
        public bool IsSettingInRule(string ruleId, string rsEntityId)
        {
            for(int i = 0; i < m_DataStore.rulesList.Count; i++)
            {
                if(m_DataStore.rulesList[i]["id"].Value<string>() == ruleId)
                {
                    var settings = (JArray)m_DataStore.rulesList[i]["value"];
                    for(int j = 0; j < settings.Count; j++)
                    {
                        if(settings[j]["metadata"]["entityId"].Value<string>() == rsEntityId)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Returns list of settings for particular rule
        /// </summary>
        /// <param name="ruleId">ID of the rule </param>
        /// <returns>list of settings used by the given rule</returns>
        public JArray GetSettingsListForRule(string ruleId)
        {
            var settingsInRule = new JArray();
            for (int i = 0; i < m_DataStore.rulesList.Count; i++)
            {
                if (m_DataStore.rulesList[i]["id"].Value<string>() == ruleId)
                {
                    settingsInRule = (JArray)m_DataStore.rulesList[i]["value"];
                }
            }
            return settingsInRule;
        }

        /// <summary>
        /// compares two lists of RsKvtData
        /// </summary>
        /// <param name="objectListNew">first list to compare </param>
        /// <param name="objectListOld">second list to compare </param>
        /// <returns>true if lists are equal</returns>
        public bool CompareKeyValueEquality(JArray objectListNew, JArray objectListOld)
        {
            if (objectListOld.Count != objectListNew.Count)
            {
                return false;
            }

            for (var i = 0; i < objectListNew.Count; i++)
            {
                if (!JToken.DeepEquals(objectListNew[i], objectListOld[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// compares two lists of RuleWithSettingsMetadata type
        /// </summary>
        /// <param name="ruleListNew">first list to compare </param>
        /// <param name="ruleListOld">second list to compare </param>
        /// <returns>true if lists are equal</returns>
        public bool CompareRulesEquality(JArray ruleListNew, JArray ruleListOld)
        {
            if (ruleListNew.Count != ruleListOld.Count)
            {
                return false;
            }

            for (var i = 0; i < ruleListNew.Count; i++)
            {
                if (!JToken.DeepEquals(ruleListNew[i], ruleListOld[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public void UpdateRuleId(string oldRuleId, string newRuleId)
        {
            var rule = GetRuleByID(oldRuleId);
            rule["id"] = newRuleId;
            RulesDataStoreChanged?.Invoke();
        }

    }
}
