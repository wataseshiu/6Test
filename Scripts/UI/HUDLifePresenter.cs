using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using GameData;
using R3;

namespace UI
{
    public class HUDLifePresenter
    {
        //PlayerとOpponentのGameStatusを受け取る
        private GameStatus _playerGameStatus;
        private GameStatus _opponentGameStatus;
        
        //HUDLifeManagerを受け取る
        private HUDLifeManager _playerHUDLifeManager;
        private HUDLifeManager _opponentHUDLifeManager;
        
        public bool IsPlayerDead = false;
        public bool IsOpponentDead = false;

        //PlayerとOpponentのGameStatusを受け取る
        public HUDLifePresenter(IEnumerable<HUDLifeManager> hudLifeManagers)
        {
            _playerGameStatus = new GameStatus();
            _opponentGameStatus = new GameStatus();
            
            var enumerable1 = hudLifeManagers.ToList();
            _playerHUDLifeManager = enumerable1.First();
            _opponentHUDLifeManager = enumerable1.Last();
        }
        
        //初期化処理
        public void Initialize()
        {
            _playerHUDLifeManager.Initialize(_playerGameStatus.MaxLife);
            _opponentHUDLifeManager.Initialize(_opponentGameStatus.MaxLife);

            //GameStatusのOnDeadをSubscribeし発火されたらIsPlayerDeadをtrueにする
            _playerGameStatus.OnDead.Subscribe(_ =>
            {
                IsPlayerDead = true;
            });
            
            //GameStatusのOnDeadをSubscribeし発火されたらIsOpponentDeadをtrueにする
            _opponentGameStatus.OnDead.Subscribe(_ =>
            {
                IsOpponentDead = true;
            });
        }
        
        public void Reset()
        {
            _playerGameStatus.Reset();
            _opponentGameStatus.Reset();
            _playerHUDLifeManager.Initialize(_playerGameStatus.MaxLife);
            _opponentHUDLifeManager.Initialize(_opponentGameStatus.MaxLife);
            IsPlayerDead = false;
            IsOpponentDead = false;
        }
        
        //PlayerのLifeを増やす
        public async void AddPlayerLife(int amount)
        {
            await AddLife(_playerGameStatus, _playerHUDLifeManager, amount);
        }
        
        //OpponentのLifeを増やす
        public async void AddOpponentLife(int amount)
        {
            await AddLife(_opponentGameStatus, _opponentHUDLifeManager, amount);
        }

        private async UniTask AddLife(GameStatus gameStatus, HUDLifeManager hudLifeManager,  int amount)
        {
            gameStatus.Heal(amount);
            await hudLifeManager.AddLife(amount);
        }
        
        //PlayerのLifeを減らす
        public async void RemovePlayerLife(int amount)
        {
            await RemoveLife(_playerGameStatus, _playerHUDLifeManager, amount);
        }
        
        //OpponentのLifeを減らす
        public async void RemoveOpponentLife(int amount)
        {
            await RemoveLife(_opponentGameStatus, _opponentHUDLifeManager, amount);
        }

        private async UniTask RemoveLife(GameStatus playerGameStatus, HUDLifeManager playerHUDLifeManager, int amount)
        {
            playerGameStatus.Damage(amount);
            await playerHUDLifeManager.RemoveLife(amount);
        }
    }
}