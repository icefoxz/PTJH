using System;
using System.Collections.Generic;
using System.Linq;
using BattleM;
using UnityEngine;
using UnityEngine.UI;
using Visual.BaseUi;

namespace Visual.BattleUi.Status
{
    public interface IBattleStatusBarController
    {
        IEnumerable<CombatStatusUi> List { get; }
        void Init();
        void AddUi(int stance, int combatId, Action<CombatStatusUi> initAction);
        void ResetUi();
        void UpdateBreath(int left, int right, int maxBreath);
        void UpdateStatus(int combatId, int hp, int fixHp, int tp, int fixTp, int mp, int fixMp);
        void UpdateStatus(int combatId, IConditionValue hp, IConditionValue tp, IConditionValue mp);
    }
    public class BattleStatusBarController : UiBase, IBattleStatusBarController
    {
        [SerializeField] private CombatStatusUi leftPrefab;
        [SerializeField] private CombatStatusUi rightPrefab;
        [SerializeField] private Transform leftParent;
        [SerializeField] private Transform rightParent;
        [SerializeField] private Slider leftSlider;
        [SerializeField] private Text leftText;
        [SerializeField] private Slider rightSlider;
        [SerializeField] private Text rightText;
        private Dictionary<int,CombatStatusUi> _map = new Dictionary<int,CombatStatusUi>();
        public IEnumerable<CombatStatusUi> List => _map.Values;

        public void Init()
        {
            var uis = leftParent.GetComponentsInChildren<CombatStatusUi>()
                .Concat(rightParent.GetComponentsInChildren<CombatStatusUi>());
            foreach (var ui in uis) ui.Hide();
        }

        public void AddUi(int stance,int combatId, Action<CombatStatusUi> initAction)
        {
            var ui = Instantiate(stance == 0 ? leftPrefab : rightPrefab,
                stance == 0 ? leftParent.transform : rightParent.transform);
            ui.gameObject.SetActive(true);
            _map.Add(combatId, ui);
            initAction.Invoke(ui);
        }

        private void RemoveList()
        {
            if (_map.Count > 0) _map.ToList().ForEach(kv => DestroyObj(kv.Value));
            _map.Clear();
        }

        public override void ResetUi()
        {
            RemoveList();
        }

        public void UpdateBreath(int left, int right, int maxBreath)
        {
            if (maxBreath <= 0)
            {
                leftSlider.value = 1;
                rightSlider.value = 1;
                leftText.text = string.Empty;
                rightText.text = string.Empty;
                return;
            }
            leftSlider.value = 1f * left / maxBreath;
            rightSlider.value = 1f * right / maxBreath;
            leftText.text = left.ToString();
            rightText.text = right.ToString();
            //_map[combatId].SetBreath(breath, maxBreath);
        }

        public void UpdateStatus(int combatId,int hp, int fixHp, int tp, int fixTp, int mp, int fixMp)
        {
            _map[combatId].UpdateStatus(hp, fixHp, tp, fixTp, mp, fixMp);
        }
        public void UpdateStatus(int combatId, IConditionValue hp, IConditionValue tp, IConditionValue mp)
        {
            _map[combatId].UpdateStatus(hp, tp, mp);
        }
    }
}