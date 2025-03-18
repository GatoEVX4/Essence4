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
using System.Text.RegularExpressions;

namespace EssenceExperiments
{
    class Program
    {
        static string CorrectLuaScript(string input)
        {
            // Esse regex procura por chamadas de função do tipo objeto:metodo("alguma string" 
            // que não estejam imediatamente seguidas de um ")" e insere o parêntese faltante.
            string pattern = @"(\w+[:.]\w+\(""[^""]*""\s*)(?!\))";
            string replacement = "$1)";
            input = Regex.Replace(input, pattern, replacement);
            return input;
        }



        static string kkk = @"lua
local Players = game:GetService(""Players""local RunService = game:GetService(""RunService""local UserInputService = game:GetService(""UserInputService""local LocalPlayer = Players.LocalPlayer

local function getClosestPlayer()
    local closestPlayer = nil
    local shortestDistance = math.huge

    for _, player in ipairs(Players:GetPlayers()) do
        if player ~= LocalPlayer and player.Character and player.Character:FindFirstChild(""HumanoidRootPart"" then
            local distance = (LocalPlayer.Character.HumanoidRootPart.Position - player.Character.HumanoidRootPart.Position).magnitude
            if distance < shortestDistance then
                closestPlayer = player
                shortestDistance = distance
            end
        end
    end

    return closestPlayer
end

local function aimAt(target)
    if LocalPlayer.Character and LocalPlayer.Character:FindFirstChildOfClass(""Humanoid"" and LocalPlayer.Character.Humanoid.Health > 0 then
        local camera = workspace.CurrentCamera
        local targetPosition = target.Character.HumanoidRootPart.Position + Vector3.new(0, target.Character.HumanoidRootPart.Size.Y / 2, 0)
        local _, lookAt = camera:WorldToScreenPoint(targetPosition)

        mousemoverel(lookAt.X - UserInputService:GetMouseLocation().X, lookAt.Y - UserInputService:GetMouseLocation().Y)
    end
end

RunService.RenderStepped:Connect(function()
    local target = getClosestPlayer()
    if target then
        aimAt(target)
    end
end)";
        static async Task Main(string[] args)
        {
            Console.WriteLine(CorrectLuaScript(kkk));
            return;
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
