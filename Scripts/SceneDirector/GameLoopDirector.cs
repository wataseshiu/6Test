using Card;
using GameData;
using R3;
using SceneDirector.GameLoopInstance;
using VContainer;

namespace SceneDirector
{
    public class GameLoopDirector
    {
        private TurnDataList _turnDataList;
        public GameState State { get; private set; } = GameState.StartGame;

        private IGameLoopInstanceBase _currentGameLoopInstanceInstance;
        private readonly IObjectResolver _resolver;

        public Subject<Unit> GameEnd { get; } = new Subject<Unit>();

        public GameLoopDirector(IObjectResolver resolver, TurnDataList turnDataList)
        {
            _resolver = resolver;
            _turnDataList = turnDataList;
        }

        public void Initialize()
        {
            State = GameState.StartGame;
            Execute();
        }

        private async void Execute()
        {
            int turn = 1;
            while (true)
            {
                _currentGameLoopInstanceInstance = CreateGameLoopInstance();

                await _currentGameLoopInstanceInstance.Initialize();
                
                //GameLoopInstanceSelectCardの場合だけ呼び出すExecuteメソッドに戻り値がある
                if(State == GameState.SelectCard)
                {
                    var instance = (GameLoopInstanceSelectCard)_currentGameLoopInstanceInstance;
                    var (playerType, opponentType, playerSelectCardIndex, opponentSelectCardIndex) = await instance.Execute();

                    _turnDataList.turnDataList.Add(new TurnData( turn, playerType,opponentType, playerSelectCardIndex, opponentSelectCardIndex));
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
        SelectCard,
        ShowBattle,
        EndGame,
        QuitGameLoop
    }
}