namespace Core
{
    public struct DefendPower
    {
        public float Strength { get; }
        public float Agility { get; }
        public float Armor { get; }
        public float Force { get; }
        public float Dodge { get; }
        public float Combat { get; }
        public float Power() => (Strength * 0.5f) + Agility + Armor + Force + Dodge + Combat;

        public DefendPower(float strength, float agility, float force, float dodge, float armor, float combat)
        {
            Strength = strength;
            Agility = agility;
            Armor = armor;
            Force = force;
            Dodge = dodge;
            Combat = combat;
        }
    }
    public struct OffendPower
    {
        public float Strength { get; }
        public float Weapon { get; }
        public float Combat { get; }
        public float Power() => (Strength * 0.5f) + Weapon + Combat;
        public OffendPower(float strength, float weapon, float combat)
        {
            Strength = strength;
            Weapon = weapon;
            Combat = combat;
        }
    }
}