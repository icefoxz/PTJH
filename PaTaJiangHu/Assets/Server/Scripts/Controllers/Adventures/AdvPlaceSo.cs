using UnityEngine;

namespace Server.Controllers.Adventures
{
    public interface IAdvPlace
    {
        int Id { get; }
        string Name { get; }
        IAdvStorySo[] Stories { get; }
    }

    /// <summary>
    /// 副本地图
    /// </summary>
    [CreateAssetMenu(fileName = "id_地点名",menuName = "副本/地点")]
    internal class AdvPlaceSo : ScriptableObject, IAdvPlace
    {
        [SerializeField] private int _id;
        [SerializeField] private string _name;
        [SerializeField] private AdvStorySo[] 故事;

        public int Id => _id;
        public string Name => _name;
        public IAdvStorySo[] Stories => 故事;
    }
}