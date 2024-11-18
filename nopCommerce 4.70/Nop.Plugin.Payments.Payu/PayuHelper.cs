using System;
using Nop.Core.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace Nop.Plugin.Payments.Payu
{
    public class PayuHelper
    {
        public string getchecksum(string key, string txnid, string amount, string productinfo, string firstname, string email, string salt)
        {
            string checksumString;
           
            checksumString = key + "|" + txnid + "|" + amount + "|" + productinfo + "|" + firstname + "|" + email + "|||||||||||" + salt;
            
            return Generatehash512(checksumString);
        }

        public string getchecksum1(string key, string txnid, string amount, string productinfo, string firstname, string email, string salt)
        {
            string checksumString;

            checksumString = key + "|" + txnid + "|" + amount + "|" + productinfo + "|" + firstname + "|" + email + "|||||||||||" + salt;

            return checksumString;
        }

        public string verifychecksum(string MerchantId, string OrderId, string Amount, string productinfo, string firstname, string email , string status , string salt)
        {
            string hashStr;
            hashStr = salt + "|" + status + "|||||||||||" + email + "|" + firstname + "|" + productinfo + "|" + Amount + "|" + OrderId + "|" + MerchantId;
            
            return Generatehash512(hashStr);
        }
        
        
        public string Generatehash512(string text)
        {

            byte[] message = System.Text.Encoding.UTF8.GetBytes(text);

            System.Text.UnicodeEncoding UE = new UnicodeEncoding();
            byte[] hashValue;
            SHA512Managed hashString = new SHA512Managed();
            string hex = "";
            hashValue = hashString.ComputeHash(message);
            foreach (byte x in hashValue)
            {
                hex += String.Format("{0:x2}", x);
            }
            return hex;

        }

    }	
}
