using Newtonsoft.Json;
using RealtimeFramework.Messaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Networking.Connectivity;
using Windows.Networking.Sockets;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace CRTE
{

    public class Message
    {
        public string id { get; set; }
        public string text { get; set; }
        public string sentAt { get; set; }
        public string sentAtDate { get; set; }
    }

    public class Code
    {
        public string id { get; set; }
        public string data { get; set; }
        public string sentAt { get; set; }
        public string sentAtDate { get; set; }
    }

    public sealed partial class MainPage : Page
    {
        private OrtcClient ortcClient;
        private string myID = "";
        private string username = "";
        private string chatcode = "chat";
        private string colcode = "chatcoll";
        string base_url = "http://halimbrian.ga/welcome/";
        private UserList lists = new UserList();

        public MainPage()
        {
            this.InitializeComponent();
            ChatCodeDialog();
            userlist.DataContext = lists;
        }

        private async void ChatCodeDialog()
        {
            var dialog1 = new ContentDialog1();
            var result = await dialog1.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var text = dialog1.Text;
                if (!string.IsNullOrWhiteSpace(text))
                {
                    chatcode = text;
                    webView1.Source = new Uri("https://www.firecode.io/sharepads/share?room=" + chatcode);
                    colcode = text + "coll";
                    lists.UserLists.Add("Connecting...");

                    dynamic res =
                        await RequestFromAPI("addOnlineUser", "username=" + username + "&channel=" + chatcode);
                    Debug.WriteLine((string)res.message);

                    ConnectToORTC();
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                        () =>
                        {
                            DispatcherTimer dTimer = new DispatcherTimer();
                            dTimer.Tick += RefreshOnlineUsers;
                            dTimer.Interval = TimeSpan.FromSeconds(7);
                            dTimer.Start();
                        });
                }
            }
        }


        void ConnectToORTC()
        {
            // Establish the Realtime connection
            ortcClient = new OrtcClient();
            ortcClient.OnConnected += OnConnected;
            ortcClient.OnException += OnException;

            ortcClient.ClusterUrl = "http://ortc-developers.realtime.co/server/2.1/";
            //ortcClient.Connect("2Ze1dz", "token");
            ortcClient.Connect("jXNrX9", "token");
            Message message = new Message();
            message.sentAtDate = DateTime.Now.ToLocalTime().ToString("dd/MM/yyyy");
            if (message.sentAtDate != null)
            {
                TxtChat.Text = message.sentAtDate;
                TxtSend.IsEnabled = true;
            }


        }

        private async void RefreshOnlineUsers(object sender, object e)
        {
            try
            {
                dynamic res =
                await RequestFromAPI("getOnlineUser", "channel=" + chatcode);
                lists.UserLists.Clear();
                string rawres = (string)res.users;
                string[] splitedres = rawres.Split(',');
                foreach (string s in splitedres)
                {
                    lists.UserLists.Add(s);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

        }

        async Task<dynamic> RequestFromAPI(string cmds, string param)
        {
            HttpClient httpClient = new HttpClient();

            string url = base_url + cmds;
            Debug.WriteLine(url);
            // request parameter
            Debug.WriteLine(param);
            // use httpClient.GetAsync() for GET method
            // use httpClient.PostAsync() for POST method
            HttpResponseMessage response = await httpClient.PostAsync(url, new StringContent(param, Encoding.UTF8, "application/x-www-form-urlencoded"));

            // get response text as string
            string responseText = await response.Content.ReadAsStringAsync();
            Debug.WriteLine(responseText);
            return JsonConvert.DeserializeObject(responseText);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            username = e.Parameter as string;
            //Dictionary<string, string> myDictionary = new Dictionary<string, string>();
            //myDictionary = e.Parameter as Dictionary<string, string>;
            //myID = myDictionary["username"].ToString();
            myID = username;
            Debug.WriteLine(myID);

        }


        void OnConnected(object sender)
        {
            // Subscribe the Realtime channel
            ortcClient.Subscribe(chatcode, true, OnMessageCallback);
            Debug.WriteLine(chatcode);
        }

        void OnException(object sender, Exception ex)
        {
            //Application.Current.Exit();
        }

        private void OnMessageCallback(object sender, string channel, string message)
        {
            Debug.WriteLine("Received message: " + message);

            Message parsedMessage = JsonConvert.DeserializeObject<Message>(message);
            TxtChat.Text += "\n" + parsedMessage.id + " (" + parsedMessage.sentAt + "): " + parsedMessage.text;


            // check if message is from another user
            if (!parsedMessage.id.Equals(myID))
            {
                // Say the message
            }
        }

        private void SendMessage(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {

                string spokenMessage = "";

                spokenMessage = TxtSend.Text;

                if (!string.IsNullOrEmpty(spokenMessage))
                {
                    // Send the recognition result text as a Realtime message
                    Message message = new Message();
                    message.id = myID;
                    message.text = spokenMessage;
                    message.sentAt = DateTime.Now.ToLocalTime().ToString("HH:mm");

                    string jsonMessage = JsonConvert.SerializeObject(message);
                    Debug.WriteLine("Sending message: " + jsonMessage);
                    ortcClient.Send(chatcode, jsonMessage);
                    TxtSend.Text = "";

                    MyScrollViewer.UpdateLayout();
                    //MyScrollViewer.ChangeView(0.0f, double.MaxValue, 1.0f);
                    MyScrollViewer.ScrollToVerticalOffset(MyScrollViewer.ScrollableHeight);
                }
            }
        }

        private async void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            dynamic res = await RequestFromAPI("deleteOnlineUser", "username=" + username + "&channel=" + chatcode);
            if (res.message == "remove success")
            {
                this.Frame.Navigate(typeof(LoginPage));
            }
        }
    }
}