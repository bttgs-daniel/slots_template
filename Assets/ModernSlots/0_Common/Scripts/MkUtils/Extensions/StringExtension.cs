using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using System;

/*
    12.09.2022 uppercase first letter
	20.05.2020 - first
*/


namespace Mkey
{
    public static class StringExtension //http://stackoverflow.com/questions/1508203/best-way-to-split-string-into-lines
    {
        //return line from string
        public static IEnumerable<string> GetLines(this string str, bool removeEmptyLines = false)
        {
            return str.Split(new[] { "\r\n", "\r", "\n" },
                removeEmptyLines ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None);
        }

        public static string[] GetLines(string text)
        {

            List<string> lines = new List<string>();
            using (MemoryStream ms = new MemoryStream())
            {
                StreamWriter sw = new StreamWriter(ms);
                sw.Write(text);
                sw.Flush();

                ms.Position = 0;

                string line;

                using (StreamReader sr = new StreamReader(ms))
                {
                    while ((line = sr.ReadLine()) != null)
                    {
                        lines.Add(line);
                    }
                }
                sw.Close();
            }
            return lines.ToArray();
        }

        /// <summary>
        /// Remove any char from string
        /// </summary>
        /// <param name="input"></param>
        /// <param name="rChar"></param>
        /// <returns></returns>
        public static string RemoveChar(this string input, char rChar)
        {
            List<char> lC = new List<char>(input.Length);
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] != rChar)
                    lC.Add(input[i]);
            }

            return new string(lC.ToArray());
        }

       public static string UppercaseFirst(this string input)
        {
            // Check for empty string.
            if (string.IsNullOrEmpty(input))
            {
                return string.Empty;
            }
            // Return char and concat substring.
            return input.Length > 1 ? char.ToUpper(input[0]) + input.Substring(1) : char.ToUpper(input[0]).ToString();
        }
    }
}