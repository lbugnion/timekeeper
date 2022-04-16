using Markdig;
using Newtonsoft.Json;
using System;

namespace Timekeeper.DataModel
{
    public class Chat
    {
        [JsonIgnore]
        public string ContainerCssClass { get; set; }

        [JsonIgnore]
        public string CssClass { get; set; }

        [JsonIgnore]
        public string LikeCssClass { get; set; }

        public string CustomColor { get; set; }

        [JsonIgnore]
        public string DisplayColor { get; set; }

        public string Key { get; set; }

        public DateTime MessageDateTime { get; set; }

        [JsonIgnore]
        public string MessageHtml => Markdown.ToHtml(MessageMarkdown);

        public string MessageMarkdown { get; set; }

        public string SenderName { get; set; }

        public string SessionName { get; set; }

        public string UniqueId { get; set; }

        public string UserId { get; set; }
    }
}