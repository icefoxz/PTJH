using AOT.Views.Abstract;
using AOT.Views.BaseUis;
using GameClient.Args;
using GameClient.Modules.BattleM;
using GameClient.System;
using UnityEngine;
using UnityEngine.UI;

namespace HotUpdate._HotUpdate.UiEffects
{
    internal class Demo_game_charPopValue
    {
        private GameObject anim_pop { get; }
        private GameObject anim_blick { get; }
        private Text text_pop { get; }
        private Text text_blick { get; }

        private Vector3 BlinkDefaultPos { get; }

        public Demo_game_charPopValue(IEffectView v)
        {
            anim_pop = v.GetObject("anim_pop");
            anim_blick = v.GetObject("anim_blink");
            text_pop = v.GetObject<Text>("text_pop");
            text_blick = v.GetObject<Text>("text_blink");
            BlinkDefaultPos = anim_blick.transform.localPosition;
            v.OnPlay += OnPlay;
            v.OnReset += OnReset;
        }

        private void OnPlay((int performerId, int performIndex, RectTransform tran) arg)
        {
            var (performerId, performIndex, tran) = arg;
            var response = Game.BattleCache.GetLastResponse(performerId, performIndex);
            var damage = response.FinalDamage;
            text_pop.text = damage.ToString();
            anim_pop.SetActive(true);
            //if(response.IsDodged)
            {
                var flipAlign = response.Target.TeamId == 0 ? -1 : 1;
                var responseText = GetResponseText(response);
                text_blick.text = responseText;
                anim_blick.transform.SetLocalX(anim_blick.transform.localPosition.x * flipAlign);
                anim_blick.SetActive(true);
            }
        }

        private string GetResponseText(CombatResponseInfo<DiziCombatUnit, DiziCombatInfo> response)
        {
            return response.Target.IsDead ? "绝杀" : response.IsDodge ? "闪避" : string.Empty;
        }

        private void OnReset()
        {
            anim_pop.SetActive(false);
            anim_blick.SetActive(false);
            text_pop.text = string.Empty;
            text_blick.text = string.Empty;
            anim_blick.transform.localPosition = BlinkDefaultPos;
        }
    }
}