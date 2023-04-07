using System;
using System.Collections;
using UnityEngine;

namespace Views
{
    /// <summary>
    /// 特效Ui,用于显示特效例:角色血条, 伤害数字, 伤害特效等<br/>
    /// 配合<see cref="Game.BattleCache"/>来传输战斗信息
    /// </summary>
    public class EffectView : View
    {
        /// <summary>
        /// 当事件特效调用的时候触发
        /// </summary>
        public static event Action<(int performerId, int performIndex, EffectView view, RectTransform target)> OnInvoke;

        /// <summary>
        /// 战斗事件特效, 用于配置调用
        /// </summary>
        /// <param name="performerId"></param>
        /// <param name="performIndex"></param>
        /// <param name="effectView"></param>
        /// <param name="target"></param>
        /// <param name="lasting"></param>
        internal static void Instance(int performerId, int performIndex, EffectView effectView, RectTransform target,
            float lasting)
        {
            OnInvoke?.Invoke((performerId, performIndex, effectView, target));
            effectView.StartCo(WaitForLasting());

            IEnumerator WaitForLasting()
            {
                yield return new WaitForSeconds(lasting);
                effectView.DestroyObj();
            }
        }

        private void DestroyObj() => Destroy(gameObject);
    }
}