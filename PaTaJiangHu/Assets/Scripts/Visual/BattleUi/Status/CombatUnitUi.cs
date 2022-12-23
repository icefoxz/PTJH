using BattleM;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Visual.BaseUis;
using Visual.BattleUi.Scene;

namespace Visual.BattleUi.Status
{
    public class CombatUnitUi : BaseUi
    {
        [SerializeField] private Text _name;
        [SerializeField] private Text _hp;
        [SerializeField] private Text _hpPop;
        [SerializeField] private Text _mp;
        [SerializeField] private Text _mpPop;
        [SerializeField] private Text _popText;
        [SerializeField] private Stickman _stickman;
        public Stickman Stickman => _stickman;
        public int CombatId => Unit?.CombatId ?? -1;
        private ICombatUnit Unit { get; set; }
        public void Set(CombatUnit unit)
        {
            Unit = unit;
            Stickman.Init(unit);
            SetTextUi(_popText, string.Empty);
            SetTextUi(_hpPop, string.Empty);
            SetTextUi(_mpPop, string.Empty);
            SetTextUi(_name, unit.Name);
            var s = unit.Status;
            UpdateTextUi(s.Hp, s.Mp);
            Show();
        }
        public void UpdateTextUi(IConditionValue hp, IConditionValue mp)
        {
            SetTextUi(_hp, SetConText(hp));
            SetTextUi(_mp, SetConText(mp));
        }
        public void UpdateTextUi(ICombatStatus status)
        {
            SetTextUi(_hp,SetConText(status.Hp));
            SetTextUi(_mp,SetConText(status.Mp));
        }
        //public void UpdateUi()
        //{
        //    SetTextUi(_hp,SetConText(Unit.Status.Hp));
        //    SetTextUi(_tp,SetConText(Unit.Status.Tp));
        //    SetTextUi(_mp,SetConText(Unit.Status.Mp));
        //}
        private static string SetConText(IConditionValue status) => $"{status.Value}/{status.Max}";
        private static string SetConText(IGameCondition status) => $"{status.Value}/{status.Max}";
        public override void ResetUi()
        {
            _stickman.transform.SetParent(transform);
            _stickman.ResetPosition();
            Unit = null;
            SetTextUi(_name, string.Empty);
            SetTextUi(_hp, string.Empty);
            SetTextUi(_mp, string.Empty);
            SetTextUi(_hpPop, string.Empty);
            SetTextUi(_mpPop, string.Empty);
        }
        private static void SetTextUi(Text ui, string text) => ui.text = text;
        public void Pop(string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            _popText.gameObject.SetActive(true);
            _popText.text = text;
            var lastPos = _popText.transform.localPosition;
            _popText.transform.DOLocalMoveY(50, 1).SetEase(Ease.OutCirc).OnComplete(() =>
            {
                _popText.gameObject.SetActive(false);
                _popText.transform.localPosition = lastPos;
            });
        }

        public void PopHp(int value)
        {
            if(value == 0) return;
            PopCon(value.ToString(), _hp, _hpPop);
        }
        public void PopMp(int value) {
            if (value == 0)
                return;
            PopCon(value.ToString(), _mp, _mpPop);
        }
        private void PopCon(string text, Text con, Text pop)
        {
            if (string.IsNullOrEmpty(text)) return;
            con.transform.DOShakeScale(0.2f).OnComplete(() =>
            {
                con.transform.localRotation = Quaternion.identity;
                con.transform.localScale = Vector3.one;
            });
            var lastPos = pop.transform.localPosition;
            pop.text = text;
            pop.transform.DOShakeScale(0.8f).SetEase(Ease.OutCirc)
                .OnComplete(() =>
                {
                    pop.text = string.Empty;
                    pop.transform.localPosition = lastPos;
                    pop.transform.localScale = Vector3.one;
                });
        }

        public void SetAnim(Stickman.Anims action) => _stickman.SetAnim(action);
    }
}