using System.Linq;
using Cryptography.ECDSA;

namespace Steepshot.Core.Utils
{
    public class KeyHelper
    {
        public static bool ValidatePrivateKey(string pass, byte[][] publicKeys)
        {
            try
            {
                var privateKey = Ditch.Core.Base58.DecodePrivateWif(pass);
                var pubKey = Secp256K1Manager.GetPublicKey(privateKey, true);
                foreach (var publicPostingKey in publicKeys)
                {
                    if (pubKey.SequenceEqual(publicPostingKey))
                        return true;
                }
            }
            catch
            {
                //todo nothing
            }
            return false;
        }

        public static bool ValidatePrivateKey(byte[] privateKey, byte[][] publicKeys)
        {
            try
            {
                var pubKey = Secp256K1Manager.GetPublicKey(privateKey, true);
                foreach (var publicPostingKey in publicKeys)
                {
                    if (pubKey.SequenceEqual(publicPostingKey))
                        return true;
                }
            }
            catch
            {
                //todo nothing
            }
            return false;
        }
    }
}
