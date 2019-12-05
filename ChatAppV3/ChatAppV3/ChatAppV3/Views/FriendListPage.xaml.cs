using ChatAppV3.ViewModels;
using ChatAppV3.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using System.Text.Json;

namespace ChatAppV3.Views
{

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
           // ChatPage page = new ChatPage();
           // page.BindingContext = viewModel;
           // await DisplayAlert("helllo","Hello","ok");
            var selectedItem = e.Item as GroupListModel;

            //MainThread.BeginInvokeOnMainThread(async () => {
            //MessagingCenter.Send<FriendListPage, string>(this, "CurrentGroup", d);
            GroupListModel group = new GroupListModel() { 
                groupName = selectedItem.groupName,
                groupID = selectedItem.groupID
            };
            await DisplayAlert("msg", group.groupName + group.groupID, "ok");
            var json = JsonSerializer.Serialize<GroupListModel>(group);
            if (!Application.Current.Properties.ContainsKey("CurrentGroup"))
            {
                
                Application.Current.Properties.Add("CurrentGroup", json);

            }
            else
            {
                Application.Current.Properties["CurrentGroup"] = json;
            }
            
            await Navigation.PushModalAsync(new ChatPage(),false); 

            //});
        }

        private void OnDelete(object sender, EventArgs e)
        {

        }

    }
}