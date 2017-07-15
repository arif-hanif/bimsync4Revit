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

namespace bimsync
{


    public static class Services
    {
        public const string client_id = "hl94XJLXaQe3ogX";
        public const string client_secret = "ZbwjiwgwWHAwcBj";

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
