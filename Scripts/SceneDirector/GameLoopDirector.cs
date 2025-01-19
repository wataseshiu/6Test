using System;
using System.Linq;
using Card;
using CardShop;
using GameData;
using MultiPlay;
using R3;
using SceneDirector.GameLoopInstance;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using Random = Unity.Mathematics.Random;

namespace SceneDirector
{
    public class GameLoopDirector
    {
        private TurnDataList _turnDataList;
        
        public GameState State { get; private set; } = GameState.StartGame;

        private IGameLoopInstanceBase _currentGameLoopInstanceInstance;
        private readonly IObjectResolver _resolver;

        public Subject<Unit> GameEnd { get; } = new Subject<Unit>();
        
        private MultiPlayManager _multiPlayManager;
        private CardShopManager _cardShopManager;

        public GameLoopDirector(IObjectResolver resolver, TurnDataList turnDataList, CardShopManager cardShopManager)
        {
            _resolver = resolver;
            _turnDataList = turnDataList;
            _cardShopManager = cardShopManager;
        }

        public void Initialize()
        {
            State = GameState.StartGame;
            _cardShopManager.InitializeRandom(new Random((uint) (DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds));
            Execute();
        }
        public void Initialize( MultiPlayManager multiPlayManager)
        {
            _multiPlayManager = multiPlayManager;
            State = GameState.StartGame;
            _cardShopManager.InitializeRandom(_multiPlayManager.cardShopRandom);
            Execute();
        }

        private async void Execute()
        {
            int turn = 1;
            while (true)
            {
                _currentGameLoopInstanceInstance = CreateGameLoopInstance();

                //マルチプレイ時はローカルプレイヤーのNetworkObjectを引数に渡す
                if(_multiPlayManager != null)
                {
                    await _currentGameLoopInstanceInstance.Initialize(_multiPlayManager);
                }
                else
                {
                    await _currentGameLoopInstanceInstance.Initialize();
                }
                
                //GameLoopInstanceSelectCardの場合だけ呼び出すExecuteメソッドに戻り値がある
                if(State == GameState.SelectCard)
                {
                    var instance = (GameLoopInstanceSelectCard)_currentGameLoopInstanceInstance;
                    var (playerType, opponentType, playerSelectCardIndex, opponentSelectCardIndex) = await instance.Execute();

                    //TurnDataListに選択したカードの情報を追加
                    _turnDataList.turnDataList.Add(new TurnData( turn, playerType,opponentType, playerSelectCardIndex, opponentSelectCardIndex));
                    Debug.Log("Enemy Select Card : " + _turnDataList.turnDataList.Last().OpponentSelectedCardIndex);
                    turn++;
                }
                else
                {
                    await _currentGameLoopInstanceInstance.Execute();
                }
                var nextState = await _currentGameLoopInstanceInstance.Terminate();

                State = nextState;
                if(State == GameState.QuitGameLoop){break;}
            }
            Terminate();
        }

        private void Terminate()
        {
            GameEnd.OnNext(Unit.Default);
        }

        private IGameLoopInstanceBase CreateGameLoopInstance()
        {
            //現在のGameStateに応じてIGameLoopBaseに実装クラスを割り当てる
            switch (State)
            {
                case GameState.StartGame:
                    return _resolver.Resolve<GameLoopInstanceStartGame>();
                case GameState.GetCard:
                    return _resolver.Resolve<GameLoopInstanceGetCard>();
                case GameState.SelectCard:
                    return _resolver.Resolve<GameLoopInstanceSelectCard>();
                case GameState.ShowBattle:
                    return _resolver.Resolve<GameLoopInstanceShowBattle>();
                case GameState.EndGame:
                    return _resolver.Resolve<GameLoopInstanceEndGame>();
            }
            return null;
        }
    }

    //ゲームの進行状態を示すenum
    public enum GameState
    {
        StartGame,
        GetCard,
        SelectCard,
        ShowBattle,
        EndGame,
        QuitGameLoop
    }
}