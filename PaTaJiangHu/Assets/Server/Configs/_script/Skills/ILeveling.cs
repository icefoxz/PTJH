namespace Server.Configs.Skills
{
    internal interface ILeveling<out T>
    {
        T GetFromLevel(int level);
        T GetMaxLevel();
    }
}