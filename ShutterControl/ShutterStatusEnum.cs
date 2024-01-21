#region "copyright"

/*
    Copyright (c) 2024 Dale Ghent <daleg@elemental.org>

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/
*/

#endregion "copyright"

namespace DaleGhent.NINA.PlaneWaveTools.ShutterControl {
    internal enum ShutterStatusEnum {
        Open = 0,
        Closed,
        Opening,
        Closing,
        Errored,
        Stopped,
        StatusError = 255
    }
}