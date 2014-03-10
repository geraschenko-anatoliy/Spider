using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Spider2Async
{
    class HTMLParser
    {       
        public static async Task<HashSet<Uri>> GetOnlyHtmls(Uri uri)
        {
            HashSet<Uri> result = new HashSet<Uri>();

            if (uri.Host != Supernumerary.siteURI.Host)
                return result;

            await Task.Run(async () =>
                  {
                      try
                      {
                          HtmlDocument doc = new HtmlDocument();
                          HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);

                          HttpWebResponse response = (HttpWebResponse)(await request.GetResponseAsync());
                          doc = new HtmlDocument();
                          doc.Load(response.GetResponseStream());
                          
                          var htmls = doc.DocumentNode.SelectNodes("//a[@href]")
                              .Select(e1 => e1.GetAttributeValue("href", null))
                              .Where(s => !String.IsNullOrEmpty(s));                   

                          result = LinksToHashSet(htmls.ToList(), "//a[@href]", true);

                          ReplaceLinksAndSave(uri, doc);
                      }
                      catch (TaskCanceledException)
                      {
                          Supernumerary.downloadedResources.AppendLine("Ошибка");
                      }
                      catch (Exception ex)
                      {
                          Messenger.SentErrorMessage(ex, uri);
                      }
                  }, Supernumerary.token);
            return result;
        }
        public static async Task<HashSet<Uri>> GetResourses(Uri uri)
        {
            
            HashSet<Uri> result = new HashSet<Uri>();
            await Task.Run(async () =>
                   {
                       try
                       {
                           Supernumerary.token.ThrowIfCancellationRequested();
                           
                           HtmlDocument doc = new HtmlDocument();
                           HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);

                           HttpWebResponse response = (HttpWebResponse)(await request.GetResponseAsync());
                           doc = new HtmlDocument();
                           doc.Load(response.GetResponseStream());

                           var images = doc.DocumentNode.Descendants("img")
                               .Select(e1 => e1.GetAttributeValue("src", null))
                               .Where(s => !String.IsNullOrEmpty(s));

                           result.UnionWith(LinksToHashSet(images.ToList(), "img", false));

                           var css = doc.DocumentNode.Descendants("link")
                               .Select(e1 => e1.GetAttributeValue("href", null))
                               .Where(s => !String.IsNullOrEmpty(s));

                           result.UnionWith(LinksToHashSet(css.ToList(), "link", false));

                           var scripts = doc.DocumentNode.Descendants("script")
                               .Select(e1 => e1.GetAttributeValue("src", null))
                               .Where(s => !String.IsNullOrEmpty(s));

                           result.UnionWith(LinksToHashSet(scripts.ToList(), "script", false));
                       }
                       catch (TaskCanceledException)
                       {
                           Supernumerary.downloadedResources.AppendLine("Отменено");
                       }
                       catch (Exception ex)
                       {
                           Messenger.SentErrorMessage(ex, uri);
                       }
                   }, Supernumerary.token);

            return result;
        }
        public static async Task<string> ReplaceLinksAndSave(Uri uri)
        {
            await Task.Run(async () =>
            {
                try
                {
                    HtmlDocument doc = new HtmlDocument();
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);

                    HttpWebResponse response = (HttpWebResponse)(await request.GetResponseAsync());
                    doc = new HtmlDocument();
                    doc.Load(response.GetResponseStream());

                    string path = GetPath(uri);

                    var xhtmls = doc.DocumentNode.SelectNodes("//a[@href]");
                    foreach (var html in xhtmls)
                    {
                        Uri uri1 = new Uri(Supernumerary.siteURI, html.GetAttributeValue("href", null));

                        if (uri1.Host == Supernumerary.siteURI.Host)
                        {
                            string temp_path = GetPath(uri1, true) + ".html";
                            html.SetAttributeValue("href", temp_path);
                        }
                    }

                    var images = doc.DocumentNode.Descendants("img");

                    foreach (var img in images)
                    {
                        Uri uri1 = new Uri(Supernumerary.siteURI, img.GetAttributeValue("src", null));
                        img.SetAttributeValue("src", GetPath(uri1, false));
                    }

                    var csses = doc.DocumentNode.Descendants("link");

                    foreach (var css in csses)
                    {
                        Uri uri1 = new Uri(Supernumerary.siteURI, css.GetAttributeValue("href", null));
                        css.SetAttributeValue("href", GetPath(uri1, false));
                    }

                    var scripts = doc.DocumentNode.Descendants("script");

                    foreach (var script in scripts)
                    {
                        Uri uri1 = new Uri(Supernumerary.siteURI, script.GetAttributeValue("src", null));
                        script.SetAttributeValue("src", GetPath(uri1, false));
                    }

                    if (!File.Exists(path + ".html"))
                    {
                        doc.Save(path + ".html", Encoding.Default);
                        Supernumerary.downloadedUris.Add(uri);
                        Supernumerary.downloadedPages = Supernumerary.downloadedPages.AppendLine(uri.AbsoluteUri);
                        Interlocked.Increment(ref Supernumerary.html_counter);
                    }
                }
                catch (TaskCanceledException)
                {
                    Supernumerary.downloadedResources.AppendLine("Ошибка");
                }
                catch (Exception ex)
                {
                    Messenger.SentErrorMessage(ex, uri);
                }
            }, Supernumerary.token );

            return uri.AbsoluteUri;
        }
        public static void ReplaceLinksAndSave(Uri uri, HtmlDocument doc)
        {
            try
            {
                var xhtmls = doc.DocumentNode.SelectNodes("//a[@href]");
                string path = GetPath(uri);

                foreach (var html in xhtmls)
                {
                    Uri uri1 = new Uri(Supernumerary.siteURI, html.GetAttributeValue("href", null));

                    html.SetAttributeValue("href", GetPath(uri1, false) + ".html");
                }

                var images = doc.DocumentNode.Descendants("img");

                foreach (var img in images)
                {
                    Uri uri1 = new Uri(Supernumerary.siteURI, img.GetAttributeValue("src", null));
                    img.SetAttributeValue("src", GetPath(uri1, false));
                }

                var csses = doc.DocumentNode.Descendants("link");

                foreach (var css in csses)
                {
                    Uri uri1 = new Uri(Supernumerary.siteURI, css.GetAttributeValue("href", null));
                    css.SetAttributeValue("href", GetPath(uri1, false));
                }

                var scripts = doc.DocumentNode.Descendants("script");

                foreach (var script in scripts)
                {
                    Uri uri1 = new Uri(Supernumerary.siteURI, script.GetAttributeValue("src", null));
                    script.SetAttributeValue("src", GetPath(uri1, false));
                }

                if (!File.Exists(path + ".html"))
                {
                    doc.Save(path + ".html", Encoding.Default);
                    Supernumerary.downloadedUris.Add(uri);
                    Supernumerary.downloadedPages = Supernumerary.downloadedPages.AppendLine(uri.AbsoluteUri);
                    Interlocked.Increment(ref Supernumerary.html_counter);
                }

            }
            catch (Exception ex)
            {
                Messenger.SentErrorMessage(ex, uri);
            }
        }
        public static HashSet<Uri> LinksToHashSet(List<string> links, string nodeType, bool onlyHTMLs)
        { 
            HashSet<Uri> result = new HashSet<Uri>();
            try
            {
                if (links != null)
                    for (int i = 0; i < links.Count; i++)
                    {
                        Uri temp = null;
                        try
                        {
                            temp = new Uri(Supernumerary.siteURI, links[i]);
                            if (!Supernumerary.downloadedUris.Contains(temp) && !result.Contains(temp))// && temp.Host == Supernumerary.siteURI.Host)
                            {
                                //option for html files
                                if (nodeType == "//a[href]")
                                    if (temp.Host == Supernumerary.siteURI.Host)
                                    {
                                        result.Add(temp);
                                    }
                                    else 
                                    {
                                        //Options for external links
                                        if (Supernumerary.external_depth > 0)
                                            result.Add(temp);
                                    }
                                //option for sourse files
                                else
                                {
                                    result.Add(temp);
                                }
                            }
                        }
                        catch (UriFormatException u_ex)
                        {
                            Messenger.SentErrorMessage(u_ex, temp);
                        }
                        catch (Exception ex)
                        {
                            Messenger.SentErrorMessage(ex, temp);
                        }
                    }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return result;
        }
        static string ReplaceIllegalChars(string path)
        {
            return path.Replace('?', '_').Replace(',', '_').Replace(':', '_');
        }
        public static string GetPath(Uri uri, bool onlypath = false)
        {
            string filePath = "";
            string prerarePath = uri.AbsoluteUri.Replace('?','_');

            try
            {
                if (uri.Host == Supernumerary.siteURI.Host)
                {
                    prerarePath = ReplaceIllegalChars(uri.LocalPath).TrimStart('/').TrimEnd('/');
                    filePath = Path.GetFullPath(Path.Combine(Supernumerary.hPath, prerarePath));
                }
                else
                {
                    prerarePath = ReplaceIllegalChars(uri.AbsoluteUri.Replace("http://", "")).TrimEnd('/');
                    filePath = Path.GetFullPath(Path.Combine(Supernumerary.hPath, prerarePath));
                }

                if (!onlypath)
                    if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            }
            catch (NotSupportedException)
            { 
                // catching non-supported exceptions
            }
            catch (Exception ex)
            {
                Messenger.SentErrorMessage(ex, uri);
            }
            return filePath;
        }
    }
}