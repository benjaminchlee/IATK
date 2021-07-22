using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NewIATK
{
    public enum IATKDataType
    {
        Undefined,
        Float,
        Int,
        Bool,
        String,
        Date,
        Time,
        Graph
    }

    public static class DataTypeExtension
    {
        public static IATKDataType inferFromString(string data)
        {
            if (isBool(data))
            {
                return IATKDataType.Bool;
            }
            else if (isDate(data))
            {
                return IATKDataType.Date;
            }
            else if (isTime(data))
            {
                return IATKDataType.Time;
            }
            else if (isInt(data))
            {
                return IATKDataType.Int;
            }
            else if (isFloat(data))
            {
                return IATKDataType.Float;
            }
            else if(isGraph(data))
            {
                return IATKDataType.Graph;
            }
            else if (!String.IsNullOrEmpty(data))
            {
                return IATKDataType.String;
            }
            else
            {
                return IATKDataType.Undefined;
            }
        }

        private static bool isGraph(string data)
        {
            return data.Contains("|");
        }

        private static bool isBool(string value)
        {
            bool res = false;
            return bool.TryParse(value, out res);
        }

        private static bool isInt(string value)
        {
            int res = 0;
            return int.TryParse(value, out res);
        }

        private static bool isFloat(string value)
        {
            float res = 0.0f;
            return float.TryParse(value, out res);
        }

        private static bool isDate(string value)
        {
            return value.Contains(@"\");
        }

        private static bool isTime(string value)
        {
            return false;// value.Contains(":");
        }
    }

}   // Namespace