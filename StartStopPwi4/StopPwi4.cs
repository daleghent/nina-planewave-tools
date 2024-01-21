#region "copyright"

/*
    Copyright Dale Ghent <daleg@elemental.org>

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/
*/

#endregion "copyright"

using Newtonsoft.Json;
using NINA.Core.Model;
using NINA.Core.Utility;
using NINA.Sequencer.SequenceItem;
using NINA.Sequencer.Validations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DaleGhent.NINA.PlaneWaveTools.StartStopPwi4 {

    [ExportMetadata("Name", "Stop PWI4")]
    [ExportMetadata("Description", "Stops PWI4")]
    [ExportMetadata("Icon", "PWI4_SVG")]
    [ExportMetadata("Category", "PlaneWave Tools")]
    [Export(typeof(ISequenceItem))]
    [JsonObject(MemberSerialization.OptIn)]
    public class StopPwi4 : SequenceItem, IValidatable {

        [ImportingConstructor]
        public StopPwi4() {
        }

        private StopPwi4(StopPwi4 copyMe) : this() {
            CopyMetaData(copyMe);
        }

        public override Task Execute(IProgress<ApplicationStatus> progress, CancellationToken token) {
            if (Process.GetProcessesByName("PWI4").Length > 0) {
                KillPwi4();
            } else {
                Logger.Error("PWI4 is not running!");
            }

            return Task.CompletedTask;
        }

        public override object Clone() {
            return new StopPwi4(this);
        }

        public override string ToString() {
            return $"Category: {Category}, Item: {Name}";
        }

        public IList<string> Issues { get; set; } = new ObservableCollection<string>();

        public bool Validate() {
            var i = new List<string>();

            if (Process.GetProcessesByName("PWI4").Length < 1) {
                i.Add("PWI4 is not running");
            }

            if (i != Issues) {
                Issues = i;
                RaisePropertyChanged(nameof(Issues));
            }

            return i.Count == 0;
        }

        private static void KillPwi4() {
            var apcc = Process.GetProcessesByName("PWI4");

            try {
                if (apcc.Length > 0) {
                    foreach (var proc in Process.GetProcessesByName("PWI4")) {
                        Logger.Info($"Killing PWI4 PID {proc.Id}");
                        proc.Kill();
                    }
                }
            } catch {
                throw new SequenceEntityFailedException("Could not stop PWI4");
            }
        }
    }
}