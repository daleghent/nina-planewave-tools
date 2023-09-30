# PlaneWave Tools

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
