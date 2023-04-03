using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 角色处理器,主要处理角色动画与装备物件
/// </summary>
public class CharacterOperator : MonoBehaviour
{
    public enum Facing
    {
        Left,Right
    }
    public enum Anims
    {
        Idle,
        MoveStep,
        Attack,
        AttackReturn,
        Dodge,
        Suffer,
        Defeat,
        Walk,
        Run,
    }
    [SerializeField] private Transform _character;
    [SerializeField] private Animator _anim;
    [SerializeField] private BasicAnimConfig _offend;
    [SerializeField] private BasicAnimConfig _offendReturn;
    [SerializeField] private SpriteRenderer[] _renderers;
    private Dictionary<SpriteRenderer,Color> ColorMapping { get; set; }

    public void Init()
    {
        ColorMapping = _renderers.ToDictionary(r => r, r => r.color);
    }

    public void ResetColor()
    {
        foreach (var (sp, color) in ColorMapping) sp.color = color;
    }
    public void SetColor(Color color)
    {
        foreach (var sp in _renderers) sp.color = color;
    }
    public void FaceTo(Facing face)
    {
        var x = face switch
        {
            Facing.Left => 1,
            Facing.Right => -1,
            _ => throw new ArgumentOutOfRangeException(nameof(face), face, null)
        };
        _character.SetLocalScaleX(x);
    }
    /// <summary>
    /// Pass in only -1 or 1
    /// </summary>
    /// <param name="x"></param>
    public void SwitchFace()
    {
        _character.SetLocalScaleX(_character.localScale.x * -1);
    }

    public void SetPosition(float xPos,Facing facing)
    {
        transform.SetLocalX(xPos);
        FaceTo(facing);
    }

    public void AttackMove(float x)
    {
        var currentPoint = transform.localPosition.x;
        StartCoroutine(AttackMove(currentPoint, x));
    }
    public IEnumerator AttackMove(float currentPoint,float targetPoint)
    {
        SetAnim(Anims.MoveStep);
        yield return OffendMove(targetPoint);
        SetAnim(Anims.Attack);
        yield return new WaitForSeconds(0.1f);
        SetAnim(Anims.AttackReturn);
        yield return OffendReturnMove(currentPoint);
    }

    public IEnumerator Dodge(float delay = 0.3f)
    {
        SetAnim(Anims.Dodge);
        yield return new WaitForSeconds(delay);
        SetAnim(Anims.Idle);
    }
    public IEnumerator Suffer(float delay = 0.3f)
    {
        SetAnim(Anims.Suffer);
        yield return new WaitForSeconds(delay);
        SetAnim(Anims.Idle);
    }

    public void SetAnim(Anims anim, Action callback = null)
    {
        var trigger = anim switch
        {
            Anims.Idle => "idle",
            Anims.MoveStep => "move_step",
            Anims.Attack => "att_1",
            Anims.AttackReturn => "att_return",
            Anims.Dodge => "dodge_1",
            Anims.Suffer => "suffer_1",
            Anims.Defeat => "defeat",
            Anims.Walk => "walking",
            Anims.Run => "run",
            _ => throw new ArgumentOutOfRangeException(nameof(anim), anim, null)
        };
        StartCoroutine(PlayAnim());

        IEnumerator PlayAnim()
        {
            _anim.SetTrigger(trigger);
            yield return _anim;
            callback?.Invoke();
        }
    }

    public IEnumerator OffendMove(float targetPoint) => MoveTo(targetPoint, _offend);
    public IEnumerator OffendReturnMove(float targetPoint) => MoveTo(targetPoint, _offendReturn);
    private IEnumerator MoveTo(float targetPoint, BasicAnimConfig cfg)
    {
        var elapsedTime = 0f;
        var startPos = transform.position;
        var distance = targetPoint - startPos.x;
        var tarPos = new Vector3(targetPoint, transform.position.y, transform.position.z);
        while (elapsedTime < cfg.Duration)
        {
            elapsedTime += Time.deltaTime;
            var t = elapsedTime / cfg.Duration;
            var curveValue = cfg.Evaluate(t);
            var xPosition = startPos.x + distance * curveValue;
            var translation = new Vector3(xPosition - transform.position.x, 0, 0);
            transform.Translate(translation,Space.Self);

            yield return null;
        }

        // Ensure the object reaches the target position
        transform.position = tarPos;
    }

    [Serializable] private class BasicAnimConfig
    {
        [SerializeField] private AnimationCurve 曲线;
        [SerializeField] private float 耗时;
        public float Evaluate(float elapsedTime) => 曲线.Evaluate(elapsedTime);
        public float Duration => 耗时;
    }

}