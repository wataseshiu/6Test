using System.Collections.Generic;
using Card;

namespace GameData
{
    public class TurnDataList
    {
        public List<TurnData> turnDataList = new List<TurnData>();
        public int WinPlayer { get; set; }
    }

    public class TurnData
    {
        public TurnData(int turnNumber, CardType playerSelectedCardType, CardType opponentSelectedCardType, int playerSelectedCardIndex, int opponentSelectedCardIndex)
        {
            TurnNumber = turnNumber;
            PlayerSelectedCardType = playerSelectedCardType;
            OpponentSelectedCardType = opponentSelectedCardType;
            PlayerSelectedCardIndex = playerSelectedCardIndex;
            OpponentSelectedCardIndex = opponentSelectedCardIndex;
            
            // Debug.Log($"Player : {playerSelectedCardType}, {playerSelectedCardIndex} vs Opponent : {opponentSelectedCardType}, {opponentSelectedCardIndex}");
        }

        public int TurnNumber { get; }
        public CardType PlayerSelectedCardType { get; }
        public CardType OpponentSelectedCardType { get; }
        public int PlayerSelectedCardIndex { get; }
        public int OpponentSelectedCardIndex { get; }
    }
}