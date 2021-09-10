using System;
using System.Globalization;
using Grammar.Helper;
using UnityEngine;

namespace Grammar.Core
{
    public enum WordType
    {
        Null,
        Rule,
        Object,
        Method
    };

    public class Word
    {
        public string Value { get; }
        public Word Parent { get; }
        public WordType Type { get; }
        public string Name { get; }
        public string Args { get; }

        public ObjWord objWord;
        public int childSentenceIndex = int.MinValue;

        public Word(string value, Word parent)
        {
            Value = value;
            Parent = parent;

            string full = Value.Trim('<', '>');
            string type = full.Substring(0, full.IndexOf(' '));
            string nameAndArgs = full.Substring(full.IndexOf(' ') + 1).RemoveWhitespace();
            
            Type = TypeReader(type);
            Name = NameReader(nameAndArgs);
            Args = ArgsReader(nameAndArgs);
        }
        
        private static WordType TypeReader(string str)
        {
            switch (str.ToLower())
            {
                case "rule":
                case "r":
                    return WordType.Rule;
                case "object":
                case "obj":
                case "o":
                    return WordType.Object;
                case "method":
                case "m":
                    return WordType.Method;
                default:
                    Debug.LogError($"Type '{str}' is invalid!");
                    return WordType.Null;
            }
        }

        // exp: SetPos(1,1,1) => return SetPos
        private static string NameReader(string str)
        {
            int startIndex = str.IndexOf('(', 0);
            int endIndex = str.LastIndexOf(')');

            if (startIndex != -1 && endIndex != -1)
                return str.Remove(startIndex, endIndex + 1 - startIndex);

            return str;
        }

        // exp: SetPos(1,1,1) => return 1,1,1
        private static string ArgsReader(string str)
        {
            int startIndex = str.IndexOf('(', 0);
            int endIndex = str.LastIndexOf(')');

            if (startIndex == -1 || endIndex == -1) return "";

            string args = str.Substring(startIndex + 1, endIndex - startIndex - 1);

            startIndex = -1;
            endIndex = -1;
            
            while (args.Contains("R["))
            {
                startIndex = args.IndexOf("R[", 0, StringComparison.Ordinal);
                endIndex = args.IndexOf(']', 0);
                string random = args.Substring(startIndex, endIndex - startIndex + 1);
                args = args.ReplaceFirst(random, random.StringToRandomFloat().ToString(CultureInfo.InvariantCulture));
            }
            return args;
        }
    }
}
