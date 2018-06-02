using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Postmate
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Register : ContentPage
    {       
        public static HttpClient _client = new HttpClient();

        public bool SignUpSecondStage = false;
        private string message;

        public Register ()
		{
			InitializeComponent ();
            _client.BaseAddress = new System.Uri("https://data.fillip72.hasura-app.io");
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));//ACCEPT header
        }

        private void FinishButtonColorReset()
        {
            registerButton.BackgroundColor = Color.FromHex("#000000");
            registerButton.TextColor = Color.FromHex("#ffffff");
            registerButton.Text = "FINISH";
        }

        private async void registerButton_Clicked(object sender, EventArgs e)
        {
            registerButton.BackgroundColor = Color.FromHex("#bcbcbc");
            registerButton.TextColor = Color.FromHex("#000000");
            registerButton.Text = "Finishing Up...";

            //DisplayAlert("Register Clicked", "You Clicked Regsiter", "Ok");
            //If both the fields aren't empty, perform login.
            if (!string.IsNullOrEmpty(name_register.Text) && !string.IsNullOrEmpty(age_register.Text)
                      && (!string.IsNullOrEmpty(userEmail_Sign.Text) &&  !string.IsNullOrEmpty(phone_register.Text))
                     )
                {
                    //await DisplayAlert("Registered", "Successfuly Registered, Select \"ok\" to continue.", "Ok");
                    //await Navigation.PushAsync(new Dashboard());
                    try
                    {
                        await Task.Run(() => NextPoster());
                    }
                    catch (Exception NextException)
                    {
                        FinishButtonColorReset();
                        await DisplayAlert("NextException", NextException.ToString(), "ok");
                        //Console.WriteLine("NextException : " + NextException);
                        throw;
                    }
                    if (SignUpSecondStage)
                    {
                        await Navigation.PushAsync(new Dashboard());
                        FinishButtonColorReset();
                }
                    else
                    {
                        FinishButtonColorReset();
                        await DisplayAlert("Failure!", message, "ok");
                    }

                }
                else
                {
                    FinishButtonColorReset();
                    await DisplayAlert("Empty Fields", "Make Sure The Fields Are Filled.", "Ok");
                }
            
        }

        private async Task NextPoster()
        {
            var json_string = new StringContent("{\"type\":\"insert\",\"args\":{\"table\":\"user_info\",\"objects\":[{\"u_name\":\"" + name_register.Text + "\",\"u_username\":\"" + models.post.username + "\",\"u_age\":\"" + age_register.Text + "\",\"u_email\":\"" + userEmail_Sign.Text + "\",\"u_phone\":\"" + phone_register.Text + "\",\"u_id_internal\":\"" + models.post.hasura_id + "\"}]}}");

            try
            {
                //Console.WriteLine("Data of Content : " + json_string);
                var result = await _client.PostAsync("/v1/query", json_string);

                string resultContent = await result.Content.ReadAsStringAsync();

                if (!string.IsNullOrEmpty(resultContent))
                {
                    dynamic obj2 = Newtonsoft.Json.Linq.JObject.Parse(resultContent.ToString());

                    if (result.IsSuccessStatusCode) // If Status Code == 200 (OK)
                    {
                        //Console.WriteLine("Obj2 : " + obj2.hasura_id);
                        // Successfull Entry!
                        SignUpSecondStage = true;
                    }
                    else // If Status Code != 200 (OK)
                    {
                        // Need to handle errors here. Ki bhai something terrible happened here!
                        SignUpSecondStage = false;
                        message = Convert.ToString(obj2.message);
                        Console.WriteLine("Error Occurred");
                    }
                }
                else
                {
                    SignUpSecondStage = false;
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