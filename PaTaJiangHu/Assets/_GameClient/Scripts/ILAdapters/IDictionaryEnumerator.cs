using System;
using System.Collections;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;

#if DEBUG && !DISABLE_ILRUNTIME_DEBUG

#else
using AutoList = ILRuntime.Other.UncheckedList<object>;
#endif

namespace ILAdapters
{   
    public class IDictionaryEnumeratorAdapter : CrossBindingAdaptor
    {
        public override Type BaseCLRType => typeof(IDictionaryEnumerator);
        public override Type AdaptorType => typeof(Adapter);
        public override object CreateCLRInstance(AppDomain appdomain, ILTypeInstance instance) => new Adapter(appdomain, instance);
        public class Adapter : IDictionaryEnumerator, CrossBindingAdaptorType
        {
            CrossBindingFunctionInfo<object> mget_Key_0 = new("get_Key");
            CrossBindingFunctionInfo<object> mget_Value_1 = new("get_Value");
            CrossBindingFunctionInfo<DictionaryEntry> mget_Entry_2 = new("get_Entry");
            CrossBindingFunctionInfo<bool> mMoveNext_3 = new("MoveNext");
            CrossBindingFunctionInfo<object> mget_Current_4 = new("get_Current");
            CrossBindingMethodInfo mReset_5 = new("Reset");

            bool isInvokingToString;
            AppDomain appdomain;

            public Adapter() { }

            public Adapter(AppDomain appdomain, ILTypeInstance instance)
            {
                this.appdomain = appdomain;
                ILInstance = instance;
            }

            public ILTypeInstance ILInstance { get; }

            public bool MoveNext() => mMoveNext_3.Invoke(ILInstance);
            public void Reset() => mReset_5.Invoke(ILInstance);
            public object Key => mget_Key_0.Invoke(ILInstance);
            public object Value => mget_Value_1.Invoke(ILInstance);
            public DictionaryEntry Entry => mget_Entry_2.Invoke(ILInstance);
            public object Current => mget_Current_4.Invoke(ILInstance);

            public override string ToString()
            {
                var m = appdomain.ObjectType.GetMethod("ToString", 0);
                m = ILInstance.Type.GetVirtualMethod(m);
                switch (m)
                {
                    case null or ILMethod when !isInvokingToString:
                    {
                        isInvokingToString = true;
                        var res = ILInstance.ToString();
                        isInvokingToString = false;
                        return res;
                    }
                    case null or ILMethod:
                        return ILInstance.Type.FullName;
                    default:
                        return ILInstance.Type.FullName;
                }
            }
        }
    }
}

