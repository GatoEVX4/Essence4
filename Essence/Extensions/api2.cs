using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace Essence
{
    public class api2
    {
        public static string workspace = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Essence", "workspace");
        
        public enum InjectionResult
        {
            Failed,
            Success
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


        [DllImport("Auth.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Execute(string src);

        internal static async Task<bool> TestExecution(int max = 0)
        {
            if (Process.GetProcessesByName("RobloxPlayerBeta").Length <= 0)
            {
                Console.WriteLine("Roblox closed. Test failed");
                api2.CloseInjectors(true);
                return false;
            }

            int num = new Random().Next(10000000, 100000000);

            if (max == 0)
                max = 10;

            for (int i = 0; i < max; i++)
            {
                await Task.Run(async () =>
                {
                    try
                    {
                        using TcpClient tcpClient = new TcpClient();
                        tcpClient.Connect("localhost", 4555);
                        using NetworkStream networkStream = tcpClient.GetStream();
                        byte[] bytes = Encoding.UTF8.GetBytes($"writefile('{num}.check', 'nice')");
                        networkStream.Write(bytes, 0, bytes.Length);
                        networkStream.Flush();
                    }
                    catch
                    {
                        //MessageBox.Show("Error: " + ex.Message);
                    }

                    //try { Execute($"writefile('{num}.check', 'nice')"); } catch { }
                });
                await Task.Delay(200);

                if (File.Exists(api2.workspace + $"\\{num}.check"))
                {
                    await Task.Delay(150);
                    try { File.Delete(api2.workspace + $"\\{num}.check"); } catch { }
                    return true;
                }
                else
                {
                    foreach (var p in Process.GetProcessesByName("RobloxPlayerBeta"))
                    {
                        try
                        {
                            if (File.Exists(Path.GetDirectoryName(p.MainModule.FileName) + $"\\workspace\\{num}.check"))
                            {
                                await Task.Delay(150);
                                try { File.Delete(Path.GetDirectoryName(p.MainModule.FileName) + $"\\workspace\\{num}.check"); } catch { }
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


        internal static readonly string EssenceFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Essence");

        internal static async Task<InjectionResult> Inject()
        {
            try
            {
                await Task.Run(() =>
                {
                    Process.Start(new ProcessStartInfo()
                    {
                        FileName = $"{EssenceFolder}\\bin\\Injector.exe",
                        WorkingDirectory = Path.Combine(EssenceFolder, "bin"),
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden
                    });
                });

                await Task.Delay(2000);
                if (await TestExecution(20))
                    return InjectionResult.Success;
            }
            catch { }
            return InjectionResult.Failed;
        }
    }
}