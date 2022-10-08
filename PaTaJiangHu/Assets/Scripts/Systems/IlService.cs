using System;
using System.Linq;
using ILRuntime.CLR.Method;
using ILRuntime.CLR.TypeSystem;
using Utls;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;

namespace Systems
{
    public class IlService
    {
        public IlService(AppDomain appDomain)
        {
            AppDomain = appDomain;
        }

        private AppDomain AppDomain { get; }

        public Type[] LoadedTypes()
        {
            var types = AppDomain.LoadedTypes.Values.ToList();
            return types.Select(t => t.ReflectionType).ToArray();
        }

        public IType GetLoadedTypes(string className) => AppDomain.LoadedTypes[className];

        public void Invoke(IMethod method, object instance, params object[] param) =>
            AppDomain.Invoke(method, instance, param);
    }
}