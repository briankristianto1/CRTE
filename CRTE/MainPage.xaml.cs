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
using System.Threading.Tasks;
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

    public sealed partial class MainPage : Page
    {
        private OrtcClient ortcClient;
        private string myID = "";
        private string username = "";

        public MainPage()
        {
            this.InitializeComponent();

            // Establish the Realtime connection
            ortcClient = new RealtimeFramework.Messaging.OrtcClient();
            ortcClient.OnConnected += OnConnected;
            ortcClient.OnException += OnException;

            ortcClient.ClusterUrl = "http://ortc-developers.realtime.co/server/2.1/";
            ortcClient.Connect("2Ze1dz", "token");
            // ortcClient.Connect("jXNrX9", "token");
            Message message = new Message();
            message.sentAtDate = DateTime.Now.ToLocalTime().ToString("dd/MM/yyyy");
            TxtChat.Text = message.sentAtDate;
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
            ortcClient.Subscribe("chat", true, OnMessageCallback);
        }

        void OnException(object sender, Exception ex)
        {
            
        }

        private void OnMessageCallback(object sender, string channel, string message)
        {
            Debug.WriteLine("Received message: " + message);

            Message parsedMessage = JsonConvert.DeserializeObject<Message>(message);
            TxtChat.Text = TxtChat.Text + "\n" + parsedMessage.id + ": " + parsedMessage.text + "\n(" + parsedMessage.sentAt + ")\n";

            // check if message is from another user
            if (!parsedMessage.id.Equals(myID))
            {
                // Say the message
            }
        }



        private async void SendMessage(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {

                string spokenMessage = "";

                spokenMessage = TxtSend.Text;

                if (spokenMessage != "")
                {
                    // Send the recognition result text as a Realtime message
                    Message message = new Message();
                    message.id = myID;
                    message.text = spokenMessage;
                    message.sentAt = DateTime.Now.ToLocalTime().ToString("HH:mm");

                    string jsonMessage = JsonConvert.SerializeObject(message);
                    Debug.WriteLine("Sending message: " + jsonMessage);
                    ortcClient.Send("chat", jsonMessage);
                    TxtSend.Text = "";
                }
            }
        }
    }
}
/*


/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainPage : Page
    {
        string ipServer = "";
        string portServer = "8888";
        string username = "";
        string publicip = "";
        
        public MainPage()
        {
            this.InitializeComponent();
           // Debug.WriteLine(username);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //username = e.Parameter as string;
            Dictionary<string, string> myDictionary = new Dictionary<string, string>();
            myDictionary = e.Parameter as Dictionary<string, string>;
            username = myDictionary["username"].ToString();
            Debug.WriteLine(username);
            publicip = myDictionary["publicip"].ToString();
            Debug.WriteLine(publicip);
        }

        private string GetLocalIp()
        {
            var icp = NetworkInformation.GetInternetConnectionProfile();
            if (icp?.NetworkAdapter == null) return null;
            var hostname = NetworkInformation.GetHostNames().SingleOrDefault(hn =>
           hn.IPInformation?.NetworkAdapter != null && hn.IPInformation.NetworkAdapter.NetworkAdapterId == icp.NetworkAdapter.NetworkAdapterId);
            //the ip address
            return hostname?.CanonicalName;
        }

        private async void TextingChat(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                // the user has pressed enter, do something here
                try
                {
                    Debug.WriteLine("[SENDING] Create StreamSocket and Establish Connection...");
                    Windows.Networking.Sockets.StreamSocket socket = new Windows.Networking.Sockets.StreamSocket();

                    Windows.Networking.HostName serverHost = new Windows.Networking.HostName(ipServer);

                    Debug.WriteLine("[SENDING] Prepare to Connect...");
                    string serverPort = portServer;
                    Debug.WriteLine("[SENDING] Connecting...");
                    await socket.ConnectAsync(serverHost, serverPort);
                    Debug.WriteLine("[SENDING] Connected");

                    Debug.WriteLine("[SENDING] Prepare Output Data...");
                    Stream streamOut = socket.OutputStream.AsStreamForWrite();
                    StreamWriter writer = new StreamWriter(streamOut);
                    string request = username+"#CHAT#"+TxtSend.Text;
                    await writer.WriteLineAsync(request);
                    await writer.FlushAsync();
                    Debug.WriteLine("[SENDING] Read Feedback from server...");
                    Stream streamIn = socket.InputStream.AsStreamForRead();
                    StreamReader reader = new StreamReader(streamIn);

                    char[] result = new char[2048];
                    await reader.ReadAsync(result, 0, 2048);
                    string s = new string(result);
                    Debug.WriteLine("[Sending] Done Data From server..." + s);
                }
                catch (Exception x)
                {

                }
            }
        }
        private async Task StartListeningAsync()
        {
            try
            {
                Debug.WriteLine("[LISTENING] Creating Stream Socket...");
                StreamSocketListener socketListener = new StreamSocketListener();

                socketListener.ConnectionReceived += SocketListener_ConnectionReceived;
                Debug.WriteLine("[LISTENING] Create Event Handler...");

                await socketListener.BindServiceNameAsync(portServer);
                Debug.WriteLine("[LISTENING] Start Listening");
            }
            catch (Exception e)
            {
                //hand
            }
        }
        private async void SocketListener_ConnectionReceived(StreamSocketListener sender,
            StreamSocketListenerConnectionReceivedEventArgs args)
        {
            Debug.WriteLine("[LISTENING] Incomming Connection...");
            Stream inStream = args.Socket.InputStream.AsStreamForRead();
            StreamReader reader = new StreamReader(inStream);
            string request = await reader.ReadLineAsync();
            Debug.WriteLine("[LISTENING] Incomming data is " + request);

            string TxtData="";

            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                TxtData += "From IP " + args.Socket.Information.RemoteAddress + " :\r\n " + request + "\r\n";
                TxtSend.Text = "";
            });

            Debug.WriteLine("[LISTENING] Sending Feed Back");
            Stream outStream = args.Socket.OutputStream.AsStreamForWrite();
            StreamWriter writer = new StreamWriter(outStream);
            char[] bytes = null;
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                request = (TxtData);
                bytes = request.ToCharArray();
            });
            await writer.WriteAsync(bytes, 0, bytes.Length);
            await writer.FlushAsync();
            Debug.WriteLine("[LISTENING] Done..." + bytes.Length);
        }

        private async void TextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                Debug.WriteLine("[SENDING] Create StreamSocket and Establish Connection...");
                Windows.Networking.Sockets.StreamSocket socket = new Windows.Networking.Sockets.StreamSocket();

                Windows.Networking.HostName serverHost = new Windows.Networking.HostName(ipServer);

                Debug.WriteLine("[SENDING] Prepare to Connect...");
                string serverPort = portServer;
                Debug.WriteLine("[SENDING] Connecting...");
                await socket.ConnectAsync(serverHost, serverPort);
                Debug.WriteLine("[SENDING] Connected");

                Debug.WriteLine("[SENDING] Prepare Output Data...");
                Stream streamOut = socket.OutputStream.AsStreamForWrite();
                StreamWriter writer = new StreamWriter(streamOut);
                string request = username + "#DATA#" + TxtCoop.Text;
                await writer.WriteLineAsync(request);
                await writer.FlushAsync();
                Debug.WriteLine("[SENDING] Read Feedback from server...");
                Stream streamIn = socket.InputStream.AsStreamForRead();
                StreamReader reader = new StreamReader(streamIn);

                char[] result = new char[2048];
                await reader.ReadAsync(result, 0, 2048);
                string s = new string(result);
                Debug.WriteLine("[Sending] Done Data From server..." + s);
            }
            catch (Exception x)
            {

            }
        }
    }
}
*/