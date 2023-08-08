using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BroMakerLib.Editor
{
    public static class StringExtensions
    {
        public static string UpperWords(this string self)
        {
            char[] array = self.ToCharArray();
            if (array.Length >= 1 && char.IsLower(array[0]))
            {
                array[0] = char.ToUpper(array[0]);
            }
            for (int i = 1; i < array.Length; i++)
            {
                if ((array[i - 1] == ' ' || array[i - 1] == '_' || array[i - 1] == '/') && char.IsLower(array[i]))
                {
                    array[i] = char.ToUpper(array[i]);
                }
            }
            self = new string(array);
            return self;
        }
        public static string AddSpaces(this string self)
        {
            bool flag = false;
            List<char> list = new List<char>();
            foreach (char c in self)
            {
                if (char.IsUpper(c))
                {
                    if (flag)
                    {
                        list.Add(' ');
                    }
                    flag = true;
                }
                list.Add(c);
            }
            self = new string(list.ToArray());
            return self;
        }
    }
}
