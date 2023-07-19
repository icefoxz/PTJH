using System;
using AOT.Views.Abstract;
using GameClient.GameScene.Animators;
using UnityEngine;

namespace GameClient.SoScripts.GameVisualize
{
    [CreateAssetMenu(fileName = "战斗演示攻击配置", menuName = "战斗/单位/攻击配置")]
    internal class DiziCombatVisualConfigSo : ScriptableObject
    {
        [SerializeField] private DiziCombatVisualResponseCfgSo 反馈配置;
        [SerializeField] private DiziCombatConfig 弟子攻击配置;
        [SerializeField] private UiEffectField[] Ui特效配置;

        public DiziCombatVisualResponseCfgSo CombatVisualResponseSo => 反馈配置;
        private DiziCombatConfig CombatConfig => 弟子攻击配置;
        public UiEffectField[] UiEffectFields => Ui特效配置;
        public BasicAnimConfig OffendReturnCurve => CombatConfig.OffendReturnCurve;
        public BasicAnimConfig OffendMovingCurve => CombatConfig.OffendMovingCurve;


        /// <summary>
        /// 弟子战斗攻击配置
        /// </summary>
        [Serializable] private class DiziCombatConfig
        {
            [SerializeField] private BasicAnimConfig 战斗移动配置;
            [SerializeField] private BasicAnimConfig 战斗返回配置;
            internal BasicAnimConfig OffendMovingCurve => 战斗移动配置;
            internal BasicAnimConfig OffendReturnCurve => 战斗返回配置;
        }

        [Serializable] internal class BasicAnimConfig
        {
            [SerializeField] private AnimationCurve 曲线;
            [SerializeField] private float 耗时;
            public float Evaluate(float elapsedTime) => 曲线.Evaluate(elapsedTime);
            public float Duration => 耗时;
        }

        [Serializable]internal class UiEffectField
        {
            [SerializeField] private DiziCombatAnimator.Events 事件;
            [SerializeField] private EffectView[] Ui特效;
            [SerializeField] private float 持续秒 = 0.5f;
            public EffectView[] Effects => Ui特效;
            public DiziCombatAnimator.Events Event => 事件;
            public float Lasting => 持续秒;
        }
    }
}