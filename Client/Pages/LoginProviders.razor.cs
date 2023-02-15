using Microsoft.AspNetCore.Components;

namespace Timekeeper.Client.Pages
{
    public partial class LoginProviders
    {
        private const string HostRedirect = "host";

        private string _redirect;

        [Parameter]
        public string SessionId { get; set; }
        
        [Parameter]
        public string Redirect 
        {
            get => _redirect;
            
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _redirect = HostRedirect;
                }
                else
                {
                    _redirect = value;
                }
            }
        }

        public LoginProviders() 
        {
            _redirect = HostRedirect;
        }
    }
}
