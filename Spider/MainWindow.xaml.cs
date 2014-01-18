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

        string path = "http://terrikon.com/football"; // @"http://wp7rocks.com/";
        string directoryPath;
        string globalPath = @"F:\";
        string currentFilePath;
        string currentDirectoryPath;

        //private void Button_Click(object sender, RoutedEventArgs e)
        //{
        //    string temp = @"F:\myfile.txt";
        //    DownloadFile(path, temp);
        //    //tbForSitePage.Text = temp;
        //}
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
            //directoryPath = @"F:\" + path.Replace(@"/", "1").Replace(":", "2");
            //Directory.CreateDirectory(directoryPath);

            //foreach (var item in lbForSitePageLinks.Items)
            //{
            //    try
            //    {
            //        System.IO.File.Create(@"F:\http211wp7rocks.com1"+item+"1").Dispose();
            //        string filePath = @"F:\http211wp7rocks.com1\" + item+"1.txt";
            //        DownloadFile(item.ToString(), filePath.Replace(@"/", @"\"));
            //        lbDownloadedPages.Items.Add(item);

            //    }
            //    catch (Exception ex)
            //    {
            //        lbForErrors.Items.Add(ex.Message);
            //    }
            //}

            var document = new HtmlWeb().Load(path);
            string header = document.DocumentNode.SelectSingleNode("//title").InnerHtml;
            //header = header.Replace('/',' ');
            ReplaceAllIllegalCharacters(ref header);

            currentDirectoryPath = globalPath + header + "_files";

            Directory.CreateDirectory(currentDirectoryPath);

            currentFilePath = globalPath + @"\" + header + ".html";

            File.WriteAllText(currentFilePath, String.Empty);

            DownloadFile(path, currentFilePath);
            lbDownloadedPages.Items.Add(File.ReadAllText(currentFilePath));
        }

        private void ReplaceAllIllegalCharacters(ref string title)
        {
            title = title.Replace(':',' ');
            title = title.Replace('/', ' ');
            //title.Replace('\', ' ');
            title = title.Replace('*', ' ');
            title = title.Replace('"', ' ');
            title = title.Replace('>', ' ');
            title = title.Replace('<', ' ');
            title = title.Replace('|', ' ');
        }

        private void SaveSinglePage_Click(object sender, RoutedEventArgs e)
        {
            DownloadCss(path);
            DownloadPage(path);
        }

        private void DownloadCss(string page_path)
        {
            var document = new HtmlWeb().Load(page_path);
            var urls = document.DocumentNode.Descendants("link")
                                            .Select(e1 => e1.GetAttributeValue("src", null))
                                            .Where(s => !String.IsNullOrEmpty(s));

            string f = "";

        }

        private void DownloadPage(string page_path)
        {
            var document = new HtmlWeb().Load(page_path);

            var urls = document.DocumentNode.Descendants("img")
                                            .Select(e1 => e1.GetAttributeValue("src", null))
                                            .Where(s => !String.IsNullOrEmpty(s));

            lbLinksToImages.ItemsSource = urls;

            string header = document.DocumentNode.SelectSingleNode("//title").InnerHtml;

            string tempHTML = File.ReadAllText(currentFilePath);

            string tempPathForCurrentImage = "";
            string token = "";

            foreach (var item in urls)
            {
                try
                {
                    if (item[0] == '/')
                    {
                        tempPathForCurrentImage = currentDirectoryPath + @"\" + item.Substring(item.LastIndexOf('/') + 1, item.Length - item.LastIndexOf('/') - 1);         
                        DownloadRemoteImageFile(page_path + item, tempPathForCurrentImage);
                        token = "./"+currentDirectoryPath.Substring(3, currentDirectoryPath.Length-3)+@"/" + item.Substring(item.LastIndexOf('/') + 1, item.Length - item.LastIndexOf('/') - 1);
                        tempHTML = tempHTML.Replace(item, token);
                    }
                    else
                    {
                        tempPathForCurrentImage = currentDirectoryPath + @"\" + item.Substring(item.LastIndexOf('/') + 1, item.Length - item.LastIndexOf('/') - 1);
                        if (tempPathForCurrentImage.LastIndexOf('?') != -1)
                        {
                            DownloadRemoteImageFile(item, tempPathForCurrentImage.Substring(0, tempPathForCurrentImage.LastIndexOf('?')));
                        }
                        else
                        {
                            DownloadRemoteImageFile(item, tempPathForCurrentImage);
                        }                        
                        token = "./" + currentDirectoryPath.Substring(3, currentDirectoryPath.Length - 3) + @"/" + item.Substring(item.LastIndexOf('/') + 1, item.Length - item.LastIndexOf('/') - 1);
                    }
                    tempHTML = tempHTML.Replace(item, token);
                }
                catch (Exception ex)
                {
                    lbErrorsWithImagesDownloading.Items.Add(ex.Message);
                }
            }
            File.WriteAllText(currentFilePath, tempHTML);
        }


        private static void DownloadRemoteImageFile(string uri, string fileName)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            if ((response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.Moved ||
                response.StatusCode == HttpStatusCode.Redirect) &&
                response.ContentType.StartsWith("image", StringComparison.OrdinalIgnoreCase))
            {
                using (Stream inputStream = response.GetResponseStream())
                using (Stream outputStream = File.OpenWrite(fileName))
                {
                    byte[] buffer = new byte[4096];
                    int bytesRead;
                    do
                    {
                        bytesRead = inputStream.Read(buffer, 0, buffer.Length);
                        outputStream.Write(buffer, 0, bytesRead);
                    } while (bytesRead != 0);
                }
            }
        }

    }
}

