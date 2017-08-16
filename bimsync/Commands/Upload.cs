#region Namespaces
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.ExtensibleStorage;
using RestSharp;
using BIM.IFC.Export.UI;
using System.IO.Compression;
#endregion

namespace bimsync.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class Upload : IExternalCommand
    {
        private string _path;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;

            using (Transaction tx = new Transaction(doc))
            {
                try
                {
                    //Refresh the token and save it
                    Token token = Services.RefreshToken(Properties.Settings.Default["Token"] as Token);
                    Properties.Settings.Default["Token"] = token;
                    Properties.Settings.Default.Save();

                    string access_token = token.access_token;

                    //Load the interface to select these project and model
                    UI.ModelSelection modelSelection = new UI.ModelSelection(access_token, doc,uiapp);

                    if (modelSelection.ShowDialog() == true)
                    {
                        tx.Start("Export to bimsync");

                        //If necessary, add Shared parameters to store bimsync model and project
                        AddSharedParameters(app, doc);

                        //Write the values to these parameters
                        WriteOnParam("project_id", doc, modelSelection.ProjectId);
                        WriteOnParam("model_id", doc, modelSelection.ModelId);

                        //Export IFC
                        ExportToIFC(doc, modelSelection);

                        CompressFile();

                        UploadTobimsync(modelSelection,access_token);

                        File.Delete(_path);

                        tx.Commit();

                        // Return Success
                        return Result.Succeeded;
                    }
                    else
                    {
                        return Autodesk.Revit.UI.Result.Cancelled;
                    }
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException exceptionCanceled)
                {
                    message = exceptionCanceled.Message;
                    if (tx.HasStarted() == true)
                    {
                        tx.RollBack();
                    }
                    return Autodesk.Revit.UI.Result.Cancelled;
                }
                catch (Exception ex)
                {
                    // unchecked exception cause command failed
                    message = ex.Message;
                    //Trace.WriteLine(ex.ToString());
                    if (tx.HasStarted() == true)
                    {
                        tx.RollBack();
                    }
                    return Autodesk.Revit.UI.Result.Failed;
                }
            }
        }

        public void ExportToIFC(Document doc, UI.ModelSelection modelSelection)
        {
            // Prepare the export options
            IFCExportOptions IFCOptions = new IFCExportOptions();

            IFCExportConfiguration selectedConfig = modelSelection.Configuration;

            ElementId activeViewId = GenerateActiveViewIdFromDocument(doc);
            selectedConfig.ActiveViewId = selectedConfig.UseActiveViewGeometry ? activeViewId.IntegerValue : -1;
            selectedConfig.UpdateOptions(IFCOptions, activeViewId);

            string folder = System.IO.Path.GetTempPath();
            string name = DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + System.IO.Path.GetFileNameWithoutExtension(doc.PathName) + ".ifc";

            _path = Path.Combine(folder, name);

            doc.Export(folder, name, IFCOptions);
        }

        public void CompressFile()
        {
            //Create new directory
            string directoryPath = Path.Combine(Path.GetDirectoryName(_path), Path.GetFileNameWithoutExtension(_path));
            DirectoryInfo directoryInfo = Directory.CreateDirectory(directoryPath);

            //copy IFC to this directory
            string resultingPath = Path.Combine(directoryPath, Path.GetFileName(_path));
            File.Move(_path, resultingPath);

            string zipPath = Path.ChangeExtension(_path, "zip");
            //string startPath = @"c:\example\start";
            //string zipPath = @"c:\example\result.zip";
            //string extractPath = @"c:\example\extract";
            ZipFile.CreateFromDirectory(directoryPath, zipPath);

            Directory.Delete(directoryPath,true);

            //Update _path
            _path = zipPath;
            
            //ZipFile.CreateFromDirectory(startPath, zipPath);
        }

        public void UploadTobimsync(UI.ModelSelection modelSelection, string access_token)
        {
            RestClient client = new RestClient("https://api.bimsync.com");
            string filename = Path.GetFileName(_path);

            //Upload the IFC model
            RestRequest revisionRequest = new RestRequest("v2/projects/" + modelSelection.ProjectId + "/revisions", Method.POST);
            revisionRequest.AddHeader("Authorization", "Bearer " + access_token);
            revisionRequest.AddHeader("Content-Type", "application/ifc");
            revisionRequest.AddHeader("Bimsync-Params", "{" +
                "\"callbackUrl\": \"http://127.0.0.1:63842/\"," +
                "\"comment\": \"" + modelSelection.Comment + "\"," +
                "\"filename\": \"" + filename + "\"," +
                "\"model\": \"" + modelSelection.ModelId + "\"}");

            byte[] data = File.ReadAllBytes(_path);
            revisionRequest.AddParameter("application/ifc", data, RestSharp.ParameterType.RequestBody);

            IRestResponse reponseUpload = client.Execute(revisionRequest);

            if (reponseUpload.ErrorException != null)
            {
                string message = "Opps! There has been an error while uploading your model. " + reponseUpload.ErrorException.Message;
                throw new Exception(message);
            }
        }

        private void AddSharedParameters(Application app, Document doc)
        {
            //Save the previous shared param file path
            string previousSharedParam = app.SharedParametersFilename;

            //Extract shared param to a txt file
            string tempPath = System.IO.Path.GetTempPath();
            string SPPath = Path.Combine(tempPath, "bimsyncSharedParameter.txt");

            if (!File.Exists(SPPath))
            {
                //extract the familly
                List<string> files = new List<string>();
                files.Add("bimsyncSharedParameter.txt");
                Services.ExtractEmbeddedResource(tempPath, "bimsync.Resources", files);
            }

            //set the shared param file
            app.SharedParametersFilename = SPPath;

            //Define a category set containing the project properties
            CategorySet myCategorySet = app.Create.NewCategorySet();
            Categories categories = doc.Settings.Categories;
            Category projectCategory = categories.get_Item(BuiltInCategory.OST_ProjectInformation);
            myCategorySet.Insert(projectCategory);

            //Retrive shared parameters
            DefinitionFile myDefinitionFile = app.OpenSharedParameterFile();

            DefinitionGroup definitionGroup = myDefinitionFile.Groups.get_Item("bimsync");

            foreach (Definition paramDef in definitionGroup.Definitions)
            {
                // Get the BingdingMap of current document.
                BindingMap bindingMap = doc.ParameterBindings;

                //the parameter does not exist
                if (!bindingMap.Contains(paramDef))
                {
                    //Create an instance of InstanceBinding
                    InstanceBinding instanceBinding = app.Create.NewInstanceBinding(myCategorySet);

                    bindingMap.Insert(paramDef, instanceBinding, BuiltInParameterGroup.PG_IDENTITY_DATA);
                }
                //the parameter is not added to the correct categories
                else if (bindingMap.Contains(paramDef))
                {
                    InstanceBinding currentBinding = bindingMap.get_Item(paramDef) as InstanceBinding;
                    currentBinding.Categories = myCategorySet;
                    bindingMap.ReInsert(paramDef, currentBinding, BuiltInParameterGroup.PG_IDENTITY_DATA);
                }
            }

            //Reset to the previous shared parameters text file
            app.SharedParametersFilename = previousSharedParam;
            File.Delete(SPPath);

        }

        private void WriteOnParam(string paramId, Document doc, string value)
        {
            ProjectInfo projectInfoElement = doc.ProjectInformation;

            IList<Autodesk.Revit.DB.Parameter> parameters = projectInfoElement.GetParameters(paramId);
            if (parameters.Count != 0)
            {
                Autodesk.Revit.DB.Parameter p = parameters.FirstOrDefault();
                if (!p.IsReadOnly)
                {
                    p.Set(value);
                }
            }
        }

        private ElementId GenerateActiveViewIdFromDocument(Document doc)
        {
            try
            {
                Autodesk.Revit.DB.View activeView = doc.ActiveView;
                ElementId activeViewId = (activeView == null) ? ElementId.InvalidElementId : activeView.Id;
                return activeViewId;
            }
            catch
            {
                return ElementId.InvalidElementId;
            }
        }
    }
}
