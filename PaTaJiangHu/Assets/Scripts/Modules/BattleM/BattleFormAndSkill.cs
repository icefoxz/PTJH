using System;
using System.Collections.Generic;
using System.Linq;

namespace BattleM
{
    /// <summary>
    /// 所有招式的底层接口
    /// </summary>
    public interface ISkillForm
    {
        string Name { get; }
    }

    /// <summary>
    /// 战斗招式
    /// </summary>
    public interface ICombatForm : IDepletionForm, IBreathNode
    {
        int TarBusy { get; }
        int OffBusy { get; }
    }
    /// <summary>
    /// 消耗类型的招式接口
    /// </summary>
    public interface IDepletionForm : ISkillForm
    {
        int Qi { get; }
        int Mp { get; }
    }

    /// <summary>
    /// 内功一式
    /// </summary>
    public interface IForceForm : IBreathNode, ISkillForm
    {
    }
    public interface IParryForm : IDepletionForm
    {
        /// <summary>
        /// 招架转化率
        /// </summary>
        int Parry { get; }

        int OffBusy { get; }
    }

    public interface IDodgeForm : IDepletionForm, IBreathNode
    {
        /// <summary>
        /// 身法化率
        /// </summary>
        int Dodge { get; }
    }
    /// <summary>
    /// 技能组合基础类，基本就是内功，轻功，武功的基类，作为各种招式的容器规范
    /// </summary>
    public interface ICombatSkill
    {
        string Name { get; }
    }
    /// <summary>
    /// 内功心法(战斗)
    /// </summary>
    public interface IForce : ICombatSkill
    {
        /// <summary>
        /// 内力转化率
        /// </summary>
        int MpRate { get; }
        /// <summary>
        /// 内力值使用在护甲上
        /// </summary>
        int MpArmor { get; }
        IList<IForceForm> Forms { get; }
    }

    /// <summary>
    /// 武功(战斗)
    /// </summary>
    public interface IMartial : ICombatSkill
    {
        Way.Armed Armed { get; }
        IList<ICombatForm> Combats { get; }
        IList<IParryForm> Parries { get; }
    }

    /// <summary>
    /// 轻功(战斗)
    /// </summary>
    public interface IDodge : ICombatSkill
    {
        IList<IDodgeForm> Forms { get; }
    }
    public class ForceSkill : IForce
    {
        private readonly List<IForceForm> _forms;

        public static ForceSkill Instance(string name, int mpRate, int mpArmor, IList<IForceForm> forms) =>
            new(name, mpRate, mpArmor, forms);

        public static ForceSkill Instance(string name, int mpRate, int mpArmor) =>
            new(name, mpRate, mpArmor, new List<IForceForm>());
        public string Name { get; }
        public int MpRate { get; }
        public int MpArmor { get; }


        public IList<IForceForm> Forms => _forms;

        public ForceSkill(string name, int mpRate, int mpArmor, IList<IForceForm> forms)
        {
            Name = name;
            MpRate = mpRate;
            MpArmor = mpArmor;
            _forms = forms.ToList();
        }

        public void AddForm(string name, int breath)
        {
            Forms.Add(new Form(name, breath));
        }

        private class Form : IForceForm
        {
            public string Name { get; }
            public int Breath { get; }

            public Form(string name, int breath)
            {
                Name = name;
                Breath = breath;
            }
        }

        public override string ToString() => Name;
    }

    public class Martial : IMartial
    {

        public static Martial Instance(string name, Way.Armed kind) =>
            Instance(name, kind, Array.Empty<ICombatForm>(), Array.Empty<IParryForm>());
        public static Martial Instance(string name, Way.Armed type, ICombatForm[] kungFuforms, IParryForm[] parryForms) =>
            new(name, type, kungFuforms, parryForms);

        private readonly List<ICombatForm> _combats;
        private readonly List<IParryForm> _parries;
        public string Name { get; }
        public Way.Armed Armed { get; }

        public IList<ICombatForm> Combats => _combats;

        public IList<IParryForm> Parries => _parries;

        private Martial(string name, Way.Armed kind, IList<ICombatForm> kungFuForms,IList<IParryForm> parryForms)
        {
            Name = name;
            Armed = kind;
            _combats = kungFuForms.ToList();
            _parries = parryForms.ToList();
        }

        public void AddParryForm(string name, int parry, int busy, int tp, int mp) =>
            _parries.Add(new ParryForm(name, parry, busy, tp, mp));

        public void AddSwordForm(string name, int breath, int tp, int mp, int tarBusy, int offBusy) =>
            _combats.Add(new WeaponForm(name, breath, tp, mp, tarBusy, offBusy, Way.Armed.Sword));

        public void AddUnarmedForm(string name, int breath, int tp, int mp, int tarBusy, int offBusy) =>
            _combats.Add(new UnarmedForm(name, breath, tp, mp, tarBusy, offBusy, Way.Armed.Unarmed));

        private class FormBase
        {
            public string Name { get; protected set; }
            public override string ToString() => Name;
        }
        /// <summary>
        /// 招架招式
        /// </summary>
        private class ParryForm : FormBase, IParryForm
        {
            public int Parry { get; }
            public int OffBusy { get; }
            public int Qi { get; }
            public int Mp { get; }

            public ParryForm(string name, int parry, int busy, int tp, int mp)
            {
                Name = name;
                Parry = parry;
                OffBusy = busy;
                Qi = tp;
                Mp = mp;
            }
        }
        /// <summary>
        /// 武器(战斗)招式
        /// </summary>
        private class WeaponForm : FormBase, ICombatForm
        {
            public int Breath { get; }
            public int Qi { get; }
            public int Mp { get; }
            public int TarBusy { get; }
            public int OffBusy { get; }
            public Way.Armed Armed { get; }

            public WeaponForm(string name, int breath, int tp, int mp, int tarBusy, int offBusy, Way.Armed armedKinds)
            {
                Name = name;
                Breath = breath;
                Qi = tp;
                Mp = mp;
                TarBusy = tarBusy;
                OffBusy = offBusy;
                Armed = armedKinds;
            }

        }
        /// <summary>
        /// 空手(战斗)招式
        /// </summary>
        private class UnarmedForm : FormBase, ICombatForm
        {
            public int Breath { get; }
            public int Qi { get; }
            public int Mp { get; }
            public int TarBusy { get; }
            public int OffBusy { get; }
            public Way.Armed Armed { get; }

            public UnarmedForm(string name, int breath, int tp, int mp, int tarBusy, int offBusy, Way.Armed armedKinds)
            {
                Name = name;
                Breath = breath;
                Qi = tp;
                Mp = mp;
                TarBusy = tarBusy;
                OffBusy = offBusy;
                Armed = armedKinds;
            }

        }

        public override string ToString() => Name;
    }
    public class DodgeSkill : IDodge
    {
        public static DodgeSkill Instance(string name) => Instance(name, Array.Empty<IDodgeForm>());
        public static DodgeSkill Instance(string name, IList<IDodgeForm> forms) => new(name, forms);
        private readonly List<IDodgeForm> _forms;

        public string Name { get; }
        public IList<IDodgeForm> Forms => _forms;

        public DodgeSkill(string name, IEnumerable<IDodgeForm> list)
        {
            Name = name;
            _forms = list.ToList();
        }

        public void AddForm(string name, int dodge, int breath, int tp, int mp) =>
            _forms.Add(new Form(name, dodge, breath, tp, mp));

        private class Form : IDodgeForm
        {
            public string Name { get; }
            public int Dodge { get; }
            public int Qi { get; }
            public int Mp { get; }
            public int Breath { get; }
            public Form(string name, int dodge, int breath, int tp, int mp)
            {
                Name = name;
                Dodge = dodge;
                Mp = mp;
                Qi = tp;
                Breath = breath;
            }

        }

        public override string ToString() => Name;
    }
}
