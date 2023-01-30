using System;
using HotFix_Project.Views.Bases;
using Systems.Messaging;
using UnityEngine.UI;
using Utls;
using Views;

namespace HotFix_Project.Managers;

public class WinAdvConfirmManager
{
    private View_winAdvConfirm WinAdvConfirm { get; set; }

    public void Init()
    {
        Game.UiBuilder.Build("view_winAdvConfirm", v =>
        {
            WinAdvConfirm = new View_winAdvConfirm(v, () => XDebug.LogWarning("弟子历练! 目前历练功能未实现"));
            Game.MainUi.SetWindow(v, false);
        }, RegEvents);
    }

    private void RegEvents()
    {

    }

    private class View_winAdvConfirm : UiBase
    {
        private Text Text_diziName { get; }
        private Text Text_message { get; }
        private Text Text_value { get; }
        private Text Text_quantifier { get; }
        private Button Btn_X { get; }
        private Button Btn_Confirm { get; }

        public View_winAdvConfirm(IView v, Action onConfirmAction) : base(v.GameObject, false)
        {
            Text_diziName = v.GetObject<Text>("text_diziName");
            Text_message = v.GetObject<Text>("text_message");
            Text_value = v.GetObject<Text>("text_value");
            Text_quantifier = v.GetObject<Text>("text_quantifier");
            Btn_X = v.GetObject<Button>("btn_x");
            Btn_X.OnClickAdd(OnClose);
            Btn_Confirm = v.GetObject<Button>("btn_confirm");
            Btn_Confirm.OnClickAdd(() =>
            {
                onConfirmAction?.Invoke();
                OnClose();
            });
        }
        public void Set(string diziName, string message, int cost, string quantifier)
        {
            Text_diziName.text = diziName;
            Text_message.text = message;
            Text_value.text = cost.ToString();
            Text_quantifier.text = quantifier;
        }

        private void OnClose() => Display(false);
    }
}
