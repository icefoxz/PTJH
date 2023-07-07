namespace AOT._AOT.Core
{
    public struct DefendPower
    {
        public float Strength { get; }
        public float Agility { get; }
        public float Power => Strength + Agility;

        public DefendPower(float strength, float agility)
        {
            Strength = strength;
            Agility = agility;
        }
    }
    public struct CombatPower
    {
        public float Strength { get; }
        public float Hp { get; }
        public float Mp { get; }
        public float Agility { get; }
        public float Power => (Strength + Agility) * (Hp + Mp) / 1000;
        public CombatPower(float strength, float agility, float hp, float mp)
        {
            Strength = strength;
            Agility = agility;
            Hp = hp;
            Mp = mp;
        }
    }
}