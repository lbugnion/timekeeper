using Microsoft.AspNetCore.Components;
using Timekeeper.DataModel;

namespace Timekeeper.Client.Pages
{
    public partial class ChatView
    {
        [Parameter]
        public Chat Chat { get; set; }

        public MarkupString GetMarkup(string html)
        {
            return new MarkupString(html);
        }
    }
}
