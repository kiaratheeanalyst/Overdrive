using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;

namespace Unity.RemoteConfig.Editor
{
    internal class RemoteConfigWindowController
    {
        public event Action rulesDataStoreChanged;
        public event Action remoteSettingsStoreChanged;
        public event Action environmentChanged;

        RemoteConfigDataManager m_DataManager;

        bool m_WebRequestReturnedEventSubscribed = false;
        bool m_PostAddRuleEventSubscribed = false;
        bool m_PostSettingsEventSubscribed = false;
        bool m_PutConfigsEventSubscribed = false;
        bool m_PostConfigEventSubscribed;
        string m_CurrentEnvironment;
        bool m_IsLoading = false;
        public string environmentId { get { return m_DataManager.GetCurrentEnvironmentId(); } }
        public string environmentName { get { return m_DataManager.GetCurrentEnvironmentName(); } }
        public bool environmentIsDefault { get { return m_DataManager.GetCurrentEnvironmentIsDefault(); } }

        // DialogBox variables
        public readonly string k_RCWindowName = "Remote Config";
        public readonly string k_RCDialogUnsavedChangesTitle = "Unsaved Changes";
        public readonly string k_RCDialogUnsavedChangesMessage = "You have unsaved changes. \n \n" +
                                                                 "If you want them saved, click 'Cancel' then 'Push'.\n" +
                                                                 "Otherwise, click 'OK' to discard the changes \n";
        public readonly string k_RCDialogUnsavedChangesOK = "OK";
        public readonly string k_RCDialogUnsavedChangesCancel = "Cancel";

        public bool isLoading
        {
            get { return m_IsLoading; }
            set { m_IsLoading = value; }
        }

        public RemoteConfigWindowController(bool shouldFetchOnInit = true, bool windowOpenOnInit = false)
        {
            m_DataManager = new RemoteConfigDataManager();
            m_DataManager.RulesDataStoreChanged += OnRulesDataStoreChanged;
            m_DataManager.RemoteSettingDataStoreChanged += OnRemoteSettingDataStoreChanged;
            m_DataManager.EnvironmentChanged += OnEnvironmentChanged;
            RemoteConfigWebApiClient.rcRequestFailed += OnFailedRequest;
            RemoteConfigWebApiClient.fetchEnvironmentsFinished += FetchDefaultEnvironment;
            if (shouldFetchOnInit && windowOpenOnInit)
            {
                FetchEnvironments();
            }
        }

        private void OnFetchEnvironmentsFinished(JArray environments)
        {
            SetEnvironmentData(environments);
        }

        public void SetManagerDirty()
        {
            m_DataManager.SetDirty();
        }

        private bool SetEnvironmentData(JArray environments)
        {
            if (environments != null && environments.Count > 0)
            {
                m_DataManager.SetEnvironmentsList(environments);
                var currentEnvironment = LoadEnvironments(environments, m_DataManager.GetCurrentEnvironmentName());
                m_DataManager.SetCurrentEnvironment(currentEnvironment);
                m_DataManager.SetLastSelectedEnvironment(currentEnvironment["name"].Value<string>());
                return true;
            }
            else
            {
                m_DataManager.SetEnvironmentsList(environments);
            }

            return false;
        }

        public JObject LoadEnvironments(JArray environments, string currentEnvName)
        {
            if (environments.Count > 0)
            {
                var currentEnvironment = environments[0];
                foreach (var environment in environments)
                {
                    if (environment["name"].Value<string>() == currentEnvName)
                    {
                        currentEnvironment = environment;
                        break;
                    }
                }
                return (JObject)currentEnvironment;
            }
            else
            {
                Debug.LogWarning("No environments loaded. Please restart the Remote Config editor window");
                return new JObject();
            }
        }

        public JArray GetRulesList()
        {
            var rulesList = m_DataManager.GetRulesList();

            if (rulesList == null)
            {
                rulesList = new JArray();
            }

            return (JArray)rulesList.DeepClone();
        }

        public JArray GetSettingsList()
        {
            var settingsList = m_DataManager.GetRSList();

            if (settingsList == null)
            {
                settingsList = new JArray();
            }

            return (JArray)settingsList.DeepClone();
        }

        public JArray GetLastCachedKeyList()
        {
            var settingsList = m_DataManager.GetLastCachedKeyList();

            if (settingsList == null)
            {
                settingsList = new JArray();
            }

            return (JArray)settingsList.DeepClone();
        }

        public JArray GetLastCachedRulesList()
        {
            var rulesList = m_DataManager.GetLastCachedRulesList();

            if (rulesList == null)
            {
                rulesList = new JArray();
            }

            return (JArray)rulesList.DeepClone();;
        }

        public JArray GetSettingsListForRule(string ruleId)
        {
            var settingsListForRule = m_DataManager.GetSettingsListForRule(ruleId);

            if (settingsListForRule == null)
            {
                settingsListForRule = new JArray();
            }

            return (JArray)settingsListForRule.DeepClone();
        }

        public void AddDefaultRule()
        {
            var newRule = new JObject();
            newRule["projectId"] = Application.cloudProjectId;
            newRule["environmentId"] = m_DataManager.GetCurrentEnvironmentId();
            newRule["configId"] = m_DataManager.configId;
            newRule["id"] = Guid.NewGuid().ToString();
            newRule["name"] = GetNewRuleId();
            newRule["enabled"] = false;
            newRule["priority"] = RemoteConfigDataManager.defaultRulePriority;
            newRule["condition"] = "";
            newRule["rolloutPercentage"] = 100;
            newRule["startDate"] = "";
            newRule["endDate"] = "";
            newRule["type"] = "segmentation";
            newRule["value"] = new JArray();
            m_DataManager.AddRule(newRule);
        }

        public string GetNewRuleId()
        {
            var currentRuleIndex = 0;
            var currentRuleIndexes = new int[m_DataManager.GetRulesList().Count];
            for (int i = 0; i < m_DataManager.GetRulesList().Count; i++){
                if (Int32.TryParse(m_DataManager.GetRulesList()[i]["name"].Value<string>().Replace("New Rule",""), out currentRuleIndex)){
                    currentRuleIndexes[i] = currentRuleIndex;
                }
            }
            return (currentRuleIndexes.Length == 0) ? "New Rule 0" : string.Format("New Rule {0}", currentRuleIndexes.Max()+1);
        }

        public void DeleteRule(string ruleId)
        {
            m_DataManager.DeleteRule(ruleId);
        }

        public void EnableOrDisableRule(string ruleId, bool enabled)
        {
            m_DataManager.EnableOrDisableRule(ruleId, enabled);
        }

        public bool HasRules()
        {
            return m_DataManager.HasRules();
        }

        public void UpdateRuleType(string ruleId, string newType)
        {
            m_DataManager.UpdateRuleType(ruleId, newType);
        }

        public void UpdateRuleAttributes(string ruleId, JObject newRule)
        {
            m_DataManager.UpdateRuleAttributes(ruleId, newRule);
        }

        public JObject GetRuleAtIndex(int selectedRuleIndex)
        {
            return (JObject)m_DataManager.GetRuleAtIndex(selectedRuleIndex).DeepClone();
        }

        public JObject GetRuleById(string ruleId)
        {
            return (JObject)m_DataManager.GetRuleByID(ruleId).DeepClone();
        }

        public int GetEnvironmentsCount()
        {
            return m_DataManager.GetEnvironments().Count;
        }

        public string GetCurrentEnvironmentName()
        {
            return m_DataManager.GetCurrentEnvironmentName();
        }

        public GenericMenu BuildPopupListForEnvironments()
        {
            var menu = new GenericMenu();

            for (int i = 0; i < GetEnvironmentsCount(); i++)
            {
                string name = m_DataManager.GetEnvironments()[i]["name"].Value<string>();
                menu.AddItem(new GUIContent(name), name == m_DataManager.GetCurrentEnvironmentName(), EnvironmentSelectionCallback, name);
            }

            return menu;
        }

        private void EnvironmentSelectionCallback(object obj)
        {
            var environmentName = (string)obj;
            var defaultEnvironmentIndex = -1;
            for (int i = 0; i < m_DataManager.GetEnvironments().Count; i++)
            {
                if (((JObject)m_DataManager.GetEnvironments()[i])["name"].Value<string>() == environmentName)
                {
                    defaultEnvironmentIndex = i;
                }
            }
            var env = (JObject)m_DataManager.GetEnvironments()[defaultEnvironmentIndex];

            //TODO: Move this logic checking if changes to env and fetch settings if current settings are not modified
            if (CompareKeyValueEquality(GetSettingsList(), GetLastCachedKeyList()) &&
                CompareRulesEquality(GetRulesList(), GetLastCachedRulesList()))
            {
                m_DataManager.SetCurrentEnvironment(env);
                FetchSettings(m_DataManager.GetEnvironments());
            }
            else
            {
                if (EditorUtility.DisplayDialog(k_RCDialogUnsavedChangesTitle, k_RCDialogUnsavedChangesMessage, k_RCDialogUnsavedChangesOK, k_RCDialogUnsavedChangesCancel))
                {
                    m_DataManager.SetCurrentEnvironment(env);
                    FetchSettings(m_DataManager.GetEnvironments());
                }
            }
        }

        public void AddSettingToRule(string selectedRuleId, string entityId)
        {
            m_DataManager.AddSettingToRule(selectedRuleId, entityId);
        }

        public void Fetch()
        {
            m_IsLoading = true;
            FetchEnvironments();
        }

        private void FetchEnvironments()
        {
            RemoteConfigWebApiClient.fetchEnvironmentsFinished += FetchSettings;
            try
            {
                RemoteConfigWebApiClient.FetchEnvironments(Application.cloudProjectId);
            }
            catch
            {
                RemoteConfigWebApiClient.fetchEnvironmentsFinished -= FetchSettings;
                DoCleanUp();
            }
        }
        private void FetchDefaultEnvironment(JArray environments)
        {
            if(environments.Count > 0)
            {
                RemoteConfigWebApiClient.fetchDefaultEnvironmentFinished += OnFetchDefaultEnvironmentFinished;
                try
                {
                    RemoteConfigWebApiClient.FetchDefaultEnvironment(Application.cloudProjectId);
                }
                catch
                {
                    DoCleanUp();
                }
            }
            else
            {
                DoCleanUp();
            }
        }

        private void FetchSettings(JArray environments)
        {
            RemoteConfigWebApiClient.fetchEnvironmentsFinished -= FetchSettings;
            if (SetEnvironmentData(environments))
            {
                RemoteConfigWebApiClient.fetchConfigsFinished += OnFetchRemoteSettingsFinished;
                try
                {
                    RemoteConfigWebApiClient.FetchConfigs(Application.cloudProjectId, m_DataManager.GetCurrentEnvironmentId());
                }
                catch
                {
                    RemoteConfigWebApiClient.fetchConfigsFinished -= OnFetchRemoteSettingsFinished;
                    DoCleanUp();
                }
            }
            else
            {
                DoCleanUp();
            }
        }

        private void FetchRules()
        {
            if (!string.IsNullOrEmpty(m_DataManager.configId))
            {
                RemoteConfigWebApiClient.fetchRulesFinished += OnFetchRulesFinished;
                try
                {
                    RemoteConfigWebApiClient.FetchRules(Application.cloudProjectId, m_DataManager.configId);
                }
                catch
                {
                    RemoteConfigWebApiClient.fetchRulesFinished -= OnFetchRulesFinished;
                    DoCleanUp();
                }
            }
            else
            {
                OnFetchRulesFinished(new JArray());
            }
        }

        public void Push()
        {

            if (m_DataManager.dataStoreStatus == RemoteConfigDataManager.m_DataStoreStatus.Error)
            {
                Debug.LogError("There are errors in the Local Data Rules and or Settings please resolve them before pushing changes");
            }
            else
            {
                string environmentId = m_DataManager.GetCurrentEnvironmentId();
                if (string.IsNullOrEmpty(m_DataManager.configId))
                {
                    RemoteConfigWebApiClient.postConfigRequestFinished += OnConfigPostFinishedPushHandler;
                    m_PostConfigEventSubscribed = true;
                    PushSettings(environmentId);
                }
                else
                {
                    PushSettings(environmentId);
                    PushAddedRules(environmentId);
                    PushUpdatedRules(environmentId);
                    PushDeletedRules(environmentId);
                }
            }
        }

        private void OnConfigPostFinishedPushHandler(string configId)
        {
            string environmentId = m_DataManager.GetCurrentEnvironmentId();
            m_DataManager.configId = configId;
            PushAddedRules(environmentId);
            PushUpdatedRules(environmentId);
            PushDeletedRules(environmentId);
            if (m_PostConfigEventSubscribed)
            {
                RemoteConfigWebApiClient.postConfigRequestFinished -= OnConfigPostFinishedPushHandler;
                m_PostConfigEventSubscribed = false;
            }
        }

        public void AddSetting()
        {
            var jSetting = new JObject();
            jSetting["metadata"] = new JObject();
            jSetting["metadata"]["entityId"] = Guid.NewGuid().ToString();
            jSetting["rs"] = new JObject();
            jSetting["rs"]["key"] = "Setting" + m_DataManager.settingsCount;
            jSetting["rs"]["value"] = "";
            jSetting["rs"]["type"] = "";
            m_DataManager.AddSetting(jSetting);
        }

        private void OnRuleRequestSuccess(string requestType, string ruleId)
        {
            switch (requestType)
            {
                case UnityWebRequest.kHttpVerbPUT:
                    m_DataManager.RemoveRuleFromUpdatedRuleIDs(ruleId);
                    m_DataManager.SetLastCachedRulesList();
                    break;
                case UnityWebRequest.kHttpVerbDELETE:
                    m_DataManager.RemoveRuleFromDeletedRuleIDs(ruleId);
                    m_DataManager.SetLastCachedRulesList();
                    break;
            }
            DoCleanUp();
        }

        private void OnSettingsRequestFinished()
        {
            m_DataManager.SetLastCachedKeyList();
            DoCleanUp();
        }

        private void OnPostConfigRequestFinished(string configId)
        {
            m_DataManager.configId = configId;
            DoCleanUp();
        }

        private void OnFailedRequest(long errorCode, string errorMsg)
        {
            DoCleanUp();
        }

        JArray StripMetadataFromRSList(JArray rsJArray)
        {
            var strippedJArray = new JArray();
            for (int i = 0; i < rsJArray.Count; i++)
            {
                strippedJArray.Add(rsJArray[i]["rs"]);
            }
            return strippedJArray;
        }

        JArray StripMetadataFromRSListForSegRule(JArray rsJArray)
        {
            var strippedJArray = new JArray();
            for (int i = 0; i < rsJArray.Count; i++)
            {
                var setting = rsJArray[i]["rs"].DeepClone();
                var settingVal = setting["value"];
                settingVal.Parent.Remove();
                var newValues = new JArray();
                newValues.Add(settingVal);
                setting["values"] = newValues;
                strippedJArray.Add(setting);
            }
            return strippedJArray;
        } 

        JObject StripMetadataFromRule(JObject ruleWithSettingsMetadata)
        {
            var jStrippedRule = (JObject)ruleWithSettingsMetadata.DeepClone();
            if (ruleWithSettingsMetadata["type"].Value<string>() == "segmentation")
            {
                jStrippedRule["value"] = StripMetadataFromRSListForSegRule(ruleWithSettingsMetadata["value"].Value<JArray>());
            }
            else if(ruleWithSettingsMetadata["type"].Value<string>() == "variant")
            {
                var variantsArr = (JArray)jStrippedRule["value"];
                for(int i = 0; i < variantsArr.Count; i++)
                {
                    variantsArr[i]["values"] = StripMetadataFromRSList(variantsArr[i]["values"].Value<JArray>());
                }
            }
            return jStrippedRule;
        }

        private void PushSettings(string environmentId)
        {
            m_IsLoading = true;
            if (string.IsNullOrEmpty(m_DataManager.configId))
            {
                RemoteConfigWebApiClient.postConfigRequestFinished += OnPostConfigRequestFinished;
                m_PostSettingsEventSubscribed = true;
                try
                {
                    RemoteConfigWebApiClient.PostConfig(Application.cloudProjectId, environmentId, StripMetadataFromRSList(m_DataManager.GetRSList()));
                }
                catch
                {
                    DoCleanUp();
                }
            }
            else
            {
                RemoteConfigWebApiClient.settingsRequestFinished += OnSettingsRequestFinished;
                m_PutConfigsEventSubscribed = true;
                try
                {
                    RemoteConfigWebApiClient.PutConfig(Application.cloudProjectId, environmentId, m_DataManager.configId, StripMetadataFromRSList(m_DataManager.GetRSList()));
                }
                catch
                {
                    DoCleanUp();
                }
            }
        }

        private void PushAddedRules(string environmentId)
        {
            var addedRuleIDs = m_DataManager.GetAddedRulesIDs();
            if (addedRuleIDs.Count > 0)
            {
                m_IsLoading = true;
                foreach (var addedRuleID in addedRuleIDs)
                {
                    if (!m_PostAddRuleEventSubscribed)
                    {
                        RemoteConfigWebApiClient.postAddRuleFinished += OnPostAddRuleFinished;
                        m_PostAddRuleEventSubscribed = true;
                    }
                    try
                    {
                        RemoteConfigWebApiClient.PostAddRule(Application.cloudProjectId, environmentId, m_DataManager.configId, StripMetadataFromRule(m_DataManager.GetRuleByID(addedRuleID)));
                    }
                    catch
                    {
                        DoCleanUp();
                    }
                }
            }
        }

        private void PushUpdatedRules(string environmentId)
        {
            var updatedRuleIDs = m_DataManager.GetUpdatedRulesIDs();
            if (updatedRuleIDs.Count > 0)
            {
                m_IsLoading = true;
                if (!m_WebRequestReturnedEventSubscribed)
                {
                    RemoteConfigWebApiClient.ruleRequestSuccess += OnRuleRequestSuccess;
                    m_WebRequestReturnedEventSubscribed = true;
                }
                foreach (var updatedRuleID in updatedRuleIDs)
                {
                    try
                    {
                        RemoteConfigWebApiClient.PutEditRule(Application.cloudProjectId, environmentId, m_DataManager.configId, StripMetadataFromRule(m_DataManager.GetRuleByID(updatedRuleID)));
                    }
                    catch(Exception e)
                    {
                        Debug.Log(e);
                        DoCleanUp();
                    }
                }
            }
        }

        private void PushDeletedRules(string environmentId)
        {
            var deletedRuleIDs = m_DataManager.GetDeletedRulesIDs();
            if (deletedRuleIDs.Count > 0)
            {
                m_IsLoading = true;
                if (!m_WebRequestReturnedEventSubscribed)
                {
                    RemoteConfigWebApiClient.ruleRequestSuccess += OnRuleRequestSuccess;
                    m_WebRequestReturnedEventSubscribed = true;
                }
                foreach (var deletedRuleID in deletedRuleIDs)
                {
                    try
                    {
                        RemoteConfigWebApiClient.DeleteRule(Application.cloudProjectId, environmentId, deletedRuleID);
                    }
                    catch
                    {
                        DoCleanUp();
                    }
                }
            }
        }

        public void OnRuleUpdated(JObject oldRule, JObject newRule)
        {
            m_DataManager.UpdateRule(oldRule, newRule);
        }

        private void OnPostAddRuleFinished(JObject ruleResponse, string oldRuleID)
        {
            m_DataManager.UpdateRuleId(oldRuleID, ruleResponse["id"].Value<string>());
            m_DataManager.RemoveRuleFromAddedRuleIDs(oldRuleID);
            m_DataManager.SetLastCachedRulesList();
            DoCleanUp();
        }

        private void OnFetchRemoteSettingsFinished(JObject config)
        {
            DoCleanUp();
            RemoteConfigWebApiClient.fetchConfigsFinished -= OnFetchRemoteSettingsFinished;
            m_DataManager.SetRSDataStore(config);
            m_DataManager.SetLastCachedKeyList();
            FetchRules();
        }

        private void OnFetchRulesFinished(JArray rules)
        {
            RemoteConfigWebApiClient.fetchRulesFinished -= OnFetchRulesFinished;
            m_DataManager.ClearRuleIDs();
            m_DataManager.SetRulesDataStore(rules);
            m_DataManager.SetLastCachedRulesList();
            DoCleanUp();
        }

        private void OnRulesDataStoreChanged()
        {
            rulesDataStoreChanged?.Invoke();
        }

        private void OnRemoteSettingDataStoreChanged()
        {
            remoteSettingsStoreChanged?.Invoke();
        }

        private void OnEnvironmentChanged()
        {
            m_IsLoading = true;
            environmentChanged?.Invoke();
        }

        private void OnFetchDefaultEnvironmentFinished(string defaultEnvironmentId)
        {
            RemoteConfigWebApiClient.fetchDefaultEnvironmentFinished -= OnFetchDefaultEnvironmentFinished;
            m_DataManager.SetDefaultEnvironment(defaultEnvironmentId);
        }

        private void DoCleanUp()
        {
            if (RemoteConfigWebApiClient.webRequestsAreDone)
            {
                if (m_PostAddRuleEventSubscribed)
                {
                    RemoteConfigWebApiClient.postAddRuleFinished -= OnPostAddRuleFinished;
                    m_PostAddRuleEventSubscribed = false;
                }
                if (m_WebRequestReturnedEventSubscribed)
                {
                    RemoteConfigWebApiClient.ruleRequestSuccess -= OnRuleRequestSuccess;
                    m_WebRequestReturnedEventSubscribed = false;
                }
                if (m_PostSettingsEventSubscribed)
                {
                    RemoteConfigWebApiClient.postConfigRequestFinished -= OnPostConfigRequestFinished;
                    m_PostSettingsEventSubscribed = false;
                }
                if (m_PutConfigsEventSubscribed)
                {
                    RemoteConfigWebApiClient.settingsRequestFinished -= OnSettingsRequestFinished;
                    m_PutConfigsEventSubscribed = false;
                }
                if (m_PostConfigEventSubscribed)
                {
                    RemoteConfigWebApiClient.postConfigRequestFinished -= OnConfigPostFinishedPushHandler;
                    m_PostConfigEventSubscribed = false;
                }
                m_IsLoading = false;
            }
        }

        public bool CompareKeyValueEquality(JArray objectListNew, JArray objectListOld)
        {
            return m_DataManager.CompareKeyValueEquality(objectListNew, objectListOld);
        }

        public bool CompareRulesEquality(JArray ruleListNew, JArray ruleListOld)
        {
            return m_DataManager.CompareRulesEquality(ruleListNew, ruleListOld);
        }

        public void UpdateRemoteSetting(JObject oldItem, JObject newItem)
        {
            m_DataManager.UpdateSetting(oldItem, newItem);
        }

        public void UpdateSettingForRule(string ruleId, JObject updatedSetting)
        {
            m_DataManager.UpdateSettingForRule(ruleId, updatedSetting);
        }

        public void DeleteRemoteSetting(string entityId)
        {
            m_DataManager.DeleteSetting(entityId);
        }

        public void DeleteSettingFromRule(string selectedRuleId, string entityId)
        {
            m_DataManager.DeleteSettingFromRule(selectedRuleId, entityId);
        }
    }
}
