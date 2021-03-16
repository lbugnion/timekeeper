using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace Timekeeper.Client.Model
{
    public class MobileHandler
    {
        private const string KeepDeviceAwakeText = "Keep device awake";
        private const string AllowDeviceToSleepText = "Allow device to sleep";

        private IJSRuntime _js;

        public bool IsMobile
        {
            get;
            private set;
        }

        public async Task<MobileHandler> Initialize(IJSRuntime js)
        {
            _js = js;
            IsMobile = await _js.InvokeAsync<bool>("nosleep.isMobile");
            NoSleepButtonText = KeepDeviceAwakeText;
            return this;
        }

        private bool _isNoSleepActive;

        public string NoSleepButtonText
        {
            get;
            private set;
        }

        public void ToggleNoSleep()
        {
            if (_isNoSleepActive)
            {
                _js.InvokeVoidAsync("nosleep.enableDisableNoSleep", false);
                _isNoSleepActive = false;
                NoSleepButtonText = KeepDeviceAwakeText;
            }
            else
            {
                _js.InvokeVoidAsync("nosleep.enableDisableNoSleep", true);
                _isNoSleepActive = true;
                NoSleepButtonText = AllowDeviceToSleepText;
            }
        }
    }
}
