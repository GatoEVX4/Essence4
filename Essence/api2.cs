﻿using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using static System.Net.Mime.MediaTypeNames;

namespace Essence
{
    public class api2
    {
        //public static string workspace = "C:\\Essence\\workspace";
        public static string workspace = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Essence", "workspace");
        
        public enum InjectionResult
        {
            Failed,
            Success,
            HomeAt
        }

        public static async void CloseInjectors(bool force = false)
        {
            if (Process.GetProcessesByName("Injector").Length > 0)
            {
                if (!force && await TestExecution() == true)
                    return;

                foreach (Process p in Process.GetProcessesByName("Injector"))
                {
                    p.Kill();
                }
            }
        }


        private static Random random = new Random();

        internal static async Task SendScript(string script)
        {
            try
            {
                using TcpClient tcpClient = new TcpClient();
                tcpClient.Connect("localhost", 5235);
                using NetworkStream networkStream = tcpClient.GetStream();
                byte[] bytes = Encoding.UTF8.GetBytes(script);
                networkStream.Write(bytes, 0, bytes.Length);
                networkStream.Flush();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        internal static async Task<bool> TestExecution(int max = 0)
        {
            if (Process.GetProcessesByName("Injector").Length <= 0 || Process.GetProcessesByName("RobloxPlayerBeta").Length <= 0)
            {
                Console.WriteLine("Injector is Closed. Test failed");
                api2.CloseInjectors(true);
                return false;
            }

            Random r = new Random();
            int randomnumber = r.Next(10000000, 100000000);

            if (max == 0)
                max = 10;

            Process[] processes = Process.GetProcessesByName("RobloxPlayerBeta");

            for (int i = 0; i < max; i++)
            {
                Task.Run(async () =>
                {
                    SendScript($"writefile('{randomnumber}.check', 'nice')");
                });
                await Task.Delay(200);

                if (File.Exists(api2.workspace + $"\\{randomnumber}.check"))
                {
                    await Task.Delay(100);
                    try { File.Delete(api2.workspace + $"\\{randomnumber}.check"); } catch { }
                    return true;
                }
                else
                {
                    foreach (var process in processes)
                    {
                        try
                        {
                            if (File.Exists(Path.GetDirectoryName(process.MainModule.FileName) + $"\\workspace\\{randomnumber}.check"))
                            {
                                await Task.Delay(100);
                                try { File.Delete(Path.GetDirectoryName(process.MainModule.FileName) + $"\\workspace\\{randomnumber}.check"); } catch { }
                                return true;
                            }
                        }
                        catch
                        {

                        }
                    }
                }
            }
            api2.CloseInjectors(true);
            return false;
        }

        internal static async Task<InjectionResult> Inject()
        {
            try
            {
                await Task.Run(() =>
                {
                    Process.Start(new ProcessStartInfo()
                    {
                        FileName = "C:\\Essence\\Injector.exe",
                        WorkingDirectory = "C:\\Essence",
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden
                    });
                });

                await Task.Delay(2000);
                if (await TestExecution(20))
                    return InjectionResult.Success;

                //foreach (Process p in Process.GetProcessesByName("RobloxPlayerBeta"))
                //{
                //    p.Kill();
                //}
            }
            catch { }
            return InjectionResult.Failed;
        }
    }
}