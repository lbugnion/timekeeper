﻿using Microsoft.Extensions.Logging;
using System;

namespace TimekeeperClient.Pages
{
    public partial class BrandingTest
    {
        public Days Today
        {
            get;
            set;
        }

        protected override void OnInitialized()
        {
            Today = new Days(Log);
        }
    }

    public class Days
    {
        private string _weekDay;
        private ILogger _log;

        public string ImagePath
        {
            get;
            set;
        }

        public string BackgroundClass
        {
            get;
            set;
        }

        public string ForegroundClass
        {
            get;
            set;
        }

        public string FooterClass
        {
            get;
            set;
        }

        public Days(ILogger log)
        {
            _log = log;

            var currentDay = DateTime.Now
                .ToString("ddd", System.Globalization.CultureInfo.InvariantCulture)
                .ToLower();

            currentDay = "mon";

            ImagePath = $"/images/hello-world/hello-world-logo-{currentDay}.png";

            BackgroundClass = $"background-day-{currentDay}";
            ForegroundClass = $"foreground-day-{currentDay}";
            FooterClass = $"footer-day-{currentDay}";

            _log.LogDebug(ImagePath);
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
                ImagePath = $"~/images/hello-world/hello-world-logo-{_weekDay}.png";
            }
        }
    }
}
