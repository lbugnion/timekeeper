using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System.Linq;
using System.Threading.Tasks;
using Timekeeper.Client.Model;
using Timekeeper.Client.Model.HelloWorld;

namespace Timekeeper.Client.Pages.HelloWorld
{
    public partial class Configure
    {
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await JSRuntime.InvokeVoidAsync("setTitle", "Hello World Backstage Channel");
            await JSRuntime.InvokeVoidAsync("setBranding", "css/hello-world.css");
        }

        public Days Today
        {
            get;
            set;
        }

        public Session CurrentSession
        {
            get;
            private set;
        }

        public EditContext CurrentEditContext
        {
            get;
            private set;
        }

        protected override async Task OnInitializedAsync()
        {
            Log.LogInformation("-> OnInitializedAsync");

            Today = new Days(Log);

            CurrentSession = await Session.GetFromStorage();

            if (CurrentSession == null)
            {
                // TODO Notify the user
            }

            CurrentEditContext = new EditContext(CurrentSession);
            CurrentEditContext.OnValidationStateChanged += CurrentEditContextOnValidationStateChanged;

            Log.LogInformation("OnInitializedAsync ->");
        }

        private async void CurrentEditContextOnValidationStateChanged(object sender, ValidationStateChangedEventArgs e)
        {
            Log.LogInformation("-> CurrentEditContextOnValidationStateChanged");

            if (CurrentEditContext.GetValidationMessages().Count() == 0)
            {
                Log.LogTrace("Saving");

                await CurrentSession.Save();
            }
        }
    }
}
