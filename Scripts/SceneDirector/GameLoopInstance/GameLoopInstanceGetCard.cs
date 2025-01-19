using System;
using System.Collections.Generic;
using System.Linq;
using Card;
using CardShop;
using Cysharp.Threading.Tasks;
using GameData;
using Hand;
using MultiPlay;
using R3;
using Timeline;
using UI;
using Unity.Netcode;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace SceneDirector.GameLoopInstance
{
    public class GameLoopInstanceGetCard : IGameLoopInstanceBase
    {
        private PhaseAnnounceManager _phaseAnnounceManager;
        private CardShopManager _cardShopManager;
        private HandManager _myHandManager;
        private HandManager _opponentHandManager;
        private TurnDataList _turnDataList;
        private MultiPlayManager _multiPlayManager;
        private HUDToDoPresenter _hudToDoPresenter;

        //IGameLoopInstanceBaseの実装
        public Subject<Unit> StateEnd { get; } = new();
        public GameState NextState { get; set; }
        
        public GameLoopInstanceGetCard(IEnumerable<HandManager> handManagers, PhaseAnnounceManager phaseAnnounceManager, CardShopManager cardShopManager, TurnDataList turnDataList, HUDToDoPresenter hudToDoPresenter)
        {
            var enumerable = handManagers.ToList();
            _myHandManager = enumerable.First();
            _opponentHandManager = enumerable.Last();
            _phaseAnnounceManager = phaseAnnounceManager;
            _cardShopManager = cardShopManager;
            _turnDataList = turnDataList;
            _hudToDoPresenter = hudToDoPresenter;
        }
        
        
        //IGameLoopInstanceBaseの実装
        public async　UniTask Initialize()
        {
            Debug.Log("GameLoopInstanceGetCard Initialize");
            return;
        }
        
        public async UniTask Initialize(MultiPlayManager multiPlayManager)
        {
            _multiPlayManager = multiPlayManager;
            await Initialize();
        }

        public async UniTask Execute()
        {
            Debug.Log("GameLoopInstanceGetCard Execute");
            
            //カード入手フェーズのアナウンスを表示
            await _phaseAnnounceManager.ShowPhaseAnnounce(Phase.GetCard, 0);
            
            CardMover selectCardFirst;
            CardMover selectCardSecond;

            if (_multiPlayManager)
            {
                bool isHostFirst = _turnDataList.turnDataList.Count % 2 == 0; 
                Debug.Log(_turnDataList.turnDataList.Count);
                Debug.Log("isHostFirst : " + isHostFirst);

                await _cardShopManager.ShowCardShop();
                _hudToDoPresenter.SetToDoTextActive(true);
                //ホスト先攻
                if (isHostFirst)
                {
                    //1人目：ホスト（選択）ゲスト（待機）
                    if (_multiPlayManager.IsHost)
                    {
                        Debug.Log("1人目：ホストはカード選択");
                        _hudToDoPresenter.SetToDoText(HUDToDoText.SelectGetCard);

                        selectCardFirst = await _cardShopManager.SelectCardFirst(_multiPlayManager);
                        await _myHandManager.AddCardToHand(selectCardFirst);
                    }
                    else
                    {
                        Debug.Log("1人目：ゲストは待機");
                        _hudToDoPresenter.SetToDoText(HUDToDoText.WaitOpponentSelectBattleCard);

                        var index = await _multiPlayManager.WaitSelectShopItemHost.FirstAsync();
                        await _opponentHandManager.AddCardToHand(_cardShopManager.GetCardFromIndex(index));
                        _cardShopManager.RemoveCardLineupFromIndex(index);
                    }
                
                    //2人目：ホスト（待機）ゲスト（選択）
                    if(_multiPlayManager.IsHost)
                    {
                        Debug.Log("2人目：ホストは待機");
                        _hudToDoPresenter.SetToDoText(HUDToDoText.WaitOpponentSelectBattleCard);

                        var index = await _multiPlayManager.WaitSelectShopItemGuest.FirstAsync();
                        await _opponentHandManager.AddCardToHand(_cardShopManager.GetCardFromIndex(index));
                        _cardShopManager.RemoveCardLineupFromIndex(index);
                    }
                    else
                    {
                        Debug.Log("2人目：ゲストはカード選択");
                        _hudToDoPresenter.SetToDoText(HUDToDoText.SelectGetCard);

                        selectCardSecond = await _cardShopManager.SelectCardSecond(_multiPlayManager);
                        await _myHandManager.AddCardToHand(selectCardSecond);
                    }
                }
                //ゲスト先行
                else
                {
                    //1人目：ホスト（待機）ゲスト（選択）
                    if (_multiPlayManager.IsHost)
                    {
                        Debug.Log("1人目：ホストは待機");
                        _hudToDoPresenter.SetToDoText(HUDToDoText.WaitOpponentSelectBattleCard);

                        var index = await _multiPlayManager.WaitSelectShopItemGuest.FirstAsync();
                        await _opponentHandManager.AddCardToHand(_cardShopManager.GetCardFromIndex(index));
                        _cardShopManager.RemoveCardLineupFromIndex(index);
                    }
                    else
                    {
                        Debug.Log("1人目：ゲストはカード選択");
                        _hudToDoPresenter.SetToDoText(HUDToDoText.SelectGetCard);

                        selectCardFirst = await _cardShopManager.SelectCardFirst(_multiPlayManager);
                        await _myHandManager.AddCardToHand(selectCardFirst);
                    }
                
                    //2人目：ホスト（選択）ゲスト（待機）
                    if(_multiPlayManager.IsHost)
                    {
                        Debug.Log("2人目：ホストはカード選択");
                        _hudToDoPresenter.SetToDoText(HUDToDoText.SelectGetCard);
                        
                        selectCardSecond = await _cardShopManager.SelectCardSecond(_multiPlayManager);
                        await _myHandManager.AddCardToHand(selectCardSecond);
                    }
                    else
                    {
                        Debug.Log("2人目：ゲストは待機");
                        _hudToDoPresenter.SetToDoText(HUDToDoText.WaitOpponentSelectBattleCard);
                        
                        var index = await _multiPlayManager.WaitSelectShopItemHost.FirstAsync();
                        await _opponentHandManager.AddCardToHand(_cardShopManager.GetCardFromIndex(index));
                        _cardShopManager.RemoveCardLineupFromIndex(index);
                    }
                }
                await _cardShopManager.HideCardShop();
                _hudToDoPresenter.SetToDoTextActive(false);
            }
            else
            {
                _hudToDoPresenter.SetToDoTextActive(true);
                //奇数ターンは自分が先攻、偶数ターンは相手が先攻
                if (_turnDataList.turnDataList.Count % 2 == 1)
                {
                    await _cardShopManager.ShowCardShop();
                    _hudToDoPresenter.SetToDoText(HUDToDoText.SelectGetCard);

                    selectCardFirst = await _cardShopManager.SelectCardFirst();
                    await _myHandManager.AddCardToHand(selectCardFirst);
                
                    selectCardSecond = await _cardShopManager.SelectCardNPC();
                    await _opponentHandManager.AddCardToHand(selectCardSecond);
                }
                else
                {
                    await _cardShopManager.ShowCardShop();

                    selectCardFirst = await _cardShopManager.SelectCardNPC();
                    await _opponentHandManager.AddCardToHand(selectCardFirst);

                    selectCardSecond = await _cardShopManager.SelectCardSecond();
                    await _myHandManager.AddCardToHand(selectCardSecond);
                }
                await _cardShopManager.HideCardShop();
                _hudToDoPresenter.SetToDoTextActive(false);
            }
        }

        public async UniTask<GameState> Terminate()
        {
            _cardShopManager.ResetCardShop();
            
            Debug.Log("GameLoopInstanceGetCard Terminate");
            StateEnd.OnNext(Unit.Default);
            NextState = GameState.SelectCard;
            return NextState;
        }
    }
}