#region "copyright"

/*
    Copyright (c) 2024 Dale Ghent <daleg@elemental.org>

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/
*/

#endregion "copyright"

using DaleGhent.NINA.PlaneWaveTools.Utility;
using System.ComponentModel;

namespace DaleGhent.NINA.PlaneWaveTools.Enum {
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum HeaterType {
        [Description("M1")]
        M1Heater = 1,

        [Description("M2")]
        M2Heater,

        [Description("M3")]
        M3Heater,

        [Description("Cable Tray")]
        CableTrayHeater,

        [Description("Azimuth Read Head")]
        AzmReadHeadHeater,

        [Description("Altitude Read Head")]
        AltReadHeadHeater,

        [Description("Rotator 1 Read Head")]
        Rot1ReadHeadHeater,

        [Description("Rotator 2 Read Head")]
        Rot2ReadHeadHeater,

        [Description("Shutter Actuator")]
        ShutterActuatorHeater,
    }
}