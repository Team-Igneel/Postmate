using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Postmate
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class HomeScreen : ContentPage
	{
		public HomeScreen ()
		{
			InitializeComponent ();
            //var myEmoji = "\U0001f627\U0001f62e\U0001f62f";
            
            Tagline_Label.Text = "Made With 💗 By Igneel";
        }

        private void newUser_Button_Clicked(object sender, EventArgs e)
        {
            newUser_Button.BackgroundColor = Color.FromHex("#bcbcbc");
            newUser_Button.TextColor = Color.FromHex("#000000");

            Navigation.PushAsync(new Register_initial());

            newUser_Button.BackgroundColor = Color.FromHex("#000000");
            newUser_Button.TextColor = Color.FromHex("#ffffff");
        }

        private void existingUser_Button_Clicked(object sender, EventArgs e)
        {
            existingUser_Button.BackgroundColor = Color.FromHex("#bcbcbc");
            existingUser_Button.TextColor = Color.FromHex("#000000");

            Navigation.PushAsync(new LoginPage());

            existingUser_Button.BackgroundColor = Color.FromHex("#000000");
            existingUser_Button.TextColor = Color.FromHex("#ffffff");
        }
    }
}