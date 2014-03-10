using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Spider2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Supernumerary.siteURI = new Uri("http://terrikon.com");
            Funk();
        }

        public async void Funk()
        { 
            Uri nu = new Uri("http://terrikon.com");
            Directory.CreateDirectory(Supernumerary.hardURI.LocalPath + nu.Host);
            //Supernumerary.dCol.Add(nu.AbsoluteUri);
            Uri k = await HTMLParser.DownloadSourceReturnHTMLs(nu, 2);
            MessageBox.Show("k = " + k);
        }


    }
}
