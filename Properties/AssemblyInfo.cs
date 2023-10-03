using System.Reflection;
using System.Runtime.InteropServices;

// [MANDATORY] The following GUID is used as a unique identifier of the plugin
[assembly: Guid("1A8145BD-4D7A-44F0-B12B-A6CE34BB68B0")]

// [MANDATORY] The assembly versioning
//Should be incremented for each new release build of a plugin
[assembly: AssemblyVersion("2.3.0.0")]
[assembly: AssemblyFileVersion("2.3.0.0")]

// [MANDATORY] The name of your plugin
[assembly: AssemblyTitle("PlaneWave Tools")]
// [MANDATORY] A short description of your plugin
[assembly: AssemblyDescription("A collection of useful tools for managing PlaneWave telescope systems")]

// The following attributes are not required for the plugin per se, but are required by the official manifest meta data

// Your name
[assembly: AssemblyCompany("Dale Ghent")]
// The product name that this plugin is part of
[assembly: AssemblyProduct("PlaneWave Tools")]
[assembly: AssemblyCopyright("Copyright © 2023 Dale Ghent")]

// The minimum Version of N.I.N.A. that this plugin is compatible with
[assembly: AssemblyMetadata("MinimumApplicationVersion", "3.0.0.1001")]

// The license your plugin code is using
[assembly: AssemblyMetadata("License", "MPL-2.0")]
// The url to the license
[assembly: AssemblyMetadata("LicenseURL", "https://www.mozilla.org/en-US/MPL/2.0/")]
// The repository where your pluggin is hosted
[assembly: AssemblyMetadata("Repository", "https://github.com/daleghent/nina-planewave-tools")]

// The following attributes are optional for the official manifest meta data

//[Optional] Your plugin homepage - omit if not applicaple
[assembly: AssemblyMetadata("Homepage", "https://daleghent.com/planewave-tools")]

//[Optional] Common tags that quickly describe your plugin
[assembly: AssemblyMetadata("Tags", "planewave,pw3,delta-t,sequencer,fans")]

//[Optional] A link that will show a log of all changes in between your plugin's versions
[assembly: AssemblyMetadata("ChangelogURL", "https://github.com/daleghent/nina-planewave-tools/blob/main/CHANGELOG.md")]

//[Optional] The url to a featured logo that will be displayed in the plugin list next to the name
[assembly: AssemblyMetadata("FeaturedImageURL", "https://daleghent.github.io/nina-plugins/assets/images/planewavetools.png")]
//[Optional] A url to an example screenshot of your plugin in action
[assembly: AssemblyMetadata("ScreenshotURL", "")]
//[Optional] An additional url to an example example screenshot of your plugin in action
[assembly: AssemblyMetadata("AltScreenshotURL", "")]
//[Optional] An in-depth description of your plugin
[assembly: AssemblyMetadata("LongDescription", @"PlaneWave Tools provides several instructions for controlling various aspects of PlaneWave OTA and mount systems.

# Requirements #

* PWI3 >= 3.5.3
* PWI4 >= 4.0.99 beta 27
* PlaneWave Shutter Control >= 1.12.0

# Sequence Instructions #

* Start PWI3, Stop PWI3 - Starts and stops the PWI3.exe application
* Start PWI4, Stop PWI4 - Starts and stops the PWI4.exe application
* Axis Control - Enable or disable mount axes
* Fan Control - Turns the OTA fans on or off
* DeltaT Control - Sets the per-heater operating modes of the Delta T heater controller
* M3 Control - Sets the postions of the M3 Nasmyth port mirror
* Shutter Control - Controls CDK700 and PW1000 shutters via the PlaneWave Shutter Control app
* TLE Follow, Set Tracking - Programs the mount to follow a two-line element ephemeris definition and start tracking along it

Open to ideas for additional controls!

# Getting help #

Help for this plugin may be found in the **#plugin-discussions** channel on the NINA project [Discord chat server](https://discord.gg/nighttime-imaging) or by filing an issue report at this plugin's [Github repository](https://github.com/daleghent/nina-planewave-tools/issues).")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]
// [Unused]
[assembly: AssemblyConfiguration("")]
// [Unused]
[assembly: AssemblyTrademark("")]
// [Unused]
[assembly: AssemblyCulture("")]