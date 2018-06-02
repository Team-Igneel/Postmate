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
	public partial class QuestionInformation : ContentPage
	{
        public static HttpClient _client = new HttpClient();
        public bool QuestionsFetch = false;
        private string q_id, message;

        protected override async void OnAppearing()
        {
            if (!QuestionsFetch)
                await questionDescriptionFetcher();
        }

        public QuestionInformation (string q_id, string q_title, string q_tags)
		{
			InitializeComponent ();
            
            this.q_id = q_id;
            question_title.Text = q_title;
            question_tags.Text = q_tags;
        }

        public async Task questionDescriptionFetcher()
        {
            try
            {
                var json_string = new StringContent("{\"type\":\"select\",\"args\":{\"table\":\"previous_questions\",\"columns\":[\"q_desc\"],\"limit\":\"1\",\"where\":{\"q_id\":{\"$eq\":\"" + this.q_id + "\"}}}}");

                try
                {
                    //Console.WriteLine("Data of Content : " + json_string);
                    var result = await _client.PostAsync("https://data.fillip72.hasura-app.io" + "/v1/query", json_string);

                    string resultContent = await result.Content.ReadAsStringAsync();
                    
                    if (!string.IsNullOrEmpty(resultContent))
                    {

                        if (result.IsSuccessStatusCode) // If Status Code == 200 (OK)
                        {
                            dynamic obj2 = JsonConvert.DeserializeObject(Convert.ToString(resultContent));

                            QuestionsFetch = true;

                            if (obj2.Count <= 0)
                            {
                                question_description.Text = "Could Not Load The Question Details. Try Again Later.";
                                question_description.FontAttributes = FontAttributes.Bold;
                            }
                            else
                            {
                                foreach (var data in obj2)
                                {
                                    question_description.Text = Convert.ToString(data.q_desc);
                                }
                            }

                        }
                        else // If Status Code != 200 (OK)
                        {
                            dynamic obj2 = Newtonsoft.Json.Linq.JObject.Parse(resultContent.ToString());

                            // Need to handle errors here. Ki bhai something terrible happened here!
                            QuestionsFetch = false;
                            message = Convert.ToString(obj2.error);
                            Console.WriteLine("Error Occurred");
                        }
                    }
                    else
                    {
                        QuestionsFetch = false;
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
            catch (Exception)
            {
                await DisplayAlert("Failed", "Could Not Connect To Servers.", "Ok");
            }

        }
    }
}