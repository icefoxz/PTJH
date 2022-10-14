using System;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
#if DEBUG && !DISABLE_ILRUNTIME_DEBUG
using AutoList = System.Collections.Generic.List<object>;
#else
using AutoList = ILRuntime.Other.UncheckedList<object>;
#endif

namespace ILRuntimeAdapters
{   
    public class IDictionaryEnumeratorAdapter : CrossBindingAdaptor
    {
        public override Type BaseCLRType
        {
            get
            {
                return typeof(System.Collections.IDictionaryEnumerator);
            }
        }

        public override Type AdaptorType
        {
            get
            {
                return typeof(Adapter);
            }
        }

        public override object CreateCLRInstance(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
        {
            return new Adapter(appdomain, instance);
        }

        public class Adapter : System.Collections.IDictionaryEnumerator, CrossBindingAdaptorType
        {
            CrossBindingFunctionInfo<System.Object> mget_Key_0 = new CrossBindingFunctionInfo<System.Object>("get_Key");
            CrossBindingFunctionInfo<System.Object> mget_Value_1 = new CrossBindingFunctionInfo<System.Object>("get_Value");
            CrossBindingFunctionInfo<System.Collections.DictionaryEntry> mget_Entry_2 = new CrossBindingFunctionInfo<System.Collections.DictionaryEntry>("get_Entry");
            CrossBindingFunctionInfo<System.Boolean> mMoveNext_3 = new CrossBindingFunctionInfo<System.Boolean>("MoveNext");
            CrossBindingFunctionInfo<System.Object> mget_Current_4 = new CrossBindingFunctionInfo<System.Object>("get_Current");
            CrossBindingMethodInfo mReset_5 = new CrossBindingMethodInfo("Reset");

            bool isInvokingToString;
            ILTypeInstance instance;
            ILRuntime.Runtime.Enviorment.AppDomain appdomain;

            public Adapter()
            {

            }

            public Adapter(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
            {
                this.appdomain = appdomain;
                this.instance = instance;
            }

            public ILTypeInstance ILInstance { get { return instance; } }

            public System.Boolean MoveNext()
            {
                return mMoveNext_3.Invoke(this.instance);
            }

            public void Reset()
            {
                mReset_5.Invoke(this.instance);
            }

            public System.Object Key
            {
            get
            {
                return mget_Key_0.Invoke(this.instance);

            }
            }

            public System.Object Value
            {
            get
            {
                return mget_Value_1.Invoke(this.instance);

            }
            }

            public System.Collections.DictionaryEntry Entry
            {
            get
            {
                return mget_Entry_2.Invoke(this.instance);

            }
            }

            public System.Object Current
            {
            get
            {
                return mget_Current_4.Invoke(this.instance);

            }
            }

            public override string ToString()
            {
                IMethod m = appdomain.ObjectType.GetMethod("ToString", 0);
                m = instance.Type.GetVirtualMethod(m);
                if (m == null || m is ILMethod)
                {
                    if (!isInvokingToString)
                    {
                        isInvokingToString = true;
                        string res = instance.ToString();
                        isInvokingToString = false;
                        return res;
                    }
                    else
                        return instance.Type.FullName;
                }
                else
                    return instance.Type.FullName;
            }
        }
    }
}

