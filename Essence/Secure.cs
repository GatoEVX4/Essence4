using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;


using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Markup;

namespace Essence
{
    internal class Secure
    {
        private static readonly string K1 = "dki5tr2bz8amqz72vxtk6166dvmu4nno";
        private static readonly string K2 = "pe38vb5t0r0rq3j5ys6vk8zlwidkur63";

        private static readonly string H_S1 = "mq77o0ibcweqo290b33ct3q8gkzfbggu";
        private static readonly string H_S2 = "jvtfiqa7j6u8dweugo4qfqzrbtsm9txu";

        internal static string pctypeshit = "";
        internal static string invitecode = "";
        internal static int invites = 0;
        internal static string Discord_ID = "";
        internal static string Roblox_IDS = "";
        internal static bool firstlogin = true;

        private static string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                foreach (byte t in bytes)
                {
                    builder.Append(t.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        static string B64UrlEncode(byte[] data)
        {
            string encoded = Convert.ToBase64String(data);
            encoded = encoded.Split('=')[0];  // Remove padding "="
            return encoded.Replace('+', '-').Replace('/', '_');  // Converte para Base64 URL-safe
        }

        static byte[] B64UrlDecode(string data)
        {
            string padded = data;
            while (padded.Length % 4 != 0)
            {
                padded += "=";
            }
            padded = padded.Replace('-', '+').Replace('_', '/');  // Converte para Base64 normal
            return Convert.FromBase64String(padded);
        }

        static string Gen()
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var result = new StringBuilder(30);

            for (int i = 0; i < 30; i++)
            {
                result.Append(chars[random.Next(chars.Length)]);
            }

            string chave = $"{K1}-{result}";

            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(chave));
                StringBuilder builder = new StringBuilder();

                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }

                return builder.ToString() + ":" + result.ToString();
            }
        }


        internal static string authenticate_request(string data = "Essence")
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                string lol = Gen();
                string request_hash = BitConverter.ToString(sha256.ComputeHash(Encoding.UTF8.GetBytes($"{H_S1}:{lol}:{data}:{H_S2}"))).Replace("-", "").ToLower();
                return $"{lol}|{request_hash}";
            }
        }

        internal static int authenticate_response(string authenticity, string original_seed, string data)
        {
            //checking authenticity format
            if (authenticity.Length < 30)
                return 1;

            //checking authenticity format
            var authenticity2 = authenticity.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            if (authenticity2.Length != 2 || authenticity2[0].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries).Length != 2)
                return 1;

            var chave = authenticity2[0].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[0];
            var n = authenticity2[0].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1];
            var requestHash = authenticity2[1];

            //checking if seed is the same as request
            if (n != original_seed)
                return 2;

            //checking if key is valid
            if (ComputeSha256Hash($"{K1}-{n}") != chave)
                return 3;           

            //checking if the response was alterated
            var computedHash = ComputeSha256Hash($"{H_S1}:{chave}:{n}:{data}:{H_S2}");
            if (computedHash != requestHash)
                return 4;

            //sucess
            return 0;
        }



        internal static string Enc(string texto, string seedr = null)
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] salt;

                if (seedr == null)
                {
                    salt = new byte[32];
                    rng.GetBytes(salt);
                }
                else
                    salt = B64UrlDecode(seedr);                

                string saltString = $"{K1}-{B64UrlEncode(salt)}";

                byte[] iv = new byte[16];
                Array.Copy(salt, iv, 16);

                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] key = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltString));
                    using (var aes = new AesManaged())
                    {
                        aes.Key = key;
                        aes.IV = iv;
                        aes.Mode = CipherMode.CBC;

                        byte[] textoBytes = Encoding.UTF8.GetBytes(texto);
                        int paddingLength = 16 - (textoBytes.Length % 16);
                        textoBytes = textoBytes.Concat(Enumerable.Repeat((byte)paddingLength, paddingLength)).ToArray();
                        using (var encryptor = aes.CreateEncryptor())
                        {
                            byte[] encryptedData = encryptor.TransformFinalBlock(textoBytes, 0, textoBytes.Length);
                            string encryptedText = B64UrlEncode(encryptedData);
                            string saltEncoded = B64UrlEncode(salt);

                            return $"{encryptedText}.{saltEncoded}";
                        }
                    }
                }
            }
        }

        internal static string Dec(string encryptedtoken)
        {
            try
            {
                byte[] encryptedData = B64UrlDecode(encryptedtoken.Split('.')[0]);
                byte[] iv = B64UrlDecode(encryptedtoken.Split('.')[1]);
                iv = iv.Length > 16 ? iv.Take(16).ToArray() : iv;
                byte[] salt = B64UrlDecode(encryptedtoken.Split('.')[1]);

                string saltString = $"{K1}-{B64UrlEncode(salt)}";
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] key = sha256.ComputeHash(Encoding.UTF8.GetBytes(saltString));

                    using (var aes = new AesManaged())
                    {
                        aes.Key = key;
                        aes.IV = iv;
                        aes.Mode = CipherMode.CBC;
                        aes.Padding = PaddingMode.PKCS7;

                        using (var decryptor = aes.CreateDecryptor())
                        {
                            byte[] decryptedData = decryptor.TransformFinalBlock(encryptedData, 0, encryptedData.Length);
                            string decryptedText = Encoding.UTF8.GetString(decryptedData).TrimEnd();
                            return decryptedText;
                        }
                    }
                }
            }
            catch
            {
                return "Errorr24542";
            }
        }





        static string DoJwt(string userid, string password, string region, string hwid)
        {
            var claims = new[]
            {
                new Claim("userid", userid),
                new Claim("password", password),
                new Claim("lang", region),
                new Claim("hwid", hwid)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(K2));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                //expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            string lol = tokenHandler.WriteToken(token);
            Properties.Settings.Default.Token = lol;
            return lol;
        }

        internal static string GetHWID()
        {
            string machineGuid = null;

            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography"))
            {
                try
                {
                    if (key != null)
                    {
                        machineGuid = key.GetValue("MachineGuid")?.ToString();
                    }
                }
                catch
                {
                    machineGuid = null;
                }
            }

            try
            {
                machineGuid = Regex.Replace(machineGuid, @"[^a-zA-Z0-9]", "");
            }
            catch (Exception ex)
            {
                machineGuid = null;
            }

            if (string.IsNullOrEmpty(machineGuid))
            {
                Random random = new Random();
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                string machineName = Environment.MachineName;
                string randomKey = new string(Enumerable.Range(0, 40 - machineName.Length).Select(_ => chars[random.Next(chars.Length)]).ToArray());

                machineGuid = machineName + randomKey + "RAMDOM-KEY";
            }
            else
            {
                Random random = new Random();
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                string randomKey = new string(Enumerable.Range(0, 40 - machineGuid.Length).Select(_ => chars[random.Next(chars.Length)]).ToArray());

                machineGuid = machineGuid + randomKey + "USING-REGS";
            }

            return machineGuid;
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
