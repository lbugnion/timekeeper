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
        public const bool CanEditSessionAndGuestName = false;
        public const string ConfigurePageTitle = "Timekeeper: Configure";
        public const string GuestPageTitle = "GX All Hands";
        public const string HostPageTitle = "GX All Hands";
        public const string LoginPageTitle = "GX All Hands: Login";
        public const string MainPageTitle = "GX All Hands";
        public const bool MustAuthorize = true;
        public const string TemplateName = "ClocksTemplate";
        public const string WindowTitle = "GX All Hands";
    }
}