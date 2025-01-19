using R3;

namespace GameData
{
    public class GameStatus
    {
        public int CurrentLife { get; set; }
        public int MaxLife { get; set; }
        
        //CurrentLifeが0になった時に発行するSubject
        public Subject<Unit> OnDead { get; } = new Subject<Unit>();
        
        public GameStatus(int maxLife = 3)
        {
            MaxLife = maxLife;
            CurrentLife = maxLife;
        }
        
        public void Damage(int damage)
        {
            CurrentLife -= damage;
            if (CurrentLife <= 0)
            {
                CurrentLife = 0;
                OnDead.OnNext(Unit.Default);
            }
        }
        
        public void Heal(int heal)
        {
            CurrentLife += heal;
            if (CurrentLife > MaxLife)
            {
                CurrentLife = MaxLife;
            }
        }
        
        public void Reset()
        {
            CurrentLife = MaxLife;
        }
    }
}