using System;
using System.Collections.Generic;
using System.Linq;
using Models;
using Server.Configs.Battles;
using Server.Configs.Characters;
using UnityEngine;
using UnityEngine.UI;
using Views;

public class TestBattle : MonoBehaviour
{
    [SerializeField] private CombatChar[] _combatChars;
    [SerializeField] private Config.GameAnimConfig _animConfig;
    [SerializeField] private Game2DLand _game2DLand;
    [SerializeField] private CharacterUiSyncHandler _characterUiSyncHandler;
    [SerializeField] private CharacterOperator _testSceneObj;
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private Canvas _sceneCanvas;

    private static BattleCache _battleCache = new BattleCache();

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

    public void StartBattle()
    {
        var fighters = CombatChars.Select(c => new { op = c.Op, combat = c.GetCombatUnit() }).ToArray();
        Battle = DiziBattle.Instance(fighters.Select(f => f.combat).ToArray());
        _battleCache.SetBattle(Battle);
        BattleAnim =
            new DiziBattleAnimator(_animConfig.DiziCombatCfg,
                fighters.ToDictionary(c => c.combat.InstanceId, c => c.op),
                CharacterUiSyncHandler, this, transform);
        StartRound();
    }
    public void StartRound()
    {
    
        if (Battle.IsFinalized)
        {
            Debug.LogError("战斗已经结束!");
            return;
        }

        var roundInfo = Battle.ExecuteRound();
        BattleAnim.PlayRound(roundInfo, () =>
        {
            if (Battle.IsFinalized)
                Debug.LogWarning($"战斗已经结束,{(Battle.IsPlayerWin ? "玩家胜利!" : "玩家战败")}");
        });
    }

    [Serializable]
    private class CombatChar : IDiziCombatUnit, ICombatSet, IDiziEquipment
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
        public IDiziEquipment Equipment => this;
        private CombatSkill Com => 武功;
        private ForceSkill Force => 内功;
        private DodgeSkill Dodge => 轻功;
        public float GetHardRate(CombatArgs arg) => Com.GetHardRate(arg);
        public float GetHardDamageRatio(CombatArgs arg) => Com.GetHardDamageRatio(arg);
        public float GetCriticalRate(CombatArgs arg) => Force.GetCriticalRate(arg);
        public float GetCriticalDamageRatio(CombatArgs arg) => Force.GetCriticalMultiplier(arg);
        public float GetMpDamage(CombatArgs arg) => Force.GetMpDamage(arg);
        public float GetMpCounteract(CombatArgs arg) => Force.GetMpCounteract(arg);
        public float GetDodgeRate(CombatArgs arg) => Dodge.GetDodgeRate(arg);

        public void SetInstanceId(int instanceId)
        {
            InstanceId = instanceId;
        }

        public DiziCombatUnit GetCombatUnit() => new(this);

        [Serializable]
        private class CombatSkill
        {
            [SerializeField] private float 重击率;
            [Range(0, 3)] [SerializeField] private float 重击伤害倍数;
            public float GetHardRate(CombatArgs arg) => 重击率;
            public float GetHardDamageRatio(CombatArgs arg) => 重击伤害倍数;
        }

        [Serializable]
        private class ForceSkill
        {
            [SerializeField] private float 伤害内力;
            [SerializeField] private float 抵消内力;
            [SerializeField] private float 会心率;
            [SerializeField] private float 会心倍率 = 3;
            public float GetCriticalRate(CombatArgs arg) => 会心率;
            public float GetMpDamage(CombatArgs arg) => 伤害内力;
            public float GetMpCounteract(CombatArgs arg) => 抵消内力;
            public float GetCriticalMultiplier(CombatArgs arg) => 会心倍率;
        }

        [Serializable]
        private class DodgeSkill
        {
            [SerializeField] private float 闪避率;
            public float GetDodgeRate(CombatArgs arg) => 闪避率;
        }

        //暂时不实现装备
        public IWeapon Weapon { get; }
        public IArmor Armor { get; }
        public IShoes Shoes { get; }
        public IDecoration Decoration { get; }
        public IEnumerable<IEquipment> AllEquipments { get; }
        public int GetPropAddon(DiziProps prop) => 0;
        public ICombatProps GetCombatProps() => new DiziCombatProps();
        public IDiziCombatUnit CombatDisarm(int teamId, IEquipment equipment) => new DiziCombatUnit(this);
        ICombatSet IDiziEquipment.GetCombatSet() => Server.Configs.Battles.CombatSet.Empty;

        private class DiziCombatProps : ICombatProps
        {
            public float StrAddon { get; } = 0;
            public float AgiAddon { get; } = 0;
            public float HpAddon { get; } = 0;
            public float MpAddon { get; } = 0;
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
            return response.Target.IsDead ? "绝杀" : response.IsDodged ? "闪避" : string.Empty;
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
