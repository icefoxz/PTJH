#if UNITY_EDITOR
using Server.Configs.Skills;
using UnityEditor;

[CustomEditor(typeof(ForceFieldSo))]
public class ForceFieldSoEditor : SkillFieldSOEditor
{
}
#endif