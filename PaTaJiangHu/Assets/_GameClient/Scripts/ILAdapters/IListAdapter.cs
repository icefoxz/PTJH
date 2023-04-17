using System;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
#if DEBUG && !DISABLE_ILRUNTIME_DEBUG

#else
using AutoList = ILRuntime.Other.UncheckedList<object>;
#endif

namespace ILAdapters
{   
    public class IListAdapter : CrossBindingAdaptor
    {
        public override Type BaseCLRType
        {
            get
            {
                return typeof(System.Collections.IList);
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

        public class Adapter : System.Collections.IList, CrossBindingAdaptorType
        {
            CrossBindingFunctionInfo<System.Int32, System.Object> mget_Item_0 = new CrossBindingFunctionInfo<System.Int32, System.Object>("get_Item");
            CrossBindingMethodInfo<System.Int32, System.Object> mset_Item_1 = new CrossBindingMethodInfo<System.Int32, System.Object>("set_Item");
            CrossBindingFunctionInfo<System.Object, System.Int32> mAdd_2 = new CrossBindingFunctionInfo<System.Object, System.Int32>("Add");
            CrossBindingFunctionInfo<System.Object, System.Boolean> mContains_3 = new CrossBindingFunctionInfo<System.Object, System.Boolean>("Contains");
            CrossBindingMethodInfo mClear_4 = new CrossBindingMethodInfo("Clear");
            CrossBindingFunctionInfo<System.Boolean> mget_IsReadOnly_5 = new CrossBindingFunctionInfo<System.Boolean>("get_IsReadOnly");
            CrossBindingFunctionInfo<System.Boolean> mget_IsFixedSize_6 = new CrossBindingFunctionInfo<System.Boolean>("get_IsFixedSize");
            CrossBindingFunctionInfo<System.Object, System.Int32> mIndexOf_7 = new CrossBindingFunctionInfo<System.Object, System.Int32>("IndexOf");
            CrossBindingMethodInfo<System.Int32, System.Object> mInsert_8 = new CrossBindingMethodInfo<System.Int32, System.Object>("Insert");
            CrossBindingMethodInfo<System.Object> mRemove_9 = new CrossBindingMethodInfo<System.Object>("Remove");
            CrossBindingMethodInfo<System.Int32> mRemoveAt_10 = new CrossBindingMethodInfo<System.Int32>("RemoveAt");
            CrossBindingMethodInfo<System.Array, System.Int32> mCopyTo_11 = new CrossBindingMethodInfo<System.Array, System.Int32>("CopyTo");
            CrossBindingFunctionInfo<System.Int32> mget_Count_12 = new CrossBindingFunctionInfo<System.Int32>("get_Count");
            CrossBindingFunctionInfo<System.Object> mget_SyncRoot_13 = new CrossBindingFunctionInfo<System.Object>("get_SyncRoot");
            CrossBindingFunctionInfo<System.Boolean> mget_IsSynchronized_14 = new CrossBindingFunctionInfo<System.Boolean>("get_IsSynchronized");
            CrossBindingFunctionInfo<System.Collections.IEnumerator> mGetEnumerator_15 = new CrossBindingFunctionInfo<System.Collections.IEnumerator>("GetEnumerator");

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

            public System.Int32 Add(System.Object value)
            {
                return mAdd_2.Invoke(this.instance, value);
            }

            public System.Boolean Contains(System.Object value)
            {
                return mContains_3.Invoke(this.instance, value);
            }

            public void Clear()
            {
                mClear_4.Invoke(this.instance);
            }

            public System.Int32 IndexOf(System.Object value)
            {
                return mIndexOf_7.Invoke(this.instance, value);
            }

            public void Insert(System.Int32 index, System.Object value)
            {
                mInsert_8.Invoke(this.instance, index, value);
            }

            public void Remove(System.Object value)
            {
                mRemove_9.Invoke(this.instance, value);
            }

            public void RemoveAt(System.Int32 index)
            {
                mRemoveAt_10.Invoke(this.instance, index);
            }

            public void CopyTo(System.Array array, System.Int32 index)
            {
                mCopyTo_11.Invoke(this.instance, array, index);
            }

            public System.Collections.IEnumerator GetEnumerator()
            {
                return mGetEnumerator_15.Invoke(this.instance);
            }

            public System.Object this [System.Int32 index]
            {
            get
            {
                return mget_Item_0.Invoke(this.instance, index);

            }
            set
            {
                mset_Item_1.Invoke(this.instance, index, value);

            }
            }

            public System.Boolean IsReadOnly
            {
            get
            {
                return mget_IsReadOnly_5.Invoke(this.instance);

            }
            }

            public System.Boolean IsFixedSize
            {
            get
            {
                return mget_IsFixedSize_6.Invoke(this.instance);

            }
            }

            public System.Int32 Count
            {
            get
            {
                return mget_Count_12.Invoke(this.instance);

            }
            }

            public System.Object SyncRoot
            {
            get
            {
                return mget_SyncRoot_13.Invoke(this.instance);

            }
            }

            public System.Boolean IsSynchronized
            {
            get
            {
                return mget_IsSynchronized_14.Invoke(this.instance);

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

