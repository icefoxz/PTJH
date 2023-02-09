using System;
using UnityEngine.UI;
using Views;

public class ScrollListView : ScrollContent
{
    public event Action<View> OnTopInElement;
    public event Action<View> OnBtmInElement;
    public void ResetSizeToElement(int index)
    {
        TempDisableTrigger(() =>
        {
            var edge = Edge switch
            {
                Edges.Top => Elements[index].RectTransform.offsetMin.y,
                Edges.Bottom => Elements[index].RectTransform.offsetMax.y,
                _ => throw new ArgumentOutOfRangeException()
            };
            AddContentSize(edge, Edge == Edges.Bottom);
            ResetListPosition(Edge == Edges.Top);
        });
    }

    public void PushToTopEdge() => PushToTopEdge(IndexOf(GetEdgeMaxElement()));
    public void PushToTopEdge(int index)
    {
        TempDisableTrigger(() =>
        {
            var maxElement = GetEdgeMaxElement();
            var size = 0f;
            for (int i = 0; i < Elements.Count; i++)
            {
                var element = Elements[i];
                size += element.RectTransform.sizeDelta.y;
                if (element == maxElement) break;
            }

            SetContentSize(size, false);
            ScrollRect.verticalNormalizedPosition = 0;
            ResetListPosition(false);
            ScrollRect.verticalNormalizedPosition = 1;
        });
        ScrollRect.movementType = ScrollRect.MovementType.Clamped;
    }

    protected override void OnScrollDownTopEdge(View element, float edgePosition, float normalizedPosition)
    {
        TempDisableTrigger(AlignLastToFirst);
        AddContentSize(100, false);
    }

    protected override void OnScrollUpTopEdge(View element, float edgePosition, float normalizedPosition)
    {
        //print($"ScrollUpTop {element?.name}");
    }

    protected override void OnScrollDownBottomEdge(View element, float edgePosition, float normalizedPosition)
    {
        //print($"ScrollDownBtm {element?.name}");
    }

    protected override void OnScrollUpBottomEdge(View element, float edgePosition, float normalizedPosition)
    {
        TempDisableTrigger(AlignFistToLast);
    }
}