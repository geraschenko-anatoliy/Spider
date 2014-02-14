using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Spider2
{
    public class HTMLParser
    {
        public static async Task<Uri> DownloadSourceReturnHTMLs(Uri uri, Uri sUri, Uri hUri, int depth)
        {

            //if (lParsedLinks.Contains(page_adress) == true || page_adress == string.Empty)
            //    return result;
            //else lParsedLinks.Add(page_adress + " " + nodeType + " " + attributeType);

            //HashSet<Uri> temp_data = new HashSet<Uri>();
            //Downloader d = new Downloader();
            //HDDWorker hddw = new HDDWorker();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            HttpWebResponse response = (HttpWebResponse)(await request.GetResponseAsync());
            
            HtmlDocument doc = new HtmlDocument();
            //string a = uri.AbsoluteUri.ToString();
            doc.Load(response.GetResponseStream());

            var links = doc.DocumentNode.SelectNodes("//a[@href]");

            doc = null;
            GC.Collect();

            while (depth > 0)
            {
                foreach (var link in links)//doc.DocumentNode.SelectNodes("//a[@href]"))
                {
                    try
                    {
                        Uri temp = new Uri(sUri, link.GetAttributeValue("href", null));
                        //Supernumerary.hsHTML.Add(temp);
                        if (!Supernumerary.dCol.Contains(temp)&&(temp.Host==sUri.Host))
                        {
                            Supernumerary.dCol.Add(temp);
                            await Downloader.Download(temp, HDDWorker.PrepareHDD(temp, sUri, hUri, "text/html"), "text/html");
                            await HTMLParser.DownloadSourceReturnHTMLs(temp, sUri, hUri, depth--);
                            //replace path in HTML
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
                links = null;
                GC.Collect();

            }

            
            //await Downloader.Download(uri, HDDWorker.PrepareHDD(uri, sUri, hUri), "text/html");
            
            
            //foreach (var link in doc.DocumentNode.SelectNodes("//a[@href]"))
            //{
            //    Uri temp = new Uri(link.GetAttributeValue("href", null));

            //    //Supernumerary.hsHTML.Add(temp);
            //    if (!Supernumerary.dCol.Contains(temp))
            //    {
            //        Supernumerary.dCol.Add(temp);
            //        await Downloader.Download(temp, HDDWorker.PrepareHDD(temp, sUri, hUri), "text/html");
            //    }
            //}

            

            return uri;
        }
    }
}
