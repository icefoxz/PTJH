using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BattleM;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Visual.BaseUi;
using Visual.BattleUi.Input;
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
        //[SerializeField] private BreathViewUi _breathView;
        //[SerializeField] private BattleOrderController _battleOrderController;
        [SerializeField] private BattleStatusBarController _battleStatusBarController;
        [SerializeField] private BreathUiController _breathUi;

        //[SerializeField] private FightStage[] Stages;
        //private IBattleOrderController BattleOrderController => _battleOrderController;
        private IBattleStatusBarController BattleStatusBarController => _battleStatusBarController;
        private BattleStanceUi[] Stances { get; set; }
        public BattleStage Stage { get; set; }
        private CombatTempoController TempoController { get; set; }
        private CombatUnit Player { get; set; }
        private bool IsAutoRound { get; set; } = true;
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
            //BattleOrderController.Init();
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
            throw new NotImplementedException();
        }

        private void PlayerRecHp()
        {
            throw new NotImplementedException();
        }

        private void PlayerWaitPlan()
        {
            Player.WaitPlan();
            CurrentBreathUpdate();
            ManualRound();
        }

        private void PlayerExertPlan(IForceForm force)
        {
            Player.ExertPlan(force);
            CurrentBreathUpdate();
            ManualRound();
        }

        private void PlayerAttackPlan(ICombatForm combat)
        {
            Player.AttackPlan(combat, Player.BreathBar.Dodge);
            CurrentBreathUpdate();
            ManualRound();
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
            //BattleOrderController.UpdateOrder(list.Select(c => (c.CombatId, c.Name, c.BreathBar.TotalBreath)).ToArray());
            _playerStrategyController.SetPlayer(Player, 
                PresetCombat, 
                PresetForce,
                PresetCancel);
            InitStickmans(list);
            CurrentBreathUpdate();
            TempoController = new CombatTempoController(this);
            PresetCancel();
        }
        public void StartBattle()
        {
            if (Stage.IsFightEnd) //如果已有胜负(考虑到战斗单位不足问题)
            {
                OnBattleFinalize(Stage.WinningStance);
                return;
            } //直接返回胜利

            if (!IsAutoRound)
            {
                Stage.PrePlan();
                CurrentBreathUpdate();
                return;
            }
            Stage.NextRound(true);
        }
        private void OnEveryRound(FightRoundRecord rec)
        {
            _battleLogger.NextRound(Stage.GetCombatUnits(), rec);
            StartCoroutine(UpdateUis(rec));

            IEnumerator UpdateUis(FightRoundRecord r)
            {
                RoundText.text = r.Round.ToString();
                CurrentBreathUpdate();
                yield return _breathUi.PlaySlider(() => { });
                yield return TempoController.Play(r);
                StartBattle();//自动回合
            }
        }
        private void ManualRound()
        {
            lastPlayerBreath = Player.BreathBar.TotalBreath;
            lastTargetBreath = Stage.GetCombatUnit(Player.Target.CombatId).BreathBar.TotalBreath;
            lastMaxBreath = Stage.GetAliveUnits().Sum(c => c.BreathBar.TotalBreath);
            UpdateLastBreath();
            Stage.NextRound(false);
            if (Stage.IsFightEnd)
            {
                OnBattleFinalize(Stage.WinningStance);
                return;
            }
            Stage.PrePlan();
            CurrentBreathUpdate();
        }
        #endregion

        #region BreathViewUi Preset
        private void PresetCancel()
        {
            BattleStatusBarController.UpdateStatus(Player.CombatId, Player.Status.Hp, Player.Status.Tp,
                Player.Status.Mp);
            CurrentBreathUpdate();
        }
        private void PresetIdle()
        {
            //_breathView.SetIdle();
            UpdateDrum(1, Stage.GetCombatUnits().Select(c => c.BreathBar.TotalBreath).Sum());
        }
        private void PresetForce(IForceForm form)
        {
            //_breathView.SetForce(form);
            UpdateDrum(form.Breath, Stage.GetCombatUnits().Select(c => c.BreathBar.TotalBreath).Sum());
        }
        private void PresetCombat(ICombatForm form)
        {
            //_breathView.SetCombat(form);
            var status = Player.Status;
            BattleStatusBarController.UpdateStatus(Player.CombatId,
                status.Hp.Value, status.Hp.Fix, status.Tp.Value - form.Qi,
                status.Tp.Fix, status.Mp.Value - form.Mp, status.Mp.Fix);
            var dodgeBreath = Player.BreathBar.Dodge?.Breath ?? 0;
            UpdateDrum(dodgeBreath + form.Breath,
                Stage.GetCombatUnits().Select(c => c.BreathBar.TotalBreath).Sum());
        }
        #endregion

        #region BreathView Update
        private int lastPlayerBreath;
        private int lastTargetBreath;
        private int lastMaxBreath;

        private void UpdateLastBreath()
        {
            _breathUi.UpdateDrum(lastPlayerBreath, lastTargetBreath, lastMaxBreath);
        }

        private void CurrentBreathUpdate()
        {
            var combatUnits = Stage.GetAliveUnits().OrderBy(c => c.BreathBar.TotalBreath).ToList();
            var playerBreath = Player.BreathBar.TotalBreath;
            var maxBreath = combatUnits.Sum(c => c.BreathBar.TotalBreath);
            UpdateDrum(playerBreath, maxBreath);
            _breathUi.SetBreathView(Player.BreathBar, Stage.GetCombatUnit(Player.Target.CombatId).BreathBar);
        }
        private void UpdateDrum(int playerBreath,int maxBreath)
        {
            var targetBreath = Stage.GetCombatUnit(Player.Target.CombatId).BreathBar.TotalBreath;
            _breathUi.UpdateDrum(playerBreath, targetBreath, maxBreath);
        }
        #endregion

        #region CombatUi Update
        private IEnumerable<CombatUnitUi> GetAllCombatUis() => Stances.SelectMany(s => s.CombatUnitUis);
        private CombatUnitUi GetCombatUi(int combatId) => GetAllCombatUis().Single(c => c.CombatId == combatId);
        private void InitStickmans(List<CombatUnit> list)
        {
            var uis = GetAllCombatUis().ToArray();
            foreach (var combat in list.OrderBy(c=>c.Position))
            {
                var stickman = uis.Single(c => c.CombatId == combat.CombatId).Stickman;
                stickman.Init(combat.CombatId);
                stickman.SetTarget(combat.Target.CombatId);
                if (combat.CombatId == 0)
                {
                    _stickmanScene.PlaceCenter(stickman);
                    continue;
                }

                var target = uis.Single(c => c.CombatId == combat.Target.CombatId).Stickman;
                _stickmanScene.AutoPlace(stickman, target, combat, false);
            }

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
        private void UpdateCombatUnit(int combatId, string popText)
        {
            var stanceUi = GetCombatAnimUi(combatId);
            if (!string.IsNullOrWhiteSpace(popText))
                stanceUi.PopText(combatId, popText);
        }
        private void UpdateStatus(IStatusRecord before,IStatusRecord after)
        {
            var stanceUi = GetCombatAnimUi(before.CombatId);
            stanceUi.PopStatus(before.CombatId,
                after.Hp.Value - before.Hp.Value,
                after.Tp.Value - before.Tp.Value,
                after.Mp.Value - before.Mp.Value
            );
            BattleStatusBarController.UpdateStatus(after.CombatId, after.Hp, after.Tp, after.Mp);
        }
        private static string GetEventName(IFightFragment eve)
        {
            return eve.Type switch
            {
                FightFragment.Types.None => string.Empty,
                FightFragment.Types.Consume => string.Empty,
                FightFragment.Types.Attack => string.Empty,
                FightFragment.Types.Parry => string.Empty,
                FightFragment.Types.Dodge => string.Empty,
                FightFragment.Types.Position => string.Empty,
                FightFragment.Types.SwitchTarget => string.Empty,
                FightFragment.Types.TryEscape => "欲想逃跑...",
                FightFragment.Types.Fling => "投掷暗器!",
                FightFragment.Types.Escaped => "逃走了!",
                FightFragment.Types.Death => "死亡!",
                FightFragment.Types.Exhausted => "败!",
                FightFragment.Types.Wait => "伺机行动...",
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        #endregion
        public override void ResetUi() => Hide();

        #region ICombatFragment
        public void OnConsumeAnim(ConsumeRecord con)
        {
            var popText = string.Empty;
            var combatId = con.Before.CombatId;
            popText = con switch
            {
                ConsumeRecord<IForceForm> force => force.Form.Name,
                ConsumeRecord<IDodgeForm> dodge => dodge.Form.Name,
                ConsumeRecord<ICombatForm> combat => combat.Form.Name,
                _ => popText
            };
            UpdateCombatUnit(combatId, popText);
        }
        public void OnReposAnim(PositionRecord pos)
        {
            var unitId = pos.Unit.CombatId;
            var stickman = GetCombatAnimUi(unitId).GetStickman(unitId);
            var tarId = pos.Target.CombatId;
            var target = GetCombatAnimUi(tarId).GetStickman(tarId);
            GetCombatAnimUi(pos.Unit.CombatId)
                .SetAction(pos.Unit.CombatId, Stickman.Anims.Dodge);
            var combatUnit = Stage.GetCombatUnit(unitId);
            _stickmanScene.AutoPlace(stickman, target, combatUnit, unitId == Player.CombatId, UpdateStickmanOrientation);
        }

        public void OnStatusUpdate(IStatusRecord before, IStatusRecord after) => UpdateStatus(before, after);
        public void OnAttackAnim(AttackRecord att)
        {
            GetCombatAnimUi(att.Unit.CombatId)
                .SetAction(att.Unit.CombatId, Stickman.Anims.Attack);
            UpdateCombatUnit(att.CombatId, $"攻-【{att.Form.Name}】:{att.DamageFormula.Finalize}");
        }
        public void OnSufferAnim(AttackRecord att) => GetCombatAnimUi(att.Target.Before.CombatId)
            .SetAction(att.Target.Before.CombatId, Stickman.Anims.Suffer);
        public void OnDodgeAnim(DodgeRecord dod)
        {
            GetCombatAnimUi(dod.CombatId).SetAction(dod.CombatId, Stickman.Anims.Dodge);
            UpdateCombatUnit(dod.CombatId, $"闪-【{dod.Form.Name}】");
        }
        public void OnParryAnim(ParryRecord par)
        {
            GetCombatAnimUi(par.CombatId).SetAction(par.CombatId, Stickman.Anims.Parry);
            UpdateCombatUnit(par.CombatId, $"架-【{par.Form.Name}】");
        }
        public void OnEventAnim(FightFragment eve) => UpdateCombatUnit(eve.CombatId, GetEventName(eve));
        public void OnSwitchTarget(SwitchTargetRecord swi)
        {
            var target = Stage.GetCombatUnit(swi.TargetId);
            UpdateCombatUnit(swi.CombatId,$"盯上了【{target.Name}】");
        }

        public void OnFragmentUpdate()
        {
            foreach (var stance in Stances) stance.UpdateUi();
        }
        #endregion
    }
    /// <summary>
    /// 战斗片段实现接口，用于<see cref="CombatTempoController"/>作为节奏控制器控制战斗各种事件触发时机
    /// </summary>
    public interface ICombatFragmentPlayer
    {
        /// <summary>
        /// 任何一种的状态消耗触发，但别用在状态更新。因为触发节奏与状态更新不一样<br/>
        /// 状态更新请用<see cref="OnStatusUpdate(IStatusRecord,IStatusRecord)"/>
        /// </summary>
        /// <param name="con"></param>
        void OnConsumeAnim(ConsumeRecord con);
        /// <summary>
        /// 移位触发
        /// </summary>
        /// <param name="pos"></param>
        void OnReposAnim(PositionRecord pos);
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
        /// <summary>
        /// 承受触发
        /// </summary>
        /// <param name="att"></param>
        void OnSufferAnim(AttackRecord att);
        /// <summary>
        /// 闪避触发
        /// </summary>
        /// <param name="dod"></param>
        void OnDodgeAnim(DodgeRecord dod);
        /// <summary>
        /// 招架触发
        /// </summary>
        /// <param name="par"></param>
        void OnParryAnim(ParryRecord par);
        /// <summary>
        /// 特殊事件触发，包括逃逸，死亡，战败等...
        /// </summary>
        /// <param name="eve"></param>
        void OnEventAnim(FightFragment eve);
        /// <summary>
        /// 切换目标
        /// </summary>
        /// <param name="swi"></param>
        void OnSwitchTarget(SwitchTargetRecord swi);
        /// <summary>
        /// 每个片段的更新，一般上用来直接更新状态Ui
        /// </summary>
        void OnFragmentUpdate();
    }
}