using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Views;

[CreateAssetMenu(fileName = "战斗攻击配置", menuName = "战斗单位/攻击配置")]
internal class DiziCombatConfigSo : ScriptableObject
{

    [SerializeField] private DiziCombatResponseCfgSo 反馈配置;
    [SerializeField] private DiziCombatConfig 弟子攻击配置;
    [SerializeField] private UiEffectField[] Ui特效配置;

    private DiziCombatResponseCfgSo CombatResponseSo => 反馈配置;
    private DiziCombatConfig CombatConfig => 弟子攻击配置;
    private UiEffectField[] UiEffectFields => Ui特效配置;

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
            case DiziBattle.Responses.Suffer:
                tarOp.SetAnim(CharacterOperator.Anims.Suffer);
                PlayUiEffects(performerId, performIndex, DiziBattle.Events.Response, rectTransform);
                break;
            case DiziBattle.Responses.Dodge:
                tarOp.SetAnim(CharacterOperator.Anims.Dodge);
                break;
            case DiziBattle.Responses.Defeat:
                tarOp.SetAnim(CharacterOperator.Anims.Defeat);
                PlayUiEffects(performerId, performIndex, DiziBattle.Events.Response, rectTransform);
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
        PlayUiEffects(info.Performer.InstanceId, performIndex, DiziBattle.Events.Perform, rectTransform);
    }

    /// <summary>
    /// 播放返回动画
    /// </summary>
    /// <param name="targetPoint"></param>
    /// <param name="performOp"></param>
    /// <returns></returns>
    public IEnumerator PlayOffendReturn(float targetPoint,CharacterOperator performOp)
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
    private void PlayUiEffects(int performerId,int performIndex ,DiziBattle.Events battleEvent, RectTransform tran)
    {
        foreach (var effect in UiEffectFields.Where(u => u.Event == battleEvent))
        {
            foreach (var view in effect.Effects)
            {
                var ui = Instantiate(view, tran.transform);
                ui.name = view.name;
                EffectView.Instance(performerId, performIndex, ui, tran, effect.Lasting);
            }
        }
    }

    //获取反馈
    private static DiziBattle.Responses GetResponseAction(CombatResponseInfo<DiziCombatUnit, DiziCombatInfo> response)
    {
        return response.IsDodged ? DiziBattle.Responses.Dodge
            : response.Target.Hp > 0 ? DiziBattle.Responses.Suffer
            : DiziBattle.Responses.Defeat;
    }
    /// <summary>
    /// 弟子战斗攻击配置
    /// </summary>
    [Serializable] private class DiziCombatConfig
    {
        [SerializeField] private BasicAnimConfig 战斗移动配置;
        [SerializeField] private BasicAnimConfig 战斗返回配置;
        internal BasicAnimConfig OffendMovingCurve => 战斗移动配置;
        internal BasicAnimConfig OffendReturnCurve => 战斗返回配置;
    }

    [Serializable] internal class BasicAnimConfig
    {
        [SerializeField] private AnimationCurve 曲线;
        [SerializeField] private float 耗时;
        public float Evaluate(float elapsedTime) => 曲线.Evaluate(elapsedTime);
        public float Duration => 耗时;
    }

    [Serializable]internal class UiEffectField
    {
        [SerializeField] private DiziBattle.Events 事件;
        [SerializeField] private EffectView[] Ui特效;
        [SerializeField] private float 持续秒 = 0.5f;
        public EffectView[] Effects => Ui特效;
        public DiziBattle.Events Event => 事件;
        public float Lasting => 持续秒;
    }
}