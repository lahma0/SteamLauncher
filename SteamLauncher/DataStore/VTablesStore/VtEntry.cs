using SteamLauncher.SteamClient;
using SteamLauncher.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Xml.Serialization;

namespace SteamLauncher.DataStore.VTablesStore
{
    public class VtEntry
    {
        public const int NON_EXISTENT_INDEX = -1;

        private VtEntry()
        {

        }

        /// <summary>
        /// Create a new <see cref="VtEntry"/> object. By not defining the optional parameter <paramref
        /// name="nonBetaIndex"/>, its default value will be used which indicates that this is a new vtable entry that
        /// exists only in the Steam beta (and is therefore non-existent in the non-beta Steam). It is also possible,
        /// but highly unlikely, that a vtable entry could be removed in the beta in which case you would assign
        /// NON_EXISTENT_INDEX (-1) to <paramref name="betaIndex"/>.
        /// </summary>
        /// <param name="name">Name of the vtable entry's function.</param>
        /// <param name="parameters">A list of <see cref="VtEntryParam"/> objects which define information about each
        /// parameter.</param>
        /// <param name="returnType">The Type of the method's return value.</param>
        /// <param name="betaIndex">The index of this vtable entry in the beta version of Steam.</param>
        /// <param name="nonBetaIndex">The index of this vtable entry in the non-beta version of Steam.</param>
        /// <param name="isUnmanagedFunctionPointer">Indicates this entry is marked with the 'UnmanagedFunctionPointer'
        /// attribute.</param>
        /// <param name="callingConvention">Defines this function's calling convention.</param>
        /// <param name="charSet">Defines this function's character set.</param>
        /// <param name="isReturnTypePtrToUtf8String">Defines whether the ReturnType is an IntPtr which must be
        /// marshaled to a UTF-8 string.</param>
        public VtEntry(string name,
                       List<VtEntryParam> parameters,
                       Type returnType,
                       int betaIndex,
                       int nonBetaIndex = NON_EXISTENT_INDEX,
                       bool isUnmanagedFunctionPointer = true,
                       CallingConvention callingConvention = CallingConvention.ThisCall,
                       CharSet charSet = CharSet.Ansi,
                       bool isReturnTypePtrToUtf8String = false)
        {
            Name = name;
            Parameters = parameters;
            ReturnType = returnType;
            BetaIndex = betaIndex;
            NonBetaIndex = nonBetaIndex;
            IsUnmanagedFunctionPointer = isUnmanagedFunctionPointer;
            CallingConvention = callingConvention;
            CharSet = charSet;
            IsReturnTypePtrToUtf8String = isReturnTypePtrToUtf8String;
            //Init();
        }

        public int GetIndex(bool isBeta)
        {
            if ((isBeta && BetaIndex == NON_EXISTENT_INDEX) ||
                (!isBeta && NonBetaIndex == NON_EXISTENT_INDEX))
            {
                throw new NotImplementedException($"This vtable entry is non-existent in the " +
                                                  $"version of the application you're interfacing " +
                                                  $"with ({nameof(isBeta)}={isBeta}).");
            }

            return isBeta ? BetaIndex : NonBetaIndex;
        }

        public int GetIndex()
        {
            return GetIndex(SteamProcessInfo.IsRunningBetaClient);
        }

        [XmlIgnore]
        public IVTable VTable { get; set; }

        [XmlAttribute]
        public string Name { get; set; }

        /// <summary>
        /// The index of a vtable entry in the beta version of Steam.
        /// </summary>
        [XmlAttribute]
        public int BetaIndex { get; set; }

        /// <summary>
        /// The index of a vtable entry in the non-beta version of Steam.
        /// </summary>
        [XmlAttribute]
        public int NonBetaIndex { get; set; }

        /// <summary>
        /// Indicates this entry is marked with the 'UnmanagedFunctionPointer' attribute.
        /// </summary>
        [XmlAttribute]
        public bool IsUnmanagedFunctionPointer { get; set; }

        /// <summary>
        /// Defines this function's calling convention.
        /// </summary>
        [XmlAttribute]
        public CallingConvention CallingConvention { get; set; }

        /// <summary>
        /// Defines this function's character set.
        /// </summary>
        [XmlAttribute]
        public CharSet CharSet { get; set; }

        /// <summary>
        /// Defines whether the ReturnType is an IntPtr which must be marshaled to a UTF-8 string.
        /// </summary>
        [XmlAttribute]
        public bool IsReturnTypePtrToUtf8String { get; set; }

        private IEnumerable<CustomAttributeBuilder> _methodAttributeBuilders;

        /// <summary>
        /// A list of <see cref="CustomAttributeBuilder"/> objects defining one or more attributes assigned to this
        /// method. Ex: [UnmanagedFunctionPointer(CallingConvention.ThisCall, CharSet = CharSet.Ansi)]
        /// </summary>
        [XmlIgnore]
        public IEnumerable<CustomAttributeBuilder> MethodAttributeBuilders
        {
            get
            {
                if (_methodAttributeBuilders == null && IsUnmanagedFunctionPointer)
                {
                    _methodAttributeBuilders = new CustomAttributeBuilder[]
                    {
                        new CustomAttributeBuilder(
                            // ReSharper disable once AssignNullToNotNullAttribute
                            typeof(UnmanagedFunctionPointerAttribute).GetConstructor(
                                new[] { typeof(CallingConvention) }),
                            new object[] { CallingConvention },
                            new FieldInfo[]
                            {
                                typeof(UnmanagedFunctionPointerAttribute).GetField(
                                    nameof(System.Runtime.InteropServices.CharSet))
                            },
                            new object[] { CharSet })
                    };
                }

                return _methodAttributeBuilders;
            }
        }

        /// <summary>
        /// A list of this entry's parameters and its associated attributes.
        /// </summary>
        public List<VtEntryParam> Parameters { get; set; }

        private Type _returnType;

        /// <summary>
        /// Defines the Type of this method's return value.
        /// </summary>
        [XmlIgnore]
        public Type ReturnType
        {
            get => _returnType;
            set
            {
                _returnType = value;
                if (string.IsNullOrWhiteSpace(ReturnTypeName))
                    ReturnTypeName = value.FullName;
            }
        }

        [XmlAttribute(AttributeName = "ReturnType")]
        public string ReturnTypeName
        {
            get => ReturnType?.FullName;
            set => ReturnType ??= Type.GetType(value);
        }

        private Type _delegateType;

        [XmlIgnore]
        public Type DelegateType
        {
            get
            {
                if (_delegateType != null)
                    return _delegateType;

                var delegateFactory = new DelegateTypeFactory();
                _delegateType = delegateFactory.CreateDelegateType(this);

                return _delegateType;
            }
        }

        [XmlIgnore]
        public Delegate Delegate
        {
            get
            {
                if (VTable.DelegateCache.TryGetValue(DelegateType, out var @delegate))
                    return @delegate;

                var functionPtr = Marshal.ReadIntPtr(VTable.VTablePtr, GetIndex() * IntPtr.Size);
                @delegate = Marshal.GetDelegateForFunctionPointer(functionPtr, DelegateType);
                VTable.DelegateCache.Add(DelegateType, @delegate);
                return @delegate;
            }
        }

        public object Invoke(params object[] args)
        {
            if (CallingConvention == CallingConvention.ThisCall)
            {
                args = new object[] { VTable.InterfacePtr }.Concat(args).ToArray();
            }

            var result = Delegate.DynamicInvoke(args);
            if (IsReturnTypePtrToUtf8String && result != null)
                return Marshal.PtrToStringUTF8((IntPtr)result);

            return result;
        }
    }
}
