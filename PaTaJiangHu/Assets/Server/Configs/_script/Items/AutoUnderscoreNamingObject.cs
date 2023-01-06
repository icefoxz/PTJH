using Data;
using MyBox;
using UnityEditor;
using UnityEngine;

namespace Server.Configs.Items
{
    /// <summary>
    /// 自动文件名SO
    /// </summary>
    public class AutoUnderscoreNamingObject : ScriptableObject, IDataElement
    {
        private const char Underscore = '_';
        [SerializeField] protected string _name;
        [ConditionalField(true,nameof(ChangeName))][SerializeField] protected int id;

        protected bool ChangeName()
        {
            var path = AssetDatabase.GetAssetPath(this);
            var newName = string.Join(Underscore, id, _name);
            var err = AssetDatabase.RenameAsset(path, newName);

            if (!string.IsNullOrWhiteSpace(err)) Debug.LogError(err);
            return true;
        }

        public virtual int Id => id;
        public virtual string Name => _name;
    }
    public class AutoDashNamingObject : ScriptableObject, IDataElement
    {
        private const char Dash = '-';
        [SerializeField] protected string _name;
        [ConditionalField(true,nameof(ChangeName))][SerializeField] protected int id;

        protected bool ChangeName()
        {
            var path = AssetDatabase.GetAssetPath(this);
            var newName = string.Join(Dash, id, _name);
            var err = AssetDatabase.RenameAsset(path, newName);

            if (!string.IsNullOrWhiteSpace(err)) Debug.LogError(err);
            return true;
        }

        public virtual int Id => id;
        public virtual string Name => _name;
    }
}