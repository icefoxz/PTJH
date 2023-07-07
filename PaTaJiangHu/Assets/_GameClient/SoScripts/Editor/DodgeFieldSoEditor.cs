#if UNITY_EDITOR
using GameClient.SoScripts.Skills;
using UnityEditor;

namespace GameClient.SoScripts.Editor
{
    [CustomEditor(typeof(DodgeFieldSo))]
    public class DodgeFieldSoEditor : SkillFieldSOEditor
    {
    }
}
#endif