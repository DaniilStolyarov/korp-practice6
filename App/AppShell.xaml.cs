namespace App;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(SecondPage), typeof(SecondPage));
        Routing.RegisterRoute(nameof(StatPage), typeof(StatPage));
    }
}
