using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Spider2
{
    public class HTMLParser
    {
        public static async Task<Uri> DownloadSourceReturnHTMLs(Uri uri, int depth)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);

            HttpWebResponse response = (HttpWebResponse)(await request.GetResponseAsync());
            //Supernumerary.HsHTML.Add(uri.AbsoluteUri);
            HtmlDocument doc = new HtmlDocument();
            doc.Load(response.GetResponseStream());

            var links = doc.DocumentNode.SelectNodes("//a[@href]");

            //doc = null;
            //request = null;
            response.Dispose();

            foreach (var link in links)
            {
                try
                {
                    Uri temp = new Uri(Supernumerary.siteURI, link.GetAttributeValue("href", null));
                    if (!File.Exists(HDDWorker.PrepareHDD(temp, Supernumerary.siteURI, Supernumerary.hardURI, "text/html", true)))
                    {
                        //Supernumerary.dCol.Add(temp.AbsoluteUri);

                        // await downloadpage
                        await DownloadPage(temp);
                        // await replacepaths
                        if (temp.Host != Supernumerary.siteURI.Host)
                            depth = 0;
                        if (depth > 0)
                            await HTMLParser.DownloadSourceReturnHTMLs(temp, depth--);

                        //replace path in HTML
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message + " " + uri.AbsoluteUri);
                }
            };

            MainWindow win = (MainWindow)Application.Current.MainWindow;
            win.LinksTB.Text += Environment.NewLine + uri.AbsoluteUri;
            uri = null;
            links = null;
            GC.Collect();
            return uri;
        }

        static async Task<Uri> DownloadPage(Uri uri)
        { 
            await Downloader.Download(uri, Supernumerary.siteURI, Supernumerary.hardURI, "text/html");
            return null;
        }


    }
}
