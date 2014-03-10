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
    public class Downloader
    {
        public static Exception ex;
        public async static Task<string> DownloadResources(Uri uri, int repeat = 5)
        {
            string filePath = HTMLParser.GetPath(uri);

            try
            {
                HttpClient client = new HttpClient();

                bool success = false;
                do
                {
                    // Send asynchronous request
                    await client.GetAsync(uri.AbsoluteUri, Supernumerary.token).ContinueWith(
                        (requestTask) =>
                        {
                            // Get HTTP response from completed task.
                            try
                            {
                                HttpResponseMessage response = requestTask.Result;

                                // Check that response was successful or throw exception
                                response.EnsureSuccessStatusCode();

                                // Read response asynchronously and save to file
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
                Supernumerary.downloadedResources.AppendLine("Ошибка");
            }
            //catch (System.OperationCanceledException)
            //{
            //    Supernumerary.downloadedPages.AppendLine("Downloading canceled");
            //    Supernumerary.downloadedResources.AppendLine("Downloading canceled");
            //}
            catch (Exception ex)
            {
                Messenger.SentErrorMessage(ex, uri);
            }
            return null;
        }
        public async static Task<int> RecursiveDownloading(Uri uri, int depth)
        {
            try
            {
                if (depth == 0)
                {
                    await DownloadHTMLs(uri);

                    return -1;
                }
                if (depth > 0)
                {
                    foreach (var item in await HTMLParser.GetOnlyHtmls(uri))
                    {
                        await RecursiveDownloading(item, depth - 1);

                        if (!Supernumerary.downloadedUris.Contains(uri))
                            await DownloadHTMLs(uri);
                    }
                    return depth - 1;
                }
            }
            catch (System.OperationCanceledException)
            {
                //MessageBox.Show("Downloading canceled");
                return -2;
            }
            catch (System.Exception)
            {
                //MessageBox.Show("Downloading failed");
                return -1; // 
            }
            //MessageBox.Show("Downloaded successfully");
            return -2; 
        }
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
    }    
}