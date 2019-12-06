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
            //Get selected item and convert it back to Original Data type
            var selectedItem = e.Item as GroupListModel;

            GroupListModel group = new GroupListModel()
            {
                groupName = selectedItem.groupName,
                groupID = selectedItem.groupID
            };

            //Add Data to Storage/Cache
            var json = JsonSerializer.Serialize<GroupListModel>(group);
            if (!Application.Current.Properties.ContainsKey("CurrentGroup"))
            {

                Application.Current.Properties.Add("CurrentGroup", json);

            }
            else
            {
                Application.Current.Properties["CurrentGroup"] = json;
            }

            //Navigate to Modal page
            await Navigation.PushModalAsync(new ChatPage());

        }

        private void OnDelete(object sender, EventArgs e)
        {

        }

    }
}