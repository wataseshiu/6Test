using Cysharp.Threading.Tasks;
using R3;

namespace SceneDirector.GameLoopInstance
{
    public interface IGameLoopInstanceBase
    {
        public Subject<Unit> StateEnd { get; }
        public GameState NextState { get; set; }
        public UniTask Initialize();
        public UniTask Execute();
        public UniTask<GameState> Terminate();
    }
}