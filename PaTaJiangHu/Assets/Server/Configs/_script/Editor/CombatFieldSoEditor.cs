#if UNITY_EDITOR
using Server.Configs.Skills;
using UnityEditor;

[CustomEditor(typeof(CombatFieldSo))]
public class CombatFieldSoEditor : SkillFieldSOEditor
{
}
#endif