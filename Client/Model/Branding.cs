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
        public const string WindowTitle = "A Bit of AI backstage";
        public const string HostPageTitle = "A Bit of AI Host Page";
        public const string GuestPageTitle = "A Bit of AI Guest Page";
        public const string ConfigurePageTitle = "A Bit of AI backstage: Configure";
        public const string AboutPageTitle = "A Bit of AI backstage: About";
        public const string MainPageTitle = "A Bit of AI backstage";
        public const string LoginPageTitle = "A Bit of AI backstage: Login";
        public const bool CanEditSessionAndGuestName = false;
        public const string TemplateName = "ClocksTemplate";

#if DEBUG
        public const bool MustAuthorize = false;
#else
        public const bool MustAuthorize = true;
#endif
    }
}
