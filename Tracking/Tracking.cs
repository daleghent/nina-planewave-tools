#region "copyright"

/*
    Copyright (c) 2024 Dale Ghent <daleg@elemental.org>

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/
*/

#endregion "copyright"

using DaleGhent.NINA.PlaneWaveTools.Utility;
using Newtonsoft.Json;
using NINA.Core.Model;
using NINA.Sequencer.SequenceItem;
using NINA.Sequencer.Validations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Primitives;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DaleGhent.NINA.PlaneWaveTools.Tracking {

    [ExportMetadata("Name", "Set Tracking")]
    [ExportMetadata("Description", "Start or stop tracking")]
    [ExportMetadata("Icon", "SpeedometerSVG")]
    [ExportMetadata("Category", "PlaneWave Tools")]
    [Export(typeof(ISequenceItem))]
    [JsonObject(MemberSerialization.OptIn)]
    public class Tracking : SequenceItem, IValidatable, INotifyPropertyChanged {

        [ImportingConstructor]
        public Tracking() {
            Pwi4IpAddress = Properties.Settings.Default.Pwi4IpAddress;
            Pwi4Port = Properties.Settings.Default.Pwi4Port;

            Properties.Settings.Default.PropertyChanged += SettingsChanged;
        }

        private bool _track = true;

        [JsonProperty]
        public bool Track {
            get => _track;
            set {
                _track = value;
                RaisePropertyChanged();
            }
        }

        public override async Task Execute(IProgress<ApplicationStatus> progress, CancellationToken ct) {
            try {
                if (Track) {
                    await StartTracking(progress, ct);
                } else {
                    await StopTracking(progress, ct);
                }
            } catch {
                throw;
            } finally {
                progress?.Report(new ApplicationStatus() { Status = string.Empty });
            }

            return;
        }

        private Tracking(Tracking copyMe) : this() {
            CopyMetaData(copyMe);
        }

        public override object Clone() {
            return new Tracking(this) {
                Track = Track,
            };
        }

        public override string ToString() {
            return $"Category: {Category}, Item: {nameof(Tracking)}";
        }

        public IList<string> Issues { get; set; } = new ObservableCollection<string>();

        public bool Validate() {
            var i = new List<string>();

            var connected = Pwi4StatusChecker.IsConnected;
            if (!connected) {
                i.Add(Pwi4StatusChecker.NotConnectedReason);
            }

            if (i != Issues) {
                Issues = i;
                RaisePropertyChanged(nameof(Issues));
            }

            return i.Count == 0;
        }

        private string Pwi4IpAddress { get; set; }
        private ushort Pwi4Port { get; set; }

        private async Task<bool> StopTracking(IProgress<ApplicationStatus> progress, CancellationToken ct) {
            // Send API call to stop tracking
            string url = "/mount/tracking_off";

            progress?.Report(new ApplicationStatus() { Status = $"Stop tracking" });
            var response = await Utilities.HttpRequestAsync(Pwi4IpAddress, Pwi4Port, url, HttpMethod.Get, string.Empty, ct);

            return response.IsSuccessStatusCode;
        }

        private async Task<bool> StartTracking(IProgress<ApplicationStatus> progress, CancellationToken ct) {
            // Send API call to stop tracking
            string url = "/mount/tracking_on";

            progress?.Report(new ApplicationStatus() { Status = $"Start tracking" });
            var response = await Utilities.HttpRequestAsync(Pwi4IpAddress, Pwi4Port, url, HttpMethod.Get, string.Empty, ct);

            return response.IsSuccessStatusCode;
        }

        private void SettingsChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(Pwi4IpAddress):
                    Pwi4IpAddress = Properties.Settings.Default.Pwi4IpAddress;
                    break;

                case nameof(Pwi4Port):
                    Pwi4Port = Properties.Settings.Default.Pwi4Port;
                    break;
            }
        }
    }
}