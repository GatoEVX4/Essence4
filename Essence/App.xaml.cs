using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using Essence.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace Essence
{
    public partial class App : Application
    {
        public static StreamWriter _logFile;
        private TextWriter _originalConsoleOut;
        internal MultiTextWriter MultiTextWriterInstance { get; set; }
        internal static StartupWindow StartupWindow;

        private bool error = false;
        internal static MainWindow window;

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (Process.GetProcessesByName("Essence").Length > 1)
            {
                System.Windows.MessageBox.Show("You already have one Essence opened", "Ops!");
                Process.GetCurrentProcess().Kill();
            }
            Console.WriteLine("AppInit");

            this.DispatcherUnhandledException += new System.Windows.Threading.DispatcherUnhandledExceptionEventHandler(App_DispatcherUnhandledException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            TaskScheduler.UnobservedTaskException += new EventHandler<UnobservedTaskExceptionEventArgs>(TaskScheduler_UnobservedTaskException);

            StartupWindow = new StartupWindow();
            StartupWindow.Show();

            PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Critical;
            try
            {
                if (!Directory.Exists("C:\\Essence"))
                    Directory.CreateDirectory("C:\\Essence");

                if (!Directory.Exists("C:\\Essence\\bin"))
                    Directory.CreateDirectory("C:\\Essence\\bin");

                if (!Directory.Exists("C:\\Essence\\userdata"))
                    Directory.CreateDirectory("C:\\Essence\\userdata");

                Environment.CurrentDirectory = "C:\\Essence";
                Directory.SetCurrentDirectory("C:\\Essence");
                //Console.Title = "Essence 2024 © Made with ♥ By m4a1_dev2";

                _originalConsoleOut = Console.Out;

                string logFilePath = "C:\\Essence\\bin\\EssenceLog.txt";
                _logFile = new StreamWriter(logFilePath, append: true, Encoding.UTF8)
                {
                    AutoFlush = true
                };

                MultiTextWriterInstance = new MultiTextWriter(_originalConsoleOut, _logFile);

                window = new MainWindow();
                window.Show();

                try
                {
                    //MultiTextWriterInstance = new MultiTextWriter(_originalConsoleOut, _logFile, new RichTextBoxWriter(window.console_RichTextBox));
                    //Console.SetOut(MultiTextWriterInstance);
                    //Console.SetError(MultiTextWriterInstance);
                }
                catch { }
                
            }
            catch (Exception ex)
            {
                if (ex is not IOException)
                    MessageBox.Show($"Erro ao iniciar o aplicativo: {ex.ToString()}");
            }
        }
        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                if (!error)
                {
                    Console.SetOut(_originalConsoleOut);
                    Console.SetError(_originalConsoleOut);

                    _logFile?.Close();
                    _logFile?.Dispose();
                }
            }
            catch { }
            base.OnExit(e);
        }


        string last_error = "";
        void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            //MessageBox.Show("App_DispatcherUnhandledException called\n\n" + e.Exception.ToString());

            last_error = "App_DispatcherUnhandledException";
            try { HandleErr(e.Exception); } catch { }
            e.Handled = true;
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                //MessageBox.Show("CurrentDomain_UnhandledException called\n\n" + ((Exception)e.ExceptionObject).ToString());

                last_error = "CurrentDomain_UnhandledException";
                try { HandleErr(ex); } catch { }
            }
        }

        void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            //MessageBox.Show("TaskScheduler_UnobservedTaskException called\n\n" + e.Exception.ToString());

            last_error = "TaskScheduler_UnobservedTaskException";
            try { HandleErr(e.Exception); } catch { }

            e.SetObserved();
        }

        internal static bool isUIResponsive;
        private async void HandleErr(Exception ex)
        {
            isUIResponsive = false;
            DateTime startTime = DateTime.Now;

            while ((DateTime.Now - startTime).TotalSeconds < 3)
            {
                if (isUIResponsive)
                    return;
                await Task.Delay(100);
            }
            LogError(ex);
        }

        private async void LogError(Exception ex)
        {
            //this.DispatcherUnhandledException -= App_DispatcherUnhandledException;
            //AppDomain.CurrentDomain.UnhandledException -= CurrentDomain_UnhandledException;
            //TaskScheduler.UnobservedTaskException -= TaskScheduler_UnobservedTaskException;

            error = true;
            string exMessage = "Could not get error message";
            string exToString = "Could not get error information";
            try
            {
                Console.WriteLine("GETTING ERROR INFORMATION [Entire error]...");
                try { exToString = ex.ToString(); } catch (Exception e) { exToString = e.Message; }

                Console.WriteLine("GETTING ERROR INFORMATION [Message]...");
                try { exMessage = ex.Message; } catch (Exception e) { }

                //Console.WriteLine("GETTING LOGS INFORMATION...");
                //string complete_log = "None";
                //try { complete_log = File.ReadAllText("C:\\Essence\\bin\\EssenceLog.txt"); } catch (Exception e) { }

                //Console.WriteLine("SENDING ERROR TO OUR SERVERS...");
                //try { await KeyGay2.Get("erro", KeyGay2.current_user + "user_error:" + "TYPE: " + last_error + "\n\n" + exMessage + "\nfull_error:" + exToString + "\nuser_log:" + complete_log, false, true); Console.WriteLine("ERROR SENDED"); } catch (Exception e) { Console.WriteLine(e.Message); }

            }
            catch { }

            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    Application.Current.MainWindow.Hide();
                }
                catch (Exception e) { }
                Thread thread = new Thread(() =>
                {
                    CriticalWindow criticalWindow = new CriticalWindow(exMessage + "\n\n\n" + exToString);
                    criticalWindow.ShowDialog();
                });
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            });

        }
    }
    //public class RichTextBoxWriter : TextWriter
    //{
    //    private readonly RichTextBox _richTextBox;

    //    public RichTextBoxWriter(RichTextBox richTextBox)
    //    {
    //        _richTextBox = richTextBox;
    //    }

    //    public override Encoding Encoding => Encoding.UTF8;

    //    //public override void Write(char value)
    //    //{
    //    //    _richTextBox.Dispatcher.Invoke(() =>
    //    //    {
    //    //        MainWindow.InternalConsolePrint(value.ToString(), _richTextBox);
    //    //        _richTextBox.ScrollToEnd();
    //    //    });
    //    //}

    //    //public override void WriteLine(string value)
    //    //{
    //    //    _richTextBox.Dispatcher.Invoke(() =>
    //    //    {
    //    //        MainWindow.InternalConsolePrint(value + Environment.NewLine, _richTextBox);
    //    //        _richTextBox.ScrollToEnd();
    //    //    });
    //    //}
    //}

    public class MultiTextWriter : TextWriter
    {
        internal readonly TextWriter[] _writers;

        internal MultiTextWriter(params TextWriter[] writers)
        {
            _writers = writers;
        }

        public override Encoding Encoding => Encoding.UTF8;

        public override void Write(char value)
        {
            try
            {
                foreach (var writer in _writers)
                {
                    writer.Write(value);
                }
            }
            catch { }
        }

        public override void WriteLine(string value)
        {
            try
            {
                foreach (var writer in _writers)
                {
                    writer.WriteLine(value);
                }
            }
            catch { }
        }

        public override void Flush()
        {
            try
            {
                foreach (var writer in _writers)
                {
                    writer.Flush();
                }
            }
            catch { }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var writer in _writers)
                {
                    writer.Dispose();
                }
            }
            base.Dispose(disposing);
        }
    }
}


