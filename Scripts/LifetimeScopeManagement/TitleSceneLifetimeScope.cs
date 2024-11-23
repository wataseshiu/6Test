using CanvasController;
using GameData;
using Hand;
using LobbyManagement;
using SceneDirector;
using SceneDirector.GameLoopInstance;
using Session;
using Timeline;
using UI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace LifetimeScopeManagement
{
    public class TitleSceneLifetimeScope : LifetimeScope
    {
        [SerializeField] TitleCanvasController titleCanvasController;
        [SerializeField] QuickSessionCanvasController quickSessionCanvasController;
        [SerializeField] HandManager myHandManager;
        [SerializeField] HandManager opponentHandManager;
        [SerializeField] HandMoveVisualizer myHandMoveVisualizer;
        [SerializeField] HandMoveVisualizer opponentHandMoveVisualizer;
        [SerializeField] PhaseAnnounceManager phaseAnnounceManager;
        [SerializeField] DialogManager dialogManager;
        
        protected override void Configure(IContainerBuilder builder)
        {
            // titleCanvasControllerを登録
            builder.RegisterInstance(titleCanvasController).AsImplementedInterfaces();
            // quickSessionCanvasControllerを登録
            builder.RegisterInstance(quickSessionCanvasController).AsImplementedInterfaces();
            // phaseAnnounceManagerを登録
            builder.RegisterInstance(phaseAnnounceManager).AsImplementedInterfaces();
            // dialogManagerを登録
            builder.RegisterInstance(dialogManager).AsImplementedInterfaces();

            // handManagerを登録
            builder.Register(_ => myHandManager, Lifetime.Scoped);
            builder.Register(_ => opponentHandManager, Lifetime.Scoped);
            
            // handMoveVisualizerを登録
            builder.Register(_ => myHandMoveVisualizer, Lifetime.Scoped);
            builder.Register(_ => opponentHandMoveVisualizer, Lifetime.Scoped);
            
            //QuickSessionDirectorを登録
            builder.Register<QuickSessionDirector>(Lifetime.Singleton);
      
            //sessionMakerを登録
            builder.Register<SessionMaker>(Lifetime.Singleton);
            
            //LobbyMakerを登録
            builder.Register<LobbyMaker>(Lifetime.Singleton);
            
            //TurnDataを登録
            builder.Register<TurnDataList>(Lifetime.Singleton);

            builder.Register<GameLoopInstanceStartGame>(Lifetime.Transient);
            builder.Register<GameLoopInstanceSelectCard>(Lifetime.Transient);
            builder.Register<GameLoopInstanceShowBattle>(Lifetime.Transient);
            builder.Register<GameLoopInstanceEndGame>(Lifetime.Transient);

            //GameLoopDirectorを登録
            builder.Register<GameLoopDirector>(Lifetime.Singleton);
            
            //TitleSceneDirectorをEntryPointとして登録
            builder.RegisterEntryPoint<TitleSceneDirector>();
        }
    }
}
