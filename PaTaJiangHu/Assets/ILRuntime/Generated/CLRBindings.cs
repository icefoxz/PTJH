using System;
using System.Collections.Generic;
using System.Reflection;
#if DEBUG && !DISABLE_ILRUNTIME_DEBUG
using AutoList = System.Collections.Generic.List<object>;
#else
using AutoList = ILRuntime.Other.UncheckedList<object>;
#endif
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
        internal static ILRuntime.Runtime.Enviorment.ValueTypeBinder<UnityEngine.Quaternion> s_UnityEngine_Quaternion_Binding_Binder = null;
        internal static ILRuntime.Runtime.Enviorment.ValueTypeBinder<UnityEngine.Vector2> s_UnityEngine_Vector2_Binding_Binder = null;

        /// <summary>
        /// Initialize the CLR binding, please invoke this AFTER CLR Redirection registration
        /// </summary>
        public static void Initialize(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            System_Collections_Generic_EqualityComparer_1_Int32_Binding.Register(app);
            System_Collections_Generic_EqualityComparer_1_ILTypeInstance_Array_Binding.Register(app);
            System_Object_Binding.Register(app);
            System_String_Binding.Register(app);
            UnityEngine_Debug_Binding.Register(app);
            Game_Binding.Register(app);
            Utls_DependencySingleton_1_ICoroutineService_Binding.Register(app);
            Systems_Coroutines_ICoroutineService_Binding.Register(app);
            System_Net_Http_HttpClient_Binding.Register(app);
            Systems_HotFixHelper_Binding.Register(app);
            UnityEngine_GameObject_Binding.Register(app);
            UnityEngine_Component_Binding.Register(app);
            UnityEngine_Transform_Binding.Register(app);
            System_Net_Http_HttpResponseMessage_Binding.Register(app);
            UnityEngine_WaitForSeconds_Binding.Register(app);
            System_NotSupportedException_Binding.Register(app);
            UnityEngine_WaitUntil_Binding.Register(app);
            Systems_Coroutines_ICoroutineInstance_Binding.Register(app);
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
            Views_IView_Binding.Register(app);
            System_InvalidOperationException_Binding.Register(app);
            System_Collections_IEnumerator_Binding.Register(app);
            System_IDisposable_Binding.Register(app);
            System_Func_2_View_ILTypeInstance_Binding.Register(app);
            System_Collections_Generic_List_1_ILTypeInstance_Binding_Enumerator_Binding.Register(app);
            System_Action_1_ILTypeInstance_Binding.Register(app);
            UnityEngine_UI_ScrollRect_Binding.Register(app);
            System_Console_Binding.Register(app);
            Utls_ObjectBag_Binding.Register(app);
            System_Convert_Binding.Register(app);
            System_Collections_ICollection_Binding.Register(app);
            System_Collections_Generic_IDictionary_2_String_ILAdapters_IListAdapter_Binding_Adapter_Binding.Register(app);
            System_Collections_Generic_ICollection_1_String_Binding.Register(app);
            System_Collections_IDictionary_Binding.Register(app);
            System_Collections_Generic_List_1_String_Binding.Register(app);
            System_Collections_Generic_IEnumerable_1_KeyValuePair_2_String_ILAdapters_IListAdapter_Binding_Adapter_Binding.Register(app);
            System_Collections_Generic_IEnumerator_1_KeyValuePair_2_String_ILAdapters_IListAdapter_Binding_Adapter_Binding.Register(app);
            System_Collections_Generic_KeyValuePair_2_String_ILAdapters_IListAdapter_Binding_Adapter_Binding.Register(app);
            System_Collections_Generic_List_1_ILAdapters_IListAdapter_Binding_Adapter_Binding.Register(app);
            System_Collections_Generic_ICollection_1_ILAdapters_IListAdapter_Binding_Adapter_Binding.Register(app);
            System_Collections_IList_Binding.Register(app);
            System_ArgumentException_Binding.Register(app);
            System_Collections_Generic_IList_1_KeyValuePair_2_String_ILAdapters_IListAdapter_Binding_Adapter_Binding.Register(app);
            System_Collections_Generic_IList_1_ILAdapters_IListAdapter_Binding_Adapter_Binding.Register(app);
            System_Collections_Generic_ICollection_1_KeyValuePair_2_String_ILAdapters_IListAdapter_Binding_Adapter_Binding.Register(app);
            System_InvalidCastException_Binding.Register(app);
            System_Collections_Specialized_IOrderedDictionary_Binding.Register(app);
            System_Collections_IEnumerable_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_ILAdapters_IListAdapter_Binding_Adapter_Binding.Register(app);
            System_Collections_Generic_List_1_KeyValuePair_2_String_ILAdapters_IListAdapter_Binding_Adapter_Binding.Register(app);
            System_Collections_DictionaryEntry_Binding.Register(app);
            System_Collections_Generic_KeyNotFoundException_Binding.Register(app);
            System_Double_Binding.Register(app);
            System_IO_StringWriter_Binding.Register(app);
            System_Collections_Generic_IDictionary_2_Type_ILTypeInstance_Binding.Register(app);
            System_Reflection_MemberInfo_Binding.Register(app);
            System_Reflection_PropertyInfo_Binding.Register(app);
            System_Reflection_ParameterInfo_Binding.Register(app);
            System_Threading_Monitor_Binding.Register(app);
            System_Collections_Generic_IDictionary_2_String_ILTypeInstance_Binding.Register(app);
            System_Reflection_FieldInfo_Binding.Register(app);
            System_Collections_Generic_IDictionary_2_Type_IList_1_ILTypeInstance_Binding.Register(app);
            System_Collections_Generic_ICollection_1_ILTypeInstance_Binding.Register(app);
            System_Collections_Generic_IDictionary_2_Type_IDictionary_2_Type_MethodInfo_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Type_MethodInfo_Binding.Register(app);
            System_Collections_Generic_IDictionary_2_Type_MethodInfo_Binding.Register(app);
            System_Nullable_Binding.Register(app);
            System_Collections_Generic_IDictionary_2_Type_IDictionary_2_Type_ILTypeInstance_Binding.Register(app);
            System_Enum_Binding.Register(app);
            System_Reflection_MethodInfo_Binding.Register(app);
            System_Reflection_MethodBase_Binding.Register(app);
            System_Activator_Binding.Register(app);
            System_Collections_ArrayList_Binding.Register(app);
            System_Array_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Type_ILTypeInstance_Binding.Register(app);
            System_IO_TextWriter_Binding.Register(app);
            System_Globalization_CultureInfo_Binding.Register(app);
            System_Collections_Generic_IEnumerable_1_ILTypeInstance_Binding.Register(app);
            System_Collections_Generic_IEnumerator_1_ILTypeInstance_Binding.Register(app);
            System_Collections_Generic_ICollection_1_KeyValuePair_2_Type_ILTypeInstance_Binding.Register(app);
            System_Collections_Generic_ICollection_1_KeyValuePair_2_Type_IDictionary_2_Type_ILTypeInstance_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Type_IDictionary_2_Type_MethodInfo_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Type_IList_1_ILTypeInstance_Binding.Register(app);
            System_Globalization_DateTimeFormatInfo_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Type_IDictionary_2_Type_ILTypeInstance_Binding.Register(app);
            System_DateTimeOffset_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Int32_IDictionary_2_Int32_Int32_Array_Binding.Register(app);
            System_Collections_Generic_IDictionary_2_Int32_IDictionary_2_Int32_Int32_Array_Binding.Register(app);
            System_Collections_Generic_IDictionary_2_Int32_Int32_Array_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Int32_Int32_Array_Binding.Register(app);
            System_UInt64_Binding.Register(app);
            System_Collections_Generic_Stack_1_Int32_Binding.Register(app);
            System_IO_StringReader_Binding.Register(app);
            System_ArgumentNullException_Binding.Register(app);
            System_Char_Binding.Register(app);
            System_Collections_Generic_Stack_1_ILTypeInstance_Binding.Register(app);
            System_Environment_Binding.Register(app);
            System_Globalization_NumberFormatInfo_Binding.Register(app);
            System_IO_TextReader_Binding.Register(app);
            Systems_Messaging_UnityEventExtension_Binding.Register(app);
            Systems_Messaging_MessagingManager_Binding.Register(app);
            Server_Controllers_DiziController_Binding.Register(app);
            Server_Controllers_DiziAdvController_Binding.Register(app);
            GameControllerServiceContainer_Binding.Register(app);
            GameWorld_Binding.Register(app);
            _GameClient_Models_Faction_Binding.Register(app);
            _GameClient_Models_Dizi_Binding.Register(app);
            BattleM_ISkillName_Binding.Register(app);
            BattleM_ISkill_Binding.Register(app);
            Core_IGameItem_Binding.Register(app);
            BattleM_IConditionValue_Binding.Register(app);
            System_ValueTuple_2_String_Color_Binding.Register(app);
            UnityEngine_UI_Text_Binding.Register(app);
            UnityEngine_UI_Image_Binding.Register(app);
            UnityEngine_UI_Scrollbar_Binding.Register(app);
            UnityEngine_UI_Graphic_Binding.Register(app);
            UnityEngine_Color_Binding.Register(app);
            UnityEngine_UI_Selectable_Binding.Register(app);
            _GameClient_Models_AutoAdventure_Binding.Register(app);
            System_ArgumentOutOfRangeException_Binding.Register(app);
            _GameClient_Models_AdvItemModel_Binding.Register(app);
            System_Collections_Generic_IReadOnlyCollection_1_IGameReward_Binding.Register(app);
            System_Collections_Generic_IReadOnlyList_1_IGameReward_Binding.Register(app);
            _GameClient_Models_Capable_Binding.Register(app);
            System_Collections_Generic_IEnumerable_1_String_Binding.Register(app);
            System_Collections_Generic_IEnumerator_1_String_Binding.Register(app);
            Server_Configs_Adventures_IAutoAdvMap_Binding.Register(app);
            System_Collections_Generic_IReadOnlyList_1_ILTypeInstance_Binding.Register(app);
            System_Collections_Generic_IReadOnlyCollection_1_ILTypeInstance_Binding.Register(app);
            System_Nullable_1_Int32_Binding.Register(app);
            System_Action_1_Int32_Binding.Register(app);
            System_Action_Binding.Register(app);
            System_Action_2_String_Int32_Binding.Register(app);
            Utls_XDebug_Binding.Register(app);
            System_Threading_Interlocked_Binding.Register(app);
            System_Linq_Enumerable_Binding.Register(app);
            System_Collections_Generic_List_1_Dizi_Binding.Register(app);
            _GameClient_Models_Dizi_Binding_StateModel_Binding.Register(app);
            Server_Configs_Skills_GradeValue_1_Int32_Binding.Register(app);
            BattleM_IWeapon_Binding.Register(app);
            _GameClient_Models_IDiziStamina_Binding.Register(app);
            System_ValueTuple_4_Int32_Int32_Int32_Int32_Binding.Register(app);
            Utls_SysTime_Binding.Register(app);
            System_Math_Binding.Register(app);
            System_TimeSpan_Binding.Register(app);
            Server_Controllers_RecruitController_Binding.Register(app);
            MainUi_Binding.Register(app);
            MainPageLayout_Binding.Register(app);
            Utls_DependencySingleton_1_ITestCaller_Binding.Register(app);
            Server_Configs_ITestCaller_Binding.Register(app);
            UiBuilder_Binding.Register(app);
            Server_Controllers_BattleSimController_Binding_Outcome_Binding.Register(app);
            Server_Controllers_BattleSimController_Binding_Outcome_Binding_Round_Binding.Register(app);
            Server_Controllers_BattleSimController_Binding_TestModel_Binding.Register(app);
            Server_Controllers_IBattleSimController_Binding.Register(app);
            System_ValueTuple_2_Server_Controllers_BattleSimController_Binding_PropCon_Binding_Skills_Boolean_Binding.Register(app);
            System_ValueTuple_2_Server_Controllers_BattleSimController_Binding_TestModel_Binding_Props_Int32_Binding.Register(app);
            Server_Controllers_BattleSimController_Binding_PropCon_Binding.Register(app);
            UnityEngine_UI_InputField_Binding.Register(app);
            Server_Controllers_BattleSimController_Binding_PropCon_Binding_Skill_Binding.Register(app);
            System_Action_1_Boolean_Binding.Register(app);
            System_Action_1_ValueTuple_2_Server_Controllers_BattleSimController_Binding_PropCon_Binding_Skills_Boolean_Binding.Register(app);
            System_Action_1_ValueTuple_2_Server_Controllers_BattleSimController_Binding_TestModel_Binding_Props_Int32_Binding.Register(app);
            Server_Controllers_TestAdventureController_Binding_AdvEvent_Binding.Register(app);
            Server_Controllers_TestAdventureController_Binding_Story_Binding.Register(app);
            Server_Controllers_TestAdventureController_Binding_BriefEvent_Binding.Register(app);
            Server_Controllers_TestAdventureController_Binding_OptionEvent_Binding.Register(app);
            Server_Controllers_TestAdventureController_Binding_BattleEvent_Binding.Register(app);
            Server_Controllers_TestAdventureController_Binding_RewardEvent_Binding.Register(app);
            Views_View_Binding.Register(app);
            System_Collections_Generic_List_1_ValueTuple_3_Int32_String_String_Binding.Register(app);
            System_ValueTuple_3_Int32_String_String_Binding.Register(app);
            Server_Controllers_TestAdventureController_Binding_DialogEvent_Binding.Register(app);
            System_Action_2_Int32_Int32_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Int32_ILTypeInstance_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Int32_ILTypeInstance_Array_Binding.Register(app);
            UnityEngine_Behaviour_Binding.Register(app);
            System_Linq_IGrouping_2_Int32_ILTypeInstance_Binding.Register(app);
            Server_Controllers_ITestDiziController_Binding.Register(app);
            Utls_DependencySingleton_1_IStaminaTimer_Binding.Register(app);
            Server_Configs_Characters_IStaminaTimer_Binding.Register(app);
            System_ValueTuple_2_String_Action_Binding.Register(app);
            System_DateTime_Binding.Register(app);
            BattleM_ConValue_Binding.Register(app);
            System_NotImplementedException_Binding.Register(app);
            System_ValueTuple_3_String_String_Int32_Binding.Register(app);
            System_Collections_Generic_List_1_ValueTuple_3_String_String_Int32_Binding.Register(app);
            System_Collections_Generic_IEnumerable_1_Dizi_Binding.Register(app);
            System_Collections_Generic_IEnumerator_1_Dizi_Binding.Register(app);
            System_Collections_Generic_IReadOnlyList_1_IWeapon_Binding.Register(app);
            System_Collections_Generic_IReadOnlyCollection_1_IWeapon_Binding.Register(app);
            System_Collections_Generic_IReadOnlyList_1_IArmor_Binding.Register(app);
            System_Collections_Generic_IReadOnlyCollection_1_IArmor_Binding.Register(app);
            System_ValueTuple_2_IMedicine_Int32_Binding.Register(app);
            System_Collections_Generic_IReadOnlyCollection_1_IGameItem_Binding.Register(app);
            System_Collections_Generic_IReadOnlyCollection_1_IAdvPackage_Binding.Register(app);
            System_Collections_Generic_List_1_ValueTuple_3_String_Int32_Int32_Binding.Register(app);
            System_ValueTuple_3_String_Int32_Int32_Binding.Register(app);
            System_Collections_Generic_List_1_ValueTuple_3_String_Int32_Int32_Binding_Enumerator_Binding.Register(app);
            Core_IStacking_1_IGameItem_Binding.Register(app);
            System_Action_4_String_Int32_Int32_Int32_Binding.Register(app);
            _GameClient_Models_RewardContainer_Binding.Register(app);
            Server_Configs_Adventures_IGameReward_Binding.Register(app);
            Server_Configs_Adventures_IAdvPackage_Binding.Register(app);
            Server_Controllers_ISkillController_Binding.Register(app);
            Server_Controllers_SkillController_Binding_Buff_Binding.Register(app);
            System_ValueTuple_2_String_String_Binding.Register(app);
            Server_Controllers_SkillController_Binding_Combat_Binding.Register(app);
            Server_Controllers_SkillController_Binding_Combat_Binding_Form_Binding.Register(app);
            Server_Controllers_SkillController_Binding_Combat_Binding_Exert_Binding.Register(app);
            Server_Controllers_SkillController_Binding_Combat_Binding_Form_Binding_ComboField_Binding.Register(app);
            Server_Controllers_SkillController_Binding_DodgeSkill_Binding.Register(app);
            Server_Controllers_SkillController_Binding_Force_Binding.Register(app);
            IMainUi_Binding.Register(app);
            UnityEngine_RectTransform_Binding.Register(app);

            ILRuntime.CLR.TypeSystem.CLRType __clrType = null;
            __clrType = (ILRuntime.CLR.TypeSystem.CLRType)app.GetType (typeof(UnityEngine.Vector3));
            s_UnityEngine_Vector3_Binding_Binder = __clrType.ValueTypeBinder as ILRuntime.Runtime.Enviorment.ValueTypeBinder<UnityEngine.Vector3>;
            __clrType = (ILRuntime.CLR.TypeSystem.CLRType)app.GetType (typeof(UnityEngine.Quaternion));
            s_UnityEngine_Quaternion_Binding_Binder = __clrType.ValueTypeBinder as ILRuntime.Runtime.Enviorment.ValueTypeBinder<UnityEngine.Quaternion>;
            __clrType = (ILRuntime.CLR.TypeSystem.CLRType)app.GetType (typeof(UnityEngine.Vector2));
            s_UnityEngine_Vector2_Binding_Binder = __clrType.ValueTypeBinder as ILRuntime.Runtime.Enviorment.ValueTypeBinder<UnityEngine.Vector2>;
        }

        /// <summary>
        /// Release the CLR binding, please invoke this BEFORE ILRuntime Appdomain destroy
        /// </summary>
        public static void Shutdown(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            s_UnityEngine_Vector3_Binding_Binder = null;
            s_UnityEngine_Quaternion_Binding_Binder = null;
            s_UnityEngine_Vector2_Binding_Binder = null;
        }
    }
}
