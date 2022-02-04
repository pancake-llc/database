using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using LiteDB;

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
            var d = new StringConverter();
            // add reasonable default converters for common .NET types. Don't forget to take culture into account, that's
            // important when parsing numbers\dates.
            d.AddConverter((string value, out bool result, out Type type) =>
            {
                bool flag = bool.TryParse(value, out result);
                type = flag ? typeof(bool) : null;
                return flag;
            });
            // Int16, UInt16, Byte, SByte => Int32
            d.AddConverter((string value, out int result, out Type type) =>
            {
                bool flag = int.TryParse(value, NumberStyles.Integer, d.Culture, out result);
                type = flag ? typeof(int) : null;
                return flag;
            });
            // UInt32 , UInt64 => Int64
            d.AddConverter((string value, out long result, out Type type) =>
            {
                bool flag = long.TryParse(value, NumberStyles.Integer, d.Culture, out result);
                type = flag ? typeof(long) : null;
                return flag;
            });
            // Single => double
            d.AddConverter((string value, out float result, out Type type) =>
            {
                bool flag = float.TryParse(value, NumberStyles.Number, d.Culture, out result);
                type = flag ? typeof(float) : null;
                return flag;
            });
            // Single => double
            d.AddConverter((string value, out double result, out Type type) =>
            {
                bool flag = double.TryParse(value, NumberStyles.Number, d.Culture, out result);
                type = flag ? typeof(double) : null;
                return flag;
            });
            d.AddConverter((string value, out DateTime result, out Type type) =>
            {
                bool flag = DateTime.TryParse(value, d.Culture, DateTimeStyles.None, out result);
                if (!flag)
                {
                    //{"$date":"2022-02-04T16:52:56.7130000Z"}
                    if (value.Contains("\"$date\":"))
                    {
                        string realValue = value.Replace("{\"$date\":\"", "");
                        realValue = realValue.Remove(realValue.Length - 2, 2);
                        flag = DateTime.TryParse(realValue, d.Culture, DateTimeStyles.None, out result);
                    }
                }

                type = flag ? typeof(DateTime) : null;
                return flag;
            });

            d.AddConverter((string value, out Guid result, out Type type) =>
            {
                //{"$guid":"ebe8f677-9f27-4303-8699-5081651beb11"}
                var flag = false;
                if (value.Contains("\"$guid\":"))
                {
                    string realValue = value.Replace("{\"$guid\":\"", "");
                    realValue = realValue.Remove(realValue.Length - 2, 2);
                    flag = Guid.TryParse(realValue, out result);
                }

                type = flag ? typeof(Guid) : null;
                return flag;
            });

            // // bianry
            // d.AddConverter((string value, out string result, out Type type) =>
            // {
            //     //{"$binary":"VHlwZSgaFc3sdcGFzUpcmUuLi4="}
            //     var flag = false;
            //     var realValue = string.Empty;
            //     if (value.Contains("\"$binary\":"))
            //     {
            //         realValue = value.Replace("{\"$binary\":\"", "");
            //         realValue = realValue.Remove(realValue.Length - 2, 2);
            //         flag = true;
            //     }
            //
            //     result = flag ? realValue : null;
            //     type = flag ? typeof(string) : null;
            //     return flag;
            // });

            // Int64
            d.AddConverter((string value, out long result, out Type type) =>
            {
                //{"$numberLong":"12200000"}
                var flag = false;
                long tmp = 0;
                if (value.Contains("\"$numberLong\":"))
                {
                    string realValue = value.Replace("{\"$numberLong\":\"", "");
                    realValue = realValue.Remove(realValue.Length - 2, 2);
                    flag = long.TryParse(realValue, NumberStyles.Integer, d.Culture, out tmp);
                }

                result = tmp;
                type = flag ? typeof(long) : null;
                return flag;
            });
            
            d.AddConverter((string value, out decimal result, out Type type) =>
            {
                //{"$numberDecimal":"122.9991"}
                var flag = false;
                decimal tmp = 0;
                if (value.Contains("\"$numberDecimal\":"))
                {
                    string realValue = value.Replace("{\"$numberDecimal\":\"", "");
                    realValue = realValue.Remove(realValue.Length - 2, 2);
                    flag = decimal.TryParse(realValue, NumberStyles.Integer, d.Culture, out tmp);
                }

                result = tmp;
                type = flag ? typeof(decimal) : null;
                return flag;
            });

            d.AddConverter((string value, out ObjectId result, out Type type) =>
            {
                //{"$oid":"507f1f55bcf96cd799438110"}
                var flag = false;
                var realValue = string.Empty;
                if (value.Contains("\"$oid\":"))
                {
                    realValue = value.Replace("{\"$oid\":\"", "");
                    realValue = realValue.Remove(realValue.Length - 2, 2);
                    flag = true;
                }

                result = flag ? new ObjectId(realValue) : null;
                type = flag ? typeof(decimal) : null;
                return flag;
            });

            d.AddConverter(s => true, s => s);
            return d;
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
                else
                {
                    result = null;
                    type = null;
                    return false;
                }
            };
        }

        public void Clear() { _converters.Clear(); }
    }
}