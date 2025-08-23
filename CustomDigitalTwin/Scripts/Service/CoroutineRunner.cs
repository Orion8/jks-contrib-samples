using UnityEngine;

namespace ProjectArcher.Coretech.CustomDigitalTwin
{
    public class CoroutineRunner : MonoBehaviour
    {
        private static CoroutineRunner _instance;
        
        public static CoroutineRunner Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("CoroutineRunnerObject");
                    _instance = go.AddComponent<CoroutineRunner>();
                }
                return _instance;
            }
        }
        
    }
}