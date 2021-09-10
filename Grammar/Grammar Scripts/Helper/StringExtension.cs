using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Grammar.Helper
{
    public static class StringExtension
    {
        ///<param name="str">Example str: 10.5,6,2.1 </param> 
        public static Vector3 StringToVector3(this string str)
        {
            Vector3 vec = Vector3.zero;

            string[] separatedStr = str.Split(',');
            vec.x = float.Parse(separatedStr[0]);
            vec.y = float.Parse(separatedStr[1]);
            vec.z = float.Parse(separatedStr[2]);

            return vec;
        }

        ///<param name="str">Example str: 10.5,2.1 </param> 
        public static Vector2 StringToVector2(this string str)
        {
            Vector2 vec = Vector2.zero;
            string[] separatedStr = str.Split(',');
            vec.x = float.Parse(separatedStr[0]);
            vec.y = float.Parse(separatedStr[1]);

            return vec;
        }

        public static string[] SplitArgs(this string str)
        {
            return str.Split(',');
        }

        ///<param name="str">Example str: R[10,20]</param> 
        public static float StringToRandomFloat(this string str)
        {
            int startIndex = str.IndexOf('[', 0);
            int endIndex = str.LastIndexOf(']');

            Vector2 boundaries = str.Substring(startIndex + 1, endIndex - startIndex - 1).StringToVector2();

            return Random.Range(boundaries.x, boundaries.y);
        }

        public static float StringToFloat(this string str)
        {
            float num = float.Parse(str);
            return num;
        }

        public static int StringToInt(this string str)
        {
            int num = int.Parse(str);
            return num;
        }

        public static bool StringToBool(this string str)
        {
            switch (str)
            {
                case "f":
                case "F":
                case "0":
                    return false;
                case "t":
                case "T":
                case "1":
                    return true;
                default:
                    return false;
            }
        }

        public static SurfaceDirection StringToSurfaceDirection(this string str)
        {
            switch (str.ToLower())
            {
                case "x+":
                    return SurfaceDirection.XPos;
                case "x-":
                    return SurfaceDirection.XNeg;
                case "z+":
                    return SurfaceDirection.ZPos;
                case "z-":
                    return SurfaceDirection.ZNeg;
                case "y+":
                    return SurfaceDirection.YPos;
                case "y-":
                    return SurfaceDirection.YNeg;
                default:
                    Debug.LogError($"Surface Direction {str} is invalid!");
                    return SurfaceDirection.Null;
            }
        }

        public static string RemoveWhitespace(this string str)
        {
            return string.Join("", str.Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));
        }

        public static string ReplaceFirst(this string text, string search, string replace)
        {
            int pos = text.IndexOf(search, StringComparison.Ordinal);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }
    }
}

