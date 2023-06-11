using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Models;
using Server.Configs.Battles;
using Server.Configs.Characters;
using Server.Configs.Items;
using Server.Configs.Skills;
using Server.Controllers;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Views;

public class TestBattle : MonoBehaviour
{
    [SerializeField] private CombatChar[] _combatChars;
    [SerializeField] private Config.GameAnimConfig _animConfig;
    [SerializeField] private ConfigureSo _configureSo;
    [SerializeField] private Game2DLand _game2DLand;
    [SerializeField] private CharacterUiSyncHandler _characterUiSyncHandler;
    [SerializeField] private CharacterOperator _testSceneObj;
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private Canvas _sceneCanvas;

    private static BattleCache _battleCache = new BattleCache();
    private ConfigureSo ConfigureSo => _configureSo;
    private CombatChar[] CombatChars => _combatChars;
    private DiziBattle Battle { get; set; }
    private DiziBattleAnimator BattleAnim { get; set; }

    private Game2DLand Game2DLand => _game2DLand;
    private CharacterUiSyncHandler CharacterUiSyncHandler => _characterUiSyncHandler;

    private ISceneObj TestSceneObj => _testSceneObj;

    private void Start()
    {
        _game2DLand.Init(_animConfig);
        CharacterUiSyncHandler.Init(_game2DLand, _sceneCanvas, _mainCamera);
        EffectView.OnInstance += OnEffectInstance;
    }
    private void OnEffectInstance(IEffectView view)
    {
        switch (view.name)
        {
            case "demo_game_charPopValue":
                new Demo_game_charPopValue(view);
                break;
            default: throw new ArgumentException($"找不到特效的控制类! view = {view.name}");
        }
    }
    public void InstanceUiToTestObj()
    {
        CharacterUiSyncHandler.AssignObjToUi(TestSceneObj);
    }

    public void InstanceBattle()
    {
        Battle = NewBattle(CombatChars.Select(c => c.GetCombatUnit()).ToArray());
        Debug.Log("<color=green>********战斗开始**********</color>");
    }

    public void InvokeRoundForLogOnly()
    {
        if (Battle.IsFinalized)
        {
            Debug.LogError("<color=red>战斗已经结束!</color>");
            return;
        }
        var info = ExecuteRound();
        Debug.Log("********战斗回合[" +
                  $"<color=yellow><b>{Battle.Rounds.Count}</b></color>]" +
                  "**********");
        foreach (var (unit, pfmInfo) in info.UnitInfoMap)
        {
            foreach (var pfm in pfmInfo)
            {
                Debug.Log($"<color=white>{unit.Name}进攻- {pfm}</color>");
                Debug.Log($"<color=#FFA07A>{pfm.Response.Target.Name}{pfm.Response}</color>");
                Debug.Log($"<color=yellow>{pfm.Performer}</color>\n<color=cyan>{pfm.Response.Target}</color>");
            }
        }
    }

    public void StartBattle()
    {
        var fighters = CombatChars.Select(c => new { op = c.Op, combat = c.GetCombatUnit() }).ToArray();
        Battle = NewBattle(fighters.Select(f => f.combat).ToArray());
        _battleCache.SetBattle(Battle);
        BattleAnim =
            new DiziBattleAnimator(_animConfig.DiziCombatVisualCfg,
                fighters.ToDictionary(c => c.combat.InstanceId, c => c.op),
                CharacterUiSyncHandler, this, transform);
        StartRound();
    }

    private DiziBattle NewBattle(DiziCombatUnit[] combatUnits) =>
        DiziBattle.Instance(ConfigureSo.Config.BattleCfg, combatUnits);

    public void StartRound()
    {
    
        if (Battle.IsFinalized)
        {
            Debug.LogError("战斗已经结束!");
            return;
        }

        var roundInfo = ExecuteRound();
        BattleAnim.PlayRound(roundInfo, () =>
        {
            if (Battle.IsFinalized)
                Debug.LogWarning($"战斗已经结束,{(Battle.IsPlayerWin ? "玩家胜利!" : "玩家战败")}");
        });
    }

    private DiziRoundInfo ExecuteRound() => Battle.ExecuteRound();

    [Serializable]
    private class CombatChar : ICombatSet,IDiziEquipment
    {
        [SerializeField] private CharacterOperator _op;
        [SerializeField] private bool 玩家;
        [SerializeField] private string 名字;
        [SerializeField] private int 血量;
        [SerializeField] private int 伤害;
        [SerializeField] private int 速度;
        [SerializeField] private int 内力;
        [SerializeField] private int 力量;
        [SerializeField] private int 敏捷;
        [SerializeField] private CombatSkill 武功;
        [SerializeField] private ForceSkill 内功;
        [SerializeField] private DodgeSkill 轻功;
        [SerializeField] private WeaponField 武器;
        [SerializeField] private ArmorField 防具;
        [SerializeField] private ShoesField 鞋子;
        [SerializeField] private DecorationField 装饰;
        public CharacterOperator Op => _op;

        public int InstanceId { get; private set; }
        public string Name => 名字;
        public int Hp => 血量;
        public int MaxHp => 血量;
        public int Damage => 伤害;
        public int Speed => 速度;
        public int TeamId => 玩家 ? 0 : 1;

        public string Guid { get; } = System.Guid.NewGuid().ToString();

        public int Mp => 内力;
        public int MaxMp => 内力;
        public int Strength => 力量;
        public int Agility => 敏捷;
        public ICombatSet CombatSet => this;
        private CombatSkill Com => 武功;
        private ForceSkill Force => 内功;
        private DodgeSkill Dodge => 轻功;

        public float GetHardRate(CombatArgs arg) => Com.GetHardRate(arg);
        public float GetHardDamageRatioAddOn(CombatArgs arg) => Com.GetHardDamageRatio(arg);
        public float GetCriticalRate(CombatArgs arg) => Force.GetCriticalRate(arg);
        public float GetCriticalDamageRatioAddOn(CombatArgs arg) => Force.GetCriticalMultiplier(arg);
        public float GetMpUses(CombatArgs arg) => Force.GetMpDamage(arg);
        public float GetMpDamageConvertRateAddOn(CombatArgs arg) => Force.GetMpDamageConvertRateAddOn;

        public float GetMpArmorRate(CombatArgs arg) => Force.GetMpCounteract(arg);
        public float GetMpArmorConvertRateAddOn(CombatArgs arg) => Force.GetMpArmorConvertRateAddOn;

        public float GetDodgeRate(CombatArgs arg) => Dodge.GetDodgeRate(arg);
        public IEnumerable<CombatBuff> GetSelfBuffs(DiziCombatUnit caster, int round, BuffManager<DiziCombatUnit> buffManager) =>
            Com.Buffs.Concat(Force.Buffs).Concat(Dodge.Buffs)
                .SelectMany(b => b.InstanceSelfBuffs(caster, round, buffManager));

        public IEnumerable<CombatBuff> GetTargetBuffs(DiziCombatUnit target, DiziCombatUnit caster, CombatArgs args,
            int round, BuffManager<DiziCombatUnit> buffManager) =>
            Com.Buffs.Concat(Force.Buffs).Concat(Dodge.Buffs)
                .SelectMany(b => b.InstanceTargetBuffs(target, caster, args, round, buffManager));

        public void SetInstanceId(int instanceId) => InstanceId = instanceId;

        public DiziCombatUnit GetCombatUnit() => new(string.Empty, TeamId, Name, Strength, Agility, Hp, Mp, this, this);

        #region Equipment
        public IWeapon Weapon => 武器;
        public IArmor Armor => 防具;
        public IShoes Shoes => 鞋子;
        public IDecoration Decoration => 装饰;
        private IEquipment[] Equipments => new IEquipment[] { Weapon, Armor, Shoes, Decoration };
        public float GetPropAddon(DiziProps prop)=>  Equipments.Sum(e => e.GetAddOn(prop));

        public ICombatSet GetCombatSet() => Equipments.Select(e => e.GetCombatSet()).Combine();

        private abstract class EuipmentBase : IEquipment
        {
            [SerializeField] private ColorGrade 品;
            [SerializeField] private int 韧性;
            [SerializeField] private EquipmentSoBase.DiziPropAddOn[] 加成;
            [SerializeField] private CombatAdvancePropField[] 高级属性;
            public abstract EquipKinds EquipKind { get; } 
            public ColorGrade Grade => 品;
            public int Quality => 韧性;
            public int Id { get; }
            public Sprite Icon { get; }
            public abstract string Name { get; }
            public string About { get; }
            public ItemType Type { get; } = ItemType.Equipment;
            private EquipmentSoBase.DiziPropAddOn[] AddOns => 加成;
            private CombatAdvancePropField[] AdvanceProps => 高级属性;

            public float GetAddOn(DiziProps prop)
            {
                var value = 0f;
                for (var i = 0; i < AddOns.Length; i++)
                {
                    var addOn = AddOns[i];
                    if (addOn.Prop == prop) value += addOn.AddOn;
                }
                return value;
            }
            public ICombatSet GetCombatSet() => AdvanceProps.Select(c => c.GetCombatSet()).Combine();
        }
        #endregion

        [Serializable]private class WeaponField : EuipmentBase, IWeapon
        {
            [SerializeField] private WeaponArmed 类型;
            public int Id { get; } = 0;
            public Sprite Icon { get; }
            public override string Name { get; } = "测试武器";
            public string About { get; }
            public override EquipKinds EquipKind { get; } = EquipKinds.Weapon;
            public WeaponArmed Armed => 类型;
        }
        [Serializable]private class ArmorField : EuipmentBase, IArmor
        {
            public int Id { get; }
            public Sprite Icon { get; }
            public override string Name { get; } = "测试防具";
            public string About { get; }
            public override EquipKinds EquipKind { get; } = EquipKinds.Armor;
        }
        [Serializable]private class ShoesField : EuipmentBase, IShoes
        {
            public int Id { get; }
            public Sprite Icon { get; }
            public override string Name { get; } = "测试鞋子";
            public string About { get; }
            public override EquipKinds EquipKind { get; } = EquipKinds.Shoes;
        }
        [Serializable]private class DecorationField : EuipmentBase, IDecoration
        {
            public int Id { get; }
            public Sprite Icon { get; }
            public override string Name { get; } = "测试饰品";
            public string About { get; }
            public override EquipKinds EquipKind { get; } = EquipKinds.Decoration;
        }
        
        [Serializable] private class CombatSkill
        {
            [SerializeField] private float 重击率;
            [Range(0, 3)] [SerializeField] private float 重击伤害倍数;
            [SerializeField] private EffectBuffSoBase[] _buffs;
            public EffectBuffSoBase[] Buffs => _buffs;
            public float GetHardRate(CombatArgs arg) => 重击率;
            public float GetHardDamageRatio(CombatArgs arg) => 重击伤害倍数;
        }
        [Serializable] private class ForceSkill
        {
            [FormerlySerializedAs("伤害内力")][SerializeField] private float 内力使用;
            [FormerlySerializedAs("抵消内力")][SerializeField] private float 内力护甲占比;
            [SerializeField] private float 会心率;
            [SerializeField] private float 会心倍率 = 3;
            [SerializeField] private float 内力伤害转化率加成;
            [SerializeField] private float 内力护甲消耗转化加成;
            [SerializeField] private EffectBuffSoBase[] _buffs;
            public EffectBuffSoBase[] Buffs => _buffs;
            public float GetMpDamageConvertRateAddOn => 内力伤害转化率加成;
            public float GetMpArmorConvertRateAddOn => 内力护甲消耗转化加成;

            public float GetCriticalRate(CombatArgs arg) => 会心率;
            public float GetMpDamage(CombatArgs arg) => 内力使用;
            public float GetMpCounteract(CombatArgs arg) => 内力护甲占比;
            public float GetCriticalMultiplier(CombatArgs arg) => 会心倍率;
        }
        [Serializable] private class DodgeSkill
        {
            [SerializeField] private float 闪避率;
            [SerializeField] private EffectBuffSoBase[] _buffs;
            public EffectBuffSoBase[] Buffs => _buffs;
            public float GetDodgeRate(CombatArgs arg) => 闪避率;
        }
    }

    private class Demo_game_charPopValue
    {
        private GameObject anim_pop { get; }
        private GameObject anim_blick { get; }
        private Text text_pop { get; }
        private Text text_blick { get; }

        private Vector3 BlinkDefaultPos { get; }

        public Demo_game_charPopValue(IEffectView v)
        {
            anim_pop = v.GetObject("anim_pop");
            anim_blick = v.GetObject("anim_blink");
            text_pop = v.GetObject<Text>("text_pop");
            text_blick = v.GetObject<Text>("text_blink");
            BlinkDefaultPos = anim_blick.transform.localPosition;
            v.OnPlay += OnPlay;
            v.OnReset += OnReset;
        }

        private void OnPlay((int performerId, int performIndex, RectTransform tran) arg)
        {
            var (performerId, performIndex, tran) = arg;
            var response = _battleCache.GetLastResponse(performerId, performIndex);
            var damage = response.FinalDamage;
            text_pop.text = damage.ToString();
            anim_pop.SetActive(true);
            //if(response.IsDodged)
            {
                var flipAlign = response.Target.TeamId == 0 ? -1 : 1;
                var responseText = GetResponseText(response);
                text_blick.text = responseText;
                anim_blick.transform.SetLocalX(anim_blick.transform.localPosition.x * flipAlign);
                anim_blick.SetActive(true);
            }
        }

        private string GetResponseText(CombatResponseInfo<DiziCombatUnit, DiziCombatInfo> response)
        {
            return response.Target.IsDead ? "绝杀" : response.IsDodge ? "闪避" : string.Empty;
        }

        private void OnReset()
        {
            anim_pop.SetActive(false);
            anim_blick.SetActive(false);
            text_pop.text = string.Empty;
            text_blick.text = string.Empty;
            anim_blick.transform.localPosition = BlinkDefaultPos;
        }
    }
}
