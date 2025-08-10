using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
/*
	14.01.19
		-add make string
		-add shuffle
		-add ToEnumerable
	26.06.19
		-add GetRandomPos
		-add Split
		-add Join 
    21.12.2020
    - fix Shuffle
    21.09.2021
    - GetRepeatedByIndex 
    - GetClampedByIndex
    - GetPingPongByIndex
    23.09.2023
    - GetRow from 2D array
    - InsertColumn in 2D array
    24.11.2023
    - apply action
*/
namespace Mkey {
    public static class CollectionExtension
    {
        /// <summary>
        /// Sum list members
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static int Sum(this List<int> list)
        {
            if (list == null || list.Count == 0) return 0;
            int res = 0;
            for (int i = 0; i < list.Count; i++)
            {
                res += list[i];
            }
            return res;
        }

        /// <summary>
        /// Make string from list members
        /// </summary>
        /// <param name="list"></param>
        /// <param name="div"></param>
        /// <returns></returns>
        public static string MakeString<T>(this List<T> list, string div)
        {
            if (list == null || list.Count == 0) return string.Empty;
            string res = "";
            for (int i = 0; i < list.Count; i++)
            {
                res += list[i].ToString();
                if (i != list.Count - 1)
                    res += div;
            }
            return res;
        }

        /// <summary>
        /// Check list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty<T>(this List<T> list)
        {
            if (list == null || list.Count == 0) return true;
            return false;
        }

        /// <summary>
        /// Check dictionary
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="dict"></param>
        /// <returns></returns>
        public static bool IsNullOrEmpty<T1, T2>(this Dictionary<T1, T2> dict)
        {
            if (dict == null || dict.Count == 0) return true;
            return false;
        }

        /// <summary>
        /// Shuffle list members
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                int k = (UnityEngine.Random.Range(0, n) % n);
                n--;
                T val = list[k];
                list[k] = list[n];
                list[n] = val;
            }
        }

        /// <summary>
        /// Array to enumerable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <returns></returns>
        //https://stackoverflow.com/questions/1183083/is-it-possible-to-extend-arrays-in-c
        public static IEnumerable<T> ToEnumerable<T>(this Array target)
        {
            foreach (var item in target)
                yield return (T)item;
        }
    
	    public static T GetRandomPos<T>(this IList<T> list)
        {
            return list[UnityEngine.Random.Range(0, list.Count)];
        }
		
		/// <summary>
        /// Split list in two lists
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static List<List<T>> Split<T>(this IList<T> list, int index)
        {
            List<List<T>> res = new List<List<T>>();

            int n = list.Count;
            List<T> l1 = new List<T>();
            List<T> l2 = new List<T>();
            if (index >= 0 && index < n)
            {
                for (int  i = 0;  i < n;  i++)
                {
                    if (i <= index) l1.Add(list[i]);
                    else l2.Add(list[i]);
                }
                
            }
            else if (index >= n)
            {
                l1 = new List<T>(list);
            }
            else if (index < 0)
            {
                l2 = new List<T>(list);
            }
            res.Add(l1);
            res.Add(l2);
            return res;
        }
		
		public static List<T> Join <T>(this IList<T> list,  IList<T> addList)
        {
            List<T> res = new List<T>(list);
            res.AddRange(addList);
            return res;
        }

        /// <summary>
        /// Repeat list, not safe, check null and list.Count > 1
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static T GetRepeatedByIndex <T> (this IList<T> list, int index)
        {
            return list[(int) Mathf.Repeat(index, list.Count)];
                 
        }

        /// <summary>
        /// Clamp list, not safe, check null and list.Count > 1
        /// </summary>
        /// <param name="list"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static T GetClampedByIndex<T>(this IList<T> list, int index)
        {
            return list[(int)Mathf.Clamp(index, 0, list.Count - 1)];
        }

        /// <summary>
        /// PingPong list, not safe, check null and list.Count > 1
        /// </summary>
        /// <param name="list"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static T GetPingPongByIndex<T>(this IList<T> list, int index)
        {
            return list[(int)Mathf.PingPong(index, list.Count - 1)];
        }

        public static T[] GetRow<T>(this T[,] source, int row)
        {
            int cols = (int)source.GetLongLength(1);
            if (row < 0 || row >= (int)source.GetLongLength(0)) return null;
            T[] result = new T[cols];

            for (int i = 0; i < cols; i++)
            {
                result[i] = source[row, i];
            }
            return result;
        }

        public static T[] GetColumn<T>(this T[,] source, int column)
        {
            int rows = (int)source.GetLongLength(0);
            if (column < 0 || column >= (int)source.GetLongLength(1)) return null;
            T[] result = new T[rows];

            for (int i = 0; i < rows; i++)
            {
                result[i] = source[i, column];
            }
            return result;
        }

        public static void InsertColumn <T>(this T[,] target, T[] column,  int columnNumber)
        {
            int rowsT = (int)target.GetLongLength(0);
            int lengthS = column.Length;
            int length = Mathf.Min(rowsT, lengthS);
            if (columnNumber<0 || columnNumber >= (int)target.GetLongLength(1)) return;

            for (int _r = 0; _r < length; _r++)
            {
                target[_r, columnNumber] = column[_r];
            }
        }

        public static void InsertRow<T>(this T[,] target, T[] row, int rowNumber)
        {
            int colsT = (int)target.GetLongLength(1);
            int lengthS = row.Length;
            int length = Mathf.Min(colsT, lengthS);
            if (rowNumber < 0 || rowNumber >= (int)target.GetLongLength(0)) return;

            for (int _c = 0; _c < length; _c++)
            {
                target[rowNumber, _c] = row[_c];
            }
        }

        /// <summary>
        /// Rotate the 2D array clockwise 90deg
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T[,] CWRotateArray2D <T>(this T[,] source)
        {
            int rows = (int)source.GetLongLength(0);
            int cols = (int)source.GetLongLength(1);

            T[,] result = new T[cols, rows];
            int lastRow = rows - 1;
            for (int _r = 0; _r < rows; _r++)
            {
                T[] row = source.GetRow(_r);
                result.InsertColumn(row, lastRow - _r);
            }
            return result;
        }

        /// <summary>
        /// Rewerse all rows in the source array
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static void ReverseRows2D<T>(this T[,] source)
        {
            int rows = (int)source.GetLongLength(0);
            int cols = (int)source.GetLongLength(1);
            T temp;
            int lastCol = cols - 1;
            int halfCols = cols / 2;
            for (int _r = 0; _r < rows; _r++)
            {
                for (int _c = 0; _c < halfCols; _c++)
                {
                    temp = source[_r, _c];
                    source[_r, _c] = source[_r, lastCol - _c];
                    source[_r, lastCol - _c] = temp;
                }
            }
        }

        /// <summary>
        /// Make copy and rewerse all rows in the new array
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T[,] CopyAndReverseRows2D<T>(this T[,] source)
        {
            T[,] result = CopyArray(source);
            result.ReverseRows2D();
            return result;
        }

        /// <summary>
        /// Rewerse all columns in the source array
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static void ReverseColumns2D<T>(this T[,] source)
        {
            int rows = (int)source.GetLongLength(0);
            int cols = (int)source.GetLongLength(1);
            T temp;
            int lastRow = rows - 1;
            int halfRows = rows / 2;
            for (int _r = 0; _r < halfRows; _r++)
            {
                for (int _c = 0; _c < cols; _c++)
                {
                    temp = source[_r, _c];
                    source[_r, _c] = source[lastRow -_r, _c];
                    source[lastRow - _r, _c] = temp;
                }
            }
        }

        /// <summary>
        /// Make copy and rewerse all columns in the new array
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T[,] CopyAndReverseColumns2D<T>(this T[,] source)
        {
            T[,] result = CopyArray(source);
            result.ReverseColumns2D();
            return result;
        }

        public static T[,] CopyArray<T>(this T[,] source)
        {
            int rows = (int)source.GetLongLength(0);
            int cols = (int)source.GetLongLength(1);
            T[,] result = new T[rows, cols];

            for (int _r = 0; _r < rows; _r++)
            {
                for (int _c = 0; _c < cols; _c++)
                {
                    result[_r, _c] = source[_r,_c];
                }
            }
            return result;
        }

        public static void ApplyAction<T>(this IList<T> list, Action<T> action) where T : class
        {
            if (action == null) return;
            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i];
                if (item != null) action(item);
            }
        }
    }
}
