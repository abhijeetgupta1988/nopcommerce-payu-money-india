using System.Security.Cryptography;
using System.Text;

namespace Nop.Plugin.Payments.Payu;

public class PayuHelper
{
    public string GetChecksum(string key, string txnid, string amount, string productinfo, string firstname, string email, string salt)
    {
        string checksumString;

        checksumString = key + "|" + txnid + "|" + amount + "|" + productinfo + "|" + firstname + "|" + email + "|||||||||||" + salt;

        return Generatehash512(checksumString);
    }

    public string Getchecksum1(string key, string txnid, string amount, string productinfo, string firstname, string email, string salt)
    {
        string checksumString;

        checksumString = key + "|" + txnid + "|" + amount + "|" + productinfo + "|" + firstname + "|" + email + "|||||||||||" + salt;

        return checksumString;
    }

    public string VerifyChecksum(string merchantId, string orderId, string amount, string productinfo, string firstname, string email, string status, string salt)
    {
        string hashStr;
        hashStr = salt + "|" + status + "|||||||||||" + email + "|" + firstname + "|" + productinfo + "|" + amount + "|" + orderId + "|" + merchantId;

        return Generatehash512(hashStr);
    }

    public string Generatehash512(string text)
    {
        byte[] message = Encoding.UTF8.GetBytes(text);
       // SHA512 hashString = SHA512.Create();
        string hex = "";
        var hashValue = SHA512.Create().ComputeHash(message);
        foreach (byte x in hashValue)
        {
            hex += String.Format("{0:x2}", x);
        }
        return hex;
    }

}
