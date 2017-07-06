#region Namespaces
using System;
using System.Collections.Generic;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Reflection;
using System.IO;
#endregion

namespace bimsync
{
    class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication a)
        {
            return Result.Succeeded;
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }
    }

    class Icons
    {
        public static void CreateIcons(RibbonPanel bim42Panel)
        {
            //Retrive dll path
            string DllPath = Assembly.GetExecutingAssembly().Location;

            //Create contextual help
            string helpPath = Path.Combine(Path.GetDirectoryName(DllPath), "bimsyncHelp.html");
            ContextualHelp help = new ContextualHelp(ContextualHelpType.ChmFile, helpPath);

            //Add Login Button
            PushButtonData loginButton = new PushButtonData("loginButton", "Login", DllPath, "AlignTag.AlignLeft");
            loginButton.ToolTip = "Align Tags Left";
            loginButton.LargeImage = RetriveImage("AlignTag.Resources.AlignLeftLarge.png");
            loginButton.Image = RetriveImage("AlignTag.Resources.AlignLeftSmall.png");
            loginButton.SetContextualHelp(help);

            bim42Panel.AddItem(loginButton);

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
