namespace Timekeeper.Client.Model
{
    public class ConfigureClock
    {
        public Session CurrentSession
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
