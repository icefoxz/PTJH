using System.Linq;
using AOT._AOT.Utls;
using UnityEngine;

namespace GameClient.SoScripts.Characters
{
    [CreateAssetMenu(fileName = "ConsumeResourcesConfig", menuName = "弟子/产出/消耗资源配置策略")]
    internal class ConsumeResourcesConfigSo : ScriptableObject
    {
        [SerializeField] private int 最低值 = 20;
        [SerializeField] private int 最大值 = 100;
        [SerializeField] private int 总值 = 400;

        private int Min => 最低值;
        private int Max => 最大值;
        private int Sum => 总值;

        public int[] GetRandomElements() =>
            Sys.RandomElementValue(4, Min, Max, Sum)
                .OrderBy(_ => Sys.Random.Next(100)).ToArray();
    }
}