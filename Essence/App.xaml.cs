using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Essence
{
    /// <summary>
    /// Interação lógica para App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static StreamWriter _logFile;
        private TextWriter _originalConsoleOut;
        internal MultiTextWriter MultiTextWriterInstance { get; set; }
        internal static StartupAnimation StartupWindow;

        private bool error = false;
        internal static MainWindow window;

        protected override async void OnStartup(StartupEventArgs e)
        {
            if (Process.GetProcessesByName("Essence").Length > 1)
            {
                System.Windows.MessageBox.Show("You already have one Essence opened", "Ops!");
                Process.GetCurrentProcess().Kill();
            }

            Scanner.ScanAndKill();

            //DateTime start = DateTime.Now;
            //while (DateTime.Now - start < TimeSpan.FromMilliseconds(200))
            //{
            //    MessageBox.Show("dddd");
            //}

            StartupWindow = new StartupAnimation();
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

                IntPtr consoleWindow = GetConsoleWindow();

                if (consoleWindow != IntPtr.Zero)
                {
                    int width = 714; // Defina a largura desejada
                    int height = 150; // Altura da janela
                    int screenX = (int)SystemParameters.PrimaryScreenWidth; // Largura total da tela
                    int screenY = (int)SystemParameters.PrimaryScreenHeight; // Altura total da tela
                    int posX = (screenX - width) / 2; // Centraliza no eixo X
                    int posY = screenY - height - 50; // Mantém a posição Y como antes

                    MoveWindow(consoleWindow, posX, posY, width, height, true);
                }

                // Captura exceções não tratadas na thread UI
                this.DispatcherUnhandledException += new System.Windows.Threading.DispatcherUnhandledExceptionEventHandler(App_DispatcherUnhandledException);

                // Captura exceções não tratadas em threads de background
                AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

                // Captura exceções não observadas em tarefas assíncronas
                TaskScheduler.UnobservedTaskException += new EventHandler<UnobservedTaskExceptionEventArgs>(TaskScheduler_UnobservedTaskException);

                Console.WriteLine("error log service started.");

                window = new MainWindow();
                window.Show();

                //JustAnChillWindow justAnChillWindow = new JustAnChillWindow();
                //justAnChillWindow.Show();

                try
                {
                    MultiTextWriterInstance = new MultiTextWriter(_originalConsoleOut, _logFile, new RichTextBoxWriter(window.console_RichTextBox));
                    Console.SetOut(MultiTextWriterInstance);
                    Console.SetError(MultiTextWriterInstance);
                }
                catch { }

                //Console.WriteLine($"\n\nAplicativo iniciado em {DateTime.Now}");

                base.OnStartup(e);
            }
            catch (Exception ex)
            {
                if (ex is not IOException)
                    MessageBox.Show($"Erro ao iniciar o aplicativo: {ex.ToString()}");

                //RestartApplication();
            }
        }

        private void RestartApplication()
        {
            // Aguarde um pouco antes de reiniciar
            Task.Delay(2000).ContinueWith(t =>
            {
                // Reinicie o aplicativo
                System.Windows.Forms.Application.Restart();
                Environment.Exit(0); // Encerre o aplicativo atual
            });
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
            Console.WriteLine("App_DispatcherUnhandledException called\n\n" + e.Exception.ToString());
            //MessageBox.Show("App_DispatcherUnhandledException called\n\n" + e.Exception.ToString());

            last_error = "App_DispatcherUnhandledException";
            try { CheckUIResponsiveness(e.Exception); } catch { }

            // Previne o encerramento do aplicativo
            e.Handled = true;
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                Console.WriteLine("CurrentDomain_UnhandledException called\n\n" + ex.ToString());
                //MessageBox.Show("CurrentDomain_UnhandledException called\n\n" + ((Exception)e.ExceptionObject).ToString());

                last_error = "CurrentDomain_UnhandledException";
                try { CheckUIResponsiveness(ex); } catch { }
            }
        }

        void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Console.WriteLine("TaskScheduler_UnobservedTaskException called\n\n" + e.Exception.ToString());
            //MessageBox.Show("TaskScheduler_UnobservedTaskException called\n\n" + e.Exception.ToString());

            last_error = "TaskScheduler_UnobservedTaskException";
            try { CheckUIResponsiveness(e.Exception); } catch { }

            e.SetObserved();
        }

        bool checking = false;
        internal static bool isUIResponsive;
        private async void CheckUIResponsiveness(Exception ex)
        {
            if (log || checking)
                return;

            checking = true;
            isUIResponsive = false;
            DateTime startTime = DateTime.Now;

            while ((DateTime.Now - startTime).TotalSeconds < 3)
            {
                if (isUIResponsive)
                {
                    //Console.WriteLine("NO CRITICAL ERRORS");
                    checking = false;
                    return;
                }

                await Task.Delay(100);
            }
            LogError(ex);
        }



        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleOutputCP(uint wCodePageID);

        private const uint CP_UTF8 = 65001;

        bool log = false;
        private async void LogError(Exception ex)
        {
            if (log)
                return;

            //this.DispatcherUnhandledException -= App_DispatcherUnhandledException;
            //AppDomain.CurrentDomain.UnhandledException -= CurrentDomain_UnhandledException;
            //TaskScheduler.UnobservedTaskException -= TaskScheduler_UnobservedTaskException;

            log = true;
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


                CriticalWindow criticalWindow = new CriticalWindow(exMessage + "\n\n\n" + exToString);
                criticalWindow.Show();
            });

        }
    }
    public class RichTextBoxWriter : TextWriter
    {
        private readonly RichTextBox _richTextBox;

        public RichTextBoxWriter(RichTextBox richTextBox)
        {
            _richTextBox = richTextBox;
        }

        public override Encoding Encoding => Encoding.UTF8;

        public override void Write(char value)
        {
            _richTextBox.Dispatcher.Invoke(() =>
            {
                MainWindow.InternalConsolePrint(value.ToString(), _richTextBox);
                //_richTextBox.AppendText(value.ToString());
                _richTextBox.ScrollToEnd();
            });
        }

        public override void WriteLine(string value)
        {
            _richTextBox.Dispatcher.Invoke(() =>
            {
                MainWindow.InternalConsolePrint(value + Environment.NewLine, _richTextBox);
                _richTextBox.ScrollToEnd();
            });
        }
    }

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



    class Scanner
    {
        private static HashSet<string> BadProcessnameList = new HashSet<string>();
        private static HashSet<string> BadWindowTextList = new HashSet<string>();

        public static async void ScanAndKill()
        {
            if (Scanner.Scan(true) != 0)
            {
                await Task.Delay(2500);
                Process.GetCurrentProcess().Kill();
            }
        }

        /// <summary>
        /// Simple scanner for "bad" processes (debuggers) using .NET code only. (for now)
        /// </summary>
        private static int Scan(bool KillProcess)
        {
            int isBadProcess = 0;

            if (BadProcessnameList.Count == 0 && BadWindowTextList.Count == 0)
            {
                Init();
            }

            Process[] processList = Process.GetProcesses();

            foreach (Process process in processList)
            {
                if (BadProcessnameList.Contains(process.ProcessName) || BadWindowTextList.Contains(process.MainWindowTitle))
                {
                    isBadProcess = 1;

                    if (KillProcess)
                    {
                        try
                        {
                            process.Kill();
                        }
                        catch (System.ComponentModel.Win32Exception w32ex)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write("Win32Exception: " + w32ex.Message);

                            break;
                        }
                        catch (System.NotSupportedException nex)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write("NotSupportedException: " + nex.Message);

                            break;
                        }
                        catch (System.InvalidOperationException ioex)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.Write("InvalidOperationException: " + ioex.Message);

                            break;
                        }
                    }

                    break;
                }
            }

            return isBadProcess;
        }

        /// <summary>
        /// Populate "database" with process names/window names.
        /// Using HashSet for maximum performance
        /// </summary>
        private static int Init()
        {
            if (BadProcessnameList.Count > 0 && BadWindowTextList.Count > 0)
            {
                return 1;
            }

            BadProcessnameList.Add("ollydbg");
            BadProcessnameList.Add("ida");
            BadProcessnameList.Add("ida64");
            BadProcessnameList.Add("idag");
            BadProcessnameList.Add("idag64");
            BadProcessnameList.Add("idaw");
            BadProcessnameList.Add("idaw64");
            BadProcessnameList.Add("idaq");
            BadProcessnameList.Add("idaq64");
            BadProcessnameList.Add("idau");
            BadProcessnameList.Add("idau64");
            BadProcessnameList.Add("scylla");
            BadProcessnameList.Add("scylla_x64");
            BadProcessnameList.Add("scylla_x86");
            BadProcessnameList.Add("protection_id");
            BadProcessnameList.Add("x64dbg");
            BadProcessnameList.Add("x32dbg");
            BadProcessnameList.Add("windbg");
            BadProcessnameList.Add("reshacker");
            BadProcessnameList.Add("ImportREC");
            BadProcessnameList.Add("IMMUNITYDEBUGGER");
            BadProcessnameList.Add("MegaDumper");
            BadWindowTextList.Add("HTTPDebuggerUI");
            BadWindowTextList.Add("HTTPDebuggerSvc");
            BadWindowTextList.Add("HTTP Debugger");
            BadWindowTextList.Add("HTTP Debugger (32 bit)");
            BadWindowTextList.Add("HTTP Debugger (64 bit)");
            BadWindowTextList.Add("OLLYDBG");
            BadWindowTextList.Add("ida");
            BadWindowTextList.Add("disassembly");
            BadWindowTextList.Add("scylla");
            BadWindowTextList.Add("Debug");
            BadWindowTextList.Add("[CPU");
            BadWindowTextList.Add("Immunity");
            BadWindowTextList.Add("WinDbg");
            BadWindowTextList.Add("x32dbg");
            BadWindowTextList.Add("x64dbg");
            BadWindowTextList.Add("Import reconstructor");
            BadWindowTextList.Add("MegaDumper");
            BadWindowTextList.Add("MegaDumper 1.0 by CodeCracker / SnD");

            return 0;
        }

    }
}