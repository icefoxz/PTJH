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
    unsafe class Server_Controllers_IBattleSimController_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            Type[] args;
            Type type = typeof(Server.Controllers.IBattleSimController);
            args = new Type[]{typeof(System.Boolean), typeof(Server.Controllers.BattleSimController.TestModel.Props), typeof(System.Int32)};
            method = type.GetMethod("SetValue", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, SetValue_0);
            args = new Type[]{typeof(System.Boolean)};
            method = type.GetMethod("ForceLevelAdd", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, ForceLevelAdd_1);
            args = new Type[]{typeof(System.Boolean)};
            method = type.GetMethod("ForceLevelSub", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, ForceLevelSub_2);
            args = new Type[]{typeof(System.Boolean)};
            method = type.GetMethod("CombatLevelAdd", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, CombatLevelAdd_3);
            args = new Type[]{typeof(System.Boolean)};
            method = type.GetMethod("CombatLevelSub", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, CombatLevelSub_4);
            args = new Type[]{typeof(System.Boolean)};
            method = type.GetMethod("DodgeLevelAdd", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, DodgeLevelAdd_5);
            args = new Type[]{typeof(System.Boolean)};
            method = type.GetMethod("DodgeLevelSub", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, DodgeLevelSub_6);
            args = new Type[]{typeof(System.Boolean)};
            method = type.GetMethod("ForceGradeAdd", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, ForceGradeAdd_7);
            args = new Type[]{typeof(System.Boolean)};
            method = type.GetMethod("ForceGradeSub", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, ForceGradeSub_8);
            args = new Type[]{typeof(System.Boolean)};
            method = type.GetMethod("CombatGradeAdd", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, CombatGradeAdd_9);
            args = new Type[]{typeof(System.Boolean)};
            method = type.GetMethod("CombatGradeSub", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, CombatGradeSub_10);
            args = new Type[]{typeof(System.Boolean)};
            method = type.GetMethod("DodgeGradeAdd", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, DodgeGradeAdd_11);
            args = new Type[]{typeof(System.Boolean)};
            method = type.GetMethod("DodgeGradeSub", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, DodgeGradeSub_12);


        }


        static StackObject* SetValue_0(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 4);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Int32 @value = ptr_of_this_method->Value;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            Server.Controllers.BattleSimController.TestModel.Props @prop = (Server.Controllers.BattleSimController.TestModel.Props)typeof(Server.Controllers.BattleSimController.TestModel.Props).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)20);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.Boolean @isPlayer = ptr_of_this_method->Value == 1;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 4);
            Server.Controllers.IBattleSimController instance_of_this_method = (Server.Controllers.IBattleSimController)typeof(Server.Controllers.IBattleSimController).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.SetValue(@isPlayer, @prop, @value);

            return __ret;
        }

        static StackObject* ForceLevelAdd_1(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Boolean @isPlayer = ptr_of_this_method->Value == 1;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            Server.Controllers.IBattleSimController instance_of_this_method = (Server.Controllers.IBattleSimController)typeof(Server.Controllers.IBattleSimController).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.ForceLevelAdd(@isPlayer);

            return __ret;
        }

        static StackObject* ForceLevelSub_2(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Boolean @isPlayer = ptr_of_this_method->Value == 1;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            Server.Controllers.IBattleSimController instance_of_this_method = (Server.Controllers.IBattleSimController)typeof(Server.Controllers.IBattleSimController).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.ForceLevelSub(@isPlayer);

            return __ret;
        }

        static StackObject* CombatLevelAdd_3(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Boolean @isPlayer = ptr_of_this_method->Value == 1;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            Server.Controllers.IBattleSimController instance_of_this_method = (Server.Controllers.IBattleSimController)typeof(Server.Controllers.IBattleSimController).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.CombatLevelAdd(@isPlayer);

            return __ret;
        }

        static StackObject* CombatLevelSub_4(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Boolean @isPlayer = ptr_of_this_method->Value == 1;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            Server.Controllers.IBattleSimController instance_of_this_method = (Server.Controllers.IBattleSimController)typeof(Server.Controllers.IBattleSimController).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.CombatLevelSub(@isPlayer);

            return __ret;
        }

        static StackObject* DodgeLevelAdd_5(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Boolean @isPlayer = ptr_of_this_method->Value == 1;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            Server.Controllers.IBattleSimController instance_of_this_method = (Server.Controllers.IBattleSimController)typeof(Server.Controllers.IBattleSimController).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.DodgeLevelAdd(@isPlayer);

            return __ret;
        }

        static StackObject* DodgeLevelSub_6(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Boolean @isPlayer = ptr_of_this_method->Value == 1;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            Server.Controllers.IBattleSimController instance_of_this_method = (Server.Controllers.IBattleSimController)typeof(Server.Controllers.IBattleSimController).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.DodgeLevelSub(@isPlayer);

            return __ret;
        }

        static StackObject* ForceGradeAdd_7(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Boolean @isPlayer = ptr_of_this_method->Value == 1;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            Server.Controllers.IBattleSimController instance_of_this_method = (Server.Controllers.IBattleSimController)typeof(Server.Controllers.IBattleSimController).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.ForceGradeAdd(@isPlayer);

            return __ret;
        }

        static StackObject* ForceGradeSub_8(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Boolean @isPlayer = ptr_of_this_method->Value == 1;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            Server.Controllers.IBattleSimController instance_of_this_method = (Server.Controllers.IBattleSimController)typeof(Server.Controllers.IBattleSimController).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.ForceGradeSub(@isPlayer);

            return __ret;
        }

        static StackObject* CombatGradeAdd_9(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Boolean @isPlayer = ptr_of_this_method->Value == 1;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            Server.Controllers.IBattleSimController instance_of_this_method = (Server.Controllers.IBattleSimController)typeof(Server.Controllers.IBattleSimController).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.CombatGradeAdd(@isPlayer);

            return __ret;
        }

        static StackObject* CombatGradeSub_10(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Boolean @isPlayer = ptr_of_this_method->Value == 1;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            Server.Controllers.IBattleSimController instance_of_this_method = (Server.Controllers.IBattleSimController)typeof(Server.Controllers.IBattleSimController).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.CombatGradeSub(@isPlayer);

            return __ret;
        }

        static StackObject* DodgeGradeAdd_11(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Boolean @isPlayer = ptr_of_this_method->Value == 1;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            Server.Controllers.IBattleSimController instance_of_this_method = (Server.Controllers.IBattleSimController)typeof(Server.Controllers.IBattleSimController).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.DodgeGradeAdd(@isPlayer);

            return __ret;
        }

        static StackObject* DodgeGradeSub_12(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Boolean @isPlayer = ptr_of_this_method->Value == 1;

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            Server.Controllers.IBattleSimController instance_of_this_method = (Server.Controllers.IBattleSimController)typeof(Server.Controllers.IBattleSimController).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            instance_of_this_method.DodgeGradeSub(@isPlayer);

            return __ret;
        }



    }
}
