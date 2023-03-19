using System;
using UnityEngine;
using Utls;
using Views;

public class MainPageLayout : View
{
    public enum Sections
    {
        Top,
        Game,
        Btm
    }
    [SerializeField] private Section _top;
    [SerializeField] private Transform _game;
    [SerializeField] private Section _btm;
    [SerializeField] private Transform _page;

    public Transform Top => _top.Tran;
    public Transform Game => _game;
    public Transform Btm => _btm.Tran;

    public void HideAll(Sections section)
    {
        var tran = GetTran(section);
        foreach (Transform t in tran)
        {
            switch (section)
            {
                case Sections.Top:
                    //背景会一直显示
                    t.gameObject.SetActive(_top.Bg.transform == t);
                    break;
                case Sections.Btm:
                    //背景会一直显示
                    t.gameObject.SetActive(_btm.Bg.transform == t);
                    break;
                case Sections.Game:
                    t.gameObject.SetActive(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(section), section, null);
            }
        }
    }
    public void Set(IView view, Sections section, bool resetPos)
    {
        var tran = GetTran(section);
        view.GameObject.transform.SetParent(tran);
        if (resetPos)
        {
            var rect = (RectTransform)view.GameObject.transform;
            rect.SetTop(0);
            rect.SetLeft(0);
            rect.SetRight(0);
            rect.SetBottom(0);
        }
    }

    private Transform GetTran(Sections section)
    {
        var sectionTran = section switch
        {
            Sections.Top => _top.Tran,
            Sections.Game => _game,
            Sections.Btm => _btm.Tran,
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