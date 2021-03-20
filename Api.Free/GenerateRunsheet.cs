using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Timekeeper.Api.Free.Model;
using GitHubHelper;

namespace Timekeeper.Api.Free
{
    public static class GenerateRunsheet
    {
        [FunctionName("GenerateRunsheet")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(
                AuthorizationLevel.Function, 
                "post", 
                Route = "make-runsheet")] 
            HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            var segments = JsonConvert.DeserializeObject<IList<Segment>>(requestBody);

            var episodes = segments
                .GroupBy(s => s.Episode);

            var generator = new RunsheetGenerator();
            var files = new List<SaveFile>();

            foreach (var episode in episodes)
            {
                // TODO Move to durable function
                files.Add(generator.Generate(episode));
            }

            return new OkObjectResult("OK");
        }
    }
}

