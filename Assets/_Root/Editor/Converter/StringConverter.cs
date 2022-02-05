using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Snorlax.Database.Editor
{
    public class StringConverter
    {
        // delegate for TryParse(string, out T)
        public delegate bool TypedConvertDelegate<T>(string value, out T result, out Type type);

        // delegate for TryParse(string, out object)
        private delegate bool UntypedConvertDelegate(string value, out object result, out Type type);

        private readonly List<UntypedConvertDelegate> _converters = new List<UntypedConvertDelegate>();

        // default converter, lazyly initialized
        // ReSharper disable once InconsistentNaming
        private static readonly Lazy<StringConverter> _default = new Lazy<StringConverter>(CreateDefault, true);

        public static StringConverter Default => _default.Value;

        public CultureInfo Culture { get; set; } = CultureInfo.CurrentCulture;

        private UntypedConvertDelegate _stringConverterRef;
        private UntypedConvertDelegate _listStringConverterRef;

        private static StringConverter CreateDefault()
        {
            var converter = new StringConverter();
            converter.AddConverter(new BoolConverter());
            converter.AddConverter(new Int32Converter(converter.Culture));
            converter.AddConverter(new Int64Converter(converter.Culture));
            converter.AddConverter(new FloatConverter(converter.Culture));
            converter.AddConverter(new DoubleConverter(converter.Culture));
            converter.AddConverter(new DateTimeConverter(converter.Culture));
            converter.AddConverter(new DecimalConverter(converter.Culture));
            converter.AddConverter(new GuidConverter());
            converter.AddConverter(new ObjectIdConverter());
            converter.AddConverter(new ColorConverter());
            converter.AddConverter(new Vector2Converter(converter.Culture));
            converter.AddConverter(new Vector2IntConverter(converter.Culture));
            converter.AddConverter(new Vector3Converter(converter.Culture));
            converter.AddConverter(new Vector3IntConverter(converter.Culture));
            converter.AddConverter(new Vector4Converter(converter.Culture));

            // string path : store under addressable 
            // AudioClip,
            // AnimationClip,
            // Animator,
            // Sprite,
            // Material,
            // Prefab,
            // ScriptableObject,

            converter.AddConverter(new ListBoolConverter());
            converter.AddConverter(new ListInt32Converter(converter.Culture));
            converter.AddConverter(new ListInt64Converter(converter.Culture));
            converter.AddConverter(new ListFloatConverter(converter.Culture));
            converter.AddConverter(new ListDoubleConverter(converter.Culture));
            converter.AddConverter(new ListDecimalConverter(converter.Culture));
            converter.AddConverter(new ListDateTimeConverter(converter.Culture));

            return converter;
        }

        public void AddConverter<T>(Predicate<string> match, Func<string, T> converter)
        {
            RemoveStringConverter();

            // create converter from match predicate and convert function
            _converters.Add((string value, out object result, out Type type) =>
            {
                if (match(value))
                {
                    result = converter(value);
                    type = typeof(T);
                    return true;
                }

                result = null;
                type = null;
                return false;
            });

            AddStringConverter();
        }

        public void AddConverter<T>(Regex match, Func<string, T> converter)
        {
            RemoveStringConverter();

            // create converter from match regex and convert function
            _converters.Add((string value, out object result, out Type type) =>
            {
                if (match.IsMatch(value))
                {
                    result = converter(value);
                    type = typeof(T);
                    return true;
                }

                result = null;
                type = null;
                return false;
            });

            AddStringConverter();
        }

        public void AddConverter<T>(TypedConvertDelegate<T> constructor)
        {
            RemoveStringConverter();

            // create converter from typed TryParse(string, out T) function
            _converters.Add(FromTryPattern(constructor));

            AddStringConverter();
        }

        public void AddConverter<T>(TypeConverter<T> typeConverter)
        {
            RemoveStringConverter();

            // create converter from typed TryParse(string, out T) function
            _converters.Add(FromTryPattern(typeConverter));

            AddStringConverter();
        }

        public bool TryConvert(string value, out object result, out Type type)
        {
            if (this != Default)
            {
                // if this is not a default converter - first try convert with default
                if (Default.TryConvert(value, out result, out type)) return true;
            }

            // then use local converters. Any will return after the first match
            object tmp = null;
            Type t = null;
            bool anyMatch = _converters.Any(c => c(value, out tmp, out t));
            result = tmp;
            type = t;
            return anyMatch;
        }

        private static UntypedConvertDelegate FromTryPattern<T>(TypedConvertDelegate<T> inner)
        {
            return (string value, out object result, out Type type) =>
            {
                if (inner.Invoke(value, out T tmp, out Type t))
                {
                    result = tmp;
                    type = t;
                    return true;
                }

                result = null;
                type = null;
                return false;
            };
        }

        private static UntypedConvertDelegate FromTryPattern<T>(TypeConverter<T> typeConverter)
        {
            return (string value, out object result, out Type type) =>
            {
                if (typeConverter.TryParse(value, out T tmp, out Type t))
                {
                    result = tmp;
                    type = t;
                    return true;
                }

                result = null;
                type = null;
                return false;
            };
        }

        // remove string converter in last index to ensure stringConverter is the last converter in the array 
        private void RemoveStringConverter()
        {
            if (_stringConverterRef != null)
            {
                _converters.Remove(_stringConverterRef);
                _stringConverterRef = null;
            }

            if (_listStringConverterRef != null)
            {
                _converters.Remove(_listStringConverterRef);
                _listStringConverterRef = null;
            }
        }

        // add string converter in last index
        private void AddStringConverter()
        {
            if (_stringConverterRef != null || _listStringConverterRef != null) RemoveStringConverter();

            // add ListStringConverter before StringConverter
            _listStringConverterRef = FromTryPattern(new ListStringConverter());
            _converters.Add(_listStringConverterRef);

            _stringConverterRef = FromTryPattern(new StringConverterInternal());
            _converters.Add(_stringConverterRef);
        }

        public void Clear() { _converters.Clear(); }
    }
}