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
using Autodesk.Revit.UI;
using BIM.IFC.Export.UI;
#endregion

namespace bimsync.UI
{
    /// <summary>
    /// Interaction logic for ExportSettings.xaml
    /// </summary>
    public partial class ExportSettingsUI : Window
    {
        /// <summary>
        /// The map contains the configurations.
        /// </summary>
        private IFCExportConfigurationsMap m_configurationsMap;

        private IFCExportConfiguration _configuration;
        public IFCExportConfiguration Configuration
        {
            get { return _configuration; }
        }

        public ExportSettingsUI(IFCExportConfigurationsMap configurationsMap, String currentConfigName)
        {
            InitializeComponent();

            m_configurationsMap = configurationsMap;

            InitializeConfigurationList(currentConfigName);

        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void Ok_Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        /// <summary>
        /// Initializes the listbox by filling the available configurations from the map.
        /// </summary>
        /// <param name="currentConfigName">The current configuration name.</param>
        private void InitializeConfigurationList(String currentConfigName)
        {
            foreach (IFCExportConfiguration configuration in m_configurationsMap.Values)
            {
                configuration.Name = configuration.Name;
                listBoxConfigurations.Items.Add(configuration);
                if (configuration.Name == currentConfigName)
                    listBoxConfigurations.SelectedItem = configuration;
            }
        }

        /// <summary>
        /// Updates the controls after listbox selection changed.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event arguments that contains the event data.</param>
        private void listBoxConfigurations_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _configuration = (IFCExportConfiguration)listBoxConfigurations.SelectedItem;
        }
    }
}
