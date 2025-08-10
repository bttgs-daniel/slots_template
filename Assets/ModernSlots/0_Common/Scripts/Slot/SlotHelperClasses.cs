using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Mkey
{
        [Serializable]
        public class ReelData
        {
            public List<int> symbOrder;
            public ReelData(List<int> symbOrder)
            {
                this.symbOrder = symbOrder;
            }
            public int Length
            {
                get { return (symbOrder == null) ? 0 : symbOrder.Count; }
            }
            public int GetSymbolAtPos(int position)
            {
                return (symbOrder == null || position >= symbOrder.Count) ? 0 : symbOrder.Count;
            }
        }

        [Serializable]
        public class PayLine
        {
            private const int maxLength = 5;
            public int[] line;
            public int pay;
            public int freeSpins;
            public bool showEvent = false;
            public UnityEvent LineEvent;
            [Tooltip("Payouts multiplier, default value = 1")]
            public int payMult = 1; // payout multiplier
            [Tooltip("Free Spins multiplier, default value = 1")]
            public int freeSpinsMult = 1; // payout multiplier

            bool useWildInFirstPosition = false;

            public PayLine()
            {
                line = new int[maxLength];
                for (int i = 0; i < line.Length; i++)
                {
                    line[i] = -1;
                }
            }

            public PayLine(PayLine pLine)
            {
                if (pLine.line != null)
                {
                    line = pLine.line;
                    RebuildLine();
                    pay = pLine.pay;
                    freeSpins = pLine.freeSpins;
                    LineEvent = pLine.LineEvent;
                    payMult = pLine.payMult;
                }
                else
                {
                    RebuildLine();
                }
            }

            public PayLine(int[] newLine, int pay, int freeSpins)
            {
                if (newLine != null)
                {
                    this.line = newLine;
                    this.pay = pay;
                    this.freeSpins = freeSpins;
                }
                RebuildLine();
            }

            public string ToString(SlotIcon[] sprites, int length)
            {
                string res = "";
                if (line == null) return res;
                for (int i = 0; i < line.Length; i++)
                {
                    if (i < length)
                    {
                        if (line[i] >= 0)
                            res += sprites[line[i]].iconSprite.name;
                        else
                        {
                            res += "any";
                        }
                        if (i < line.Length - 1) res += ";";
                    }
                }
                return res;
            }

            public string[] Names(SlotIcon[] sprites, int length)
            {
                if (line == null) return null;
                List<string> res = new List<string>();
                for (int i = 0; i < line.Length; i++)
                {
                    if (i < length)
                    {
                        if (line[i] >= 0)
                            res.Add((sprites[line[i]] != null && sprites[line[i]].iconSprite != null) ? sprites[line[i]].iconSprite.name : "failed");
                        else
                        {
                            res.Add("any");
                        }
                    }
                }
                return res.ToArray();
            }

            public double GetPayOutProb(SlotController sC)
            {
                return GetProbability(sC) * 100.0 * (double)pay;
            }

            /// <summary>
            /// return line win probability (1/100 %)
            /// </summary>
            /// <param name="sC"></param>
            /// <returns></returns>
            public double GetProbability(SlotController sC)
            {
                double res = 0;
                if (!sC) return res;
                if (line == null || sC.slotGroupsBeh == null || sC.slotGroupsBeh.Length > line.Length) return res;
                double[] rP = sC.slotGroupsBeh[0].GetReelSymbHitPropabilities(sC.slotIcons);

                //avoid "any" symbol error in first position
                res = (line[0] >= 0) ? rP[line[0]] : 1; //  res = rP[line[0]];
                int mainID = line[0];
                bool useWild = sC.useWild;
                int wildID = sC.wild_id;
                bool lineIsBroken = false;      // for broken slotline last symbols not affects
                useWild = useWild && sC.slotIcons[mainID].useWildSubstitute;

                for (int i = 1; i < sC.slotGroupsBeh.Length; i++)
                {
                    rP = sC.slotGroupsBeh[i].GetReelSymbHitPropabilities(sC.slotIcons);
                    if (line[i] >= 0)                               // not any,  any.ID = -1
                    {
                        res *= rP[line[i]];
                    }
                    else                                            // any.ID = -1, ANY != mainID, if (usewild) ANY != wildID
                    {
                        if (!lineIsBroken)
                        {
                            if (useWild && mainID != wildID) res *= (1.0 - rP[mainID] - rP[wildID]);
                            else if (!useWild) res *= (1.0 - rP[mainID]);
                        }
                        else
                        {
                            res *= 1.0;
                        }
                        lineIsBroken = true;
                    }
                }
                return res;
            }

            /// <summary>
            /// Create and return additional lines for this line with wild symbol,  only if symbol can be substitute with wild
            /// </summary>
            /// <returns></returns>
            public List<PayLine> GetWildLines(SlotController sC)
            {
                int workLength = sC.slotGroupsBeh.Length;
                List<PayLine> res = new List<PayLine>();
                if (!sC) return res; // return empty list
                if (!sC.useWild) return res; // return empty list

                int wild_id = sC.wild_id;
                useWildInFirstPosition = sC.useWildInFirstPosition;
                List<int> wPoss = GetPositionsForWild(wild_id, sC);
                int maxWildsCount = (useWildInFirstPosition) ? wPoss.Count - 1 : wPoss.Count;
                int minWildsCount = 1;
                ComboCounter cC = new ComboCounter(wPoss);
                while (cC.NextCombo())
                {
                    List<int> combo = cC.combo; // 
                    int comboSum = combo.Sum(); // count of wilds in combo

                    if (comboSum >= minWildsCount && comboSum <= maxWildsCount)
                    {
                        PayLine p = new PayLine(this);
                        for (int i = 0; i < wPoss.Count; i++)
                        {
                            int pos = wPoss[i];
                            if (combo[i] == 1)
                            {
                                p.line[pos] = wild_id;
                            }
                        }
                        if (!p.IsEqual(this, workLength) && !ContainEqualLine(res, p, workLength)) res.Add(p);
                    }
                }

                return res;
            }

            private bool IsEqual(PayLine pLine, int workLength)
            {
                if (pLine == null) return false;
                if (pLine.line == null) return false;
                if (line.Length != pLine.line.Length) return false;
                for (int i = 0; i < line.Length; i++)
                {
                    if (line[i] != pLine.line[i]) return false;
                }
                return true;
            }

            private bool ContainEqualLine(List<PayLine> pList, PayLine pLine, int workLength)
            {
                if (pList == null) return false;
                if (pLine == null) return false;
                if (pLine.line == null) return false;

                foreach (var item in pList)
                {
                    if (item.IsEqual(pLine, workLength)) return true;
                }
                return false;
            }

            /// <summary>
            /// return list position on line for wild symbols (0 - line.length -1)  
            /// </summary>
            /// <param name="wild_id"></param>
            /// <param name="sC"></param>
            /// <returns></returns>
            private List<int> GetPositionsForWild(int wild_id, SlotController sC)
            {
                List<int> wPoss = new List<int>();
                int counter = 0;
                int length = sC.slotGroupsBeh.Length;

                for (int i = 0; i < line.Length; i++)
                {
                    if (i < length)
                    {
                        if (line[i] != -1 && line[i] != wild_id)
                        {
                            if (!useWildInFirstPosition && counter == 0) // don't use first
                            {
                                counter++;
                            }
                            else
                            {
                                if (sC.slotIcons[line[i]].useWildSubstitute) wPoss.Add(i);
                                counter++;
                            }
                        }
                    }
                }
                return wPoss;
            }

            public void RebuildLine()
            {
                // if (line.Length == maxLength) return;
                int[] lineT = new int[maxLength];
                for (int i = 0; i < maxLength; i++)
                {
                    if (line != null && i < line.Length) lineT[i] = line[i];
                    else lineT[i] = -1;
                }
                line = lineT;
            }

            public void ClampLine(int workLength)
            {
                RebuildLine();
                for (int i = 0; i < maxLength; i++)
                {
                    if (i >= workLength) line[i] = -1;
                }
            }
        }

        [Serializable]
        public class ScatterPay
        {
            public int scattersCount;
            public int pay;
            public int freeSpins;
            public int payMult = 1;
            public int freeSpinsMult = 1;
            public UnityEvent WinEvent;

            public ScatterPay()
            {
                payMult = 1;
                freeSpinsMult = 1;
                scattersCount = 3;
                pay = 0;
                freeSpins = 0;
            }
        }

        public static class ClassExt
        {
            public enum FieldAllign { Left, Right, Center }

            /// <summary>
            /// Return formatted string; (F2, N5, e, r, p, X, D12, C)
            /// </summary>
            /// <param name="fNumber"></param>
            /// <param name="format"></param>
            /// <param name="field"></param>
            /// <returns></returns>
            public static string ToString(this float fNumber, string format, int field)
            {
                string form = "{0," + field.ToString() + ":" + format + "}";
                string res = String.Format(form, fNumber);
                return res;
            }

            /// <summary>
            /// Return formatted string; (F2, N5, e, r, p, X, D12, C)
            /// </summary>
            /// <param name="fNumber"></param>
            /// <param name="format"></param>
            /// <param name="field"></param>
            /// <returns></returns>
            public static string ToString(this string s, int field)
            {
                string form = "{0," + field.ToString() + "}";
                string res = String.Format(form, s);
                return res;
            }

            /// <summary>
            /// Return formatted string; (F2, N5, e, r, p, X, D12, C)
            /// </summary>
            /// <param name="fNumber"></param>
            /// <param name="format"></param>
            /// <param name="field"></param>
            /// <returns></returns>
            public static string ToString(this string s, int field, FieldAllign fAllign)
            {
                int length = s.Length;
                if (length >= field)
                {
                    string form = "{0," + field.ToString() + "}";
                    return String.Format(form, s);
                }
                else
                {
                    if (fAllign == FieldAllign.Center)
                    {
                        int lCount = (field - length) / 2;
                        int rCount = field - length - lCount;
                        string lSp = new string('*', lCount);
                        string rSp = new string('*', rCount);
                        return (lSp + s + rSp);
                    }
                    else if (fAllign == FieldAllign.Left)
                    {
                        int lCount = (field - length);
                        string lSp = new string('*', lCount);
                        return (s + lSp);
                    }
                    else
                    {
                        string form = "{0," + field.ToString() + "}";
                        return String.Format(form, s);
                    }
                }
            }

            private static string ToStrings<T>(T[] a)
            {
                string res = "";
                for (int i = 0; i < a.Length; i++)
                {
                    res += a[i].ToString();
                    res += " ";
                }
                return res;
            }

            private static string ToStrings(float[] a, string format, int field)
            {
                string res = "";
                for (int i = 0; i < a.Length; i++)
                {
                    res += a[i].ToString(format, field);
                    res += " ";
                }
                return res;
            }

            private static string ToStrings(string[] a, int field, ClassExt.FieldAllign allign)
            {
                string res = "";
                for (int i = 0; i < a.Length; i++)
                {
                    res += a[i].ToString(field, allign);
                    res += " ";
                }
                return res;
            }

            private static float[] Mul(float[] a, float[] b)
            {
                if (a.Length != b.Length) return null;
                float[] res = new float[a.Length];
                for (int i = 0; i < res.Length; i++)
                {
                    res[i] = a[i] * b[i];
                }
                return res;
            }

        }

        /// <summary>
        /// Helper class to make combinations from symbols with wild
        /// </summary>
        public class ComboCounter
        {
            public List<int> combo;
            public List<int> positions;

            List<byte> counterSizes;

            public ComboCounter(List<int> positions)
            {
                this.positions = positions;
                counterSizes = GetComboCountsForSymbols();
                combo = new List<int>(counterSizes.Count);

                for (int i = 0; i < counterSizes.Count; i++) // create in counter first combination
                {
                    combo.Add(0);
                }
            }

            /// <summary>
            /// get list with counts of combinations for each position
            /// </summary>
            /// <returns></returns>
            private List<byte> GetComboCountsForSymbols()
            {
                List<byte> res = new List<byte>();
                foreach (var item in positions)
                {
                    res.Add((byte)(1)); // wild or symbol (0 or 1)
                }
                return res;
            }

            private bool Next()
            {
                for (int i = counterSizes.Count - 1; i >= 0; i--)
                {
                    if (combo[i] < counterSizes[i])
                    {
                        combo[i]++;
                        if (i != counterSizes.Count - 1) // reset low "bytes"
                        {
                            for (int j = i + 1; j < counterSizes.Count; j++)
                            {
                                combo[j] = 0;
                            }
                        }
                        return true;
                    }
                }
                return false;
            }

            public bool NextCombo()
            {
                if (Next())
                {
                    return true;
                }
                return false;
            }
        }
}
