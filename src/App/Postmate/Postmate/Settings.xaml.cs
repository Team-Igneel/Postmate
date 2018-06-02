using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Postmate
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Settings : CarouselPage
    {
        public static HttpClient _client = new HttpClient();

        public bool SettingsSave = false;
        private string message;

        public Settings()
        {
            InitializeComponent();
            soUsername.Text = models.post.so_username;
            soPassword.Text = models.post.so_password;
            redditUsername.Text = models.post.reddit_username;
            redditPassword.Text = models.post.reddit_password;
        }

        private void SaveButtonColorReset()
        {
            SaveInfo.BackgroundColor = Color.FromHex("#ffc400");
            SaveInfo.TextColor = Color.FromHex("#ffffff");
            SaveInfo.Text = "SAVE";
        }

        private async void Save_Clicked(object sender, EventArgs e)
        {
            SaveInfo.BackgroundColor = Color.FromHex("#c79400");
            SaveInfo.TextColor = Color.FromHex("#000000");
            SaveInfo.Text = "Saving...";

            if (!string.IsNullOrEmpty(soUsername.Text) && !string.IsNullOrEmpty(soPassword.Text) && !string.IsNullOrEmpty(redditUsername.Text)
                && !string.IsNullOrEmpty(redditPassword.Text))
            {
                try
                {
                    await Task.Run(() => Save());

                }
                catch (Exception SavingException)
                {
                    SaveButtonColorReset();
                    await DisplayAlert("SavingException", SavingException.ToString(), "ok");

                    throw;
                }

                if (SettingsSave)
                {
                    SaveButtonColorReset();
                    //await Navigation.PushAsync(new Dashboard());
                    await DisplayAlert("Success!", "Saved Your Credentials!", "ok");
                }
                else
                {
                    SaveButtonColorReset();
                    await DisplayAlert("Failure!", message, "ok");
                }

            }
            else
            {
                SaveButtonColorReset();
                await DisplayAlert("Empty Fields", "Make Sure The Fields Are Filled.", "Ok");
            }

        }

        private async Task Save()
        {
            // Need to re-build the tables before working on this. So, ignore this for now.
            var json_string = new StringContent("{\"type\":\"update\",\"args\":{\"table\":\"user_info\",\"where\":{\"u_id_internal\"" +
                ":{\"$eq\":\"" + models.post.hasura_id + "\"}},\"$set\":{\"u_SO_username\":\"" + soUsername.Text + "\",\"u_SO_password\":\"" 
                + soPassword.Text + "\",\"u_reddit_username\":\"" + redditUsername.Text + 
                "\",\"u_reddit_password\":\"" + redditPassword.Text + "\"}}}");

            try
            {
                //Console.WriteLine("Data of Content : " + json_string);
                var result = await _client.PostAsync("https://data.fillip72.hasura-app.io/v1/query", json_string);

                string resultContent = await result.Content.ReadAsStringAsync();

                if (!string.IsNullOrEmpty(resultContent))
                {
                    dynamic obj2 = Newtonsoft.Json.Linq.JObject.Parse(resultContent.ToString());

                    if (result.IsSuccessStatusCode) // If Status Code == 200 (OK)
                    {
                        
                        // Successfull Entry!
                        SettingsSave = true;
                    }
                    else // If Status Code != 200 (OK)
                    {
                        // Need to handle errors here. Ki bhai something terrible happened here!
                        SettingsSave = false;
                        message = Convert.ToString(obj2.message);
                        Console.WriteLine("Error Occurred");
                    }
                }
                else
                {
                    SettingsSave = false;
                    message = "Empty Web Page Response! Try again later.";
                    //Console.WriteLine("Empty Response");
                }


            }
            catch (Exception TryDikkat)
            {
                //Console.WriteLine("TryDikkat : " + TryDikkat);
                await DisplayAlert("TryDikkat", TryDikkat.ToString(), "ok");
                throw;
            }
        }

    }
}