using Newtonsoft.Json;
using UnityEngine;
using Application = UnityEngine.Device.Application;


namespace ProjectArcher.Coretech.CustomDigitalTwin
{
    
    
    
    public static class LocalSnapshot
    {
        
        
        private static string _filePath = Application.persistentDataPath + "/CDTSnapshotNew.json";
        
        
        
        public static void SaveSnapshot(string snapshot)
        {
            bool isNewFormat = SnapshotJSONSchemaChecker.CheckIfNewFormat(snapshot);
            if (isNewFormat)
            {
                Save(snapshot, _filePath);
            }
            else
            {
                Debug.LogError("JKS Snapshot is not in valid format. Please check the schema."); 
            }
        }
        
         

        
        
        private static void Save(string snapshot, string path)
        {
            System.IO.File.WriteAllText(path, snapshot);
            
            // Check if the file exists
            if (System.IO.File.Exists(path))
            {
                Debug.Log("JKS Snpashot file was successfully written.\n");
            }
            else
            {
                Debug.LogError("JKS Failed to write the file.\n");
            }
        }

        
        
        
        public static string Load()
        {
            Debug.Log("JKS Load snapshot file.");
            return System.IO.File.ReadAllText(_filePath);
        }
        
    }
    
    
}