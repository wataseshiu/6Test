using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace Session
{
    public class SessionMaker
    {
        public string RelayCode { private set; get; } = "null";

        public async UniTask<string> CreateRelay()
        {
            try
            {
                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);
                RelayCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
                Debug.Log(RelayCode);
                
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                    allocation.RelayServer.IpV4,
                    (ushort)allocation.RelayServer.Port,
                    allocation.AllocationIdBytes,
                    allocation.Key,
                    allocation.ConnectionData
                    );
                
                NetworkManager.Singleton.StartHost();
                return RelayCode;
            }
            catch (RelayServiceException e)
            {
                Debug.Log(e);
                return null;
            }
        }

        public async UniTask JoinRelay(string joinRelayCode)
        {
            try
            {
                JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinRelayCode);
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                    joinAllocation.RelayServer.IpV4,
                    (ushort)joinAllocation.RelayServer.Port,
                    joinAllocation.AllocationIdBytes,
                    joinAllocation.Key,
                    joinAllocation.ConnectionData,
                    joinAllocation.HostConnectionData
                );
                NetworkManager.Singleton.StartClient();
            }
            catch (RelayServiceException e)
            {
                Debug.Log(e); 
            }
        }

        public async UniTask IsAllMemberConnectedRelay()
        {
            await UniTask.WaitUntil(()=> NetworkManager.Singleton.ConnectedClients.Count == 2);
        }
    }
}