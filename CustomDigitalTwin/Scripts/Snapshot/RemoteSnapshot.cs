using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;


namespace ProjectArcher.Coretech.CustomDigitalTwin
{
    public static class RemoteSnapshot
    {
        
        public static void DownloadURL(Action<string> callback)
        {

            Task task = BootProperties.GetSnapshotURL(url =>
            {
                callback(url);
            });

        }
        
        
        
        public static IEnumerator DownloadJson(string url, Action<string> callback)
        {
            Debug.Log("JKS Downloading snapshot from: " + url); 
            
            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                yield return webRequest.SendWebRequest();
                
                if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.Log(webRequest.error);
                }
                else
                {
                    Debug.Log("JKS Downloading successful ");

                    callback(webRequest.downloadHandler.text);
                }
            }
        }
        
        
        private static YieldInstruction _wait = new WaitForSeconds(1);

                
        public static IEnumerator CheckInternetConnection(Action<bool> callback)
        {
            //jks intended delay : delay needed for manual console log target selection.
            // Debug.Log("JKS Checking internet connection in 5 sec. ");
            // yield return _wait;
            // Debug.Log("JKS Checking internet connection in 4 sec. ");
            // yield return _wait;
            // Debug.Log("JKS Checking internet connection in 3 sec. ");
            // yield return _wait;
            // Debug.Log("JKS Checking internet connection in 2 sec. ");
            // yield return _wait;
            // Debug.Log("JKS Checking internet connection in 1 sec. ");
            // yield return _wait;
            // Debug.Log("JKS Checking internet connection now. ");
            
            using (UnityWebRequest webRequest = UnityWebRequest.Get("https://www.google.com"))
            {
                yield return webRequest.SendWebRequest();

                callback(webRequest.result == UnityWebRequest.Result.Success);
            }
            
        }
        
        
      }
    
    
}