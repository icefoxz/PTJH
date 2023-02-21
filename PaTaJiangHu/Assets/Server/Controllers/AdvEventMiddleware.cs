using System;
using _GameClient.Models;
using Server.Configs.Adventures;
using Server.Configs.BattleSimulation;

namespace Server.Controllers
{
    /// <summary>
    /// 历练事件处理器中间件, 子类处理事件的交互处理,并给出<see cref="IAdvArg"/>
    /// </summary>
    internal abstract class AdvEventMiddlewareBase : IAdvEventMiddleware
    {
        public AdvEventMiddlewareBase(BattleSimulatorConfigSo simulator, ConditionPropertySo cfg)
        {
            Simulator = simulator;
            Cfg = cfg;
        }

        protected BattleSimulatorConfigSo Simulator { get; }
        protected ConditionPropertySo Cfg { get; }
        public abstract IAdvArg Invoke(IAdvEvent advEvent, IRewardHandler rewardHandler, Dizi dizi);
    }

    /// <summary>
    /// 自动事件处理器(其中包括历练与闲置状态的处理)
    /// </summary>
    internal class AdvEventMiddleware : AdvEventMiddlewareBase
    {
        public AdvEventMiddleware(BattleSimulatorConfigSo simulator, ConditionPropertySo cfg) : base(simulator, cfg)
        {
        }

        public override IAdvArg Invoke(IAdvEvent advEvent, IRewardHandler rewardHandler, Dizi dizi)
        {
            var arg = new AdvArg(dizi, rewardHandler);
            switch (advEvent.AdvType)
            {
                case AdvTypes.Option:
                case AdvTypes.Battle:
                    throw new NotSupportedException($"历练不支持事件={advEvent.AdvType}!");
                case AdvTypes.Story:
                case AdvTypes.Dialog:
                case AdvTypes.Pool:
                case AdvTypes.Term:
                case AdvTypes.Adjust:
                case AdvTypes.Reward: break; //其余的直接执行判断
                case AdvTypes.Simulation: //执行模拟战斗
                    if (advEvent is not BattleSimulationEventSo bs)
                        throw new NotImplementedException($"{advEvent.name} 事件类型错误!");
                    var diziSim = Cfg.GetSimulation(dizi.Name, dizi.Strength, dizi.Agility,
                        dizi.WeaponPower, dizi.ArmorPower,
                        dizi.CombatSkill.Grade, dizi.CombatSkill.Level,
                        dizi.ForceSkill.Grade, dizi.ForceSkill.Level,
                        dizi.DodgeSkill.Grade, dizi.DodgeSkill.Level);
                    var npc = bs.GetNpc(Cfg);
                    var outcome = Simulator.CountSimulationOutcome(diziSim, npc);
                    var staminaController = Game.Controllers.Get<StaminaController>();
                    if (outcome.IsPlayerWin && dizi.Stamina.Con.Value >= outcome.Result)
                    {
                        staminaController.ConsumeStamina(dizi.Guid, -outcome.Result);
                    }
                    else //当弟子战斗失败,或是血量不够扣除
                    {
                        staminaController.SetStaminaZero(dizi.Guid, true);
                    }

                    arg.SetSimulationOutcome(outcome);
                    break;
                case AdvTypes.Quit: //结束事件由故事处理器处理
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return arg;
        }
    }
}