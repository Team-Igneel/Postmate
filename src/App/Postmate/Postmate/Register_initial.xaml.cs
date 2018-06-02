using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Postmate
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Register_initial : ContentPage
	{
        public static HttpClient _client = new HttpClient();

        //private bool LogInStatus = false, ClusterSleeping = false;
        private string message;

        public bool  SignUpFirstStage = false, ClusterSleeping = false;

        public Register_initial ()
		{
			InitializeComponent ();

            var login_Label_Tap = new TapGestureRecognizer();

            login_Label_Tap.Tapped += (s, e) =>
            {
                Navigation.PushAsync(new LoginPage());
            };
            login_Label.GestureRecognizers.Add(login_Label_Tap);
        }

        private void NextButtonColorReset()
        {
            nextButton.BackgroundColor = Color.FromHex("#000000");
            nextButton.TextColor = Color.FromHex("#ffffff");
            nextButton.Text = "NEXT";
        }

        private async void nextButton_Clicked(object sender, EventArgs e)
        {
            nextButton.BackgroundColor = Color.FromHex("#bcbcbc");
            nextButton.TextColor = Color.FromHex("#000000");
            nextButton.Text = "Please Wait...";

            //await Navigation.PushAsync(new MainScreen());
            //If both the fields aren't empty, perform login.
            if (!string.IsNullOrEmpty(username_register.Text) && !string.IsNullOrEmpty(password_register.Text))
            {

                try
                {
                    await Task.Run(() => NextPoster());
                }
                catch (Exception NextException)
                {
                    NextButtonColorReset();
                    await DisplayAlert("NextException", NextException.ToString(), "ok");
                    await DisplayAlert("Server Sleeping", "Seems like our servers are sleeping. We will open a link and you should see when the server is up (generally takes 2-3 minutes). Try after that.", "Ok");
                    //Device.OpenUri(new Uri("http://auth.fillip72.hasura-app.io/ui"));
                    await Task.Run(() => { Device.OpenUri(new Uri("http://auth.fillip72.hasura-app.io/ui")); });
                    return;
                    //Console.WriteLine("NextException : " + NextException);
                    throw;
                }
                if (SignUpFirstStage)
                {
                    await Navigation.PushAsync(new Register());
                    NextButtonColorReset();
                }
                else
                {
                    NextButtonColorReset();
                    await DisplayAlert("Failure!", message, "ok");
                }

            }
            else
            {
                NextButtonColorReset();
                await DisplayAlert("Empty Fields", "Make Sure The Fields Are Filled.", "Ok");
            }
        }

        private async Task NextPoster()
        {
            var json_string = new StringContent("{\"provider\":\"username\",\"data\":{\"username\":\"" + username_register.Text + "\",\"password\":\"" + password_register.Text + "\"}}");

            try
            {
                Console.WriteLine("Data of Content : " + json_string);
                var result = await _client.PostAsync("https://auth.fillip72.hasura-app.io/v1/signup", json_string);

                string resultContent = await result.Content.ReadAsStringAsync();

                if (!string.IsNullOrEmpty(resultContent))
                {
                    dynamic obj2 = Newtonsoft.Json.Linq.JObject.Parse(resultContent.ToString());

                    if (result.IsSuccessStatusCode) // If Status Code == 200 (OK)
                    {
                        //Console.WriteLine("Obj2 : " + obj2.hasura_id);
                        SignUpFirstStage = true;
                        models.post.hasura_id = Convert.ToString(obj2.hasura_id);
                        models.post.username = Convert.ToString(obj2.username);
                        // Just to make sure any earlier exception would reset this.
                        ClusterSleeping = false;
                    }
                    else // If Status Code != 200 (OK)
                    {
                        // Need to handle errors here. Ki bhai something terrible happened here!
                        SignUpFirstStage = false;
                        message = Convert.ToString(obj2.message);
                        Console.WriteLine("Error Occurred");
                    }
                }
                else
                {
                    SignUpFirstStage = false;
                    message = "Empty Web Page Response! Try again later.";
                    Console.WriteLine("Empty Response");
                }


            }
            catch (WebException WE)
            {
                //await DisplayAlert("Server Sleeping", "Seems like our servers are sleeping. We will open a link and you should see when the server is up (generally takes 2-3 minutes. Try after that.", "Ok");
                ////Device.OpenUri(new Uri("http://auth.fillip72.hasura-app.io/ui"));
                //await Task.Run(() => { Device.OpenUri(new Uri("http://auth.fillip72.hasura-app.io/ui")); });
                ClusterSleeping = true;
            }
            catch (Exception TryDikkat)
            {
                //Console.WriteLine("TryDikkat : " + TryDikkat);
                await DisplayAlert("TryDikkat", TryDikkat.ToString(), "ok");
                throw;
                //ClusterSleeping = true;
            }

        }
    }
}