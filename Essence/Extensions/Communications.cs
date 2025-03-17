using Essence.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

#nullable disable
namespace Essence
{
    class Communications
    {
        internal static event Action OFF;
        internal static event Action MODIFY;
        internal static event Action NET;
        internal static event Action E429;

        internal static event Action Online;
        public delegate void StreamResponse(string response);
        public delegate void Srvcall(IntPtr response);
        static Srvcall serverCallbackInstance = new Srvcall(ServerCallback);
        internal static readonly string url = "https://essenceapi.discloud.app/";

        [DllImport("Auth.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr PostReq(string final, bool force_return, Srvcall Srvcall);

        [DllImport("Auth.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr RequestResource(string res, string extradata, Srvcall Srvcall, StreamResponse nigga = null, bool force_return = false);


        internal static async Task<bool> TestServer(string url)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(5);
                    HttpResponseMessage response = await client.GetAsync(url);
                    return response.IsSuccessStatusCode;
                }
            }
            catch
            {
                return false;
            }
        }

        public static void ServerCallback(IntPtr response)
        {
            try
            {
                string message = Marshal.PtrToStringUTF8(response);
                Console.WriteLine(message);
                switch (message)
                {
                    case "server-online":
                        Online.Invoke();
                        break;
                    case "server-offline":
                        OFF.Invoke();
                        break;
                    case "pc-offline":
                        NET.Invoke();
                        break;
                    default:
                        last_reason = message;
                        MODIFY.Invoke();
                        break;
                }
            }
            catch { }
        }





        [DllImport("Auth.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr InitServer(string jwt, Srvcall srvcall);


        [DllImport("Auth.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void CloseServer();
        


        internal static string last_reason = "Client changed server response";




        internal static async Task<string> START()
        {
            string result = "";
            await Task.Run(() =>
            {
                IntPtr resultPtr = InitServer(MainWindow.SettingsManager.Settings.Token, ServerCallback);
                result = Marshal.PtrToStringUTF8(resultPtr);
            });
            return result;
        }




        internal static async Task<string> Post(string final, bool force_return = false)
        {
            string result = "";
            await Task.Run(() =>
            {
                IntPtr resultPtr = PostReq(final, force_return, ServerCallback);
                result = Marshal.PtrToStringUTF8(resultPtr);
            });
            return result;
        }

        internal static async Task<string> RequestResource(string res, string extradata = null, bool force_return = false, Action<string> streamdata0 = null)
        {
            string result = "";
            await Task.Run(() =>
            {
                StreamResponse streamdata = null;
                if (streamdata0 != null)
                    streamdata = (response) => streamdata0(response);

                IntPtr resultPtr = RequestResource(res, extradata?.ToString() ?? "Essence", ServerCallback, streamdata, force_return);
                result = Marshal.PtrToStringAnsi(resultPtr);

            });
            return result;
        }


        [DllImport("Auth.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GenDownloadRA();
        internal static async Task<bool> DownloadFile(string url, string destinationPath, Action<double> progress = null, bool auth = true)
        {
            string directoryPath = Path.GetDirectoryName(destinationPath);
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            using (HttpClient client = new HttpClient())
            {
                if (auth)
                {
                    await Task.Run(() =>
                    {
                        client.DefaultRequestHeaders.Add("authenticity", Marshal.PtrToStringAnsi(GenDownloadRA()));
                    });
                }

                client.Timeout = TimeSpan.FromSeconds(10);

                HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

                if (response.IsSuccessStatusCode)
                {
                    long totalBytes = response.Content.Headers.ContentLength ?? 0;
                    long downloadedBytes = 0;

                    using (Stream contentStream = await response.Content.ReadAsStreamAsync(),
                           fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        byte[] buffer = new byte[2048];
                        int bytesRead;
                        while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, bytesRead);

                            if (progress != null)
                            {
                                downloadedBytes += bytesRead;
                                if (totalBytes > 0)
                                {
                                    double progress2 = (double)downloadedBytes / totalBytes * 100;
                                    progress(progress2);
                                }
                            }
                        }
                    }

                    FileInfo fileInfo = new FileInfo(destinationPath);
                    double fileSize = fileInfo.Length / 1024.0;
                    if (fileSize < 1)
                        throw new NotImplementedException();

                    return true;
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }
    }
}
