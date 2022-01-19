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
        public const string AboutPageTitle = "Timekeeper: About";
        public const bool AllowSessionSelection = true;
        public const bool CanEditGuestName = true;
        public const string ConfigurePageTitle = "Learn Live: Configure";
        public const string PollsPageTitle = "Learn Live Polls";
        public const string GuestPageTitle = "Learn Live Guest Page";
        public const string LoginPageTitle = "Learn Live: Login";
        public const string MainPageTitle = "Learn Live Timekeeper";
        public const string SessionPageTitle = "Learn Live: Sessions";
        public const string WindowTitle = "Learn Live";

#if DEBUG
        public const bool MustAuthorize = false;
#else
        public const bool MustAuthorize = true;
#endif

        public static string HeaderClass => "header";

        public static string ImagePath => "images/header-logo.png";

        public static string ForegroundClass => "foreground";

        public static string FooterClass => "footer";
    }
}