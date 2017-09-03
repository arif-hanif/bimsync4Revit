<p align="center"><img width=12.5% src="https://github.com/simonmoreau/bimsync4Revit/blob/master/files/bimsyncLogo.png"></p>
<h1 align="center">
  <br>
  bimsync®
  <br>
</h1>

<h4 align="center">A bimsync® unoffical plugin for Revit.</h4>

![screenshot](https://github.com/simonmoreau/bimsync4Revit/blob/master/files/bimsync_upload.gif)

## Key Features

This plugin allows you to upload effortlessly your Revit models to bimsync®.

* Login into your bimsync® account
* Select your bimsync® project and model and upload your Revit model as an IFC file in a single click
* Save the references to your bimsync® project and model for futur uploads

A free bimsync® trial is available for new users on the [bimsync® website](https://bimsync.com/).

This plugin is developed by a third party, and is **_not_** endorsed by Catenda®.

## Getting Started

Edit bimsync.csproj, and make sure that the following lines a correctly pointing to your Revit installation folder:
* Line 29:     <StartProgram>$(ProgramW6432)\Autodesk\Revit 2018\Revit.exe</StartProgram>
* Line 39:     <StartProgram>$(ProgramW6432)\Autodesk\Revit 2018\Revit.exe</StartProgram>
* Line 48:     <HintPath>$(ProgramW6432)\Autodesk\Revit 2018\RevitAPI.dll</HintPath>
* Line 52:     <HintPath>$(ProgramW6432)\Autodesk\Revit 2018\RevitAPIUI.dll</HintPath>
* Line 129 to 134: <PostBuildEvent>...</PostBuildEvent>

You will also need to install the last version of the Revit 2018 IFC plugin, available [here](https://apps.autodesk.com/RVT/en/Detail/Index?id=6193770166503453647&appLang=en&os=Win64&autostart=true)

Open the solution in Visual Studio 2017, buid it to retrieve the packages from Nuget, and hit "Start" to run Revit in debug mode.

## Installation

There is two ways to install this plugin:

### The easy way

Download the installer on the [Autodesk App Exchange](https://apps.autodesk.com/RVT/en/Home/Index)

### The (not so) easy way

You install bimsync® just [like any other Revit add-in](http://help.autodesk.com/view/RVT/2018/ENU/?guid=GUID-4FFDB03E-6936-417C-9772-8FC258A261F7), by copying the add-in manifest ("bimsync.addin"), the assembly DLL ("bimsync.dll"), its dependancy (here, "RestSharp.dll") and the associated help files ("bimsyncHelp.html" and the "bimsyncHelp_Files" folder) to the Revit Add-Ins folder (%APPDATA%\Autodesk\Revit\Addins\2018).

If you specify the full DLL pathname in the add-in manifest, it can also be located elsewhere. However, this DLL, its dependanties and help files must be locted in the same folder.

Futhermore, the Visual Studio solution contain all the necessary post-build scripts to copy these files into appropriates folders.

## Built With

* .NET and [Visual Studio Community](https://www.visualstudio.com/vs/community/)
* [RestSharp](http://restsharp.org/) - The .NET REST client
* The Visual Studio Revit C# and VB add-in templates from [The Building Coder](http://thebuildingcoder.typepad.com/blog/2017/04/revit-2018-visual-studio-c-and-vb-net-add-in-wizards.html)

## Development
Want to contribute? Great, I would be happy to integrate your improvements!

To fix a bug or enhance an existing module, follow these steps:

- Fork the repo
- Create a new branch (`git checkout -b improve-feature`)
- Make the appropriate changes in the files
- Add changes to reflect the changes made
- Commit your changes (`git commit -am 'Improve feature'`)
- Push to the branch (`git push origin improve-feature`)
- Create a Pull Request 

## Bug / Feature Request

If you find a bug (connection issue, error while uploading, ...), kindly open an issue [here](https://github.com/simonmoreau/bimsync4Revit/issues/new) by including a screenshot of your problem and the expected result.

If you'd like to request a new function, feel free to do so by opening an issue [here](https://github.com/simonmoreau/bimsync4Revit/issues/new). Please include workflows samples and their corresponding results.

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details

## Contact information
This software is an open-source project mostly maintained by myself, Simon Moreau. If you have any questions or request, feel free to contact me at [simon@bim42.com](mailto:simon@bim42.com) or on Twitter [@bim42](https://twitter.com/bim42?lang=en).
