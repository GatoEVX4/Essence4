using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace EssenceExperiments
{
    class Program
    {

        private static readonly string K1 = D(new int[] { 102, 123, 123, 61, 120, 96, 115, 109, 51, 51, 36, 51, 98, 125, 113, 115, 63, 62, 119, 100, 96, 84, 49, 119, 111, 63, 127, 125, 56, 108, 98, 109 });

        private static readonly string K2 = D(new int[] { 58, 51, 127, 120, 98, 109, 108, 126, 101, 119, 49, 98, 115, 51, 122, 124, 56, 103, 58, 100, 127, 57, 127, 57, 125, 56, 111, 115, 49, 58, 104, 121 });

        private static readonly string H_S1 = D(new int[] { 120, 110, 110, 111, 99, 119, 98, 110, 49, 100, 58, 125, 106, 58, 58, 111, 57, 60, 63, 102, 100, 104, 126, 106, 111, 108, 57, 102, 62, 62, 100, 96 });

        private static readonly string H_S2 = D(new int[] { 120, 113, 125, 60, 96, 122, 125, 111, 127, 119, 100, 99, 100, 61, 102, 110, 120, 104, 126, 109, 49, 120, 51, 103, 62, 84, 100, 108, 99, 125, 115, 103 });

        private static string D(int[] x)
        {
            return new string(Enumerable.Reverse(x).Select(v => (char)((v ^ 0xA) + (3 * (v % 2 == 0 ? 1 : -1)))).ToArray());
        }

        static void Authy()
        {
            StreamWriter sw = new StreamWriter(".\\lol.txt");




            string key = "dki5tr2bz8amqz72vxtk6166dvmu4nno";
            int[] obfuscated = key.Reverse().Select(c => ((int)c + (3 * (c % 2 == 0 ? 1 : -1))) ^ 0xA).ToArray();
            sw.WriteLine("private static readonly string VAR_NAME = D(new int[] { " + string.Join(", ", obfuscated) + " });");

            key = "pe38vb5t0r0rq3j5ys6vk8zlwidkur63";
            obfuscated = key.Reverse().Select(c => ((int)c + (3 * (c % 2 == 0 ? 1 : -1))) ^ 0xA).ToArray();
            sw.WriteLine("private static readonly string VAR_NAME = D(new int[] { " + string.Join(", ", obfuscated) + " });");


            key = "mq77o0ibcweqo290b33ct3q8gkzfbggu";
            obfuscated = key.Reverse().Select(c => ((int)c + (3 * (c % 2 == 0 ? 1 : -1))) ^ 0xA).ToArray();
            sw.WriteLine("private static readonly string VAR_NAME = D(new int[] { " + string.Join(", ", obfuscated) + " });");


            key = "jvtfiqa7j6u8dweugo4qfqzrbtsm9txu";
            obfuscated = key.Reverse().Select(c => ((int)c + (3 * (c % 2 == 0 ? 1 : -1))) ^ 0xA).ToArray();
            sw.WriteLine("private static readonly string VAR_NAME = D(new int[] { " + string.Join(", ", obfuscated) + " });");

            sw.WriteLine(K1);
            sw.WriteLine(K2);
            sw.WriteLine(H_S1);
            sw.WriteLine(H_S2);
            sw.Close();
        }


        public delegate void srvcall(string response);

        public static void ServerCallback(string response)
        {
            Console.WriteLine("Stream de dados recebido: " + response);
        }


        [DllImport("Auth.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr InitAuth(string jwt, srvcall srvcall);
        static void Main(string[] args)
        {
            Console.WriteLine("hhh");
            try
            {                
                IntPtr authRequestPtr = InitAuth("lol", ServerCallback);
                string authRequest = Marshal.PtrToStringAnsi(authRequestPtr);
                Console.WriteLine("Auth Request: " + authRequest);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Erro: " + ex.Message);
            }

            Console.ReadLine();
        }
    }
}
