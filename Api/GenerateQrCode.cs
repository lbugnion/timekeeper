using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using QRCoder;
using System.Drawing;
using System.Threading.Tasks;
using System.Web;

namespace Timekeeper
{
    public static class GenerateQrCode
    {
        private static byte[] ImageToByteArray(Image image)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(image, typeof(byte[]));
        }

        [FunctionName("GenerateQrCode")]
        public static IActionResult Run(
            [HttpTrigger(
                AuthorizationLevel.Anonymous,
                "get",
                Route = "qr")]
            HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string text = req.Query["text"];

            if (string.IsNullOrEmpty(text))
            {
                return new BadRequestObjectResult("Usage: /qr?text={text%20to%20be%20encoded}");
            }

            log.LogDebug($"Text: {text}");

            text = HttpUtility.UrlDecode(text);

            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(
                text,
                QRCodeGenerator.ECCLevel.Q);

            var qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);

            return new FileContentResult(ImageToByteArray(qrCodeImage), "image/png");
        }
    }
}