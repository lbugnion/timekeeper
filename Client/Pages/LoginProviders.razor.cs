using Microsoft.AspNetCore.Components;

namespace Timekeeper.Client.Pages
{
    public partial class LoginProviders
    {
        [Parameter]
        public string SessionId { get; set; }
        
        [Parameter]
        public string Redirect { get; set; }

        public LoginProviders() 
        {
            Redirect = "host";
        }
    }
}
