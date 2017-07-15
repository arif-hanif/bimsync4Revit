#region Namespaces
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using RestSharp;
using Autodesk.Revit.DB;
#endregion

namespace bimsync.UI
{
    /// <summary>
    /// Interaction logic for ModelSelection.xaml
    /// </summary>
    public partial class ModelSelection : Window
    {
        public ObservableCollection<Project> ProjectsList { get; set; }
        public ObservableCollection<Model> ModelsList { get; set; }
        public string Comment { get; set; }

        private string _projectId;
        public string ProjectId
        {
            get { return _projectId; }
        }

        private string _modelId;
        public string ModelId
        {
            get { return _modelId; }
        }

        private Document _doc;
        private string _access_token;

        public ModelSelection(string access_token, Document doc)
        {

            _doc = doc;
            _access_token = access_token;

            InitializeComponent();

            //Create the lists
            ProjectsList = new ObservableCollection<Project>();
            ModelsList = new ObservableCollection<Model>();

            GetUserProjects();
            GetUserModels();

            this.DataContext = this;
        }

        private void GetUserProjects()
        {
            RestClient client = new RestClient("https://api.bimsync.com");

            //Get users projects
            RestRequest projectsRequest = new RestRequest("v2/projects", Method.GET);
            projectsRequest.AddHeader("Authorization", "Bearer " + _access_token);

            IRestResponse<List<Project>> projectsResponse = client.Execute<List<Project>>(projectsRequest);

            ProjectsList.Clear();
            foreach (Project project in projectsResponse.Data)
            {
                ProjectsList.Add(project);
            }

            //Is there is an existing project id ?
            Project selectedProject = null;
            if (GetValueOrDefault("project_id") != "")
            {
                _projectId = GetValueOrDefault("project_id");
                selectedProject = projectsResponse.Data.Where(x => x.id == _projectId).FirstOrDefault();
                if (selectedProject != null)
                {
                    comboBoxProjects.SelectedItem = selectedProject;
                }
            }
            
            if (selectedProject == null)
            {
                selectedProject = ProjectsList.FirstOrDefault();
                comboBoxProjects.SelectedItem = selectedProject;
                _projectId = selectedProject.id;
            }
        }

        private void GetUserModels()
        {
            RestClient client = new RestClient("https://api.bimsync.com");

            //List all models in the project
            RestRequest modelsRequest = new RestRequest("v2/projects/" + _projectId + "/models", Method.GET);
            modelsRequest.AddHeader("Authorization", "Bearer " + _access_token);

            IRestResponse<List<Model>> modelsResponse = client.Execute<List<Model>>(modelsRequest);

            ModelsList.Clear();
            foreach (Model model in modelsResponse.Data)
            {
                ModelsList.Add(model);
            }


            //Is there is an existing project id ?
            Model selectedModel = null;
            if (GetValueOrDefault("model_id") != "")
            {
                _modelId = GetValueOrDefault("model_id");
                selectedModel = modelsResponse.Data.Where(x => x.id == _modelId).FirstOrDefault();
                if (selectedModel != null)
                {
                    comboBoxModels.SelectedItem = selectedModel;
                }
            }

            if (selectedModel == null)
            {
                selectedModel = ModelsList.FirstOrDefault();
                comboBoxModels.SelectedItem = selectedModel;
            }
        }

        private string GetValueOrDefault(string parameterName)
        {
            ProjectInfo projectInfoElement = _doc.ProjectInformation;
            //Check if the param exist
            if (projectInfoElement.GetParameters(parameterName).Count != 0)
            {
                Autodesk.Revit.DB.Parameter param = projectInfoElement.GetParameters(parameterName).FirstOrDefault();
                if (param.AsString() != null || param.AsString() != "")
                {
                    return param.AsString();
                }

                return "";
            }
            else
            {
                return "";
            }
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void Upload_Button_Click(object sender, RoutedEventArgs e)
        {
            Comment = commentTextBox.Text;
            this.DialogResult = true;
            this.Close();
        }

        private void comboBoxProjects_changed(object sender, SelectionChangedEventArgs e)
        {
            Project selectedProject = comboBoxProjects.SelectedItem as Project;
            _projectId = selectedProject.id;
            GetUserModels();
        }

        private void comboBoxModels_changed(object sender, SelectionChangedEventArgs e)
        {
            Model selectedModel = comboBoxModels.SelectedItem as Model;
            _modelId = selectedModel.id;
        }
    }
}
