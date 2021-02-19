using TimekeeperClient.Model.HelloWorld;

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
}
