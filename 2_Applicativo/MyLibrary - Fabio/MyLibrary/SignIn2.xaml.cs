namespace MyLibrary;

public partial class SignIn2 : ContentPage
{
	public SignIn2()
	{
		InitializeComponent();
	}

	public void OnBackClicked(object sender, EventArgs e)
	{
		Navigation.PopAsync();
    }
}