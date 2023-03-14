using Server.Configs.Adventures;
using Server.Controllers;
using System.Collections.Generic;
using Core;
using UnityEngine;
using Utls;
using Views;

public class TestAdvPlayer : MonoBehaviour
{
    [SerializeField] private View _view;
    private IView View => _view;

    public void SetViewSize(int height)
    {
        var r = View.RectTransform.rect;
        _view.RectTransform.SetHeight(height);
    }
}
