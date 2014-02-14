using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
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

namespace Spider2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //SomeFunk(new Uri("http://terrikon.com/posts/180682"));


            //HtmlDocument doc = new HtmlDocument();
            //doc.Load("http://terrikon.com/posts/180682");

            
            Funk();
            Task.WaitAll();
            Supernumerary Statistics = new Supernumerary();
        }

        public async void Funk()
        { 
            Uri nu = new Uri("http://terrikon.com");
            Directory.CreateDirectory(hardURI.LocalPath + nu.Host);
            Supernumerary.hsHTML.Add(nu);
            await HTMLParser.DownloadSourceReturnHTMLs(nu, siteURI, hardURI, 2);
            await Downloader.Download(nu, HDDWorker.PrepareHDD(nu, siteURI, hardURI, "text/html"), "text/html");
        }

        public Uri siteURI =  new Uri("http://terrikon.com");
        public Uri hardURI = new Uri("D:/");
        public HashSet<Uri> htmls;
        public BlockingCollection<string> col = new BlockingCollection<string>();

        private async void SomeFunk(Uri uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            HttpWebResponse response = (HttpWebResponse)(await request.GetResponseAsync());

            Stream inputStreamx = response.GetResponseStream();

            using (Stream inputStream = response.GetResponseStream())
            {
                MemoryStream ms = new MemoryStream();
                inputStream.CopyTo(ms);
                
                
                //load html
                HtmlDocument docum = new HtmlDocument();
                docum.Load(ms);

                Downloader d = new Downloader();
                //d.SomeFunk(Uri);
 

                docum.Save(ms);
                ms.Seek(0, SeekOrigin.Begin);
                
                //get urls from page



                //async download all resourses
                
                string SourceDataPath;
                string SourceFolderPath;
                if (uri.Host == siteURI.Host)
                {
                   SourceFolderPath = hardURI.LocalPath + siteURI.Host + @"\" + uri.AbsolutePath.Replace(uri.Segments[uri.Segments.Count() - 1], "");
                   SourceDataPath = hardURI.LocalPath + siteURI.Host + @"\" + uri.AbsolutePath; 
                }
                else
                {
                   SourceFolderPath = hardURI.LocalPath + siteURI.Host + @"\" + uri.Host + uri.AbsolutePath.Replace(uri.Segments[uri.Segments.Count() - 1], "");
                   SourceDataPath = hardURI.LocalPath + siteURI.Host + @"\" + uri.Host + uri.AbsolutePath;
                }
                if (!Directory.Exists(SourceFolderPath))
                {
                    Directory.CreateDirectory(SourceFolderPath);
                }



                using (Stream outputStream = File.OpenWrite(SourceDataPath))
                {
                    byte[] buffer = new byte[4096];
                    int bytesRead;
                    do
                    {
                        bytesRead = await ms.ReadAsync(buffer, 0, buffer.Length);
                        await outputStream.WriteAsync(buffer, 0, bytesRead);
                    } while (bytesRead != 0);
                }
            }


        }

        

        //private async void SimpleDownload(Uri uri)
        //{
        //    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
        //    HttpWebResponse response = (HttpWebResponse)(await request.GetResponseAsync());

        //    if ((response.StatusCode == HttpStatusCode.OK ||
        //            response.StatusCode == HttpStatusCode.Moved ||
        //            response.StatusCode == HttpStatusCode.Redirect) &&
        //            response.ContentType.StartsWith(fileType, StringComparison.OrdinalIgnoreCase))
        //    {

        //        await Task.Run(async () =>
        //        {
        //            using (Stream inputStream = response.GetResponseStream())
        //            using (Stream outputStream = File.OpenWrite(filePath))
        //            {
        //                byte[] buffer = new byte[4096];
        //                int bytesRead;
        //                do
        //                {
        //                    bytesRead = await inputStream.ReadAsync(buffer, 0, buffer.Length);
        //                    await outputStream.WriteAsync(buffer, 0, bytesRead);
        //                } while (bytesRead != 0);
        //            }
        //        });
        //    }
        //    else
        //    {
        //        LErrorList.Add(string.Format("Function - ProcessURL, current item - {0}, error message - Error in response, {1}, {2}", url, response.StatusCode, response.ContentType));
        //    }
        //}
    }
}
