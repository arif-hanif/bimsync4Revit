#region Namespaces
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
#endregion

namespace bimsync.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class Logout : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                //Wipe the token
                Token token = new Token();
                Properties.Settings.Default["Token"] = token;
                Properties.Settings.Default.Save();

                //Show the login panel
                UI.Ribbon.ShowInitialPanel();
                UI.Ribbon.HideLoggedPanel();

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
}
