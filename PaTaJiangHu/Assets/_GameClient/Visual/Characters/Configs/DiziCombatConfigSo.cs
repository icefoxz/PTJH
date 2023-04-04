using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "战斗攻击配置", menuName = "战斗单位/攻击配置")]
public class DiziCombatConfigSo : ScriptableObject
{
    [SerializeField] private DiziCombatResponseCfgSo 反馈配置;

    private DiziCombatResponseCfgSo CombatResponseSo => 反馈配置;

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

    public void PlayOffendReturn(CharacterOperator performOp)
    {
        performOp.SetAnim(CharacterOperator.Anims.AttackReturn);//move return
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
        yield return op.OffendMove(targetPos);
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
    public class DiziCombatConfig
    {

    }
}