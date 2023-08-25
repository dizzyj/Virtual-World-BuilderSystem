using UnityEngine;

public class NetworkOfficeStartPosition : MonoBehaviour
{
    public void Awake()
        {
            CVWNetworkManager.RegisterOfficeStartPosition(transform);
        }

        public void OnDestroy()
        {
            CVWNetworkManager.UnregisterOfficeStartPosition(transform);
        }
}
