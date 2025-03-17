using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Windows;
using System.IO;

namespace Essence
{
    internal class ExecSettings
    {
        internal static readonly string CurrentVersion = "1.1.0.0";
        internal static readonly string versiontype = "beta";

        internal static string build_number = "????";
        internal static string build_date = "????";
        internal static readonly string EssenceFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Essence");
    }

    public class general
    {
        public static List<string> robloxsupported { get; set; }
        public static List<string> robloxlocalversions { get; set; }
        public static string robloxlocalcurrent { get; set; }
        public static string DiscordId { get; set; }
        public static string RobloxId { get; set; } = "null";
    }

    public class musicevent
    {
        public static string name { get; set; }
        public static string from { get; set; }
        public static string song { get; set; }
        public static string id { get; set; }
        public static string users { get; set; }
        public static string time { get; set; }
        public static string duration { get; set; }
    }

    public class gameevent
    {
    }

    public class KeyInfo
    {
        public DateTime CreationDate { get; set; }
        public string author { get; set; }
        public string support { get; set; }
        public string duration { get; set; }
        public string transactionid { get; set; }
        public string owner { get; set; }
        public string exp { get; set; }
    }

    public class AuthorizedDevice
    {
        public string Hwid { get; set; }
        public bool Online { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastLogin { get; set; }
        public string LastLocation { get; set; } = "unknown";
        public string Device { get; set; } = "unknown";
        public string LastIp { get; set; }
        public string CreationIp { get; set; }
    }

    public class UserData
    {
        public DateTime RegistrationDate { get; set; }
        public List<AuthorizedDevice> AuthorizedDevices { get; set; }
        public string UserId { get; set; }
        public string UserRole { get; set; }
        public string pctypeshit { get; set; }
        public string invitecode { get; set; }
        public int invites { get; set; }

        public string Email { get; set; } = "null";
        public string Password { get; set; } = "null";


        public string Subscription { get; set; } = "null";
    }

    public class JWTData
    {
        public string login { get; set; }
        public string password { get; set; }
        public string lang { get; set; }
        public string hwid { get; set; }
    }
}
