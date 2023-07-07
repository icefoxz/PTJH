#if UNITY_EDITOR
using GameClient.SoScripts.Skills;
using UnityEditor;

namespace GameClient.SoScripts.Editor
{
    [CustomEditor(typeof(CombatFieldSo))]
    public class CombatFieldSoEditor : SkillFieldSOEditor
    {
    }
}
#endif