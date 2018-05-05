using System;

namespace Miner_WPF.Models
{
    public class Binary
    {
        public const int K = 1024;
        public const int M = 1024 * 1024;
        public const int G = 1024 * 1024 * 1024;
        public const long T = 1099511627776L;
        public const long P = 1125899906842624L;
        public static string GetBinaryString(double value, int digits = 1)
        {

            if (value >= P)
            {
                return $"{Math.Round(value / P, digits)}P";
            }
            else if (value >= T)
            {
                return $"{Math.Round(value / T, digits)}T";
            }
            else if (value >= G)
            {
                return $"{Math.Round(value / G, digits)}G";
            }
            else if (value >= M)
            {
                return $"{Math.Round(value / M, digits)}M";
            }
            else if (value >= K)
            {
                return $"{Math.Round(value / K, digits)}K";
            }
            else
            {
                return Math.Round(value, digits).ToString();
            }
        }
    }
}
