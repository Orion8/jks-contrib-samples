using Newtonsoft.Json.Linq;
using UnityEngine;

namespace ProjectArcher.Coretech.CustomDigitalTwin
{
    public static class SnapshotVersion
    {
 
        //todo: set this in Unity inspector
        private const string LatestVersion = "2.0.0";
        

        
        public static bool IsNewVersion(string jsonString)
        {
            string version = GetVersionFromJson(jsonString);

            if (version == null)
            {
                Debug.LogError($"Snapshot version is not supported. Use version {LatestVersion}");
                return false;
            }   
            
            if(GetVersionFromJson(jsonString) == LatestVersion)
                return true;
            
            Debug.LogError($"Snapshot version is not supported. Use version {LatestVersion}");
            return false;
        }
        



        public static string GetVersionFromJson(string jsonString)
        {
            JObject jsonObject = JObject.Parse(jsonString);

            // Ensure the "version" field exists.
            if (jsonObject.ContainsKey("formatVersion"))
            {
                string version = jsonObject["formatVersion"].Value<string>();
                return version;
            }
            
            Debug.LogError("No 'version' field found in the provided JSON.");
            return null;
        }
    }
}