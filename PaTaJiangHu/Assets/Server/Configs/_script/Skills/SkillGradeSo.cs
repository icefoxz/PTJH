using System;
using System.Linq;
using MyBox;
using UnityEngine;
using Utls;

namespace Server.Configs.Skills
{
    public abstract class SkillGradeSo<T> : ScriptableObject where T : ScriptableObject
    {
        [SerializeField] private WeightField[] _list;
        private WeightField[] List => _list;

        public T PickSkill()
        {
            if (List.Any(s => s == null)) throw new Exception($"{nameof(SkillFieldSo)}.{name}: 技能列表中有空的技能");
            if (List.Length == 0) throw new Exception($"{nameof(SkillFieldSo)}.{name}: 技能列表中没有技能");
            return List.WeightPick().FieldSo;
        }

        [Serializable] public class WeightField : IWeightElement
        {
            private bool Rename()
            {
                if (FieldSo == null) return false;
                _name = $"{FieldSo.name} : {Weight}";
                return true;
            }

            [ConditionalField(true,nameof(Rename))][SerializeField][ReadOnly] private string _name;
            [SerializeField] private int 权重 = 1;
            [SerializeField] private T 技能;

            public int Weight => 权重;
            public T FieldSo => 技能;
        }
    }
}