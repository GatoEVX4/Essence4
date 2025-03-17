using EssenceUpdater;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace EssenceUpdater
{
    /// <summary>
    /// Interação lógica para App.xaml
    /// </summary>
    public partial class App : Application
    {
        [STAThread]
        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            DispatcherUnhandledException += App_DispatcherUnhandledException;

            base.OnStartup(e);

            var args = Environment.GetCommandLineArgs();
            bool isUpdate = Array.Exists(args, arg => arg.Equals("--update", StringComparison.OrdinalIgnoreCase));

            if (isUpdate)
            {
                UpdateWindow updateWindow = new UpdateWindow();
                updateWindow.Show();
            }
            else
            {
                InstallWindow installWindow = new InstallWindow();
                installWindow.Show();
            }
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                HandleException(ex);
            }
        }
        
        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            HandleException(e.Exception);
            e.Handled = true;
        }


        internal static bool isUIResponsive;
        private async void HandleException(Exception ex)
        {
            LogError(ex);

            if (await IsCriticalError())
                ShowErrorWindow(ex);
        }

        private async Task<bool> IsCriticalError()
        {
            Thread.Sleep(100);

            isUIResponsive = false;
            DateTime startTime = DateTime.Now;

            while ((DateTime.Now - startTime).TotalSeconds < 3)
            {
                if (isUIResponsive)
                {
                    return false;
                }

                await Task.Delay(100);
            }
            return true;
        }

        private void LogError(Exception ex)
        {
            try
            {
                string logFilePath = "error_log.txt"; // Caminho do arquivo de log
                string errorMessage = $"[{DateTime.Now}] - {ex.GetType()} - {ex.Message}\n{ex.StackTrace}\n\n";
                File.AppendAllText(logFilePath, errorMessage);

                Console.WriteLine(errorMessage);
            }
            catch (Exception logEx)
            {
                Console.WriteLine("Erro ao gravar o log de erro: " + logEx.Message);
            }
        }

        private void ShowErrorWindow(Exception ex)
        {
            string displayMessage = $"Ocorreu um erro crítico: {ex.Message}\n\n{ex.StackTrace}";
            Application.Current.Dispatcher.Invoke(() =>
            {
                ErrorWindow errorWindow = new ErrorWindow(displayMessage);
                errorWindow.ShowDialog();
            });
        }
    }
}
