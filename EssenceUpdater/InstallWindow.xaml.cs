using IWshRuntimeLibrary;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Threading;

namespace EssenceUpdater
{
    /// <summary>
    /// Lógica interna para InstallWindow.xaml
    /// </summary>
    public partial class InstallWindow : Window
    {
        private readonly string discordinvite = "https://discord.com/invite/Ku5HGekNQw";
        private static readonly string url = "https://essenceapi.discloud.app/";
        private static readonly string EssenceFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Essence");




        private DispatcherTimer uiResponsivenessTimer;
        public void OnUIResponse(object sender, EventArgs e)
        {
            try 
            { 

                App.isUIResponsive = true;

            } catch { }
        }
        public InstallWindow()
        {
            InitializeComponent();
            CancelB.Visibility = Visibility.Collapsed;
            instprocess.Visibility = Visibility.Collapsed;
            HelpB.Visibility = Visibility.Collapsed;
            ADVgrid.Visibility = Visibility.Collapsed;
            EULA.Visibility = Visibility.Collapsed;

            discordB.Visibility = Visibility.Collapsed;
            e.Visibility = Visibility.Collapsed;
            startB.Visibility = Visibility.Collapsed;

            socialmtext.Visibility = Visibility.Collapsed;
            socialmedi.Visibility = Visibility.Collapsed;

            uiResponsivenessTimer = new DispatcherTimer();
            uiResponsivenessTimer.Interval = TimeSpan.FromSeconds(1);
            uiResponsivenessTimer.Tick += OnUIResponse;
            uiResponsivenessTimer.Start();
        }

        public void Fade(double speed, UIElement Object, double Get, double Set)
        {
            DoubleAnimation Animation = new DoubleAnimation()
            {
                From = Get,
                To = Set,
                Duration = TimeSpan.FromSeconds(speed),
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseInOut }
            };
            Object.BeginAnimation(OpacityProperty, Animation);
        }
        public void Move(double speed, UIElement Object, Thickness Get, Thickness Set)
        {
            ThicknessAnimation Animation = new ThicknessAnimation()
            {
                From = Get,
                To = Set,
                Duration = TimeSpan.FromSeconds(speed),
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseInOut }
            };
            Object.BeginAnimation(MarginProperty, Animation);
        }

        private bool sdasdsajdasjd = true;
        private Dictionary<string, string> phrases = new Dictionary<string, string>
        {
            { "The Ultimate Execution Power", "Ultimate" },
            { "Your Go-To Execution Tool", "Go-To" },
            { "Next-Level Execution, Redefined", "Next-Level" },
            { "Unmatched Speed, Unstoppable Execution", "Unstoppable" },
            { "The King of All Executors", "King" },
            { "Execution Like Never Before", "Never" },
            { "Your Supreme Execution Solution", "Supreme" },
            { "Precision. Power. Perfection.", "Perfection." },
            { "The Future of Execution Starts Here", "Here" },
            { "Flawless Execution, Every Time", "Flawless" }
        };

        LinearGradientBrush gradient = new LinearGradientBrush
        {
            StartPoint = new System.Windows.Point(0, 0),
            EndPoint = new System.Windows.Point(1, 1),
            GradientStops = new GradientStopCollection
            {
                new GradientStop((Color)ColorConverter.ConvertFromString("#FFFF8B8A"), 0),
                new GradientStop((Color)ColorConverter.ConvertFromString("#FF895078"), 1)
            }
        };

        private async void AnimateP()
        {
            await Task.Delay(2000);

            while (true)
            {
                if (!sdasdsajdasjd)
                    await Task.Delay(2000);

                foreach (var phrase in phrases)
                {
                    string text = phrase.Key;
                    string w = phrase.Value;

                    foreach (UIElement element in ((WrapPanel)lol.Children[0]).Children)
                    {
                        Move(0.4, element, new Thickness(0), new Thickness(0, -100, 0, 0));
                        await Task.Delay(80);
                    }
                    await Task.Delay(400);
                    lol.Children.Clear();

                    var wrapp = new WrapPanel { Width = 350, HorizontalAlignment = HorizontalAlignment.Left };
                    lol.Children.Add(wrapp);

                    string[] split = text.Split(' ');

                    foreach (var part in split)
                    {
                        if (part == w)
                        {
                            var highlight = new TextBlock
                            {
                                Text = part,
                                Foreground = gradient,
                                FontFamily = new FontFamily("/EssenceUpdater;component/Graphics/Fonts/#Atkinson Hyperlegible"),
                                FontSize = 28,
                                FontWeight = FontWeights.Bold,
                                Margin = new Thickness(0, 0, 8, 0)
                            };
                            var border = new Border
                            {
                                Height = 3,
                                HorizontalAlignment = HorizontalAlignment.Stretch,
                                VerticalAlignment = VerticalAlignment.Bottom,
                                Background = gradient,
                                Effect = new BlurEffect { Radius = 2 },
                                Margin = new Thickness(0, 0, 8, 0)
                            };

                            var grid = new Grid();

                            grid.Children.Add(highlight);
                            grid.Children.Add(border);


                            wrapp.Children.Add(grid);
                            Move(0.6, grid, new Thickness(0, 0, 0, -200), new Thickness(0));
                        }
                        else
                        {
                            var blc = new TextBlock
                            {
                                Text = part + " ",
                                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEAE6F5")),
                                FontFamily = new FontFamily("/EssenceUpdater;component/Graphics/Fonts/#Atkinson Hyperlegible"),
                                FontSize = 28,
                                FontWeight = FontWeights.Bold
                            };
                            wrapp.Children.Add(blc);
                            Move(0.6, blc, new Thickness(0, 0, 0, -200), new Thickness(0));
                        }
                        await Task.Delay(100);
                    }

                    await Task.Delay(2000);
                }
            }
        }

        private async void ExitB_Click(object sender, RoutedEventArgs e)
        {
            await Task.Delay(300);
            FormFadeOut.Begin();
        }

        private void CloseWindow(object sender, EventArgs e)
        {
            Process.GetCurrentProcess().Kill();
        }

        private async void MinimizeB_Click(object sender, RoutedEventArgs e)
        {
            await Task.Delay(300);
            WindowState = WindowState.Minimized;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try { DragMove(); } catch { }
        }









        int adload = 0;
        private string adlink = "";
        MediaElement LoadedMediaElement = null;
        DispatcherTimer Spin;
        private int sspeed = 4;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //bool vm = true;
            //try
            //{
            //    using (var searcher = new System.Management.ManagementObjectSearcher(@"root\WMI", "SELECT * FROM MSAcpi_ThermalZoneTemperature"))
            //    {
            //        foreach (var queryObj in searcher.Get())
            //        {
            //            if (queryObj["CurrentTemperature"] != null)
            //            {
            //                double _ = ((Convert.ToDouble(queryObj["CurrentTemperature"]) - 2732) / 10.0);
            //                vm = false;
            //            }
            //        }
            //    }
            //}
            //catch { }

            //if (vm)
            //    MessageBox.Show(
            //        "Warning: \n\n" +
            //        "If this is malware testing, note that advanced obfuscation and virtualization techniques are used, which may trigger high-level detections.",
            //        "VM Detected",
            //        MessageBoxButton.OK,
            //        MessageBoxImage.Warning
            //    );

            //string jwt = Marshal.PtrToStringAnsi(Helpers.DoJwt("login:mysterysas32@gmail.com,region:PT,password:12345678"));
            //Console.WriteLine(jwt);

            Show();
            Startup();
        }

        private async Task Startup()
        {
            AnimateP();
            Spin = new DispatcherTimer(TimeSpan.FromMilliseconds(10), DispatcherPriority.Normal, delegate
            {
                rotate.Angle += sspeed;
            }, System.Windows.Application.Current.Dispatcher);
            Spin.Start();

            int first_line = 1;
            string[] palavras = new string[0] { /*"Essence", "Essence.", "Ian", "m4a1_lindo2.", "\"Essence\"", "\"Essence\".", "\"Essence\":", "(\"Essence\")", "\"Essence\","*/ };
            Dispatcher.Invoke(() => EULARichTextBox.Document.Blocks.Clear());
            string[] array = Properties.Resources.EULA.Split(new string[2] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (string line in array)
            {
                Paragraph paragraph = new Paragraph();
                string[] array2 = line.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                bool lineContainsSpecialWord = false;
                string[] array3 = array2;
                for (int num2 = 0; num2 < array3.Length; num2++)
                {
                    string processedWord = array3[num2].TrimEnd('#');
                    Run run = new Run(processedWord.Replace("#", "") + " ");
                    if (processedWord.StartsWith("#"))
                    {
                        lineContainsSpecialWord = true;
                    }
                    else if (palavras.Contains(processedWord, StringComparer.OrdinalIgnoreCase))
                    {
                        run.FontWeight = FontWeights.Bold;
                        run.Foreground = Brushes.DodgerBlue;
                    }
                    paragraph.Inlines.Add(run);
                }
                if (lineContainsSpecialWord)
                {
                    paragraph.FontWeight = FontWeights.Bold;
                    paragraph.FontSize = 13.5;
                    paragraph.Foreground = Brushes.White;
                }
                else if (first_line < 3)
                {
                    first_line += 1;
                    paragraph.FontWeight = FontWeights.Bold;
                    paragraph.FontSize = 15.0;
                    paragraph.Foreground = Brushes.White;
                }
                else
                {
                    paragraph.FontWeight = FontWeights.Normal;
                    paragraph.FontSize = 11.0;
                    paragraph.Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 200));
                }
                Dispatcher.Invoke(() => EULARichTextBox.Document.Blocks.Add(paragraph));
            }
            try
            {
                string image = "";
                JObject jsonObj = JObject.Parse(await httpreq("externals/ads"));
                image = jsonObj["installer"]?["image"].ToString() ?? "https://www.pixground.com/colorful-abstract-background-moving-waves-4k-wallpaper/?download-img=4k";
                adlink = jsonObj["installer"]?["link"].ToString() ?? discordinvite;

                if (await Helpers.DownloadFile(image, Path.Combine(EssenceFolder, "advertisement.mp4"), auth:false))
                {
                    LoadedMediaElement = new MediaElement
                    {
                        Source = new Uri(Path.Combine(EssenceFolder, "advertisement.mp4"), UriKind.RelativeOrAbsolute),
                        LoadedBehavior = MediaState.Manual,
                        UnloadedBehavior = MediaState.Manual,
                        Volume = 0,
                        Position = TimeSpan.Zero,
                        SpeedRatio = 2,
                        StretchDirection = StretchDirection.Both,
                        Stretch = Stretch.UniformToFill
                    };

                    LoadedMediaElement.MediaEnded += (e2, sender2) =>
                    {
                        LoadedMediaElement.Stop();
                        ADimage.Child = null;
                        LoadedMediaElement = null;

                        LoadedMediaElement = new MediaElement
                        {
                            Source = new Uri(Path.Combine(EssenceFolder, "advertisement.mp4"), UriKind.RelativeOrAbsolute),
                            LoadedBehavior = MediaState.Manual,
                            UnloadedBehavior = MediaState.Manual,
                            Volume = 0,
                            Position = TimeSpan.Zero,
                            SpeedRatio = 2,
                            StretchDirection = StretchDirection.Both,
                            Stretch = Stretch.UniformToFill
                        };
                        ADimage.Child = LoadedMediaElement;
                        LoadedMediaElement.Play();
                    };

                    ADimage.Child = LoadedMediaElement;
                    adload = 1;
                }
            }
            catch
            {
                adload = 2;
            }
        }

        private async void AcceptB_Click(object sender, RoutedEventArgs e)
        {
            AcceptB.IsEnabled = false;
            EulaB.IsEnabled = false;
            sdasdsajdasjd = false;
            BackB.IsEnabled = false;

            if (EULA.Visibility == Visibility.Visible)
            {
                Fade(0.4, EULA, EULA.Opacity, 0);
                await Task.Delay(400);
                EULA.Visibility = Visibility.Collapsed;
            }

            things.Children.Remove(fasdfadfdf);
            maingrid.Children.Add(fasdfadfdf);
            fasdfadfdf.Margin = new Thickness(50, 60, 0, 0);
            lol.Margin = new Thickness(0, 52, 0, 0);

            Move(0.4, MinimizeB, MinimizeB.Margin, new Thickness(0, 10, 10, 0));
            ExitB.Visibility = Visibility.Collapsed;

            Move(0.4, AcceptB, AcceptB.Margin, new Thickness(-100, 0, 0, -100));
            await Task.Delay(80);
            Move(0.4, EulaB, EulaB.Margin, new Thickness(-100, 0, 0, -100));

            await Task.Delay(500);
            Fade(0.3, things, 1, 0);

            Storyboard sb = (Storyboard)FindResource("GradientAnimation");
            sb.Begin();


            await Task.Delay(500);
            things.Visibility = Visibility.Collapsed;
            Move(0.6, CancelB, new Thickness(0, 85, -200, 0), new Thickness(0, 85, 50, 00));
            CancelB.Visibility = Visibility.Visible;

            await Task.Delay(500);

            Fade(1, instprocess, 0, 1);
            instprocess.Visibility = Visibility.Visible;

            await Task.Delay(500);

            Fade(1, HelpB, 0, 1);
            HelpB.Visibility = Visibility.Visible;

            DoRealStuff();
        }

        private async void EulaB_Click(object sender, RoutedEventArgs e)
        {
            AcceptB.IsEnabled = false;
            EulaB.IsEnabled = false;
            await Task.Delay(200);
            Fade(0.6, EULA, 0, 1);
            EULA.Visibility = Visibility.Visible;
            await Task.Delay(600);
            AcceptB.IsEnabled = true;
            EulaB.IsEnabled = true;
        }

        private async void CancelB_Click(object sender, RoutedEventArgs e)
        {
            stopstuff = true;
            CancelB.IsEnabled = false;

            await Task.Delay(500);

            instprocess.Text = "Stopping installation...";
            Storyboard sb = (Storyboard)FindResource("GradientAnimation");
            sb.Begin();

            if (dwn || ext)
            {
                instprocess.Text = "Cleaning Stuff...";
                await Task.Delay(1000);
                dwn = false;
                ext = false;

                while (stopstuff) { await Task.Delay(100); }
            }

            
            CancelStuff();
        }

        private async void BackB_Click(object sender, RoutedEventArgs e)
        {
            BackB.IsEnabled = false;
            AcceptB.IsEnabled = false;
            await Task.Delay(200);
            Fade(0.6, EULA, 1, 0);
            await Task.Delay(600);
            EULA.Visibility = Visibility.Collapsed;
            BackB.IsEnabled = true;
            AcceptB.IsEnabled = true;
        }

        private void HelpB_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(discordinvite);
        }

        private void ADB_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(adlink);
        }

        private void Run_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start(discordinvite);
        }



        private void RestartBar()
        {
            Storyboard sb = (Storyboard)FindResource("GradientAnimation");
            sb.Stop();
            Stop0.BeginAnimation(GradientStop.OffsetProperty, null);
            Stop1.BeginAnimation(GradientStop.OffsetProperty, null);
            Stop2.BeginAnimation(GradientStop.OffsetProperty, null);

            Stop0.Offset = -0.6;
            Stop1.Offset = -0.3;
            Stop2.Offset = 0;
        }



        private async void CancelStuff()
        {
            CancelB.IsEnabled = false;
            Storyboard sb = (Storyboard)FindResource("GradientAnimation");
            sspeed = 4;

            //string[] rfiles = Directory.GetFiles(EssenceFolder, "*", SearchOption.AllDirectories);

            //if (rfiles.Length > 0)
            //{                
            //    sb.Begin();
            //    instprocess.Text = "Clearing files...";
            //    int j = 1;
            //    foreach (string ek in rfiles)
            //    {
            //        instprocess.Text = $"Clearing files [{j}/{rfiles.Length}]...";
            //        System.IO.File.Delete(ek);
            //        j++;
            //        await Task.Delay(1000/ rfiles.Length);
            //    }
            //    foreach (string dir in Directory.GetDirectories(EssenceFolder, "*", SearchOption.AllDirectories))
            //    {
            //        if (Directory.GetFiles(dir).Length == 0 && Directory.GetDirectories(dir).Length == 0)
            //        {
            //            Directory.Delete(dir, true);
            //        }
            //    }
            //    await Task.Delay(1000);
            //}
            instprocess.Text = "Returning...";
            RestartBar();

            await Task.Delay(500);
            Fade(0.4, HelpB, 1, 0);
            Move(0.4, CancelB, CancelB.Margin, new Thickness(0, 0, -200, 0));
            Fade(1, instprocess, 1, 0);

            if (adload == 1)
            {
                Fade(1, ADVgrid, 1, 0);
                Move(0.4, ADVgrid, ADVgrid.Margin, new Thickness(50, 0, 50, -250));
            }

            Move(0.4, MinimizeB, MinimizeB.Margin, new Thickness(0, 10, 44, 0));
            await Task.Delay(500);
            ExitB.Visibility = Visibility.Visible;
            HelpB.Visibility = Visibility.Collapsed;
            Fade(0.4, things, 0, 1);
            things.Visibility = Visibility.Visible;
            await Task.Delay(300);

            maingrid.Children.Remove(fasdfadfdf);
            things.Children.Insert(0, fasdfadfdf);
            fasdfadfdf.Margin = new Thickness(0);
            lol.Margin = new Thickness(0, 10, 0, 0);

            instprocess.Visibility = Visibility.Collapsed;
            AcceptB.IsEnabled = true;
            EulaB.IsEnabled = true;
            sdasdsajdasjd = true;
            BackB.IsEnabled = true;

            Move(0.4, AcceptB, AcceptB.Margin, new Thickness(50, 0, 0, 40));
            await Task.Delay(80);
            Move(0.4, EulaB, EulaB.Margin, new Thickness(210, 0, 0, 40));
            CancelB.IsEnabled = true;


        }

        int tries = 0;
        private async Task<string> httpreq(string endpoint)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    //client.DefaultRequestHeaders.Add("authenticity", Helpers.a_r());
                    client.Timeout = TimeSpan.FromSeconds(5);

                    HttpResponseMessage response = await client.GetAsync(url + endpoint);
                    if (response.IsSuccessStatusCode)
                    {
                        return await response.Content.ReadAsStringAsync();
                    }
                    else
                    {
                        if (tries < 3)
                        {
                            tries++;
                            return await httpreq(endpoint);
                        }
                        else
                        {
                            return "e";
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return "e";
            }
        }


        internal static bool dwn = false;
        internal static bool ext = false;
        string finalmsg = "Essence has been successfully installed!";
        internal static bool stopstuff = false;
        Dictionary<string, string> downloads = new Dictionary<string, string>();
        Dictionary<string, string> hashes = new Dictionary<string, string>();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr LoadLibrary(string dll);

        [DllImport("kernel32.dll")]
        private static extern bool FreeLibrary(IntPtr dl);

        private bool loadedd;
        private IntPtr dl;
        private async Task DoRealStuff()
        {
            downloads.Clear();
            hashes.Clear();

            instprocess.Text = "Connecting To Server..."; 
            Storyboard sb = (Storyboard)FindResource("GradientAnimation");
            sb.Begin();

            await Task.Delay(2000);
            if (stopstuff) { stopstuff = false; return; }            

            if (!await Helpers.TestServer(url))
            {
                if (!await Helpers.TestServer("http://www.google.com"))
                {
                    MessageBox.Show("You're not connected to internet");
                    CancelStuff();
                    return;
                }
                else
                {
                    MessageBox.Show("EssenceUpdater could not reach Essence Servers. Try again later");
                    CancelStuff();
                    return;
                }
            }

            Fade(0.3, afsdassaads, 0, 1);
            afsdassaads.Visibility = Visibility.Visible;
            dsadasdad.Text = "Loading";
            int upd = 1;
            await Task.Run(() =>
            {
                if (loadedd)
                {
                    if (FreeLibrary(dl))
                        loadedd = false;
                }

                if (!loadedd)
                {
                    try
                    {
                        Dispatcher.Invoke(() => dsadasdad.Text = "Unpacking Auth");
                        string dllP = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Auth.dll");
                        if (System.IO.File.Exists(dllP))
                            System.IO.File.Delete(dllP);

                        using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("EssenceUpdater.Dlls.Auth.dll"))
                        {
                            using (FileStream fileStream = new FileStream(dllP, FileMode.Create, FileAccess.Write))
                            {
                                stream.CopyTo(fileStream);
                            }
                        }
                    }
                    catch { }
                }

                if (!loadedd)
                {
                    Dispatcher.Invoke(() => dsadasdad.Text = "Loading Auth");
                    dl = LoadLibrary("Auth.dll");
                    loadedd = (dl != IntPtr.Zero);
                }

                Dispatcher.Invoke(() => dsadasdad.Text = "Checking Updates");               
                try { upd = Helpers.InitAuth(); } catch (Exception ex) { MessageBox.Show(ex.ToString()); }
            });

            if (upd != 0)
            {
                string discordlink = await httpreq("externals/discord");
                dsadasdad.Text = "Visit our website to download the latest version";
                MessageBox.Show("This Updater version is oudated. Please visit our discord for new version.");
                Process.Start(discordlink);
                CancelStuff();
                Fade(0.3, afsdassaads, 1, 0);
                await Task.Delay(300);
                afsdassaads.Visibility = Visibility.Collapsed;
                return;
            }

            dsadasdad.Text = "Updated!";
            Fade(0.3, afsdassaads, 1, 0);
            await Task.Delay(300);
            afsdassaads.Visibility = Visibility.Collapsed;

            if (!Directory.Exists(EssenceFolder))
                Directory.CreateDirectory(EssenceFolder);

            if (stopstuff) { stopstuff = false; return; }

            sspeed = 10;
            instprocess.Text = "Getting Links & Hashes...";
            string resposta = await httpreq("externals/files");
            if(resposta == "e")
            {
                MessageBox.Show("Could not get stuff from server. Try again");
                CancelStuff();
                return;
            }
            
            await Task.Delay(200);
            instprocess.Text = "Checking Files...";
            if (stopstuff) { stopstuff = false; return; }
            try
            {
                JObject jsonObj = JObject.Parse(resposta);
                
                if (jsonObj["maintence"] is JObject mdata)
                {
                    if(mdata["enabled"].ToString() != "False")
                    {
                        if (mdata["message"].ToString().Length > 1)
                            MessageBox.Show(mdata["message"].ToString());
                        else
                            MessageBox.Show("Essence servers are under maintence rn. Try again in some secconds");
                        
                        CancelStuff();
                        return;
                    }
                }

                if (jsonObj["files"] is JObject files)
                {
                    if (files.Children<JProperty>().Count() == 0)
                    {
                        MessageBox.Show("Could not get files from server. Try again", "Server didint sent files");
                        CancelStuff();
                        return;
                    }

                    foreach (var file in files.Children<JProperty>())
                    {
                        var location = file.Value["Location"]?.ToString();
                        var download = file.Value["Download"]?.ToString();
                        var hash = file.Value["Hash"]?.ToString();
                        var link = file.Value["Link"]?.ToString();
                        if (location != null && hash != null && link != null && download != null)
                        {
                            if (Helpers.CalculateSHA256(Path.Combine(EssenceFolder, location)) != hash)
                            {
                                downloads.Add(download, link);
                                hashes.Add(Path.Combine(EssenceFolder, location), hash);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Error getting files. Try again", "Json format is wrong");
                            CancelStuff();
                            return;
                        }
                    }
                }
            }
            catch
            {
                MessageBox.Show("Error getting files. Try again", "Unknow Error");
                CancelStuff();
                return;
            }


            if (downloads.Count == 0)
            {
                finalmsg = "All files were already updated.";
                DoFinalStuff();
                return;
            }

            if (adload == 1)
            {
                LoadedMediaElement.Play();
                await Task.Delay(2000);
                Fade(1, ADVgrid, 0, 1);
                Move(0.4, ADVgrid, new Thickness(50, 0, 50, -250), new Thickness(50, 0, 50, 40));
                ADVgrid.Visibility = Visibility.Visible;
            }

            instprocess.Text = "Downloading Files...";
            RestartBar();


            await Task.Delay(1000);
            if (stopstuff) { stopstuff = false; return; }

            if (Process.GetProcessesByName("Essence").Length > 0)
            {
                if (MessageBox.Show("Essence is currently running. Do you want to close it and continue the installation?", "Info", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    foreach (Process p in Process.GetProcessesByName("Essence"))
                    {
                        try { p.Kill(); } catch { }
                    }
                    await Task.Delay(2000);
                    if (Process.GetProcessesByName("Essence").Length > 0)
                    {
                        MessageBox.Show("Could not close Essence. Please, do it manually");
                        CancelStuff();
                        return;
                    }
                }
                else
                {
                    CancelStuff();
                    return;
                }
            }

            foreach (var file in downloads)
            {
                bool ok = false;
                instprocess.Text = "...";
                Stop1.Offset = -0.3;
                Stop2.Offset = 0;

                try
                {
                    instprocess.Text = $"Downloading {file.Key}...\nFrom {file.Value}";                    
                    await Helpers.DownloadFile(file.Value, Path.Combine(EssenceFolder, file.Key), async (progress) =>
                    {
                        instprocess.Text = $"Downloading {file.Key} [{progress}%]\nFrom {file.Value}";
                        Stop1.Offset = progress / 100;
                        Stop2.Offset = (progress / 100) + 0.1;
                    });
                    ok = true;
                    Stop1.Offset = -0.3;
                    Stop2.Offset = 0;
                }
                catch 
                {
                    instprocess.Text = $"Failed to download {file.Key}. Trying Again...";
                    Stop1.Offset = -0.3;
                    Stop2.Offset = 0;

                    for (int i = 1; i <= 5; i++)
                    {
                        try
                        {
                            instprocess.Text = $"Downloading {file.Key}...\nFrom {file.Value}";

                            await Helpers.DownloadFile(file.Value, Path.Combine(EssenceFolder, file.Key), async (progress) =>
                            {
                                instprocess.Text = $"Downloading {file.Key} [{progress}%]";
                                Stop1.Offset = progress / 100;
                                Stop2.Offset = (progress / 100) + 0.3;
                            });
                            ok = true;
                            break;
                        }
                        catch
                        {
                            instprocess.Text = $"Failed to download {file.Key}. Trying Again... [{i}/5]";
                            Stop1.Offset = -0.3;
                            Stop2.Offset = 0;
                            await Task.Delay(1000);
                        }
                        if (stopstuff) { stopstuff = false; return; }
                    }

                    if (!ok)
                    {
                        MessageBox.Show("Could not download files. Check your antivirus/internet connection and try again");
                        CancelStuff();
                        return;
                    }
                }
                await Task.Delay(500);
                if (stopstuff) { stopstuff = false; return; }
            }


            await Helpers.ExtractAll(EssenceFolder, (nigga) =>
            {
                instprocess.Text = $"Extracting Files... {nigga}";
            },

            (progress) =>
            {
                Stop1.Offset = progress / 100;
                Stop2.Offset = (progress / 100) + 0.1;
            });
            DoFinalStuff();
        }

        private async Task DoFinalStuff()
        {
            RestartBar();            
            instprocess.Text = "Checking Files Again...";
            Storyboard sb = (Storyboard)FindResource("GradientAnimation");
            sb.Begin();

            await Task.Delay(2000);

            foreach(var files in hashes)
            {
                if (Helpers.CalculateSHA256(files.Key) != files.Value)
                {
                    MessageBox.Show("WOW some files mysteriously disappeared!", files.Key);
                    MessageBox.Show("please turn off your antivirus and try again");
                    CancelStuff();
                    return;
                }
            }

            RestartBar();
            CancelB.IsEnabled = false;
            sspeed = 6;

            if (adload == 1)
            {
                Fade(1, ADVgrid, 1, 0);
                Move(0.4, ADVgrid, ADVgrid.Margin, new Thickness(50, 0, 50, -250));
            }

            Fade(0.4, HelpB, 1, 0);
            Move(0.4, CancelB, CancelB.Margin, new Thickness(0, 0, -200, 0));
            Move(0.4, MinimizeB, MinimizeB.Margin, new Thickness(0, 10, 44, 0));
            await Task.Delay(500);
            ExitB.Visibility = Visibility.Visible;


            instprocess.Text = finalmsg;
            Color1.Color = (Color)ColorConverter.ConvertFromString("#FF7EE07E");
            Color2.Color = (Color)ColorConverter.ConvertFromString("#FF3EDD69");

            try
            {
                var d = new
                {
                    CollectGameData = CollectGameData.IsChecked.ToString(),
                    Date = DateTime.Now.ToString()
                };
                System.IO.File.WriteAllText(Path.Combine(EssenceFolder, "InstallSettings.json"), JsonConvert.SerializeObject(d, Formatting.Indented));
            }
            catch
            {

            }

            try
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string startMenuPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs");

                string shortcutName = "Essence.lnk";
                string desktopShortcutPath = Path.Combine(desktopPath, shortcutName);
                string startMenuShortcutPath = Path.Combine(startMenuPath, shortcutName);

                string targetPath = Path.Combine(EssenceFolder, "Essence.exe");

                WshShell shell = new WshShell();

                IWshShortcut desktopShortcut = (IWshShortcut)shell.CreateShortcut(desktopShortcutPath);
                desktopShortcut.Description = "Essence Launcher";
                desktopShortcut.TargetPath = targetPath;
                desktopShortcut.IconLocation = targetPath;
                desktopShortcut.Save();

                IWshShortcut startMenuShortcut = (IWshShortcut)shell.CreateShortcut(startMenuShortcutPath);
                startMenuShortcut.Description = "Essence Launcher";
                startMenuShortcut.TargetPath = targetPath;
                startMenuShortcut.IconLocation = targetPath;
                startMenuShortcut.Save();
            }
            catch
            {

            }

            instprocess.Text = finalmsg + "\n" + "An shortcut has been created in your desktop!";

            Thickness l = new Thickness(0);

            l = discordB.Margin;
            Fade(0.3, discordB, 0, 1);
            Move(0.6, discordB, new Thickness(l.Left, l.Top-40, l.Right, l.Bottom), l);
            discordB.Visibility = Visibility.Visible;

            await Task.Delay(120);

            l = e.Margin;
            Fade(0.3, e, 0, 1);
            Move(0.6, e, new Thickness(l.Left, l.Top-40, l.Right, l.Bottom), l);
            e.Visibility = Visibility.Visible;

            await Task.Delay(120);

            l = startB.Margin;
            Fade(0.3, startB, 0, 1);
            Move(0.6, startB, new Thickness(l.Left, l.Top - 40, l.Right, l.Bottom), l);
            startB.Visibility = Visibility.Visible;

            await Task.Delay(120);

            l = socialmtext.Margin;
            Fade(0.3, socialmtext, 0, 1);
            Move(0.6, socialmtext, new Thickness(l.Left, l.Top - 40, l.Right, l.Bottom), l);
            socialmtext.Visibility = Visibility.Visible;

            await Task.Delay(120);

            l = socialmedi.Margin;
            Fade(0.3, socialmedi, 0, 1);
            Move(0.6, socialmedi, new Thickness(l.Left, l.Top - 40, l.Right, l.Bottom), l);
            socialmedi.Visibility = Visibility.Visible;
        }

        private void discordB_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(discordinvite);
        }

        private void startB_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(Path.Combine(EssenceFolder, "Essence.exe"));
                FormFadeOut.Begin();
            }
            catch
            {

            }
        }

        private void CollectGameData_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
