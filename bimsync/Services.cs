using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Xml.Serialization;
using System.IO;
using RestSharp;
using System.Runtime.InteropServices;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Security.Principal;

namespace bimsync
{


    public static class Services
    {
        public const string client_id = "0y7heCMCq1dDSoL";
        public const string client_secret = "xFlgdmtRb9rsceu";

        // DLL imports from user32.dll to set focus to
        // Revit to force it to forward the external event
        // Raise to actually call the external event 
        // Execute.

        /// <summary>
        /// The GetForegroundWindow function returns a 
        /// handle to the foreground window.
        /// </summary>
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        /// <summary>
        /// Move the window associated with the passed 
        /// handle to the front.
        /// </summary>
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        public static Token RefreshToken(Token token)
        {
            //Test for an internet connection
            if (!Services.CheckForInternetConnection())
            {
                throw new Exception("Your computer seems to be currently offline. Please connect it to the internet and try again.");
            }

            RestClient client = new RestClient("https://api.bimsync.com");

            //Refresh token
            RestRequest refrechTokenRequest = new RestRequest("oauth2/token", Method.POST);
            //refrechTokenRequest.AddHeader("Authorization", "Bearer " + token.access_token);

            refrechTokenRequest.AddParameter("refresh_token", token.refresh_token);
            refrechTokenRequest.AddParameter("grant_type", "refresh_token");
            refrechTokenRequest.AddParameter("client_id", client_id);
            refrechTokenRequest.AddParameter("client_secret", client_secret);

            IRestResponse<Token> responseToken = client.Execute<Token>(refrechTokenRequest);

            if (responseToken.ErrorException != null)
            {
                string message = "Error retrieving your access token. " + responseToken.ErrorException.Message;
                return new Token();
            }

            return responseToken.Data;
        }

        public static void ExtractEmbeddedResource(string outputDir, string resourceLocation, List<string> files)
        {
            foreach (string file in files)
            {
                using (System.IO.Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceLocation + @"." + file))
                {
                    using (System.IO.FileStream fileStream = new System.IO.FileStream(System.IO.Path.Combine(outputDir, file), System.IO.FileMode.Create))
                    {
                        for (int i = 0; i < stream.Length; i++)
                        {
                            fileStream.WriteByte((byte)stream.ReadByte());
                        }
                        fileStream.Close();
                    }
                }
            }
        }

        public static bool CheckForInternetConnection()
        {
            try
            {
                using (var client = new WebClient())
                {
                    using (var stream = client.OpenRead("http://www.google.com"))
                    {
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        public static bool CheckForPortAvailability(string hostURI, int portNum)
        {
            try
            {
                using (TcpClient tcpClient = new TcpClient())
                {
                    tcpClient.Connect(hostURI, portNum);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        //method stolen from an SO thread. sorry can't remember the author
        //grant permissions to a particular URL, to run HttpListener in non-admin mode 
        public static void AddAddress(string address, string domain, string user)
        {
            //netsh http add urlacl url=http://+:80/MyUri user=DOMAIN\user
            string args = string.Format(@"http add urlacl url={0}", address) + " user=\"" + domain + "\\" + user + "\"";

            ProcessStartInfo psi = new ProcessStartInfo("netsh", args);
            psi.Verb = "runas";
            psi.CreateNoWindow = true;
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.UseShellExecute = true;

            Process.Start(psi).WaitForExit();
        }

        public static bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }

    public class Project
    {
        public string id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string createdAt { get; set; }
        public string updatedAt { get; set; }
    }

    public class Model
    {
        public string id { get; set; }
        public string name { get; set; }
    }

    public class User
    {
        public string createdAt { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string username { get; set; }
    }

    public class Revision
    {
        public string comment { get; set; }
        public string createdAt { get; set; }
        public string id { get; set; }
        public Model model { get; set; }
        public User user { get; set; }
        public int version { get; set; }
    }

    [SettingsSerializeAs(SettingsSerializeAs.Xml)]
    public class Token
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
        public string refresh_token { get; set; }
    }
}
