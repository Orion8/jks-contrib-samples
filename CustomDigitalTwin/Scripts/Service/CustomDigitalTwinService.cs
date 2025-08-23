using System.Threading.Tasks;
using UnityEngine; 
using System;
using System.Net;
using ProjectArcher.CoreTech.CloudServices;
using ProjectArcher.CoreTech.Foundations.Services;
using Scripts.Utility;


namespace ProjectArcher.Coretech.CustomDigitalTwin
{
    
    
    public class CustomDigitalTwinService : ICustomDigitalTwinService
    {
        
        private Transform _cdtStoreRoot;
        
        private bool _isOfflineMode = false;
        
        
        public CustomDigitalTwinService(in bool isOffline)
        {
            _isOfflineMode = isOffline;
        }

        
        public async Task Initialize(ServiceManagerBootstrapper serviceManagerBootstrapper)
        {
            // if internet connection is NOT available, do not attempt to download the latest snapshot(scene description) from cloud
            if (_isOfflineMode || !CheckInternet.IsConnected())
            {
                await Task.CompletedTask;
                return;
            }          
            
            await serviceManagerBootstrapper.GetDependencyService<ICloudManagerService>();
            await serviceManagerBootstrapper.GetDependencyService<ICloudUserProvider>();
            await serviceManagerBootstrapper.GetDependencyService<IAppInfoService>();

            var tcs = new TaskCompletionSource<object>();
            MakeSnapshotFileReady(() => tcs.SetResult(null));
            await tcs.Task;
        }
        
        public Task Shutdown()
        {
            return Task.CompletedTask;
        }


         
        
        private void MakeSnapshotFileReady(Action onLocalSnapshotReady = null)
        {
            Debug.Log("JKS Internet connection is available.. MakeSnapshotFileReady()");

            RemoteSnapshot.DownloadURL(url =>
            {
                if (string.IsNullOrEmpty(url))
                    url = StoreConstants.GetTemporaryDefaultSnapshotURL();

                CoroutineRunner.Instance.
                    StartCoroutine(
                        RemoteSnapshot.DownloadJson(url, json =>
                        {
                            LocalSnapshot.SaveSnapshot(json);

                            onLocalSnapshotReady?.Invoke();
                        })
                    );

            });
        }
        
        
        public void SetStoreRoot(Transform storeRoot)
        {
            _cdtStoreRoot = storeRoot;
        }
        
        public Transform GetStoreRoot()
        {
            return _cdtStoreRoot;
        }
        
        
        
    }
    
    
    
}