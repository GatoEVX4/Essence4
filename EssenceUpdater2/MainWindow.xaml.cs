using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using MessageBox = System.Windows.Forms.MessageBox;

namespace EssenceUpdater
{
    /// <summary>
    /// Interação lógica para MainWindow.xam
    /// </summary>
    public partial class MainWindow : Window
    {
        private int try_count = 0;
        private static WebClient Web = new WebClient();

        public static void Fade(DependencyObject ElementName, double Start, double End, double Time)
        {
            DoubleAnimation Anims = new DoubleAnimation()
            {
                From = Start,
                To = End,
                Duration = TimeSpan.FromSeconds(Time),
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseInOut }
            };
            Storyboard.SetTarget(Anims, ElementName);
            Storyboard.SetTargetProperty(Anims, new PropertyPath(OpacityProperty));
            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(Anims);

            System.Windows.Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Send, new Action(() =>
            {
                storyboard.Begin();
            }));
        }

        public MainWindow()
        {
            InitializeComponent();

            Stuff.Opacity = 0;
            Servidor_Off.Visibility = Visibility.Collapsed;
        }

        static async Task<bool> CheckUrl(string url = "http://www.example.com")
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(5);
                    using (var response = await client.GetAsync(url))
                    {
                        return response.IsSuccessStatusCode;
                    }
                }
            }
            catch
            {
                return false;
            }
        }
        private string CalculateMD5(string filePath)
        {
            using (FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                using (MD5 md5 = MD5.Create())
                {
                    byte[] hash = md5.ComputeHash(file);
                    return BitConverter.ToString(hash).Replace("-", "").ToUpper();
                }
            }
        }
        private async Task<string> GetLink()
        {
            try_count++;          
            HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);

            HttpResponseMessage response = await client.GetAsync("https://essenceapi.discloud.app/v7/evx");

            if (response.IsSuccessStatusCode)
            {
                await Task.Delay(2000);

                string link = await response.Content.ReadAsStringAsync();
                if (link.Length < 10)
                {
                    if (try_count < 5)
                        return await GetLink();
                }
                else
                    return link;
            }
            else if((int)response.StatusCode == 429)
            {
                await Task.Delay(5000);

                if (try_count < 5)
                    return await GetLink();
            }
            else
            {
                await Task.Delay(2000);

                if (try_count < 5)
                    return await GetLink();
            }
            return "Error";
        }

        private void MoveProgress(int margin)
        {
            ThicknessAnimation lol = new ThicknessAnimation()
            {
                From = new Thickness(30, 0, progressbar.Margin.Right, 20),
                To = new Thickness(30, 0, margin, 20),
                Duration = TimeSpan.FromSeconds(0.3)
            };
            progressbar.BeginAnimation(MarginProperty, lol);
        }

        private async void CloseApp()
        {
            logo.BeginAnimation(OpacityProperty, null);            
            MoveProgress(370);
            logo.Opacity = 1;

            await Task.Delay(1000);
            Close();
        }

        private bool CheckFilesAndHashes(string rootDirectory, Dictionary<string, string> knownHashes)
        {
            bool downloadRequired = false;
            foreach (var kvp in knownHashes)
            {
                string fileName = kvp.Key;
                string knownHash = kvp.Value;

                string[] filesFound = Directory.GetFiles(rootDirectory, fileName, SearchOption.AllDirectories);

                if (filesFound.Length == 0)
                {
                    downloadRequired = true;
                   // MessageBox.Show("gay");
                    break;
                }

                bool fileHashMatches = true;
                foreach (string filePath in filesFound)
                {                    
                    string fileHash = CalculateMD5(filePath);
                    if (fileHash != knownHash)
                    {
                        File.Delete(filePath);
                        fileHashMatches = false;
                        break;
                    }
                }

                if (!fileHashMatches)
                {
                    downloadRequired = true;
                    break;
                }
            }

            return downloadRequired;
        }


        private async Task<bool> DoStuff()
        {
            Fade(Servidor_Off, 1, 0, 0.7);
            await Task.Delay(300);
            Fade(Stuff, 0, 1, 1.3);

            await Task.Delay(200);

            if (!Directory.Exists("C:\\Essence"))
                Directory.CreateDirectory("C:\\Essence");


            TXTPR.Content = "Checking connection...";
            await Task.Delay(100);
            if (!await CheckUrl())
            {
                MessageBox.Show("You cant download Essence without internet", "Opsie!");
            }

            MoveProgress(340);

            DoubleAnimation lol = new DoubleAnimation()
            {
                From = 1.0,
                To = 0.3,
                Duration = TimeSpan.FromSeconds(1.5),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            logo.BeginAnimation(OpacityProperty, lol);

            TXTPR.Content = "Getting Link...";
            string link = await GetLink();
            TXTPR.Content = "Scanning Files...";
            await Task.Delay(300);

            if (link == "Error")
            {
                Fade(Stuff, 1, 0, 0.7);
                await Task.Delay(300);
                Fade(Servidor_Off, 0, 1, 1.3);
                Servidor_Off.Visibility = Visibility.Visible;
                return false;
            }

            foreach (Process p in Process.GetProcessesByName("Evolution"))
            {
                TXTPR.Content = "Stopping Process...";
                p.Kill();
            }

            string[] filesToKeep = new string[]
            {
                "logindata.txt",
                "LastGame.txt",
                "GameHistory.txt",
                "workspace",
                "UserImgs",
                "EvoTabs",
                "AI_history.txt"
            };

            Dictionary<string, string> knownHashes = new Dictionary<string, string>();
            string[] hashesss = link.Split(new string[] { "||||" }, StringSplitOptions.RemoveEmptyEntries)[0].Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string hash in hashesss)
            {
                string name = hash.Split(new string[] { " | " }, StringSplitOptions.RemoveEmptyEntries)[0];
                string id = hash.Split(new string[] { " | " }, StringSplitOptions.RemoveEmptyEntries)[1];
                knownHashes.Add(name, id);
            }


            TXTPR.Content = "Verifying Files...";
            bool downloadRequired = CheckFilesAndHashes("C:\\Essence", knownHashes);


            if (downloadRequired)
            {
                TXTPR.Content = "Updating Stuff...";
                try
                {
                    //TXTPR.Content = "Clearing Files...";
                    // DeleteAllExcept("C:\\Essence", filesToKeep, true);
                    //finished = true;
                }
                catch
                {
                    //finished = true;
                }

                // while (!finished) { await Task.Delay(1000); }

                bool done = false;
                string file_patch = "";
                string download_link = "";
                int total = 0;
                int current = 0;

                Web.DownloadProgressChanged += delegate (object ssd, DownloadProgressChangedEventArgs ee)
                {
                    TXTPR.Content = $"{ee.ProgressPercentage}% [{current}/{total}]";
                    double marginRight = 370 - (ee.ProgressPercentage / 100.0 * 340);
                    progressbar.Margin = new Thickness(30, 0, marginRight, 20);
                };

                Web.DownloadFileCompleted += async delegate (object dww, AsyncCompletedEventArgs es)
                {
                    if (es.Error == null && !es.Cancelled)
                    {
                        progressbar.Margin = new Thickness(30, 0, 30, 20);

                        try
                        {
                            if (file_patch.EndsWith(".zip"))
                            {
                                string extractPath = Path.GetDirectoryName(file_patch);

                                if (!Directory.Exists(extractPath))
                                {
                                    Directory.CreateDirectory(extractPath);
                                }

                                using (ZipArchive zip = ZipFile.OpenRead(file_patch))
                                {
                                    foreach (ZipArchiveEntry entry in zip.Entries)
                                    {
                                        string fullExtractPath = Path.Combine(extractPath, entry.FullName);

                                        if (!File.Exists(fullExtractPath))
                                        {
                                            if (entry.FullName.EndsWith("/")) // Verifica se é uma pasta
                                            {
                                                Directory.CreateDirectory(fullExtractPath);
                                            }
                                            else
                                            {
                                                entry.ExtractToFile(fullExtractPath, true);
                                            }
                                        }
                                    }
                                }

                                File.Delete(file_patch);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message + "\n\nThe extraction failed. Try turning off your antivirus and opening the updater again.");
                            try { File.Delete(file_patch); } catch { }
                        }

                        done = true;
                    }
                    else
                    {
                        MoveProgress(370);
                        try { File.Delete(file_patch); } catch { }
                        await Task.Delay(1000);

                        progressbar.BeginAnimation(MarginProperty, null);
                        Web.DownloadFileAsync(new Uri(download_link), file_patch);
                    }

                };

                progressbar.BeginAnimation(MarginProperty, null);

                string[] data = link.Split(new string[] { "||||" }, StringSplitOptions.RemoveEmptyEntries)[1].Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);
                total = data.Length;
                foreach (string file in data)
                {
                    try
                    {
                        string[] parts = file.Split(new string[] { " | " }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length < 2) continue;
                        download_link = parts[0];
                        file_patch = parts[1];

                        string directoryPath = Path.GetDirectoryName(file_patch);

                        if (!string.IsNullOrEmpty(directoryPath))
                        {
                            Directory.CreateDirectory(directoryPath);
                        }

                        current++;
                        done = false;
                        Web.DownloadFileAsync(new Uri(download_link), file_patch);

                        while (!done) { await Task.Delay(1000); }
                        progressbar.Margin = new Thickness(30, 0, 370, 20);
                    }
                    catch
                    {
                    }
                }
            }
            else
            {
                MoveProgress(30);
                TXTPR.Content = "Files are up-to-date!";
                await Task.Delay(1500);

                Process.Start("C:\\Essence\\Essence.exe");
                Process.GetCurrentProcess().Kill();
            }

            TXTPR.Content = "Starting...";
            MoveProgress(30);

            await Task.Delay(2000);

            try
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string shortcutPath = Path.Combine(desktopPath, "Essence.lnk");
                string targetPath = "C:\\Essence\\Essence.exe";

                IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();
                IWshRuntimeLibrary.IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(shortcutPath);
                shortcut.Description = "Essence Launcher";
                shortcut.TargetPath = targetPath;
                shortcut.IconLocation = targetPath;
                shortcut.Save();
            }
            catch
            {
            }

            Process.Start("C:\\Essence\\Essence.exe");
            Process.GetCurrentProcess().Kill();
            return true;
        }


        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            bool vm = true;
            try
            {
                using (var searcher = new ManagementObjectSearcher(@"root\WMI", "SELECT * FROM MSAcpi_ThermalZoneTemperature"))
                {
                    foreach (var queryObj in searcher.Get())
                    {
                        if (queryObj["CurrentTemperature"] != null)
                        {
                            double _ = ((Convert.ToDouble(queryObj["CurrentTemperature"]) - 2732) / 10.0);
                            vm = false;
                        }
                    }
                }
            }
            catch { }

            if (vm)
                MessageBox.Show(
                    "Warning: This program is running inside a virtual machine.\n\n" +
                    "If this is malware testing, note that advanced obfuscation and virtualization techniques are used, which may trigger high-level detections.",
                    "VM Detected",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            

            while (true)
            {
                if (await DoStuff())
                    break;

                await Task.Delay(5000);
            }
        }
    }
}
