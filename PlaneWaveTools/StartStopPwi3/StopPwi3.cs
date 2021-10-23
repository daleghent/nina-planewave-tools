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

namespace DaleGhent.NINA.PlaneWaveTools.StartStopPwi3 {

    [ExportMetadata("Name", "Stop PWI3")]
    [ExportMetadata("Description", "Stops PWI3")]
    [ExportMetadata("Icon", "PWI3_SVG")]
    [ExportMetadata("Category", "PlaneWave Tools")]
    [Export(typeof(ISequenceItem))]
    [JsonObject(MemberSerialization.OptIn)]
    public class StopPwi3 : SequenceItem, IValidatable {

        [ImportingConstructor]
        public StopPwi3() {
        }

        private StopPwi3(StopPwi3 copyMe) : this() {
            CopyMetaData(copyMe);
        }

        public override Task Execute(IProgress<ApplicationStatus> progress, CancellationToken token) {
            if (Process.GetProcessesByName("PWI3").Length > 0) {
                KillPwi3();
            } else {
                Logger.Error("PWI3 is not running!");
            }

            return Task.CompletedTask;
        }

        public override object Clone() {
            return new StopPwi3(this);
        }

        public override string ToString() {
            return $"Category: {Category}, Item: {nameof(StopPwi3)}";
        }

        public IList<string> Issues { get; set; } = new ObservableCollection<string>();

        public bool Validate() {
            return true;
        }

        private void KillPwi3() {
            var apcc = Process.GetProcessesByName("PWI3");

            try {
                if (apcc.Length > 0) {
                    foreach (var proc in Process.GetProcessesByName("PWI3")) {
                        Logger.Info($"Killing PWI3 PID {proc.Id}");
                        proc.Kill();
                    }
                }
            } catch {
                throw new SequenceEntityFailedException("Could not stop PWI3");
            }
        }
    }
}