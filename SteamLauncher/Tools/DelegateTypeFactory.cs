using SteamLauncher.DataStore.VTablesStore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace SteamLauncher.Tools
{
    public class DelegateTypeFactory
    {
        private readonly ModuleBuilder _module;

        public DelegateTypeFactory(string assemblyName = "DelegateTypeFactory")
        {
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(
                new AssemblyName(assemblyName), AssemblyBuilderAccess.RunAndCollect);
            _module = assemblyBuilder.DefineDynamicModule(assemblyName);
        }

        public Type CreateDelegateType(MethodInfo method, string methodName = null)
        {
            if (method == null || method.DeclaringType == null)
                return null;

            var nameBase = $"{method.DeclaringType.Name}{method.Name}";
            var name = GetUniqueName(nameBase);

            if (!string.IsNullOrWhiteSpace(methodName) && _module.GetType(methodName) == null)
                name = methodName;

            var typeBuilder = _module.DefineType(name,
                                                 TypeAttributes.Sealed |
                                                 TypeAttributes.Public |
                                                 TypeAttributes.AutoClass,
                                                 typeof(MulticastDelegate));

            var constructor = typeBuilder.DefineConstructor(
                MethodAttributes.RTSpecialName | MethodAttributes.HideBySig | MethodAttributes.Public,
                CallingConventions.Standard, new[] { typeof(object), typeof(IntPtr) });

            constructor.SetImplementationFlags(MethodImplAttributes.CodeTypeMask);

            var parameters = method.GetParameters();

            var parameterTypes = parameters.Select(p => p.ParameterType).Prepend(typeof(IntPtr)).ToArray();

            var invokeMethod = typeBuilder.DefineMethod("Invoke",
                                                        MethodAttributes.HideBySig | MethodAttributes.Virtual |
                                                        MethodAttributes.VtableLayoutMask | MethodAttributes.Public,
                                                        method.ReturnType,
                                                        parameterTypes);

            invokeMethod.SetImplementationFlags(MethodImplAttributes.CodeTypeMask);

            invokeMethod.DefineParameter(1, ParameterAttributes.None, "thisPtr");

            for (var i = 1; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                invokeMethod.DefineParameter(i + 1, ParameterAttributes.None, parameter.Name);
            }

            var attributeType = typeof(UnmanagedFunctionPointerAttribute);
            var callingConventionInfo = attributeType.GetConstructor(new[] { typeof(CallingConvention) });
            var charSetFieldInfo = new[] { attributeType.GetField("CharSet") };

            if (callingConventionInfo == null)
                throw new Exception($"Could not retrieve {nameof(CallingConvention)} constructor.");

            var unmanagedFuncPtrBuilder = new CustomAttributeBuilder(callingConventionInfo,
                                                                     new object[] { CallingConvention.ThisCall });

            invokeMethod.SetCustomAttribute(unmanagedFuncPtrBuilder);

            return typeBuilder.CreateType();
        }

        public Type CreateDelegateType(Type returnType, params Type[] paramTypes)
        {
            return CreateDelegateType(null, returnType, paramTypes);
        }

        public Type CreateDelegateType(VtEntry vtEntry)
        {
            var name = vtEntry.Name;

            if (!IsValidIdentifier(name))
                name = RandomIdentifier(10);

            if (_module.GetType(name) != null)
                name = GetUniqueName(name);

            var paramList = new List<Type>();
            foreach (var entry in vtEntry.Parameters)
            {
                var param = entry.ParamType;
                if (entry.IsArrayType)
                    param = param.MakeArrayType();
                if (entry.IsByRef)
                {
                    param = param.MakeByRefType();
                }

                paramList.Add(param);
            }

            var parameters = paramList.ToArray();

            var typeBuilder = _module.DefineType(name,
                                                 TypeAttributes.Sealed |
                                                 TypeAttributes.Public |
                                                 TypeAttributes.AutoClass,
                                                 typeof(MulticastDelegate));

            var constructor = typeBuilder.DefineConstructor(
                MethodAttributes.RTSpecialName | MethodAttributes.HideBySig | MethodAttributes.Public,
                CallingConventions.Standard, new[] { typeof(object), typeof(IntPtr) });

            constructor.SetImplementationFlags(MethodImplAttributes.CodeTypeMask);

            var invokeMethod = typeBuilder.DefineMethod("Invoke",
                                                        MethodAttributes.HideBySig | MethodAttributes.Virtual |
                                                        MethodAttributes.VtableLayoutMask | MethodAttributes.Public,
                                                        vtEntry.ReturnType,
                                                        parameters);

            invokeMethod.SetImplementationFlags(MethodImplAttributes.CodeTypeMask);

            var paramNum = 1;
            foreach (var entry in vtEntry.Parameters)
            {
                string paramName = null;
                if (IsValidIdentifier(entry.Name))
                    paramName = entry.Name;

                var param = invokeMethod.DefineParameter(paramNum, entry.ParamAttributes, paramName);
                if (entry.AttributeBuilders != null)
                {
                    foreach (var attr in entry.AttributeBuilders)
                    {
                        param.SetCustomAttribute(attr);
                    }
                }

                paramNum += 1;
            }

            foreach (var attr in vtEntry.MethodAttributeBuilders)
            {
                invokeMethod.SetCustomAttribute(attr);

            }

            return typeBuilder.CreateType();
        }

        public Type CreateDelegateType(string methodName, Type returnType, params Type[] paramTypes)
        {
            // When calling this function, this is how you define one of the parameters as a "ref" type
            //paramTypes[1] = paramTypes[1].MakeByRefType();

            // When calling this function, this is how you define one of the parameters is an array of that type
            //paramTypes[1] = paramTypes[1].MakeArrayType();

            var name = methodName;

            if (string.IsNullOrWhiteSpace(methodName))
                name = $"{_module.Name}DynamicDelegate";

            if (_module.GetType(name) != null)
                name = GetUniqueName(methodName);

            var typeBuilder = _module.DefineType(name,
                                                 TypeAttributes.Sealed |
                                                 TypeAttributes.Public |
                                                 TypeAttributes.AutoClass,
                                                 typeof(MulticastDelegate));

            var constructor = typeBuilder.DefineConstructor(
                MethodAttributes.RTSpecialName | MethodAttributes.HideBySig | MethodAttributes.Public,
                CallingConventions.Standard, new[] { typeof(object), typeof(IntPtr) });

            constructor.SetImplementationFlags(MethodImplAttributes.CodeTypeMask);

            var invokeMethod = typeBuilder.DefineMethod("Invoke",
                                                        MethodAttributes.HideBySig | MethodAttributes.Virtual |
                                                        MethodAttributes.VtableLayoutMask | MethodAttributes.Public,
                                                        returnType,
                                                        paramTypes);

            invokeMethod.SetImplementationFlags(MethodImplAttributes.CodeTypeMask);

            // This is how you would name parameter #1 if you wanted to:
            //invokeMethod.DefineParameter(1, ParameterAttributes.None, "thisPtr");

            // START - This is how you would set the "[MarshalAs(UnmanagedType.LPUTF8Str)]" attribute on parameter #2
            //
            //var marshalAsAttributeType = typeof(MarshalAsAttribute);
            //var marshalAsConstructorInfo = marshalAsAttributeType.GetConstructor(new[] { typeof(UnmanagedType) });
            //if (marshalAsConstructorInfo == null)
            //    throw new Exception($"Could not retrieve {nameof(UnmanagedType)} constructor.");

            //var marshalAsAttributeBuilder = new CustomAttributeBuilder(marshalAsConstructorInfo, new object[]
            //{
            //    UnmanagedType.LPUTF8Str
            //});

            //var param2 = invokeMethod.DefineParameter(2, ParameterAttributes.HasFieldMarshal, null);
            //param2.SetCustomAttribute(marshalAsAttributeBuilder);
            //
            // END

            var attributeType = typeof(UnmanagedFunctionPointerAttribute);
            var callingConventionConstructorInfo = attributeType.GetConstructor(new[] { typeof(CallingConvention) });

            if (callingConventionConstructorInfo == null)
                throw new Exception($"Could not retrieve {nameof(CallingConvention)} constructor.");

            var unmanagedFuncPtrBuilder = new CustomAttributeBuilder(callingConventionConstructorInfo,
                                                                     new object[] { CallingConvention.ThisCall });

            // START - If you also want to set the "CharSet = CharSet.Ansi" attribute on the delegate, substitute the
            // above assignment with this:
            //
            //var charSetFieldInfo = new[] { attributeType.GetField("CharSet") };
            //var unmanagedFuncPtrBuilder = new CustomAttributeBuilder(callingConventionInfo,
            //                                                         new object[] { CallingConvention.ThisCall }, 
            //                                                         charSetFieldInfo, 
            //                                                         new object[] {CharSet.Ansi});
            //
            // END

            invokeMethod.SetCustomAttribute(unmanagedFuncPtrBuilder);

            return typeBuilder.CreateType();
        }

        private string GetUniqueName(string nameBase)
        {
            var number = 2;
            var name = nameBase;
            while (_module.GetType(name) != null)
                name = nameBase + number++;
            return name;
        }

        private string GetUniqueMethodName(string nameBase)
        {
            if (string.IsNullOrWhiteSpace(nameBase))
                nameBase = RandomIdentifier(10);

            var number = 2;
            var name = nameBase;
            while (_module.GetType(name) != null)
                name = nameBase + number++;
            return name;
        }

        public static bool IsValidIdentifier(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;
            if (!char.IsLetter(text[0]) && text[0] != '_')
                return false;
            for (var ix = 1; ix < text.Length; ++ix)
                if (!char.IsLetterOrDigit(text[ix]) && text[ix] != '_')
                    return false;
            return true;
        }

        private static Random _random;

        private static Random Random
        {
            get { return _random ??= new Random(); }
        }

        private static string RandomIdentifier(int length)
        {

            const string lettersUppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lettersLowercase = "abcdefghijklmnopqrstuvwxyz";
            const string numbers = "0123456789";
            const string alphanumeric = lettersLowercase + numbers;
            var identifier = lettersUppercase[Random.Next(lettersUppercase.Length)].ToString();
            return identifier + new string(
                Enumerable.Repeat(alphanumeric, length - 1).Select(s => s[Random.Next(s.Length)]).ToArray());
        }

        public static Delegate CreateDelegateFromTypeFactory(object instance, MethodInfo method)
        {
            var delegateFactory = new DelegateTypeFactory();
            return Delegate.CreateDelegate(delegateFactory.CreateDelegateType(method), instance, method);
        }
    }
}
