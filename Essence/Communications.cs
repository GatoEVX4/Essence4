using Essence.Properties;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Shapes;

namespace Essence
{
    public class Communications
    {
        internal static event Action OFF;
        internal static event Action MODIFY;
        internal static event Action NET;
        internal static event Action E429;

        internal static event Action Cancel_Error;
        public delegate void StreamResponse(string response);
        public delegate void srvcall(string response);

        public static void ServerCallback(string response)
        {
            Console.WriteLine("Stream de dados recebido: " + response);
        }

        internal static CancellationTokenSource cts = new CancellationTokenSource();
        internal static string last_reason = "Reason: Tryed to chnage server response";
        internal static readonly string url = "https://essenceapi.discloud.app/";


        [DllImport("Auth.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr PostReq(string claims);


        [DllImport("Auth.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr RequestResource(string res, string extradata, srvcall srvcall, StreamResponse nigga = null, bool force_return = false);

        [DllImport("Auth.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr InitAuth(string jwt, srvcall srvcall);


        internal static void START()
        {
            IntPtr authRequestPtr = InitAuth("lol", ServerCallback);
            string authRequest = Marshal.PtrToStringAnsi(authRequestPtr);
            Console.WriteLine("Auth Request: " + authRequest);
        }




        internal static async Task<string> Post(string data)
        {
            IntPtr resultPtr = PostReq(data);
            string result = Marshal.PtrToStringAnsi(resultPtr);
            Marshal.FreeCoTaskMem(resultPtr);
            return result;
        }

        internal static async Task<string> RequestResource(string res, object extradata = null, bool force_return = false, Action<string> nigga = null)
        {
            StreamResponse streamdata = null;
            if (nigga != null)
                streamdata = (response) => nigga(response);            

            IntPtr resultPtr = RequestResource(res, extradata?.ToString() ?? "Essence", ServerCallback, streamdata, force_return);
            string result = Marshal.PtrToStringAnsi(resultPtr);
            Marshal.FreeCoTaskMem(resultPtr);
            return result;
        }
    }
}