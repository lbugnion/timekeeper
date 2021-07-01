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
        public const string AboutPageTitle = "A Bit of AI backstage: About";
        public const bool AllowSessionSelection = true;
        public const bool CanEditGuestName = false;
        public const string ConfigurePageTitle = "A Bit of AI backstage: Configure";
        public const string GuestPageTitle = "A Bit of AI Guest Page";
        public const string LoginPageTitle = "A Bit of AI backstage: Login";
        public const string MainPageTitle = "A Bit of AI backstage";
        public const string SessionPageTitle = "A Bit of AI backstage: Sessions";
        public const string WindowTitle = "A Bit of AI backstage";

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