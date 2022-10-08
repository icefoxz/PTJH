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

namespace ILRuntime.Runtime.Generated
{
    unsafe class Game_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            MethodBase method;
            Type[] args;
            Type type = typeof(global::Game);
            args = new Type[]{};
            method = type.GetMethod("Run", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, Run_0);
            args = new Type[]{};
            method = type.GetMethod("get_SceneCanvas", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_SceneCanvas_1);
            args = new Type[]{};
            method = type.GetMethod("get_MessagingManager", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_MessagingManager_2);
            args = new Type[]{};
            method = type.GetMethod("get_UiBuilder", flag, null, args, null);
            app.RegisterCLRMethodRedirection(method, get_UiBuilder_3);


        }


        static StackObject* Run_0(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);


            global::Game.Run();

            return __ret;
        }

        static StackObject* get_SceneCanvas_1(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);


            var result_of_this_method = global::Game.SceneCanvas;

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* get_MessagingManager_2(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);


            var result_of_this_method = global::Game.MessagingManager;

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* get_UiBuilder_3(ILIntepreter __intp, StackObject* __esp, IList<object> __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* __ret = ILIntepreter.Minus(__esp, 0);


            var result_of_this_method = global::Game.UiBuilder;

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }



    }
}
