using System;
using System.Collections.Generic;
using System.Globalization;
using LiteDB;
using Snorlax.Common;
using UnityEngine;

namespace Snorlax.Database.Editor
{
    public abstract class TypeConverter<T>
    {
        public abstract bool TryParse(string value, out T result, out Type type);
    }

    #region primitive

    public class BoolConverter : TypeConverter<bool>
    {
        public override bool TryParse(string value, out bool result, out Type type)
        {
            bool flag = bool.TryParse(value, out result);
            type = flag ? typeof(bool) : null;
            return flag;
        }
    }

    public class Int32Converter : TypeConverter<int>
    {
        private readonly CultureInfo _culture;

        public Int32Converter(CultureInfo culture) { _culture = culture; }

        public Int32Converter() { _culture = CultureInfo.CurrentCulture; }

        public override bool TryParse(string value, out int result, out Type type)
        {
            bool flag = int.TryParse(value, NumberStyles.Integer, _culture, out result);
            type = flag ? typeof(int) : null;
            return flag;
        }
    }

    public class Int64Converter : TypeConverter<long>
    {
        private readonly CultureInfo _culture;

        public Int64Converter(CultureInfo culture) { _culture = culture; }

        public Int64Converter() { _culture = CultureInfo.CurrentCulture; }

        public override bool TryParse(string value, out long result, out Type type)
        {
            bool flag = long.TryParse(value, NumberStyles.Integer, _culture, out result);
            if (!flag)
            {
                //{"$numberLong":"12200000"}
                if (value.Contains("\"$numberLong\":"))
                {
                    string realValue = value.Replace("{\"$numberLong\":\"", "");
                    realValue = realValue.Remove(realValue.Length - 2, 2);
                    flag = long.TryParse(realValue, NumberStyles.Integer, _culture, out result);
                }
            }

            type = flag ? typeof(long) : null;
            return flag;
        }
    }

    public class FloatConverter : TypeConverter<float>
    {
        private readonly CultureInfo _culture;

        public FloatConverter(CultureInfo culture) { _culture = culture; }

        public FloatConverter() { _culture = CultureInfo.CurrentCulture; }

        public override bool TryParse(string value, out float result, out Type type)
        {
            bool flag = float.TryParse(value, NumberStyles.Number, _culture, out result);
            type = flag ? typeof(float) : null;
            return flag;
        }
    }

    public class DoubleConverter : TypeConverter<double>
    {
        private readonly CultureInfo _culture;

        public DoubleConverter(CultureInfo culture) { _culture = culture; }

        public DoubleConverter() { _culture = CultureInfo.CurrentCulture; }

        public override bool TryParse(string value, out double result, out Type type)
        {
            bool flag = double.TryParse(value, NumberStyles.Number, _culture, out result);
            type = flag ? typeof(double) : null;
            return flag;
        }
    }


    // {"$numberDecimal":"122.9991"}
    public class DecimalConverter : TypeConverter<decimal>
    {
        private readonly CultureInfo _culture;

        public DecimalConverter(CultureInfo culture) { _culture = culture; }

        public DecimalConverter() { _culture = CultureInfo.CurrentCulture; }

        public override bool TryParse(string value, out decimal result, out Type type)
        {
            var flag = false;
            decimal tmp = 0;
            if (value.Contains("\"$numberDecimal\":"))
            {
                string realValue = value.Replace("{\"$numberDecimal\":\"", "");
                realValue = realValue.Remove(realValue.Length - 2, 2);
                flag = decimal.TryParse(realValue, NumberStyles.Integer, _culture, out tmp);
            }

            result = tmp;
            type = flag ? typeof(decimal) : null;
            return flag;
        }
    }

    public class StringConverterInternal : TypeConverter<string>
    {
        public override bool TryParse(string value, out string result, out Type type)
        {
            result = value;
            type = typeof(string);
            return true;
        }
    }

    #endregion

    #region .net

    // {"$date":"2022-02-04T16:52:56.7130000Z"}
    public class DateTimeConverter : TypeConverter<DateTime>
    {
        private readonly CultureInfo _culture;

        public DateTimeConverter(CultureInfo culture) { _culture = culture; }

        public DateTimeConverter() { _culture = CultureInfo.CurrentCulture; }

        public override bool TryParse(string value, out DateTime result, out Type type)
        {
            bool flag = DateTime.TryParse(value, _culture, DateTimeStyles.None, out result);
            if (!flag)
            {
                if (value.Contains("\"$date\":"))
                {
                    string realValue = value.Replace("{\"$date\":\"", "");
                    realValue = realValue.Remove(realValue.Length - 2, 2);
                    flag = DateTime.TryParse(realValue, _culture, DateTimeStyles.None, out result);
                }
            }

            type = flag ? typeof(DateTime) : null;
            return flag;
        }
    }

    // {"$guid":"ebe8f677-9f27-4303-8699-5081651beb11"}
    public class GuidConverter : TypeConverter<Guid>
    {
        public override bool TryParse(string value, out Guid result, out Type type)
        {
            var flag = false;
            if (value.Contains("\"$guid\":"))
            {
                string realValue = value.Replace("{\"$guid\":\"", "");
                realValue = realValue.Remove(realValue.Length - 2, 2);
                flag = Guid.TryParse(realValue, out result);
            }

            type = flag ? typeof(Guid) : null;
            return flag;
        }
    }

    #endregion

    #region unity

    // {"$oid":"507f1f55bcf96cd799438110"}
    public class ObjectIdConverter : TypeConverter<ObjectId>
    {
        public override bool TryParse(string value, out ObjectId result, out Type type)
        {
            var flag = false;
            var realValue = string.Empty;
            if (value.Contains("\"$oid\":"))
            {
                realValue = value.Replace("{\"$oid\":\"", "");
                realValue = realValue.Remove(realValue.Length - 2, 2);
                flag = true;
            }

            result = flag ? new ObjectId(realValue) : null;
            type = flag ? typeof(ObjectId) : null;
            return flag;
        }
    }

    // #FFFFFFFF
    public class ColorConverter : TypeConverter<Color>
    {
        public override bool TryParse(string value, out Color result, out Type type)
        {
            bool flag = value.TryParseHtmlString(out result);
            type = flag ? typeof(Color) : null;
            return flag;
        }
    }

    // {"$v2":"1.0:1.0"}
    public class Vector2Converter : TypeConverter<Vector2>
    {
        private readonly CultureInfo _culture;

        public Vector2Converter(CultureInfo culture) { _culture = culture; }

        public Vector2Converter() { _culture = CultureInfo.CurrentCulture; }

        public override bool TryParse(string value, out Vector2 result, out Type type)
        {
            var flag = false;
            var realValue = string.Empty;
            if (value.Contains("\"$v2\":"))
            {
                realValue = value.Replace("{\"$v2\":\"", "");
                realValue = realValue.Remove(realValue.Length - 2, 2);
                flag = true;
            }

            if (flag)
            {
                result = Vector2.zero;
                string[] vectors = realValue.Split(':');
                float.TryParse(vectors[0], NumberStyles.Number, _culture, out result.x);
                float.TryParse(vectors[1], NumberStyles.Number, _culture, out result.y);
                type = typeof(Vector2);
            }
            else
            {
                result = Vector2.zero;
                type = null;
            }

            return flag;
        }
    }

    // {"$v2Int":"1:1"}
    public class Vector2IntConverter : TypeConverter<Vector2Int>
    {
        private readonly CultureInfo _culture;

        public Vector2IntConverter(CultureInfo culture) { _culture = culture; }

        public Vector2IntConverter() { _culture = CultureInfo.CurrentCulture; }

        public override bool TryParse(string value, out Vector2Int result, out Type type)
        {
            var flag = false;
            var realValue = string.Empty;
            if (value.Contains("\"$v2Int\":"))
            {
                realValue = value.Replace("{\"$v2Int\":\"", "");
                realValue = realValue.Remove(realValue.Length - 2, 2);
                flag = true;
            }

            if (flag)
            {
                result = Vector2Int.zero;
                string[] vectors = realValue.Split(':');
                int.TryParse(vectors[0], NumberStyles.Integer, _culture, out int resultX);
                int.TryParse(vectors[1], NumberStyles.Integer, _culture, out int resultY);
                result.Set(resultX, resultY);
                type = typeof(Vector2Int);
            }
            else
            {
                result = Vector2Int.zero;
                type = null;
            }

            return flag;
        }
    }

    // {"$v3":"1.0:1.0:1.0"}
    public class Vector3Converter : TypeConverter<Vector3>
    {
        private readonly CultureInfo _culture;

        public Vector3Converter(CultureInfo culture) { _culture = culture; }

        public Vector3Converter() { _culture = CultureInfo.CurrentCulture; }

        public override bool TryParse(string value, out Vector3 result, out Type type)
        {
            var flag = false;
            var realValue = string.Empty;
            if (value.Contains("\"$v3\":"))
            {
                realValue = value.Replace("{\"$v3\":\"", "");
                realValue = realValue.Remove(realValue.Length - 2, 2);
                flag = true;
            }

            if (flag)
            {
                result = Vector3.zero;
                string[] vectors = realValue.Split(':');
                float.TryParse(vectors[0], NumberStyles.Number, _culture, out result.x);
                float.TryParse(vectors[1], NumberStyles.Number, _culture, out result.y);
                float.TryParse(vectors[2], NumberStyles.Number, _culture, out result.z);
                type = typeof(Vector3);
            }
            else
            {
                result = Vector3.zero;
                type = null;
            }

            return flag;
        }
    }

    // {"$v3Int":"1:1:1"}
    public class Vector3IntConverter : TypeConverter<Vector3Int>
    {
        private readonly CultureInfo _culture;

        public Vector3IntConverter(CultureInfo culture) { _culture = culture; }

        public Vector3IntConverter() { _culture = CultureInfo.CurrentCulture; }

        public override bool TryParse(string value, out Vector3Int result, out Type type)
        {
            var flag = false;
            var realValue = string.Empty;
            if (value.Contains("\"$v3Int\":"))
            {
                realValue = value.Replace("{\"$v3Int\":\"", "");
                realValue = realValue.Remove(realValue.Length - 2, 2);
                flag = true;
            }

            if (flag)
            {
                result = Vector3Int.zero;
                string[] vectors = realValue.Split(':');
                int.TryParse(vectors[0], NumberStyles.Integer, _culture, out int resultX);
                int.TryParse(vectors[1], NumberStyles.Integer, _culture, out int resultY);
                int.TryParse(vectors[2], NumberStyles.Integer, _culture, out int resultZ);
                result.Set(resultX, resultY, resultZ);
                type = typeof(Vector3Int);
            }
            else
            {
                result = Vector3Int.zero;
                type = null;
            }

            return flag;
        }
    }

    // {"$v4":"1.0:1.0:1.0:1.0"}
    public class Vector4Converter : TypeConverter<Vector4>
    {
        private readonly CultureInfo _culture;

        public Vector4Converter(CultureInfo culture) { _culture = culture; }

        public Vector4Converter() { _culture = CultureInfo.CurrentCulture; }

        public override bool TryParse(string value, out Vector4 result, out Type type)
        {
            var flag = false;
            var realValue = string.Empty;
            if (value.Contains("\"$v4\":"))
            {
                realValue = value.Replace("{\"$v4\":\"", "");
                realValue = realValue.Remove(realValue.Length - 2, 2);
                flag = true;
            }

            if (flag)
            {
                result = Vector4.zero;
                string[] vectors = realValue.Split(':');
                float.TryParse(vectors[0], NumberStyles.Number, _culture, out result.x);
                float.TryParse(vectors[1], NumberStyles.Number, _culture, out result.y);
                float.TryParse(vectors[2], NumberStyles.Number, _culture, out result.z);
                float.TryParse(vectors[3], NumberStyles.Number, _culture, out result.w);
                type = typeof(Vector4);
            }
            else
            {
                result = Vector4.zero;
                type = null;
            }

            return flag;
        }
    }

    // {"$audio":"Attack"}
    public class AudioClipConverter : TypeConverter<AudioClip>
    {
        public override bool TryParse(string value, out AudioClip result, out Type type)
        {
            throw null;
        }
    }

    #endregion

    #region collection BsonArray : [1,2] or ["A","B","C"] or [1.5,3,2]

    public class ListBoolConverter : TypeConverter<List<bool>>
    {
        // [true,false]
        public override bool TryParse(string value, out List<bool> result, out Type type)
        {
            string temp = value;
            temp = temp.Remove(0, 1);
            temp = temp.Remove(temp.Length - 1, 1);
            string[] arrs = temp.Split(',');

            result = new List<bool>();
            for (var i = 0; i < arrs.Length; i++)
            {
                bool flag = bool.TryParse(arrs[i], out bool b);
                if (!flag)
                {
                    result.Clear();
                    type = null;
                    return false;
                }

                result.Add(b);
            }

            type = typeof(List<bool>);
            return true;
        }
    }

    public class ListInt32Converter : TypeConverter<List<int>>
    {
        private readonly Int32Converter _int32Converter;

        public ListInt32Converter(CultureInfo culture) { _int32Converter = new Int32Converter(culture); }

        public ListInt32Converter()
        {
            var culture = CultureInfo.CurrentCulture;
            _int32Converter = new Int32Converter(culture);
        }

        public override bool TryParse(string value, out List<int> result, out Type type)
        {
            string temp = value;
            temp = temp.Remove(0, 1);
            temp = temp.Remove(temp.Length - 1, 1);
            string[] arrs = temp.Split(',');

            result = new List<int>();
            for (var i = 0; i < arrs.Length; i++)
            {
                bool flag = _int32Converter.TryParse(arrs[i], out int b, out _);
                if (!flag)
                {
                    result.Clear();
                    type = null;
                    return false;
                }

                result.Add(b);
            }

            type = typeof(List<int>);
            return true;
        }
    }

    public class ListInt64Converter : TypeConverter<List<long>>
    {
        private readonly Int64Converter _int64Converter;

        public ListInt64Converter(CultureInfo culture) { _int64Converter = new Int64Converter(culture); }

        public ListInt64Converter()
        {
            var culture = CultureInfo.CurrentCulture;
            _int64Converter = new Int64Converter(culture);
        }

        public override bool TryParse(string value, out List<long> result, out Type type)
        {
            string temp = value;
            temp = temp.Remove(0, 1);
            temp = temp.Remove(temp.Length - 1, 1);
            string[] arrs = temp.Split(',');

            result = new List<long>();
            for (var i = 0; i < arrs.Length; i++)
            {
                bool flag = _int64Converter.TryParse(arrs[i], out long b, out _);
                if (!flag)
                {
                    result.Clear();
                    type = null;
                    return false;
                }

                result.Add(b);
            }

            type = typeof(List<long>);
            return true;
        }
    }

    public class ListFloatConverter : TypeConverter<List<float>>
    {
        private readonly FloatConverter _floatConverter;

        public ListFloatConverter(CultureInfo culture) { _floatConverter = new FloatConverter(culture); }

        public ListFloatConverter()
        {
            var culture = CultureInfo.CurrentCulture;
            _floatConverter = new FloatConverter(culture);
        }

        public override bool TryParse(string value, out List<float> result, out Type type)
        {
            string temp = value;
            temp = temp.Remove(0, 1);
            temp = temp.Remove(temp.Length - 1, 1);
            string[] arrs = temp.Split(',');

            result = new List<float>();
            for (var i = 0; i < arrs.Length; i++)
            {
                bool flag = _floatConverter.TryParse(arrs[i], out float b, out _);
                if (!flag)
                {
                    result.Clear();
                    type = null;
                    return false;
                }

                result.Add(b);
            }

            type = typeof(List<float>);
            return true;
        }
    }

    public class ListDoubleConverter : TypeConverter<List<double>>
    {
        private readonly DoubleConverter _doubleConverter;

        public ListDoubleConverter(CultureInfo culture) { _doubleConverter = new DoubleConverter(culture); }

        public ListDoubleConverter()
        {
            var culture = CultureInfo.CurrentCulture;
            _doubleConverter = new DoubleConverter(culture);
        }

        public override bool TryParse(string value, out List<double> result, out Type type)
        {
            string temp = value;
            temp = temp.Remove(0, 1);
            temp = temp.Remove(temp.Length - 1, 1);
            string[] arrs = temp.Split(',');

            result = new List<double>();
            for (var i = 0; i < arrs.Length; i++)
            {
                bool flag = _doubleConverter.TryParse(arrs[i], out double b, out _);
                if (!flag)
                {
                    result.Clear();
                    type = null;
                    return false;
                }

                result.Add(b);
            }

            type = typeof(List<double>);
            return true;
        }
    }

    public class ListStringConverter : TypeConverter<List<string>>
    {
        public override bool TryParse(string value, out List<string> result, out Type type)
        {
            string temp = value;
            temp = temp.Remove(0, 1);
            temp = temp.Remove(temp.Length - 1, 1);
            string[] arrs = temp.Split(',');

            result = new List<string>(arrs);
            type = typeof(List<string>);
            return true;
        }
    }

    // [{"$date":"2022-02-05T11:28:51.1800000Z"},{"$date":"1996-04-25T00:30:00.0000000Z"}]
    public class ListDateTimeConverter : TypeConverter<List<DateTime>>
    {
        private readonly DateTimeConverter _dateTimeConverter;

        public ListDateTimeConverter(CultureInfo culture) { _dateTimeConverter = new DateTimeConverter(culture); }

        public ListDateTimeConverter()
        {
            var culture = CultureInfo.CurrentCulture;
            _dateTimeConverter = new DateTimeConverter(culture);
        }

        public override bool TryParse(string value, out List<DateTime> result, out Type type)
        {
            string temp = value;
            temp = temp.Remove(0, 1);
            temp = temp.Remove(temp.Length - 1, 1);
            string[] arrs = temp.Split(',');

            result = new List<DateTime>();
            for (var i = 0; i < arrs.Length; i++)
            {
                bool flag = _dateTimeConverter.TryParse(arrs[i], out var date, out _);
                if (!flag)
                {
                    result.Clear();
                    type = null;
                    return false;
                }

                result.Add(date);
            }

            type = typeof(List<DateTime>);
            return true;
        }
    }

    public class ListDecimalConverter : TypeConverter<List<decimal>>
    {
        private readonly DecimalConverter _decimalConverter;

        public ListDecimalConverter(CultureInfo culture) { _decimalConverter = new DecimalConverter(culture); }

        public ListDecimalConverter()
        {
            var culture = CultureInfo.CurrentCulture;
            _decimalConverter = new DecimalConverter(culture);
        }

        public override bool TryParse(string value, out List<decimal> result, out Type type)
        {
            string temp = value;
            temp = temp.Remove(0, 1);
            temp = temp.Remove(temp.Length - 1, 1);
            string[] arrs = temp.Split(',');

            result = new List<decimal>();
            for (var i = 0; i < arrs.Length; i++)
            {
                bool flag = _decimalConverter.TryParse(arrs[i], out decimal item, out _);
                if (!flag)
                {
                    result.Clear();
                    type = null;
                    return false;
                }

                result.Add(item);
            }

            type = typeof(List<decimal>);
            return true;
        }
    }

    #endregion
}