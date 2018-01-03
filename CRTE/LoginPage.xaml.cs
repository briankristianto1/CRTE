using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.ApplicationModel.Core;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace CRTE
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        //string base_url = "https://davidberlian.com/pemvis_wk06/index.php/api/";
        string base_url = "http://halimbrian.ga/welcome/";
        public LoginPage()
        {
            this.InitializeComponent();
        }

        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Password;

            HttpClient httpClient = new HttpClient();

            string url = base_url + "login";
            Debug.WriteLine(url);
            // request parameter
            string param = "username=" + username + "&password=" + password;
            Debug.WriteLine(param);
            // use httpClient.GetAsync() for GET method
            // use httpClient.PostAsync() for POST method
            HttpResponseMessage response = await httpClient.PostAsync(url, new StringContent(param, Encoding.UTF8, "application/x-www-form-urlencoded"));

            // get response text as string
            string responseText = await response.Content.ReadAsStringAsync();
            Debug.WriteLine(responseText);
            JsonArray jsonArray = JsonValue.Parse(responseText).GetArray();
            string jsonString = jsonArray[0].ToString();

            // convert string json to Object User using DeserializeObject
            var res = JsonConvert.DeserializeObject<User>(jsonString);

            string message;
            if (res.success == "true")
            {
                //message = "success!";
                message = res.data.message;
                MessageDialog dialog = new MessageDialog(message);
                await dialog.ShowAsync();
                // tokennya ga perlu dioper kan?
                if(res.data.message == "login success")
                {
                    Dictionary<string, string> newDictionary = new Dictionary<string, string>();
                    newDictionary.Add("username", username);
                    newDictionary.Add("publicip", res.data.publicip);
                    this.Frame.Navigate(typeof(MainPage), newDictionary);
                }
            }
            else
            {
                //message = "fail!";
                message = res.data.message;
                MessageDialog dialog = new MessageDialog(message);
                await dialog.ShowAsync();
            }

          
        }

        private async void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            CoreApplicationView newView = CoreApplication.CreateNewView();
            int newViewId = 0;
            await newView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var frame = new Frame();
                frame.Navigate(typeof(RegisterPage));
                Window.Current.Content = frame;
                Window.Current.Activate();
                newViewId = ApplicationView.GetForCurrentView().Id;
            });
            bool success = await ApplicationViewSwitcher.TryShowAsStandaloneAsync(newViewId);
        }

    }
}
