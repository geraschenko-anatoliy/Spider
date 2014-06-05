using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Spider2Async
{
    // Class for all downloading functions
    public class Downloader
    {
        //Recursive func for site downloading
        public async static Task<int> RecursiveDownloading(Uri uri, int depth)
        {
            try
            {
                if (depth == 0)
                {
                    await DownloadHTMLs(uri);

                    return 0;
                }
                if (depth > 0)
                {
                    foreach (var item in await HTMLParser.GetOnlyHtmls(uri))
                    {
                        await RecursiveDownloading(item, depth - 1);

                        if (!Supernumerary.downloadedUris.Contains(uri))
                            await DownloadHTMLs(uri);
                    }
                    return 0;
                }
            }
            catch (System.OperationCanceledException)
            {
                //Downloading canceled
                return -1;
            }
            catch (System.Exception)
            {
                //Downloading failed"
                return -2; // 
            }
            //Downloaded successfully
            return 0; 
        }

        //Downloading HTML
        public async static Task<int> DownloadHTMLs(Uri uri)
        {
            await HTMLParser.ReplaceLinksAndSave(uri);

            IEnumerable<Uri> urlList = await HTMLParser.GetResourses(uri);

            var downloadResoursesTasks = (from temp_uri in urlList select Downloader.DownloadResources(temp_uri)).ToList();

            while (downloadResoursesTasks.Any())
            {
                Task<string> firstFinishedTask = await Task.WhenAny(downloadResoursesTasks);

                downloadResoursesTasks.Remove(firstFinishedTask);
            }
            return 1;
        }

        //Downloading resourses for current HTML
        public async static Task<string> DownloadResources(Uri uri, int repeat = 5)
        {
            string filePath = HTMLParser.GetPath(uri);

            if (!File.Exists(filePath))
            try
            {
                HttpClient client = new HttpClient();
                Exception ex = new Exception();
                bool success = false;
                do
                {
                    await client.GetAsync(uri.AbsoluteUri, Supernumerary.token).ContinueWith(
                        (requestTask) =>
                        {
                            try
                            {
                                HttpResponseMessage response = requestTask.Result;

                                response.EnsureSuccessStatusCode();

                                response.Content.ReadAsFileAsync(filePath, true);

                                Supernumerary.downloadedResources = Supernumerary.downloadedResources.AppendLine(uri.AbsoluteUri);

                                Interlocked.Increment(ref Supernumerary.resources_counter);

                                Supernumerary.downloadedUris.Add(uri);

                                success = true;
                            }
                            catch (Exception ex1)
                            {
                                ex = ex1;
                                repeat--;
                            }

                        }, Supernumerary.token);
                } while (repeat > 0 && !success);

                if (repeat == 0)
                {
                    Messenger.SentErrorMessage(ex, uri);
                    Supernumerary.downloadedUris.Add(uri);
                }
            }
            catch (TaskCanceledException)
            {
                Supernumerary.downloadedResources.AppendLine("Canceled");
            }
            catch (Exception ex)
            {
                Messenger.SentErrorMessage(ex, uri);
            }
            return null;
        }
    
    }    
}