using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using QRCoder;

namespace BidQr
{
    public static class QrCoder
    {        
        [FunctionName("GetQrCode")]
        public static async Task<string> GetQrCode(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("GetQrCode function called");

            string bidUrl =  req.Query["code"];

            var requestBody =  new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(await requestBody);
            bidUrl ??= data?.code;

            using var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(bidUrl, QRCodeGenerator.ECCLevel.Q);
            string qrImage;
            
            using (Base64QRCode qrCode = new(qrCodeData))
            {
                 qrImage =  qrCode.GetGraphic(20);  
            }

            string responseMessage = string.IsNullOrEmpty(bidUrl)
                ? "Enter a valid bid code"
                : qrImage;

             return responseMessage;

        }
    }
}
