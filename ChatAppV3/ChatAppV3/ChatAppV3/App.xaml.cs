using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ChatAppV3.Views;

namespace ChatAppV3
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();



            MainPage = new NavigationPage(new LoginPage()) {BarBackgroundColor = Color.FromHex("#2ac2c5"), Title = "Chat App" };



        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
