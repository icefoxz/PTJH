using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Views;

namespace HotFix_Project.UiEffects;

internal class Demo_game_charPopValue 
{
    private GameObject anim_pop { get; }
    private GameObject anim_blick { get; }
    private Text text_pop { get; }
    private Text text_blick { get; }
    public Demo_game_charPopValue(int performerId,int performIndex, EffectView v, RectTransform tran)
    {
        anim_pop = v.GetObject("anim_pop");
        anim_blick = v.GetObject("anim_blink");
        text_pop = v.GetObject<Text>("text_pop");
        text_blick = v.GetObject<Text>("text_blink");
        var response = Game.BattleCache.GetLastResponse(performerId, performIndex);
        var damage = response.FinalDamage;
        text_pop.text = damage.ToString();
        anim_pop.SetActive(true);
        if(response.IsDodged)
        {
            var responseText = GetResponseText(response);
            text_blick.text = responseText;
            anim_blick.SetActive(true);
        }
    }

    private string GetResponseText(CombatResponseInfo<DiziCombatUnit, DiziCombatInfo> response) => response.IsDodged?"闪避":string.Empty;
}