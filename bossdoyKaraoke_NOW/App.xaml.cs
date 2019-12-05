extern alias NLog4;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using bossdoyKaraoke_NOW.Media;
using NLog4.NLog;
using NLog4.NLog.Config;
using NLog4.NLog.Targets;

namespace bossdoyKaraoke_NOW
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //credit to: natelowry/DynamicSplashScreen https://github.com/natelowry/DynamicSplashScreen

        public static ISplashScreen SplashScreen;

        private static Logger _logger = LogManager.GetCurrentClassLogger();
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
            SplashThread.Name = "BossdoyKaraoke_NOW";
            SplashThread.Start();

            // Wait for the blocker to be signaled before continuing. This is essentially the same as: while(ResetSplashCreated.NotSet) {}
            ResetSplashCreated.WaitOne();
            base.OnStartup(e);

            SetupExceptionHandling();
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

        private void SetupExceptionHandling()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                LogUnhandledException((Exception)e.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");

            DispatcherUnhandledException += (s, e) =>
                LogUnhandledException(e.Exception, "Application.Current.DispatcherUnhandledException");

            TaskScheduler.UnobservedTaskException += (s, e) =>
                LogUnhandledException(e.Exception, "TaskScheduler.UnobservedTaskException");
        }

        private void LogUnhandledException(Exception exception, string source)
        {
            var config = new LoggingConfiguration();
            var fileTarget = new FileTarget();
            var message = $"Unhandled exception ({source})";

            config.AddTarget("Logs", fileTarget);
            fileTarget.FileName = PlayerBase.FilePath + @"logs\nlog-${shortdate}.txt";
            fileTarget.Layout = "Exception Type: ${exception:format=Type}${newline} Target Site:  ${event-context:TargetSite}${newline} Message: ${message}";

            var loggingRule = new LoggingRule("*", LogLevel.Trace, fileTarget);
            config.LoggingRules.Add(loggingRule);

            LogManager.Configuration = config;

            LogEventInfo eventInfo = new LogEventInfo(LogLevel.Trace, "", _logger.Name);
            eventInfo.Properties["TargetSite"] = exception.TargetSite;
            eventInfo.Exception = exception;
            _logger.Log(eventInfo);

            //try
            //{
            //    System.Reflection.AssemblyName assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName();
            //    message = string.Format("Unhandled exception in {0} v{1}", assemblyName.Name, assemblyName.Version);
            //}
            //catch (Exception ex)
            //{
            //    _logger.Error(ex, "Exception in LogUnhandledException");
            //}
            //finally
            //{
            //    _logger.Error(exception, message);
            //}
        }
    }
}
