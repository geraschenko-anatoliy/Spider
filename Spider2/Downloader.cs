using HtmlAgilityPack;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Spider2
{
    public class Downloader
    {
        //public Uri siteURI = new Uri("http://football.ua");
        //public Uri hardURI = new Uri("D:/");
        //public static HashSet<Uri> htmls;
        //public BlockingCollection<string> col = new BlockingCollection<string>();

        public async static Task<string> Download(Uri uri, string filePath, string fileType)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri.AbsoluteUri);
            HttpWebResponse response = (HttpWebResponse)(await request.GetResponseAsync());

            if ((response.StatusCode == HttpStatusCode.OK ||
                        response.StatusCode == HttpStatusCode.Moved ||
                        response.StatusCode == HttpStatusCode.Redirect) &&
                        response.ContentType.StartsWith(fileType, StringComparison.OrdinalIgnoreCase))
            {
                await Task.Run(async () =>
                {
                    using (Stream inputStream = response.GetResponseStream())
                    using (Stream outputStream = File.OpenWrite(filePath.TrimEnd('/')))
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
                //error 
            }
            return uri.AbsolutePath;
        }

        //public async Task<int> SimpleDownload(Uri uri, string fileType, string filePath)
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
        //        //LErrorList.Add(string.Format("Function - ProcessURL, current item - {0}, error message - Error in response, {1}, {2}", url, response.StatusCode, response.ContentType));
        //    }
        //    return 1;
        //}
    }
}
