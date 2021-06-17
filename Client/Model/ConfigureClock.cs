using Timekeeper.DataModel;

namespace Timekeeper.Client.Model
{
    public class ConfigureClock
    {
        public Clock CurrentClock
        {
            get;
            set;
        }

        public SignalRHost Host
        {
            get;
            set;
        }
    }
}