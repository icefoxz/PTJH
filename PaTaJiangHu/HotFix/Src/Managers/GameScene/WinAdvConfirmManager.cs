using System;
using HotFix_Project.Views.Bases;
using Systems.Messaging;
using UnityEngine.UI;
using Utls;
using Views;

namespace HotFix_Project.Managers.GameScene;

internal class WinAdvConfirmManager : UiManagerBase
{
    private View_winAdvConfirm WinAdvConfirm { get; set; }

    protected override MainUiAgent.Sections Section => MainUiAgent.Sections.Window;
    protected override string ViewName => "view_winAdvConfirm";
    protected override bool IsDynamicPixel => false;

    public WinAdvConfirmManager(GameSceneAgent uiAgent) : base(uiAgent)
    {
    }

    protected override void Build(IView view)
    {
        WinAdvConfirm = new View_winAdvConfirm(view, () => XDebug.LogWarning("弟子历练! 目前历练功能未实现"));
    }

    protected override void RegEvents()
    {

    }

    public override void Show() => WinAdvConfirm.Display(true);

    public override void Hide() => WinAdvConfirm.Display(false);

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