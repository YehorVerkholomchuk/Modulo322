namespace MyLibrary
{
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnNextSignIn(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SignIn2());
        }
    }

}
