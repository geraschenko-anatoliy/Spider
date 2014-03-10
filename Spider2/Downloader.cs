using HtmlAgilityPack;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Spider2
{
    public class Downloader
    {
        public async static Task<string> Download(Uri uri, Uri sUri, Uri hUri, string fileType)
        {
            string filePath = HDDWorker.PrepareHDD(uri, sUri, hUri, fileType, false);
            string path = uri.AbsoluteUri;

            if (fileType == null)
                return null;

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(path);
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

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Downloader " + ex.Message + " " + uri.AbsoluteUri);
            }

            lock (Supernumerary.dLocker)
                Supernumerary.dCounter++;
            
            return null;
        }
    }
}
