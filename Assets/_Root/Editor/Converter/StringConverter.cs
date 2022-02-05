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

            // string path :
            // AudioClip,
            // AnimationClip,
            // Animator,
            // Sprite,
            // Material,
            // Prefab,
            // ScriptableObject,

            // BsonArray : [1,2] or ["A","B","C"] or [1.5,3,2]
            // List<bool>
            // List<int>  // like vector2, vector3, vector4, quaternion
            // List<long>
            // List<float>
            // List<double>
            // List<string>


            converter.AddConverter(new StringConverterInternal());
            return converter;
        }

        public CultureInfo Culture { get; set; } = CultureInfo.CurrentCulture;

        public void AddConverter<T>(Predicate<string> match, Func<string, T> converter)
        {
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
        }

        public void AddConverter<T>(Regex match, Func<string, T> converter)
        {
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
        }

        public void AddConverter<T>(TypedConvertDelegate<T> constructor)
        {
            // create converter from typed TryParse(string, out T) function
            _converters.Add(FromTryPattern(constructor));
        }

        public void AddConverter<T>(TypeConverter<T> typeConverter)
        {
            // create converter from typed TryParse(string, out T) function
            _converters.Add(FromTryPattern(typeConverter));
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

        public void Clear() { _converters.Clear(); }
    }
}