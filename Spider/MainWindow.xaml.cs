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

        string path = @"http://wp7rocks.com/"; //"http://terrikon.com";
        string directoryPath;
        string globalPath = @"F:\";
        string currentFilePath;
        string currentDirectoryPath;

        public static void DownloadFile(string remoteFilename, string localFilename)
        {
            WebClient client = new WebClient();
            client.DownloadFile(remoteFilename, localFilename);
        }

        private void PageLinksScan_Click(object sender, RoutedEventArgs e)
        {
            ScanForLinks(path);
        }

        private void ScanForLinks(string pathForLinks)
        { 
            WebClient webClient = new WebClient();
            webClient.OpenReadCompleted += new OpenReadCompletedEventHandler(webClient_OpenReadCompleted);
            Uri URL = new Uri(pathForLinks);
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

        private void DownloadWebSite()
        {
            var document = new HtmlWeb().Load(path);

            currentDirectoryPath = globalPath + @"\"+path.Substring(7, path.Length-7);

            Directory.CreateDirectory(currentDirectoryPath);

            currentFilePath = currentDirectoryPath + @"\" + "index.html";

            File.WriteAllText(currentFilePath, String.Empty);

            DownloadFile(path, currentFilePath);

            lbDownloadedPages.Items.Add(File.ReadAllText(currentFilePath));


        }
        //private void DownloadHTML(string path)
        //{
        //    //string filePath = ParsePath(item, amount);
        //    //string fileName = item.Substring(item.LastIndexOf("/"), item.Length - item.LastIndexOf("/"));
        //    //DownloadRemoteFile(path + item, filePath + fileName, "text/less");
        //}

        private void ReplaceAllIllegalCharacters(ref string title)
        {
            title = title.Replace(':',' ');
            title = title.Replace('/', ' ');
            title = title.Replace('*', ' ');
            title = title.Replace('"', ' ');
            title = title.Replace('>', ' ');
            title = title.Replace('<', ' ');
            title = title.Replace('|', ' ');
        }

        private void SaveSinglePage_Click(object sender, RoutedEventArgs e)
        {
            DownloadHTML(path);
            DownloadCSS(path);
            DownloadJS(path);
            DownloadLess(path);
            DownloadImages(path);
        }
        private void DownloadHTML(string page_path)
        {
            var document = new HtmlWeb().Load(page_path);
            var urls = document.DocumentNode.SelectNodes("//a[@href]")
                                            .Select(e1 => e1.GetAttributeValue("href", null))
                                            .Where(s => !String.IsNullOrEmpty(s));

            string tempHTML = File.ReadAllText(currentFilePath);

            foreach (var item in urls)
            {
                try
                {
                    if (item[0] == '/' && item.Length > 1)
                    {
                        int amount = new Regex(@"/").Matches(item).Count;
                        if (amount > 1)
                        {
                            string filePath = ParsePath(item, amount);
                            string fileName = item.Substring(item.LastIndexOf("/"), item.Length - item.LastIndexOf("/"));
                            DownloadRemoteFile(path + item, filePath + fileName, "text/html");
                        }
                        else
                        {
                            DownloadRemoteFile(path + item, currentDirectoryPath + item.Substring(item.LastIndexOf("/"), item.Length - item.LastIndexOf("/")), "text/html");
                        }
                        //string t_item = "=\""+item;
                        //t_item = t_item[0] + t_item.Substring(2, t.item.length - 2);
                        tempHTML = tempHTML.Replace("=\"" + item, "=\"" + item.Substring(1, item.Length - 1)); // МЕТАМОРФОЗЫ КАКИЕ-то
                    }
                    else
                    {
                        //To do something with external image links
                    }
                }
                catch (Exception ex)
                {
                    lbErrorsWithImagesDownloading.Items.Add(ex.Message);
                }
            }
            File.WriteAllText(currentFilePath, tempHTML);
        }



        private void DownloadLess(string page_path)
        {
            var document = new HtmlWeb().Load(page_path);
            var urls = document.DocumentNode.Descendants("link")
                                            .Select(e1 => e1.GetAttributeValue("href", null))
                                            .Where(s => !String.IsNullOrEmpty(s));

            string tempHTML = File.ReadAllText(currentFilePath);

            foreach (var item in urls)
            {
                try
                {
                    if (item[0] == '/' && item.LastIndexOf(".less") != -1)
                    {
                        int amount = new Regex(@"/").Matches(item).Count;
                        if (amount > 1)
                        {
                            string filePath = ParsePath(item, amount);
                            string fileName = item.Substring(item.LastIndexOf("/"), item.Length - item.LastIndexOf("/"));
                            DownloadRemoteFile(path + item, filePath + fileName, "text/less");
                        }
                        else
                        {
                            DownloadRemoteFile(path + item, currentDirectoryPath + item.Substring(item.LastIndexOf("/"), item.Length - item.LastIndexOf("/")), "text/less");
                        }
                        tempHTML = tempHTML.Replace(item, item.Substring(1, item.Length - 1));
                    }
                    else
                    {
                        //To do something with external image links
                    }
                }
                catch (Exception ex)
                {
                    lbErrorsWithImagesDownloading.Items.Add(ex.Message);
                }
            }
            File.WriteAllText(currentFilePath, tempHTML);
        }
        private void DownloadJS(string page_path)
        {
            var document = new HtmlWeb().Load(page_path);
            var urls = document.DocumentNode.Descendants("script")
                                            .Select(e1 => e1.GetAttributeValue("src", null))
                                            .Where(s => !String.IsNullOrEmpty(s));

            string tempHTML = File.ReadAllText(currentFilePath);

            foreach (var item in urls)
            {
                try
                {
                    if (item[0] == '/')
                    {
                        int amount = new Regex(@"/").Matches(item).Count;
                        if (amount > 1)
                        {
                            string filePath = ParsePath(item, amount);
                            string fileName = item.Substring(item.LastIndexOf("/"), item.Length - item.LastIndexOf("/"));
                            DownloadRemoteFile(path + item, filePath + fileName, "application/x-javascript");
                        }
                        else
                        {
                            DownloadRemoteFile(item, currentDirectoryPath + item.Substring(item.LastIndexOf("/"), item.Length - item.LastIndexOf("/")), "application/x-javascript");                         
                        }
                        tempHTML = tempHTML.Replace(item, item.Substring(1, item.Length - 1));
                    }
                    else
                    {
                        //To do something with external image links
                    }
                }
                catch (Exception ex)
                {
                    lbErrorsWithImagesDownloading.Items.Add(ex.Message);
                }
            }
            File.WriteAllText(currentFilePath, tempHTML);
        }
        private void DownloadCSS(string page_path)
        {
            var document = new HtmlWeb().Load(page_path);
            var urls = document.DocumentNode.Descendants("link")
                                            .Select(e1 => e1.GetAttributeValue("href", null))
                                            .Where(s => !String.IsNullOrEmpty(s));

            string tempHTML = File.ReadAllText(currentFilePath);

            foreach (var item in urls)
            {
                try
                {
                    if ((item[0] == '/') && item.LastIndexOf(".css") != -1)
                    {
                        int amount = new Regex(@"/").Matches(item).Count;
                        if (amount > 1)
                        {
                            string filePath = ParsePath(item, amount);
                            string fileName = item.Substring(item.LastIndexOf("/"), item.Length - item.LastIndexOf("/"));
                            DownloadRemoteFile(path + item, filePath + fileName, "text/css");
                        }
                        else
                        {
                            DownloadRemoteFile(item, currentDirectoryPath + item.Substring(item.LastIndexOf("/"), item.Length - item.LastIndexOf("/")), "text/css");
                        }
                        tempHTML = tempHTML.Replace(item, item.Substring(1, item.Length - 1));
                    }
                    else
                    {
                        //To do something with external image links
                    }
                }
                catch (Exception ex)
                {
                    lbErrorsWithImagesDownloading.Items.Add(ex.Message);
                }
            }
            File.WriteAllText(currentFilePath, tempHTML);
            
        }
        private void DownloadImages(string page_path)
        {
            var document = new HtmlWeb().Load(page_path);

            var urls = document.DocumentNode.Descendants("img")
                                            .Select(e1 => e1.GetAttributeValue("src", null))
                                            .Where(s => !String.IsNullOrEmpty(s));

            lbLinksToImages.ItemsSource = urls;

            string tempHTML = File.ReadAllText(currentFilePath);

            foreach (var item in urls)
            {
                try
                {
                    if (item[0] == '/')
                    {
                        int amount = new Regex(@"/").Matches(item).Count;
                        if (amount > 1)
                        {
                            string filePath = ParsePath(item, amount);
                            string fileName = item.Substring(item.LastIndexOf("/"), item.Length - item.LastIndexOf("/"));
                            DownloadRemoteFile(path + item, filePath + fileName, "image");
                        }
                        else
                        {
                            DownloadRemoteFile(item, currentDirectoryPath + item.Substring(item.LastIndexOf("/"), item.Length - item.LastIndexOf("/")), "image");                         
                        }
                        tempHTML = tempHTML.Replace(item, item.Substring(1, item.Length - 1));
                    }
                    else
                    {
                        //To do something with external image links
                    }
                }
                catch (Exception ex)
                {
                    lbErrorsWithImagesDownloading.Items.Add(ex.Message);
                }
            }
            File.WriteAllText(currentFilePath, tempHTML);
        }

        string ParsePath(string pathForParsing, int amount)
        {
            string temp_pathForParsing = pathForParsing.Substring(1, pathForParsing.Length - 1);
            string temp_currentDirectoryPath = currentDirectoryPath;

            while (amount > 1)
            {
                string nextDirectoryName = temp_pathForParsing.Substring(0, temp_pathForParsing.IndexOf(@"/"));
                temp_pathForParsing = temp_pathForParsing.Substring(temp_pathForParsing.IndexOf(@"/")+1, temp_pathForParsing.Length - temp_pathForParsing.IndexOf(@"/")-1);
                Directory.CreateDirectory(temp_currentDirectoryPath + @"/" + nextDirectoryName);
                temp_currentDirectoryPath += @"/" + nextDirectoryName;
                amount--;
            }
            return temp_currentDirectoryPath;
        }

        private static void DownloadRemoteFile(string uri, string fileName, string fileType)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            if ((response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.Moved ||
                response.StatusCode == HttpStatusCode.Redirect) &&
                response.ContentType.StartsWith(fileType, StringComparison.OrdinalIgnoreCase))
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

        private void PagesDownloadBtn(object sender, RoutedEventArgs e)
        {
            DownloadWebSite();
        }

    }
}

