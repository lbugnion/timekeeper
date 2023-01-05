using Markdig;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Timekeeper.DataModel
{
    public class Chat
    {
        [JsonIgnore]
        public string CssClass { get; set; }

        [JsonIgnore]
        public string ButtonLikeCssClass { get; set; }

        [JsonIgnore]
        public string BackgroundLikeCssClass { get; set; }

        [JsonIgnore]
        public string SpanLikeCssClass { get; set; }

        [JsonIgnore]
        public string LikeThumbCssClass { get; set; }

        public string LikeButtonTitle
        { 
            get
            {
                if (Likes.Count == 0)
                {
                    return "Noone likes this yet";
                }

                var allNames = Likes
                    .Select(l => l.DisplayName)
                    .OrderBy(m => m == Constants.YouName ? -1 : 0)
                    .Aggregate((l1, l2) => $"{l1}, {l2}");

                var likes = "like";

                if (Likes.Count == 1
                    && Likes[0].CustomName != Constants.YouName)
                {
                    likes = "likes";
                }

                return $"{allNames} {likes} this";
            }
        }

        public string CustomColor { get; set; }

        [JsonIgnore]
        public string DisplayColor { get; set; }

        [JsonIgnore]
        public string Suffix { get; set; }

        public string Key { get; set; }

        public DateTime MessageDateTime { get; set; }

        [JsonIgnore]
        public string MessageHtml => Markdown.ToHtml(MessageMarkdown);

        public string MessageMarkdown { get; set; }

        public string SenderName { get; set; }

        public string SessionName { get; set; }

        public string SessionId { get; set; }

        public string UniqueId { get; set; }

        public string UserId { get; set; }

        public IList<PeerMessage> Likes { get; set; } = new List<PeerMessage>();
    }
}