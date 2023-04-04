using System;
using System.Linq;
using UnityEngine;

public interface ICombatEffect
{
    float LastingSecs { get; }
    GameObject Invoke(GameObject parent);
}

[CreateAssetMenu(fileName = "战斗反馈配置", menuName = "战斗单位/战斗反馈")]
public class DiziCombatResponseCfgSo : ScriptableObject
{
    public enum Responses
    {
        Suffer,
        Dodge,
        Defeat
    }

    [SerializeField] private BasicEffect[] _suffer;
    [SerializeField] private BasicEffect[] _dodge;
    [SerializeField] private BasicEffect[] _defeat;
    public ICombatEffect[] GetResponseEffect(Responses response) => response switch
    {
        Responses.Suffer => _suffer,
        Responses.Dodge => _dodge,
        Responses.Defeat => _defeat,
        _ => throw new ArgumentOutOfRangeException(nameof(response), response, null)
    };

    [Serializable]private class BasicEffect : CombatEffect
    {
        [SerializeField] private GameObject _obj;
        [SerializeField] private float 时长 = 0.2f;

        public override GameObject Obj => _obj;
        public override float LastingSecs => 时长;
        public override GameObject Invoke(GameObject parent)
        {
            var effect = Instantiate(_obj, parent.transform);
            effect.SetActive(true);
            return effect;
        }
    }
}

[Serializable]
public abstract class CombatEffect : ICombatEffect
{
    public abstract GameObject Obj { get; }
    public abstract float LastingSecs { get; }
    public abstract GameObject Invoke(GameObject parent);
}
