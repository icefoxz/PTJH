using System;
using System.Collections.Generic;
using System.Reflection;

namespace ILRuntime.Runtime.Generated
{
    class CLRBindings
    {

//will auto register in unity
#if UNITY_5_3_OR_NEWER
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
#endif
        static private void RegisterBindingAction()
        {
            ILRuntime.Runtime.CLRBinding.CLRBindingUtils.RegisterBindingAction(Initialize);
        }

        internal static ILRuntime.Runtime.Enviorment.ValueTypeBinder<UnityEngine.Vector3> s_UnityEngine_Vector3_Binding_Binder = null;
        internal static ILRuntime.Runtime.Enviorment.ValueTypeBinder<UnityEngine.Vector2> s_UnityEngine_Vector2_Binding_Binder = null;
        internal static ILRuntime.Runtime.Enviorment.ValueTypeBinder<UnityEngine.Quaternion> s_UnityEngine_Quaternion_Binding_Binder = null;

        /// <summary>
        /// Initialize the CLR binding, please invoke this AFTER CLR Redirection registration
        /// </summary>
        public static void Initialize(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            UnityEngine_Debug_Binding.Register(app);
            Game_Binding.Register(app);
            Utls_DependencySingleton_1_ICoroutineService_Binding.Register(app);
            Systems_Coroutines_ICoroutineService_Binding.Register(app);
            System_Net_Http_HttpClient_Binding.Register(app);
            Systems_HotFixHelper_Binding.Register(app);
            Data_Views_TestViewBag_Binding.Register(app);
            System_Collections_Generic_List_1_ViewBag_Binding.Register(app);
            System_Collections_Generic_List_1_ViewBag_Binding_Enumerator_Binding.Register(app);
            Data_Views_ViewBag_Binding.Register(app);
            System_IDisposable_Binding.Register(app);
            UnityEngine_GameObject_Binding.Register(app);
            UnityEngine_Component_Binding.Register(app);
            UnityEngine_Transform_Binding.Register(app);
            Systems_Messaging_UnityButtonExtension_Binding.Register(app);
            Systems_Messaging_MessagingManager_Binding.Register(app);
            System_Net_Http_HttpResponseMessage_Binding.Register(app);
            System_String_Binding.Register(app);
            UnityEngine_WaitForSeconds_Binding.Register(app);
            System_NotSupportedException_Binding.Register(app);
            UnityEngine_WaitUntil_Binding.Register(app);
            System_Int32_Binding.Register(app);
            System_Collections_Generic_List_1_Int32_Binding.Register(app);
            CLRBindingTestClass_Binding.Register(app);
            CoroutineDemo_Binding.Register(app);
            UnityEngine_Time_Binding.Register(app);
            System_Single_Binding.Register(app);
            TestDelegateMethod_Binding.Register(app);
            TestDelegateFunction_Binding.Register(app);
            System_Action_1_String_Binding.Register(app);
            DelegateDemo_Binding.Register(app);
            TestClassBase_Binding.Register(app);
            System_Collections_Generic_List_1_ILTypeInstance_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_ILTypeInstance_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_Int32_Binding.Register(app);
            LitJson_JsonMapper_Binding.Register(app);
            System_Object_Binding.Register(app);
            System_Diagnostics_Stopwatch_Binding.Register(app);
            System_Text_StringBuilder_Binding.Register(app);
            Performance_Binding.Register(app);
            UnityEngine_Object_Binding.Register(app);
            System_Int64_Binding.Register(app);
            UnityEngine_Vector3_Binding.Register(app);
            System_Type_Binding.Register(app);
            UnityEngine_Renderer_Binding.Register(app);
            UnityEngine_Input_Binding.Register(app);
            UnityEngine_Quaternion_Binding.Register(app);
            System_Boolean_Binding.Register(app);
            UnityEngine_Vector2_Binding.Register(app);
            System_InvalidOperationException_Binding.Register(app);
            UiBuilder_Binding.Register(app);
            Utls_DependencySingleton_1_IServiceCaller_Binding.Register(app);
            Server_IServiceCaller_Binding.Register(app);
            Views_IView_Binding.Register(app);
            Server_Controllers_Adventures_Adventure_Binding.Register(app);
            Data_Views_ObjectBag_1_Adventure_Binding.Register(app);
            System_Linq_Enumerable_Binding.Register(app);
            Server_Controllers_Adventures_AdvEvent_Binding.Register(app);
            UnityEngine_UI_Text_Binding.Register(app);
            System_Collections_Generic_List_1_ILTypeInstance_Binding_Enumerator_Binding.Register(app);
            Server_Controllers_Adventures_AdvUnit_Binding.Register(app);
            UnityEngine_UI_ScrollRect_Binding.Register(app);
            Server_Controllers_Adventures_UnitStatus_Binding.Register(app);
            BattleM_ConValue_Binding.Register(app);
            Views_View_Binding.Register(app);
            System_Action_1_AdvUnit_Binding.Register(app);

            ILRuntime.CLR.TypeSystem.CLRType __clrType = null;
            __clrType = (ILRuntime.CLR.TypeSystem.CLRType)app.GetType (typeof(UnityEngine.Vector3));
            s_UnityEngine_Vector3_Binding_Binder = __clrType.ValueTypeBinder as ILRuntime.Runtime.Enviorment.ValueTypeBinder<UnityEngine.Vector3>;
            __clrType = (ILRuntime.CLR.TypeSystem.CLRType)app.GetType (typeof(UnityEngine.Vector2));
            s_UnityEngine_Vector2_Binding_Binder = __clrType.ValueTypeBinder as ILRuntime.Runtime.Enviorment.ValueTypeBinder<UnityEngine.Vector2>;
            __clrType = (ILRuntime.CLR.TypeSystem.CLRType)app.GetType (typeof(UnityEngine.Quaternion));
            s_UnityEngine_Quaternion_Binding_Binder = __clrType.ValueTypeBinder as ILRuntime.Runtime.Enviorment.ValueTypeBinder<UnityEngine.Quaternion>;
        }

        /// <summary>
        /// Release the CLR binding, please invoke this BEFORE ILRuntime Appdomain destroy
        /// </summary>
        public static void Shutdown(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            s_UnityEngine_Vector3_Binding_Binder = null;
            s_UnityEngine_Vector2_Binding_Binder = null;
            s_UnityEngine_Quaternion_Binding_Binder = null;
        }
    }
}
