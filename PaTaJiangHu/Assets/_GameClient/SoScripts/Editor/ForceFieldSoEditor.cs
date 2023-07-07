#if UNITY_EDITOR
using GameClient.SoScripts.Skills;
using UnityEditor;

namespace GameClient.SoScripts.Editor
{
    [CustomEditor(typeof(ForceFieldSo))]
    public class ForceFieldSoEditor : SkillFieldSOEditor
    {
    }
}
#endif