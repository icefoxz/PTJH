using BattleM;
using System;

namespace Server.Controllers.Skills
{
    internal interface ILeveling<out T>
    {
        T GetFromLevel(int level);
        T GetMaxLevel();
    }
}