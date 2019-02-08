using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace bossdoyKaraoke_NOW
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //credit to: natelowry/DynamicSplashScreen https://github.com/natelowry/DynamicSplashScreen

        public static ISplashScreen SplashScreen;

        private ManualResetEvent ResetSplashCreated;
        private Thread SplashThread;
        protected override void OnStartup(StartupEventArgs e)
        {
            // ManualResetEvent acts as a block.  It waits for a signal to be set.
            ResetSplashCreated = new ManualResetEvent(false);

            // Create a new thread for the splash screen to run on
            SplashThread = new Thread(ShowSplash);
            SplashThread.SetApartmentState(ApartmentState.STA);
            SplashThread.IsBackground = true;
            SplashThread.Name = "bossdoyKaraoke_NOW";
            SplashThread.Start();

            // Wait for the blocker to be signaled before continuing. This is essentially the same as: while(ResetSplashCreated.NotSet) {}
            ResetSplashCreated.WaitOne();
            base.OnStartup(e);
            ShutdownMode = ShutdownMode.OnMainWindowClose;
        }

        private void ShowSplash()
        {
            // Create the window
            SplashScreen splashScreenWindow = new SplashScreen();
            SplashScreen = splashScreenWindow;

            // Show it
            splashScreenWindow.Show();

            // Now that the window is created, allow the rest of the startup to run
            ResetSplashCreated.Set();
            System.Windows.Threading.Dispatcher.Run();
        }
    }
}
