#region Namespaces
using System;
using System.Collections.Generic;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
#endregion

namespace bimsync
{
    class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication a)
        {
            try
            {
                //Create the initial UI
                //Create the panel for the plugin
                RibbonPanel bimsyncPanel = a.CreateRibbonPanel("bimsync");

                //Create two sets of button
                UI.Ribbon.CreateInitialPanel(bimsyncPanel);
                UI.Ribbon.CreateLoggedPanel(bimsyncPanel);

                //check if the user is connected
                Token token = Properties.Settings.Default["Token"] as Token;
                if (string.IsNullOrEmpty(token.access_token))
                {
                    //There is no availlable token, we have to authorize the app
                    //Hide the logged UI 
                    UI.Ribbon.HideLoggedPanel();
                }
                //Test for an internet connection
                else if (!Services.CheckForInternetConnection())
                {
                    //There is no availlable token, we have to login first
                    //Hide the logged UI 
                    UI.Ribbon.HideLoggedPanel();
                }
                else
                {
                    //We have to check if the current token is still valid
                    Token newToken = Services.RefreshToken(token);
                    if (string.IsNullOrEmpty(newToken.access_token))
                    {
                        //The token is no longer valid, we have to authorize the app again
                        //Hide the logged UI
                        UI.Ribbon.HideLoggedPanel();
                    }
                    else
                    {
                        //The token is valid, we save the new one
                        Properties.Settings.Default["Token"] = newToken;
                        Properties.Settings.Default.Save();

                        //We show the logged UI
                        UI.Ribbon.HideInitialPanel();
                    }
                }

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                // Return Failure
                TaskDialog.Show("bimsync Error", ex.Message);
                return Result.Failed;
            }
        }

        public Result OnShutdown(UIControlledApplication a)
        {
            return Result.Succeeded;
        }
    }


}
