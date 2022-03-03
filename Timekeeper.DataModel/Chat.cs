using Markdig;
using Newtonsoft.Json;
using System;

namespace Timekeeper.DataModel
{
    public class Chat
    {
        public string Key { get; set; }
        public string SenderName { get; set; }

        public string MessageMarkdown { get; set; }

        [JsonIgnore]
        public string MessageHtml => Markdown.ToHtml(MessageMarkdown);

        public string SessionName { get; set; }

        public DateTime MessageDateTime { get; set; }

        public string UserId { get; set; }

        [JsonIgnore]
        public string DisplayColor { get; set; }

        public string CustomColor { get; set; }

        [JsonIgnore]
        public string CssClass { get; set; }

        [JsonIgnore]
        public string ContainerCssClass { get; set; }

        public string UniqueId { get; set; }
    }
}
