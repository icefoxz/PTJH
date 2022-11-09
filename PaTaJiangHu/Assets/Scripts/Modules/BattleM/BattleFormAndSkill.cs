using System.Collections.Generic;
using UnityEngine;

namespace BattleM
{
    /// <summary>
    /// 玩家介入操作接口
    /// </summary>
    public interface IExert
    {
        IPerform.Activities Activity { get; }
        IPerform GetPerform(IBreathBar bar);
    }
    /// <summary>
    /// 武功履行总接口
    /// </summary>
    public interface IPerform
    {
        /** 需求：效果类型
        * 1.连击(循环) - 主动攻击
        * 2.叠加(值++) - buff功能
        * 3.强化(力，敏，架，身) - buff强化
        * 4.打击目标(hp & tp,hp,tp,mp)
        * 5.转化伤害(硬转,内转,气转,血转)(消耗转)
        * 6.固加(力，敏，架，身)
        */
        /// <summary>
        /// 活动类别(主动行动)
        /// </summary>
        public enum Activities
        {
            [InspectorName("进攻")]Attack,
            [InspectorName("恢复")]Recover,
            [InspectorName("自动")]Auto,
            //Function
        }

        IForce ForceSkill { get; }
        IDodge DodgeSkill { get;  }
        /// <summary>
        /// 主动活动类型
        /// </summary>
        Activities Activity { get; }
        ICombatForm CombatForm { get; }
        IRecovery Recover { get; }
        bool IsReposition { get; }
    }

    /// <summary>
    /// 所有招式的底层接口
    /// </summary>
    public interface ISkillForm : ISkillName
    {
    }

    /// <summary>
    /// 战斗招式
    /// </summary>
    public interface ICombatForm : IParryForm, IBreathNode
    {
        /// <summary>
        /// 连击
        /// </summary>
        ICombo Combo { get; }
        /// <summary>
        /// 内力使用
        /// </summary>
        int CombatMp { get; }
        /// <summary>
        /// 打掉对方内力
        /// </summary>
        int DamageMp { get; }
        /// <summary>
        /// 获取buff
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="append"></param>
        /// <returns></returns>
        IBuffInstance[] GetBuffs(ICombatUnit unit, ICombatBuff.Appends append);
    }
    /// <summary>
    /// 连击招式
    /// </summary>
    public interface ICombo
    {
        /// <summary>
        /// 每个连击打出的伤害百分比
        /// </summary>
        int[] Rates { get; }
    }

    /// <summary>
    /// 消耗类型的招式接口
    /// </summary>
    public interface IDepletionForm : ISkillForm
    {
    }

    ///// <summary>
    ///// 内功一式
    ///// </summary>
    //public interface IForceForm : IBreathNode, ISkillForm
    //{
    //}
    public interface IParryForm : IDepletionForm
    {
        /// <summary>
        /// 招架值
        /// </summary>
        int Parry { get; }
        int ParryMp { get; }
        int TarBusy { get; }
        int OffBusy { get; }
    }

    //public interface IDodgeForm : IDepletionForm, IBreathNode
    //{
    //    /// <summary>
    //    /// 身法值
    //    /// </summary>
    //    int Dodge { get; }
    //}
    /// <summary>
    /// 技能组合基础类，基本就是内功，轻功，武功的基类，作为各种招式的容器规范
    /// </summary>
    public interface ISkill : ISkillName
    {
    }

    public interface ISkillName
    {
        string Name { get; }
    }
    /// <summary>
    /// 恢复技能
    /// </summary>
    public interface IRecovery : ISkill
    {
        /// <summary>
        /// 补血或气的消耗值
        /// </summary>
        int Recover { get; }
        /// <summary>
        /// 获取Buff
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="append"></param>
        /// <returns></returns>
        IBuffInstance[] GetBuffs(ICombatUnit unit, ICombatBuff.Appends append);
    }
    /// <summary>
    /// 内功心法(战斗)
    /// </summary>
    public interface IForce : IRecovery,IBreathNode
    {
        /// <summary>
        /// 内功转化率
        /// </summary>
        int ForceRate { get; }
        /// <summary>
        /// 蓄转内
        /// </summary>
        int MpConvert { get; }
        /// <summary>
        /// 护甲消耗
        /// </summary>
        int ArmorDepletion { get; }
        /// <summary>
        /// 内力值使用在护甲上
        /// </summary>
        int Armor { get; }
    }

    /// <summary>
    /// 武功(战斗)
    /// </summary>
    public interface IMartial : ISkill
    {
        Way.Armed Armed { get; }
        IList<ICombatForm> Combats { get; }
        IList<IExert> Exerts { get; }
    }

    /// <summary>
    /// 轻功(战斗)
    /// </summary>
    public interface IDodge : ISkill, IDepletionForm, IBreathNode
    {
        /// <summary>
        /// 身法值
        /// </summary>
        int Dodge { get; }
        int DodgeMp { get; }
        //IList<IDodgeForm> Forms { get; }
    }
}
