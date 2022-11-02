using System;
using System.Collections.Generic;
using System.Linq;

namespace BattleM
{
    /// <summary>
    /// 特殊招式
    /// </summary>
    public interface IExert
    {
        /** 效果类型
         * 1.连击(循环)
         * 2.叠加(值++)
         * 3.强化(力，敏，架，身)
         * 4.打击目标(hp & tp,hp,tp,mp)
         * 5.转化伤害(硬转,内转,气转,血转)(消耗转)
         * 6.固加(力，敏，架，身)
         */
        /** 持久
         * 1.回合制
         * 2.消耗制(攻,受击,架,闪,移)
         */
        enum Effects
        {
            None,
            /// <summary>
            /// 叠加
            /// </summary>
            Stack,
            /// <summary>
            /// 硬直伤害
            /// </summary>
            BusyToDamage
        }
        int Combo { get; }
        int Lasting { get; }
        enum Harms
        {
            Default,
            HpOnly,
            TpOnly,
            MpOnly
        }

        Harms Harm();
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
        int TarBusy { get; }
        ICombo Combo { get; }
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
        int Tp { get; }
        int Mp { get; }
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
    public interface ICombatSkill : ISkillName
    {
    }

    public interface ISkillName
    {
        string Name { get; }
    }
    /// <summary>
    /// 内功心法(战斗)
    /// </summary>
    public interface IForce : ICombatSkill, IBreathNode
    {
        /// <summary>
        /// 内力转化率
        /// </summary>
        int MpRate { get; }

        /// <summary>
        /// 内力值使用在护甲上
        /// </summary>
        int MpArmor { get; }
        //IList<IForceForm> Forms { get; }
    }

    /// <summary>
    /// 武功(战斗)
    /// </summary>
    public interface IMartial : ICombatSkill
    {
        Way.Armed Armed { get; }
        IList<ICombatForm> Combats { get; }
    }

    /// <summary>
    /// 轻功(战斗)
    /// </summary>
    public interface IDodge : ICombatSkill, IDepletionForm, IBreathNode
    {
        /// <summary>
        /// 身法值
        /// </summary>
        int Dodge { get; }
        //IList<IDodgeForm> Forms { get; }
    }
}
