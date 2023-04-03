using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TestBattle : MonoBehaviour
{
    [SerializeField]private CombatChar[] _combatChars;
    [SerializeField]private CharAnimSo _charAnimSo;
    private CombatChar[] CombatChars => _combatChars;
    private DiziBattle Battle { get; set; }
    private Dictionary<int,CharacterOperator> OpMap { get; set; }
    public void StartBattle()
    {
        var fighters = CombatChars.Select(c => new { op = c.Op, combat = c.GetCombatUnit() }).ToArray();
        Battle = DiziBattle.Instance(fighters.Select(f => f.combat).ToArray());
        OpMap = fighters.ToDictionary(c => c.combat.InstanceId, c => c.op);
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
        PlayRound(roundInfo, () =>
        {
            if(Battle.IsFinalized)
                Debug.LogWarning($"战斗已经结束,{(Battle.IsPlayerWin?"玩家胜利!":"玩家战败")}");
        });
    }

    private void PlayRound(RoundInfo<DiziCombatUnit, DiziCombatPerformInfo> roundInfo,Action callback)
    {
        StartCoroutine(PlayRoundCo(roundInfo, callback));
    }

    private IEnumerator PlayRoundCo(RoundInfo<DiziCombatUnit, DiziCombatPerformInfo> roundInfo, Action callback)
    {
        foreach (var (pfm, performInfos) in roundInfo.UnitInfoMap)
        {
            var performOp = OpMap[pfm.InstanceId];
            var performPos = GetLocationPoint(performOp);
            foreach (var info in performInfos)
            {
                var response = info.Reponse;
                var targetOp = OpMap[response.Target.InstanceId];
                var targetPos = GetLocationPoint(targetOp);
                performOp.SetAnim(CharacterOperator.Anims.MoveStep);
                yield return performOp.OffendMove(targetPos);
                var reAct = response.IsDodged ? CharAnimSo.Responses.Dodge
                    : info.Reponse.Target.Hp > 0 ? CharAnimSo.Responses.Suffer
                    : CharAnimSo.Responses.Defeat;
                var effects = _charAnimSo.GetResponseEffect(reAct);
                StartCoroutine(PlayEffects(effects, targetOp.gameObject));
                performOp.SetAnim(CharacterOperator.Anims.Attack);
                switch (reAct)
                {
                    case CharAnimSo.Responses.Suffer:
                        targetOp.SetAnim(CharacterOperator.Anims.Suffer, () =>
                        {
                            SetOpPos(targetOp.transform, targetPos);
                            targetOp.SetAnim(CharacterOperator.Anims.Idle);
                        });
                        break;
                    case CharAnimSo.Responses.Dodge:
                        targetOp.SetAnim(CharacterOperator.Anims.Dodge, () =>
                        {
                            SetOpPos(targetOp.transform, targetPos);
                            targetOp.SetAnim(CharacterOperator.Anims.Idle);
                        });
                        break;
                    case CharAnimSo.Responses.Defeat:
                        targetOp.SetAnim(CharacterOperator.Anims.Defeat, () => SetOpPos(targetOp.transform, targetPos));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                yield return new WaitForSeconds(0.2f);
                performOp.SetAnim(CharacterOperator.Anims.AttackReturn);
                yield return performOp.OffendReturnMove(performPos);
            }
        }

        callback?.Invoke();
        yield return null;

        void SetOpPos(Transform tran, float pos) => tran.transform.SetX(pos);
    }


    private IEnumerator PlayEffects(ICombatEffect[] effects,GameObject parent)
    {
        var list = new List<(float,GameObject)>();
        foreach (var effect in effects) list.Add((effect.LastingSecs, effect.Invoke(parent)));
        var uSec = 0.1f;
        var time = 0f;
        while (list.Count > 0)
        {
            yield return new WaitForSeconds(uSec);
            time += uSec;
            foreach (var arg in list.Where(e => e.Item1 <= time).ToArray())
            {
                var (_, go) = arg;
                list.Remove(arg);
                Destroy(go);
            }
        }
    }

    private float GetLocationPoint(CharacterOperator op) => op.transform.localPosition.x;

    [Serializable] private class CombatChar : ICombatUnit
    {
        [SerializeField] private CharacterOperator _op;
        [SerializeField] private bool 玩家;
        [SerializeField] private string 名字;
        [SerializeField] private int 血量;
        [SerializeField] private int 伤害;
        [SerializeField] private int 速度;
        public CharacterOperator Op => _op;

        public int InstanceId { get; }
        public string Name => 名字;
        public int Hp => 血量;
        public int MaxHp => 血量;
        public int Damage => 伤害;
        public int Speed => 速度;
        public int TeamId => 玩家 ? 0 : 1;
        public DiziCombatUnit GetCombatUnit() => new(this);
    }
}
