using System;
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

    #region type

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
}