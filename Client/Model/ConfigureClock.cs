namespace Timekeeper.Client.Model
{
    public class ConfigureClock
    {
        public Clock CurrentClock
        {
            get;
            set;
        }

        public Session CurrentSession
        {
            get;
            set;
        }
    }
}