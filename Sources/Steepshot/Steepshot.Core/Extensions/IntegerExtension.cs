using System;
namespace Steepshot.Core.Extensions
{
    public static class IntegerExtension
    {
        public static string CounterFormat(this int number)
        {
            int i = (int)Math.Pow(10, (int)Math.Max(0, Math.Log10(number) - 2));
            number = number / i * i;

            if (number >= 1000000)
                return (number / 1000000D).ToString("0.#m");
            if (number >= 100000)
                return (number / 1000D).ToString("0k");
            if (number >= 100000)
                return (number / 1000D).ToString("0.#k");
            if (number >= 10000)
                return (number / 1000D).ToString("0.#k");
            return number.ToString();
        }
    }
}
