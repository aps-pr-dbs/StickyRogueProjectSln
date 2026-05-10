using StickyRogueProject.Views;

namespace StickyRogueProject
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            // ลงทะเบียน Route ทั้งหมดที่ใช้ GoToAsync
            Routing.RegisterRoute("MainPage", typeof(MainPage));
            Routing.RegisterRoute("ClassSelectPage", typeof(ClassSelectPage));
            Routing.RegisterRoute("ShopPage", typeof(ShopPage));
            Routing.RegisterRoute("RopPage", typeof(RopPage));
            Routing.RegisterRoute("CombatPage", typeof(Views.CombatPage));
            Routing.RegisterRoute("ChurchPage", typeof(Views.ChurchPage));
            Routing.RegisterRoute("CasinoMenu", typeof(Views.CasinoMenu));
            Routing.RegisterRoute("BlackjackPage", typeof(Views.BlackjackPage));
            Routing.RegisterRoute("HighLowPage", typeof(Views.HighLowPage));
            Routing.RegisterRoute("StoryPage", typeof(Views.StoryPage));

        }
    }
       
}
