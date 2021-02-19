using Microsoft.AspNetCore.Components;

namespace TimekeeperClient.Pages
{
    public partial class Index
    {
        [Parameter]
        public string Session
        {
            get;
            set;
        }
    }
}
