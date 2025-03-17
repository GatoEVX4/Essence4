using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using System.Diagnostics;
using System.Management;
using System.Net.NetworkInformation;
using System.Net.Http;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using System.IO.Compression;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Reflection;

namespace EssenceExperiments
{
    class Program
    {
        static async Task Main(string[] args)
        {
            while (true)
            {
                string src = "";
                using (SHA256 sha256Hash = SHA256.Create())
                {
                    string input = Console.ReadLine();
                    byte[] bytess = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes($"pe38vb5t0r0rq3j5ys6vk8zlwidkur63-{input}"));
                    StringBuilder builder = new StringBuilder();
                    foreach (byte b in bytess)
                    {
                        builder.Append(b.ToString("x2"));
                    }
                    src = builder.ToString() + ":" + input;
                    Console.WriteLine(src);
                }

                try
                {
                    using (TcpClient tcpClient = new TcpClient())
                    {
                        tcpClient.Connect("localhost", 4555);
                        using (NetworkStream networkStream = tcpClient.GetStream())
                        {
                            using (SHA256 sha256Hash = SHA256.Create())
                            {
                                byte[] bytes = Encoding.UTF8.GetBytes(src);
                                networkStream.Write(bytes, 0, bytes.Length);
                                networkStream.Flush();
                            }
                        }
                    }
                }
                catch
                {
                    Console.WriteLine("Could not send tcp req");
                }
            }
        }
    }
}
