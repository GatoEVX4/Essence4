using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.IO.Compression;
using static System.Net.Mime.MediaTypeNames;
using System.Runtime.InteropServices;
using System.Threading;

namespace EssenceUpdater
{
    class Helpers
    {
        [DllImport("Auth.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int InitAuth();

        internal static async Task ExtractAll(string pastaOrigem, Action<string> nigga = null, Action<double> progress = null)
        {
            try
            {
                string[] arquivosZip = Directory.GetFiles(pastaOrigem, "*.zip", SearchOption.AllDirectories);

                long totalBytes = 0;
                long bytesExtraidos = 0;

                foreach (string zipPath in arquivosZip)
                {
                    using (ZipArchive archive = ZipFile.OpenRead(zipPath))
                    {
                        totalBytes += archive.Entries.Sum(entry => entry.Length);
                    }
                }

                foreach (string zipPath in arquivosZip)
                {
                    InstallWindow.ext = true;
                    string pastaDestino = Path.GetDirectoryName(zipPath);
                    if (!Directory.Exists(pastaDestino))
                    {
                        Directory.CreateDirectory(pastaDestino);
                    }

                    using (ZipArchive archive = ZipFile.OpenRead(zipPath))
                    {
                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            string destino = Path.Combine(pastaDestino, entry.FullName);

                            if (!Directory.Exists(Path.GetDirectoryName(destino)) && Path.GetDirectoryName(destino) != "Essence")
                            {
                                Directory.CreateDirectory(Path.GetDirectoryName(destino));
                            }

                            //check if the file is an directory
                            if (entry.Name.Contains("."))
                            {
                                using (FileStream destinoStream = new FileStream(destino, FileMode.Create, FileAccess.Write))
                                using (Stream entryStream = entry.Open())
                                {
                                    byte[] buffer = new byte[2048];
                                    int bytesLidos;

                                    int e = 0;
                                    while ((bytesLidos = entryStream.Read(buffer, 0, buffer.Length)) > 0)
                                    {
                                        //smooth
                                        e++;
                                        if(e == 100)
                                        {
                                            e = 0;
                                            await Task.Delay(10);
                                        }                                       


                                        if (InstallWindow.stopstuff)
                                        {
                                            try { destinoStream.Close(); } catch { }
                                            try { File.Delete(destino); } catch { }
                                            InstallWindow.stopstuff = false;
                                            InstallWindow.ext = false;
                                            return;
                                        }

                                        destinoStream.Write(buffer, 0, bytesLidos);

                                        if (nigga != null && progress != null)
                                        {
                                            bytesExtraidos += bytesLidos;
                                            double progresso = (double)bytesExtraidos / totalBytes * 100;
                                            progress(progresso);
                                            nigga($"[{progresso:F2}%] ({FormatBytes(bytesExtraidos)} of {FormatBytes(totalBytes)})");
                                        }
                                    }
                                }
                            }
                        }
                    }

                    await Task.Delay(100);

                    try { File.Delete(zipPath); } catch { }
                }
            }
            catch(Exception ex)
            {
                InstallWindow.stopstuff = false;
                InstallWindow.ext = false;
                MessageBox.Show(ex.ToString());
                throw new NotImplementedException();
            }
        }

        public static string FormatBytes(long bytes)
        {
            if (bytes < 1024)
                return $"{bytes} B";
            else if (bytes < 1048576)
                return $"{bytes / 1024.0:F2} KB";
            else if (bytes < 1073741824)
                return $"{bytes / 1048576.0:F2} MB";
            else
                return $"{bytes / 1073741824.0:F2} GB";
        }



        [DllImport("Auth.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GenDownloadRA();
        internal static async Task<bool> DownloadFile(string url, string destinationPath, Action<double> nigga = null, bool auth = true)
        {
            InstallWindow.dwn = true;
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
                            if (InstallWindow.stopstuff)
                            {
                                try { fileStream.Close(); } catch { }
                                try { File.Delete(destinationPath); } catch { }
                                return false;
                            }

                            await fileStream.WriteAsync(buffer, 0, bytesRead);

                            if (nigga != null)
                            {
                                downloadedBytes += bytesRead;
                                if (totalBytes > 0)
                                {
                                    double progress = (double)downloadedBytes / totalBytes * 100;
                                    nigga(progress);
                                }
                            }
                        }
                    }
                    InstallWindow.dwn = false;

                    FileInfo fileInfo = new FileInfo(destinationPath);
                    double fileSize = fileInfo.Length / 1024.0;
                    if(fileSize < 1)
                        throw new NotImplementedException();

                    return true;
                }
                else
                {
                    InstallWindow.dwn = false;
                    throw new NotImplementedException();
                }
            }
        }

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

        internal static string CalculateSHA256(string filePath)
        {
            try
            {
                using (FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    using (SHA256 sha256 = SHA256.Create())
                    {
                        byte[] hash = sha256.ComputeHash(file);
                        return BitConverter.ToString(hash).Replace("-", "").ToUpper();
                    }
                }
            }
            catch
            {
                return "e";
            }
        }
    }
}
