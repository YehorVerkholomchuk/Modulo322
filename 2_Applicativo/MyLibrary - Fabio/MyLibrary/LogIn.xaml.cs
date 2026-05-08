namespace MyLibrary
{
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();
        }

        async private void OnCounterClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new FilmPage());
        }
    }

}
