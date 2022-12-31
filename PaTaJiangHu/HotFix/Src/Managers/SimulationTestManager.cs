using System;
using HotFix_Project.Views.Bases;
using Server.Configs._script;
using Server.Configs._script.Adventures;
using Systems.Messaging;
using UnityEngine.UI;
using Views;

namespace HotFix_Project.Managers;

internal class SimulationTestManager
{
    private IBattleSimController Controller { get; set; }

    private BattleSimulationView BattleSimView { get; set; }
    public void Init()
    {
        Controller = TestCaller.Instance.InstanceBattleSimController();
        InitUi();
    }

    private void InitUi()
    {
        Game.UiBuilder.Build("view_testSimulation",
            v => BattleSimView = new BattleSimulationView(v, Controller));
        Game.MessagingManager.RegEvent(EventString.Test_SimulationUpdateModel,
            bag => BattleSimView.UpdateModel(bag.Get<BattleSimController.TestModel>(0)));
        Game.MessagingManager.RegEvent(EventString.Test_SimulationOutcome,
            bag => BattleSimView.UpdateMessage(bag.Get<BattleSimController.Outcome>(0)));
        Game.MessagingManager.RegEvent(EventString.Test_SimulationStart, bag =>
        {
            BattleSimView.Display(true);
        });
    }

    private class BattleSimulationView : UiBase
    {
        private BattleUnitView PlayerUi { get; }
        private BattleUnitView EnemyUi { get; }
        private ScrollRect Scroll_simulation { get; }
        private Button Btn_simulate { get; }
        private ListViewUi<MessageView> MsgListView { get; }

        private IBattleSimController Controller { get; }

        public BattleSimulationView(IView v,IBattleSimController controller) : base(v.GameObject, false)
        {
            Controller = controller;
            PlayerUi = new BattleUnitView(v.GetObject<View>("view_player"),
                o => OnLevelAction(o.skill, o.isAdd, true),
                o => OnGradeAction(o.skill, o.isAdd, true),
                o => OnPropAction(o.prop, o.value, true));
            EnemyUi = new BattleUnitView(v.GetObject<View>("view_enemy"),
                o => OnLevelAction(o.skill, o.isAdd, false),
                o => OnGradeAction(o.skill, o.isAdd, false),
                o => OnPropAction(o.prop, o.value, false));
            Scroll_simulation = v.GetObject<ScrollRect>("scroll_simulation");
            Btn_simulate = v.GetObject<Button>("btn_simulate");
            MsgListView = new ListViewUi<MessageView>(v.GetObject<View>("prefab_message"),
                Scroll_simulation.content.gameObject);

            Btn_simulate.OnClickAdd(controller.SimulateResult);
        }

        public void UpdateMessage(BattleSimController.Outcome outcome)
        {
            MsgListView.ClearList(u => u.Destroy());
            for (var i = 0; i < outcome.Rounds.Length; i++)
            {
                var round = outcome.Rounds[i];
                var ui = InstanceUi();
                var playerText = $"回合({i + 1}): 玩家攻({outcome.PlayerOffend})->对手血 = {round.EnemyDefend}!";
                var enemyText = $"****对手攻({outcome.EnemyOffend})->玩家血 = {round.PlayerDefend}!";
                ui.SetMessage(playerText, enemyText);
            }
            var sumText = $"{(outcome.IsPlayerWin ? "玩家" : "对手")}胜利!";
            var hp = outcome.IsPlayerWin ? outcome.PlayerDefend : outcome.EnemyDefend;
            var percent = 100f * outcome.RemainingHp / hp;
            var sumText2 = $"剩余血 = {outcome.RemainingHp}, = {percent:F1}%, 体力伤害 = {outcome.Result}";
            var sum = InstanceUi();
            sum.SetMessage(sumText, sumText2);

            MessageView InstanceUi()
            {
                var ui = MsgListView.Instance(v => new MessageView(v));
                return ui;
            }
        }

        public void UpdateModel(BattleSimController.TestModel model)
        {
            PlayerUi.Update(model.Player);
            EnemyUi.Update(model.Enemy);
        }
        private void OnPropAction(BattleSimController.TestModel.Props prop, int value, bool isPlayer) =>
            Controller.SetValue(isPlayer, prop, value);
        private void OnLevelAction(BattleSimController.PropCon.Skills skill, bool isAdd, bool isPlayer)
        {
            switch (skill)
            {
                case BattleSimController.PropCon.Skills.Force:
                    if (isAdd) Controller.ForceLevelAdd(isPlayer);else Controller.ForceLevelSub(isPlayer);
                    break;
                case BattleSimController.PropCon.Skills.Combat:
                    if (isAdd) Controller.CombatLevelAdd(isPlayer);else Controller.CombatLevelSub(isPlayer);
                    break;
                case BattleSimController.PropCon.Skills.Dodge:
                    if (isAdd) Controller.DodgeLevelAdd(isPlayer);else Controller.DodgeLevelSub(isPlayer);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(skill), skill, null);
            }
        }
        private void OnGradeAction(BattleSimController.PropCon.Skills skill, bool isAdd, bool isPlayer)
        {
            switch (skill)
            {
                case BattleSimController.PropCon.Skills.Force:
                    if (isAdd) Controller.ForceGradeAdd(isPlayer);else Controller.ForceGradeSub(isPlayer);
                    break;
                case BattleSimController.PropCon.Skills.Combat:
                    if (isAdd) Controller.CombatGradeAdd(isPlayer);else Controller.CombatGradeSub(isPlayer);
                    break;
                case BattleSimController.PropCon.Skills.Dodge:
                    if (isAdd) Controller.DodgeGradeAdd(isPlayer);else Controller.DodgeGradeSub(isPlayer);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(skill), skill, null);
            }
        }
        private class BattleUnitView : UiBase
        {
            private SkillView Combat { get; }
            private SkillView Force { get; }
            private SkillView Dodge { get; }
            private InputField Input_strength { get; }
            private InputField Input_agility { get; }
            private InputField Input_weapon { get; }
            private InputField Input_armor { get; }
            private Text Text_power { get; }
            private Text Text_armorPower { get; }
            private Text Text_strengthPower { get; }
            private Text Text_agilityPower { get; }
            private Text Text_weaponPower { get; }
            private Text Text_offend { get; }
            private Text Text_defend { get; }

            public BattleUnitView(IView v
                , Action<(BattleSimController.PropCon.Skills skill, bool isAdd)> onLevelAction
                , Action<(BattleSimController.PropCon.Skills skill, bool isAdd)> onGradeAction
                , Action<(BattleSimController.TestModel.Props prop, int value)> onPropAction
                ) : base(v.GameObject, true)
            {
                Combat = new SkillView(v.GetObject<View>("view_combat"),
                    isAdd => onGradeAction((BattleSimController.PropCon.Skills.Combat,isAdd)),
                    isAdd => onLevelAction((BattleSimController.PropCon.Skills.Combat,isAdd))
                    );
                Force = new SkillView(v.GetObject<View>("view_force"),
                    isAdd => onGradeAction((BattleSimController.PropCon.Skills.Force, isAdd)),
                    isAdd => onLevelAction((BattleSimController.PropCon.Skills.Force, isAdd)));
                Dodge = new SkillView(v.GetObject<View>("view_dodge"),
                    isAdd => onGradeAction((BattleSimController.PropCon.Skills.Dodge, isAdd)),
                    isAdd => onLevelAction((BattleSimController.PropCon.Skills.Dodge, isAdd)));
                Input_strength = v.GetObject<InputField>("input_strength");
                Input_strength.onValueChanged.AddAction(text => onPropAction((BattleSimController.TestModel.Props.Strength, InputValueChange(text))));
                Input_agility = v.GetObject<InputField>("input_agility");
                Input_agility.onValueChanged.AddAction(text => onPropAction((BattleSimController.TestModel.Props.Agility, InputValueChange(text))));
                Input_weapon = v.GetObject<InputField>("input_weapon");
                Input_weapon.onValueChanged.AddAction(text => onPropAction((BattleSimController.TestModel.Props.Weapon, InputValueChange(text))));
                Input_armor = v.GetObject<InputField>("input_armor");
                Input_armor.onValueChanged.AddAction(text => onPropAction((BattleSimController.TestModel.Props.Armor, InputValueChange(text))));
                Text_power = v.GetObject<Text>("text_power");
                Text_armorPower = v.GetObject<Text>("text_armorPower");
                Text_weaponPower = v.GetObject<Text>("text_weaponPower");
                Text_agilityPower = v.GetObject<Text>("text_agilityPower");
                Text_strengthPower = v.GetObject<Text>("text_strengthPower");
                Text_offend = v.GetObject<Text>("text_offend");
                Text_defend = v.GetObject<Text>("text_defend");
            }

            private int InputValueChange(string text) => string.IsNullOrEmpty(text) ? 0 : int.TryParse(text, out var result) ? result : 0;

            public void Update(BattleSimController.PropCon prop)
            {
                Combat.Update(prop.Combat);
                Force.Update(prop.Force);
                Dodge.Update(prop.Dodge);
                var power = prop.Offend + prop.Defend;
                Text_power.text = power.ToString();
                Text_armorPower.text = prop.ArmorPower.ToString("F1");
                Text_strengthPower.text = prop.StrengthPower.ToString("F1");
                Text_agilityPower.text = prop.AgilityPower.ToString("F1");
                Text_weaponPower.text = prop.WeaponPower.ToString("F1");
                Text_offend.text = prop.Offend.ToString();
                Text_defend.text = prop.Defend.ToString();
            }

            private class SkillView : UiBase
            {
                private enum Grade
                {
                    E,
                    D,
                    C,
                    B,
                    A,
                    S,
                }
                private Text Text_power { get; }
                private StepView GradeUi { get; }
                private StepView LevelUi { get; }

                public SkillView(IView v, Action<bool> onGradeAction, Action<bool> onLevelAction) : base(v.GameObject,
                    true)
                {
                    Text_power = v.GetObject<Text>("text_power");
                    GradeUi = new StepView(v.GetObject<View>("view_stepGrade"), () => onGradeAction(true),
                        () => onGradeAction(false));
                    LevelUi = new StepView(v.GetObject<View>("view_stepLevel"), () => onLevelAction(true),
                        () => onLevelAction(false));
                }

                public void Update(BattleSimController.PropCon.Skill skill)
                {
                    LevelUi.SetValue(skill.Level.ToString());
                    GradeUi.SetValue(((Grade)skill.Grade).ToString());
                    Text_power.text = skill.Power.ToString("F1");
                }

                private class StepView : UiBase
                {
                    private Text Text_value { get; }
                    private Button Btn_add { get; }
                    private Button Btn_sub { get; }

                    public StepView(IView v, Action onAddAction, Action onSubAction) : base(v.GameObject, true)
                    {
                        Text_value = v.GetObject<Text>("text_value");
                        Btn_add = v.GetObject<Button>("btn_add");
                        Btn_sub = v.GetObject<Button>("btn_sub");
                        Btn_add.OnClickAdd(onAddAction);
                        Btn_sub.OnClickAdd(onSubAction);
                    }

                    public void SetValue(string text) => Text_value.text = text;
                }

            }

        }
        private class MessageView : UiBase
        {
            private Text Text_playerMsg { get; }
            private Text Text_enemyMsg { get; }
            public MessageView(IView v) : base(v.GameObject, true)
            {
                Text_enemyMsg = v.GetObject<Text>("text_enemyMsg");
                Text_playerMsg = v.GetObject<Text>("text_playerMsg");
            }

            public void SetMessage(string player, string enemy)
            {
                Text_enemyMsg.text = enemy;
                Text_playerMsg.text = player;
            }
        }
    }
}