using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Spider2Async
{
   // public static enum counter {errors, resources, htmls};
    class Supernumerary
    {
        public static readonly object dLocker = new object();

        public static MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;

        public static CancellationTokenSource cts = new CancellationTokenSource();
        public static CancellationToken token = cts.Token;

        public static StringBuilder downloadedPages = new StringBuilder();
        public static StringBuilder downloadedResources = new StringBuilder();

        public static BlockingCollection<Uri> downloadedUris = new BlockingCollection<Uri>();

        public static string hPath = @"D:\google.com"; 
        public static Uri siteURI = new Uri("http://google.com");

        public static int external_depth = 0;
        public static int depth = 0;

        public static int errors_counter = 0;
        public static int resources_counter = 0;
        public static int html_counter = 0;

        public static void ResetToDefault()
        {
            downloadedPages = new StringBuilder();
            downloadedResources = new StringBuilder();

            errors_counter = 0;
            resources_counter = 0;
            html_counter = 0;

            downloadedUris = new BlockingCollection<Uri>();

            external_depth = 0;
            depth = 0;

            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;
        }
    }
}
