using System.IO;
using UnityEditor;
using UnityEngine;
using Utls;

namespace Systems
{
    /// <summary>
    /// 资源管理器
    /// </summary>
    public class ResMgr : UnitySingleton<ResMgr>
    {
        public const string AssetsPackagePath = "Assets/AssetsPackage/";
        public T GetAssetCache<T>(string fileName) where T : Object
        {
            T obj = null;
#if UNITY_EDITOR
            var path = Path.Combine(AssetsPackagePath, fileName);
            obj = AssetDatabase.LoadAssetAtPath<T>(path);
#endif
            return obj;
        }
    }
}