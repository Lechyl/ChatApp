using ChatAppV3.ViewModels;
using ChatAppV3.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ChatAppV3.Views
{
    public class GroupNameClass
    {
        public string Name { get; set; }
    }
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FriendListPage : ContentPage
    {
        public FriendListPage()
        {
            InitializeComponent();
        }

        private void MenuItem_Clicked(object sender, EventArgs e)
        {

        }

        private async void MyTappedItem(object sender, ItemTappedEventArgs e)
        {
            //var d = e.Item as GroupNameClass;

            // ChatPageVM viewModel = new ChatPageVM();
            //ChatPage page = new ChatPage();
            //page.BindingContext = viewModel;
            await DisplayAlert("helllo","Hello","ok");

        }

        private void OnDelete(object sender, EventArgs e)
        {

        }

    }
}