using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
using ILRuntime.Reflection;
using ILRuntime.CLR.Utils;
#if DEBUG && !DISABLE_ILRUNTIME_DEBUG
using AutoList = System.Collections.Generic.List<object>;
#else
using AutoList = ILRuntime.Other.UncheckedList<object>;
#endif
namespace ILRuntime.Runtime.Generated
{
    unsafe class System_Collections_Generic_IList_1_KeyValuePair_2_String_ILAdapters_IListAdapter_Binding_Adapter_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            Type[] args;
            Type type = typeof(System.Collections.Generic.IList<System.Collections.Generic.KeyValuePair<System.String, ILAdapters.IListAdapter.Adapter>>);
            args = new Type[]{typeof(System.Int32)};
            method = type.GetMethod("get_Item", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_Item_0);
            args = new Type[]{typeof(System.Int32), typeof(System.Collections.Generic.KeyValuePair<System.String, ILAdapters.IListAdapter.Adapter>)};
            method = type.GetMethod("set_Item", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, set_Item_1);
            args = new Type[]{typeof(System.Int32)};
            method = type.GetMethod("RemoveAt", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, RemoveAt_2);
            args = new Type[]{typeof(System.Int32), typeof(System.Collections.Generic.KeyValuePair<System.String, ILAdapters.IListAdapter.Adapter>)};
            method = type.GetMethod("Insert", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Insert_3);


        }


        static StackObject* get_Item_0(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 @index = ptr_of_this_method->Value;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Collections.Generic.IList<System.Collections.Generic.KeyValuePair<System.String, ILAdapters.IListAdapter.Adapter>> instance_of_this_method = (System.Collections.Generic.IList<System.Collections.Generic.KeyValuePair<System.String, ILAdapters.IListAdapter.Adapter>>)typeof(System.Collections.Generic.IList<System.Collections.Generic.KeyValuePair<System.String, ILAdapters.IListAdapter.Adapter>>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            var result_of_this_method = instance_of_this_method[index];

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* set_Item_1(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Collections.Generic.KeyValuePair<System.String, ILAdapters.IListAdapter.Adapter> @value = (System.Collections.Generic.KeyValuePair<System.String, ILAdapters.IListAdapter.Adapter>)typeof(System.Collections.Generic.KeyValuePair<System.String, ILAdapters.IListAdapter.Adapter>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)16);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Int32 @index = ptr_of_this_method->Value;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.Collections.Generic.IList<System.Collections.Generic.KeyValuePair<System.String, ILAdapters.IListAdapter.Adapter>> instance_of_this_method = (System.Collections.Generic.IList<System.Collections.Generic.KeyValuePair<System.String, ILAdapters.IListAdapter.Adapter>>)typeof(System.Collections.Generic.IList<System.Collections.Generic.KeyValuePair<System.String, ILAdapters.IListAdapter.Adapter>>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method[index] = value;

            return __ret;
        }

        static StackObject* RemoveAt_2(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 @index = ptr_of_this_method->Value;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Collections.Generic.IList<System.Collections.Generic.KeyValuePair<System.String, ILAdapters.IListAdapter.Adapter>> instance_of_this_method = (System.Collections.Generic.IList<System.Collections.Generic.KeyValuePair<System.String, ILAdapters.IListAdapter.Adapter>>)typeof(System.Collections.Generic.IList<System.Collections.Generic.KeyValuePair<System.String, ILAdapters.IListAdapter.Adapter>>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.RemoveAt(@index);

            return __ret;
        }

        static StackObject* Insert_3(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Collections.Generic.KeyValuePair<System.String, ILAdapters.IListAdapter.Adapter> @item = (System.Collections.Generic.KeyValuePair<System.String, ILAdapters.IListAdapter.Adapter>)typeof(System.Collections.Generic.KeyValuePair<System.String, ILAdapters.IListAdapter.Adapter>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)16);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Int32 @index = ptr_of_this_method->Value;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.Collections.Generic.IList<System.Collections.Generic.KeyValuePair<System.String, ILAdapters.IListAdapter.Adapter>> instance_of_this_method = (System.Collections.Generic.IList<System.Collections.Generic.KeyValuePair<System.String, ILAdapters.IListAdapter.Adapter>>)typeof(System.Collections.Generic.IList<System.Collections.Generic.KeyValuePair<System.String, ILAdapters.IListAdapter.Adapter>>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.Insert(@index, @item);

            return __ret;
        }



    }
}
