namespace MultiPlay
{
    public enum NetworkState
    {
        StartGetCardPhase,
        WaitOtherPlayerGetCard,
        StartSelectCardPhase,
        WaitOtherPlayerSelectCard,
        StartShowBattlePhase,
        WaitShowBattleEnd,
    }
}