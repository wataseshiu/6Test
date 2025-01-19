using System;
using System.Linq;
using Unity.Netcode;
using Unity.Networking.Transport;
using UnityEngine;

namespace MultiPlay
{
    public class CustomNetworkManager : NetworkManager
    {
        [SerializeField] private NetworkPrefab multiPlayManagerPrefab;
        private NetworkObject _multiPlayManagerNetworkObject;
        private MultiPlayManager _multiPlayManager;
        private void Start()
        {
            multiPlayManagerPrefab = NetworkConfig.Prefabs.Prefabs.FirstOrDefault(prefab => prefab.Prefab.name == "NetworkPlayer");
            Debug.Log("multiPlayManagerPrefab: " + multiPlayManagerPrefab);

            OnServerStarted += OnServerStartedCallback;
        }
        private void OnServerStartedCallback()
        {
            Debug.Log("OnServerStarted");
            if(IsHost && _multiPlayManager == null)
            {
                _multiPlayManager = Instantiate(multiPlayManagerPrefab.Prefab).GetComponent<MultiPlayManager>();
                
                _multiPlayManagerNetworkObject = _multiPlayManager.GetComponent<NetworkObject>();
                _multiPlayManagerNetworkObject.Spawn();
                Debug.Log($"MultiPlayManager Spawned : {_multiPlayManager.name}");
            }
        }
    }
}