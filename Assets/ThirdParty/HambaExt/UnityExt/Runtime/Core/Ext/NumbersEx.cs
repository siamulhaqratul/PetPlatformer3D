using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityExt
{
    /// <summary>
    /// Extension methods for numeric utilities and coin UI string formatting.
    /// </summary>
    public static class ExNumbers
    {
        // ========================= PUBLIC METHODS ========================

        /// <summary>
        /// Returns true if the float is greater than or equal to <paramref name="min"/> and less than <paramref name="max"/>.
        /// </summary>
        /// <param name="a">The value to check.</param>
        /// <param name="min">Inclusive lower bound.</param>
        /// <param name="max">Exclusive upper bound.</param>
        public static bool ExIsBetween(this float a, float min, float max)
        {
            return a >= min && a < max;
        }

        /// <summary>
        /// Returns true if the float is within <paramref name="tolerence"/> of <paramref name="b"/>.
        /// </summary>
        /// <param name="a">The value to check.</param>
        /// <param name="b">The center value.</param>
        /// <param name="tolerence">The allowed difference (default 0.01).</param>
        public static bool ExIsAround(this float a, float b, float tolerence = 0.01f)
        {
            return a > b - tolerence && a < b + tolerence;
        }

        /// <summary>
        /// Remaps a value from one range to another.
        /// </summary>
        /// <param name="value">The value to remap.</param>
        /// <param name="from1">Source range minimum.</param>
        /// <param name="to1">Source range maximum.</param>
        /// <param name="from2">Target range minimum.</param>
        /// <param name="to2">Target range maximum.</param>
        /// <returns>The remapped value in the target range.</returns>
        public static float ExRemap(this float value, float from1, float to1, float from2, float to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }

        /// <summary>
        /// Formats a long coin value as a UI string with K, M, B, or T suffixes.
        /// </summary>
        /// <param name="coinNum">The coin value to format.</param>
        /// <returns>Formatted string (e.g., 1.2K, 3.4M, 5.6B, 7.8T).</returns>
        public static string ExGetCoinUIString(this long coinNum)
        {
            long MarkL_K = 9999L;
            long MarkL_M = 1000L * 100L * 10L - 1;
            long MarkL_B = 1000L * 100L * 10L * 1000L - 1;
            long MarkL_T = 1000L * 100L * 10L * 1000L * 1000L - 1;

            string coinStr = "";
            if (coinNum <= MarkL_K)
            {
                coinStr = "" + coinNum.ToString();
            }
            else if (coinNum > MarkL_K && coinNum <= MarkL_M)
            {
                double ln = (double)coinNum / (double)1000L;
                coinStr = ln.ToString("0.0") + "K";
            }
            else if (coinNum > MarkL_M && coinNum <= MarkL_B)
            {
                double ln = (double)coinNum / (double)(MarkL_M + 1);
                coinStr = ln.ToString("0.0") + "M";
            }
            else if (coinNum > MarkL_B && coinNum <= MarkL_T)
            {
                double ln = (double)coinNum / (double)(MarkL_B + 1);
                coinStr = ln.ToString("0.0") + "B";
            }
            else if (coinNum > MarkL_T)
            {
                double ln = (double)coinNum / (double)(MarkL_T + 1);
                coinStr = ln.ToString("0.0") + "T";
            }
            return coinStr;
        }

        /// <summary>
        /// Formats an int coin value as a UI string with K, M, or B suffixes, with optional threshold for K.
        /// </summary>
        /// <param name="coinNum">The coin value to format.</param>
        /// <param name="startsAfterTenK">If true, K suffix starts after 10,000; otherwise after 1,000.</param>
        /// <returns>Formatted string (e.g., 1.2K, 3.4M, 5.6B).</returns>
        public static string ExGetCoinUIString(this int coinNum, bool startsAfterTenK = false)
        {
            int MarkI_1K = 999;
            int MarkI_10K = 9999;
            int MarkI_M = 1000 * 100 * 10 - 1;
            int MarkI_B = 1000 * 100 * 10 * 1000 - 1;

            string coinStr = "";
            if (startsAfterTenK)
            {
                if (coinNum <= MarkI_10K)
                {
                    coinStr = "" + coinNum.ToString();
                }
                else if (coinNum > MarkI_10K && coinNum <= MarkI_M)
                {
                    double ln = (double)coinNum / (double)1000L;
                    coinStr = ln.ToString("0.0") + "K";
                }
                else if (coinNum > MarkI_M && coinNum <= MarkI_B)
                {
                    double ln = (double)coinNum / (double)(MarkI_M + 1);
                    coinStr = ln.ToString("0.0") + "M";
                }
                else if (coinNum > MarkI_B)
                {
                    double ln = (double)coinNum / (double)(MarkI_B + 1);
                    coinStr = ln.ToString("0.0") + "B";
                }
            }
            else
            {
                if (coinNum <= MarkI_1K)
                {
                    coinStr = "" + coinNum.ToString();
                }
                else if (coinNum > MarkI_1K && coinNum <= MarkI_M)
                {
                    double ln = (double)coinNum / (double)1000L;
                    coinStr = ln.ToString("0.0") + "K";
                }
                else if (coinNum > MarkI_M && coinNum <= MarkI_B)
                {
                    double ln = (double)coinNum / (double)(MarkI_M + 1);
                    coinStr = ln.ToString("0.0") + "M";
                }
                else if (coinNum > MarkI_B)
                {
                    double ln = (double)coinNum / (double)(MarkI_B + 1);
                    coinStr = ln.ToString("0.0") + "B";
                }
            }
            
            return coinStr;
        }
    }
}