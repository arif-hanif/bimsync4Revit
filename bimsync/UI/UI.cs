#region Namespaces
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.UI;
using System.Reflection;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
#endregion


namespace bimsync.UI
{
    class Ribbon
    {
        private static RibbonItem _loginButton;
        private static RibbonItem _uploadButton;
        private static SplitButton _accountButton;


        public static RibbonPanel CreateInitialPanel(RibbonPanel bimsyncPanel)
        {
            //Retrive dll path
            string DllPath = Assembly.GetExecutingAssembly().Location;

            //Create contextual help
            string helpPath = Path.Combine(Path.GetDirectoryName(DllPath), "bimsyncHelp.html");
            ContextualHelp help = new ContextualHelp(ContextualHelpType.ChmFile, helpPath);

            //Add Login Button
            PushButtonData loginButton = new PushButtonData("loginButton", "Login", DllPath, "bimsync.Commands.Login");
            loginButton.ToolTip = "Login to bimsync";
            loginButton.LargeImage = RetriveImage("bimsync.Resources.logo_large.png");
            loginButton.Image = RetriveImage("bimsync.Resources.logo_small.png");
            loginButton.SetContextualHelp(help);

            _loginButton = bimsyncPanel.AddItem(loginButton);

            return bimsyncPanel;
        }

        public static void HideInitialPanel()
        {
            _loginButton.Visible = false;
        }

        public static void ShowInitialPanel()
        {
            _loginButton.Visible = true;
        }

        public static RibbonPanel CreateLoggedPanel(RibbonPanel bimsyncPanel)
        {
            //Retrive dll path
            string DllPath = Assembly.GetExecutingAssembly().Location;

            //Create contextual help
            string helpPath = Path.Combine(Path.GetDirectoryName(DllPath), "bimsyncHelp.html");
            ContextualHelp help = new ContextualHelp(ContextualHelpType.ChmFile, helpPath);

            //Add Logged Buttons
            PushButtonData profileButton = new PushButtonData("profileButton", "Profile", DllPath, "bimsync.Commands.Profile");
            profileButton.ToolTip = "Open your bimsync account";
            profileButton.LargeImage = RetriveImage("bimsync.Resources.user_large.png");
            profileButton.Image = RetriveImage("bimsync.Resources.user_small.png");
            profileButton.SetContextualHelp(help);

            PushButtonData logoutButton = new PushButtonData("logoutButton", "Logout", DllPath, "bimsync.Commands.Logout");
            logoutButton.ToolTip = "Logout";
            logoutButton.LargeImage = RetriveImage("bimsync.Resources.power-off_large.png");
            logoutButton.Image = RetriveImage("bimsync.Resources.power-off_small.png");
            logoutButton.SetContextualHelp(help);

            SplitButtonData accountButton = new SplitButtonData("AccountButton", "Account");
            _accountButton = bimsyncPanel.AddItem(accountButton) as SplitButton;
            _accountButton.AddPushButton(profileButton);
            _accountButton.AddPushButton(logoutButton);

            //Add upload to bimsync Button
            PushButtonData uploadButton = new PushButtonData("uploadButton", "Upload", DllPath, "bimsync.Commands.Upload");
            uploadButton.ToolTip = "Upload your model to bimsync";
            uploadButton.LargeImage = RetriveImage("bimsync.Resources.cloud-upload_large.png");
            uploadButton.Image = RetriveImage("bimsync.Resources.cloud-upload_small.png");
            uploadButton.SetContextualHelp(help);

            _uploadButton = bimsyncPanel.AddItem(uploadButton);

            return bimsyncPanel;

        }

        public static void HideLoggedPanel()
        {
            _accountButton.Visible = false;
            _uploadButton.Visible = false;
        }

        public static void ShowLoggedPanel()
        {
            _accountButton.Visible = true;
            _uploadButton.Visible = true;
        }

        private static ImageSource RetriveImage(string imagePath)
        {
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(imagePath);

            switch (imagePath.Substring(imagePath.Length - 3))
            {
                case "jpg":
                    var jpgDecoder = new System.Windows.Media.Imaging.JpegBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                    return jpgDecoder.Frames[0];
                case "bmp":
                    var bmpDecoder = new System.Windows.Media.Imaging.BmpBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                    return bmpDecoder.Frames[0];
                case "png":
                    var pngDecoder = new System.Windows.Media.Imaging.PngBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                    return pngDecoder.Frames[0];
                case "ico":
                    var icoDecoder = new System.Windows.Media.Imaging.IconBitmapDecoder(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
                    return icoDecoder.Frames[0];
                default:
                    return null;
            }
        }
    }
}
