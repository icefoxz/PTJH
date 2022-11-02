using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BattleM;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Visual.BaseUi;
using Visual.BattleUi.Input;
using Visual.BattleUi.Scene;
using Visual.BattleUi.Status;

namespace Visual.BattleUi
{
    public interface IBattleWindow
    {
        void Init(UnityAction<bool> battleResultCallback);
        /// <summary>
        /// 第一单位必须是玩家
        /// </summary>
        /// <param name="roles"></param>
        /// <param name="judgment"></param>
        void BattleSetup((int,CombatUnit)[] roles, CombatManager.Judgment judgment);
        void ResetUi();
        void Show();
        void StartBattle();
    }

    public class BattleWindow : UiBase, IBattleWindow,ICombatFragmentPlayer
    {
        [SerializeField] private Button WinBtn;
        [SerializeField] private Button LoseBtn;
        [SerializeField] private BattleLogger _battleLogger;
        [SerializeField] private BattleStanceUi StanceA;
        [SerializeField] private BattleStanceUi StanceB;
        [SerializeField] private Text RoundText;
        [SerializeField] private float _autoRoundSecs = 1;
        [SerializeField] private int _minEscapeRounds = 10;
        [SerializeField] private StickmanSceneController _stickmanScene;
        [SerializeField] private RectTransform _mainCanvas;
        [SerializeField] private PlayerStrategyController _playerStrategyController;
        [SerializeField] private BattleStatusBarController _battleStatusBarController;
        [SerializeField] private BreathUiController _breathUi;
        [SerializeField] private PromptEventUi _leftPromptEventUi;
        [SerializeField] private PromptEventUi _rightPromptEventUi;

        private IBattleStatusBarController BattleStatusBarController => _battleStatusBarController;
        private BattleStanceUi[] Stances { get; set; }
        public BattleStage Stage { get; set; }
        private CombatTempoPlayer CombatTempoPlayer { get; set; }
        private CombatUnit Player { get; set; }
        private event UnityAction<bool> OnBattleResultCallback;
        public void Init(UnityAction<bool> battleResultCallback)
        {
            _playerStrategyController.Init(new (CombatUnit.Strategies strategy, UnityAction action)[]
            {
                (CombatUnit.Strategies.Steady, () => PlayerSetStrategy(CombatUnit.Strategies.Steady)),
                (CombatUnit.Strategies.Hazard, () => PlayerSetStrategy(CombatUnit.Strategies.Hazard)),
                (CombatUnit.Strategies.Defend, () => PlayerSetStrategy(CombatUnit.Strategies.Defend)),
                (CombatUnit.Strategies.RunAway, () => PlayerSetStrategy(CombatUnit.Strategies.RunAway)),
                (CombatUnit.Strategies.DeathFight, () => PlayerSetStrategy(CombatUnit.Strategies.DeathFight))
            }, PlayerAttackPlan, PlayerExertPlan, PlayerRecHp,PlayerRecTp);
            BattleStatusBarController.Init();
            OnBattleResultCallback = isWin => battleResultCallback?.Invoke(isWin);
            Stances = new[] { StanceA, StanceB };
            foreach (var stance in Stances) stance.Init();
            WinBtn.onClick.AddListener(() => OnBattleFinalize(0));
            LoseBtn.onClick.AddListener(() => OnBattleFinalize(1));
            _stickmanScene.Init(_mainCanvas);
            Hide();
            ResetLocalPos();
        }

        #region PlayerCommands
        private void PlayerRecTp()
        {
            var force = Player.Force;
            //Player.RecoverTpPlan(forceForm);
        }

        private void PlayerRecHp()
        {
            var force = Player.Force;
            //Player.RecoverHpPlan(forceForm);
        }

        private void PlayerWaitPlan()
        {
            //Player.WaitPlan();
            //CurrentBreathUpdate();
            //ManualRound();
        }

        private void PlayerExertPlan(IForce force)
        {
            //Player.ExertPlan(force);
            //CurrentBreathUpdate();
            //ManualRound();
        }

        private void PlayerAttackPlan(ICombatForm combat)
        {
            //Player.AttackPlan(combat, Player.BreathBar.Dodge);
            //CurrentBreathUpdate();
            //ManualRound();
        }

        private void PlayerSetStrategy(CombatUnit.Strategies steady)
        {
            Player.SetStrategy(steady);
        }
        #endregion

        #region BattleRound
        private void OnBattleFinalize(int winningStance)
        {
            Stage.OnEveryRound -= OnEveryRound;

            StartCoroutine(FinalizeBattle(winningStance));

            IEnumerator FinalizeBattle(int win)
            {
                yield return new WaitForSeconds(2);
                _stickmanScene.ResetUi();
                foreach (var stance in Stances) stance.ResetUi();
                OnBattleResultCallback?.Invoke(win == 0);
                Player = null;
                Debug.Log("战斗结束！");
            }
        }
        public void BattleSetup((int, CombatUnit)[] roles, CombatManager.Judgment judgment)
        {
            var list = new List<CombatUnit>();
            Player = null;
            for (var index = 0; index < roles.Length; index++)
            {
                var (stance, combatUnit) = roles[index];
                if (index == 0) Player = combatUnit;
                combatUnit.SetStandingPoint(stance);
                list.Add(combatUnit);
                var stanceUi = Stances[stance];
                stanceUi.AddCombatUnit(combatUnit);
            }
            RoundText.text = string.Empty;
            if (Player == null)
                throw new NullReferenceException($"{nameof(BattleSetup)}:找不到玩家单位,游戏不支持无玩家的战斗模式!");
            Stage = new BattleStage(list.ToArray(), judgment, _minEscapeRounds);
            Stage.OnEveryRound += OnEveryRound;
            var maxBreath = list.Sum(c => c.BreathBar.TotalBreath);
            list.ForEach(c =>
            {
                BattleStatusBarController.AddUi(c.StandingPoint, c.CombatId,
                    ui => ui.Set(c.Name, c.Status));
            });
            _playerStrategyController.SetPlayer(Player, 
                PresetCombat, 
                PresetForce,
                PresetCancel);
            InitStickmans(list);
            BreathBarsInit();
            CombatTempoPlayer = new CombatTempoPlayer(this);
        }
        public void StartBattle()
        {
            if (Stage.IsFightEnd) //如果已有胜负(考虑到战斗单位不足问题)
            {
                OnBattleFinalize(Stage.WinningStance);
                return;
            } //直接返回胜利
            Stage.NextRound(true);
        }
        private void OnEveryRound(CombatRoundRecord rec)
        {
            _battleLogger.NextRound(rec);
            StartCoroutine(UpdateUis(rec));

            IEnumerator UpdateUis(CombatRoundRecord r)
            {
                RoundText.text = r.Round.ToString();
                yield return _breathUi.PlaySlider();
                yield return CombatTempoPlayer.Play(r);
                StartBattle();//自动回合
            }
        }
        //private void ManualRound()
        //{
        //    lastPlayerBreath = Player.BreathBar.TotalBreath;
        //    lastTargetBreath = Stage.GetCombatUnit(Player.Target.CombatId).BreathBar.TotalBreath;
        //    lastMaxBreath = Stage.GetAliveUnits().Sum(c => c.BreathBar.TotalBreath);
        //    UpdateLastBreath();
        //    Stage.NextRound(false);
        //    if (Stage.IsFightEnd)
        //    {
        //        OnBattleFinalize(Stage.WinningStance);
        //        return;
        //    }
        //    Stage.PrePlan();
        //    CurrentBreathUpdate();
        //}
        #endregion

        #region BreathViewUi Preset
        private void PresetCancel()
        {
            //BattleStatusBarController.UpdateStatus(Player.CombatId, Player.Status.Hp, Player.Status.Tp, Player.Status.Mp);
            //CurrentBreathUpdate();
        }
        private void PresetIdle()
        {
            //_breathView.SetIdle();
            //UpdateDrum(1, Stage.GetCombatUnits().Select(c => c.BreathBar.TotalBreath).Sum());
        }
        private void PresetForce(IForce force)
        {
            //_breathView.SetForce(form);
            //UpdateDrum(form.Breath, Stage.GetCombatUnits().Select(c => c.BreathBar.TotalBreath).Sum());
        }
        private void PresetCombat(ICombatForm form)
        {
            //_breathView.SetCombat(form);
            var status = Player.Status;
            //BattleStatusBarController.UpdateStatus(Player.CombatId,
            //    status.Hp.Value, status.Hp.Fix, status.Tp.Value - form.Tp,
            //    status.Tp.Fix, status.Mp.Value - form.Mp, status.Mp.Fix);
            var breathBar = Player.BreathBar;
            var dodgeBreath = breathBar.IsReposition ? breathBar.Dodge?.Breath : 0;
            //UpdateDrum(dodgeBreath + form.Breath, Stage.GetCombatUnits().Select(c => c.BreathBar.TotalBreath).Sum());
        }
        #endregion

        #region BreathView Update
        private void BreathBarsInit()
        {
            var combatUnits = Stage.GetAliveUnits().OrderBy(c => c.BreathBar.TotalBreath).ToList();
            var playerBreathBar = Player.BreathBar;
            var targetBreathBar = Stage.GetCombatUnit(Player.Target.CombatId).BreathBar;
            var playerBreath = playerBreathBar.TotalBreath;
            var maxBreath = combatUnits.Sum(c => c.BreathBar.TotalBreath);
            //UpdateDrum(playerBreath, maxBreath);
            _leftPromptEventUi.Set(GetTitle(playerBreathBar),playerBreath);
            _rightPromptEventUi.Set(GetTitle(targetBreathBar), targetBreathBar.TotalBreath);
            StartCoroutine(_breathUi.SetLeft(playerBreathBar));
            StartCoroutine(_breathUi.SetRight(targetBreathBar));
            string GetTitle(IBreathBar breathBar)
            {
                switch (breathBar.Plan)
                {
                    case CombatPlans.Attack:
                        return breathBar.Combat?.Name ?? string.Empty;
                    case CombatPlans.RecoverHp:
                    case CombatPlans.RecoverTp:
                        return breathBar.Force?.Name ?? string.Empty;
                    case CombatPlans.Exert:
                        return "运功";
                    case CombatPlans.Wait:
                        return "等待";
                    case CombatPlans.Surrender:
                        return "伺机逃跑...";
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        //private void UpdateDrum(int playerBreath,int maxBreath)
        //{
        //    var targetBreath = Stage.GetCombatUnit(Player.Target.CombatId).BreathBar.TotalBreath;
        //    _breathUi.UpdateDrum(playerBreath, targetBreath, maxBreath);
        //}
        #endregion

        #region CombatUi Update
        private IEnumerable<CombatUnitUi> GetAllCombatUis() => Stances.SelectMany(s => s.CombatUnitUis);
        private CombatUnitUi GetCombatUi(int combatId) => GetAllCombatUis().Single(c => c.CombatId == combatId);
        private void InitStickmans(List<CombatUnit> list)
        {
            var uis = GetAllCombatUis().ToArray();
            Stickman player = null;
            foreach (var combat in list.OrderBy(c=>c.Position))
            {
                var stickman = uis.Single(c => c.CombatId == combat.CombatId).Stickman;
                if (combat.CombatId == 1)
                {
                    player = stickman;
                    _stickmanScene.PlaceCenter(stickman);
                    continue;
                }
                var target = uis.Single(c => c.CombatId == combat.Target.CombatId).Stickman;
                _stickmanScene.AutoPlace(stickman, target, combat, false, StickmanSceneController.Placing.Right);
            }

            _stickmanScene.Centralize(player);
            UpdateStickmanOrientation();
        }
        private void UpdateStickmanOrientation()
        {
            foreach (var unit in Stage.GetCombatUnits())
            {
                var stickman = GetCombatUi(unit.CombatId).Stickman;
                var target = GetCombatUi(unit.Target.CombatId).Stickman;
                _stickmanScene.UpdateStickmanOrientation(stickman, target);
            }
        }
        private BattleStanceUi GetCombatAnimUi(int combatId) => Stances.Single(s => s.HandledIds.Contains(combatId));
        private static string GetEventName(SubEventRecord eve)
        {
            return eve.Type switch
            {
                SubEventRecord.EventTypes.None => string.Empty,
                SubEventRecord.EventTypes.Death => "死亡!",
                _ => throw new ArgumentOutOfRangeException()
            };
            //{
            //    FightFragment.Types.None => string.Empty,
            //    FightFragment.Types.Consume => string.Empty,
            //    FightFragment.Types.Attack => string.Empty,
            //    FightFragment.Types.Parry => string.Empty,
            //    FightFragment.Types.Dodge => string.Empty,
            //    FightFragment.Types.Position => string.Empty,
            //    FightFragment.Types.SwitchTarget => string.Empty,
            //    FightFragment.Types.TryEscape => "欲想逃跑...",
            //    FightFragment.Types.Fling => "投掷暗器!",
            //    FightFragment.Types.Escaped => "逃走了!",
            //    FightFragment.Types.Death => "死亡!",
            //    FightFragment.Types.Exhausted => "败!",
            //    FightFragment.Types.Wait => "伺机行动...",
            //    _ => throw new ArgumentOutOfRangeException()
            //};
        }
        #endregion
        public override void ResetUi() => Hide();

        #region ICombatFragment
        public IEnumerator OnReposAnim(PositionRecord pos)
        {
            var unitId = pos.Unit.CombatId;
            var stickman = GetCombatAnimUi(unitId).GetStickman(unitId);
            var tarId = pos.Target.CombatId;
            var target = GetCombatAnimUi(tarId).GetStickman(tarId);
            GetCombatAnimUi(pos.Unit.CombatId)
                .SetAction(pos.Unit.CombatId, Stickman.Anims.Dodge);
            var combatUnit = Stage.GetCombatUnit(unitId);
            var isComplete = false;
            _stickmanScene.AutoPlace(stickman, target, combatUnit, unitId == Player.CombatId,
                StickmanSceneController.Placing.Random, () =>
                {
                    isComplete = true;
                    UpdateStickmanOrientation();
                });
            yield return new WaitUntil(() => isComplete);
        }

        public void OnStatusUpdate(IStatusRecord before, IStatusRecord after) => UpdateStatusWithDifPop(before, after);
        public void OnAttackAnim(AttackRecord att)
        {
            GetPromptEventUi(att.CombatId).UpdateEvent(PromptEventUi.CombatEvents.Attack);
            GetCombatAnimUi(att.Unit.CombatId)
                .SetAction(att.Unit.CombatId, Stickman.Anims.Attack);
            var status = att.AttackConsume.After;
            UpdateStatusUi(att.CombatId, status.Hp, status.Tp, status.Mp);
        }

        public void OnEscapeAnim(EscapeRecord esc)
        {
            GetPromptEventUi(esc.Attacker.CombatId).UpdateEvent(PromptEventUi.CombatEvents.Attack);
            GetCombatAnimUi(esc.Attacker.CombatId).SetAction(esc.Attacker.CombatId, Stickman.Anims.Attack);
        }

        private PromptEventUi GetPromptEventUi(int combatId)
        {
            var isPlayer = Player.CombatId == combatId;
            var comEvent = isPlayer ? _leftPromptEventUi : _rightPromptEventUi;
            return comEvent;
        }

        public void OnSufferAnim(ConsumeRecord rec)
        {
            var combatId = rec.CombatId;
            var status = rec.After;
            GetPromptEventUi(combatId).UpdateEvent(PromptEventUi.CombatEvents.Suffer);
            GetCombatAnimUi(combatId).SetAction(combatId, Stickman.Anims.Suffer);
            UpdateStatusUi(combatId, status.Hp, status.Tp, status.Mp);
        }

        public void OnDodgeAnim(ConsumeRecord<IDodge> rec)
        {
            var combatId = rec.CombatId;
            var status = rec.After;
            GetPromptEventUi(combatId).UpdateEvent(PromptEventUi.CombatEvents.Dodge);
            GetCombatAnimUi(combatId).SetAction(combatId, Stickman.Anims.Dodge);
            UpdateStatusUi(combatId, status.Hp, status.Tp, status.Mp);
        }
        public void OnParryAnim(ConsumeRecord<IParryForm> rec)
        {
            var combatId = rec.CombatId;
            var status = rec.After;
            GetPromptEventUi(combatId).UpdateEvent(PromptEventUi.CombatEvents.Parry);
            GetCombatAnimUi(combatId).SetAction(combatId, Stickman.Anims.Parry);
            UpdateStatusUi(combatId, status.Hp, status.Tp, status.Mp);
        }

        public void OnOtherEventAnim(SubEventRecord eve)
        {
            switch (eve.Type)
            {
                case SubEventRecord.EventTypes.Death:
                    GetCombatAnimUi(eve.CombatId).SetAction(eve.CombatId, Stickman.Anims.Death);
                    break;
                case SubEventRecord.EventTypes.None:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void OnReCharge(RechargeRecord charge)
        {
            var status = charge.Consume.After;
            UpdateStatusUi(charge.CombatId, status.Hp, status.Tp, status.Mp);
        }

        public IEnumerator FullRecord(CombatRoundRecord rec)
        {
            var listCo = new List<Coroutine>();
            void StartCo(IEnumerator enumerator) => listCo.Add(StartCoroutine(enumerator));

            if (rec.ExecutorId == -1)
            {
                foreach (var bar in rec.BreathBars) StartCo(BarUpdate(bar, Player.CombatId == bar.CombatId));
                foreach (var co in listCo)
                    yield return co;
                yield break;
            }

            var exeBar = rec.BreathBars.Single(b => b.CombatId == rec.ExecutorId);
            var tarBar = rec.BreathBars.SingleOrDefault(b => b.CombatId == rec.TargetId);
            var maxBreath = exeBar.GetBreath() + tarBar?.GetBreath() ?? 0;
            StartCo(BarUpdate(exeBar, rec.ExecutorId == Player.CombatId));
            if (tarBar != null) StartCo(BarUpdate(tarBar, rec.TargetId == Player.CombatId));

            foreach (var co in listCo) yield return co;

            _breathUi.UpdateDrum(exeBar.GetBreath(), tarBar?.GetBreath() ?? 0, maxBreath);

            IEnumerator BarUpdate(BreathBarRecord bar, bool isPlayer)
            {
                UpdateStatusUi(bar.CombatId, bar.Unit.Hp, bar.Unit.Tp, bar.Unit.Mp);
                var popEventUi = GetPromptEventUi(bar.CombatId);
                popEventUi.Set(bar.Title, bar.GetBreath());
                var busy = bar.Breathes.FirstOrDefault(b => b.Type == BreathRecord.Types.Busy)?.Value ?? 0;
                var charge = bar.Breathes.FirstOrDefault(b => b.Type == BreathRecord.Types.Charge)?.Value ?? 0;
                var exert = bar.Breathes.FirstOrDefault(b => b.Type == BreathRecord.Types.Exert);
                var attack = bar.Breathes.FirstOrDefault(b => b.Type == BreathRecord.Types.Attack);
                var placing = bar.Breathes.FirstOrDefault(b => b.Type == BreathRecord.Types.Placing);
                //UpdateDrum();
                if (isPlayer)
                    yield return _breathUi.SetLeft(bar.Plan, busy, charge, attack, placing, exert);
                else yield return _breathUi.SetRight(bar.Plan, busy, charge, attack, placing, exert);
            }
        }

        private void UpdateStatusWithDifPop(IStatusRecord before, IStatusRecord after)
        {
            var stanceUi = GetCombatAnimUi(before.CombatId);
            stanceUi.PopStatus(before.CombatId,
                after.Hp.Value - before.Hp.Value,
                after.Tp.Value - before.Tp.Value,
                after.Mp.Value - before.Mp.Value
            );
            BattleStatusBarController.UpdateStatus(after.CombatId, after.Hp, after.Tp, after.Mp);
        }

        private void UpdateStatusUi(int combatId, IConditionValue hp, IConditionValue tp, IConditionValue mp)
        {
            var combatUi = GetCombatUi(combatId);
            combatUi.UpdateTextUi(hp, mp, tp);
            BattleStatusBarController.UpdateStatus(combatId, hp, tp, mp);
        }

        //public void OnFragmentUpdate()
        //{
        //    foreach (var stance in Stances) stance.UpdateUi();
        //}
        #endregion
    }
    /// <summary>
    /// 战斗片段实现接口，用于<see cref="CombatTempoPlayer"/>作为节奏控制器控制战斗各种事件触发时机
    /// </summary>
    public interface ICombatFragmentPlayer
    {
        /// <summary>
        /// 移位触发
        /// </summary>
        /// <param name="pos"></param>
        IEnumerator OnReposAnim(PositionRecord pos);
        /// <summary>
        /// 状态更新器,保证与任何事件的节奏同步更新
        /// </summary>
        /// <param name="before"></param>
        /// <param name="after"></param>
        void OnStatusUpdate(IStatusRecord before, IStatusRecord after);
        /// <summary>
        /// 攻击触发
        /// </summary>
        /// <param name="att"></param>
        void OnAttackAnim(AttackRecord att);
        void OnEscapeAnim(EscapeRecord esc);
        /// <summary>
        /// 承受触发
        /// </summary>
        void OnSufferAnim(ConsumeRecord suf);
        /// <summary>
        /// 闪避触发
        /// </summary>
        void OnDodgeAnim(ConsumeRecord<IDodge> rec);
        /// <summary>
        /// 招架触发
        /// </summary>
        void OnParryAnim(ConsumeRecord<IParryForm> rec);
        /// <summary>
        /// 特殊事件触发，包括逃逸，死亡，战败等...
        /// </summary>
        /// <param name="eve"></param>
        void OnOtherEventAnim(SubEventRecord eve);
        /// <summary>
        /// 切换目标
        /// </summary>
        /// <param name="swi"></param>
        //void OnSwitchTarget(SwitchTargetRecord swi);
        /// <summary>
        /// 息恢复
        /// </summary>
        /// <param name="charge"></param>
        void OnReCharge(RechargeRecord charge);

        /// <summary>
        /// 总记录
        /// </summary>
        /// <param name="bar"></param>
        IEnumerator FullRecord(CombatRoundRecord bar);
    }
}