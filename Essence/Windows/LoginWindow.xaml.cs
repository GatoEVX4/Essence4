using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WpfAnimatedGif;
using Path = System.IO.Path;
using static MaterialDesignThemes.Wpf.Theme;
using Essence.Properties;
using System.Windows.Data;
using System.Net.Http.Json;

namespace Essence.Windows
{
    /// <summary>
    /// Lógica interna para LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        LanguageManager lm;


        public LoginWindow()
        {
            lm = new LanguageManager();



            InitializeComponent();
            Opacity = 0;

            W2_page0.Visibility = Visibility.Collapsed;
            W2_page2.Visibility = Visibility.Visible;
            //W2_premium2.Visibility = Visibility.Collapsed;

            W2_page32.Visibility = Visibility.Collapsed;
            W2_page42.Visibility = Visibility.Collapsed;


            EmailConfirmationPage2.Visibility = Visibility.Collapsed;
            DiscordConfirmationPage2.Visibility = Visibility.Collapsed;
            InsertInviteBorder.Visibility = Visibility.Collapsed;

            Gif5.Visibility = Visibility.Collapsed;
            Gif6.Visibility = Visibility.Collapsed;
            Gif7.Visibility = Visibility.Collapsed;

            Fader.Visibility = Visibility.Collapsed;
        }

        private async void MainWin_Loaded(object sender, RoutedEventArgs e)
        {
            Inicializar1();
        }

        MediaPlayer _mediaPlayerrrr;
        private async Task Inicializar1()
        {
            Show();
            MainWin.BeginAnimationP(UIElement.OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromSeconds(0.4)));

            if (!Directory.Exists(ExecSettings.EssenceFolder))
                Directory.CreateDirectory(ExecSettings.EssenceFolder);

            if (!Directory.Exists($"{ExecSettings.EssenceFolder}\\bin"))
                Directory.CreateDirectory($"{ExecSettings.EssenceFolder}\\bin");


            if (MainWindow.SettingsManager.Settings.Language == "default")
            {
                W2_page2.Visibility = Visibility.Collapsed;
                W2_page0.Visibility = Visibility.Visible;
            }
            TransL();

            if(!MainWindow.SettingsManager.Settings.FirstLogin)
            {
                iscreatingacc = false;

                Welcome_Page3_Title1.Text = "Welcome back!";

                Fade(signup_txt, 1, 0.7, 0.3);
                Fade(signupborder, 1, 0, 0.3);

                Fade(login_txt, 0.7, 1, 0.5);
                Fade(loginborder, 0, 1, 0.5);

                LoginBtnTXT.Text = "Log In";

                LoginTXT.Text = "Enter your email";
                PassTXT.Text = "Enter your password";

                name_border.Visibility = Visibility.Collapsed;
                forgotpass.Visibility = Visibility.Visible;
                //byb77b7tvb.Visibility = Visibility.Collapsed;
                email_in_use.Visibility = Visibility.Collapsed;
                Inviteshitty.Text = "Insert it here!";
            }
            else
            {
                iscreatingacc = true;

                Welcome_Page3_Title1.Text = "Sign up account";

                Fade(login_txt, 1, 0.7, 0.3);
                Fade(loginborder, 1, 0, 0.3);

                Fade(signup_txt, 0.7, 1, 0.5);
                Fade(signupborder, 0, 1, 0.5);

                LoginBtnTXT.Text = "Sign Up";

                LoginTXT.Text = "Enter an email";
                PassTXT.Text = "Create a password";

                name_border.Visibility = Visibility.Visible;
                forgotpass.Visibility = Visibility.Collapsed;

            }
        }




        private async void Mini(object sender, RoutedEventArgs e)
        {
            await Task.Delay(300);
            base.WindowState = WindowState.Minimized;
        }

        private bool CloseCompleted;
        private async void ExitMD(object sender, RoutedEventArgs e)
        {
            await Task.Delay(300);
            FormFadeOut.Begin();
        }

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


        private async void TransL()
        {
            try
            {
                Welcome_Page2_Title.Text = lm.Translate("Sorry, your language is not supported");
                Welcome_Page2_Desc.Text = lm.Translate("Choose one of the available languages below.");
                ConfirmLangButton.Content = lm.Translate("Confirm");

                Welcome_Page3_Title.Text = lm.Translate("Profile Settings");
                Welcome_Page3_Desc.Text = lm.Translate("Customize your account");
                Welcome_Page3_Name.Text = lm.Translate("Username");
                Welcome_Page3_Name_Copiar.Text = lm.Translate("Profile Picture");
                Welcome_Page3_Continue.Content = lm.Translate("Continue");
                user_avatar.Text = lm.Translate("Insert an image link here!");

                Welcome_Page4_Title.Text = lm.Translate("All Done!");
                Welcome_Page4_Desc.Text = lm.Translate("Now Your Essence are Fully Configured");
            }
            catch
            {
                await Task.Delay(1000);
                TransL();
            }
        }

        public void Fade(DependencyObject ElementName, double Start, double End, double Time)
        {
            DoubleAnimation Anims = new DoubleAnimation()
            {
                From = Start,
                To = End,
                Duration = TimeSpan.FromSeconds(Time),
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseInOut }
            };
            Storyboard.SetTarget(Anims, ElementName);
            Storyboard.SetTargetProperty(Anims, new PropertyPath(OpacityProperty)); // well i don't actually think transparency has this effect
            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(Anims);

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Send, () =>
            {
                storyboard.Begin();
            });
        }

        public void Move(DependencyObject ElementName, Thickness Origin, Thickness Location, double Time)
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

            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Send, () =>
            {
                storyboard.Begin();
            });
        }


        private void English_Btn_Click(object sender, RoutedEventArgs e)
        {
            Fade(dwhududnwi0, dwhududnwi0.Opacity, 1, 0.3);
            Fade(dwhududnwi1, dwhududnwi1.Opacity, 0, 0.3);
            Fade(dwhududnw2, dwhududnw2.Opacity, 0, 0.3);

            lm.lang = 1;

            try
            {
                TransL();
                MainWindow.SettingsManager.Settings.Language = "EN";
            }
            catch { }
        }

        private void Portuguese_Btn_Click(object sender, RoutedEventArgs e)
        {
            Fade(dwhududnwi0, dwhududnwi0.Opacity, 0, 0.3);
            Fade(dwhududnwi1, dwhududnwi1.Opacity, 1, 0.3);
            Fade(dwhududnw2, dwhududnw2.Opacity, 0, 0.3);

            lm.lang = 2;

            try
            {
                TransL();
                MainWindow.SettingsManager.Settings.Language = "PT";
            }
            catch { }
        }

        private void Spanish_Btn_Click(object sender, RoutedEventArgs e)
        {
            Fade(dwhududnwi0, dwhududnwi0.Opacity, 0, 0.3);
            Fade(dwhududnwi1, dwhududnwi1.Opacity, 0, 0.3);
            Fade(dwhududnw2, dwhududnw2.Opacity, 1, 0.3);

            lm.lang = 3;

            try
            {
                TransL();
                MainWindow.SettingsManager.Settings.Language = "ES";
            }
            catch { }
        }



        private bool animatiooo = false;

        private void Rectangle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {

                if (WindowState == WindowState.Maximized)
                {
                    WindowState = WindowState.Normal;

                    Width = 740;
                    Height = 457;

                    Point mousePosition = Mouse.GetPosition(System.Windows.Application.Current.MainWindow);
                    Point mousePositionOnScreen = System.Windows.Application.Current.MainWindow.PointToScreen(mousePosition);

                    double newLeft = mousePositionOnScreen.X - this.Left - (Width / 2);
                    double newTop = mousePositionOnScreen.Y - this.Top - 40;
                    this.Top = newTop;
                    this.Left = newLeft;
                }
                DragMove();
            }
            catch { }
        }

        private async void ConfirmLangButton_Click(object sender, RoutedEventArgs e)
        {
            if (animatiooo)
                return;

            animatiooo = true;

            Move(W2_page0, new Thickness(60, 0, 0, 0), new Thickness(-1395, 0, 0, 0), 1.2);
            Move(W2_page2, new Thickness(900, 0, 0, 0), new Thickness(0, 0, 0, 0), 1.2);
            W2_page2.Visibility = Visibility.Visible;

            await Task.Delay(1000);
            animatiooo = false;
        }


        private async void change_img1_Click(object sender, RoutedEventArgs e)
        {
            await Task.Delay(500);

            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Multiselect = false,
                Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif",
                Title = "Select an Image"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    string destinationDirectory = $"{ExecSettings.EssenceFolder}\\userdata\\UserImgs";
                    if (!Directory.Exists(destinationDirectory))
                    {
                        Directory.CreateDirectory(destinationDirectory);
                    }

                    string destinationFilePath = Path.Combine(destinationDirectory, Path.GetFileName(openFileDialog.FileName));
                    File.Copy(openFileDialog.FileName, destinationFilePath, true);

                    user_avatar.Text = destinationFilePath;
                    MainWindow.SettingsManager.Settings.Avatar = destinationFilePath;
                    await Task.Delay(200);

                    BitmapImage defaultBitmap = new BitmapImage(new Uri(destinationFilePath, UriKind.RelativeOrAbsolute));
                    ImageBehavior.SetAnimatedSource(User_Img2, defaultBitmap);
                }
                catch
                {

                }
            }
        }



        private async void finishbtn_Click(object sender, RoutedEventArgs e)
        {
            finishbtn.IsEnabled = false;
            await Task.Delay(600);

            MainWindow.SettingsManager.Settings.Name = user_name7.Text;
            MainWindow.SettingsManager.Settings.Avatar = user_avatar.Text;
            MainWindow.SettingsManager.Settings.FirstLogin = false;
            MainWindow.SettingsManager.Settings.Token = await Secure.UpdateJWT($"globalname:{user_name7.Text},avatar:{user_avatar.Text}");
            MainWindow.SettingsManager.SaveSettings();

            Fade(this, 1, 0, 2);
            await Task.Delay(2000);

            Process.Start(Process.GetCurrentProcess().MainModule.FileName);
            Process.GetCurrentProcess().Kill();
        }


        private void reset_img_Click(object sender, RoutedEventArgs e)
        {
            user_avatar.Text = AvatarImage;
        }










        bool iscreatingacc = true;
        List<string> usernames;
        private async void CreateAccount_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (iscreatingacc)
                {
                    if (UsernameTXT.Text.Length < 3)
                    {
                        invalid_username.Text = "Try a longer username";
                        invalid_username.Visibility = Visibility.Visible;
                        ShakeTextBox(UsernameTXT);
                        return;
                    }

                    if (!Regex.IsMatch(UsernameTXT.Text, @"^[a-zA-Z0-9_.]+$"))
                    {
                        invalid_username.Text = "Only letters, numbers, '.' and '_'";
                        invalid_username.Visibility = Visibility.Visible;
                        ShakeTextBox(UsernameTXT);
                        return;
                    }
                }

                if (!Regex.IsMatch(LoginTXT.Text, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
                {
                    invalid_email.Visibility = Visibility.Visible;
                    ShakeTextBox(LoginTXT);
                    return;
                }

                if (PassTXT.Text.Length <= 5)
                {
                    ShakeTextBox(PassTXT);
                    return;
                }
            }
            catch
            {
                return;
            }

            LoginTXT.IsEnabled = false;
            PassTXT.IsEnabled = false;
            UsernameTXT.IsEnabled = false;
            CreateAccount.IsEnabled = false;
            EnterUsingDiscord.IsEnabled = false;
            Gif6.Visibility = Visibility.Visible;
            Move(Gif6, new Thickness(0, 0, 0, -80), new Thickness(0, 0, 0, 0), 0.6);
            Move(LoginBtnTXT2, new Thickness(0, 0, 0, 0), new Thickness(0, -80, 0, 0), 0.4);
            Fade(Fader, 0, 0.7, 0.3);
            Fader.Visibility = Visibility.Visible;

            MainWindow.SettingsManager.Settings.Token = await Secure.UpdateJWT($"username:{UsernameTXT.Text},login:{LoginTXT.Text},password:{PassTXT.Text},region:{MainWindow.SettingsManager.Settings.Language}");
            await Task.Delay(1200);

            string response = "";
            if (iscreatingacc)
                response = await Communications.Post("externals/excregister");
            else
                response = await Communications.Post("externals/exclogin");

            response = response.Replace("'","\"");

            try
            {
                var responseObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(response);

                if (responseObject != null && responseObject.ContainsKey("data"))
                {
                    var userData = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseObject["data"].ToString());
                    try
                    {
                        user_name7.Text = userData["username"].ToString();
                        user_avatar.Text = userData["avatar"].ToString();                        

                        await Task.Delay(200);

                        BitmapImage bitmap = new BitmapImage(new Uri(userData["avatar"].ToString(), UriKind.RelativeOrAbsolute));
                        ImageBehavior.SetAnimatedSource(User_Img2, bitmap);
                    }
                    catch { }

                    if (animatiooo)
                        return;

                    animatiooo = true;
                    Move(W2_page2, new Thickness(0, 0, 0, 0), new Thickness(-1495, 0, 0, 0), 1.2);

                    Move(W2_page32, new Thickness(900, 0, 0, 0), new Thickness(0, 0, 0, 0), 1.2);
                    W2_page32.Visibility = Visibility.Visible;

                    await Task.Delay(1200);
                    animatiooo = false;
                }
                else if (responseObject != null && responseObject.ContainsKey("message"))
                {
                    LoginTXT.IsEnabled = true;
                    PassTXT.IsEnabled = true;
                    UsernameTXT.IsEnabled = true;
                    switch (responseObject["message"]?.ToString() ?? "Unknown Error")
                    {
                        case "user-created":
                            discc = false;
                            InsertInviteBorder.Visibility = Visibility.Visible;
                            Move(InsertInviteBorder, new Thickness(0, 0, 0, -300), new Thickness(0, 0, 0, -5), 0.6);
                            break;

                        case "username-already-exists":
                            if (responseObject.ContainsKey("usernames"))
                                usernames = JsonConvert.DeserializeObject<List<string>>(responseObject["usernames"].ToString());

                            CloseEmailConfirmation();
                            username_in_use.Visibility = Visibility.Visible;
                            ShakeTextBox(UsernameTXT);
                            break;

                        case "email-already-exist":
                            CloseEmailConfirmation();
                            email_in_use.Visibility = Visibility.Visible;
                            ShakeTextBox(UsernameTXT);
                            break;

                        case "new-location-detected" or "verification-pending":
                            Welcome_Page3_Title1_Copiar2.Text = "We've sent a code to '" + LoginTXT.Text + "'";
                            EmailConfirmationPage2.Visibility = Visibility.Visible;
                            Move(EmailConfirmationPage2, new Thickness(0, 40, 0, -300), new Thickness(0, 40, 0, 0), 0.6);

                            await Task.Delay(300);

                            ClearEmailCodes();
                            dfuwaduwbdu = true;
                            break;

                        case "multiple-machines-using-this-account":
                            MessageBox.Show("Other machine is already using this account and you don't have a signature that supports that.");
                            CloseEmailConfirmation();
                            break;

                        case "incorrect-password-login":
                            CloseEmailConfirmation();
                            ShakeTextBox(LoginTXT);
                            ShakeTextBox(PassTXT);
                            break;

                        default:
                            MessageBox.Show(response, "Internal Server Error. Try Again.");
                            CloseEmailConfirmation();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                CloseEmailConfirmation();
                MessageBox.Show(ex.ToString());
            }
        }

        bool doinsdsd = false;
        private async void forgotpass_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (doinsdsd)
                return;
            doinsdsd = true;

            if(await Communications.Post("externals/pswchangereq", true) == "sent")
            {
                dsjadnjadn.Visibility = Visibility.Collapsed;
                Welcome_Page3_Title1_Copiar2.Text = "We just sent you a password reset link";
                EmailConfirmationPage2.Visibility = Visibility.Visible;
                Move(EmailConfirmationPage2, new Thickness(0, 0, 0, -300), new Thickness(0, 0, 0, -5), 0.6);
                return;
            }
            await Task.Delay(2000);
            MessageBox.Show("Could not send email");
            doinsdsd = false;
        }

        private void ClearEmailCodes()
        {
            digit1.Clear();
            digit1.IsEnabled = true;
            digit1.Focus();

            digit2.Clear();
            digit2.IsEnabled = true;

            digit3.Clear();
            digit3.IsEnabled = true;

            digit4.Clear();
            digit4.IsEnabled = true;

            digit5.Clear();
            digit5.IsEnabled = true;

            digit6.Clear();
            digit6.IsEnabled = true;

            Gif7.Visibility = Visibility.Collapsed;
            //ResendEmailCode.Visibility = Visibility.Visible;
        }


        private async void LoginTXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                await Task.Delay(500);
                invalid_email.Visibility = Visibility.Collapsed;
                email_in_use.Visibility = Visibility.Collapsed;
                if (LoginTXT.Text != "Enter an email" && LoginTXT.Text != "Enter your email")
                {
                    //if (!Regex.IsMatch(LoginTXT.Text, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
                    //    email_mal_formatado.Visibility = Visibility.Visible;

                    //if (userssss.Contains(LoginTXT.Text) && iscreatingacc && Regex.IsMatch(LoginTXT.Text, @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"))
                    //    email_in_use.Visibility = Visibility.Visible;
                }
            }
            catch { }
        }

        private void PassTXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (LoginTXT.Text.Length > 2 && PassTXT.Text.Length > 5)
                {
                    //loginbtn_border.Background = new SolidColorBrush(Color.FromRgb(200, 20, 20));
                }
                else
                {
                    //loginbtn_border.Background = new SolidColorBrush(Color.FromRgb(50, 50, 50));
                }
            }
            catch { }
        }



        private async void CloseEmailConfirmation()
        {
            Inviteshitty.IsEnabled = true;
            SkipInvitation.IsEnabled = true;

            dfuwaduwbdu = false;
            Fade(Fader, 0.7, 0, 0.3);
            await Task.Delay(200);
            Move(EmailConfirmationPage2, new Thickness(0, 0, 0, -5), new Thickness(0, 0, 0, -600), 0.4);
            await Task.Delay(100);
            Fader.Visibility = Visibility.Collapsed;

            await Task.Delay(300);
            Move(Gif6, new Thickness(0, 0, 0, 0), new Thickness(0, 0, 0, -80), 0.4);
            Move(LoginBtnTXT2, new Thickness(0, -80, 0, 0), new Thickness(0, 0, 0, 0), 0.6);

            EmailConfirmationPage2.Visibility = Visibility.Collapsed;

            LoginTXT.IsEnabled = true;
            PassTXT.IsEnabled = true;
            UsernameTXT.IsEnabled = true;

            CreateAccount.IsEnabled = true;
            EnterUsingDiscord.IsEnabled = true;

            Gif7.Visibility = Visibility.Collapsed;
            //ResendEmailCode.Visibility = Visibility.Visible;
        }



        private void EmailConfirmationPage_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            CloseEmailConfirmation();
        }

        //private void ResendEmailCode_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    Notificar2("Confirmation", "faaineifnaeifimaeif?");
        //}

        private async void checkcodee()
        {
            digit1.IsEnabled = false;
            digit2.IsEnabled = false;
            digit3.IsEnabled = false;
            digit4.IsEnabled = false;
            digit5.IsEnabled = false;
            digit6.IsEnabled = false;


            //ResendEmailCode.Visibility = Visibility.Collapsed;
            Gif7.Visibility = Visibility.Visible;

            await Task.Delay(2000);

            try
            {
                Topmost = false;
                MainWindow.SettingsManager.Settings.Token = await Secure.UpdateJWT($"emailcode:{digit1.Text + digit2.Text + digit3.Text + digit4.Text + digit5.Text + digit6.Text},invite:{Inviteshitty.Text}");
                string response = await Communications.Post("externals/verifycode");

                var responseObject = JsonConvert.DeserializeObject<Dictionary<string, object>>(response);

                if (responseObject != null && responseObject.ContainsKey("data"))
                {
                    var userData = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseObject["data"].ToString());
                    try
                    {
                        user_name7.Text = userData["name"].ToString();
                        user_avatar.Text = userData["avatar"].ToString();

                        await Task.Delay(200);

                        BitmapImage bitmap = new BitmapImage(new Uri(userData["avatar"].ToString(), UriKind.RelativeOrAbsolute));
                        ImageBehavior.SetAnimatedSource(User_Img2, bitmap);
                    }
                    catch { }

                    if (animatiooo)
                        return;

                    animatiooo = true;
                    Move(W2_page2, new Thickness(0, 0, 0, 0), new Thickness(-1495, 0, 0, 0), 1.2);

                    Move(W2_page32, new Thickness(900, 0, 0, 0), new Thickness(0, 0, 0, 0), 1.2);
                    W2_page32.Visibility = Visibility.Visible;

                    await Task.Delay(1200);
                    animatiooo = false;
                }
                else if (responseObject != null && responseObject.ContainsKey("message"))
                {
                    LoginTXT.IsEnabled = true;
                    PassTXT.IsEnabled = true;
                    UsernameTXT.IsEnabled = true;
                    switch (responseObject["message"]?.ToString() ?? "Unknown Error")
                    {
                        case "incorrect-code":
                            ShakeTextBox(digit1);
                            ShakeTextBox(digit2);
                            ShakeTextBox(digit3);
                            ShakeTextBox(digit4);
                            ShakeTextBox(digit5);
                            ShakeTextBox(digit6);
                            break;

                        default:
                            MessageBox.Show(response, "Error");
                            CloseEmailConfirmation();
                            break;
                    }
                }
                Topmost = true;
            }
            catch
            {
                CloseEmailConfirmation();
            }
        }



        private bool dfuwaduwbdu = false;
        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!dfuwaduwbdu) return;

            if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                if (Clipboard.ContainsText())
                {
                    string pastedText = Clipboard.GetText();

                    var matches = Regex.Matches(pastedText, @"\d");
                    string[] numericStrings = matches.Cast<Match>().Select(m => m.Value).Take(6).ToArray();

                    if (numericStrings.Length == 6)
                    {
                        digit1.Text = numericStrings[0];
                        digit2.Text = numericStrings[1];
                        digit3.Text = numericStrings[2];
                        digit4.Text = numericStrings[3];
                        digit5.Text = numericStrings[4];
                        digit6.Text = numericStrings[5];

                        checkcodee();
                    }

                }
            }
        }


        private void digit1_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key >= Key.D0 && e.Key <= Key.D9) || (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9))
                Dispatcher.BeginInvoke(new Action(() => digit2.Focus()), System.Windows.Threading.DispatcherPriority.Background);
            else if (e.Key != Key.Back && e.Key != Key.Delete)
                e.Handled = true;
        }

        private void digit2_KeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = !(e.Key >= Key.D0 && e.Key <= Key.D9 || e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9);

            if (e.Key == Key.Back || e.Key == Key.Delete)
            {
                if (digit2.Text != "")
                    digit2.Text = "";
                else
                {
                    digit1.Text = "";
                    digit1.Focus();
                }
            }
            else if ((e.Key >= Key.D0 && e.Key <= Key.D9) || (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9))
                Dispatcher.BeginInvoke(new Action(() => digit3.Focus()), System.Windows.Threading.DispatcherPriority.Background);
            else
                e.Handled = true;
        }

        private void digit3_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back || e.Key == Key.Delete)
            {
                if (digit3.Text != "")
                    digit3.Text = "";
                else
                {
                    digit2.Text = "";
                    digit2.Focus();
                }
            }
            else if ((e.Key >= Key.D0 && e.Key <= Key.D9) || (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9))
                Dispatcher.BeginInvoke(new Action(() => digit4.Focus()), System.Windows.Threading.DispatcherPriority.Background);
            else
                e.Handled = true;
        }

        private void digit4_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back || e.Key == Key.Delete)
            {
                if (digit4.Text != "")
                    digit4.Text = "";
                else
                {
                    digit3.Text = "";
                    digit3.Focus();
                }
            }
            else if ((e.Key >= Key.D0 && e.Key <= Key.D9) || (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9))
                Dispatcher.BeginInvoke(new Action(() => digit5.Focus()), System.Windows.Threading.DispatcherPriority.Background);
            else
                e.Handled = true;
        }

        private void digit5_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Back || e.Key == Key.Delete)
            {
                if (digit5.Text != "")
                    digit5.Text = "";
                else
                {
                    digit4.Text = "";
                    digit4.Focus();
                }
            }
            else if ((e.Key >= Key.D0 && e.Key <= Key.D9) || (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9))
                Dispatcher.BeginInvoke(new Action(() => digit6.Focus()), System.Windows.Threading.DispatcherPriority.Background);
            else
                e.Handled = true;
        }

        private async void digit6_KeyDown(object sender, KeyEventArgs e)
        {
            if (digit1.IsEnabled == false)
                return;

            if (e.Key == Key.Back || e.Key == Key.Delete)
            {
                if (digit6.Text != "")
                    digit6.Text = "";
                else
                {
                    digit5.Text = "";
                    digit5.Focus();
                }
            }
            else if ((e.Key >= Key.D0 && e.Key <= Key.D9) || (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9))
            {
                Dispatcher.BeginInvoke(new Action(() => checkcodee()), System.Windows.Threading.DispatcherPriority.Background);
            }
            else
                e.Handled = true;
        }



        private static int GetAvailablePort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            try
            {
                listener.Start();
                return ((IPEndPoint)listener.LocalEndpoint).Port;
            }
            catch
            {
                return 49160;
            }
            finally
            {
                listener.Stop();
            }
        }

        private async void OpenDCAuth()
        {
            MainWindow.SettingsManager.Settings.Token = await Secure.UpdateJWT($"invite:{Inviteshitty.Text}");

            DiscordConfirmationPage2.Visibility = Visibility.Visible;
            Move(DiscordConfirmationPage2, new Thickness(0, 0, 0, -300), new Thickness(0, 0, 0, -5), 0.6);

            int port = GetAvailablePort();
            Secure.DiscordAuth(port);
            await Task.Delay(3000);

            string url = $"http://localhost:{port}/";
            using var server = new HttpListener();
            server.Prefixes.Add(url);
            server.Start();

            var context = await server.GetContextAsync();
            if (context.Request.IsWebSocketRequest)
            {
                var webSocketContext = await context.AcceptWebSocketAsync(null);
                var webSocket = webSocketContext.WebSocket;

                var buffer = new byte[1024];
                while (webSocket.State == WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        try
                        {
                            string ermm = Encoding.UTF8.GetString(buffer, 0, result.Count);
                            MessageBox.Show(ermm);
                            Console.WriteLine(ermm);

                            string data = await Secure.Dec(ermm);

                            JObject jsonObj = JObject.Parse(data);
                            string id = jsonObj["id"].ToString();
                            string username = jsonObj["username"].ToString();
                            string avatar = jsonObj["avatar"].ToString();
                            string email = jsonObj["email"].ToString();

                            MainWindow.SettingsManager.Settings.Token = await Secure.UpdateJWT($"discordid:{id},login:{email}");
                            MainWindow.SettingsManager.Settings.Name = username;
                            MainWindow.SettingsManager.Settings.Avatar = avatar;

                            Username = username;
                            user_name7.Text = username;

                            AvatarImage = avatar;
                            user_avatar.Text = avatar;
                            ermmm();
                            return;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Could not connect to discord.");
                            await Task.Delay(400);
                            CloseDc();
                            return;
                        }
                    }
                }
            }
            else
            {
                context.Response.StatusCode = 400;
                context.Response.Close();
            }
        }

        private string Username = "";
        private string AvatarImage = "";
        private string last_auth_link = "";
        private async void EnterUsingDiscord_Click(object sender, RoutedEventArgs e)
        {
            //if (byb77b7tvb.Visibility == Visibility.Visible && Inviteshitty.Text.Length != 6 && MessageBox.Show("Proceed without adding an invite code?", "Question", MessageBoxButton.YesNo) == MessageBoxResult.No)
            //    return;


            Gif5.Visibility = Visibility.Visible;
            CreateAccount.IsEnabled = false;
            EnterUsingDiscord.IsEnabled = false;

            Move(Gif5, new Thickness(0, 0, 0, -80), new Thickness(0, 0, 0, 0), 0.6);
            Move(EnterUsingDiscordTXT, new Thickness(0, 0, 0, 0), new Thickness(0, -80, 0, 0), 0.4);

            Fade(Fader, 0, 0.7, 0.3);
            Fader.Visibility = Visibility.Visible;

            await Task.Delay(1200);

            InsertInviteBorder.Visibility = Visibility.Visible;
            Move(InsertInviteBorder, new Thickness(0, 0, 0, -300), new Thickness(0, 0, 0, -5), 0.6);
            discc = true;
        }

        private async void CloseDc()
        {
            Inviteshitty.IsEnabled = true;
            SkipInvitation.IsEnabled = true;
            Fade(Fader, 0.7, 0, 0.3);
            await Task.Delay(200);
            Move(DiscordConfirmationPage2, new Thickness(0, 0, 0, -5), new Thickness(0, 0, 0, -600), 0.4);
            await Task.Delay(100);
            Fader.Visibility = Visibility.Collapsed;


            await Task.Delay(300);
            DiscordConfirmationPage2.Visibility = Visibility.Collapsed;

            Move(Gif5, new Thickness(0, 0, 0, 0), new Thickness(0, 0, 0, -80), 0.4);
            Move(EnterUsingDiscordTXT, new Thickness(0, -80, 0, 0), new Thickness(0, 0, 0, 0), 0.6);

            LoginTXT.IsEnabled = true;
            PassTXT.IsEnabled = true;

            CreateAccount.IsEnabled = true;
            EnterUsingDiscord.IsEnabled = true;
        }

        private async void CloseDiscordConfirmationPage_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            CloseDc();
        }

        private void ReopenAuthLink_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start(last_auth_link);
        }


        private void LoginTXT_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (LoginTXT.Text == "Enter your email" || LoginTXT.Text == "Enter an email")
                LoginTXT.Clear();
        }

        private void PassTXT_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (PassTXT.Text == "Enter your password" || PassTXT.Text == "Create a password")
                PassTXT.Clear();
        }



        private async void ermmm()
        {
            Fade(Fader, 0.7, 0, 0.3);
            await Task.Delay(200);
            Move(DiscordConfirmationPage2, new Thickness(0, 0, 0, -5), new Thickness(0, 0, 0, -600), 0.4);
            await Task.Delay(100);
            Fader.Visibility = Visibility.Collapsed;
            await Task.Delay(300);

            animatiooo = true;
            Move(W2_page2, new Thickness(0, 0, 0, 0), new Thickness(-1495, 0, 0, 0), 1.2);
            Move(W2_page32, new Thickness(900, 0, 0, 0), new Thickness(0, 0, 0, 0), 1.2);
            W2_page32.Visibility = Visibility.Visible;

            await Task.Delay(1200);
            animatiooo = false;
        }

        private async void Continue2_Click(object sender, RoutedEventArgs e)
        {
            if (animatiooo)
                return;

            animatiooo = true;

            Move(W2_page32, new Thickness(0, 0, 0, 0), new Thickness(-1395, 0, 0, 0), 1.2);
            Move(W2_page42, new Thickness(900, 0, 0, 0), new Thickness(0, 0, 0, 0), 1.2);



            W2_page42.Visibility = Visibility.Visible;

            await Task.Delay(1200);
            W2_page32.Visibility = Visibility.Collapsed;
            animatiooo = false;
        }

        private void ShakeTextBox(System.Windows.Controls.TextBox textBox)
        {
            var animation = new DoubleAnimationUsingKeyFrames
            {
                Duration = new Duration(TimeSpan.FromSeconds(0.3)),
                RepeatBehavior = new RepeatBehavior(1)
            };

            var keyFrames = new DoubleKeyFrameCollection
            {
                new LinearDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0))),
                new LinearDoubleKeyFrame(-15, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.05))),
                new LinearDoubleKeyFrame(15, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.1))),
                new LinearDoubleKeyFrame(-15, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.15))),
                new LinearDoubleKeyFrame(15, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.2))),
                new LinearDoubleKeyFrame(-15, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.25))),
                new LinearDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.3)))
            };

            animation.KeyFrames = keyFrames;

            var transform = new TranslateTransform();
            textBox.RenderTransform = transform;

            transform.BeginAnimation(TranslateTransform.XProperty, animation);
        }



        private async void user_avatar_TextChanged(object sender, TextChangedEventArgs e)
        {
            //if (user_avatar.Text == "")
            //    user_avatar.Text = "https://i.imgur.com/hxNqiz8.png";

            Welcome_Page3_Continue.IsEnabled = false;

            if (Uri.TryCreate(user_avatar.Text, UriKind.RelativeOrAbsolute, out Uri? uriResult) && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps || uriResult.Scheme == Uri.UriSchemeFile))
            {
                try
                {
                    BitmapImage eee = new BitmapImage(new Uri("pack://application:,,,/Graphics/Images/Settings/Rolling-1s-100px.gif"));
                    ImageBehavior.SetAnimatedSource(User_Img2, eee);

                    if (uriResult.Scheme != Uri.UriSchemeFile)
                    {
                        using (HttpClient client = new HttpClient())
                        {
                            client.Timeout = TimeSpan.FromSeconds(15);
                            HttpResponseMessage response = await client.GetAsync(user_avatar.Text);
                            if (response.IsSuccessStatusCode)
                            {
                                eee = new BitmapImage(new Uri(user_avatar.Text, UriKind.RelativeOrAbsolute));
                                ImageBehavior.SetAnimatedSource(User_Img2, eee);
                            }
                            else
                            {
                                eee = new BitmapImage(new Uri("https://i.imgur.com/hxNqiz8.png", UriKind.RelativeOrAbsolute));
                                ImageBehavior.SetAnimatedSource(User_Img2, eee);
                            }
                        }
                    }
                    else
                    {
                        if (File.Exists(user_avatar.Text))
                        {
                            eee = new BitmapImage(new Uri(user_avatar.Text, UriKind.RelativeOrAbsolute));
                            ImageBehavior.SetAnimatedSource(User_Img2, eee);
                        }
                        else
                        {
                            eee = new BitmapImage(new Uri("https://i.imgur.com/hxNqiz8.png", UriKind.RelativeOrAbsolute));
                            ImageBehavior.SetAnimatedSource(User_Img2, eee);
                        }
                    }
                }
                catch
                {
                    BitmapImage eee = new BitmapImage(new Uri("https://i.imgur.com/hxNqiz8.png", UriKind.RelativeOrAbsolute));
                    ImageBehavior.SetAnimatedSource(User_Img2, eee);
                }
                Welcome_Page3_Continue.IsEnabled = true;
            }
        }

        private void Inviteshitty_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if(Inviteshitty.Text.Length == 0)
                {
                    Inviteshitty.Text = "Paste an invite code here!";
                    saudnsaud.Text = "I dont have one";
                }

                Inviteshitty.Text = Inviteshitty.Text.ToUpper();
                if (Inviteshitty.Text.Length == 6)
                {                    
                    inviteborder.BorderBrush = Brushes.Green;
                    saudnsaud.Text = "Confirm";
                }
                else
                {
                    inviteborder.BorderBrush = new SolidColorBrush(Color.FromRgb(40,40,40));
                    saudnsaud.Text = "I dont have one";
                }
            }
            catch { }
        }

        private void Inviteshitty_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Inviteshitty.Text == "Insert it here!")
                Inviteshitty.Text = "";
        }

        private void SignUpBtn_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            iscreatingacc = true;

            Welcome_Page3_Title1.Text = "Sign up account";

            Fade(login_txt, 1, 0.7, 0.3);
            Fade(loginborder, 1, 0, 0.3);

            Fade(signup_txt, 0.7, 1, 0.5);
            Fade(signupborder, 0, 1, 0.5);

            LoginBtnTXT.Text = "Sign Up";

            LoginTXT.Text = "Enter an email";
            PassTXT.Text = "Create a password";

            name_border.Visibility = Visibility.Visible;
            forgotpass.Visibility = Visibility.Collapsed;

            //if (KeyGay2.firstlogin)
            //    byb77b7tvb.Visibility = Visibility.Visible;
        }

        private void LogInBtn_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            iscreatingacc = false;

            Welcome_Page3_Title1.Text = "Welcome back!";

            Fade(signup_txt, 1, 0.7, 0.3);
            Fade(signupborder, 1, 0, 0.3);

            Fade(login_txt, 0.7, 1, 0.5);
            Fade(loginborder, 0, 1, 0.5);

            LoginBtnTXT.Text = "Log In";

            LoginTXT.Text = "Enter your email";
            PassTXT.Text = "Enter your password";

            name_border.Visibility = Visibility.Collapsed;
            forgotpass.Visibility = Visibility.Visible;
            //byb77b7tvb.Visibility = Visibility.Collapsed;
            email_in_use.Visibility = Visibility.Collapsed;
            Inviteshitty.Text = "Insert it here!";
        }


        bool discc = false;
        private async void SkipInvitation_Click(object sender, RoutedEventArgs e)
        {
            Inviteshitty.IsEnabled = false;
            SkipInvitation.IsEnabled = false;
            if (Inviteshitty.Text != "Paste an invite code here!")
                MainWindow.SettingsManager.Settings.Token = await Secure.UpdateJWT($"invite:{Inviteshitty.Text}");

            Move(InsertInviteBorder, new Thickness(0, 0, 0, -5), new Thickness(0, 0, 0, -600), 0.4);
            await Task.Delay(100);

            if (!discc)
            {
                Welcome_Page3_Title1_Copiar2.Text = "We've sent a code to '" + LoginTXT.Text + "'";
                EmailConfirmationPage2.Visibility = Visibility.Visible;
                Move(EmailConfirmationPage2, new Thickness(0, 0, 0, -300), new Thickness(0, 0, 0, -5), 0.6);

                await Task.Delay(300);
                ClearEmailCodes();
                dfuwaduwbdu = true;
            }
            else
            {
                OpenDCAuth();
            }
        }

        private void UsernameTXT_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (UsernameTXT.Text == "Enter an username")
                UsernameTXT.Clear();
        }

        private async void Image_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Fade(Fader, 0.7, 0, 0.3);
            await Task.Delay(200);
            Move(InsertInviteBorder, new Thickness(0, 0, 0, -5), new Thickness(0, 0, 0, -600), 0.4);
            if (discc)
            {
                CloseDc();
            }
            else
            {
                CloseEmailConfirmation();
            }
            await Task.Delay(100);
            Fader.Visibility = Visibility.Collapsed;
        }

        private void UsernameTXT_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (usernames.Contains(UsernameTXT.Text))
                    username_in_use.Visibility = Visibility.Visible;
                else
                    username_in_use.Visibility = Visibility.Collapsed;
            }
            catch { }
        }
    }
}
