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
        public const string ChatsPageTitle = "Timekeeper Chats";
        public const string ConfigurePageTitle = "Timekeeper: Configure";
        public const string GuestPageTitle = "Timekeeper Guest Page";
        public const string LoginPageTitle = "Timekeeper: Login";
        public const string MainPageTitle = "Timekeeper";
        public const bool MustAuthorize = false;
        public const string PollsPageTitle = "Timekeeper Polls";
        public const string SessionPageTitle = "Timekeeper: Sessions";
        public const string WindowTitle = "Timekeeper";

        public static string FooterClass => "footer";

        public static string ForegroundClass => "foreground";

        public static string HeaderClass => "header";

        public static string ImagePath => "images/header-logo.png";
    }
}