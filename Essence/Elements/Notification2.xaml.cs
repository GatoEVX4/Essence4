using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Essence.Elements
{
    /// <summary>
    /// Lógica interna para Notification2.xaml
    /// </summary>
    public partial class Notification2 : Window
    {
        string text = "";
        double duration = 1;
        public Notification2(string textt, double durationn)
        {
            kk = -1;
            text = textt;
            duration = durationn;
            InitializeComponent();
            dafsdfdsfd.Visibility = Visibility.Collapsed;
        }



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

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, () =>
            {
                storyboard.Begin();
            });
        }

        public static void Move(DependencyObject ElementName, Thickness Origin, Thickness Location, double Time)
        {
            ThicknessAnimation Anims = new ThicknessAnimation()
            {
                From = Origin,
                To = Location,
                Duration = TimeSpan.FromSeconds(Time),
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseInOut }
            };
            Storyboard.SetTarget(Anims, ElementName);
            Storyboard.SetTargetProperty(Anims, new PropertyPath(MarginProperty));
            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(Anims);

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, () =>
            {
                storyboard.Begin();
            });
        }

        private void AnimateImage()
        {
            ScaleTransform scaleTransform = new ScaleTransform(1, 1);
            RotateTransform rotateTransform = new RotateTransform(0);
            TransformGroup transformGroup = new TransformGroup();
            transformGroup.Children.Add(scaleTransform);
            transformGroup.Children.Add(rotateTransform);
            dhnwedunwnd.RenderTransform = transformGroup;
            dhnwedunwnd.RenderTransformOrigin = new Point(0.5, 0.5);

            DoubleAnimation growAnimation = new DoubleAnimation
            {
                To = 1.2,
                Duration = TimeSpan.FromSeconds(0.3),
                AutoReverse = true
            };

            DoubleAnimationUsingKeyFrames shakeAnimation = new DoubleAnimationUsingKeyFrames
            {
                Duration = TimeSpan.FromSeconds(0.3),
                AutoReverse = true
            };

            shakeAnimation.KeyFrames.Add(new LinearDoubleKeyFrame(5, KeyTime.FromPercent(0.1)));
            shakeAnimation.KeyFrames.Add(new LinearDoubleKeyFrame(-5, KeyTime.FromPercent(0.2)));
            shakeAnimation.KeyFrames.Add(new LinearDoubleKeyFrame(3, KeyTime.FromPercent(0.3)));
            shakeAnimation.KeyFrames.Add(new LinearDoubleKeyFrame(-3, KeyTime.FromPercent(0.4)));
            shakeAnimation.KeyFrames.Add(new LinearDoubleKeyFrame(2, KeyTime.FromPercent(0.5)));
            shakeAnimation.KeyFrames.Add(new LinearDoubleKeyFrame(-2, KeyTime.FromPercent(0.6)));
            shakeAnimation.KeyFrames.Add(new LinearDoubleKeyFrame(0, KeyTime.FromPercent(1.0)));

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(growAnimation);
            storyboard.Children.Add(shakeAnimation);
            Storyboard.SetTarget(growAnimation, dhnwedunwnd);
            Storyboard.SetTargetProperty(growAnimation, new PropertyPath("RenderTransform.Children[0].ScaleX"));

            DoubleAnimation growAnimationY = growAnimation.Clone();
            Storyboard.SetTargetProperty(growAnimationY, new PropertyPath("RenderTransform.Children[0].ScaleY"));
            storyboard.Children.Add(growAnimationY);

            Storyboard.SetTarget(shakeAnimation, dhnwedunwnd);
            Storyboard.SetTargetProperty(shakeAnimation, new PropertyPath("RenderTransform.Children[1].Angle"));

            storyboard.Begin();
        }

        internal int kk = 0;
        private async void dafsdfdsfd_Loaded(object sender, RoutedEventArgs e)
        {
            kk = 0;
            this.Activate();
            ieanfjeanfifnuineufenafn.Text = text;
            Fade(dunw3udqnu, 0, 1, 0.2);
            Fade(dhnwedunwnd, 0, 1, 0.4);
            Move(dunw3udqnu, new Thickness(185, 75, 185, -25), new Thickness(160, 0, 160, 0), 0.5);

            await Task.Delay(400);
            Topmost = true;

            AnimateImage();

            await Task.Delay(700);
            Move(dunw3udqnu, new Thickness(160, 0, 160, 0), new Thickness(0), 0.3);

            Fade(dafsdfdsfd, 0, 1, 0.5);
            dafsdfdsfd.Visibility = Visibility.Visible;

            await Task.Delay(TimeSpan.FromSeconds(duration));

            try
            {
                if (increments.Count >= 1)
                {
                    Queue<double> queue = new Queue<double>();
                    queue = new Queue<double>(increments);
                    int lastCount = increments.Count;

                    while (queue.Count > 0)
                    {
                        double kkk = queue.Dequeue();
                        await Task.Delay(TimeSpan.FromSeconds(kkk));
                        for (int i = lastCount; i < increments.Count; i++)
                        {
                            queue.Enqueue(increments[i]);
                        }
                        lastCount = increments.Count;
                    }
                }
            }
            catch { }

            kk = 1;


            Fade(dafsdfdsfd, 1, 0, 0.5);
            await Task.Delay(500);

            Move(dunw3udqnu, new Thickness(0, 0, 0, 0), new Thickness(185, 85, 185, -30),  1);
            await Task.Delay(500);
            Fade(dunw3udqnu, 1, 0, 0.5);
            await Task.Delay(1000);
            kk = 2;
        }

        List<double> increments = new List<double>();
        bool k = false;
        bool waiting = false;
        internal async void updateText(string text, double duration, double lastduration)
        {
            increments.Add(duration-0.5);

            while (waiting) { await Task.Delay(100); }
            waiting = true;

            await Task.Delay(TimeSpan.FromSeconds(lastduration-0.5));

            if (!k)
            {
                ieanfjeanfifnuineufenafn2.Text = text;
                Move(ieanfjeanfifnuineufenafn, ieanfjeanfifnuineufenafn.Margin, new Thickness(0, -120, 0, 0), 0.3);
                await Task.Delay(100);
                Move(ieanfjeanfifnuineufenafn2, ieanfjeanfifnuineufenafn2.Margin, new Thickness(0, 0, 0, 0), 0.5);

                await Task.Delay(200);
                Move(ieanfjeanfifnuineufenafn, new Thickness(0, 0, 0, -110), new Thickness(0, 0, 0, -120), 0.1);
            }
            else
            {
                ieanfjeanfifnuineufenafn.Text = text;
                Move(ieanfjeanfifnuineufenafn2, ieanfjeanfifnuineufenafn2.Margin, new Thickness(0, -120, 0, 0), 0.3);
                await Task.Delay(100);
                Move(ieanfjeanfifnuineufenafn, ieanfjeanfifnuineufenafn.Margin, new Thickness(0, 0, 0, 0), 0.5);

                await Task.Delay(200);
                Move(ieanfjeanfifnuineufenafn2, new Thickness(0, 0, 0, -110), new Thickness(0, 0, 0, -120), 0.1);
            }

            k = !k;
            waiting = false;
        }
    }
}
