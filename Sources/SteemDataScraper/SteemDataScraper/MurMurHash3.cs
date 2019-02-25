using System;
using System.Text;

namespace SteemDataScraper
{
    public static class MurMurHash3
    {
        //Change to suit your needs
        const uint Seed = 144;

        public static int Hash(string value)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            const uint c1 = 0xcc9e2d51;
            const uint c2 = 0x1b873593;

            uint h1 = Seed;
            uint streamLength = 0;
            while (bytes.Length > streamLength)
            {
                var cl = Math.Min(bytes.Length - (int)streamLength, 4);

                uint k1 = bytes[streamLength];
                if (cl > 1)
                    k1 |= (uint)(bytes[streamLength + 1] << 8);

                if (cl > 2)
                    k1 |= (uint)(bytes[streamLength + 2] << 16);

                if (cl > 3)
                    k1 |= (uint)(bytes[streamLength + 3] << 24);

                k1 *= c1;
                k1 = (k1 << 15) | (k1 >> (32 - 15));
                k1 *= c2;
                h1 ^= k1;

                if (cl == 4)
                {
                    h1 = (h1 << 13) | (h1 >> (32 - 13));
                    h1 = h1 * 5 + 0xe6546b64;
                }

                streamLength += (uint)cl;
            }

            // finalization, magic chants to wrap it all up
            h1 ^= streamLength;
            h1 ^= h1 >> 16;
            h1 *= 0x85ebca6b;
            h1 ^= h1 >> 13;
            h1 *= 0xc2b2ae35;
            h1 ^= h1 >> 16;

            unchecked //ignore overflow
            {
                return (int)h1;
            }
        }
    }
}