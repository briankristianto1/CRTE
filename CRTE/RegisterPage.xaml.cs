using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
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
    public sealed partial class RegisterPage : Page
    {
        string base_url = "http://halimbrian.ga/welcome/";

        public RegisterPage()
        {
            this.InitializeComponent();
        }

        private async void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Password;
            string email = txtEmail.Text;

            HttpClient httpClient = new HttpClient();

            string url = base_url + "register";

            // request parameter
            string param = "username=" + username + "&password=" + password + "&email=" + email;

            // use httpClient.GetAsync() for GET method
            // use httpClient.PostAsync() for POST method
            HttpResponseMessage response = await httpClient.PostAsync(url, new StringContent(param, Encoding.UTF8, "application/x-www-form-urlencoded"));

            // get response text as string
            string responseText = await response.Content.ReadAsStringAsync();
            JsonArray jsonArray = JsonValue.Parse(responseText).GetArray();
            string jsonString = jsonArray[0].ToString();

            // convert string json to Object User using DeserializeObject
            var res = JsonConvert.DeserializeObject<User>(jsonString);

            string message;
            if (res.success == "true")
            {
                message = "success! ";
            }
            else
            {
                message = "fail! ";
            }

            message += res.data.message;
            MessageDialog dialog = new MessageDialog(message);
            await dialog.ShowAsync();
            Window.Current.Close();
        }
    }
}
