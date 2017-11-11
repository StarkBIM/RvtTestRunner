# RvtTestRunner

## About
* Test runner for XUnit tests that runs inside of Revit.  
* This initial release has only a very basic interface and no ability to configure any options.  
* There is also no ability to view or export the results in any manner other than the dialog box displayed after execution.  

## Installation
* Clone and build the repository. There are different build configurations for versions 2015-2018 of Revit.  
* The .addin file must be copied to the appropriate directory for each version of Revit, and modified to point to the correct path for the DLL

## Usage
* Click the "Test Runner" button from the Add-Ins toolbar in Revit
* Click the Add button to open a file browser dialog which will allow assemblies to be selected. Multiple assemblies can be selected at once
* Repeat, if necessary, adding other assemblies
* To remove an assembly from the list, select it and click remove
* Click Execute to run all the tests contained in those assemblies
* Results are displayed in a dialog box. Click the arrow to expand the dialog box and see the full execution log

## Debugging Unit Tests
* To debug running unit tests, attach an instance of Visual Studio that has the test project open to the Revit.exe process.
* Ensure that the selected build configuration matches the currently open version of Revit.

* The ReAttach extension is recommended to save a few clicks on this process:  
[https://marketplace.visualstudio.com/items?itemName=ErlandR.ReAttach]


## To Do
* Add ability to configure all of XUnit's execution options
* Display all tests and their results in a list or grid format
* Add the ability to save sets of assemblies and options that can be loaded and run
* Add the ability to select which tests should be run
* Set up CI
* Create installer
* Add icon
* Investigate whether RvtTestRunner can be integrated into other CI pipelines
