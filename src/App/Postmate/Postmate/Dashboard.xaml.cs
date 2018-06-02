using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
	public partial class Dashboard : TabbedPage
	{
        
        private ObservableCollection<models.previousQuestions> _previousQuestionsList_Private = new ObservableCollection<models.previousQuestions> { };
        public static HttpClient _client = new HttpClient();

        private string message;

        public bool UserInfoFetchStatus = false, QuestionsFetch = false;

        public Dashboard ()
		{
			InitializeComponent ();
            
            //_client.BaseAddress = new System.Uri("https://data.fillip72.hasura-app.io/");
            //_client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));//ACCEPT header

        }

        protected override async void OnAppearing()
        {
            previousQuestions_ListView.IsRefreshing = true;
            await previousQuestionsFetching();
            previousQuestions_ListView.ItemsSource = _previousQuestionsList_Private;
            previousQuestions_ListView.EndRefresh();
            
            // If the questions asked count is 0, i.e., no questions asked or new user, we won't show the list view.
            // Instead, we'll show them the label that reads that you haven't asked anything yet.
            if (_previousQuestionsList_Private.Count == 0)
            {
                previousQuestions_ListView.IsVisible = false;
                tempLable.IsVisible = true;
            }

        }

        private async void newQuestion_Clicked(object sender, EventArgs e)
        {
            //DisplayAlert("Register Clicked", "You Clicked Regsiter", "Ok");
            newQuestion_Button.BackgroundColor = Color.FromHex("#BCBCBC");
            newQuestion_Button.TextColor = Color.FromHex("#000000");
            newQuestion_Button.Text = "X";

            await Navigation.PushAsync(new NewQuestion());

            newQuestion_Button.BackgroundColor = Color.FromHex("#000000");
            newQuestion_Button.TextColor = Color.FromHex("#ffffff");
            newQuestion_Button.Text = "+";
        }

        async void previousQuestionsListView_Refresh(object sender, System.EventArgs e)
        {
            await previousQuestionsFetching();
            previousQuestions_ListView.ItemsSource = _previousQuestionsList_Private;
            previousQuestions_ListView.EndRefresh();
        }

        private async void previousQuestions_ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem == null)
                return;

            var selectedRequest = e.SelectedItem as models.previousQuestions;
            await Navigation.PushAsync(new QuestionInformation(selectedRequest.question_id, selectedRequest.question_title, selectedRequest.question_tags));

            previousQuestions_ListView.SelectedItem = null;
        }

        public async Task previousQuestionsFetching()
        {
            _previousQuestionsList_Private.Clear();

            try
            {
                var json_string = new StringContent("{\"type\":\"select\",\"args\":{\"table\":\"previous_questions\",\"columns\":[\"*\"],\"order_by\":[{\"column\":\"q_post_date\"}],\"where\":{\"q_poster\":{\"$eq\":\"" + models.post.hasura_id + "\"}}}}");

                try
                {
                    //Console.WriteLine("Data of Content : " + json_string);
                    var result = await _client.PostAsync("https://data.fillip72.hasura-app.io/v1/query", json_string);

                    string resultContent = await result.Content.ReadAsStringAsync();

                    if (!string.IsNullOrEmpty(resultContent))
                    {

                        if (result.IsSuccessStatusCode) // If Status Code == 200 (OK)
                        {
                            dynamic obj2 = JsonConvert.DeserializeObject(Convert.ToString(resultContent));

                            QuestionsFetch = true;

                            if (obj2.Count <= 0)
                                return;
                            else
                            {
                                foreach (var data in obj2)
                                {
                                    _previousQuestionsList_Private.Add(new models.previousQuestions
                                    {
                                        question_id = Convert.ToString(data.q_id),
                                        question_title = Convert.ToString(data.q_title),
                                        question_posted_date = Convert.ToString(data.q_post_date),
                                        question_reply_count = "0",
                                        question_tags = Convert.ToString(data.q_tags)
                                    });
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