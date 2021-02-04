using System.Threading.Tasks;
using TimekeeperClient.Model;

namespace TimekeeperClient.Pages
{
    public partial class Guest
    {
        public SignalRGuest Handler
        {
            get;
            private set;
        }

        protected override async Task OnInitializedAsync()
        {
            Handler = new SignalRGuest(
                Config,
                Log,
                Http);

            await Handler.Connect();
        }
    }
}
