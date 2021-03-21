using Microsoft.Extensions.Logging;
using System;

namespace Timekeeper.Client.Model
{
    public class Days
    {
        private ILogger _log;
        private string _weekDay;

        public string BackgroundClass
        {
            get;
            set;
        }

        public string FooterClass
        {
            get;
            set;
        }

        public string ForegroundClass
        {
            get;
            set;
        }

        public string ImagePath
        {
            get;
            set;
        }

        public string WeekDay
        {
            get
            {
                return _weekDay;
            }
            set
            {
                _weekDay = value;
                ImagePath = $"~/images/hello-world-logo-{_weekDay}.png";
            }
        }

        public Days(ILogger log)
        {
            _log = log;

            var currentDay = DateTime.Now
                .ToString("ddd", System.Globalization.CultureInfo.InvariantCulture)
                .ToLower();

            ImagePath = $"/images/hello-world-logo-{currentDay}.png";

            BackgroundClass = $"background-day-{currentDay}";
            ForegroundClass = $"foreground-day-{currentDay}";
            FooterClass = $"footer-day-{currentDay}";

            _log.LogDebug(ImagePath);
        }
    }
}