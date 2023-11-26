using App.ViewModel;

namespace App;

public partial class SecondPage : ContentPage
{
	public SecondPage(SecondViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}