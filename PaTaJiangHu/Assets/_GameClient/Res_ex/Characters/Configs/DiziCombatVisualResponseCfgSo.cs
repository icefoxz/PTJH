using System;
using System.Linq;
using UnityEngine;

/// <summary>
/// 战斗效果接口,主要用于2d游戏物件,Ui不适用
/// </summary>
public interface ICombat2DEffect
{
    /// <summary>
    /// 战斗效果停留的时长,(常规处理会自动销毁)
    /// </summary>
    float LastingSecs { get; }
    /// <summary>
    /// 战斗效果, 当战斗事件触发的时候, 会调用这个方法, 生成一个效果, 并返回这个效果的GameObject
    /// </summary>
    /// <param name="parent"></param>
    /// <returns></returns>
    GameObject Invoke(Transform parent);
}

[CreateAssetMenu(fileName = "战斗演示反馈配置", menuName = "战斗/单位/战斗反馈")]
internal class DiziCombatVisualResponseCfgSo : ScriptableObject
{
    [SerializeField] private BasicEffect[] _suffer;
    [SerializeField] private BasicEffect[] _dodge;
    [SerializeField] private BasicEffect[] _defeat;
    public ICombat2DEffect[] GetResponseEffect(DiziCombatAnimator.Responses response) => response switch
    {
        DiziCombatAnimator.Responses.Suffer => _suffer,
        DiziCombatAnimator.Responses.Dodge => _dodge,
        DiziCombatAnimator.Responses.Defeat => _defeat,
        _ => throw new ArgumentOutOfRangeException(nameof(response), response, null)
    };

    [Serializable]private class BasicEffect : CombatEffect
    {
        [SerializeField] private GameObject _obj;
        [SerializeField] private float 时长 = 0.2f;

        public override GameObject Obj => _obj;
        public override float LastingSecs => 时长;
        public override GameObject Invoke(Transform parent)
        {
            var effect = Instantiate(_obj, parent);
            effect.SetActive(true);
            return effect;
        }
    }
}

[Serializable]
public abstract class CombatEffect : ICombat2DEffect
{
    public abstract GameObject Obj { get; }
    public abstract float LastingSecs { get; }
    public abstract GameObject Invoke(Transform parent);
}
