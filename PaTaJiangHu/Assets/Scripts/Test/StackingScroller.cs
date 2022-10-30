using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public interface IStackingScroller
{
    IReadOnlyList<Transform> List { get; }
    void Init(StackingScroller.Modes mode);
    IEnumerator PlaceAndWaitStacking(Transform tran,float duration);
    void StackTransform(Transform tran,float duration);
    void RemoveTransform(Transform tran);
    void ClearList();
}

public class StackingScroller : MonoBehaviour, IStackingScroller
{
    public enum Modes
    {
        LeftToRight,
        RightToLeft,
    }
    public IStackingScroller Instance => this;
    private RectTransform _rect;
    private RectTransform Rect
    {
        get
        {
            if (!_rect) _rect = (RectTransform)transform;
            return _rect;
        }
    }

    [SerializeField] private Modes _mode;
    private float Width => Rect.rect.width;
    private float Height => Rect.rect.height;
    private float X => Rect.localPosition.x;
    private float Y => Rect.localPosition.y;

    private List<Transform> _list = new List<Transform>();
    public IReadOnlyList<Transform> List=> _list;
    public void Init(Modes mode) => _mode = mode;

    /// <summary>
    /// 获取主体矩形的的右内侧(物件在内侧)。
    /// </summary>
    /// <param name="tran"></param>
    /// <param name="align"></param>
    /// <returns></returns>
    public Vector3 GetInnerRightVector(Transform tran,float align = 0)
    {
        var rect = (RectTransform)tran;
        return new Vector3(GetXRight(Width, rect) - align, 0, 0);
    }
    /// <summary>
    /// 获取主体矩形的的左内侧(物件在内侧)。
    /// </summary>
    /// <param name="tran"></param>
    /// <param name="align"></param>
    /// <returns></returns>
    public Vector3 GetInnerLeftVector(Transform tran, float align = 0)
    {
        var rect = (RectTransform)tran;
        return new Vector3(GetXLeft(Width, rect) + align, 0, 0);
    }
    //右内侧算法
    private static float GetXRight(float width, RectTransform rect) => width * 0.5f - rect.rect.width * 0.5f;
    //左内侧算法
    private static float GetXLeft(float width,RectTransform rect) => -width * 0.5f + rect.rect.width * 0.5f;

    private IEnumerator AddTransformToStack(IList<Transform> list, Transform tran, Modes mode, float duration = 1f)
    {
        var width = list.Cast<RectTransform>().Sum(r => r.rect.width); //获取列表总长度
        var from = mode == Modes.LeftToRight ? GetInnerLeftVector(tran) : GetInnerRightVector(tran);
        var to = mode == Modes.LeftToRight ? GetInnerRightVector(tran, width) : GetInnerLeftVector(tran, width);
        tran.localPosition = from;
        list.Add(tran);
        yield return tran.DOLocalMoveX(to.x, duration).WaitForCompletion();
    }

    public IEnumerator PlaceAndWaitStacking(Transform tran,float duration)
    {
        tran.SetParent(transform);
        yield return AddTransformToStack(_list, tran, _mode, duration);
    }
    public void StackTransform(Transform tran,float duration)
    {
        tran.SetParent(transform);
        StartCoroutine(AddTransformToStack(_list, tran, _mode, duration));
    }

    public void RemoveTransform(Transform tran) => _list.Remove(tran);
    public void ClearList() => _list.Clear();
}
