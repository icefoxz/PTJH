using GameClient.Modules.Data;
using MyBox;
using UnityEditor;
using UnityEngine;

namespace GameClient.SoScripts.Items
{
    /// <summary>
    /// 自动文件名SO
    /// </summary>
    public class AutoUnderscoreNamingObject : AutoNameSoBase
    {
        private const char Underscore = '_';
        protected override char Separator => Underscore;
    }
    public class AutoDashNamingObject : AutoNameSoBase
    {
        private const char Dash = '-';
        protected override char Separator => Dash;
    }
    public class AutoBacktickNamingObject : AutoNameSoBase
    {
        private const char Backtick = '`';
        protected override char Separator => Backtick;
    }
    public class AutoHashNamingObject : AutoNameSoBase
    {
        private const char Hash = '#';
        protected override char Separator => Hash;
    }
    public class AutoAtNamingObject : AutoNameSoBase
    {
        private const char At = '@';
        protected override char Separator => At;
    }

    public abstract class AutoNameSoBase : ScriptableObject, IDataElement
    {
        protected virtual string Prefix { get; }
        protected virtual string Suffix { get; }

        [ConditionalField(true, nameof(GetReference))][ReadOnly][SerializeField] private ScriptableObject _so;
        protected bool GetReference()
        {
#if UNITY_EDITOR
            if (_so == null) _so = this;
#endif
            return true;
        }

        public virtual int Id => id;
        public virtual string Name => _name;
        [SerializeField] protected string _name;

        [ConditionalField(true, nameof(ChangeName))] [SerializeField] protected int id;

        protected bool ChangeName()
        {
#if UNITY_EDITOR
            var path = AssetDatabase.GetAssetPath(this);
            var newName = string.Join(Separator, id, Prefix + _name + Suffix);
            var err = AssetDatabase.RenameAsset(path, newName);

            if (!string.IsNullOrWhiteSpace(err)) Debug.LogError(err);
#endif
            return true;
        }

        protected abstract char Separator { get; }
    }
}