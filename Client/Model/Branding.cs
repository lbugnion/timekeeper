using System;

namespace Timekeeper.Client.Model
{
    /// <summary>
    /// When you create a new branch, use the data in this class
    /// to customize the branding.
    ///
    /// Other things you can customize:
    /// - branch.css file in wwwroot/css
    /// - logo.png file in wwwroot/images
    /// - favicon.ico file in wwwroot
    ///
    /// You can also define a session template in appsettings.json
    /// </summary>
    public class Branding
    {
        public const string AboutPageTitle = "Hello World Backstage: About";
        public const bool AllowSessionSelection = true;
        public const bool CanEditGuestName = false;
        public const string ConfigurePageTitle = "Hello World Backstage: Configure";
        public const string GuestPageTitle = "Welcome Backstage!";
        public const string LoginPageTitle = "Hello World Backstage: Login";
        public const string MainPageTitle = "Hello World Backstage";
        public const string SessionPageTitle = "Hello World Backstage: Sessions";
        public const string WindowTitle = "Hello World Backstage";

#if DEBUG
        public const bool MustAuthorize = false;
#else
        public const bool MustAuthorize = true;
#endif

        public static string HeaderClass
        {
            get;
            private set;
        }

        public static string ImagePath
        {
            get;
            private set;
        }

        public static string ForegroundClass
        {
            get;
            private set;
        }

        public static string FooterClass
        {
            get;
            private set;
        }

        static Branding()
        {
            var currentDay = DateTime.Now
                .ToString("ddd", System.Globalization.CultureInfo.InvariantCulture)
                .ToLower();

            ImagePath = $"/images/hello-world-logo-{currentDay}.png";

            HeaderClass = $"background-day-{currentDay}";
            ForegroundClass = $"foreground-day-{currentDay}";
            FooterClass = $"footer-day-{currentDay}";
        }
    }
}