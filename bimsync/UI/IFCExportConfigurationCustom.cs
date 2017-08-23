extern alias IFCExportUIOverride;
extern alias IFCExportUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.DB;


namespace bimsync.UI
{
    public class IFCExportConfigurationCustom
    {
        public IFCExportConfigurationCustom(IFCLibrary ifcLibrary, object ifcExportConfiguration)
        {
            _IFCLibrary = ifcLibrary;
            if (_IFCLibrary == IFCLibrary.Standard)
            {
                IFCExportConfigurationCustomStandard(ifcExportConfiguration);
            }
            else if (_IFCLibrary == IFCLibrary.Override)
            {
                IFCExportConfigurationCustomOvverided(ifcExportConfiguration);
            }
            else if (_IFCLibrary == IFCLibrary.Deprecated)
            {
                IFCExportConfigurationCustomDeprecated(ifcExportConfiguration);
            }
        }

        private void IFCExportConfigurationCustomStandard(object ifcExportConfiguration)
        {
            IFCExportUI::BIM.IFC.Export.UI.IFCExportConfiguration config = (IFCExportUI::BIM.IFC.Export.UI.IFCExportConfiguration)ifcExportConfiguration;
            _IFCExportConfiguration = config;
            _IFCVersion = config.IFCVersion;
            _Name = config.Name;
            _ActiveViewId = config.ActiveViewId;
            _UseActiveViewGeometry = config.UseActiveViewGeometry;
        }

        private void IFCExportConfigurationCustomOvverided(object ifcExportConfiguration)
        {
            IFCExportUIOverride::BIM.IFC.Export.UI.IFCExportConfiguration config = (IFCExportUIOverride::BIM.IFC.Export.UI.IFCExportConfiguration)ifcExportConfiguration;
            _IFCExportConfiguration = config;
            _IFCVersion = config.IFCVersion;
            _Name = config.Name;
            _ActiveViewId = config.ActiveViewId;
            _UseActiveViewGeometry = config.UseActiveViewGeometry;
        }

        private void IFCExportConfigurationCustomDeprecated(object ifcExportConfiguration)
        {
            IFCExportConfigurationDeprecated config = (IFCExportConfigurationDeprecated)ifcExportConfiguration;
            _IFCExportConfiguration = config;
            _IFCVersion = config.IFCVersion;
            _Name = config.Name;
            _ActiveViewId = config.ActiveViewId;
            _UseActiveViewGeometry = config.CurrentViewOnly;
        }

        private IFCLibrary _IFCLibrary;

        private IFCVersion _IFCVersion;
        public IFCVersion IFCVersion
        {
            get { return _IFCVersion; }
        }

        private string _Name;
        public string Name
        {
            get { return _Name; }
        }

        private int _ActiveViewId;
        public int ActiveViewId
        {
            set { _ActiveViewId = value; }
            get { return _ActiveViewId; }
        }

        private bool _UseActiveViewGeometry;
        public bool UseActiveViewGeometry
        {
            get { return _UseActiveViewGeometry; }
        }

        private object _IFCExportConfiguration;
        public object IFCExportConfiguration
        {
            get { return _IFCExportConfiguration; }
        }

        public void UpdateOptions(IFCExportOptions IFCOptions, ElementId activeViewId)
        {
            if (_IFCLibrary == IFCLibrary.Standard)
            {
                UpdateOptionStandard(IFCOptions,activeViewId);
            }
            else if (_IFCLibrary == IFCLibrary.Override)
            {
                UpdateOptionOverrided(IFCOptions, activeViewId);
            }
            else if (_IFCLibrary == IFCLibrary.Deprecated)
            {
                UpdateOptionDeprecated(IFCOptions, activeViewId);
            }
        }

        private void UpdateOptionStandard(IFCExportOptions IFCOptions, ElementId activeViewId)
        {
            IFCExportUI::BIM.IFC.Export.UI.IFCExportConfiguration config = _IFCExportConfiguration as IFCExportUI::BIM.IFC.Export.UI.IFCExportConfiguration;
            config.UpdateOptions(IFCOptions, activeViewId);
        }

        private void UpdateOptionOverrided(IFCExportOptions IFCOptions, ElementId activeViewId)
        {
            IFCExportUIOverride::BIM.IFC.Export.UI.IFCExportConfiguration config = _IFCExportConfiguration as IFCExportUIOverride::BIM.IFC.Export.UI.IFCExportConfiguration;
            config.UpdateOptions(IFCOptions, activeViewId);
        }

        private void UpdateOptionDeprecated(IFCExportOptions IFCOptions, ElementId activeViewId)
        {
            IFCExportConfigurationDeprecated config = _IFCExportConfiguration as IFCExportConfigurationDeprecated;

            IFCOptions.ExportBaseQuantities = config.ExportBaseQuantities;
            IFCOptions.FileVersion = config.IFCVersion;
            if (config.CurrentViewOnly) { IFCOptions.FilterViewId = activeViewId; }
            IFCOptions.SpaceBoundaryLevel = 1;
            IFCOptions.WallAndColumnSplitting = config.SplitWall;
        }
    }

    public class IFCExportConfigurationsMapCustom
    {
        public IFCExportConfigurationsMapCustom()
        {
            _Values = new List<IFCExportConfigurationCustom>();

            try
            {
                //Try the overrided UI. This should works in 2016, 2017 and 2018 when the custom IFC exporter is here
                GetUserIFCExportConfigurationOverrided();
            }
            catch (System.IO.FileNotFoundException ex)
            {
                string message = ex.Message;
                if (message.Contains("IFCExportUIOverride"))
                {
                    try
                    {
                        //Try the standard UI. This should always works in 2017 and 2018
                        GetUserIFCExportConfigurationStandard();
                    }
                    catch (System.IO.FileNotFoundException exDeprecated)
                    {
                        string messageOld = exDeprecated.Message;
                        if (message.Contains("IFCExportUI"))
                        {
                            //Try this for Revit 2016 without the custom IFC Exporter
                            GetUserIFCExportConfigurationDeprecated();
                        }
                        else
                        {
                            throw ex;
                        }
                    }
                    
                }
                else
                {
                    throw ex;
                }
            }
        }

        private List<IFCExportConfigurationCustom> _Values;
        public List<IFCExportConfigurationCustom> Values
        {
            get { return _Values; }
        }

        private void GetUserIFCExportConfigurationStandard()
        {
            //IFCExportUI::BIM.IFC.Export.UI.

            IFCExportUI::BIM.IFC.Export.UI.IFCExportConfigurationsMap configurationsMap = new IFCExportUI::BIM.IFC.Export.UI.IFCExportConfigurationsMap();
            configurationsMap.Add(CreateDefaultbimsyncConfiguration());
            configurationsMap.Add(IFCExportUI::BIM.IFC.Export.UI.IFCExportConfiguration.CreateDefaultConfiguration());
            configurationsMap.Add(IFCExportUI::BIM.IFC.Export.UI.IFCExportConfiguration.GetInSession());
            configurationsMap.AddBuiltInConfigurations();
            configurationsMap.AddSavedConfigurations();

            foreach (IFCExportUI::BIM.IFC.Export.UI.IFCExportConfiguration config in configurationsMap.Values)
            {
                _Values.Add(new IFCExportConfigurationCustom(IFCLibrary.Standard,config));
            }
        }

        private void GetUserIFCExportConfigurationOverrided()
        {
            //IFCExportUIOverride::BIM.IFC.Export.UI.

            IFCExportUIOverride::BIM.IFC.Export.UI.IFCExportConfigurationsMap configurationsMap = new IFCExportUIOverride::BIM.IFC.Export.UI.IFCExportConfigurationsMap();
            configurationsMap.Add(CreateOverrridedbimsyncConfiguration());
            configurationsMap.Add(IFCExportUIOverride::BIM.IFC.Export.UI.IFCExportConfiguration.CreateDefaultConfiguration());
            configurationsMap.Add(IFCExportUIOverride::BIM.IFC.Export.UI.IFCExportConfiguration.GetInSession());
            configurationsMap.AddBuiltInConfigurations();
            configurationsMap.AddSavedConfigurations();

            foreach (IFCExportUIOverride::BIM.IFC.Export.UI.IFCExportConfiguration config in configurationsMap.Values)
            {
                _Values.Add(new IFCExportConfigurationCustom(IFCLibrary.Override, config));
            }
        }

        private void GetUserIFCExportConfigurationDeprecated()
        {
            
            //Loop on all possibilities here
            IFCExportConfigurationDeprecated config = new IFCExportConfigurationDeprecated(false, false, true);
            config.Name = "<bimsync Config>";
            _Values.Add(new IFCExportConfigurationCustom(IFCLibrary.Deprecated, config));

            config = new IFCExportConfigurationDeprecated(false, false, false);
            _Values.Add(new IFCExportConfigurationCustom(IFCLibrary.Deprecated, config));

            config = new IFCExportConfigurationDeprecated(false, false, true);
            _Values.Add(new IFCExportConfigurationCustom(IFCLibrary.Deprecated, config));

            config = new IFCExportConfigurationDeprecated(false, true, true);
            _Values.Add(new IFCExportConfigurationCustom(IFCLibrary.Deprecated, config));

            config = new IFCExportConfigurationDeprecated(false, true, false);
            _Values.Add(new IFCExportConfigurationCustom(IFCLibrary.Deprecated, config));

            config = new IFCExportConfigurationDeprecated(true, false, true);
            _Values.Add(new IFCExportConfigurationCustom(IFCLibrary.Deprecated, config));

            config = new IFCExportConfigurationDeprecated(true, false, false);
            _Values.Add(new IFCExportConfigurationCustom(IFCLibrary.Deprecated, config));

            config = new IFCExportConfigurationDeprecated(true, true, true);
            _Values.Add(new IFCExportConfigurationCustom(IFCLibrary.Deprecated, config));

            config = new IFCExportConfigurationDeprecated(true, true, false);
            _Values.Add(new IFCExportConfigurationCustom(IFCLibrary.Deprecated, config));
        }

        //private IFCExportUI::BIM.IFC.Export.UI.IFCExportConfiguration CreateStandardConfiguration(IFCExportUIOverride::BIM.IFC.Export.UI.IFCExportConfiguration overridedConfiguration)
        //{
        //    IFCExportUI::BIM.IFC.Export.UI.IFCExportConfiguration standardConfig = IFCExportUI::BIM.IFC.Export.UI.IFCExportConfiguration.CreateDefaultConfiguration();

        //    //Copy the custom config into a stand one
        //    System.Reflection.PropertyInfo[] sourcePropertyInfos = overridedConfiguration.GetType().GetProperties();
        //    foreach (System.Reflection.PropertyInfo sourcePropertyInfo in sourcePropertyInfos)
        //    {
        //        var sourcePropertyValue = sourcePropertyInfo.GetValue(overridedConfiguration);
        //        var outputPropertyInfo = standardConfig.GetType().GetProperty(sourcePropertyInfo.Name);
        //        if (outputPropertyInfo != null)
        //        {
        //            Type t = Nullable.GetUnderlyingType(outputPropertyInfo.PropertyType) ?? outputPropertyInfo.PropertyType;
        //            object safeValue = (sourcePropertyValue == null) ? null : Convert.ChangeType(sourcePropertyValue, t);
        //            if (outputPropertyInfo.CanWrite)
        //            {
        //                outputPropertyInfo.SetValue(standardConfig, safeValue);
        //            }
        //        }
        //    }

        //    return standardConfig;
        //}

        private IFCExportUI::BIM.IFC.Export.UI.IFCExportConfiguration CreateDefaultbimsyncConfiguration()
        {
            IFCExportUI::BIM.IFC.Export.UI.IFCExportConfiguration selectedConfig = IFCExportUI::BIM.IFC.Export.UI.IFCExportConfiguration.CreateDefaultConfiguration();

            selectedConfig.Name = "<bimsync Setup>";
            selectedConfig.IFCVersion = IFCVersion.IFC2x3CV2;
            selectedConfig.SpaceBoundaries = 1;
            selectedConfig.ActivePhaseId = ElementId.InvalidElementId;
            selectedConfig.ExportBaseQuantities = true;
            selectedConfig.SplitWallsAndColumns = false;
            selectedConfig.VisibleElementsOfCurrentView = false;
            selectedConfig.Use2DRoomBoundaryForVolume = false;
            selectedConfig.UseFamilyAndTypeNameForReference = true;
            selectedConfig.ExportInternalRevitPropertySets = true;
            selectedConfig.ExportIFCCommonPropertySets = true;
            selectedConfig.Export2DElements = false;
            selectedConfig.ExportPartsAsBuildingElements = true;
            selectedConfig.ExportBoundingBox = false;
            selectedConfig.ExportSolidModelRep = false;
            selectedConfig.ExportSchedulesAsPsets = false;
            selectedConfig.ExportUserDefinedPsets = false;
            selectedConfig.ExportUserDefinedPsetsFileName = "";
            selectedConfig.ExportLinkedFiles = false;
            selectedConfig.IncludeSiteElevation = true;
            selectedConfig.UseActiveViewGeometry = false;
            selectedConfig.ExportSpecificSchedules = false;
            selectedConfig.TessellationLevelOfDetail = 0;
            selectedConfig.StoreIFCGUID = true;
            selectedConfig.ExportRoomsInView = true;

            return selectedConfig;
        }

        private IFCExportUIOverride::BIM.IFC.Export.UI.IFCExportConfiguration CreateOverrridedbimsyncConfiguration()
        {
            IFCExportUIOverride::BIM.IFC.Export.UI.IFCExportConfiguration selectedConfig = IFCExportUIOverride::BIM.IFC.Export.UI.IFCExportConfiguration.CreateDefaultConfiguration();

            selectedConfig.Name = "<bimsync Setup>";
            selectedConfig.IFCVersion = IFCVersion.IFC2x3CV2;
            selectedConfig.SpaceBoundaries = 1;
            selectedConfig.ActivePhaseId = ElementId.InvalidElementId;
            selectedConfig.ExportBaseQuantities = true;
            selectedConfig.SplitWallsAndColumns = false;
            selectedConfig.VisibleElementsOfCurrentView = false;
            selectedConfig.Use2DRoomBoundaryForVolume = false;
            selectedConfig.UseFamilyAndTypeNameForReference = true;
            selectedConfig.ExportInternalRevitPropertySets = true;
            selectedConfig.ExportIFCCommonPropertySets = true;
            selectedConfig.Export2DElements = false;
            selectedConfig.ExportPartsAsBuildingElements = true;
            selectedConfig.ExportBoundingBox = false;
            selectedConfig.ExportSolidModelRep = false;
            selectedConfig.ExportSchedulesAsPsets = false;
            selectedConfig.ExportUserDefinedPsets = false;
            selectedConfig.ExportUserDefinedPsetsFileName = "";
            selectedConfig.ExportLinkedFiles = false;
            selectedConfig.IncludeSiteElevation = true;
            selectedConfig.UseActiveViewGeometry = false;
            selectedConfig.ExportSpecificSchedules = false;
            selectedConfig.TessellationLevelOfDetail = 0;
            selectedConfig.StoreIFCGUID = true;
            selectedConfig.ExportRoomsInView = true;

            return selectedConfig;
        }


    }

    public class IFCExportConfigurationDeprecated
    {
        public IFCExportConfigurationDeprecated(
            bool currentViewOnly,
            bool splitWall,
            bool exportBaseQuantities)
        {
            CurrentViewOnly = currentViewOnly;
            SplitWall = splitWall;
            ExportBaseQuantities = exportBaseQuantities;
            Name = String.Format("IFC 2x3{0}{1}{2}", 
                CurrentViewOnly ? "-Current View" : "",
                SplitWall ? "-Split Wall" : "",
                ExportBaseQuantities ? "-Base Quantities" : "");
            IFCVersion = IFCVersion.IFC2x3;
        }

        public bool CurrentViewOnly { get; set; }
        public bool SplitWall { get; set; }
        public bool ExportBaseQuantities { get; set; }
        public string Name { get; set; }
        public IFCVersion IFCVersion { get; set; }
        public int ActiveViewId { get; set; }
    }

    public enum IFCLibrary
    {
        Override, Standard, Deprecated
    }
}
