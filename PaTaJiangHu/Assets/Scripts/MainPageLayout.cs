using System;
using UnityEngine;
using Utls;
using Views;

public class MainPageLayout : MonoBehaviour
{
    public enum Sections
    {
        Top,
        Mid,
        Btm
    }
    [SerializeField] private Transform _top;
    [SerializeField] private Transform _mid;
    [SerializeField] private Transform _btm;

    public Transform Top => _top;
    public Transform Mid => _mid;
    public Transform Btm => _btm;

    public void Set(IView view, Sections section, bool resetPos)
    {
        var sectionTran = GetTransform(section);
        view.GameObject.transform.SetParent(sectionTran);
        if (resetPos)
        {
            var rect = (RectTransform)view.GameObject.transform;
            rect.SetTop(0);
            rect.SetLeft(0);
            rect.SetRight(0);
            rect.SetBottom(0);
        }
    }

    private Transform GetTransform(Sections section)
    {
        var sectionTran = section switch
        {
            Sections.Top => Top,
            Sections.Mid => Mid,
            Sections.Btm => Btm,
            _ => throw new ArgumentOutOfRangeException(nameof(section), section, null)
        };
        return sectionTran;
    }
}