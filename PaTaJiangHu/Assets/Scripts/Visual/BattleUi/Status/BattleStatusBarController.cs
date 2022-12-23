using System;
using System.Collections.Generic;
using System.Linq;
using BattleM;
using UnityEngine;
using Visual.BaseUis;

namespace Visual.BattleUi.Status
{
    public interface IBattleStatusBarController
    {
        IEnumerable<CombatStatusUi> List { get; }
        void Init();
        void AddUi(int stance, int combatId, Action<CombatStatusUi> initAction);
        void ResetUi();
        void UpdateStatus(int combatId, int hp, int fixHp, int mp, int fixMp);
        void UpdateStatus(int combatId, IConditionValue hp, IConditionValue mp);
        void UpdateText(int combatId, IConditionValue hp, IConditionValue mp);
    }
    public class BattleStatusBarController : BaseUi, IBattleStatusBarController
    {
        [SerializeField] private CombatStatusUi leftPrefab;
        [SerializeField] private CombatStatusUi rightPrefab;
        [SerializeField] private Transform leftParent;
        [SerializeField] private Transform rightParent;
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

        public void UpdateStatus(int combatId,int hp, int fixHp, int mp, int fixMp)
        {
            _map[combatId].UpdateSlider(hp, fixHp, mp, fixMp);
        }
        public void UpdateStatus(int combatId, IConditionValue hp, IConditionValue mp)
        {
            _map[combatId].UpdateStatus(hp, mp);
        }
        public void UpdateText(int combatId, IConditionValue hp, IConditionValue mp)
        {
            _map[combatId].UpdateText(hp, mp);
        }
    }
}