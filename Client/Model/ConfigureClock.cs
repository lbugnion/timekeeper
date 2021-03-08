namespace Timekeeper.Client.Model
{
    public class ConfigureClock
    {
        public HostSession CurrentSession
        {
            get;
            set;
        }

        public Clock CurrentClock
        {
            get;
            set;
        }
    }
}
