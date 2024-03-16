#region "copyright"

/*
    Copyright (c) 2024 Dale Ghent <daleg@elemental.org>

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/
*/

#endregion "copyright"

using NINA.Core.Utility;
using System.ComponentModel;

namespace DaleGhent.NINA.PlaneWaveTools.Enum {

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    internal enum ShutterStatusEnum {

        [Description("LblOpen")]
        Open = 0,

        [Description("LblClose")]
        Closed,

        [Description("LblOpening")]
        Opening,

        [Description("LblClosing")]
        Closing,

        [Description("LblError")]
        Errored,

        [Description("LblTrackingStopped")]
        Stopped,

        [Description("LblUnknown")]
        StatusError = 255
    }

    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum ShutterAction {

        [Description("LblOpen")]
        Open = 0,

        [Description("LblClose")]
        Close,
    }
}