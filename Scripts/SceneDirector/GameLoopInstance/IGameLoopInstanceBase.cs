using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using MultiPlay;
using R3;
using Unity.Netcode;

namespace SceneDirector.GameLoopInstance
{
    public interface IGameLoopInstanceBase
    {
        public Subject<Unit> StateEnd { get; }
        public GameState NextState { get; set; }
        public UniTask Initialize();
        public UniTask Execute();
        public UniTask<GameState> Terminate();
        public UniTask Initialize(MultiPlayManager multiPlayManager);
    }
}