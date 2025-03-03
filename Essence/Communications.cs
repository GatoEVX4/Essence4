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

        internal static List<string> robloxversions2;
        internal static string robloxlocalcurrent;


        internal static CancellationTokenSource cts = new CancellationTokenSource();
        internal static string last_reason = "Reason: Tryed to chnage server response";
        internal static readonly string url = "https://essenceapi.discloud.app/";


        public Communications()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_SystemEnclosure");

            foreach (ManagementObject obj in searcher.Get())
            {
                if (obj["ChassisTypes"] != null)
                {
                    var chassisTypes = (ushort[])obj["ChassisTypes"];
                    foreach (var type in chassisTypes)
                    {
                        if (type == 8 || type == 9 || type == 10)
                        {
                            Secure.pctypeshit = "Notebook";
                            break;
                        }
                        else
                        {
                            Secure.pctypeshit = "Desktop";
                        }
                    }
                }
            }
        }

        static async Task<bool> TestServer(string url)
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


        internal static async Task Ban(string id)
        {
            await RequestResource("moderation");
        }





        internal static async Task<string> RetryUntilSuccess(string final, bool encrypt)
        {
            int hidh = 1;
            while (true)
            {
                await Task.Delay(2000 * hidh);
                string result = await Get(final, encrypt, true);

                if (result != "Erro" && result != "777" && result != "429")
                {
                    Cancel_Error?.Invoke();

                    await Task.Delay(3000);
                    return result;
                }

                if (hidh < 4)
                    hidh++;
            }
        }


        private static bool repeat;
        internal static async Task<string> Get(string final, bool encrypted = true, bool force_return = false)
        {
            string data = Properties.Settings.Default.Token;
            if (App.dnu3ndf3ndn23nd && (IsBeingDebugged() || System.Diagnostics.Debugger.IsAttached))
            {
                if (force_return)
                    return "Erro";

                if (repeat)
                {
                    repeat = false;
                    last_reason = "The server response was not protected";
                    MODIFY?.Invoke();
                }
                else
                    repeat = true;

                return await RetryUntilSuccess(final, encrypted);
            }


            //MessageBox.Show(final);
            try
            {
                if (!NetworkInterface.GetIsNetworkAvailable())
                {
                    if (!await TestServer("http://www.msftconnecttest.com/connecttest.txt"))
                    {
                        if (force_return)
                            return "Erro";

                        NET?.Invoke();
                        return await RetryUntilSuccess(final, encrypted);
                    }
                }

                using HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Add("DeviceKind", Secure.pctypeshit);
                string original_authencity = Secure.authenticate_request(data);
                client.DefaultRequestHeaders.Add("authenticity", original_authencity);

                using (SHA256 sha256 = SHA256.Create())
                {
                    client.DefaultRequestHeaders.Add("Request-HASH", BitConverter.ToString(sha256.ComputeHash(Encoding.UTF8.GetBytes("Nigger" + data))).Replace("-", "").ToLower());
                }

                client.Timeout = TimeSpan.FromSeconds(9.5);
                var content = new StringContent(data, Encoding.UTF8, "application/x-www-form-urlencoded");


                HttpResponseMessage response = await client.PostAsync(url + final, content);

                if ((int)response.StatusCode == 429 && force_return)
                {
                    return "429";
                }
                if (response.IsSuccessStatusCode)
                {
                    string resposta = await response.Content.ReadAsStringAsync();

                    if (encrypted)
                    {
                        if (resposta.Length < 40)
                        {
                            if (force_return)
                                return "777";

                            if (repeat)
                            {
                                repeat = false;
                                OFF?.Invoke();
                            }
                            else
                                repeat = true;

                            return await RetryUntilSuccess(final, encrypted);
                        }

                        if (resposta.Split('.').Length != 2)
                        {
                            if (force_return)
                                return "Erro";

                            if (repeat)
                            {
                                repeat = false;

                                last_reason = "The server response was not protected";
                                MODIFY?.Invoke();
                            }
                            else
                                repeat = true;

                            return await RetryUntilSuccess(final, encrypted);
                        }

                        if(resposta.Split('.')[1] != original_authencity.Split('|')[0].Split('.')[1])
                        {
                            if (force_return)
                                return "Erro";

                            if (repeat)
                            {
                                repeat = false;
                                last_reason = "Response seed does not match request seed";
                                MODIFY?.Invoke();
                            }
                            else
                                repeat = true;

                            return await RetryUntilSuccess(final, encrypted);
                        }


                        string re = Secure.Dec(resposta);

                        if (re == "Errorr24542")
                        {
                            if (force_return)
                                return "Erro";

                            last_reason = "User tried to change server response";
                            MODIFY?.Invoke();
                            return await RetryUntilSuccess(final, encrypted);
                        }

                        Cancel_Error?.Invoke();
                        return re;
                    }
                    else
                        return resposta;
                }
                else
                {
                    if (force_return)
                        return "777";

                    OFF?.Invoke();
                    return await RetryUntilSuccess(final, encrypted);
                }
            }
            catch (HttpRequestException ex)
            {
                if (ex.HResult.ToString() == "-2146233088")
                {
                    if (force_return)
                        return "Erro";

                    NET?.Invoke();
                    return await RetryUntilSuccess(final, encrypted);
                }
                else
                {
                    if (force_return)
                        return "777";

                    OFF?.Invoke();
                    return await RetryUntilSuccess(final, encrypted);
                }
            }
            catch (TaskCanceledException)
            {
                if (await TestServer("http://www.msftconnecttest.com/connecttest.txt"))
                {
                    if (force_return)
                        return "777";

                    OFF?.Invoke();
                }
                else
                {
                    if (force_return)
                        return "Erro";

                    NET?.Invoke();
                }

                return await RetryUntilSuccess(final, encrypted);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Fatal Error when communicating with server");
                return "Erro";
            }
        }




        private static ClientWebSocket client;
        private static bool isConnected = false;
        private static bool starting_connection = false;
        static async Task StartWebSocketConnection()
        {
            if (isConnected) return;

            if (starting_connection)
            {
                while (starting_connection)
                    await Task.Delay(1000);

                return;
            }


            starting_connection = true;

            if (!await TestServer("https://essenceapi.discloud.app"))
            {
                if (!await TestServer("http://www.msftconnecttest.com/connecttest.txt"))
                    NET.Invoke();
                else
                    OFF.Invoke();

                await ReconnectWebSocket();
                return;
            }

            Uri serverUri = new Uri("ws://essenceapi.discloud.app/internals/heartbeat/");
            client = new ClientWebSocket();

            try
            {
                string authh = Secure.authenticate_request();
                client.Options.SetRequestHeader("authenticity", authh);
                client.Options.SetRequestHeader("essence-token", Properties.Settings.Default.Token);
                await client.ConnectAsync(serverUri, cts.Token);
                isConnected = true;
                starting_connection = false;
                Cancel_Error.Invoke();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to connect: {ex.Message}");
                await Task.Delay(3500);
                await ReconnectWebSocket();
            }
        }

        static async Task CloseConnectionAsync()
        {
            if (client != null && client.State == WebSocketState.Open)
            {
                Console.WriteLine("[INFO] Fechando conexão WebSocket.");
                await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Fechando conexão", CancellationToken.None);
                client?.Dispose();
                client = null;
                isConnected = false;
            }
        }

        static async Task ReconnectWebSocket()
        {
            Console.WriteLine("[INFO] Tentando reconectar...");
            await CloseConnectionAsync();
            await Task.Delay(2000);
            await StartWebSocketConnection();
        }


        static void filhodaputa(string vsf = "")
        {
            audsdnsadns = false;
            last_reason = vsf;
            MODIFY?.Invoke();
        }

        static bool audsdnsadns = false;
        internal static async Task<string> RequestResource(string res, object extradata = null, bool force_return = false, Action<string> nigga = null)
        {
            if (!isConnected || client == null || client.State != WebSocketState.Open)
            {
                await ReconnectWebSocket();
                return await RequestResource(res, extradata);
            }

            if (extradata == null)
                extradata = "Essence";

            string original_authencity = Secure.authenticate_request(JsonConvert.SerializeObject(extradata));
            string n = original_authencity.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries)[0].Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries)[1];

            var message = new
            {
                resource = res,
                authencity = original_authencity,
                data = extradata
            };
            byte[] byteArray = Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(message));

            try
            {
                await client.SendAsync(new ArraySegment<byte>(byteArray), WebSocketMessageType.Text, true, CancellationToken.None);
              
                byte[] buffer = new byte[1024];
                StringBuilder responseMessage = new StringBuilder();
                while (client.State == WebSocketState.Open && !cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        var result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token);
                        if (result.MessageType == WebSocketMessageType.Text)
                        {
                            responseMessage.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
                            if (nigga != null)
                                nigga(Encoding.UTF8.GetString(buffer, 0, result.Count));

                            if (result.EndOfMessage)
                                break;
                        }
                        else if (result.MessageType == WebSocketMessageType.Close)
                        {
                            await ReconnectWebSocket();
                            return await RequestResource(res, extradata, force_return, nigga);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());

                        await Task.Delay(2000);
                        if (!force_return)
                            return await RequestResource(res, extradata, force_return, nigga);
                        else
                            return "error";
                    }
                }

                JObject json = JObject.Parse(responseMessage.ToString());
                if (json.ContainsKey("authenticity") && json.ContainsKey("response"))
                {
                    int xd = Secure.authenticate_response(json["authenticity"].ToString(), n, json["response"].ToString());
                    if (xd == 0)
                        return json["response"].ToString();

                    string msg = "";
                    switch (xd)
                    {
                        case 1:
                            msg = "missing authenticity values";
                            break;
                        case 2:
                            msg = "response seed does not match request seed";
                            break;
                        case 3:
                            msg = "key pair does not match";
                            break;
                        case 4:
                            msg = "response hash does not match response text";
                            break;
                    }

                    if (audsdnsadns)
                        filhodaputa(msg);
                    else
                        audsdnsadns = true;

                    return await RequestResource(res, extradata, force_return, nigga);
                }

                if (force_return)
                    return "error";
                else
                    return await RequestResource(res, extradata, force_return, nigga);
            }
            catch (WebSocketException ex)
            {
                Console.WriteLine($"[ERROR] Falha ao enviar dados: {ex.Message}");

                if (force_return)
                    return "error";

                await ReconnectWebSocket();
                return await RequestResource(res, extradata, force_return, nigga);
            }
        }

        [DllImport("kernel32.dll")]
        public static extern int GetTickCount();

        public static bool IsBeingDebugged()
        {
            int ticksBefore = GetTickCount();
            System.Threading.Thread.Sleep(100);
            int ticksAfter = GetTickCount();

            return ticksAfter < ticksBefore;
        }
    }
}