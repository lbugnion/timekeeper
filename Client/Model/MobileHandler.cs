using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace Timekeeper.Client.Model
{
    public class MobileHandler
    {
        private IJSRuntime _js;

        public bool IsNoSleepVisible
        {
            get;
            private set;
        }

        public void EnableNoSleep()
        {
            _js.InvokeVoidAsync("nosleep.enableDisableNoSleep", true);
            IsNoSleepVisible = false;
        }

        public async Task<MobileHandler> Initialize(IJSRuntime js)
        {
            _js = js;
            IsNoSleepVisible = await _js.InvokeAsync<bool>("nosleep.isMobile");
            return this;
        }
    }
}