
using System;
using Newtonsoft.Json;
using Scripts.Utility;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;


namespace ProjectArcher.Coretech.CustomDigitalTwin
{
    public class CreateStoreFromSceneDescription : MonoBehaviour
    {
#if UNITY_EDITOR

        [Tooltip(@"Used for selecting a alternative scene to load after CDT initialization.
                 The main scene is useful for creating a clean scene while developing a feature.")]
        [SerializeField] private SceneAsset _mainScene;
#endif
        [HideInInspector] [SerializeField] private string _mainSceneName;

        
        [SerializeField] private bool _overrideBootPropertySnapshot = false;
        [SerializeField] private string _snapshotToOverrideWith;  // snapshot file name
        [SerializeField] private string _snapshotURLToOverrideWith; //url
        private string SnapshotToOverrideWith => _snapshotURLToOverrideWith + _snapshotToOverrideWith + ".json";
        
        
        [SerializeField] private FixtureModelLibrary _fixtureModelLibrary;

        private void Awake()
        {
            FixtureModelLibraryManager.Initialize(_fixtureModelLibrary);
        }

 
        
        private void Start()
        {
            Debug.Log("JKS CreateStoreFromSceneDescription.cs Start()");
            
            if (_overrideBootPropertySnapshot)
                CreateStoreFromLocalSceneDescriptionFile_UseOverride(OnStoreCreationCompleted);
            else
                CreateStoreFromLocalSceneDescriptionFile(OnStoreCreationCompleted);
            
            CreateLight();
        }
        
        

        private void CreateStoreFromLocalSceneDescriptionFile(Action onStoreCreated) // scene description = snapshot
        {
            string snapshotJson = LocalSnapshot.Load();
            if (snapshotJson != null)
                CreateStore(snapshotJson);
            
            onStoreCreated?.Invoke();
        }
        
        
        private void CreateStoreFromLocalSceneDescriptionFile_UseOverride(Action onOverridenStoreCreated)
        {
            if (!CheckInternet.IsConnected())
            {
                string snapshotJson = LocalSnapshot.Load();
                if (snapshotJson != null)
                    CreateStore(snapshotJson);
                
                onOverridenStoreCreated?.Invoke();
                return;
            }
            
            StartCoroutine(
                RemoteSnapshot.DownloadJson(SnapshotToOverrideWith, json =>
                {
                    LocalSnapshot.SaveSnapshot(json);

                    string snapshotJson = LocalSnapshot.Load();
                    if (snapshotJson != null)
                        CreateStore(snapshotJson);
                    
                    onOverridenStoreCreated?.Invoke();
                })
            );
        }


         
        
        private void CreateStore(string json)
        {
            StoreJsonModel storeJsonModel = JsonConvert.DeserializeObject<StoreJsonModel>(json);

            Store.Create(storeJsonModel);

            RenderModeManager rmm = FindObjectOfType<RenderModeManager>();
            if (rmm != null)
                rmm.SetFixturesRenderMode(RenderMode.OcclusionDebugDraw);
        }




        private void OnStoreCreationCompleted()
        {
            if (!string.IsNullOrEmpty(_mainSceneName))
            {
                Debug.Log($"JKS OnStoreCreationCompleted.. Load {_mainSceneName} scene");

                SceneManager.LoadScene(_mainSceneName, LoadSceneMode.Additive);
            }            
            //activate marker tracker
            Instantiate(Resources.Load("MarkerTracker"));
        }


        private void CreateLight()
        {
            if (GameObject.Find("Directional Light") != null || FindObjectOfType<Light>() != null)
                return;
            
            //create directional light
            GameObject lightGameObject = new GameObject("Directional Light");
            
            Light lightComp = lightGameObject.AddComponent<Light>();
            lightComp.type = LightType.Directional;
            lightComp.color = Color.white;
            lightComp.intensity = 1f;
            lightComp.shadows = LightShadows.None;

            lightGameObject.transform.rotation = Quaternion.Euler(50, -30, 0);
        }

        
        
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            _mainSceneName = _mainScene != null ? _mainScene.name : "";
        }
#endif

    }
}