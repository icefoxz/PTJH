using System;
using UnityEngine;
using Utls;
using Views;

public class MainPageLayout : View
{
    public enum Sections
    {
        Top,
        Mid,
        Btm,
        Page
    }
    [SerializeField] private Section _top;
    [SerializeField] private Section _mid;
    [SerializeField] private Section _btm;
    [SerializeField] private Section _page;

    public Transform Top => _top.Tran;
    public Transform Mid => _mid.Tran;
    public Transform Btm => _btm.Tran;
    public Transform Page => _page.Tran;

    public void HideAll(Sections section)
    {
        var sect = GetSect(section);
        foreach (Transform t in sect.Tran) 
            t.gameObject.SetActive(sect.Bg == t);//背景会一直显示
    }
    public void Set(IView view, Sections section, bool resetPos)
    {
        var sect = GetSect(section);
        view.GameObject.transform.SetParent(sect.Tran);
        if (resetPos)
        {
            var rect = (RectTransform)view.GameObject.transform;
            rect.SetTop(0);
            rect.SetLeft(0);
            rect.SetRight(0);
            rect.SetBottom(0);
        }
    }

    private Section GetSect(Sections section)
    {
        var sectionTran = section switch
        {
            Sections.Top => _top,
            Sections.Mid => _mid,
            Sections.Btm => _btm,
            Sections.Page => _page,
            _ => throw new ArgumentOutOfRangeException(nameof(section), section, null)
        };
        return sectionTran;
    }

    [Serializable] private class Section
    {
        [SerializeField] private Transform _tran;
        [SerializeField] private GameObject _bg;

        public Transform Tran => _tran;
        public GameObject Bg => _bg;
    }
}