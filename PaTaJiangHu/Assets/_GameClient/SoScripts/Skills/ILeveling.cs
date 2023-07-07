namespace GameClient.SoScripts.Skills
{
    internal interface ILeveling<out T>
    {
        T GetFromLevel(int level);
        T GetMaxLevel();
    }
}