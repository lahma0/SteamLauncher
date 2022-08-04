//using System;
//using System.Linq;
//using System.Reflection;
//using System.Reflection.Emit;
//using System.Runtime.InteropServices;

//namespace SteamLauncher.SteamClient.Interop
//{
//    public class DynamicUnmanagedFuncCaller
//    {
//        private static Delegate GetFunction(Type d, IntPtr vTablePtr, int vTableIndex)
//        {
//            var functionPtr = Marshal.ReadIntPtr(vTablePtr,
//                                                 vTableIndex * IntPtr.Size);
//            return Marshal.GetDelegateForFunctionPointer(functionPtr, d);
//        }

//        public static Type GetDynamicMethodType(string methodName, params object[] args)
//        {
//            return GetDynamicMethodType(methodName, null, args);
//        }

//        public static Type GetDynamicMethodType(string methodName, Type returnType, params object[] args)
//        {
//            var asmNameStr = "SteamClientInterfaces";
//            var typeNameStr = "SteamClientInterfacesType";
//            var asmBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(asmNameStr),
//                                                                   AssemblyBuilderAccess.Run);
//            var modBuilder = asmBuilder.DefineDynamicModule(asmNameStr);
//            var typeBuilder = modBuilder.DefineType(typeNameStr, TypeAttributes.Class | TypeAttributes.Public);

//            var unmanagedFuncPtrCtor = typeof(UnmanagedFunctionPointerAttribute)
//                .GetConstructor(new Type[] { typeof(CallingConvention) });

//            var unmanagedFuncPtrBuilder = new CustomAttributeBuilder(unmanagedFuncPtrCtor!,
//                                                                     new object[]
//                                                                     {
//                                                                         CallingConvention.ThisCall, CharSet.Ansi
//                                                                     });

//            var methodBuilder = typeBuilder.DefineMethod(
//                name: methodName,
//                attributes: MethodAttributes.Public,
//                callingConvention: CallingConventions.ExplicitThis | CallingConventions.HasThis,
//                returnType: returnType == null ? typeof(void) : returnType,
//                parameterTypes: args.Select(p => p.GetType()).ToArray());

//            methodBuilder.SetCustomAttribute(unmanagedFuncPtrBuilder);

//            Type dynamicType = typeBuilder.CreateType();
//            if (dynamicType == null)
//                throw new NullReferenceException($"Could not create dynamic type for method '{methodName}'.");

//            return dynamicType;
//        }

//        public static TReturn Execute<TReturn>(Type dynamicType, IntPtr vTablePtr, int vTableIndex, params object[] args)
//        {
//            return (TReturn) GetFunction(dynamicType, vTablePtr, vTableIndex).DynamicInvoke(args);
//        }

//        public static void Execute(Type dynamicType, IntPtr vTablePtr, int vTableIndex, params object[] args)
//        {
//            GetFunction(dynamicType, vTablePtr, vTableIndex).DynamicInvoke(args);
//        }

//        public static TReturn Execute<TReturn>(IntPtr vTablePtr, string methodName, int vTableIndex, params object[] args)
//        {
//            var asmNameStr = "SteamClientInterfaces";
//            var typeNameStr = "SteamClientInterfacesType";
//            var asmBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(asmNameStr), 
//                                                                   AssemblyBuilderAccess.Run);
//            var modBuilder = asmBuilder.DefineDynamicModule(asmNameStr);
//            var typeBuilder = modBuilder.DefineType(typeNameStr, TypeAttributes.Class | TypeAttributes.Public);

//            var unmanagedFuncPtrCtor = typeof(UnmanagedFunctionPointerAttribute)
//                .GetConstructor(new Type[] {typeof(CallingConvention)});

//            var unmanagedFuncPtrBuilder = new CustomAttributeBuilder(unmanagedFuncPtrCtor!, 
//                                                                     new object[]
//                                                                     {
//                                                                         CallingConvention.ThisCall, CharSet.Ansi
//                                                                     });

//            var methodBuilder = typeBuilder.DefineMethod(
//                name: methodName, 
//                attributes: MethodAttributes.Public, 
//                callingConvention: CallingConventions.ExplicitThis | CallingConventions.HasThis, 
//                returnType: typeof(TReturn), 
//                parameterTypes: args.Select(p => p.GetType()).ToArray());

//            methodBuilder.SetCustomAttribute(unmanagedFuncPtrBuilder);

//            Type dynamicType = typeBuilder.CreateType();
//            if (dynamicType == null)
//                throw new NullReferenceException($"Could not create dynamic type for method '{methodName}'.");

//            return (TReturn)GetFunction(dynamicType, vTablePtr, vTableIndex).DynamicInvoke(args);
//        }

//        public static void ShowMsgBox()
//        {
//            var asmNameStr = "Win32";
//            var dllNameStr = "User32";

//            var asmName = new AssemblyName(asmNameStr);
//            var asmBuilder = AssemblyBuilder.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.Run);
//            //var asmBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.Run);
//            var modBuilder = asmBuilder.DefineDynamicModule(asmNameStr);
//            //var modBuilder = asmBuilder.DefineDynamicModule("Win32", emitSymbolInfo: false);
//            var typeBuilder = modBuilder.DefineType($"{asmNameStr}.{dllNameStr}", TypeAttributes.Class | TypeAttributes.Public);

//            // Optional: Use if you need to set properties on DllImportAttribute
//            var dllImportCtor = typeof(DllImportAttribute).GetConstructor(new Type[] { typeof(string) });
//            var dllImportBuilder = new CustomAttributeBuilder(dllImportCtor!, new object[] { $"{dllNameStr}.dll" });

//            var pInvokeBuilder = typeBuilder.DefinePInvokeMethod(
//                name: "ShowMessageBox",
//                dllName: $"{dllNameStr}.dll",
//                entryName: "MessageBoxW",
//                attributes: MethodAttributes.Static | MethodAttributes.Public,
//                callingConvention: CallingConventions.Standard,
//                returnType: typeof(int),  // typeof(void) if there is no return value.
//                // TODO: Construct this array from user input somehow:
//                parameterTypes: new Type[] { typeof(IntPtr), typeof(string), typeof(string), typeof(uint) },
//                nativeCallConv: CallingConvention.Winapi,
//                nativeCharSet: CharSet.Unicode);

//            pInvokeBuilder.SetCustomAttribute(dllImportBuilder);

//            Type user32Type = typeBuilder.CreateType();
            
//            if (user32Type == null)
//                throw new NullReferenceException($"Could not create type '{nameof(user32Type)}'.");

//            // ReSharper disable once InconsistentNaming
//            // ReSharper disable once IdentifierTypo
//            const uint MB_YESNOCANCEL = 3;

//            user32Type?
//                .GetMethod("ShowMessageBox", BindingFlags.Static | BindingFlags.Public)?
//                .Invoke(null, new object[] { IntPtr.Zero, "Message Text", "Message Caption", MB_YESNOCANCEL });
//        }
//    }
//}
