using Spider2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spider2
{
    public class HDDWorker
    {
        public static string PrepareHDD(Uri uri, Uri siteURI, Uri hardURI, string fileType)
        {
            string SourceDataPath;
            string SourceFolderPath;
            if (uri.Host == siteURI.Host)
            {
                //if (uri.IsFile == true)
                    SourceFolderPath = hardURI.LocalPath + siteURI.Host + @"\" + uri.AbsolutePath.Replace(uri.Segments[uri.Segments.Count() - 1], "");
                //else
                //    SourceFolderPath = hardURI.LocalPath + siteURI.Host + @"\" + uri.AbsolutePath;
         
                SourceDataPath = hardURI.LocalPath + siteURI.Host + @"\" + uri.AbsolutePath;
            }
            else
            {
                SourceFolderPath = hardURI.LocalPath + siteURI.Host + @"\" + uri.Host + uri.AbsolutePath.Replace(uri.Segments[uri.Segments.Count() - 1], "");
                SourceDataPath = hardURI.LocalPath + siteURI.Host + @"\" + uri.Host + uri.AbsolutePath;
            }

            
            char[] charsToTrim = { '/', '\\'};
            while(SourceDataPath[SourceDataPath.Length-1] == '/' || SourceDataPath[SourceDataPath.Length-1] == '\\')
            {
                SourceDataPath = SourceDataPath.TrimEnd(charsToTrim);
            }
            


            if (!Directory.Exists(SourceFolderPath))
            {
                Directory.CreateDirectory(SourceFolderPath);
            }

            switch (fileType)
            {
                case "image": //SourceDataPath = fileName;
                    break;
                //case "text/less": filePath = fileName + ".less";
                //    break;
                //case "text/css": filePath = fileName + ".css";
                //    break;
                //case "application/x-javascript": filePath = fileName + ".js";
                //    break;
                default: SourceDataPath = SourceDataPath + ".html";
                    break;
            }

            return SourceDataPath;
        }
    }
}
