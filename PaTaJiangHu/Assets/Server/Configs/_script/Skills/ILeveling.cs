namespace Server.Configs._script.Skills
{
    internal interface ILeveling<out T>
    {
        T GetFromLevel(int level);
        T GetMaxLevel();
    }
}