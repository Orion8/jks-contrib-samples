using UnityEngine;

namespace ProjectArcher.Coretech.CustomDigitalTwin
{
    
    
    public interface ICustomDigitalTwinService : IService
    {
        
        public void SetStoreRoot(Transform storeRoot);
        
        public Transform GetStoreRoot();
        
    }
    
    
    
}