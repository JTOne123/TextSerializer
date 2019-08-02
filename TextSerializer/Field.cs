﻿using System;
using System.Reflection;
using TheCodingMonkey.Serialization.Formatters;

namespace TheCodingMonkey.Serialization
{
    /// <summary>Class which contains the configuration details of a field/property that is created either using Attributes or Fluent Configuration</summary>
    public abstract class Field
    {
        /// <summary>Default Constructor</summary>
        public Field()
        {
            Position = -1;
            Size = -1;
            Optional = false;
        }

        private Type _formatterType;
        private ITextFormatter _formatter;

        /// <summary>Position (column) where this field is serialized in the CSV file.</summary>
        public int Position { get; set; }

        /// <summary>Maximum length in the CSV file that this field should take up.</summary>
        public int Size { get; set; }
        
        /// <summary>Determines whether this field is optional. Because of the nature of CSV and FixedWidth file formats,
        /// optional fields should only be a the end of the record.</summary>
        public bool Optional { get; set; }

        /// <summary>Optional class which is used to control custom serialization/deserialization of this field.
        /// This class must implement the <see cref="ITextFormatter">ITextFormatter</see> interface.</summary>
        public Type FormatterType
        {
            get { return _formatterType; }
            set
            {
                _formatterType = value;
                Formatter = (ITextFormatter)_formatterType.Assembly.CreateInstance(_formatterType.FullName);
            }
        }

        /// <summary>The reflected MemberInfo details of the field/property that this configures.</summary>
        public MemberInfo Member { get; set; }

        /// <summary>The Formatter to be used for Serialization/Deserialization if the default formatting is not used.</summary>
        public ITextFormatter Formatter {
            get { return _formatter; }
            internal set
            {
                _formatter = value;
                _formatterType = value.GetType();
            }
        }

        /// <summary>Defines the allowed characters that can be used for a field in the file.</summary>
        public object[] AllowedValues { get; set; }

        internal Type GetNativeType()
        {
            if (Member is PropertyInfo)
                return ((PropertyInfo)Member).PropertyType;
            else if (Member is FieldInfo)
                return ((FieldInfo)Member).FieldType;
            else
                throw new TextSerializationException("Invalid MemberInfo type encountered");
        }

        /// <summary>If there is a custom formatter, then use that to deserialize the string, otherwise use the default .NET behvavior.</summary>
        /// <param name="text">Deserialized text</param>
        /// <returns>Properly converted object using the formatter if there is one</returns>
        internal object FormatValue(string text)
        {
            return Formatter != null ? Formatter.Deserialize(text) : Convert.ChangeType(text, GetNativeType());
        }

        /// <summary>Get the string representation for the object.  If there is a custom formatter for this field, then use that, 
        /// otherwise use the default ToString behavior.</summary>
        /// <param name="objValue">Object to Serialize</param>
        /// <returns>String value using the Formatter or Default .NET behavior</returns>
        internal string FormatString(object objValue)
        {
            return Formatter != null ? Formatter.Serialize(objValue) : objValue.ToString();
        }

        internal virtual void InitializeFromAttributes()
        {
            Type memberType = GetNativeType();

            // Check for the AllowedValues Attribute and if it's there, store away the values into the other holder attribute
            AllowedValuesAttribute allowedAttr = Member.GetCustomAttribute<AllowedValuesAttribute>();
            if (allowedAttr != null)
                AllowedValues = allowedAttr.AllowedValues;

            if (memberType.IsEnum && FormatterType == null)
            {
                FormatEnumAttribute enumAttr = Member.GetCustomAttribute<FormatEnumAttribute>();
                if (enumAttr != null)
                    Formatter = new EnumFormatter(memberType, enumAttr.Options);
                else
                    Formatter = new EnumFormatter(memberType);
            }
        }
    }
}