#region Namespaces
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using RestSharp;
using System.Net;
#endregion

namespace bimsync.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class Login : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData,ref string message,ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            try
            {
                Token token = AuthorizeApp();
                Properties.Settings.Default["Token"] = token;
                Properties.Settings.Default.Save();

                UI.Ribbon.HideInitialPanel();
                UI.Ribbon.ShowLoggedPanel();

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                // unchecked exception cause command failed
                message = ex.Message;

                return Autodesk.Revit.UI.Result.Failed;
            }
        }

        public Token AuthorizeApp()
        {
            Uri baseUrl = new Uri("https://api.bimsync.com");

            //Get Revit windows handles
            IntPtr revitWindow = Services.GetForegroundWindow();

            // Creates a redirect URI using an available port on the loopback address.
            string redirectURI = string.Format("http://{0}:{1}/", IPAddress.Loopback, "63842");// GetRandomUnusedPort());

            // Creates an HttpListener to listen for requests on that redirect URI.
            var http = new HttpListener();
            http.Prefixes.Add(redirectURI);
            http.Start();

            // Creates the OAuth 2.0 authorization request.
            string authorizationRequest = string.Format("{0}/oauth2/authorize?response_type=code&redirect_uri={1}&client_id={2}&state={3}",
                                                        baseUrl.OriginalString,
                                                        System.Uri.EscapeDataString(redirectURI),
                                                        bimsync.Services.client_id,
                                                        "1");

            // Opens request in the browser.
            Process browserProcess = Process.Start(authorizationRequest);

            //Get the browser in front
            Services.SetForegroundWindow(browserProcess.MainWindowHandle);

            // Waits for the OAuth authorization response.
            HttpListenerContext context = http.GetContext();

            System.Threading.Thread.Sleep(200);

            // Brings Revit back to the foreground.
            Services.SetForegroundWindow(revitWindow);

            // Sends an HTTP response to the browser.
            var response = context.Response;
            string responseString = string.Format("<html><head><meta http-equiv='refresh' content='5;url=https://www.bimsync.com'></head><body>You can now return to Revit.</body></html>");
            var buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            var responseOutput = response.OutputStream;
            System.Threading.Tasks.Task responseTask = responseOutput.WriteAsync(buffer, 0, buffer.Length)
                .ContinueWith((task) =>
                {
                    responseOutput.Close();
                    http.Stop();
                    Console.WriteLine("HTTP server stopped.");
                });

            // Checks for errors.

            //RawUrl = "/?state=1&error=access_denied"
            if (context.Request.QueryString.Get("error") != null)
            {
                throw new Exception(String.Format("OAuth authorization error: {0}.", context.Request.QueryString.Get("error")));
            }
            if (context.Request.QueryString.Get("code") == null
                || context.Request.QueryString.Get("state") == null)
            {
                throw new Exception("Malformed authorization response. " + context.Request.QueryString);
            }

            // extracts the code
            var code = context.Request.QueryString.Get("code");
            var incoming_state = context.Request.QueryString.Get("state");

            // Compares the receieved state to the expected value, to ensure that
            // this app made the request which resulted in authorization.
            if (incoming_state != "1")
            {
                throw new Exception(String.Format("Received request with invalid state ({0})", incoming_state));
            }

            RestClient client = new RestClient("https://api.bimsync.com");

            //Request the access token
            RestRequest accessTokenRequest = new RestRequest("oauth2/token", Method.POST);
            //accessTokenRequest.AddHeader("Accept", "application/json");
            accessTokenRequest.AddHeader("Content-Type", "application/x-www-form-urlencoded");

            accessTokenRequest.AddParameter("code", code);
            accessTokenRequest.AddParameter("grant_type", "authorization_code");
            accessTokenRequest.AddParameter("client_id", Services.client_id);
            accessTokenRequest.AddParameter("client_secret", Services.client_secret);
            accessTokenRequest.AddParameter("redirect_uri", redirectURI);

            IRestResponse<Token> responseToken = client.Execute<Token>(accessTokenRequest);

            if (responseToken.ErrorException != null)
            {
                string message = "Error retrieving your access token. " + responseToken.ErrorException.Message;
                throw new Exception(message);
            }

            return responseToken.Data;
        }
    }
}
