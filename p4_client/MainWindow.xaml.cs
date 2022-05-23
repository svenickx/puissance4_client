using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace p4_client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Socket _ClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            LoopConnect();

            Thread listening_thread = new Thread(Receive);
            listening_thread.Start();

            req.IsEnabled = true;
            send.IsEnabled = true;
        }

        private void send_Click(object sender, RoutedEventArgs e)
        {
            Send(req.Text);
        }

        public void Send(string req)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(req);
            _ClientSocket.Send(buffer);
        }
        public void Receive()
        {
            Console.WriteLine("Now listening...");
            while (true)
            {
                byte[] receiveBuf = new byte[1024];
                int rec = _ClientSocket.Receive(receiveBuf);
                byte[] data = new byte[rec];
                Array.Copy(receiveBuf, data, rec);
                Console.WriteLine("Received: " + Encoding.ASCII.GetString(data));
                // UPDATE UI
                Dispatcher.Invoke(() =>
                {
                    Time.Content = Encoding.ASCII.GetString(data);
                });
            }
        }
        public void LoopConnect()
        {
            int attempts = 0;
            while (!_ClientSocket.Connected)
            {
                try
                {
                    attempts++;
                    _ClientSocket.Connect(IPAddress.Loopback, 10000);
                }
                catch (SocketException)
                {
                    Console.Clear();
                    Console.WriteLine("Connection attempts: " + attempts.ToString());
                }
            }
            Console.Clear();
            Console.WriteLine("Connected");
        }
    }
}