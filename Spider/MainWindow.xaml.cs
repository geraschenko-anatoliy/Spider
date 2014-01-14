using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
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

namespace Spider
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

        string path = @"http://wp7rocks.com/";
        string directoryPath;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string temp = @"F:\myfile.txt";
            DownloadFile(path, temp);
            //tbForSitePage.Text = temp;
        }
        public static void DownloadFile(string remoteFilename, string localFilename)
        {
            WebClient client = new WebClient();
            client.DownloadFile(remoteFilename, localFilename);
        }

        private void PageLinksScan_Click(object sender, RoutedEventArgs e)
        {
            WebClient webClient = new WebClient();
            webClient.OpenReadCompleted += new OpenReadCompletedEventHandler(webClient_OpenReadCompleted);
            Uri URL = new Uri(path);
            webClient.OpenReadAsync(URL);
        }

        private void webClient_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message);
            }
            else
            {
                HtmlWeb hw = new HtmlWeb();
                HtmlDocument doc = hw.Load(path);
                foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]"))
                {
                    lbForSitePageLinks.Items.Add(link.GetAttributeValue("href", null)) ;
                }
            }
        }

       

        private void PagesDownLoad_Click(object sender, RoutedEventArgs e)
        {
            directoryPath = @"F:\" + path.Replace(@"/", "1").Replace(":", "2");
            Directory.CreateDirectory(directoryPath);


            foreach (var item in lbForSitePageLinks.Items)
            {
                try
                {
                    System.IO.File.Create(@"F:\http211wp7rocks.com1"+item+"1").Dispose();
                    string filePath = @"F:\http211wp7rocks.com1\" + item+"1.txt";
                    DownloadFile(item.ToString(), filePath.Replace(@"/",@"\"));
                    lbDownloadedPages.Items.Add(item);

                }
                catch (Exception ex)
                {
                    lbForErrors.Items.Add(ex.Message);
                }
            }

        }
    }
}

