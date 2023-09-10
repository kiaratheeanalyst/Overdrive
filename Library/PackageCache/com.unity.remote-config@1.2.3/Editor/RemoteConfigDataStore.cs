using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Unity.RemoteConfig.Editor.Tests")]

namespace Unity.RemoteConfig.Editor
{
    internal class RemoteConfigDataStore : ScriptableObject, ISerializationCallbackReceiver
    {
        // Data stores for Remote Settings
        public JArray rsKeyList;
        public JArray rsLastCachedKeyList;
        public string configId;

        public string currentEnvironment;
        public string currentEnvironmentId;
        public bool currentEnvironmentIsDefault;

        public JArray environments;

        // Data stores for Rules
        public JArray rulesList;
        public JArray lastCachedRulesList;

        public List<string> addedRulesIDs;
        public List<string> updatedRulesIDs;
        public List<string> deletedRulesIDs;

        // Values for Serialization
        public string _rsKeyList;
        public string _rsLastCachedKeyList;
        public string _environments;
        public string _rulesList;
        public string _lastCachedRulesList;

        public void OnBeforeSerialize()
        {
            _rsKeyList = rsKeyList == null ? "" : rsKeyList.ToString();
            _rsLastCachedKeyList = rsLastCachedKeyList == null ? "" : rsLastCachedKeyList.ToString();
            _environments = environments == null ? "" : environments.ToString();
            _rulesList = rulesList == null ? "" : rulesList.ToString();
            _lastCachedRulesList = lastCachedRulesList == null ? "" : lastCachedRulesList.ToString();
        }

        public void OnAfterDeserialize()
        {
            rsKeyList = string.IsNullOrEmpty(_rsKeyList) ? new JArray() : JArray.Parse(_rsKeyList);
            rsLastCachedKeyList = string.IsNullOrEmpty(_rsLastCachedKeyList) ? new JArray() : JArray.Parse(_rsLastCachedKeyList);
            environments = string.IsNullOrEmpty(_environments) ? new JArray() : JArray.Parse(_environments);
            rulesList = string.IsNullOrEmpty(_rulesList) ? new JArray() : JArray.Parse(_rulesList);
            lastCachedRulesList = string.IsNullOrEmpty(_lastCachedRulesList) ? new JArray() : JArray.Parse(_lastCachedRulesList);
        }
    }
}