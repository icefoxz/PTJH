using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "战斗攻击配置", menuName = "战斗单位/攻击配置")]
internal class DiziCombatConfigSo : ScriptableObject
{
    [SerializeField] private DiziCombatResponseCfgSo 反馈配置;
    [SerializeField] private DiziCombatConfig 弟子攻击配置;

    private DiziCombatResponseCfgSo CombatResponseSo => 反馈配置;
    private DiziCombatConfig CombatConfig => 弟子攻击配置;

    public void PlayResponse(CombatResponseInfo<DiziCombatUnit> response, CharacterOperator tarOp)
    {
        var reAct = GetResponseAction(response);
        switch (reAct)
        {
            case DiziCombatResponseCfgSo.Responses.Suffer:
                tarOp.SetAnim(CharacterOperator.Anims.Suffer, () => tarOp.SetAnim(CharacterOperator.Anims.Idle));
                break;
            case DiziCombatResponseCfgSo.Responses.Dodge:
                tarOp.SetAnim(CharacterOperator.Anims.Dodge, () => tarOp.SetAnim(CharacterOperator.Anims.Idle));
                break;
            case DiziCombatResponseCfgSo.Responses.Defeat:
                tarOp.SetAnim(CharacterOperator.Anims.Defeat);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void PlayPerform(DiziCombatPerformInfo info, CharacterOperator performOp)
    {
        //暂时只有攻击
        performOp.SetAnim(CharacterOperator.Anims.Attack);//attack
    }

    public IEnumerator PlayOffendReturn(float targetPoint,CharacterOperator performOp)
    {
        performOp.SetAnim(CharacterOperator.Anims.AttackReturn);//move return
        yield return performOp.OffendReturnMove(targetPoint, CombatConfig.OffendReturnCurve);
    }

    public IEnumerator PlayResponseEffects(CombatResponseInfo<DiziCombatUnit> response, CharacterOperator tarOp)
    {
        var reAct = GetResponseAction(response);
        var effects = CombatResponseSo.GetResponseEffect(reAct);

        var list = new List<(float, GameObject)>();
        foreach (var effect in effects) list.Add((effect.LastingSecs, effect.Invoke(tarOp.gameObject)));
        var uSec = 0.1f;
        var secsElapse = 0f;
        while (list.Count > 0)
        {
            yield return new WaitForSeconds(uSec);//每0.1秒更新
            secsElapse += uSec;
            foreach (var arg in list.Where(e => e.Item1 <= secsElapse).ToArray())
            {
                var (_, go) = arg;
                list.Remove(arg);
                Destroy(go);
            }
        }
    }

    public IEnumerator PlayOffendMove(DiziCombatPerformInfo info, CharacterOperator op, float targetPos)
    {
        op.SetAnim(CharacterOperator.Anims.MoveStep);
        yield return op.OffendMove(targetPos, CombatConfig.OffendMovingCurve);
    }
    //获取反馈
    private static DiziCombatResponseCfgSo.Responses GetResponseAction(CombatResponseInfo<DiziCombatUnit> response)
    {
        return response.IsDodged ? DiziCombatResponseCfgSo.Responses.Dodge
            : response.Target.Hp > 0 ? DiziCombatResponseCfgSo.Responses.Suffer
            : DiziCombatResponseCfgSo.Responses.Defeat;
    }
    /// <summary>
    /// 弟子战斗攻击配置
    /// </summary>
    [Serializable]
    private class DiziCombatConfig
    {
        [SerializeField] private BasicAnimConfig 战斗移动配置;
        [SerializeField] private BasicAnimConfig 战斗返回配置;
        internal BasicAnimConfig OffendMovingCurve => 战斗移动配置;
        internal BasicAnimConfig OffendReturnCurve => 战斗返回配置;
    }
    [Serializable]
    internal class BasicAnimConfig
    {
        [SerializeField] private AnimationCurve 曲线;
        [SerializeField] private float 耗时;
        public float Evaluate(float elapsedTime) => 曲线.Evaluate(elapsedTime);
        public float Duration => 耗时;
    }
}