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
	public partial class NewQuestion : ContentPage
	{
        public static HttpClient _client = new HttpClient();

        public bool QuestionPosted = false;
        public string message;

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Title = "New Question";
        }

        public NewQuestion ()
		{
			InitializeComponent ();
		}

        private void SubmitButtonColorReset()
        {
            submitButton.BackgroundColor = Color.FromHex("#ffc400");
            submitButton.TextColor = Color.FromHex("#ffffff");
            submitButton.Text = "SUBMIT";
        }

        private async void submitButton_Clicked(object sender, EventArgs e)
        {
            submitButton.BackgroundColor = Color.FromHex("#c79400");
            submitButton.TextColor = Color.FromHex("#000000");
            submitButton.Text = "Posting...";

            //await DisplayAlert("Posted", "Posting", "ok");
            if (!string.IsNullOrEmpty(question_description.Text) && !string.IsNullOrEmpty(question_tags.Text) && !string.IsNullOrEmpty(question_title.Text))
            {

                if (models.post.so_username != "None" && models.post.so_password != "None"
                    && models.post.reddit_username != "None" && models.post.reddit_password != "None")
                {
                    try
                    {
                        await Task.Run(() => QuestionPoster());
                    }
                    catch (Exception NextException)
                    {
                        SubmitButtonColorReset();
                        await DisplayAlert("NextException", NextException.ToString(), "ok");
                        //Console.WriteLine("NextException : " + NextException);
                        throw;
                    }
                    if (QuestionPosted)
                    {
                        SubmitButtonColorReset();
                        await DisplayAlert("Success!", "Your Question Was Posted!", "ok");
                        question_title.Text = "";
                        question_description.Text = "";
                        question_tags.Text = "";
                        base.OnBackButtonPressed();
                    }
                    else
                    {
                        SubmitButtonColorReset();
                        await DisplayAlert("Failure!", message, "ok");
                    }
                }
                else
                {
                    SubmitButtonColorReset();
                    await DisplayAlert("Failure!", "Make sure you have saved your credentials.", "ok");
                }

            }
            else
            {
                SubmitButtonColorReset();
                await DisplayAlert("Empty Fields", "Make Sure The Fields Are Filled.", "Ok");
            }
        }

        private async Task QuestionPoster()
        {
            var json_string = new StringContent("{\"type\":\"insert\",\"args\":{\"table\":\"previous_questions\",\"objects\":[{\"q_desc\":\"" + question_description.Text + "\",\"q_tags\":\"" + question_tags.Text + "\",\"q_title\":\"" + question_title.Text + "\",\"q_poster\":\"" + models.post.hasura_id + "\"}]}}");

            try
            {
                Console.WriteLine("Data of Content : " + json_string);
                var result = await _client.PostAsync("https://data.fillip72.hasura-app.io/v1/query", json_string);

                string resultContent = await result.Content.ReadAsStringAsync();

                if (!string.IsNullOrEmpty(resultContent))
                {
                    dynamic obj2 = Newtonsoft.Json.Linq.JObject.Parse(resultContent.ToString());

                    if (result.IsSuccessStatusCode) // If Status Code == 200 (OK)
                    {
                        //Console.WriteLine("Obj2 : " + obj2.hasura_id);
                        QuestionPosted = true;
                        //models.post.hasura_id = Convert.ToString(obj2.hasura_id);
                        //models.post.username = Convert.ToString(obj2.username);
                    }
                    else // If Status Code != 200 (OK)
                    {
                        // Need to handle errors here. Ki bhai something terrible happened here!
                        QuestionPosted = false;
                        message = Convert.ToString(obj2.message);
                        Console.WriteLine("Error Occurred");
                    }
                }
                else
                {
                    QuestionPosted = false;
                    message = "Empty Web Page Response! Try again later.";
                    Console.WriteLine("Empty Response");
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