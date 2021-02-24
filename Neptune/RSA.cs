using System;
using NBitcoin;
using Neptune;

namespace RSA
{
    //add new Nuget package
    //NBitcoin
    public  static class RSA
    {
        public static Wallet KeyGenerate()
        {
            Key privatekey = new Key();
            var v = privatekey.GetBitcoinSecret(Network.Main).GetAddress();
            // var  v = privatekey.BitcoinSecret.GetAddress()
            var address = BitcoinAddress.Create(v.ToString(), Network.Main);
            return new Wallet {PublicKey = v.ToString(), PrivateKey = privatekey.GetBitcoinSecret(Network.Main).ToString() };
        }

        // signing ----- Upcoin, Neptune
        public static string Sign(string privKey, string msgtoString)
        {
            var secret = Network.Main.CreateBitcoinSecret(privKey);
            //secret = BitcoinAddress.Create(secret.ToString(), Network.Main);
            var signature = secret.PrivateKey.SignMessage(msgtoString);
            var v = secret.PubKey.VerifyMessage(msgtoString,signature);

            return signature;
        }

        // verification for signature
        public static bool Verify(string pbkey , string orginalMessage, string signedMessage)
        {
            var address = BitcoinAddress.Create(pbkey, Network.Main);
            var pkh = (address as IPubkeyHashUsable);

            var _bol = pkh.VerifyMessage(orginalMessage,signedMessage);
            return _bol;
        }
    }
}
