using System;
using MyBox;
using UnityEngine;
using Utls;

namespace Server.Configs._script.Skills
{
    public abstract class SkillGradeSo<T> : ScriptableObject where T : ScriptableObject
    {
        [SerializeField] private WeightField[] _list;
        private WeightField[] List => _list;

        public T PickSkill() => List.WeightPick().FieldSo;

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