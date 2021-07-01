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
        public const bool AllowSessionSelection = false;
        public const bool CanEditGuestName = false;
        public const string ConfigurePageTitle = "Timekeeper: Configure";
        public const string GuestPageTitle = "GX All Hands";
        public const string LoginPageTitle = "GX All Hands: Login";
        public const string MainPageTitle = "GX All Hands";
        public const string SessionPageTitle = "GX All Hands: Sessions";
        public const string WindowTitle = "GX All Hands";
        
        public const bool MustAuthorize = true;

        public static string HeaderClass => "header";

        public static string ImagePath => "images/header-logo.png";

        public static string ForegroundClass => "foreground";

        public static string FooterClass => "footer";
    }
}