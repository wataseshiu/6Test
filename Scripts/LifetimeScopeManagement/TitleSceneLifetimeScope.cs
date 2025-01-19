using Audio;
using CanvasController;
using CardShop;
using GameData;
using Hand;
using LobbyManagement;
using MultiPlay;
using SceneDirector;
using SceneDirector.GameLoopInstance;
using Session;
using Timeline;
using UI;
using UI.Fade;
using UnityEngine;
using UnityEngine.Serialization;
using VContainer;
using VContainer.Unity;

namespace LifetimeScopeManagement
{
    public class TitleSceneLifetimeScope : LifetimeScope
    {
        [SerializeField] TitleCanvasController titleCanvasController;
        [SerializeField] HandManager myHandManager;
        [SerializeField] HandManager opponentHandManager;
        [SerializeField] HandMover myHandMover;
        [SerializeField] HandMover opponentHandMover;
        [SerializeField] PhaseAnnounceManager phaseAnnounceManager;
        [SerializeField] DialogManager dialogManager;
        [SerializeField] HUDLifeManager playerHUDLifeManager;
        [SerializeField] HUDLifeManager opponentHUDLifeManager;
        [SerializeField] CardShopPresenter cardShopPresenter;
        [SerializeField] BGMManager bgmManager;
        [SerializeField] SeManager seManager;
        [SerializeField] FadeView fadeView;
        [SerializeField] CustomNetworkManager customNetworkManager;
        [SerializeField] HUDToDoPresenter hudToDoPresenter;
        
        protected override void Configure(IContainerBuilder builder)
        {
            // fadeViewを登録
            builder.RegisterInstance(fadeView).AsImplementedInterfaces();
            // titleCanvasControllerを登録
            builder.RegisterInstance(titleCanvasController).AsImplementedInterfaces();
            // phaseAnnounceManagerを登録
            builder.RegisterInstance(phaseAnnounceManager).AsImplementedInterfaces();
            // dialogManagerを登録
            builder.RegisterInstance(dialogManager).AsImplementedInterfaces();
            //CardShopPresenterを登録
            builder.RegisterInstance(cardShopPresenter).AsImplementedInterfaces();
            // BGMManagerを登録
            builder.RegisterInstance(bgmManager).AsImplementedInterfaces();
            // SeManagerを登録
            builder.RegisterInstance(seManager).AsImplementedInterfaces();
            // customNetworkManagerを登録
            builder.RegisterInstance(customNetworkManager).AsImplementedInterfaces();
            // HUDToDoPresenterを登録
            builder.RegisterInstance(hudToDoPresenter).AsImplementedInterfaces();

            // CardShopManagerを登録
            builder.Register<CardShopManager>(Lifetime.Singleton);
            
            // HUDLifePresenterを登録
            builder.Register<HUDLifePresenter>(Lifetime.Singleton);

            // FadePresenterを登録
            builder.Register<FadePresenter>(Lifetime.Singleton);
            
            // handManagerを登録
            builder.Register(_ => myHandManager, Lifetime.Scoped);
            builder.Register(_ => opponentHandManager, Lifetime.Scoped);
            
            // handMoveVisualizerを登録
            builder.Register(_ => myHandMover, Lifetime.Scoped);
            builder.Register(_ => opponentHandMover, Lifetime.Scoped);
            
            // HUDLifeManagerを登録
            builder.Register(_ => playerHUDLifeManager, Lifetime.Scoped);
            builder.Register(_ => opponentHUDLifeManager, Lifetime.Scoped);
            
            //QuickSessionDirectorを登録
            builder.Register<QuickSessionDirector>(Lifetime.Singleton);
      
            //sessionMakerを登録
            builder.Register<SessionMaker>(Lifetime.Singleton);
            
            //LobbyMakerを登録
            builder.Register<LobbyMaker>(Lifetime.Singleton);
            
            //TurnDataを登録
            builder.Register<TurnDataList>(Lifetime.Singleton);

            builder.Register<GameLoopInstanceStartGame>(Lifetime.Transient);
            builder.Register<GameLoopInstanceGetCard>(Lifetime.Transient);
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
