using System;
using System.Collections.Generic;
using Card;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using R3;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using random = Unity.Mathematics.Random;

namespace MultiPlay
{
    public class MultiPlayManager : NetworkBehaviour
    {
        private int _initialValue = 99;
        //カード選択用
        private Subject<int> WaitSelectCardHost = new Subject<int>();
        private Subject<int> WaitSelectCardGuest = new Subject<int>();
        
        NetworkVariable<int> _hostSelectCard = new NetworkVariable<int>(99);
        NetworkVariable<int> _guestSelectCard = new NetworkVariable<int>(99);
        public NetworkVariable<NetworkState> hostState = new NetworkVariable<NetworkState>(NetworkState.StartSelectCardPhase);
        public NetworkVariable<NetworkState> guestState = new NetworkVariable<NetworkState>(NetworkState.StartSelectCardPhase);

        [SerializeField] private int _hostSelectCardView;
        [SerializeField] private int _guestSelectCardView;

        //カードショップ用乱数_乱数のタネを同期して、それをホストゲストで同じ順序で用いたら結果が同じになる想定
        NetworkVariable<int> _cardShopRandomSeed = new NetworkVariable<int>();
        [SerializeField] private int _cardShopRandomSeedView;
        public random cardShopRandom;
        
        //カードショップアイテム選択用
        NetworkVariable<int> _hostSelectShopItem = new NetworkVariable<int>(99);
        NetworkVariable<int> _guestSelectShopItem = new NetworkVariable<int>(99);
        public Subject<int> WaitSelectShopItemHost = new Subject<int>();
        public Subject<int> WaitSelectShopItemGuest = new Subject<int>();
        
        public NetworkVariable<int> TurnOwner { get; private set; } = new NetworkVariable<int>(0);

        public async void Initialize()
        {
            Debug.Log("MultiPlayManager Initialize");

            SetupOnTurnStart();

            _cardShopRandomSeed.OnValueChanged += OnCreateRandomFromRandomSeed;
            //カードショップの乱数シードを設定
            if (IsHost)
            {
                SetRandomSeedServerRPC();
            }
        }
        
        public void SetupOnTurnStart()
        {
            //イベントハンドラを念の為解除
            _hostSelectCard.OnValueChanged -= OnOtherHostPlayerSelectCard;
            _guestSelectCard.OnValueChanged -= OnOtherGuestPlayerSelectCard;
            
            _hostSelectShopItem.OnValueChanged -= OnSelectShopItemHost;
            _guestSelectShopItem.OnValueChanged -= OnSelectShopItemGuest;
            
            //NetworkVariableのターン開始時初期値を設定
            _hostSelectCard.Value = _initialValue;
            _guestSelectCard.Value = _initialValue;

            _hostSelectShopItem.Value = _initialValue;
            _guestSelectShopItem.Value = _initialValue;
            
            //ビューに値反映
            _hostSelectCardView = _hostSelectCard.Value;
            _guestSelectCardView = _guestSelectCard.Value;
            
            //イベントハンドラを設定
            _hostSelectCard.OnValueChanged += OnOtherHostPlayerSelectCard;
            _guestSelectCard.OnValueChanged += OnOtherGuestPlayerSelectCard;
            
            _hostSelectShopItem.OnValueChanged += OnSelectShopItemHost;
            _guestSelectShopItem.OnValueChanged += OnSelectShopItemGuest;
        }
        
        //ショップアイテム入手処理用のメソッド
        private void OnSelectShopItemHost(int previousValue, int newValue)
        {
            Debug.Log("OnSelectShopItem : "+newValue);
            SelectShopItemCore(newValue, _hostSelectShopItem, WaitSelectShopItemHost);
        }

        private void OnSelectShopItemGuest(int previousValue, int newValue)
        {
            Debug.Log("OnSelectShopItem : "+newValue);
            SelectShopItemCore(newValue, _guestSelectShopItem, WaitSelectShopItemGuest);
        }
        
        private void SelectShopItemCore(int newValue, NetworkVariable<int> handler, Subject<int> subject)
        {
            subject.OnNext(newValue);
        }

        private void OnCreateRandomFromRandomSeed( int previousValue, int newValue)
        {
            Debug.Log($"Seed Changed :{newValue}");
            _cardShopRandomSeedView = newValue;
            cardShopRandom = new random((uint) _cardShopRandomSeed.Value);
            cardShopRandom.NextInt();//初回の乱数は捨てる
            Debug.Log("CreateRandomFromRandomSeed : "+cardShopRandom.NextInt(0,3));
            Debug.Log("CreateRandomFromRandomSeed : "+cardShopRandom.NextInt(0,3));
            Debug.Log("CreateRandomFromRandomSeed : "+cardShopRandom.NextInt(0,3));
            
            //このコールバックを解除する
            _cardShopRandomSeed.OnValueChanged -= OnCreateRandomFromRandomSeed;
        }

        [ServerRpc(RequireOwnership = false)]
        private void SetRandomSeedServerRPC()
        {
            //UNIX時間をシードに設定
            _cardShopRandomSeed.Value = (int) (DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds;
            Debug.Log("SetRandomSeedServerRPC : "+_cardShopRandomSeed.Value);
        }

        public void SendSelectShopCard(int cardMover)
        {
            if (IsHost)
            {
                Debug.Log("ホストが自分のショップアイテムを選択しRPC通知をコールした: " + cardMover);
                SendClientSelectShopCardHostServerRpc(cardMover);
            }
            else if (IsClient)
            {
                Debug.Log("ゲストが自分のショップアイテムを選択しRPC通知をコールした: " + cardMover);
                SendClientSelectShopCardGuestServerRpc(cardMover);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void SendClientSelectShopCardGuestServerRpc(int index)
        {
            _guestSelectShopItem.Value = index;
            WaitSelectShopItemGuest.OnNext(index);
        }

        [ServerRpc(RequireOwnership = false)]
        private void SendClientSelectShopCardHostServerRpc(int index)
        {
            _hostSelectShopItem.Value = index;
            WaitSelectShopItemHost.OnNext(index);
        }

        public void SendSelectCard(int index)
        {
            if(IsHost)
            {
                Debug.Log("ホストが自分のカードを選択しRPC通知をコールした: " + index);
                SendClientSelectCardHostServerRpc( index, NetworkState.WaitOtherPlayerSelectCard);
            }
            else if (IsClient)
            {
                Debug.Log("ゲストが自分のカードを選択しRPC通知をコールした: " + index);
                SendClientSelectCardGuestServerRpc( index, NetworkState.WaitOtherPlayerSelectCard);
            }
        }
        [Unity.Netcode.ServerRpc(RequireOwnership = false)]
        private void SendClientSelectCardHostServerRpc(int index, NetworkState state)
        {
            _hostSelectCard.Value = index;
            hostState.Value = state;
            Debug.Log($"RPCでセットした : _hostSelectCard = {_hostSelectCard.Value} : hostState = {hostState.Value}");
        }
        [Unity.Netcode.ServerRpc(RequireOwnership = false)]
        private void SendClientSelectCardGuestServerRpc(int index, NetworkState state)
        {
            _guestSelectCard.Value = index;
            guestState.Value = state;
            Debug.Log($"RPCでセットした : _guestSelectCard = {_guestSelectCard.Value} : guestState = {guestState.Value}");
        }
        
        /// <summary>
        /// ホストの場合はゲストのカードが決まるまでawaitする、ゲストの場合はホストのカードが決まるまでawaitする
        /// </summary>
        /// <returns></returns>
        public async UniTask<int> WaitOtherPlayerSelectCard()
        {
            if(IsHost)
            {
                Debug.Log("ホストはゲストのカードが決まるのを待っています");
//                _guestSelectCard.OnValueChanged += OnOtherGuestPlayerSelectCard;
                await UniTask.Yield();
                var result = await WaitSelectCardGuest.FirstAsync();
                Debug.Log("ホスト側でゲストのカード決定を受け取りました :"+result);
                return result;
            }
            else if (IsClient)
            {
                Debug.Log("ゲストはホストのカードが決まるのを待っています");
//                _hostSelectCard.OnValueChanged += OnOtherHostPlayerSelectCard;
                await UniTask.Yield();
                var result = await WaitSelectCardHost.FirstAsync();
                Debug.Log("ゲスト側でホストのカード決定を受け取りました :"+ result);
                return result;
            }
            return 999; //ここにはこない
        }
        
        /// <summary>
        /// ホストのカード選択がNetworkVariableでセットされたときに呼ばれる
        /// </summary>
        /// <param name="previousValue"></param>
        /// <param name="newValue"></param>
        private void OnOtherHostPlayerSelectCard(int previousValue, int newValue)
        {
            Debug.Log("OnOtherHostPlayerSelectCard triggered");
            if (IsHost)
            {
                Debug.Log("ホスト側");
                _hostSelectCardView = newValue;
            }
            else if(IsClient)
            {
                Debug.Log("ゲスト側");
                _hostSelectCardView = newValue;
                Debug.Log("ホストのカードが決定したのでSubject通知 :"+ newValue);
                Debug.Log("ホストのカードが決定したのでSubject通知 :"+ _hostSelectCardView);
                WaitSelectCardHost.OnNext(newValue);
            }
        }
        /// <summary>
        /// ゲストのカード選択がNetworkVariableでセットされたときに呼ばれる
        /// </summary>
        /// <param name="previousValue"></param>
        /// <param name="newValue"></param>
        private void OnOtherGuestPlayerSelectCard(int previousValue, int newValue)
        {
            Debug.Log("OnOtherGuestPlayerSelectCard triggered");
            if (IsHost)
            {
                Debug.Log("ホスト側");
                _guestSelectCardView = newValue;
                Debug.Log("ゲストのカードが決定したのでSubject通知 :"+ newValue);
                Debug.Log("ゲストのカードが決定したのでSubject通知 :"+ _guestSelectCardView);
                WaitSelectCardGuest.OnNext(newValue);
            }
            else if (IsClient)
            {
                Debug.Log("ゲスト側");
                _guestSelectCardView = newValue;
            }
        }
    }
}