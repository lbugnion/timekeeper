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
        public const string AboutPageTitle = "GX All Hands: About";
        public const bool AllowSessionSelection = true;
        public const bool CanEditGuestName = false;
        public const string ChatsPageTitle = "GX All Hands Chats";
        public const string ConfigurePageTitle = "Timekeeper: Configure";
        public const string GuestPageTitle = "GX All Hands";
        public const string LoginPageTitle = "GX All Hands: Login";
        public const string MainPageTitle = "GX All Hands";

#if DEBUG
        public const bool MustAuthorize = false;
#else
        public const bool MustAuthorize = true;
#endif

        public const string PollsPageTitle = "GX All Hands Polls";
        public const string SessionPageTitle = "GX All Hands: Sessions";
        public const string WindowTitle = "GX All Hands";

        public static string FooterClass => "footer";

        public static string ForegroundClass => "foreground";

        public static string HeaderClass => "header";

        public static string ImagePath => "images/header-logo.png";
    }
}