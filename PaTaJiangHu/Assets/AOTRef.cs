using System;
using System.Collections.Generic;
using System.Linq;
using AOT._AOT.Core;
using AOT._AOT.Utls;
using GameClient.Args;
using GameClient.Models;
using GameClient.Modules.BattleM;
using GameClient.SoScripts.Adventures;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class AOTRef : MonoBehaviour
{
    public void AotReferences()
    {
        var ob = ObjectBag.Serialize(new int[0]);
        var b = ObjectBag.DeSerialize(ob);
        _ = b.Get<int[]>(0);
    }

    public void AotGenericReferences()
    {
        IStacking<object> _11 = default;
        DiziSkill.SkillMap<ISkill> _12 = default;
        CombatResponseInfo<DiziCombatUnit,DiziCombatInfo> _13 = default;
        CombatUnitInfo<DiziCombatUnit> _14 = default;
        ISkillMap<ISkillInfo> _15 = default;
        Action<SkillType,int> _16 = default;
        Action<ValueTuple<int,int,object>> _17 = default;
        Action<int> _18 = default;
        Action<object,SkillType,int> _19 = default;
        Action<object,IAdjustment.Types> _21 = default;
        Action<object,int,int> _22 = default;
        Action<object,int> _23 = default;
        Action<object,object> _24 = default;
        Action<object> _25 = default;
        Dictionary<object,byte> _26 = default;
        Dictionary<object,object> _27 = default;
        ICollection<object> _28 = default;
        IEnumerable<object> _29 = default;
        IEnumerator<object> _30 = default;
        IReadOnlyCollection<object> _31 = default;
        IReadOnlyList<object> _32 = default;
        KeyValuePair<object,byte> _33 = default;
        List<IEnumerator<int>> _34 = default;
        List<IEnumerator<object>> _35 = default;
        List<ValueTuple<int,int>> _36 = default;
        List<ValueTuple<object,int>> _37 = default;
        List<ValueTuple<object,object,int,int,int>> _38 = default;
        List<int> _39 = default;
        List<object> _40 = default;
        Func<KeyValuePair<object,byte>,byte> _41 = default;
        Func<KeyValuePair<object,byte>,int> _42 = default;
        Func<byte> _43 = default;
        Func<int,object,object> _44 = default;
        Func<object,ValueTuple<int,object,object>> _45 = default;
        Func<object,byte> _46 = default;
        Func<object,object> _47 = default;
        Func<object> _48 = default;
        IGrouping<int,KeyValuePair<object,byte>> _49 = default;
        ValueTuple<int,int,int,int> _50 = default;
        ValueTuple<int,int,object> _51 = default;
        ValueTuple<int,int> _52 = default;
        ValueTuple<int,object,object> _53 = default;
        ValueTuple<object,Color> _54 = default;
        ValueTuple<object,int> _55 = default;
        ValueTuple<object,object,int,int,int> _56 = default;
        ValueTuple<object, object> _57 = default;
    }

    public void AotMethods()
    {
    }
}
