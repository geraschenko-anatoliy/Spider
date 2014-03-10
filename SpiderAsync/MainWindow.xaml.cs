using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
using System.Windows.Shapes;
using HtmlAgilityPack;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;
using System.Net.Http;
namespace SpiderAsync
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            downloadedLinks = new HashSet<string>();
            nDownloadedSources = 0;
            lParsedLinks = new List<string>();
            nParsedLinks = 0;
            currentDirectoryPath = globalPath + @"\" + path.Substring(7, path.Length - 7);
            lErrorList = new List<string>();
            currentResoursesInDownloading = new List<string>();
        }

        static HttpClient hc = new HttpClient();
        // Main info
        string path = @"http://wp7rocks.com";
        string globalPath = @"F:\";
        string currentDirectoryPath;
        private object mylock = new object();
        // Statistics
        HashSet<string> downloadedLinks;
        List<string> lParsedLinks;
        List<string> lErrorList;
        int nParsedLinks;
        int nDownloadedSources;
        string currentParsingLink;
        List<string> currentResoursesInDownloading;

        #region Properties_and_events
        public string CurrentParsingLink
        {
            get { return currentParsingLink; }
            set
            {
                currentParsingLink = value;
                OnCurrentParsingLinkChanged();
            }
        }

        protected virtual void OnCurrentParsingLinkChanged()
        {
            ParsingPagesTB.Text = String.Format("\r\n Name of parsing page : {0}", currentParsingLink) + ParsingPagesTB.Text;
        }
        public int NDownloadedSources
        { 
            get { return nDownloadedSources; }
            set
            {
                nDownloadedSources = value;
                OnNDownloadedSources();
            }
        }
        protected virtual void OnNDownloadedSources()
        {
            nDownloadedSourcesTB.Text = String.Format("Number of downloaded resources : {0}", nDownloadedSources);
        }
        public int NParsedLinks
        {
            get { return nParsedLinks; }
            set
            {
                nParsedLinks = value;
                OnNParsedLinksChanged();
            }
        }
        protected virtual void OnNParsedLinksChanged()
        {
            nParsedLinksTB.Text = String.Format("Number of parsed links : {0}", nParsedLinks);
        }
        public List<string> CurrentResoursesInDownloading
        {
            get 
            {
                //List<string> result;
                //lock(mylock)
                //{
                //    result = currentResoursesInDownloading;
                //    ONCurrentResoursesInDownloading();
                //}

                //return result;
                return currentResoursesInDownloading;
            }
            set
            {
                lock (mylock)
                {
                    currentResoursesInDownloading = value;
                }
                //ONCurrentResoursesInDownloading();
            }
        }
        protected virtual void ONCurrentResoursesInDownloading()
        {
            currentResoursesInDownloadingTB.Text = String.Join(Environment.NewLine, currentResoursesInDownloading);
        }

        public List<string> LErrorList
        {
            get 
            {
                //ONLErrorListChanged();    
                return lErrorList; 
            }
            set
            {
                lErrorList = value;
                //ONLErrorListChanged();
            }
        }

        protected virtual void ONLErrorListChanged()
        {
            //currentResoursesInDownloadingTB.Text = String.Join(Environment.NewLine, lErrorList);
            nErrorListTB.Text = "Number of errors -" + lErrorList.Count.ToString();
        }

        #endregion
        async void DownloadBtn_Click(object sender, RoutedEventArgs e)
        {
            DownloadedSourcesListTB.Clear();
            path = pathTB.Text;
            int depth = -1;
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

            globalPath = globalPathBT.Text;
        
            currentDirectoryPath = globalPath + @"\" + path.Substring(7, path.Length - 7);

            await RecursiveDownloading(path, depth);

        }

        #region TasksForDownloadingSources
        async private Task<int> RecursiveDownloading(string url, int depth)
        {
            if (depth == 0)
            {
                await DownloadAll(url);

                return -1;
            }
            if (depth > 0)
            {
                foreach (var item in await GetUrlsFromPage(url, "//a[@href]", "href", "text/html"))
                {
                    await RecursiveDownloading(item, depth - 1);
                    
                    if (downloadedLinks.Contains(url) == false)
                        await DownloadAll(url);
                }
                return depth - 1;
            }
            return -2;
        }
        async private Task<int> DownloadAll(string url)
        {
            //await DownloadPages(await GetUrlsFromPage(url, "//a[@href]", "href", "text/html"), "text/html");
            //await DownloadPages(await GetUrlsFromPage(url, "img", "src", "image"), "image");
            //await DownloadPages(await GetUrlsFromPage(url, "link", "href", "text/less"), "text/less");
            //await DownloadPages(await GetUrlsFromPage(url, "script", "src", "application/x-javascript"), "application/x-javascript");
            //await DownloadPages(await GetUrlsFromPage(url, "link", "href", "text/css"), "text/css");

            await DownloadPages(url, "//a[@href]", "href", "text/html");
            await DownloadPages(url, "img", "src", "image");
            await DownloadPages(url, "link", "href", "text/less");
            await DownloadPages(url, "script", "src", "application/x-javascript");
            await DownloadPages(url, "link", "href", "text/css");

            return 0;
        }
        async Task<int> DownloadPages(string link, string nodeType, string attributeType, string fileType)
        {
            IEnumerable<string> urlList = await GetUrlsFromPage(link, nodeType, attributeType, fileType);

            var urlBundles = from url in urlList.AsParallel() select new Tuple<string, string>(url, getFilePath(url));

            var downloadTasks = (from url in urlBundles select ProcessURL(url.Item1, url.Item2, fileType)).ToList();

            int counter = downloadTasks.Count;

            while (downloadTasks.Count > 0)
            {
                Task<string> firstFinishedTask = await Task.WhenAny(downloadTasks);

                downloadTasks.Remove(firstFinishedTask);

                string finishedURL = "";

                if (firstFinishedTask.Exception == null)
                    finishedURL = firstFinishedTask.Result;
                else
                    return -1;

                DownloadedSourcesListTB.Text = String.Format("\r\n Name of downloaded page : {0}", finishedURL) + DownloadedSourcesListTB.Text;
            }

            NDownloadedSources++;

            DownloadedSourcesListTB.Text = String.Format("\r\n Page downloaded - {0} , {1}, {2}, {3}",  link, nodeType, attributeType, fileType) + DownloadedSourcesListTB.Text;

            ONLErrorListChanged();

            return NDownloadedSources;
        }
        #endregion

        #region ParsingPathForHDD

        string getFilePath(string item)
        {
            string result = currentDirectoryPath + "/" + path.Replace("http://", "");

            try
            {
                if (item.Contains(path))
                    item = item.Replace(path, "");

                if (item[0] == '/' && item.Length > 1)
                {
                    int amount = new Regex(@"/").Matches(item).Count;

                    if (amount > 1)
                    {
                        string filePath = ParsePath(item, amount);

                        string fileName = item.Substring(item.LastIndexOf("/"), item.Length - item.LastIndexOf("/"));
                        
                        result = filePath + fileName;
                    }
                    else
                    {
                        result = currentDirectoryPath + item.Substring(item.LastIndexOf("/"), item.Length - item.LastIndexOf("/"));
                    }
                    //string t_item = "=\""+item;
                    //t_item = t_item[0] + t_item.Substring(2, t.item.length - 2);
                    //tempHTML = tempHTML.Replace("=\"" + item, "=\"" + item.Substring(1, item.Length - 1)); // МЕТАМОРФОЗЫ КАКИЕ-то
                }
                else
                {
                    // THIS PLACE FOR EXTERNAL LINKS
                    #region external_links
                    //item =  item.Replace("http://","/");
                    //item = item.Substring(item.IndexOf("/"), item.Length - item.IndexOf("/"));
                    //int amount = new Regex(@"/").Matches(item).Count;
                    //if (amount > 1)
                    //{
                    //    string filePath = ParsePath(item, amount);
                    //    string fileName = item.Substring(item.LastIndexOf("/"), item.Length - item.LastIndexOf("/"));
                    //    result = filePath + fileName;
                    //}
                    //else
                    //{
                    //    result = currentDirectoryPath + item.Substring(item.LastIndexOf("/"), item.Length - item.LastIndexOf("/"));
                    //}
                    //To do something with external image links
                    #endregion
                }
            }
            catch (Exception ex)
            {
                LErrorList.Add(string.Format("Function - getFilePath, current item - {0}, error message -{1}", item, ex.Message));
            }

            result = ReplaceAllIllegalCharacters(result);

            return result;
        }
        
        private string ReplaceAllIllegalCharacters(string title) // It can be implemented in regular expressions
        {
            string result;
            //result = title.Replace(':', ' ');
            result = title.Replace('*', ' ');
            result = result.Replace('"', ' ');
            result = result.Replace('?', ' ');
            result = result.Replace('>', ' ');
            result = result.Replace('<', ' ');
            result = result.Replace('|', ' ');
            return result;
        }
        
        string ParsePath(string pathForParsing, int amount)
        {
            string temp_pathForParsing = pathForParsing.Substring(1, pathForParsing.Length - 1);

            string temp_currentDirectoryPath = currentDirectoryPath;

            while (amount > 1)
            {
                string nextDirectoryName = temp_pathForParsing.Substring(0, temp_pathForParsing.IndexOf(@"/"));
                temp_pathForParsing = temp_pathForParsing.Substring(temp_pathForParsing.IndexOf(@"/") + 1, temp_pathForParsing.Length - temp_pathForParsing.IndexOf(@"/") - 1);
                Directory.CreateDirectory(temp_currentDirectoryPath + @"/" + nextDirectoryName);
                temp_currentDirectoryPath += @"/" + nextDirectoryName;
                amount--;

            }
            return temp_currentDirectoryPath;
        }
        
        #endregion

        #region ParsingPathForWeb
        async private Task<IEnumerable<string>> GetUrlsFromPage(string page_adress, string nodeType, string attributeType, string fileType)
        {
            HashSet<string> result = new HashSet<string>();
            if (lParsedLinks.Contains(page_adress) == true || page_adress == string.Empty)
                    return result;
            else lParsedLinks.Add(page_adress + " " + nodeType + " " + attributeType);
            await Task.Run(async () =>
            {
                HashSet<string> temp_data = new HashSet<string>();

                try
                {
                    Stream page_stream = await hc.GetStreamAsync(page_adress);
                    HtmlDocument doc = new HtmlDocument();
                    doc.Load(page_stream);

                    if (nodeType == "//a[@href]")
                    {
                        foreach (HtmlNode link in doc.DocumentNode.SelectNodes(nodeType))
                        {
                            temp_data.Add(link.GetAttributeValue("href", null));
                        }
                    }
                    else
                    {
                        var urls = doc.DocumentNode.Descendants(nodeType)
                                    .Select(e1 => e1.GetAttributeValue(attributeType, null))
                                    .Where(s => !String.IsNullOrEmpty(s));
                        foreach (string item in urls)
                        {
                            temp_data.Add(item);
                        }
                    }

                    foreach (var temp in temp_data)
                    {
                        #region commented_code
                        //string f_result = path;
                        ////string temp = link.GetAttributeValue(attributeType, null);// "href", null);

                        //if (temp[0] == '/')
                        //{
                        //    if (temp != "/")
                        //        f_result += temp;
                        //    else
                        //    {
                        //        // f_result is path; 
                        //    }
                        //}
                        //else
                        //{
                        //    if (temp.Contains(path))
                        //    {
                        //        //but links like vk
                        //        f_result = temp;
                        //    }
                        //    else
                        //    {
                        //        //else outgoing link 
                        //    }
                        //}
                        #endregion

                        string fullLink = GetFullLinkFromTempHashSet(temp);
                        if (downloadedLinks.Contains(fullLink) == false)
                            switch (fileType)
                            {
                                case "text/less":
                                    //if (fullLink.Contains(".less") == true)
                                    if (fullLink.LastIndexOf(".less") == fullLink.Length - 5)
                                    {
                                        result.Add(fullLink);
                                        break;
                                    }
                                    else
                                        break;
                                case "text/css":
                                    //if (fullLink.Contains(".css") == true)
                                    if (fullLink.LastIndexOf(".css") == fullLink.Length - 4)
                                    {
                                        result.Add(fullLink);
                                        break;
                                    }
                                    else
                                        break;
                                case "application/x-javascript":
                                    if (fullLink.LastIndexOf(".js") == fullLink.Length - 3)
                                    //if (fullLink.Contains(".js") == true)
                                    {
                                        result.Add(fullLink);
                                        break;
                                    }
                                    else
                                        break;
                                case "image":
                                    result.Add(fullLink);
                                    break;
                                case "text/html":
                                    result.Add(fullLink);
                                    break;
                                default:
                                    break;
                            }
                    }
                    foreach (var item in result)
                    {
                        downloadedLinks.Add(item);
                    }
                }
                catch (Exception ex)
                {
                    LErrorList.Add(string.Format("Function - GetUrlsFromPage, current item - {0}, error message - {1}", page_adress, ex.Message));
                }
            });
            NParsedLinks += result.Count;
            CurrentParsingLink = page_adress + " " + nodeType + " " + attributeType+ " " + fileType+  " ";
            return result;
        }
        string GetFullLinkFromTempHashSet(string temp)
        {
            string f_result = path;
     
            if (temp[0] == '/')
            {
                if (temp != "/")
                    f_result += temp;
                else
                {
                    // f_result is path; 
                }
            }
            else
            {
                if (temp.Contains(path))
                {
                    //but links like http:/vk.com/habrahabr.ru 
                    f_result = temp;
                }
                else
                {
                    //else outgoing link 
                    //f_result = temp;
                }
            }
            return f_result;
        }
        #endregion

        #region FunctionForDownloadingSource

        async Task<string> ProcessURL(string url, string fileName, string fileType)
        {
            CurrentResoursesInDownloading.Add(url);
            string filePath = "";
            switch (fileType)
            {
                case "image": filePath = fileName;
                    break;
                //case "text/less": filePath = fileName + ".less";
                //    break;
                //case "text/css": filePath = fileName + ".css";
                //    break;
                //case "application/x-javascript": filePath = fileName + ".js";
                //    break;
                default: filePath = fileName + ".html";
                    break;
            }
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse response = (HttpWebResponse)(await request.GetResponseAsync());

                if ((response.StatusCode == HttpStatusCode.OK ||
                        response.StatusCode == HttpStatusCode.Moved ||
                        response.StatusCode == HttpStatusCode.Redirect) &&
                        response.ContentType.StartsWith(fileType, StringComparison.OrdinalIgnoreCase))
                {

                    await Task.Run(async () =>
                        {
                            using (Stream inputStream = response.GetResponseStream())
                            using (Stream outputStream = File.OpenWrite(filePath))
                            {
                                byte[] buffer = new byte[4096];
                                int bytesRead;
                                do
                                {
                                    bytesRead = await inputStream.ReadAsync(buffer, 0, buffer.Length);
                                    await outputStream.WriteAsync(buffer, 0, bytesRead);
                                } while (bytesRead != 0);
                            }
                        });
                }
                else
                {
                    //MessageBox.Show("Error in response" + url);
                    LErrorList.Add(string.Format("Function - ProcessURL, current item - {0}, error message - Error in response, {1}, {2}", url, response.StatusCode, response.ContentType));
                }
            }
            catch (Exception ex)
            {
                LErrorList.Add(string.Format("Function - ProcessURL, current item - {0}, error message - {1}", url, ex.Message));
            }


            CurrentResoursesInDownloading.Remove(url);
            return url;
        }
        #endregion

    }
}
