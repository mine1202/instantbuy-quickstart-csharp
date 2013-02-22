
using System;
using System.Web;
using System.Web.UI;
using System.IO;
using JWT;
using Newtonsoft.Json;
using InstantBuyLibrary;

namespace InstantBuySample
{
  public class NotifyHandler : System.Web.IHttpHandler
  {
		
    public class Request {
      public String jwt;
    }

    public virtual bool IsReusable {
      get {
        return false;
      }
    }
		
    public virtual void ProcessRequest (HttpContext context)
    {
      HttpRequest request = context.Request;
      HttpResponse response = context.Response;

      StreamReader streamReader = new StreamReader (request.InputStream);
      
      String input;
      String json = "";
      while ((input = streamReader.ReadLine()) != null) {
        Console.WriteLine (input); 
        json += input;
      }

      Request req = JsonConvert.DeserializeObject<Request> (json);
      String jsonResponse = JsonWebToken.Decode (req.jwt, Config.getMerchantSecret(), false);
      JwtResponse jwtResponse = JsonConvert.DeserializeObject<JwtResponse>(jsonResponse); 

      WalletBody tsb = new WalletBody.TransactionStatusNotificationBuilder()
          .GoogleTransactionId(jwtResponse.response.googleTransactionId)
          .ClientId (Config.getOauthClientId())
          .MerchantName(Config.getMerchantName())
          .Origin(Config.getOrigin (request))
          .SigningCertificateFingerprint(new SigningCert(Config.getFingerPrint (), SigningCert.SHA1))
          .Status(WalletBody.Status.SUCCESS)
          .Build ();

      JwtRequest tsn = new JwtRequest (JwtRequest.FULL_WALLET, Config.getMerchantId (), tsb);
      
      tsn.exp = Convert.ToInt64 (DateTime.Now.Subtract (new DateTime (1970, 1, 1, 0, 0, 0)).TotalSeconds) + 60000L;
      
      String mwrJwt = JsonWebToken.Encode (tsn, Config.getMerchantSecret(), JwtHashAlgorithm.HS256); 
      
      response.Write (mwrJwt);
    }
  }
}
