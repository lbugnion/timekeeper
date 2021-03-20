using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Timekeeper.Api.Free.Model
{
    public class RunsheetGenerator
    {
        public async Task<SaveFile> Generate(
            IGrouping<string, Segment> episode,
            ILogger log)
        {
            var segments = episode
                .OrderBy(s => s.Rank);

            var builder = new StringBuilder()
                .AppendLine($"# {episode.Key}")
                .AppendLine();

            foreach (var segment in segments)
            {
                builder
                    .AppendLine($"## {segment.Title}")
                    .AppendLine()
                    .AppendLine($"> Until: TODO")
                    .AppendLine()
                    .AppendLine("### Hosts")
                    .AppendLine();

                foreach (var host in segment.Hosts)
                {
                    builder.AppendLine($"- {host}");
                }

                builder
                    .AppendLine()
                    .AppendLine($"### Details")
                    .AppendLine();

                // Parse the description

                var index = segment.Description.ToLower().IndexOf("<ol>");
                var minutes = new List<Minute>();

                if (index > -1)
                {
                    do
                    {
                        index = segment.Description.ToLower().IndexOf("<li>");

                        if (index > -1)
                        {
                            var parsedDescription = segment.Description.Substring(index);
                            index = parsedDescription.ToLower().IndexOf("</li>");

                            if (index > -1)
                            {
                                var minuteString = parsedDescription.Substring(0, index);
                                var minute = Minute.Parse(minuteString);
                                minutes.Add(minute);
                            }
                            else
                            {
                                var message = $"Invalid description found for segment {episode.Key} / {segment.Title}";
                                log.LogError(message);

                                var notificationInfo = new NotificationInfo
                                {
                                    IsError = true,
                                    Title = "Issue with segment",
                                    Message = message
                                };

                                await NotificationService.Notify(notificationInfo, log);
                            }
                        }
                    }
                    while (index > -1);
                }

                if (minutes.Count == 0)
                {
                    builder.AppendLine("No details found in description");
                }

            }

            var result = new SaveFile
            {
                Content = builder.ToString()
            };

            return result;
        }
    }
}