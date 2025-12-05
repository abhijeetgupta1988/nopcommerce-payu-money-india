using System.Text.Json.Serialization;

namespace Nop.Plugin.Payments.Payu.Models;
public class PayUIPNResponse
{
    [JsonPropertyName("mihpayid")]
    public string Mihpayid { get; set; }

    [JsonPropertyName("mode")]
    public string Mode { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("unmappedstatus")]
    public string Unmappedstatus { get; set; }

    [JsonPropertyName("key")]
    public string Key { get; set; }

    [JsonPropertyName("txnid")]
    public string Txnid { get; set; }

    [JsonPropertyName("amount")]
    public string Amount { get; set; }

    [JsonPropertyName("cardCategory")]
    public string CardCategory { get; set; }

    [JsonPropertyName("discount")]
    public string Discount { get; set; }

    [JsonPropertyName("net_amount_debit")]
    public string NetAmountDebit { get; set; }

    [JsonPropertyName("addedon")]
    public string Addedon { get; set; }

    [JsonPropertyName("productinfo")]
    public string Productinfo { get; set; }

    [JsonPropertyName("firstname")]
    public string Firstname { get; set; }

    [JsonPropertyName("lastname")]
    public string Lastname { get; set; }

    [JsonPropertyName("address1")]
    public string Address1 { get; set; }

    [JsonPropertyName("address2")]
    public string Address2 { get; set; }

    [JsonPropertyName("city")]
    public string City { get; set; }

    [JsonPropertyName("state")]
    public string State { get; set; }

    [JsonPropertyName("country")]
    public string Country { get; set; }

    [JsonPropertyName("zipcode")]
    public string Zipcode { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; }

    [JsonPropertyName("phone")]
    public string Phone { get; set; }

    [JsonPropertyName("udf1")]
    public string Udf1 { get; set; }

    [JsonPropertyName("udf2")]
    public string Udf2 { get; set; }

    [JsonPropertyName("udf3")]
    public string Udf3 { get; set; }

    [JsonPropertyName("udf4")]
    public string Udf4 { get; set; }

    [JsonPropertyName("udf5")]
    public string Udf5 { get; set; }

    [JsonPropertyName("udf6")]
    public string Udf6 { get; set; }

    [JsonPropertyName("udf7")]
    public string Udf7 { get; set; }

    [JsonPropertyName("udf8")]
    public string Udf8 { get; set; }

    [JsonPropertyName("udf9")]
    public string Udf9 { get; set; }

    [JsonPropertyName("udf10")]
    public string Udf10 { get; set; }

    [JsonPropertyName("hash")]
    public string Hash { get; set; }

    [JsonPropertyName("field1")]
    public string Field1 { get; set; }

    [JsonPropertyName("field2")]
    public string Field2 { get; set; }

    [JsonPropertyName("field3")]
    public string Field3 { get; set; }

    [JsonPropertyName("field4")]
    public string Field4 { get; set; }

    [JsonPropertyName("field5")]
    public string Field5 { get; set; }

    [JsonPropertyName("field6")]
    public string Field6 { get; set; }

    [JsonPropertyName("field7")]
    public string Field7 { get; set; }

    [JsonPropertyName("field8")]
    public string Field8 { get; set; }

    [JsonPropertyName("field9")]
    public string Field9 { get; set; }

    [JsonPropertyName("payment_source")]
    public string PaymentSource { get; set; }

    [JsonPropertyName("PG_TYPE")]
    public string PGType { get; set; }

    [JsonPropertyName("bank_ref_num")]
    public string BankRefNum { get; set; }

    [JsonPropertyName("bankcode")]
    public string Bankcode { get; set; }

    [JsonPropertyName("error")]
    public string Error { get; set; }

    [JsonPropertyName("error_Message")]
    public string ErrorMessage { get; set; }

    [JsonPropertyName("cardnum")]
    public string Cardnum { get; set; }

    [JsonPropertyName("cardhash")]
    public string Cardhash { get; set; }
}
