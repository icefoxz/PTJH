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
    unsafe class System_ValueTuple_2_Server_Controllers_BattleSimController_Binding_PropCon_Binding_Skills_Boolean_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            FieldInfo field;
            Type[] args;
            Type type = typeof(System.ValueTuple<Server.Controllers.BattleSimController.PropCon.Skills, System.Boolean>);

            field = type.GetField("Item1", flag);
            app.RegisterCLRFieldGetter(field, get_Item1_0);
            app.RegisterCLRFieldSetter(field, set_Item1_0);
            app.RegisterCLRFieldBinding(field, CopyToStack_Item1_0, AssignFromStack_Item1_0);
            field = type.GetField("Item2", flag);
            app.RegisterCLRFieldGetter(field, get_Item2_1);
            app.RegisterCLRFieldSetter(field, set_Item2_1);
            app.RegisterCLRFieldBinding(field, CopyToStack_Item2_1, AssignFromStack_Item2_1);

            app.RegisterCLRCreateDefaultInstance(type, () => new System.ValueTuple<Server.Controllers.BattleSimController.PropCon.Skills, System.Boolean>());

            args = new Type[]{typeof(Server.Controllers.BattleSimController.PropCon.Skills), typeof(System.Boolean)};
            method = type.GetConstructor(flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Ctor_0);

        }

        static void WriteBackInstance(ILRuntime.Runtime.Enviorment.AppDomain __domain, StackObject* ptr_of_this_method, AutoList __mStack, ref System.ValueTuple<Server.Controllers.BattleSimController.PropCon.Skills, System.Boolean> instance_of_this_method)
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
                        var instance_of_arrayReference = __mStack[ptr_of_this_method->Value] as System.ValueTuple<Server.Controllers.BattleSimController.PropCon.Skills, System.Boolean>[];
                        instance_of_arrayReference[ptr_of_this_method->ValueLow] = instance_of_this_method;
                    }
                    break;
            }
        }


        static object get_Item1_0(ref object o)
        {
            return ((System.ValueTuple<Server.Controllers.BattleSimController.PropCon.Skills, System.Boolean>)o).Item1;
        }

        static StackObject* CopyToStack_Item1_0(ref object o, ILIntepreter __intp, StackObject* __ret, AutoList __mStack)
        {
            var result_of_this_method = ((System.ValueTuple<Server.Controllers.BattleSimController.PropCon.Skills, System.Boolean>)o).Item1;
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static void set_Item1_0(ref object o, object v)
        {
            System.ValueTuple<Server.Controllers.BattleSimController.PropCon.Skills, System.Boolean> ins =(System.ValueTuple<Server.Controllers.BattleSimController.PropCon.Skills, System.Boolean>)o;
            ins.Item1 = (Server.Controllers.BattleSimController.PropCon.Skills)v;
            o = ins;
        }

        static StackObject* AssignFromStack_Item1_0(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, AutoList __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            Server.Controllers.BattleSimController.PropCon.Skills @Item1 = (Server.Controllers.BattleSimController.PropCon.Skills)typeof(Server.Controllers.BattleSimController.PropCon.Skills).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)20);
            System.ValueTuple<Server.Controllers.BattleSimController.PropCon.Skills, System.Boolean> ins =(System.ValueTuple<Server.Controllers.BattleSimController.PropCon.Skills, System.Boolean>)o;
            ins.Item1 = @Item1;
            o = ins;
            return ptr_of_this_method;
        }

        static object get_Item2_1(ref object o)
        {
            return ((System.ValueTuple<Server.Controllers.BattleSimController.PropCon.Skills, System.Boolean>)o).Item2;
        }

        static StackObject* CopyToStack_Item2_1(ref object o, ILIntepreter __intp, StackObject* __ret, AutoList __mStack)
        {
            var result_of_this_method = ((System.ValueTuple<Server.Controllers.BattleSimController.PropCon.Skills, System.Boolean>)o).Item2;
            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method ? 1 : 0;
            return __ret + 1;
        }

        static void set_Item2_1(ref object o, object v)
        {
            System.ValueTuple<Server.Controllers.BattleSimController.PropCon.Skills, System.Boolean> ins =(System.ValueTuple<Server.Controllers.BattleSimController.PropCon.Skills, System.Boolean>)o;
            ins.Item2 = (System.Boolean)v;
            o = ins;
        }

        static StackObject* AssignFromStack_Item2_1(ref object o, ILIntepreter __intp, StackObject* ptr_of_this_method, AutoList __mStack)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            System.Boolean @Item2 = ptr_of_this_method->Value == 1;
            System.ValueTuple<Server.Controllers.BattleSimController.PropCon.Skills, System.Boolean> ins =(System.ValueTuple<Server.Controllers.BattleSimController.PropCon.Skills, System.Boolean>)o;
            ins.Item2 = @Item2;
            o = ins;
            return ptr_of_this_method;
        }


        static StackObject* Ctor_0(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);
            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Boolean @item2 = ptr_of_this_method->Value == 1;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            Server.Controllers.BattleSimController.PropCon.Skills @item1 = (Server.Controllers.BattleSimController.PropCon.Skills)typeof(Server.Controllers.BattleSimController.PropCon.Skills).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)20);
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = new System.ValueTuple<Server.Controllers.BattleSimController.PropCon.Skills, System.Boolean>(@item1, @item2);

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
