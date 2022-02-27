using Markdig;
using Newtonsoft.Json;
using System;

namespace Timekeeper.DataModel
{
    public class Chat
    {
        private string _messageMarkdown;

        public string Key { get; set; }
        public string SenderName { get; set; }

        public string MessageMarkdown
        {
            get => _messageMarkdown; 
            
            set
            {
                _messageMarkdown = value;
                MessageHtml = Markdown.ToHtml(_messageMarkdown);
            }
        }

        [JsonIgnore]
        public string MessageHtml { get; set; }

        public string SessionName { get; set; }

        public DateTime MessageDateTime { get; set; }

        public string UserId { get; set; }

        public string Color { get; set; }

        [JsonIgnore]
        public string CssClass { get; set; }

        [JsonIgnore]
        public string ContainerCssClass { get; set; }

        public string UniqueId { get; set; }
    }
}
