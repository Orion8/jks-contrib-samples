using Scripts.Utility;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectArcher.Coretech.CustomDigitalTwin
{
    public class CDTBootStrapper : MonoBehaviour
    {
        [Tooltip("When enabled, the system assumes no internet connection is available and bypasses Archer101 Bootstrapper initialization. Instead, it directly loads the CDT Scene")]
        [SerializeField] private bool _forcedOfflineMode = false;
        
        [Tooltip("When enabled, the system will load Archer101 Bootstrapper scene regardless whether it is online or offline.")]
        [SerializeField] private bool _alwaysLoad101Scene = false;

        
#if UNITY_EDITOR
        [Header("Scene")]
        
        [Tooltip("Archer101 Bootstrapper scene for initialization.")]
        [SerializeField] private SceneAsset _101Scene;
     
        [Tooltip("CDTScene is used for selecting a Custom Digital Twin scene.")]
        [SerializeField] private SceneAsset _cdtScene;
#endif
        
        [HideInInspector] [SerializeField] private string _cdtSceneName;
        [HideInInspector] [SerializeField] private string _101SceneName;

        private void Start()
        {
            LoadScene();
        }


        private void LoadScene()
        {
            if (!_forcedOfflineMode && CheckInternet.IsConnected() )
            {
                Debug.Log($"JKS Internet connection is available.. Load {_101SceneName} scene");
                SceneManager.LoadScene(_101SceneName, LoadSceneMode.Single);
            }
            else
            {
                Debug.Log($"JKS Internet connection is NOT available.. Load {_cdtSceneName} scene");
                SceneManager.LoadScene(_cdtSceneName, LoadSceneMode.Additive);
            }

        }
        
        
         
        
        
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            _cdtSceneName = _cdtScene != null ? _cdtScene.name : "CDTMain";
            _101SceneName = _101Scene != null ? _101Scene.name : "Bootstrapper";
        }
#endif

    }
}