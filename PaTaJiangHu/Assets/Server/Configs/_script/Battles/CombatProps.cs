namespace Server.Configs.Battles
{
    /// <summary>
    /// 战斗单位基础属性
    /// </summary>
    internal record CombatProps : ICombatProps
    {
        public float StrAddon { get; }
        public float AgiAddon { get; }
        public float HpAddon { get; }
        public float MpAddon { get; }

        public CombatProps(float strength, float agility, float hp, float mp)
        {
            StrAddon = strength;
            AgiAddon = agility;
            HpAddon = hp;
            MpAddon = mp;
        }
    }
}