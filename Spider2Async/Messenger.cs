using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Spider2Async
{
    public static class Messenger
    {
        public static DispatcherTimer dispatcherTimer = new DispatcherTimer();
        public static void StartDispatcher()
        {
            Messenger.dispatcherTimer.Tick += new EventHandler(Messenger.dispatcherTimer_Tick);
            Messenger.dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            Messenger.dispatcherTimer.Start();
        }
        public static void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            lock (Supernumerary.downloadedPages)
            {
                Supernumerary.mainWindow.LinksTB.Text = Supernumerary.downloadedPages + Supernumerary.mainWindow.LinksTB.Text;
                Supernumerary.mainWindow.ResoursesTB.Text = Supernumerary.downloadedResources + Supernumerary.mainWindow.ResoursesTB.Text;
                
                Supernumerary.mainWindow.nLinksTB.Text = Supernumerary.html_counter.ToString();
                Supernumerary.mainWindow.nErrorsTB.Text = Supernumerary.errors_counter.ToString();
                Supernumerary.mainWindow.nSourceTB.Text = Supernumerary.resources_counter.ToString();

                Supernumerary.downloadedPages.Clear();
                Supernumerary.downloadedResources.Clear();
            }
        }
        public static void SentErrorMessage(Exception ex, Uri temp)
        {
            Interlocked.Increment(ref Supernumerary.errors_counter);
            try
            {
                Supernumerary.mainWindow.ErrorsTB.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, (ThreadStart)delegate
                {
                    Supernumerary.mainWindow.ErrorsTB.Text = temp.AbsoluteUri + " " + ex.Message + Environment.NewLine + Supernumerary.mainWindow.ErrorsTB.Text;
                });
            }
            catch (NullReferenceException null_ex)
            {
                MessageBox.Show(null_ex.Message);
            }
            catch (Exception main_ex)
            {
                MessageBox.Show(main_ex.Message);
            }
        }
    }
}