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
        public const string WindowTitle = "Hello World Backstage";
        public const string HostPageTitle = "Welcome Backstage!";
        public const string GuestPageTitle = "Welcome Backstage!";
        public const string ConfigurePageTitle = "Hello World Backstage: Configure";
        public const string AboutPageTitle = "Hello World Backstage: About";
        public const string MainPageTitle = "Hello World Backstage";
        public const string LoginPageTitle = "Hello World Backstage: Login";
    }
}
