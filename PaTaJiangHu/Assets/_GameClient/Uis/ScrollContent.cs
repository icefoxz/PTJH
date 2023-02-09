using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Views;

public abstract class ScrollContent : MonoBehaviour
{
    protected enum Edges
    {
        Top,
        Bottom
    }
    [SerializeField] protected ScrollRect _scrollRect;
    [SerializeField] private View _prefab;
    [SerializeField] private Edges _edge;
    [SerializeField] private float _targetViewOffset = 5f;

    protected ScrollRect ScrollRect => _scrollRect;
    protected RectTransform.Axis Vertical => RectTransform.Axis.Vertical;
    protected IView Prefab => _prefab;
    protected Edges Edge => _edge;

    protected IReadOnlyList<View> Elements => _elements;
    protected int IndexOf(View element) => _elements.IndexOf(element);
    private EdgeTrigger TopEdgeTrigger { get; set; }
    private EdgeTrigger BtmEdgeTrigger { get; set; }

    // 获取可视范围
    private float GetViewSize() => ScrollRect.viewport.rect.GetSize(Vertical);

    #region DisableTriggerMethod
    //暂停触发器
    private bool _disableTrigger;
    private readonly List<View> _elements = new List<View>();

    //暂停触发器并执行action方法,并且在结束后重置触发范围
    protected void TempDisableTrigger(Action action)
    {
        _disableTrigger = true;
        action?.Invoke();
        _disableTrigger = false;
    }
    //当滚动的时候
    private void OnScrolling(Vector2 vec)
    {
        if (_disableTrigger) return;
        var minPos = GetMinEdge();
        var maxPos = GetMaxEdge();
        BtmEdgeTrigger?.Update(minPos, NormalizedPosition());
        TopEdgeTrigger?.Update(maxPos, NormalizedPosition());
    }
    #endregion

    void Start()//每次开启确保可视范围Viewport的大小一致
    {
        ScrollRect.content.sizeDelta = new Vector2(0, ScrollRect.viewport.rect.height);
        switch (Vertical)
        {
            case RectTransform.Axis.Horizontal:
                ScrollRect.horizontalNormalizedPosition = 0;
                break;
            case RectTransform.Axis.Vertical:
                ScrollRect.verticalNormalizedPosition = 0;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        ScrollRect.onValueChanged.RemoveListener(OnScrolling);
        ScrollRect.onValueChanged.AddListener(OnScrolling);
    }

    public void InstancePrefab() => CreatePrefab(v =>
    {
        v.GameObject.SetActive(true);
        v.name = $"Element{Elements.Count}";
        v.GetObject<Text>("text").text = Elements.Count.ToString();
    });

    public View CreatePrefab(Action<View> instanceAction)
    {
        var v = Instantiate(_prefab, _scrollRect.content);
        AlignToLastElementPosition(v, Vertical, Edge);
        _elements.Add(v);
        AutoResizeContent();
        //如果是由下到上,需要重新调整列表位置,但上到下不需要
        if (Edge == Edges.Bottom &&
            Elements.Sum(e => e.RectTransform.rect.GetSize(Vertical)) > GetViewSize()) 
        {
            var last = Elements[^1];
            var firstPos = GetFromAxis(Vertical,
                GetEdgePointFromOffset(Edges.Top, Elements[0].RectTransform));
            AlignListFromPosition(firstPos - GetFromAxis(last.RectTransform.offsetMax), Edges.Bottom);
        }

        instanceAction?.Invoke(v);
        return v;
    }

    public void ResetListPosition(bool alignFromTopEdge) => ResetListPosition(alignFromTopEdge, Edge switch
    {
        Edges.Top => GetMaxEdge(),
        Edges.Bottom => GetMinEdge(),
        _ => throw new ArgumentOutOfRangeException()
    });
    protected void ResetListPosition(bool alignFromTopEdge, float edgePosition)
    {
        AlignListFromPosition(edgePosition, Edge);
        if (alignFromTopEdge) //如果是根据上边缘重置位置,则需要获取上边缘的元素与边缘的差值,重新排列
        {
            edgePosition -= GetOffsetFromElement();
            AlignListFromPosition(edgePosition, Edge);
        }

        float GetOffsetFromElement()
        {
            var offsetMax = GetFromAxis(GetEdgeMaxElement(GetViewEdge(true, NormalizedPosition()))
                .RectTransform.offsetMax);
            var topEdge = GetNormalizeEdgePosition(true);
            return offsetMax - topEdge;
        }
    }

    //重设可视范围的元素触发器
    public void ResetMinMaxTrigger() => ResetMinMaxTrigger(GetMinEdge(), GetMaxEdge());

    protected void ResetMinMaxTrigger(float minPoint, float maxPoint)
    {
        var maxElement = GetEdgeMaxElement(maxPoint);
        var minElement = GetEdgeMinElement(minPoint);
        if (TopEdgeTrigger == null) RegTopTrigger(maxElement);
        else TopEdgeTrigger?.SetElement(maxElement);
        if (BtmEdgeTrigger == null) RegBtmTrigger(minElement);
        else BtmEdgeTrigger?.SetElement(minElement);
    }

    //调整列表位置从(起始位置)
    private void AlignListFromPosition(float startPosition,Edges edge)
    {
        var edgePosition = startPosition;
        //自动把物件位置添加到最后一个元素的边缘
        for (var i = 0; i < Elements.Count; i++)
        {
            var element = Elements[i];
            var elementPosition = GetElementPosFromEdgePoint(element, edgePosition, Vertical, edge);
            SetElementLocalPosition(element, elementPosition);
            edgePosition = GetFromAxis(GetEdgePointFromOffset(edge, element.RectTransform));
        }
    }

    /// <summary>
    /// 获取滚动的最小边缘值(下面)
    /// </summary>
    /// <returns></returns>
    private float GetMinEdge()=> GetNormalizeEdgePosition(false);
    /// <summary>
    /// 获取滚动的最大边缘值(上面)
    /// </summary>
    /// <returns></returns>
    private float GetMaxEdge()=> GetNormalizeEdgePosition(true);

    /// <summary>
    /// 获取列表中上面边缘位置的元素
    /// </summary>
    /// <returns></returns>
    protected View GetEdgeMaxElement() => GetEdgeMaxElement(GetMaxEdge());
    private View GetEdgeMaxElement(float edgePoint)
    {
        //如果是上到下,最大值是(上面)第一个元素
        //如果是下到上,最大值是最后一个元素
        return Edge switch
        {
            Edges.Top => Elements.FirstOrDefault(e =>
                IsInElementBody(e, edgePoint, Vertical)),
            Edges.Bottom => Elements.FirstOrDefault(e =>
                IsInElementBody(e, edgePoint, Vertical)),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static bool IsInElementBody(View e, float edgePoint, RectTransform.Axis axis) =>
        GetFromAxis(axis, e.RectTransform.offsetMin) < edgePoint &&
        GetFromAxis(axis, e.RectTransform.offsetMax) >= edgePoint;

    /// <summary>
    /// 获取列表中最下面边缘位置的元素
    /// </summary>
    /// <returns></returns>
    protected View GetEdgeMinElement() => GetEdgeMinElement(GetMinEdge());
    private View GetEdgeMinElement(float edgePoint)
    {
        //如果是上到下,最小值是(下面)最后一个元素
        //如果是下到上,最小值是第一个元素
        var element = Edge switch
        {
            Edges.Top => Elements.FirstOrDefault(e =>
                GetFromAxis(e.RectTransform.offsetMin) <= edgePoint &&
                GetFromAxis(e.RectTransform.offsetMax) > edgePoint),
            Edges.Bottom => Elements.FirstOrDefault(e =>
                GetFromAxis(e.RectTransform.offsetMin) <= edgePoint &&
                GetFromAxis(e.RectTransform.offsetMax) > edgePoint),
            _ => throw new ArgumentOutOfRangeException()
        };
        return element;
    }

    //把第一个元素的排位插到最后一个元素
    public void AlignFistToLast()
    {
        var element = Elements[0];
        if (IsElementShowingInViewport(element, Edge)) return;
        AlignToLastElementPosition(element, Vertical, Edge);
        _elements.Remove(element);
        _elements.Add(element);

        bool IsElementShowingInViewport(View view, Edges edge)
        {
            return edge switch
            {
                Edges.Top => GetFromAxis(view.RectTransform.offsetMin) <= GetMaxEdge(),
                Edges.Bottom => GetFromAxis(view.RectTransform.offsetMax) > GetMinEdge(),
                _ => throw new ArgumentOutOfRangeException(nameof(edge), edge, null)
            };
        }
    }

    //把最后一个元素排到第一个
    public void AlignLastToFirst()
    {
        var element = Elements[^1];
        if (IsElementShowingInViewport(element, Edge)) return;
        AlignToFirstElementPosition(element, Edge);
        _elements.Remove(element);
        _elements.Insert(0, element);

        bool IsElementShowingInViewport(View view, Edges edge)
        {
            return edge switch
            {
                Edges.Top => GetFromAxis(view.RectTransform.offsetMax) >= GetMinEdge(),
                Edges.Bottom => GetFromAxis(view.RectTransform.offsetMin) < GetMaxEdge(),
                _ => throw new ArgumentOutOfRangeException(nameof(edge), edge, null)
            };
        }

        void AlignToFirstElementPosition(View v, Edges d)
        {
            var first = Elements[0].RectTransform;
            var edgePoint = d switch
            {
                Edges.Top => GetFromAxis(first.offsetMax),
                Edges.Bottom => GetFromAxis(first.offsetMin),
                _ => throw new ArgumentOutOfRangeException(nameof(d), d, null)
            };
            SetElementLocalPosition(v, edgePoint + GetEdgePointAlignment(first));
            v.transform.SetAsFirstSibling();

            float GetEdgePointAlignment(RectTransform r)
            {
                return r.rect.GetSize(Vertical) * d switch
                {
                    Edges.Top => 0.5f,
                    Edges.Bottom => -0.5f,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }
    }

    //根据边缘点给出rect边缘值
    private static Vector2 GetEdgePointFromOffset(Edges edge, RectTransform rectTransform)
    {
        return edge switch
        {
            Edges.Top => rectTransform.offsetMin,
            Edges.Bottom => rectTransform.offsetMax,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    //设置元素位置
    private void SetElementLocalPosition(View element, float position)
    {
        switch (Vertical)
        {
            case RectTransform.Axis.Horizontal:
                element.transform.SetLocalX(position);
                break;
            case RectTransform.Axis.Vertical:
                element.transform.SetLocalY(position);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static float GetFromAxis(RectTransform.Axis axis, Vector3 vector) => axis switch
    {
        RectTransform.Axis.Horizontal => vector.x,
        RectTransform.Axis.Vertical => vector.y,
        _ => throw new ArgumentOutOfRangeException()
    };

    private float GetFromAxis(Vector3 vector) => GetFromAxis(Vertical, vector);

    //更新滑动栏大小
    private void AutoResizeContent()
    {
        var contentSize = ScrollRect.content.rect.GetSize(Vertical);
        var elementsSize = Elements.Sum(e => e.RectTransform.rect.GetSize(Vertical));
        var size = MathF.Max(contentSize, elementsSize);
        if (elementsSize >= contentSize)//如果一样大小需要更新位置, 因为会发生元素位置调整
        {
            switch (Edge)
            {
                case Edges.Top:
                {
                    var vec = ScrollRect.content.sizeDelta;
                    ScrollRect.content.sizeDelta =
                        Vertical switch
                        {
                            RectTransform.Axis.Horizontal => new Vector2(size, vec.y),
                            RectTransform.Axis.Vertical => new Vector2(vec.x, size),
                            _ => throw new ArgumentOutOfRangeException()
                        };
                }
                    break;
                case Edges.Bottom:
                {
                    AddContentSize(size - GetFromAxis(ScrollRect.content.sizeDelta), true);
                }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    protected void AddContentSize(float size, bool targetTop)
    {
        var contentSize = GetFromAxis(Vertical, ScrollRect.content.sizeDelta);
        var finalSize = size + contentSize;
        SetContentSize(finalSize, targetTop);
    }

    protected void SetContentSize(float finalSize, bool targetTop)
    {
        var max = ScrollRect.content.offsetMax;
        var min = ScrollRect.content.offsetMin;
        if (targetTop)
        {
            ScrollRect.content.offsetMax = Vertical switch
            {
                RectTransform.Axis.Horizontal =>
                    new Vector2(finalSize + min.x, max.y),
                RectTransform.Axis.Vertical =>
                    new Vector2(max.x, min.y + finalSize),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        else
        {
            ScrollRect.content.offsetMin = Vertical switch
            {
                RectTransform.Axis.Horizontal =>
                    new Vector2(max.x - finalSize, min.y),
                RectTransform.Axis.Vertical =>
                    new Vector2(min.x, max.y - finalSize),
                _ => throw new ArgumentOutOfRangeException()
            };

        }
    }

    //物件位置格式 = 物件边缘 + 位置
    private float GetElementPosFromEdgePoint(View view, float position, RectTransform.Axis axis, Edges edge)
    {
        return GetEdgePointAlignment(view, axis, edge) + position;
        //获取元素边缘的调整点, 从中心点(物件大小/2)(前点往内进,后点往外推)
        float GetEdgePointAlignment(View element, RectTransform.Axis a, Edges d)
        {
            return element.RectTransform.rect.GetSize(a) * d switch
            {
                Edges.Top => -0.5f,
                Edges.Bottom => 0.5f,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    //自动排位到最后一个元素
    private void AlignToLastElementPosition(View v, RectTransform.Axis axis, Edges edge)
    {
        SetElementLocalPosition(v, Elements.Count == 0
            ? GetElementPosFromEdgePoint(v, GetNormalizeEdgePosition(edge == Edges.Top), axis, edge)
            : GetElementPosFromEdgePoint(v, GetFromAxis(GetEdgePointFromOffset(edge, Elements[^1].RectTransform)), axis, edge));
        v.transform.SetAsLastSibling();
    }

    private void DestroyPrefab(View v)
    {
        _elements.Remove(v);
        Destroy(v.GameObject);
        AutoResizeContent();
        AlignListFromPosition(GetNormalizeEdgePosition(Edge == Edges.Top), Edge);
    }

    /// <summary>
    /// 根据Normalize算出当前的边缘位置
    /// </summary>
    /// <returns></returns>
    private float GetNormalizeEdgePosition(bool topEdge)
    {
        var normalizedPosition = NormalizedPosition();

        return GetViewEdge(topEdge, normalizedPosition);
    }

    private float NormalizedPosition()
    {
        var normalizedPosition = Vertical switch
        {
            RectTransform.Axis.Horizontal => ScrollRect.horizontalNormalizedPosition,
            RectTransform.Axis.Vertical => ScrollRect.verticalNormalizedPosition,
            _ => throw new ArgumentOutOfRangeException(nameof(Vertical), Vertical, null)
        };
        return normalizedPosition;
    }

    private float GetViewEdge(bool topEdge, float normalizedPosition)
    {
        var contentSize = ScrollRect.content.rect.GetSize(Vertical);
        var viewPortSize = GetViewSize();
        var edgeAlign = topEdge ? 0 : -viewPortSize;
        var size = contentSize - viewPortSize;
        var normalizeFromSize = normalizedPosition * size + edgeAlign;
        return normalizeFromSize - size;
    }


    #region EdgeTriggers
    private EdgeTrigger InstanceEdgeTrigger(string n,View element)
    {
        return Edge switch
        {
            Edges.Bottom => new EdgeTrigger(n, Vertical, Edge, element, OnTriggerReplacePreviousElement,
                OnTriggerReplaceNextElement),
            Edges.Top => new EdgeTrigger(n, Vertical, Edge, element, OnTriggerReplaceNextElement,
                OnTriggerReplacePreviousElement),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    //注册位置max触发器
    private void RegTopTrigger(View element)
    {
        TopEdgeTrigger = InstanceEdgeTrigger("Top", element);
        TopEdgeTrigger.OnDownEdgeUpdate += OnScrollUpTopEdge;
        TopEdgeTrigger.OnUpEdgeUpdate += OnScrollDownTopEdge;
    }

    //注册位置min触发器
    private void RegBtmTrigger(View element)
    {
        BtmEdgeTrigger = InstanceEdgeTrigger("Btm", element);
        BtmEdgeTrigger.OnDownEdgeUpdate += OnScrollUpBottomEdge; 
        BtmEdgeTrigger.OnUpEdgeUpdate += OnScrollDownBottomEdge;
    }

    //上边往上替换元素
    protected abstract void OnScrollDownTopEdge(View element, float edgePosition, float normalizedPosition);
    //{
    //    //switch (Edge)
    //    //{
    //    //    case Edges.Top:
    //    //        AlignFistToLast();
    //    //        break;
    //    //    case Edges.Bottom:
    //    //        AlignLastToFirst();
    //    //        TempDisableTrigger(() => SetContentSize(Elements.Last().RectTransform.rect.GetSize(Vertical), false));
    //    //        break;
    //    //    default:
    //    //        throw new ArgumentOutOfRangeException();
    //    //}
    //    print($"TopUp {element?.name}");
    //}
    //上边往下替换元素
    protected abstract void OnScrollUpTopEdge(View element, float edgePosition, float normalizedPosition);
    //下边往下替换元素
    protected abstract void OnScrollDownBottomEdge(View element, float edgePosition, float normalizedPosition);
    //{
    //    switch (Edge)
    //    {
    //        case Edges.Top:
    //            AlignLastToFirst();
    //            break;
    //        case Edges.Bottom:
    //            AlignFistToLast();
    //            TempDisableTrigger(() =>
    //            {
    //                SetContentSize(Elements.Last().RectTransform.rect.GetSize(Vertical), true);
    //                ResetListPosition();
    //            });
    //            break;
    //        default:
    //            throw new ArgumentOutOfRangeException();
    //    }
    //    print($"BtmDown {element?.name}");
    //}
    //下边往上替换元素
    protected abstract void OnScrollUpBottomEdge(View element, float edgePosition, float normalizedPosition);

    private View OnTriggerReplaceNextElement(View v)
    {
        if (v == null) return null;
        var index = _elements.IndexOf(v);
        if (index == Elements.Count - 1) return null;
        var next = Elements[index + 1];
        //print("ElementNext = " + next.name);
        return next;
    }
    private View OnTriggerReplacePreviousElement(View v)
    {
        if (v == null) return null;
        var index = _elements.IndexOf(v);
        if (index == 0) return null;
        var next = Elements[index - 1];
        //print("ElementPrev = " + next.name);
        return next;
    }

    //位置触发器
    private class EdgeTrigger
    {
        private string Name { get; }
        public event Action<View,float,float> OnDownEdgeUpdate;
        public event Action<View,float,float> OnUpEdgeUpdate;
        private event Func<View, View> OnMinEdgeTrigger;
        private event Func<View, View> OnMaxEdgeTrigger;
        public RectTransform.Axis Axis { get; }
        public Edges Edge { get; }
        public View Element { get; private set; }

        public EdgeTrigger(string name,RectTransform.Axis axis, Edges edge, View element,
            Func<View, View> onMinTrigger, Func<View, View> onMaxTrigger)
        {
            Name = name;
            Axis = axis;
            Edge = edge;
            OnMinEdgeTrigger = onMinTrigger;
            OnMaxEdgeTrigger = onMaxTrigger;
            SetElement(element);
        }

        public void Update(float edgePosition,float normalizedPosition)
        {
            if (Element == null) return;

            if (GetFromAxis(Axis, Element.RectTransform.offsetMin) >= edgePosition)
            {
                //print($"{Name}! pos = {normalizedPosition}, " + $"edgeMin {GetFromAxis(Axis, Element.RectTransform.offsetMin)}");
                ChangeElement(OnMinEdgeTrigger);
                OnUpEdgeUpdate?.Invoke(Element, edgePosition, normalizedPosition);
                return;
            }

            if (GetFromAxis(Axis, Element.RectTransform.offsetMax) < edgePosition)
            {
                //print($"{Name}! pos = {normalizedPosition}, " + $"edgeMax {GetFromAxis(Axis, Element.RectTransform.offsetMax)}");
                ChangeElement(OnMaxEdgeTrigger);
                OnDownEdgeUpdate?.Invoke(Element, edgePosition, normalizedPosition);
            }
            
            void ChangeElement(Func<View, View> onElementChangeFunc)
            {
                Element = onElementChangeFunc?.Invoke(Element);
                SetElement(Element);
            }
        }

        public void SetElement(View element)
        {
            if (element == null) return;
            Element = element;
        }
    }
    #endregion
}