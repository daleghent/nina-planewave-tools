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
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DaleGhent.NINA.PlaneWaveTools.Fans {

    [ExportMetadata("Name", "Fan Control (PWI4)")]
    [ExportMetadata("Description", "Turns cooling fans on or off via PWI4")]
    [ExportMetadata("Icon", "FanControlPwi4_SVG")]
    [ExportMetadata("Category", "PlaneWave Tools")]
    [Export(typeof(ISequenceItem))]
    [JsonObject(MemberSerialization.OptIn)]
    public class FanControlPwi4 : SequenceItem, IValidatable, INotifyPropertyChanged {
        private bool fanState = false;

        [ImportingConstructor]
        public FanControlPwi4() {
            Pwi4IpAddress = Properties.Settings.Default.Pwi4IpAddress;
            Pwi4Port = Properties.Settings.Default.Pwi4Port;

            Properties.Settings.Default.PropertyChanged += SettingsChanged;
        }

        [JsonProperty]
        public bool FanState {
            get => fanState;
            set {
                fanState = value;
                RaisePropertyChanged();
            }
        }

        private FanControlPwi4(FanControlPwi4 copyMe) : this() {
            CopyMetaData(copyMe);
        }

        public override async Task Execute(IProgress<ApplicationStatus> progress, CancellationToken ct) {
            var fanUrl = $"/fans/{(fanState ? "on" : "off")}";

            var response = await Utilities.HttpRequestAsync(Pwi4IpAddress, Pwi4Port, fanUrl, HttpMethod.Get, string.Empty, ct);

            if (response.StatusCode != System.Net.HttpStatusCode.OK) {
                var error = await response.Content.ReadAsStringAsync(ct);
                throw new SequenceEntityFailedException($"Could not set fan state to {(fanState ? "On" : "Off")}: {error.Trim()}");
            }

            return;
        }

        public override object Clone() {
            return new FanControlPwi4(this) {
                FanState = FanState,
            };
        }

        public override string ToString() {
            return $"Category: {Category}, Item: {Name}, FanState: {(fanState ? "On" : "Off")}";
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