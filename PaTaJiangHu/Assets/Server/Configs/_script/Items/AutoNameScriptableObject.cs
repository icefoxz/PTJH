using Data;
using MyBox;
using UnityEditor;
using UnityEngine;

namespace Server.Configs._script.Items
{
    /// <summary>
    /// 自动文件名SO
    /// </summary>
    public class AutoNameScriptableObject : ScriptableObject, IDataElement
    {
        [SerializeField] private string _name;
        [ConditionalField(true,nameof(ChangeName))][SerializeField] private int id;

        protected bool ChangeName()
        {
            var path = AssetDatabase.GetAssetPath(this);
            var newName = $"{id}_{_name}";
            var err = AssetDatabase.RenameAsset(path, newName);

            if (!string.IsNullOrWhiteSpace(err)) Debug.LogError(err);
            return true;
        }

        public int Id => id;
        public string Name => _name;
    }
}