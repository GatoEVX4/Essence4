using Microsoft.Win32;
using System;

using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;


using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IO;
using System.Windows;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;

namespace Essence
{
    internal class Secure
    {

        [DllImport("Auth.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr DoJwt(string claims);

        [DllImport("Auth.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr DecJwt(string token);

        [DllImport("Auth.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Enc(string texto, string seedr = "");

        [DllImport("Auth.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr Dec(string enctoken);

        [DllImport("Auth.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetHWID();

        [DllImport("Auth.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GenRequestAuth(string data = "Essence");


        [DllImport("Auth.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int CheckResponse(string authyauthenticity, string original_seed, string data);
        


        internal static string authenticate_request(string data = "Essence")
        {
            return Marshal.PtrToStringAnsi(GenRequestAuth());
        }

        internal static int authenticate_response(string a, string s, string d)
        {
            return CheckResponse(a, s, d);
        }

        internal static string DoJwt2(string u, string p, string r, string hwid)
        {
            string claims = $"userid:{u},password:{p},region:{r}";
            return Marshal.PtrToStringAnsi(DoJwt(claims));
        }


        internal static JWTData DecodeJwt(string t)
        {
            JObject json = JObject.Parse(Marshal.PtrToStringAnsi(DecJwt(t)));

            return new JWTData
            {
                login = json["response"]?.ToString() ?? null,
                password = json["response"]?.ToString() ?? null,
                lang = json["response"]?.ToString() ?? null,
                hwid = json["response"]?.ToString() ?? null
            };
        }

        internal static TimeSpan TimeLeft(string file)
        {
            try
            {
                if (!File.Exists(file))
                    return TimeSpan.FromMilliseconds(1);

                string dados = File.ReadAllText(file);

                if (dados.Split('.').Length != 2)
                    return TimeSpan.FromMilliseconds(1);                
                

                string result = Dec(dados);
                if (DateTime.TryParse(result, out DateTime savedDate))
                {
                    TimeSpan difference = savedDate - DateTime.Now;

                    if (difference.TotalMinutes < 12 * 60 && difference.TotalMinutes > 0)
                    {
                        return difference;
                    }
                }

                return TimeSpan.FromMilliseconds(1);
            }
            catch (Exception ex)
            {
                return TimeSpan.FromMilliseconds(1);
            }
        }
    }
}
