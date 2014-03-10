using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Spider2
{
            
    public class Supernumerary
    {   
        public static readonly object dLocker = new object();

        public static int dCounter = 0;
        //public static HashSet<string> downloadedUris = new HashSet<string>();
        //public static HashSet<string> hsHTML = new HashSet<string>();
        public static Uri siteURI = new Uri("http://terrikon.com");
        public static Uri hardURI = new Uri("D:/");
        //public static HashSet<string> HsHTML 
        //{ 
        //    get
        //    {
        //        return hsHTML;
        //    }
        //    set
        //    {
        //        hsHTML = value;
        //    } 
        //}

        public void OnHTMLChanged()
        { 
            
        }

        //public static HashSet<string> errors = new HashSet<string>();
        //public static string lastItem = "";

        //public static BlockingCollection<string> dCol = new BlockingCollection<string>();

        //public HashSet<Uri> hsImage = new HashSet<Uri>();
        //public HashSet<Uri> hsJS = new HashSet<Uri>();
        //public HashSet<Uri> hsCSS = new HashSet<Uri>();
        //public HashSet<Uri> hsLess = new HashSet<Uri>();

        //HashSet<string> downloadedLinks;
        //List<string> lParsedLinks;
        //List<string> lErrorList;
        //int nParsedLinks;
        //int nDownloadedSources;
        //string currentParsingLink;
        //List<string> currentResoursesInDownloading;
    }
}
