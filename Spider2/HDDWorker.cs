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
        static readonly object locker = new object();

        public static string PrepareHDD(Uri uri, Uri siteURI, Uri hardURI, string fileType)
        {
            string SourceDataPath;
            string SourceFolderPath;

            if (uri.Host == siteURI.Host)
            {
                SourceDataPath = hardURI.LocalPath + siteURI.Host + @"\" + uri.AbsolutePath;
                SourceFolderPath = hardURI.LocalPath + siteURI.Host + @"\" + getSegments(uri);
            }
            else
            {
                SourceDataPath = hardURI.LocalPath + siteURI.Host + @"\" + uri.Host + uri.AbsolutePath;
                SourceFolderPath = hardURI.LocalPath + siteURI.Host + @"\" + uri.Host + getSegments(uri);
            }

            char[] charsToTrim = { '/', '\\' };
            while (SourceDataPath[SourceDataPath.Length - 1] == '/' || SourceDataPath[SourceDataPath.Length - 1] == '\\')
            {
                SourceDataPath = SourceDataPath.TrimEnd(charsToTrim);
            }

            if (!Directory.Exists(SourceDataPath))
                Directory.CreateDirectory(SourceDataPath);

            switch (fileType)
            {
                case "image": //SourceDataPath = fileName;
                    break;
                case "text/less": SourceDataPath = SourceDataPath + ".less";
                    break;
                case "text/css": SourceDataPath = SourceDataPath + ".css";
                    break;
                case "application/x-javascript": SourceDataPath = SourceDataPath + ".js";
                    break;
                default: SourceDataPath = SourceDataPath + ".html";
                    break;
            }

            return SourceDataPath;
        }     
        public static string getSegments(Uri uri)
        {
            string result = "";
            for (int i = 0; i < uri.Segments.Count()-1; i++)
                result += uri.Segments[i];
            return result;
        }
    }
}
