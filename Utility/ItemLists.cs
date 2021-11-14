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
    }
}