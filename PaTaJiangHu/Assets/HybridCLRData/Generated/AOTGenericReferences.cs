using System.Collections.Generic;
public class AOTGenericReferences : UnityEngine.MonoBehaviour
{

	// {{ AOT assemblies
	public static readonly IReadOnlyList<string> PatchedAOTAssemblyList = new List<string>
	{
		"AOT.dll",
		"GameClient.dll",
		"System.Core.dll",
		"UnityEngine.CoreModule.dll",
		"mscorlib.dll",
	};
	// }}

	// {{ constraint implement type
	// }} 

	// {{ AOT generic types
	// AOT._AOT.Core.IStacking<object>
	// GameClient.Models.DiziSkill.SkillMap<object>
	// GameClient.Modules.BattleM.CombatResponseInfo<object,object>
	// GameClient.Modules.BattleM.CombatUnitInfo<object>
	// GameClient.Modules.BattleM.ISkillMap<object>
	// System.Action<GameClient.Modules.BattleM.SkillType,int>
	// System.Action<System.ValueTuple<int,int,object>>
	// System.Action<int>
	// System.Action<object,GameClient.Modules.BattleM.SkillType,int>
	// System.Action<object,GameClient.SoScripts.Adventures.IAdjustment.Types>
	// System.Action<object,int,int>
	// System.Action<object,int>
	// System.Action<object,object>
	// System.Action<object>
	// System.Collections.Generic.Dictionary<object,byte>
	// System.Collections.Generic.Dictionary<object,object>
	// System.Collections.Generic.ICollection<object>
	// System.Collections.Generic.IEnumerable<object>
	// System.Collections.Generic.IEnumerator<object>
	// System.Collections.Generic.IReadOnlyCollection<object>
	// System.Collections.Generic.IReadOnlyList<object>
	// System.Collections.Generic.KeyValuePair<object,byte>
	// System.Collections.Generic.List.Enumerator<int>
	// System.Collections.Generic.List.Enumerator<object>
	// System.Collections.Generic.List<System.ValueTuple<int,int>>
	// System.Collections.Generic.List<System.ValueTuple<object,int>>
	// System.Collections.Generic.List<System.ValueTuple<object,object,int,int,int>>
	// System.Collections.Generic.List<int>
	// System.Collections.Generic.List<object>
	// System.Func<System.Collections.Generic.KeyValuePair<object,byte>,byte>
	// System.Func<System.Collections.Generic.KeyValuePair<object,byte>,int>
	// System.Func<byte>
	// System.Func<int,object,object>
	// System.Func<object,System.ValueTuple<int,object,object>>
	// System.Func<object,byte>
	// System.Func<object,object>
	// System.Func<object>
	// System.Linq.IGrouping<int,System.Collections.Generic.KeyValuePair<object,byte>>
	// System.ValueTuple<int,int,int,int>
	// System.ValueTuple<int,int,object>
	// System.ValueTuple<int,int>
	// System.ValueTuple<int,object,object>
	// System.ValueTuple<object,UnityEngine.Color>
	// System.ValueTuple<object,int>
	// System.ValueTuple<object,object,int,int,int>
	// System.ValueTuple<object,object>
	// }}

	public void RefMethods()
	{
		// object AOT._AOT.Core.GameControllerServiceContainer.Get<object>()
		// object AOT._AOT.Utls.ObjectBag.Get<object>(int)
		// object AOT._AOT.Views.Abstract.IView.GetObject<object>(string)
		// System.ValueTuple<object,object>[] System.Array.Empty<System.ValueTuple<object,object>>()
		// object[] System.Array.Empty<object>()
		// bool System.Linq.Enumerable.All<object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,bool>)
		// bool System.Linq.Enumerable.Any<object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,bool>)
		// bool System.Linq.Enumerable.Contains<object>(System.Collections.Generic.IEnumerable<object>,object)
		// int System.Linq.Enumerable.Count<System.Collections.Generic.KeyValuePair<object,byte>>(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,byte>>)
		// object System.Linq.Enumerable.First<object>(System.Collections.Generic.IEnumerable<object>)
		// object System.Linq.Enumerable.FirstOrDefault<object>(System.Collections.Generic.IEnumerable<object>)
		// object System.Linq.Enumerable.FirstOrDefault<object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,bool>)
		// System.Collections.Generic.IEnumerable<System.Linq.IGrouping<int,System.Collections.Generic.KeyValuePair<object,byte>>> System.Linq.Enumerable.GroupBy<System.Collections.Generic.KeyValuePair<object,byte>,int>(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,byte>>,System.Func<System.Collections.Generic.KeyValuePair<object,byte>,int>)
		// System.Collections.Generic.IEnumerable<System.ValueTuple<int,object,object>> System.Linq.Enumerable.Select<object,System.ValueTuple<int,object,object>>(System.Collections.Generic.IEnumerable<object>,System.Func<object,System.ValueTuple<int,object,object>>)
		// System.ValueTuple<int,object,object>[] System.Linq.Enumerable.ToArray<System.ValueTuple<int,object,object>>(System.Collections.Generic.IEnumerable<System.ValueTuple<int,object,object>>)
		// object[] System.Linq.Enumerable.ToArray<object>(System.Collections.Generic.IEnumerable<object>)
		// System.Collections.Generic.List<object> System.Linq.Enumerable.ToList<object>(System.Collections.Generic.IEnumerable<object>)
		// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,byte>> System.Linq.Enumerable.Where<System.Collections.Generic.KeyValuePair<object,byte>>(System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,byte>>,System.Func<System.Collections.Generic.KeyValuePair<object,byte>,bool>)
		// System.Collections.Generic.IEnumerable<object> System.Linq.Enumerable.Where<object>(System.Collections.Generic.IEnumerable<object>,System.Func<object,bool>)
		// object System.Threading.Interlocked.CompareExchange<object>(object&,object,object)
		// object UnityEngine.Component.GetComponent<object>()
		// object UnityEngine.GameObject.GetComponent<object>()
		// object UnityEngine.Object.Instantiate<object>(object,UnityEngine.Transform)
	}
}