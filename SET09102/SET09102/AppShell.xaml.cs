using Microsoft.Maui.Controls;

namespace SET09102
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Register routes for role-specific pages
            Routing.RegisterRoute("Administrator/MainPage", typeof(Administrator.Pages.MainPage));
            Routing.RegisterRoute("Administrator/MapPage", typeof(Administrator.Pages.MapPage));
            Routing.RegisterRoute("Administrator/SettingsPage", typeof(Administrator.Pages.SettingsPage));
            Routing.RegisterRoute("OperationsManager/MainPage", typeof(OperationsManager.Pages.MainPage));
            Routing.RegisterRoute("EnvironmentalScientist/MainPage", typeof(EnvironmentalScientist.Pages.MainPage));
        }
    }
}
