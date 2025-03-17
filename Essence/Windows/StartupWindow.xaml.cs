using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Essence.Windows
{
    /// <summary>
    /// Lógica interna para StartupWindow.xaml
    /// </summary>
    public partial class StartupWindow : Window
    {
        public StartupWindow()
        {
            InitializeComponent();
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            kkkk.Visibility = Visibility.Collapsed;
            w13.Visibility = Visibility.Collapsed;
            SpinningBorder.Visibility = Visibility.Collapsed;
            SpinningBorder2.Visibility = Visibility.Collapsed;
            egggg.Margin = new Thickness(35, 0, -200, 0);
        }

        private void MainWin_Loaded(object sender, RoutedEventArgs e)
        {
            Inicializar();
        }

        private async void ee_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                //ermm.Text = (SpinningBorder.Width / 2).ToString();
                ggg.Radius = SpinningBorder.Width / 3;
            }, DispatcherPriority.Send);
        }

        internal DispatcherTimer RGBTime;
        private RegistryKey registryKey1;
        private async Task Inicializar()
        {
            await Task.Delay(1500);
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;

            Width = 850;
            Height = 550;
            Left = (screenWidth - Width) / 2.0;
            Top = (screenHeight - Height) / 2.0;


            WindowBorder3.Margin = new Thickness(320, 178.5, 320, 178.5);
            compilationtxt.Text = $"Version: {ExecSettings.CurrentVersion} | Compilation: {Encoding.UTF8.GetString(Properties.Resources.BuildInfos).Split('+')[0]} ~ {Encoding.UTF8.GetString(Properties.Resources.BuildInfos).Split('+')[1]}";

            try
            {
                await Dispatcher.InvokeAsync(async () =>
                {
                    ThicknessAnimation lol = new ThicknessAnimation()
                    {
                        To = new Thickness(150),
                        Duration = TimeSpan.FromSeconds(0.5),
                        EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
                    };
                    WindowBorder3.BeginAnimation(MarginProperty, lol);

                    DoubleAnimation fadein = new DoubleAnimation()
                    {
                        From = 0,
                        To = 1,
                        Duration = TimeSpan.FromSeconds(0.5)
                    };
                    kkkk.BeginAnimation(OpacityProperty, fadein);
                    kkkk.Visibility = Visibility.Visible;

                    DoubleAnimation sizeb1 = new DoubleAnimation()
                    {
                        From = 0,
                        To = 300,
                        Duration = TimeSpan.FromSeconds(0.7),
                        EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                    };
                    w13.BeginAnimation(WidthProperty, sizeb1);
                    w13.Visibility = Visibility.Visible;
                    await Task.Delay(1000);

                    DispatcherTimer RGBTime = new DispatcherTimer(TimeSpan.FromMilliseconds(40), DispatcherPriority.Send, delegate
                    {
                        try
                        {
                            rgbRotation.Angle += 20;
                            rgbRotation2.Angle += 20;
                        }
                        catch { }
                    }, System.Windows.Application.Current.Dispatcher);
                    RGBTime.Start();

                    //DoubleAnimation sizeb2 = new DoubleAnimation()
                    //{
                    //    From = 0,
                    //    0,
                    //    Duration = TimeSpan.FromSeconds(1),
                    //    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                    //};
                    //SpinningBorder.BeginAnimation(WidthProperty, sizeb2);
                    SpinningBorder.Visibility = Visibility.Visible;
                    SpinningBorder2.Visibility = Visibility.Visible;

                }, DispatcherPriority.Send);
            }
            catch { }
        }

        bool CloseCompleted;
        private void MainWin_Closing(object sender, CancelEventArgs e)
        {
            if (!CloseCompleted)
            {
                FormFadeOut.Begin();
                e.Cancel = true;
            }

            Process.GetCurrentProcess().Kill();

            //using (Dispatcher.DisableProcessing())
            //{

            //}
        }

        private void CloseWindow(object sender, EventArgs e)
        {
            CloseCompleted = true;
            Process.GetCurrentProcess().Kill();
        }

        private async void ExitMD(object sender, MouseButtonEventArgs e)
        {
            await Task.Delay(600);
            FormFadeOut.Begin();
        }

        internal async void opsie()
        {
            WhatIsThis.Visibility = Visibility.Visible;

            lolexitbtn.Opacity = 0;
            banimage.Visibility = Visibility.Collapsed;
            banimage.Visibility = Visibility.Visible;

            var marginAnimation = new ThicknessAnimation
            {
                From = new Thickness(170, 170, 170, 170),
                To = new Thickness(10, 10, 10, 10),
                Duration = TimeSpan.FromSeconds(0.65),
                EasingFunction = new CircleEase { EasingMode = EasingMode.EaseInOut },
            };
            WindowBorder3.BeginAnimation(MarginProperty, marginAnimation);

            //try
            //{
            //    SoundPlayer player = new SoundPlayer(Properties.Resources.death_sound2);
            //    player.Play();
            //}
            //catch { }
            killbeffect2.Opacity = 0.7;

            DoubleAnimation animlo = new DoubleAnimation()
            {
                From = 2000,
                To = 680,
                Duration = new Duration(TimeSpan.FromMilliseconds(400)),
                AutoReverse = false,
                EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut }
            };
            banimage.BeginAnimation(Image.WidthProperty, animlo);

            await Task.Delay(2500);
            DoubleAnimation animlolo = new DoubleAnimation()
            {
                From = 1.0,
                To = 0.0,
                Duration = TimeSpan.FromMilliseconds(500),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            killbeffect.BeginAnimation(OpacityProperty, animlolo);

            DoubleAnimation animlolok = new DoubleAnimation()
            {
                From = 1,
                To = 0.0,
                Duration = TimeSpan.FromMilliseconds(7000),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            killbeffect2.BeginAnimation(OpacityProperty, animlolok);

            DoubleAnimation animlolokk = new DoubleAnimation()
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(3000),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
            };
            lolexitbtn.BeginAnimation(OpacityProperty, animlolokk);
        }
    }
}
