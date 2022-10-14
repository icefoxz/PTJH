#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using Systems;

[System.Reflection.Obfuscation(Exclude = true)]
public class IlRuntimeClrBinding
{

    [MenuItem("ILRuntime/通过自动分析热更DLL生成CLR绑定")]
    static void GenerateCLRBindingByAnalysis()
    {
        //用新的分析热更dll调用引用来生成绑定代码
        ILRuntime.Runtime.Enviorment.AppDomain domain = new ILRuntime.Runtime.Enviorment.AppDomain();
        using (System.IO.FileStream fs = new System.IO.FileStream(IlRuntimeManager.HotfixDllPath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
        {
            domain.LoadAssembly(fs);

            //Crossbind Adapter is needed to generate the correct binding code
            IlRuntimeManager.RegCLRBinding(domain);
            ILRuntime.Runtime.CLRBinding.BindingCodeGenerator.GenerateBindingCode(domain, IlRuntimeManager.GeneratedCode);
        }

        AssetDatabase.Refresh();
    }

    [MenuItem("ILRuntime/删除所有CLR绑定")]
    static void DeleteCLRBindins()
    {
        System.IO.Directory.Delete(IlRuntimeManager.GeneratedCode, true);
        AssetDatabase.Refresh();
    }

    [MenuItem("ILRuntime/生成跨域继承适配器")]
    static void GenerateCrossbindAdapter()
    {
        //由于跨域继承特殊性太多，自动生成无法实现完全无副作用生成，所以这里提供的代码自动生成主要是给大家生成个初始模版，简化大家的工作
        //大多数情况直接使用自动生成的模版即可，如果遇到问题可以手动去修改生成后的文件，因此这里需要大家自行处理是否覆盖的问题

        //using (System.IO.StreamWriter sw = new System.IO.StreamWriter("Assets/3rd/Samples/ILRuntime/Basic Demo/Scripts/Examples/04_Inheritance/InheritanceAdapter.cs"))
        //{
        //    sw.WriteLine(ILRuntime.Runtime.Enviorment.CrossBindingCodeGenerator.GenerateCrossBindingAdapterCode(typeof(TestClassBase), "ILRuntimeDemo"));
        //}
        //using (System.IO.StreamWriter sw = new System.IO.StreamWriter("Assets/Scripts/ILAdapters/ObjectBagAdapter.cs"))
        //{
        //    sw.WriteLine(ILRuntime.Runtime.Enviorment.CrossBindingCodeGenerator.GenerateCrossBindingAdapterCode(typeof(ObjectBag), "ILRuntimeAdapters"));
        //}        
        //using (System.IO.StreamWriter sw = new System.IO.StreamWriter("Assets/Scripts/ILAdapters/IListAdapter.cs"))
        //{
        //    sw.WriteLine(ILRuntime.Runtime.Enviorment.CrossBindingCodeGenerator.GenerateCrossBindingAdapterCode(typeof(IList), "ILRuntimeAdapters"));
        //}
        //using (System.IO.StreamWriter sw = new System.IO.StreamWriter("Assets/Scripts/ILAdapters/ApplicationException.cs"))
        //{
        //    sw.WriteLine(ILRuntime.Runtime.Enviorment.CrossBindingCodeGenerator.GenerateCrossBindingAdapterCode(typeof(ApplicationException), "ILRuntimeAdapters"));
        //}
        //using (System.IO.StreamWriter sw = new System.IO.StreamWriter("Assets/Scripts/ILAdapters/IDictionaryEnumerator.cs"))
        //{
        //    sw.WriteLine(ILRuntime.Runtime.Enviorment.CrossBindingCodeGenerator.GenerateCrossBindingAdapterCode(typeof(IDictionaryEnumerator), "ILRuntimeAdapters"));
        //}

        AssetDatabase.Refresh();
    }
}
#endif