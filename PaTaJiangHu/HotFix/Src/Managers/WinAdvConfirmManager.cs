using HotFix_Project.Views.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Systems.Messaging;
using UnityEngine.UI;
using Views;

namespace HotFix_Project.Src.Managers;

public class WinAdvConfirmManager
{
    private View_winAdvConfirm WinAdvConfirm { get; set; }

    public void Init()
    {
        InitUi();
        RegEvents();
    }

    private void RegEvents()
    {
        
    }

    private void InitUi()
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
            //Btn_Confirm.OnClickAdd();
        }

        public void SetName(string name)=> Text_diziName.text = name;
        public void SetMessage(string message)=> Text_message.text = message;
        public void SetRequireValue(int value)=> Text_value.text = value.ToString();
        public void SetQuantifier(string quantifier) => Text_quantifier.text = quantifier;
        private void OnClose() => Display(false);
    }
}
