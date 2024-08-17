# PlaneWave Tools

## 2.9.0.0 - 2024-08-17
* Added **Load Model** instruction. This loads a specified PointXP model file into PWI4 to use as the active mount model.

## 2.8.1.0 - 2024-08-09
* Fixed a bug concerning PWI4 version parsing

## 2.8.0.0 - 2024-03-16
* Added the **Shutter Control (PWI4)** instruction to control CDK and PW1000 mirror shutters via PWI4. PWI4 version 4.1.3 added support for controlling shutters. This functionality replaces the separate *PlaneWave Shutter Control* app.
* Internal functions that query PWI4's status API have been made more efficient through the use of caching. Contributed by Stefan Berg.

## 2.7.0.0 - 2024-01-21
* Changes to backend utility class to fix retrying PWI4 API queries upon failure, plus some additional touch-ups. Contributed by Stefan Berg.

## 2.6.0.0 - 2024-01-21
**NOTICE:** As of this release, the PWI3-specific instructions (**Start/Stop PWI3**, **Fan Control (PWI3)**, and **Delta-T Control (PWI3)**) are deprecated and will be removed in a future release. Please switch to using the PWI4-based replacements as soon as possible. PWI 4.1.0 and later contains the mount/OTA fan and heater controls that prior versions of PWI 4.0 lacked.

The two following instructions were introduced to permit fan and heater control via PWI 4.1.0 and later:
* Added **Fan Control (PWI4)**
* Added **Heater Control (PWI4)**

The two following instructions were renamed to denote that they are specific to PWI3:
* Renamed **Fan Control** to **Fan Control (PWI3)**
* Renamed **Delta-T Control** to **Delta-T Control (PWI3)**

Other changes:
* **M3 Control**: M3 status check was made more efficient to avoid an extra call to the PWI4 API
* Plugin framework retargeted to .NET 8
* Migrated file dialogs to CommunityToolkit.Mvvm
* Minimum supported NINA version is now 3.0 Beta 1

## 2.4.0.0 - 2.5.0.0
* Unreleased builds

## 2.3.0.0 - 2023-10-03
* Fixed parsing of boolean key:value pairs in the PWI4 API status response
* Enhanced validation of M3 device presence in the **M3 Control** instruction

## 2.2.0.0 - 2023-09-30
* Added **TLE Follow** and PlaneWave-specific **Set Tracking** instructions to enable the input and tracking of objects via Two-Line Element ephemeris definitions. Thanks to Nick Hardy for providing these.
* Added **Shutter Control** instructions to control CDK700 and PW1000 shutters via PlaneWave Shutter Control
* Various small fixes and code cleanups to validation logic

## 2.1.0.0 - 2023-03-16
* Added **M3 Control** instruction to set the position of the M3 Nasmyth port mirror

## 2.0.0.0 - 2022-11-12
* Ported plugin to .NET 7 for NINA 3.0 compatibility

## 1.2.0.0 - 2022-02-23
* Added a check and paused after issuing commands to PWI3 and PWI4 to allow them to process the request before moving on to the next instruction

## 1.1.0.0 - 2022-01-20
* Added Start/Stop PWI4 instructions
* Added **Set Axis State** instruction for enabling and disabling mount axes via PWI4
* Changed formatting of DeltaT combobox contents
* Updated icons
* Minimum supported NINA version is now 2.0 Beta 31

## 1.0.5.0 - 2021-11-14
* Minimum supported NINA version is now 2.0 Beta 1

## 1.0.0.0 - 2021-10-27
* Added new **Start PWI3** and **Stop PWI3** instructions
* Added new **Fans Control** instruction for turning OTA fans on and off
* Added new **DeltaT Control** instruction for setting the operating mode of the Delta T controller
