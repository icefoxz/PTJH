using System;
using System.Linq;
using UnityEngine;

public class TestBattle : MonoBehaviour
{
    [SerializeField] private CombatChar[] _combatChars;
    [SerializeField] private DiziCombatConfigSo _combatCfg;
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
        _game2DLand.Init();
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
            new DiziBattleAnimator(_combatCfg, fighters.ToDictionary(c => c.combat.InstanceId, c => c.op),
                CharacterUiSyncHandler, this);
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
    private class CombatChar : ICombatUnit
    {
        [SerializeField] private CharacterOperator _op;
        [SerializeField] private bool 玩家;
        [SerializeField] private string 名字;
        [SerializeField] private int 血量;
        [SerializeField] private int 伤害;
        [SerializeField] private int 速度;
        public CharacterOperator Op => _op;

        public int InstanceId { get; private set; }
        public string Name => 名字;
        public int Hp => 血量;
        public int MaxHp => 血量;
        public int Damage => 伤害;
        public int Speed => 速度;
        public int TeamId => 玩家 ? 0 : 1;
        public void SetInstanceId(int instanceId)
        {
            InstanceId = instanceId;
        }

        public DiziCombatUnit GetCombatUnit() => new(this);
    }
}
