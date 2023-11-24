using App.ViewModel;
using System.Collections.ObjectModel;

namespace App;


public partial class MainPage : ContentPage
{
    
    public MainPage(MainViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        
    }
}
