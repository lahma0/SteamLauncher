using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Xml.Serialization;

namespace SteamLauncher.DataStore.VTablesStore
{
    public class VtEntryParam
    {
        private VtEntryParam()
        {
            //Init();
        }

        /// <summary>
        /// Create a new <see cref="VtEntryParam"/> object used to dynamically define this parameter and its associated
        /// attributes for use with <see cref="VtEntry"/>.
        /// </summary>
        /// <param name="paramType">The Type of the parameter.</param>
        /// <param name="name">The optional parameter name. Not needed for code execution purposes. Mostly used just
        /// for serialization/identification purposes.</param>
        /// <param name="isMarshalAsUtf8String">Indicates whether this parameter should be marked with the attribute
        /// '[MarshalAs(UnmanagedType.LPUTF8Str)]'.</param>
        /// <param name="isByRef">Indicates that this parameter is passed to the method by reference. [Example:
        /// 'void Method(ref int intValue)']</param>
        /// <param name="isArrayType">Indicates that this parameter is a one-dimensional array of Type <paramref
        /// name="paramType"/>.</param>
        public VtEntryParam(Type paramType,
                            string name = null,
                            bool isMarshalAsUtf8String = false,
                            bool isByRef = false,
                            bool isArrayType = false)
        {
            ParamType = paramType;
            Name = name;
            IsMarshalAsUtf8String = isMarshalAsUtf8String;
            IsByRef = isByRef;
            IsArrayType = isArrayType;
            //Init();
        }

        //private void Init()
        //{
        //    if (!IsMarshalAsUtf8String)
        //        return;

        //    var marshalAsUnmanaged = typeof(MarshalAsAttribute).GetConstructor(new[] { typeof(UnmanagedType) });
        //    if (marshalAsUnmanaged == null)
        //        throw new NullReferenceException($"A null value was returned when retrieving the constructor " +
        //                                         $"for '{nameof(MarshalAsAttribute)}'.");

        //    AttributeBuilders = new CustomAttributeBuilder[]
        //    {
        //        new CustomAttributeBuilder(marshalAsUnmanaged, new object[] { UnmanagedType.LPUTF8Str })
        //    };
        //    ParamAttributes = ParameterAttributes.HasFieldMarshal;
        //}

        private Type _paramType;

        /// <summary>
        /// The parameter's data type.
        /// </summary>
        [XmlIgnore]
        public Type ParamType
        {
            get => _paramType;
            set
            {
                _paramType = value;
                if (string.IsNullOrWhiteSpace(ParamTypeName))
                    ParamTypeName = value.FullName;
            }
        }

        [XmlAttribute(AttributeName = "ParamType")]
        public string ParamTypeName
        {
            get => ParamType?.FullName;
            set
            {
                if (ParamType == null)
                    ParamType = Type.GetType(value);
            }
        }

        /// <summary>
        /// The parameter's name. Not technically required. Mostly used just for serialization/identification purposes.
        /// </summary>
        [XmlAttribute]
        public string Name { get; set; }

        /// <summary>
        /// Indicates whether this parameter should be marked with the attribute
        /// '[MarshalAs(UnmanagedType.LPUTF8Str)]'.
        /// </summary>
        [XmlAttribute]
        public bool IsMarshalAsUtf8String { get; set; }

        private IEnumerable<CustomAttributeBuilder> _attributeBuilders;

        /// <summary>
        /// A list of <see cref="CustomAttributeBuilder"/> objects defining one or more attributes assigned to this
        /// parameter. Ex: [MarshalAs(UnmanagedType.LPUTF8Str)]
        /// </summary>
        [XmlIgnore]
        public IEnumerable<CustomAttributeBuilder> AttributeBuilders
        {
            get
            {
                if (_attributeBuilders == null && IsMarshalAsUtf8String)
                {
                    var marshalAsUnmanaged = typeof(MarshalAsAttribute).GetConstructor(new[] { typeof(UnmanagedType) });
                    if (marshalAsUnmanaged == null)
                        throw new NullReferenceException($"A null value was returned when retrieving the constructor " +
                                                         $"for '{nameof(MarshalAsAttribute)}'.");

                    _attributeBuilders = new CustomAttributeBuilder[]
                    {
                        new CustomAttributeBuilder(marshalAsUnmanaged, new object[] { UnmanagedType.LPUTF8Str })
                    };
                }

                return _attributeBuilders;
            }
        }

        private ParameterAttributes? _paramAttributes;

        /// <summary>
        /// One or more (combine with logical OR) ParameterAttributes which help to define information about the
        /// parameter.
        /// </summary>
        [XmlIgnore]
        public ParameterAttributes ParamAttributes
        {
            get
            {
                if (_paramAttributes == null && IsMarshalAsUtf8String)
                {
                    _paramAttributes = ParameterAttributes.HasFieldMarshal;
                }

                return _paramAttributes ?? ParameterAttributes.None;
            }
        }

        /// <summary>
        /// Indicates if the parameter is passed by reference.
        /// </summary>
        [XmlAttribute]
        public bool IsByRef { get; set; }

        /// <summary>
        /// Indicates that a one-dimensional array of <see cref="ParamType"/> is expected.
        /// </summary>
        [XmlAttribute]
        public bool IsArrayType { get; set; }

        //public static VtEntryParam CreateMarshalUtf8StringParam(string paramName)
        //{
        //    return new VtEntryParam(typeof(string),
        //                            paramName,
        //                            true);
        //}
    }
}
