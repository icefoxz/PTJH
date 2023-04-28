#if UNITY_EDITOR
using Server.Configs.Skills;
using UnityEditor;

[CustomEditor(typeof(DodgeFieldSo))]
public class DodgeFieldSoEditor : SkillFieldSOEditor
{
}
#endif