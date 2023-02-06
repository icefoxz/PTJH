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
    unsafe class System_ValueTuple_3_String_String_Int32_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            FieldInfo field;
            Type[] args;
            Type type = typeof(System.ValueTuple<System.String, System.String, System.Int32>);

            field = type.GetField("Item2", flag);
            app.RegisterCLRFieldGetter(field, get_Item2_0);
            app.RegisterCLRFieldSetter(field, set_Item2_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_Item2_0, AssignFromStack_Item2_0);
            field = type.GetField("Item1", flag);
            app.RegisterCLRFieldGetter(field, get_Item1_1);
            app.RegisterCLRFieldSetter(field, set_Item1_1);
            app.RegisterCLRFieldBinding(field, CopyToStack_Item1_1, AssignFromStack_Item1_1);
            field = type.GetField("Item3", flag);
            app.RegisterCLRFieldGetter(field, get_Item3_2);
            app.RegisterCLRFieldSetter(field, set_Item3_2);
            app.RegisterCLRFieldBinding(field, CopyToStack_Item3_2, AssignFromStack_Item3_2);

            app.RegisterCLRCreateDefaultInstance(type, () => new System.ValueTuple<System.String, System.String, System.Int32>());

            args = new Type[]{typeof(System.String), typeof(System.String), typeof(System.Int32)};
            method = type.GetConstructor(flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Ctor_0);

        }

        static void WriteBackInstance(ILRuntime.Runtime.Enviorment.AppDomain __domain, StackObject* ptr_of_this_method, AutoList __mStack, ref System.ValueTuple<System.String, System.String, System.Int32> instance_of_this_method)
        {
            ptr_of_this_method = ILIntepreter.GetObjectAndResolveReference(ptr_of_this_method);
            switch(ptr_of_this_method->ObjectType)
            {
                case ObjectTypes.Object:
                    {
                        __mStack[ptr_of_this_method->Value] = instance_of_this_method;
                    }
                    break;
                case ObjectTypes.FieldReference:
                    {
                        var ___obj = __mStack[ptr_of_this_method->Value];
                        if(___obj is ILTypeInstance)
                        {
                            ((ILTypeInstance)___obj)[ptr_of_this_method->ValueLow] = instance_of_this_method;
                        }
                        else
                        {
                            var t = __domain.GetType(___obj.GetType()) as CLRType;
                            t.SetFieldValue(ptr_of_this_method->ValueLow, ref ___obj, instance_of_this_method);
                        }
                    }
                    break;
                case ObjectTypes.StaticFieldReference:
                    {
                        var t = __domain.GetType(ptr_of_this_method->Value);
                        if(t is ILType)
                        {
                            ((ILType)t).StaticInstance[ptr_of_this_method->ValueLow] = instance_of_this_method;
                        }
                        else
                        {
                            ((CLRType)t).SetStaticFieldValue(ptr_of_this_method->ValueLow, instance_of_this_method);
                        }
                    }
                    break;
                 case ObjectTypes.ArrayReference:
                    {
                        var instance_of_arrayReference = __mStack[ptr_of_this_method->Value] as System.ValueTuple<System.String, System.String, System.Int32>[];
                        instance_of_arrayReference[ptr_of_this_method->ValueLow] = instance_of_this_method;
                    }
                    break;
            }
        }


        static object get_Item2_0(ref object o)
        {
            return ((System.ValueTuple<System.String, System.String, System.Int32>)o).Item2;
        }

        static StackObject* CopyToStack_Item2_0(ref object o, ILIntepreter __intp, StackObject* __ret, AutoList __mStack)
        {
            var result_of_this_method = ((System.ValueTuple<System.String, System.String, System.Int32>)o).Item2;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_Item2_0(ref object o, object v)
        {
            System.ValueTuple<System.String, System.String, System.Int32> ins =(System.ValueTuple<System.String, System.String, System.Int32>)o;
            ins.Item2 = (System.String)v;
            o = ins;
        }

        static StackObject* AssignFromStack_Item2_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, AutoList __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.String @Item2 = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            System.ValueTuple<System.String, System.String, System.Int32> ins =(System.ValueTuple<System.String, System.String, System.Int32>)o;
            ins.Item2 = @Item2;
            o = ins;
            return ptr_of_this_method;
        }

        static object get_Item1_1(ref object o)
        {
            return ((System.ValueTuple<System.String, System.String, System.Int32>)o).Item1;
        }

        static StackObject* CopyToStack_Item1_1(ref object o, ILIntepreter __intp, StackObject* __ret, AutoList __mStack)
        {
            var result_of_this_method = ((System.ValueTuple<System.String, System.String, System.Int32>)o).Item1;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_Item1_1(ref object o, object v)
        {
            System.ValueTuple<System.String, System.String, System.Int32> ins =(System.ValueTuple<System.String, System.String, System.Int32>)o;
            ins.Item1 = (System.String)v;
            o = ins;
        }

        static StackObject* AssignFromStack_Item1_1(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, AutoList __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.String @Item1 = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            System.ValueTuple<System.String, System.String, System.Int32> ins =(System.ValueTuple<System.String, System.String, System.Int32>)o;
            ins.Item1 = @Item1;
            o = ins;
            return ptr_of_this_method;
        }

        static object get_Item3_2(ref object o)
        {
            return ((System.ValueTuple<System.String, System.String, System.Int32>)o).Item3;
        }

        static StackObject* CopyToStack_Item3_2(ref object o, ILIntepreter __intp, StackObject* __ret, AutoList __mStack)
        {
            var result_of_this_method = ((System.ValueTuple<System.String, System.String, System.Int32>)o).Item3;
            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static void set_Item3_2(ref object o, object v)
        {
            System.ValueTuple<System.String, System.String, System.Int32> ins =(System.ValueTuple<System.String, System.String, System.Int32>)o;
            ins.Item3 = (System.Int32)v;
            o = ins;
        }

        static StackObject* AssignFromStack_Item3_2(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, AutoList __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Int32 @Item3 = ptr_of_this_method->Value;
            System.ValueTuple<System.String, System.String, System.Int32> ins =(System.ValueTuple<System.String, System.String, System.Int32>)o;
            ins.Item3 = @Item3;
            o = ins;
            return ptr_of_this_method;
        }


        static StackObject* Ctor_0(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 @item3 = ptr_of_this_method->Value;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.String @item2 = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.String @item1 = (System.String)typeof(System.String).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = new System.ValueTuple<System.String, System.String, System.Int32>(@item1, @item2, @item3);

            if(!isNewObj)
            {
                __ret--;
                WriteBackInstance(__domain, __ret, __mStack, ref result_of_this_method);
                return __ret;
            }

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }


    }
}
