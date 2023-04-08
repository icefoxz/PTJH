using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Pool;

namespace Views
{
    public interface IEffectView : IView
    {
        /// <summary>
        /// 当特效调用的时候触发
        /// </summary>
        event Action<(int performerId, int performIndex, RectTransform target)> OnPlay;
        /// <summary>
        /// 当特效重置
        /// </summary>
        event Action OnReset;
    }
    /// <summary>
    /// 特效Ui,用于显示特效例:角色血条, 伤害数字, 伤害特效等<br/>
    /// 配合<see cref="Game.BattleCache"/>来传输战斗信息
    /// </summary>
    public class EffectView : View,IEffectView
    {
        private Vector3 DefaultLocalPos { get; set; }
        /// <summary>
        /// 当特效生成的时候
        /// </summary>
        public static event Action<IEffectView> OnInstance;
        /// <summary>
        /// 当特效调用的时候触发
        /// </summary>
        public event Action<(int performerId, int performIndex, RectTransform target)> OnPlay;
        /// <summary>
        /// 当特效重置
        /// </summary>
        public event Action OnReset;

        /// <summary>
        /// 战斗事件特效, 用于配置调用
        /// </summary>
        /// <param name="view"></param>
        internal static void Init(EffectView view)
        {
            view.DefaultLocalPos = view.transform.localPosition;
            OnInstance?.Invoke(view);
        }

        public void ResetPos() => transform.localPosition = DefaultLocalPos;

        /// <summary>
        /// 播放特效,并且根据lasting重置
        /// </summary>
        /// <param name="performerId"></param>
        /// <param name="performIndex"></param>
        /// <param name="target"></param>
        /// <param name="lasting"></param>
        public void PlayEffect(int performerId, int performIndex, RectTransform target, float lasting)
        {
            OnPlay?.Invoke((performerId, performIndex, target));
            StartCoroutine(WaitForLasting());

            IEnumerator WaitForLasting()
            {
                yield return new WaitForSeconds(lasting);
                OnReset?.Invoke();
            }
        }
    }
}