namespace Timekeeper.DataModel
{
    public class UpdateHostInfo
    {
        public UpdateAction Action
        {
            get;
            set;
        }

        public StartClockMessage Clock
        {
            get;
            set;
        }

        public string PreviousClockId { get; set; }

        public string SessionName
        {
            get;
            set;
        }
    }
}