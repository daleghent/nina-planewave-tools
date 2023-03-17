#region "copyright"

/*
    Copyright Dale Ghent <daleg@elemental.org>

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/
*/

#endregion "copyright"

using System.Collections.Generic;

namespace DaleGhent.NINA.PlaneWaveTools.Utility {

    public class ItemLists {

        public static readonly IList<string> DeltaTHeaterModes = new List<string> {
            "Off",
            "On",
            "Control",
            "On when less than"
        };

        public static readonly IList<string> DeltaTHeaters = new List<string> {
            "Primary backplate",
            "Secondary mirror",
        };

        public static readonly IList<string> AxisNames = new List<string> {
            "Azimuth",
            "Altitude",
        };

        public static readonly IList<string> AxisStates = new List<string> {
            "Disable",
            "Enable",
        };

        public static readonly IList<string> Mirrors = new List<string> {
            "Primary mirror",
            "Primary mirror backplate",
            "Secondary mirror",
        };

        public static readonly IList<string> AmbientTemperatureSources = new List<string> {
            "Delta-T ambient sensor",
            "EFA ambient sensor",
            "ASCOM Focuser sensor",
            "N.I.N.A. weather source",
        };

        public static readonly IList<string> M3Ports = new List<string> {
            "1",
            "2",
        };
    }
}