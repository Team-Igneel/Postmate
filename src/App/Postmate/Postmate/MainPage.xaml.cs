using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Net;


namespace Postmate
{
    public partial class MainPage : CarouselPage
    {
        public static HttpClient _client = new HttpClient();

        private string message;

        public bool SignInStatus = false, SignUpFirstStage=false, ClusterSleeping = false;

        public MainPage()
        {
            InitializeComponent();
            
            //userEmail_Login.Text = "Xonshiz";
            //userPassword_Login.Text = "12345678";
        }

        private async void loginButton_Clicked(object sender, EventArgs e)
        {

            if (!string.IsNullOrEmpty(userEmail_Login.Text) && !string.IsNullOrEmpty(userPassword_Login.Text))
            {
                try
                {
                    await Task.Run(() => LogInPoster());
                    //await Navigation.PushAsync(new Dashboard());
                }
                catch (Exception LoginException)
                {
                    await DisplayAlert("LoginException", LoginException.ToString(), "ok");
                    //Console.WriteLine("NextException : " + NextException);
                    throw;
                }

                // After loggin in, we will fetch the account details of the user.

                try
                {
                    await Task.Run(() => UserInfoFetcher());
                }
                catch (Exception UserInfoFetchException)
                {
                    //await DisplayAlert("UserInfoFetchException", UserInfoFetchException.ToString(), "ok");
                    //throw;
                    await DisplayAlert("Server Sleeping", "Seems like our servers are sleeping. We will open a link and you should see when the server is up (generally takes 2-3 minutes. Try after that.", "Ok");
                    Device.OpenUri(new Uri("http://auth.fillip72.hasura-app.io/ui"));
                }

                // If cluster is sleeping and we get an WebException, we gotta wake the cluster up.
                if (ClusterSleeping)
                {
                    await DisplayAlert("Server Sleeping", "Seems like our servers are sleeping. We will open a link and you should see when the server is up (generally takes 2-3 minutes. Try after that.", "Ok");
                    //Device.OpenUri(new Uri("http://auth.fillip72.hasura-app.io/ui"));
                    await Task.Run(() => { Device.OpenUri(new Uri("http://auth.fillip72.hasura-app.io/ui")); });
                    return;
                }

                if (SignInStatus)
                {
                    await Navigation.PushAsync(new Dashboard());
                }
                else
                {
                    await DisplayAlert("Failure!", message, "ok");
                }

            }
            else
            {
                await DisplayAlert("Empty Fields", "Make Sure The Fields Are Filled.", "Ok");
            }

        }

        private async Task LogInPoster()
        {
            var json_string1 = new StringContent("{\"provider\":\"username\",\"data\":{\"username\":\"" + userEmail_Login.Text + "\",\"password\":\"" + userPassword_Login.Text + "\"}}");

            try
            {

                var result = await _client.PostAsync("https://auth.fillip72.hasura-app.io/v1/login", json_string1);

                string resultContent = await result.Content.ReadAsStringAsync();

                if (!string.IsNullOrEmpty(resultContent))
                {
                    dynamic obj2 = Newtonsoft.Json.Linq.JObject.Parse(resultContent.ToString());

                    if (result.IsSuccessStatusCode) // If Status Code == 200 (OK)
                    {

                        SignInStatus = true;
                        models.post.hasura_id = Convert.ToString(obj2.hasura_id);
                        models.post.username = Convert.ToString(obj2.username);
                        //Console.WriteLine("models.post.hasura_id Main Login : " + models.post.hasura_id);
                    }
                    else // If Status Code != 200 (OK)
                    {
                        // Need to handle errors here. Ki bhai something terrible happened here!
                        SignInStatus = false;
                        message = Convert.ToString(obj2.message);

                    }
                }
                else
                {
                    SignInStatus = false;
                    message = "Empty Web Page Response! Try again later.";

                }


            }
            catch (Exception LoginEr)
            {
                ClusterSleeping = true;
                //await DisplayAlert("TryDikkat", LoginEr.ToString(), "ok");
                //throw;
            }
        }

        private async Task UserInfoFetcher()
        {
            var json_string1 = new StringContent("{\"type\":\"select\",\"args\":{\"table\":\"user_info\",\"columns\":[\"*\"],\"where\":{\"u_id_internal\":{\"$eq\":\"" + models.post.hasura_id + "\"}}}}");

            try
            {

                var result = await _client.PostAsync("https://data.fillip72.hasura-app.io/v1/query", json_string1);

                string resultContent = await result.Content.ReadAsStringAsync();

                if (!string.IsNullOrEmpty(resultContent))
                {
                    dynamic obj2 = JsonConvert.DeserializeObject(Convert.ToString(resultContent));

                    if (result.IsSuccessStatusCode) // If Status Code == 200 (OK)
                    {
                        SignInStatus = true;

                        if (obj2.Count <= 0)
                        {
                            SignInStatus = false;
                            return;
                        }

                        foreach (var data in obj2)
                        {
                            models.post.hasura_id = Convert.ToString(data.u_id_internal);
                            models.post.username = Convert.ToString(data.u_username);
                            models.post.so_username = Convert.ToString(data.u_SO_username);
                            models.post.so_password = Convert.ToString(data.u_SO_password);
                            models.post.reddit_username = Convert.ToString(data.u_reddit_username);
                            models.post.reddit_password = Convert.ToString(data.u_reddit_password);
                            models.post.u_age = Convert.ToString(data.u_age);
                            models.post.u_email = Convert.ToString(data.u_email);
                            models.post.u_name = Convert.ToString(data.u_name);
                            models.post.u_phone = Convert.ToString(data.u_phone);

                            //Console.WriteLine("models.post.hasura_id Not Main : " + models.post.hasura_id);
                            //Console.WriteLine("Convert.ToString(data.u_id_internal) Not Main : " + Convert.ToString(data.u_id_internal));

                            //Console.WriteLine("models.post.so_username : " + models.post.so_username);
                            //Console.WriteLine("models.post.u_SO_username : " + Convert.ToString(data.u_SO_username));
                        }

                        // Just to make sure any earlier exception would reset this.
                        ClusterSleeping = false;
                    }
                    else // If Status Code != 200 (OK)
                    {
                        // Need to handle errors here. Ki bhai something terrible happened here!
                        SignInStatus = false;
                        message = Convert.ToString(obj2.message);

                    }
                }
                else
                {
                    SignInStatus = false;
                    message = "Empty Web Page Response! Try again later.";

                }


            }
            catch (Exception LoginEr)
            {
                //await DisplayAlert("TryDikkat", LoginEr.ToString(), "ok");
                //ClusterSleeping = true;
                throw;
            }
        }

        private async void nextButton_Clicked(object sender, EventArgs e)
        {
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
                        await DisplayAlert("NextException", NextException.ToString(), "ok");
                        //Console.WriteLine("NextException : " + NextException);
                        throw;
                    }
                    if (SignUpFirstStage)
                    {
                        await Navigation.PushAsync(new Register());
                    }
                    else
                    {
                        await DisplayAlert("Failure!", message, "ok");
                    }
                    
                }
                else
                {
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


