using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Views;
using Object = UnityEngine.Object;

/// <summary>
/// 弟子战斗演示器
/// </summary>
internal class DiziCombatAnimator
{
    /// <summary>
    /// 战斗事件, 用于定义战斗配置,
    /// </summary>
    public enum Events
    {
        Perform,
        Response,
        RoundEnd,
        BattleEnd,
    }

    /// <summary>
    /// 战斗反馈, 用于定义战斗配置
    /// </summary>
    public enum Responses
    {
        Suffer,
        Dodge,
        Defeat
    }

    private DiziCombatConfigSo CombatConfig { get; }
    private DiziCombatResponseCfgSo CombatResponseSo => CombatConfig.CombatResponseSo;
    private EffectViewsPool EffectViewsPool { get; } = new EffectViewsPool();
    private Transform PoolTransform { get; }

    public DiziCombatAnimator(DiziCombatConfigSo combatConfig, Transform poolTransform)
    {
        CombatConfig = combatConfig;
        PoolTransform = poolTransform;
    }

    /// <summary>
    /// 播放反馈动画
    /// </summary>
    /// <param name="performerId"></param>
    /// <param name="performIndex"></param>
    /// <param name="response"></param>
    /// <param name="tarOp"></param>
    /// <param name="rectTransform"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void PlayResponseAnim(int performerId, int performIndex,
        CombatResponseInfo<DiziCombatUnit, DiziCombatInfo> response, CharacterOperator tarOp,
        RectTransform rectTransform)
    {
        var reAct = GetResponseAction(response);
        switch (reAct)
        {
            case DiziCombatAnimator.Responses.Suffer:
                tarOp.SetAnim(CharacterOperator.Anims.Suffer);
                PlayUiEffects(performerId, performIndex, DiziCombatAnimator.Events.Response, rectTransform);
                break;
            case DiziCombatAnimator.Responses.Dodge:
                tarOp.SetAnim(CharacterOperator.Anims.Dodge);
                break;
            case DiziCombatAnimator.Responses.Defeat:
                tarOp.SetAnim(CharacterOperator.Anims.Defeat);
                PlayUiEffects(performerId, performIndex, DiziCombatAnimator.Events.Response, rectTransform);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    /// <summary>
    /// 播放进攻动画
    /// </summary>
    /// <param name="performIndex"></param>
    /// <param name="info"></param>
    /// <param name="performOp"></param>
    /// <param name="rectTransform"></param>
    public void PlayPerformAnim(int performIndex, DiziCombatPerformInfo info, CharacterOperator performOp,
        RectTransform rectTransform)
    {
        performOp.SetAnim(CharacterOperator.Anims.Attack); //attack
        PlayUiEffects(info.Performer.InstanceId, performIndex, DiziCombatAnimator.Events.Perform, rectTransform);
    }

    /// <summary>
    /// 播放返回动画
    /// </summary>
    /// <param name="targetPoint"></param>
    /// <param name="performOp"></param>
    /// <returns></returns>
    public IEnumerator PlayOffendReturn(float targetPoint, CharacterOperator performOp)
    {
        performOp.SetAnim(CharacterOperator.Anims.AttackReturn);//move return
        yield return performOp.OffendReturnMove(targetPoint, CombatConfig.OffendReturnCurve);
    }
    /// <summary>
    /// 播放2d效果
    /// </summary>
    /// <param name="response"></param>
    /// <param name="tarOp"></param>
    /// <returns></returns>
    public IEnumerator PlayResponse2DEffects(CombatResponseInfo<DiziCombatUnit, DiziCombatInfo> response, CharacterOperator tarOp)
    {
        var reAct = GetResponseAction(response);
        var effects = CombatResponseSo.GetResponseEffect(reAct);

        var list = new List<(float, GameObject)>();
        foreach (var effect in effects) list.Add((effect.LastingSecs, effect.Invoke(tarOp.transform)));
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
                Object.Destroy(go);
            }
        }
    }
    /// <summary>
    /// 播放进攻后返回动作(移动)
    /// </summary>
    /// <param name="info"></param>
    /// <param name="op"></param>
    /// <param name="targetPos"></param>
    /// <returns></returns>
    public IEnumerator PlayOffendMove(DiziCombatPerformInfo info, CharacterOperator op, float targetPos)
    {
        op.SetAnim(CharacterOperator.Anims.MoveStep);
        yield return op.OffendMove(targetPos, CombatConfig.OffendMovingCurve);
    }

    /// <summary>
    /// 播放Ui类的效果,(如:伤害数值,或是事件描述之类)
    /// </summary>
    /// <param name="performerId"></param>
    /// <param name="performIndex"></param>
    /// <param name="battleEvent"></param>
    /// <param name="tran"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void PlayUiEffects(int performerId, int performIndex, DiziCombatAnimator.Events battleEvent, RectTransform tran)
    {
        foreach (var effect in CombatConfig.UiEffectFields.Where(u => u.Event == battleEvent))
        {
            foreach (var view in effect.Effects)
            {
                var ui = EffectViewsPool.GetEffectView(view.name, () => InstanceEffectView(view,tran));
                ui.transform.SetParent(tran);
                ui.ResetPos();
                ui.PlayEffect(performerId, performIndex, tran, effect.Lasting);
            }
        }
    }

    private EffectView InstanceEffectView(EffectView view,RectTransform tran)
    {
        var e = Object.Instantiate(view, tran.transform);
        e.name = view.name;
        e.OnReset += ResetEffectView;
        EffectView.Init(e);
        return e;

        void ResetEffectView()
        {
            e.transform.SetParent(PoolTransform);
            EffectViewsPool.ReturnEffectView(e);
        }
    }


    //获取反馈
    private static DiziCombatAnimator.Responses GetResponseAction(CombatResponseInfo<DiziCombatUnit, DiziCombatInfo> response)
    {
        return response.IsDodged ? DiziCombatAnimator.Responses.Dodge
            : response.Target.Hp > 0 ? DiziCombatAnimator.Responses.Suffer
            : DiziCombatAnimator.Responses.Defeat;
    }
}

public class EffectViewsPool
{
    private Dictionary<string, Stack<EffectView>> effectPools = new Dictionary<string, Stack<EffectView>>();

    public EffectView GetEffectView(string type,Func<EffectView> instanceFunc)
    {
        if (!effectPools.ContainsKey(type)) 
            effectPools.Add(type, new Stack<EffectView>());

        var effectView = effectPools[type].Count > 0 
            ? effectPools[type].Pop() 
            : instanceFunc();
        return effectView;
    }

    public void ReturnEffectView(EffectView effectView)
    {
        if (effectView == null) return;

        var type = effectView.name;
        if (!effectPools.ContainsKey(type)) 
            effectPools.Add(type, new Stack<EffectView>());

        effectPools[type].Push(effectView);
    }
}