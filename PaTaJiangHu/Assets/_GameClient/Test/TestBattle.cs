using System;
using System.Linq;
using UnityEngine;

public class TestBattle : MonoBehaviour
{
    [SerializeField] private CombatChar[] _combatChars;
    [SerializeField] private Config.GameAnimConfig _animConfig;
    [SerializeField] private Game2DLand _game2DLand;
    [SerializeField] private CharacterUiSyncHandler _characterUiSyncHandler;
    [SerializeField] private CharacterOperator _testSceneObj;
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private Canvas _sceneCanvas;

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
    }

    public void InstanceUiToTestObj()
    {
        CharacterUiSyncHandler.AssignObjToUi(TestSceneObj);
    }

    public void StartBattle()
    {
        var fighters = CombatChars.Select(c => new { op = c.Op, combat = c.GetCombatUnit() }).ToArray();
        Battle = DiziBattle.Instance(fighters.Select(f => f.combat).ToArray());
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
    private class CombatChar : IDiziCombatUnit, IBattle
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
        public IBattle Battle => this;
        public ICombat Combat => 武功;
        public IForce Force => 内功;
        public IDodge Dodge => 轻功;
        public float GetHardRate(CombatArgs arg)=> Combat.GetHardRate(arg);
        public float GetHardDamageRatio(CombatArgs arg)=> Combat.GetHardDamageRatio(arg);
        public float GetCriticalRate(CombatArgs arg)=> Force.GetCriticalRate(arg);
        public float GetMpDamage(CombatArgs arg)=> Force.GetMpDamage(arg);
        public float GetMpCounteract(CombatArgs arg)=> Force.GetMpCounteract(arg);
        public float GetDodgeRate(CombatArgs arg)=> Dodge.GetDodgeRate(arg);
        public void SetInstanceId(int instanceId)
        {
            InstanceId = instanceId;
        }

        public DiziCombatUnit GetCombatUnit() => new(this);

        [Serializable]
        private class CombatSkill : ICombat
        {
            [SerializeField] private float 重击率;
            [Range(0, 3)] [SerializeField] private float 重击伤害倍数;
            public float GetHardRate(CombatArgs arg) => 重击率;
            public float GetHardDamageRatio(CombatArgs arg) => 重击伤害倍数;
        }

        [Serializable]
        private class ForceSkill : IForce
        {
            [SerializeField] private float 伤害内力;
            [SerializeField] private float 抵消内力;
            [SerializeField] private float 会心率;
            public float GetCriticalRate(CombatArgs arg) => 会心率;
            public float GetMpDamage(CombatArgs arg) => 伤害内力;
            public float GetMpCounteract(CombatArgs arg) => 抵消内力;
        }

        [Serializable]
        private class DodgeSkill : IDodge
        {
            [SerializeField] private float 闪避率;
            public float GetDodgeRate(CombatArgs arg) => 闪避率;
        }
    }
}
