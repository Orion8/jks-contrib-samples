using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using UnityEngine;

namespace ProjectArcher.Coretech.CustomDigitalTwin
{
    public static class SnapshotJSONSchemaChecker
    {
       
        public static bool CheckIfNewFormat(string jsonString)
        {
            // Validate the JSON data against the schema
            if (IsValidFormat(jsonString))
            {
                Debug.Log("JSON data matches the schema.");
                return true;
            }
            
            return false;
        }
        
        //todo: support multiple schemas/versions
        private static bool IsValidFormat(string jsonString)
        {
            if (!SnapshotVersion.IsNewVersion(jsonString))
                return false;
            
            JObject obj = JObject.Parse(jsonString);
            return ValidateSimpleSchema(obj, _snapshot_2_0_0_Schema);
        }
        
        
        private class SimplifiedSnapshotJsonSchema
        {
            public Dictionary<string, JTokenType> Properties { get; set; }
            
            public Dictionary<string, JTokenType> FixtureLibraryProperties { get; set; }
            public Dictionary<string, JTokenType> FixturesProperties { get; set; }
        }
        
        private static SimplifiedSnapshotJsonSchema _snapshot_2_0_0_Schema = new SimplifiedSnapshotJsonSchema
        {
            Properties = new Dictionary<string, JTokenType>
            {
                { "store", JTokenType.String },
                { "name", JTokenType.String },
                { "fixtureLibrary", JTokenType.Array },
                { "fixtures", JTokenType.Array }
            },
            
            FixtureLibraryProperties = new Dictionary<string, JTokenType>
            {
                { "width", JTokenType.Float},
                { "height", JTokenType.Float},
                { "length", JTokenType.Float},
                { "centerX", JTokenType.Float },
                { "centerZ", JTokenType.Float },
                { "frontCenterX", JTokenType.Float },
                { "frontCenterZ", JTokenType.Float },
                { "fixtureTypeId", JTokenType.String },
            },
            
            FixturesProperties = new Dictionary<string, JTokenType>
            {
                { "centerPivotX", JTokenType.Float },
                { "centerPivotZ", JTokenType.Float },
                { "rotationY", JTokenType.Integer },
                { "fixtureTypeId", JTokenType.String },
                { "zoneAisleSection", JTokenType.String }
            }
        };

        private static bool ValidateSimpleSchema(JObject jsonObj, SimplifiedSnapshotJsonSchema schema)
        {
            foreach (var property in schema.Properties)
            {
                JToken token;
                if (!jsonObj.TryGetValue(property.Key, out token) || token.Type != property.Value)
                {
                    return false;
                }
            }
            
            
            JArray fixtureLibrary = (JArray)jsonObj["fixtureLibrary"];
            foreach (JObject fixture in fixtureLibrary)
            {
                foreach (var property in schema.FixtureLibraryProperties)
                {
                    JToken token;
                    if (!fixture.TryGetValue(property.Key, out token))
                    {
                        return false;
                    }
                }
            }
            

            JArray fixtures = (JArray)jsonObj["fixtures"];
            foreach (JObject fixture in fixtures)
            {
                foreach (var property in schema.FixturesProperties)
                {
                    JToken token;
                    if (!fixture.TryGetValue(property.Key, out token))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        
    }
}