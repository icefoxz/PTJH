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
    unsafe class System_Linq_Enumerable_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            MethodBase method;
            Type[] args;
            Type type = typeof(System.Linq.Enumerable);
            Dictionary<string, List<MethodInfo>> genericMethods = new Dictionary<string, List<MethodInfo>>();
            List<MethodInfo> lst = null;                    
            foreach(var m in type.GetMethods())
            {
                if(m.IsGenericMethodDefinition)
                {
                    if (!genericMethods.TryGetValue(m.Name, out lst))
                    {
                        lst = new List<MethodInfo>();
                        genericMethods[m.Name] = lst;
                    }
                    lst.Add(m);
                }
            }
            args = new Type[]{typeof(_GameClient.Models.Dizi)};
            if (genericMethods.TryGetValue("ToList", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(System.Collections.Generic.List<_GameClient.Models.Dizi>), typeof(System.Collections.Generic.IEnumerable<_GameClient.Models.Dizi>)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, ToList_0);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(System.Int32)};
            if (genericMethods.TryGetValue("First", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(System.Int32), typeof(System.Collections.Generic.IEnumerable<System.Int32>)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, First_1);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(System.ValueTuple<System.Int32, System.String, System.String>)};
            if (genericMethods.TryGetValue("ToList", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(System.Collections.Generic.List<System.ValueTuple<System.Int32, System.String, System.String>>), typeof(System.Collections.Generic.IEnumerable<System.ValueTuple<System.Int32, System.String, System.String>>)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, ToList_2);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(ILRuntime.Runtime.Intepreter.ILTypeInstance), typeof(System.Int32)};
            if (genericMethods.TryGetValue("GroupBy", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(System.Collections.Generic.IEnumerable<System.Linq.IGrouping<System.Int32, ILRuntime.Runtime.Intepreter.ILTypeInstance>>), typeof(System.Collections.Generic.IEnumerable<ILRuntime.Runtime.Intepreter.ILTypeInstance>), typeof(System.Func<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Int32>)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, GroupBy_3);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(System.Linq.IGrouping<System.Int32, ILRuntime.Runtime.Intepreter.ILTypeInstance>), typeof(ILRuntime.Runtime.Intepreter.ILTypeInstance)};
            if (genericMethods.TryGetValue("Select", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(System.Collections.Generic.IEnumerable<ILRuntime.Runtime.Intepreter.ILTypeInstance>), typeof(System.Collections.Generic.IEnumerable<System.Linq.IGrouping<System.Int32, ILRuntime.Runtime.Intepreter.ILTypeInstance>>), typeof(System.Func<System.Linq.IGrouping<System.Int32, ILRuntime.Runtime.Intepreter.ILTypeInstance>, ILRuntime.Runtime.Intepreter.ILTypeInstance>)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, Select_4);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(ILRuntime.Runtime.Intepreter.ILTypeInstance), typeof(ILRuntime.Runtime.Intepreter.ILTypeInstance), typeof(System.Int32), typeof(ILRuntime.Runtime.Intepreter.ILTypeInstance)};
            if (genericMethods.TryGetValue("Join", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(System.Collections.Generic.IEnumerable<ILRuntime.Runtime.Intepreter.ILTypeInstance>), typeof(System.Collections.Generic.IEnumerable<ILRuntime.Runtime.Intepreter.ILTypeInstance>), typeof(System.Collections.Generic.IEnumerable<ILRuntime.Runtime.Intepreter.ILTypeInstance>), typeof(System.Func<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Int32>), typeof(System.Func<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Int32>), typeof(System.Func<ILRuntime.Runtime.Intepreter.ILTypeInstance, ILRuntime.Runtime.Intepreter.ILTypeInstance, ILRuntime.Runtime.Intepreter.ILTypeInstance>)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, Join_5);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(ILRuntime.Runtime.Intepreter.ILTypeInstance), typeof(System.Int32), typeof(ILRuntime.Runtime.Intepreter.ILTypeInstance[])};
            if (genericMethods.TryGetValue("ToDictionary", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(System.Collections.Generic.Dictionary<System.Int32, ILRuntime.Runtime.Intepreter.ILTypeInstance[]>), typeof(System.Collections.Generic.IEnumerable<ILRuntime.Runtime.Intepreter.ILTypeInstance>), typeof(System.Func<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Int32>), typeof(System.Func<ILRuntime.Runtime.Intepreter.ILTypeInstance, ILRuntime.Runtime.Intepreter.ILTypeInstance[]>)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, ToDictionary_6);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(ILRuntime.Runtime.Intepreter.ILTypeInstance)};
            if (genericMethods.TryGetValue("ToArray", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(ILRuntime.Runtime.Intepreter.ILTypeInstance[]), typeof(System.Collections.Generic.IEnumerable<ILRuntime.Runtime.Intepreter.ILTypeInstance>)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, ToArray_7);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(System.Int32)};
            if (genericMethods.TryGetValue("Cast", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(System.Collections.Generic.IEnumerable<System.Int32>), typeof(System.Collections.IEnumerable)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, Cast_8);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(System.Int32), typeof(System.ValueTuple<System.String, System.Action>)};
            if (genericMethods.TryGetValue("Select", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(System.Collections.Generic.IEnumerable<System.ValueTuple<System.String, System.Action>>), typeof(System.Collections.Generic.IEnumerable<System.Int32>), typeof(System.Func<System.Int32, System.ValueTuple<System.String, System.Action>>)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, Select_9);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(System.ValueTuple<System.String, System.Action>)};
            if (genericMethods.TryGetValue("ToArray", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(System.ValueTuple<System.String, System.Action>[]), typeof(System.Collections.Generic.IEnumerable<System.ValueTuple<System.String, System.Action>>)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, ToArray_10);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(System.Int32)};
            if (genericMethods.TryGetValue("Concat", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(System.Collections.Generic.IEnumerable<System.Int32>), typeof(System.Collections.Generic.IEnumerable<System.Int32>), typeof(System.Collections.Generic.IEnumerable<System.Int32>)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, Concat_11);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(System.Int32)};
            if (genericMethods.TryGetValue("Distinct", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(System.Collections.Generic.IEnumerable<System.Int32>), typeof(System.Collections.Generic.IEnumerable<System.Int32>)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, Distinct_12);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(System.Int32)};
            if (genericMethods.TryGetValue("ToArray", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(System.Int32[]), typeof(System.Collections.Generic.IEnumerable<System.Int32>)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, ToArray_13);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(ILRuntime.Runtime.Intepreter.ILTypeInstance)};
            if (genericMethods.TryGetValue("Any", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(System.Boolean), typeof(System.Collections.Generic.IEnumerable<ILRuntime.Runtime.Intepreter.ILTypeInstance>), typeof(System.Func<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Boolean>)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, Any_14);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(ILRuntime.Runtime.Intepreter.ILTypeInstance)};
            if (genericMethods.TryGetValue("FirstOrDefault", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(ILRuntime.Runtime.Intepreter.ILTypeInstance), typeof(System.Collections.Generic.IEnumerable<ILRuntime.Runtime.Intepreter.ILTypeInstance>), typeof(System.Func<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Boolean>)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, FirstOrDefault_15);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(ILRuntime.Runtime.Intepreter.ILTypeInstance)};
            if (genericMethods.TryGetValue("Where", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(System.Collections.Generic.IEnumerable<ILRuntime.Runtime.Intepreter.ILTypeInstance>), typeof(System.Collections.Generic.IEnumerable<ILRuntime.Runtime.Intepreter.ILTypeInstance>), typeof(System.Func<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Boolean>)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, Where_16);

                        break;
                    }
                }
            }
            args = new Type[]{typeof(ILRuntime.Runtime.Intepreter.ILTypeInstance)};
            if (genericMethods.TryGetValue("ToList", out lst))
            {
                foreach(var m in lst)
                {
                    if(m.MatchGenericParameters(args, typeof(System.Collections.Generic.List<ILRuntime.Runtime.Intepreter.ILTypeInstance>), typeof(System.Collections.Generic.IEnumerable<ILRuntime.Runtime.Intepreter.ILTypeInstance>)))
                    {
                        method = m.MakeGenericMethod(args);
                        app.RegisterCLRMethodRedirection(method, ToList_17);

                        break;
                    }
                }
            }


        }


        static StackObject* ToList_0(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Collections.Generic.IEnumerable<_GameClient.Models.Dizi> @source = (System.Collections.Generic.IEnumerable<_GameClient.Models.Dizi>)typeof(System.Collections.Generic.IEnumerable<_GameClient.Models.Dizi>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = System.Linq.Enumerable.ToList<_GameClient.Models.Dizi>(@source);

            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance);
            }
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* First_1(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Collections.Generic.IEnumerable<System.Int32> @source = (System.Collections.Generic.IEnumerable<System.Int32>)typeof(System.Collections.Generic.IEnumerable<System.Int32>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = System.Linq.Enumerable.First<System.Int32>(@source);

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method;
            return __ret + 1;
        }

        static StackObject* ToList_2(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Collections.Generic.IEnumerable<System.ValueTuple<System.Int32, System.String, System.String>> @source = (System.Collections.Generic.IEnumerable<System.ValueTuple<System.Int32, System.String, System.String>>)typeof(System.Collections.Generic.IEnumerable<System.ValueTuple<System.Int32, System.String, System.String>>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = System.Linq.Enumerable.ToList<System.ValueTuple<System.Int32, System.String, System.String>>(@source);

            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance);
            }
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* GroupBy_3(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Func<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Int32> @keySelector = (System.Func<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Int32>)typeof(System.Func<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Int32>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Collections.Generic.IEnumerable<ILRuntime.Runtime.Intepreter.ILTypeInstance> @source = (System.Collections.Generic.IEnumerable<ILRuntime.Runtime.Intepreter.ILTypeInstance>)typeof(System.Collections.Generic.IEnumerable<ILRuntime.Runtime.Intepreter.ILTypeInstance>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = System.Linq.Enumerable.GroupBy<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Int32>(@source, @keySelector);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* Select_4(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Func<System.Linq.IGrouping<System.Int32, ILRuntime.Runtime.Intepreter.ILTypeInstance>, ILRuntime.Runtime.Intepreter.ILTypeInstance> @selector = (System.Func<System.Linq.IGrouping<System.Int32, ILRuntime.Runtime.Intepreter.ILTypeInstance>, ILRuntime.Runtime.Intepreter.ILTypeInstance>)typeof(System.Func<System.Linq.IGrouping<System.Int32, ILRuntime.Runtime.Intepreter.ILTypeInstance>, ILRuntime.Runtime.Intepreter.ILTypeInstance>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Collections.Generic.IEnumerable<System.Linq.IGrouping<System.Int32, ILRuntime.Runtime.Intepreter.ILTypeInstance>> @source = (System.Collections.Generic.IEnumerable<System.Linq.IGrouping<System.Int32, ILRuntime.Runtime.Intepreter.ILTypeInstance>>)typeof(System.Collections.Generic.IEnumerable<System.Linq.IGrouping<System.Int32, ILRuntime.Runtime.Intepreter.ILTypeInstance>>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = System.Linq.Enumerable.Select<System.Linq.IGrouping<System.Int32, ILRuntime.Runtime.Intepreter.ILTypeInstance>, ILRuntime.Runtime.Intepreter.ILTypeInstance>(@source, @selector);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* Join_5(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 5);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Func<ILRuntime.Runtime.Intepreter.ILTypeInstance, ILRuntime.Runtime.Intepreter.ILTypeInstance, ILRuntime.Runtime.Intepreter.ILTypeInstance> @resultSelector = (System.Func<ILRuntime.Runtime.Intepreter.ILTypeInstance, ILRuntime.Runtime.Intepreter.ILTypeInstance, ILRuntime.Runtime.Intepreter.ILTypeInstance>)typeof(System.Func<ILRuntime.Runtime.Intepreter.ILTypeInstance, ILRuntime.Runtime.Intepreter.ILTypeInstance, ILRuntime.Runtime.Intepreter.ILTypeInstance>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Func<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Int32> @innerKeySelector = (System.Func<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Int32>)typeof(System.Func<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Int32>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.Func<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Int32> @outerKeySelector = (System.Func<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Int32>)typeof(System.Func<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Int32>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 4);
            System.Collections.Generic.IEnumerable<ILRuntime.Runtime.Intepreter.ILTypeInstance> @inner = (System.Collections.Generic.IEnumerable<ILRuntime.Runtime.Intepreter.ILTypeInstance>)typeof(System.Collections.Generic.IEnumerable<ILRuntime.Runtime.Intepreter.ILTypeInstance>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 5);
            System.Collections.Generic.IEnumerable<ILRuntime.Runtime.Intepreter.ILTypeInstance> @outer = (System.Collections.Generic.IEnumerable<ILRuntime.Runtime.Intepreter.ILTypeInstance>)typeof(System.Collections.Generic.IEnumerable<ILRuntime.Runtime.Intepreter.ILTypeInstance>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = System.Linq.Enumerable.Join<ILRuntime.Runtime.Intepreter.ILTypeInstance, ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Int32, ILRuntime.Runtime.Intepreter.ILTypeInstance>(@outer, @inner, @outerKeySelector, @innerKeySelector, @resultSelector);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* ToDictionary_6(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 3);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Func<ILRuntime.Runtime.Intepreter.ILTypeInstance, ILRuntime.Runtime.Intepreter.ILTypeInstance[]> @elementSelector = (System.Func<ILRuntime.Runtime.Intepreter.ILTypeInstance, ILRuntime.Runtime.Intepreter.ILTypeInstance[]>)typeof(System.Func<ILRuntime.Runtime.Intepreter.ILTypeInstance, ILRuntime.Runtime.Intepreter.ILTypeInstance[]>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Func<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Int32> @keySelector = (System.Func<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Int32>)typeof(System.Func<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Int32>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 3);
            System.Collections.Generic.IEnumerable<ILRuntime.Runtime.Intepreter.ILTypeInstance> @source = (System.Collections.Generic.IEnumerable<ILRuntime.Runtime.Intepreter.ILTypeInstance>)typeof(System.Collections.Generic.IEnumerable<ILRuntime.Runtime.Intepreter.ILTypeInstance>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = System.Linq.Enumerable.ToDictionary<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Int32, ILRuntime.Runtime.Intepreter.ILTypeInstance[]>(@source, @keySelector, @elementSelector);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* ToArray_7(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Collections.Generic.IEnumerable<ILRuntime.Runtime.Intepreter.ILTypeInstance> @source = (System.Collections.Generic.IEnumerable<ILRuntime.Runtime.Intepreter.ILTypeInstance>)typeof(System.Collections.Generic.IEnumerable<ILRuntime.Runtime.Intepreter.ILTypeInstance>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = System.Linq.Enumerable.ToArray<ILRuntime.Runtime.Intepreter.ILTypeInstance>(@source);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* Cast_8(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Collections.IEnumerable @source = (System.Collections.IEnumerable)typeof(System.Collections.IEnumerable).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = System.Linq.Enumerable.Cast<System.Int32>(@source);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* Select_9(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Func<System.Int32, System.ValueTuple<System.String, System.Action>> @selector = (System.Func<System.Int32, System.ValueTuple<System.String, System.Action>>)typeof(System.Func<System.Int32, System.ValueTuple<System.String, System.Action>>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Collections.Generic.IEnumerable<System.Int32> @source = (System.Collections.Generic.IEnumerable<System.Int32>)typeof(System.Collections.Generic.IEnumerable<System.Int32>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = System.Linq.Enumerable.Select<System.Int32, System.ValueTuple<System.String, System.Action>>(@source, @selector);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* ToArray_10(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Collections.Generic.IEnumerable<System.ValueTuple<System.String, System.Action>> @source = (System.Collections.Generic.IEnumerable<System.ValueTuple<System.String, System.Action>>)typeof(System.Collections.Generic.IEnumerable<System.ValueTuple<System.String, System.Action>>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = System.Linq.Enumerable.ToArray<System.ValueTuple<System.String, System.Action>>(@source);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* Concat_11(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Collections.Generic.IEnumerable<System.Int32> @second = (System.Collections.Generic.IEnumerable<System.Int32>)typeof(System.Collections.Generic.IEnumerable<System.Int32>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Collections.Generic.IEnumerable<System.Int32> @first = (System.Collections.Generic.IEnumerable<System.Int32>)typeof(System.Collections.Generic.IEnumerable<System.Int32>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = System.Linq.Enumerable.Concat<System.Int32>(@first, @second);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* Distinct_12(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Collections.Generic.IEnumerable<System.Int32> @source = (System.Collections.Generic.IEnumerable<System.Int32>)typeof(System.Collections.Generic.IEnumerable<System.Int32>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = System.Linq.Enumerable.Distinct<System.Int32>(@source);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* ToArray_13(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Collections.Generic.IEnumerable<System.Int32> @source = (System.Collections.Generic.IEnumerable<System.Int32>)typeof(System.Collections.Generic.IEnumerable<System.Int32>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = System.Linq.Enumerable.ToArray<System.Int32>(@source);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* Any_14(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Func<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Boolean> @predicate = (System.Func<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Boolean>)typeof(System.Func<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Boolean>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Collections.Generic.IEnumerable<ILRuntime.Runtime.Intepreter.ILTypeInstance> @source = (System.Collections.Generic.IEnumerable<ILRuntime.Runtime.Intepreter.ILTypeInstance>)typeof(System.Collections.Generic.IEnumerable<ILRuntime.Runtime.Intepreter.ILTypeInstance>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = System.Linq.Enumerable.Any<ILRuntime.Runtime.Intepreter.ILTypeInstance>(@source, @predicate);

            __ret->ObjectType = ObjectTypes.Integer;
            __ret->Value = result_of_this_method ? 1 : 0;
            return __ret + 1;
        }

        static StackObject* FirstOrDefault_15(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Func<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Boolean> @predicate = (System.Func<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Boolean>)typeof(System.Func<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Boolean>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Collections.Generic.IEnumerable<ILRuntime.Runtime.Intepreter.ILTypeInstance> @source = (System.Collections.Generic.IEnumerable<ILRuntime.Runtime.Intepreter.ILTypeInstance>)typeof(System.Collections.Generic.IEnumerable<ILRuntime.Runtime.Intepreter.ILTypeInstance>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = System.Linq.Enumerable.FirstOrDefault<ILRuntime.Runtime.Intepreter.ILTypeInstance>(@source, @predicate);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* Where_16(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 2);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Func<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Boolean> @predicate = (System.Func<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Boolean>)typeof(System.Func<ILRuntime.Runtime.Intepreter.ILTypeInstance, System.Boolean>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)8);
            __intp.Free(ptr_of_this_method);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 2);
            System.Collections.Generic.IEnumerable<ILRuntime.Runtime.Intepreter.ILTypeInstance> @source = (System.Collections.Generic.IEnumerable<ILRuntime.Runtime.Intepreter.ILTypeInstance>)typeof(System.Collections.Generic.IEnumerable<ILRuntime.Runtime.Intepreter.ILTypeInstance>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = System.Linq.Enumerable.Where<ILRuntime.Runtime.Intepreter.ILTypeInstance>(@source, @predicate);

            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }

        static StackObject* ToList_17(ILIntepreter __intp, StackObject* __esp, AutoList __mStack, CLRMethod __method, bool isNewObj)
        {
            ILRuntime.Runtime.Enviorment.AppDomain __domain = __intp.AppDomain;
            StackObject* ptr_of_this_method;
            StackObject* __ret = ILIntepreter.Minus(__esp, 1);

            ptr_of_this_method = ILIntepreter.Minus(__esp, 1);
            System.Collections.Generic.IEnumerable<ILRuntime.Runtime.Intepreter.ILTypeInstance> @source = (System.Collections.Generic.IEnumerable<ILRuntime.Runtime.Intepreter.ILTypeInstance>)typeof(System.Collections.Generic.IEnumerable<ILRuntime.Runtime.Intepreter.ILTypeInstance>).CheckCLRTypes(StackObject.ToObject(ptr_of_this_method, __domain, __mStack), (CLR.Utils.Extensions.TypeFlags)0);
            __intp.Free(ptr_of_this_method);


            var result_of_this_method = System.Linq.Enumerable.ToList<ILRuntime.Runtime.Intepreter.ILTypeInstance>(@source);

            object obj_result_of_this_method = result_of_this_method;
            if(obj_result_of_this_method is CrossBindingAdaptorType)
            {    
                return ILIntepreter.PushObject(__ret, __mStack, ((CrossBindingAdaptorType)obj_result_of_this_method).ILInstance);
            }
            return ILIntepreter.PushObject(__ret, __mStack, result_of_this_method);
        }



    }
}
