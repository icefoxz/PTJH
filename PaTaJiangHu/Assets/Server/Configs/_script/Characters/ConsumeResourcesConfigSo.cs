using System.Linq;
using UnityEngine;
using Utls;

namespace Server.Configs.Characters
{
    [CreateAssetMenu(fileName = "ConsumeResourcesConfig", menuName = "资质/弟子消耗资源配置策略")]
    internal class ConsumeResourcesConfigSo : ScriptableObject
    {
        [SerializeField] private int 最低值 = 20;
        [SerializeField] private int 最大值 = 100;
        [SerializeField] private int 总值 = 400;

        private int Min => 最低值;
        private int Max => 最大值;
        private int Sum => 总值;

        public int[] GetRandomElements() =>
            Sys.RandomElementValue(5, Min, Max, Sum).OrderBy(_ => Sys.Random.Next(100)).ToArray();
    }
}