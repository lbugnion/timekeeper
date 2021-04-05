using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Timekeeper.Client.Pages
{
    public partial class SessionSelection
    {
        protected override async Task OnInitializedAsync()
        {
            Log.LogInformation("-> OnInitializedAsync");

            await Session.LoadAllSessions(Log);

            Log.LogInformation("OnInitializedAsync ->");
        }
    }
}
