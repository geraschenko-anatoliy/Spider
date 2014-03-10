using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace Spider2Async
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void DownloadBtn_Click(object sender, RoutedEventArgs e)
        {
            Supernumerary.ResetToDefault();
            Supernumerary.depth = GetDepth();

            Supernumerary.siteURI = new Uri(pathTB.Text);
            Supernumerary.hPath = System.IO.Path.Combine(globalPathTB.Text, Supernumerary.siteURI.Host);

            Messenger.StartDispatcher();

            int download_result = await  Downloader.RecursiveDownloading(Supernumerary.siteURI, Supernumerary.depth);
            MessageBox.Show("Конец работы");
        }
        private void StopBtn_Click(object sender, RoutedEventArgs e)
        {
            Supernumerary.cts.Cancel();
        }

        private int GetDepth()
        {
            int depth = 0;
            switch (DownloadingDepthCB.SelectedIndex)
            {
                case 0:
                    depth = 0;
                    break;
                case 1:
                    depth = 1;
                    break;
                case 2:
                    depth = 2;
                    break;
                case 3:
                    depth = 3;
                    break;
                case 4:
                    depth = 99;
                    break;
            }
            return depth;
        }
    }
}