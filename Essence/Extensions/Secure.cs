using Essence.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

#nullable disable
namespace Essence
{
    class Secure
    {

        [DllImport("bin/Auth.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr DoJWT(string claims);

        [DllImport("bin/Auth.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Decrypt(string encrypted);

        [DllImport("bin/Auth.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetHWID();

        [DllImport("bin/Auth.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GenDownloadRA();


        [DllImport("bin/Auth.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DiscordAuth(int port);

        [DllImport("bin/Auth.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern long TimeLeft(string file);

        [DllImport("bin/Auth.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Exec(string script);

        [DllImport("bin/Auth.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int InitAuth(string tnk);


        public static string GenDownloadAuth()
        {
            return Marshal.PtrToStringUTF8(GenDownloadRA());
        }
        public static async Task<string> UpdateJWT(string claims)
        {
            string result = "";
            await Task.Run(() =>
            {
                result = Marshal.PtrToStringUTF8((DoJWT(claims)));
            });
            return result;
        }
        public static async Task<string> Dec(string encrypted)
        {
            string result = "";
            await Task.Run(() =>
            {
                result = Marshal.PtrToStringUTF8(Decrypt(encrypted));
            });
            return result;
        }
    }
}
