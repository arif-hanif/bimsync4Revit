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
                //Token token = AuthorizeApp();
                // A new handler to handle request posting by the dialog
                ExternalEventAuthorizationHandler handler = new ExternalEventAuthorizationHandler();

                // External Event for the dialog to use (to post requests)
                ExternalEvent exEvent = ExternalEvent.Create(handler);

                // We give the objects to the new dialog;
                // The dialog becomes the owner responsible for disposing them, eventually.
                ExternalEventAuthorization externalEventAuthorizationAnswer = new ExternalEventAuthorization(exEvent, handler);

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                // unchecked exception cause command failed
                message = ex.Message;

                return Autodesk.Revit.UI.Result.Failed;
            }
        }


    }

    public class ExternalEventAuthorizationHandler : IExternalEventHandler
    {
        /// <summary>
        /// A public property to access the current HttpListenerContext
        /// </summary>
        /// 
        private HttpListenerContext context;
        public HttpListenerContext Context
        {
            get { return context; }
            set { context = value; }
        }

        /// <summary>
        /// A public property to access the current HttpListener
        /// </summary>
        /// 
        private HttpListener http;
        public HttpListener Http
        {
            get { return http; }
            set { http = value; }
        }

        /// <summary>
        /// A public property to access the current Revit Handle
        /// </summary>
        /// 
        private IntPtr revitWindow;
        public IntPtr RevitWindow
        {
            get { return revitWindow; }
            set { revitWindow = value; }
        }

        /// <summary>
        /// A public property to set the redirect URI
        /// </summary>
        /// 
        private string redirectURI;
        public string RedirectURI
        {
            get { return redirectURI; }
            set { redirectURI = value; }
        }

        public void Execute(UIApplication app)
        {
            try
            {
                AuthorizationAnswer();
            }
            catch (Exception ex)
            {
                // unchecked exception cause command failed
                string message = ex.Message;
                TaskDialog.Show("bimsync Error", message);
            }
        }

        public string GetName()
        {
            return "bimsync Login";
        }

        private void AuthorizationAnswer()
        {
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

            Token token = responseToken.Data;

            Properties.Settings.Default["Token"] = token;
            Properties.Settings.Default.Save();

            UI.Ribbon.HideInitialPanel();
            UI.Ribbon.ShowLoggedPanel();
        }

    }

    public partial class ExternalEventAuthorization
    {
        private ExternalEvent m_ExEvent;
        private ExternalEventAuthorizationHandler m_Handler;

        private HttpListener http;
        private IntPtr revitWindow;
        private string redirectURI;

        public ExternalEventAuthorization(ExternalEvent exEvent, ExternalEventAuthorizationHandler handler)
        {
            m_ExEvent = exEvent;
            m_Handler = handler;

            AuthorizeApp();

            AuthorizationAnswer();
        }

        private void AuthorizeApp()
        {
            Uri baseUrl = new Uri("https://api.bimsync.com");

            //Test for an internet connection
            if (!Services.CheckForInternetConnection())
            {
                throw new Exception("Your computer seems to be currently offline. Please connect it to the internet and try again.");
            }

            //Get Revit windows handles
            revitWindow = Services.GetForegroundWindow();
            m_Handler.RevitWindow = revitWindow;

            int portNumber = 63842;

            // Creates a redirect URI using an available port on the loopback address.
            redirectURI = string.Format("http://{0}:{1}/", IPAddress.Loopback, portNumber.ToString());// GetRandomUnusedPort());
            m_Handler.RedirectURI = redirectURI;

            //grant permissions to the redirectURI, to run HttpListener in non-admin mode
            if (!Services.IsAdministrator())
            {
                Services.AddAddress(redirectURI, Environment.UserDomainName, Environment.UserName);
            }

            // Creates an HttpListener to listen for requests on that redirect URI.
            if (http != null)
            {
                if (http.IsListening)
                {
                    http.Stop();
                }
            }

            http = new HttpListener();
            http.Prefixes.Add(redirectURI);

            //Catch a specific error on the HttpListener
            //https://stackoverflow.com/questions/4019466/httplistener-access-denied
            try
            {
                http.Start();
            }
            catch (HttpListenerException ex)
            {
                if (ex.ErrorCode == 183)
                {
                    throw new Exception("Something went wrong with the login process. Please restart Revit before trying again.");
                }

                throw new Exception("The application can't login without admin access right. " + ex.Message);
            }

            if (!Services.CheckForPortAvailability(IPAddress.Loopback.ToString(), portNumber))
            {
                http.Stop();
                throw new Exception("It seems that the port N°63842 is closed or unavailable at the moment. " +
                    "The URL " + redirectURI + " cannot be reached. " +
                    "The application could not connect to the bimsync server. Contact your system administrator.");
            }

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

            m_Handler.Http = http;

        }   

        private async void AuthorizationAnswer()
        {
            // Waits for the OAuth authorization response.
            //System.Threading.Tasks.Task<HttpListenerContext> 
            HttpListenerContext context = await http.GetContextAsync();

            m_Handler.Context = context;

            m_ExEvent.Raise();
        }


    }

}
