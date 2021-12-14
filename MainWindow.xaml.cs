using Microsoft.Win32;
using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;

namespace DAAC
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MediaWindow mediaWindow;

        public MainWindow()
        {
            InitializeComponent();
            mediaWindow = new MediaWindow(Dispatcher);
            //griddata.DataContext = this;
            StreamerServer.Instance.setDispatcher(Dispatcher);
            griddata.ItemsSource = StreamerServer.Instance.endPoints;
            stop.IsEnabled = false;
        }

        private void MediaWindow_Closed(object sender, EventArgs e)
        {
            selectmedia.IsEnabled = true;
        }

        private void start_Click(object sender, RoutedEventArgs e)
        {
            var ip = NetworkInterface.GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up)
                .Where(n => n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .Where(n => n.GetIPProperties()?.GatewayAddresses.Count != 0)
                .SelectMany(n => n.GetIPProperties()?.UnicastAddresses)
                .Select(g => g?.Address)
                .Where(a => a != null)
                .Where(a => a.AddressFamily == AddressFamily.InterNetwork)
                .Where(a => Array.FindIndex(a.GetAddressBytes(), b => b != 0) >= 0)
                .FirstOrDefault();
            displayIP.Text = ip.ToString();
            StreamerServer.Instance.StartServer();
            start.IsEnabled = false;
            stop.IsEnabled = true;
        }

        private void stop_Click(object sender, RoutedEventArgs e)
        {
            StreamerServer.Instance.StopServer();
            stop.IsEnabled = false;
            start.IsEnabled = true;
        }

        private void selectmedia_Click(object sender, RoutedEventArgs e)
        {
            SelectFile();
            if(mediaWindow.IsVisible)
            {
                selectmedia.IsEnabled = false;
            }
        }

        private void SelectFile()
        {
            OpenFileDialog openFile = new OpenFileDialog();
            if (openFile.ShowDialog().Value)
            {
                mediaWindow = new MediaWindow(Dispatcher);
                mediaWindow.Closed += MediaWindow_Closed;
                mediaWindow.getMediaLocation(openFile.FileName);
                mediaWindow.Show();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            StreamerServer.Instance.StopServer();
            Environment.Exit(0);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var ep = ((Button)sender).DataContext as EndPoint;
            var index = StreamerServer.Instance.endPoints.IndexOf(ep);
            StreamerServer.Instance.endPoints[index].Status = !StreamerServer.Instance.endPoints[index].Status;
            if (StreamerServer.Instance.endPoints[index].Status) ((Button)sender).Content = "Block";
            else ((Button)sender).Content = "Unblock";
        }
    }
}
