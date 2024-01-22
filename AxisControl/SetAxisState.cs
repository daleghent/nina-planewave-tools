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

namespace DaleGhent.NINA.PlaneWaveTools.AxisControl {

    [ExportMetadata("Name", "Set Axis State")]
    [ExportMetadata("Description", "Sets PlaneWave mount axis state via PWI4")]
    [ExportMetadata("Icon", "AltAz_SVG")]
    [ExportMetadata("Category", "PlaneWave Tools")]
    [Export(typeof(ISequenceItem))]
    [JsonObject(MemberSerialization.OptIn)]
    public class SetAxisState : SequenceItem, IValidatable, INotifyPropertyChanged {
        private int axis = 0;
        private int axisState = 1;
        private bool connectMount = true;

        [ImportingConstructor]
        public SetAxisState() {
            Pwi4IpAddress = Properties.Settings.Default.Pwi4IpAddress;
            Pwi4Port = Properties.Settings.Default.Pwi4Port;

            Properties.Settings.Default.PropertyChanged += SettingsChanged;
        }

        [JsonProperty]
        public int Axis {
            get => axis;
            set {
                axis = value;
                RaisePropertyChanged();
            }
        }

        [JsonProperty]
        public int AxisState {
            get => axisState;
            set {
                axisState = value;
                RaisePropertyChanged();
            }
        }

        [JsonProperty]
        public bool ConnectMount {
            get => connectMount;
            set {
                connectMount = value;
                RaisePropertyChanged();
            }
        }

        public static IList<string> AxisNames => ItemLists.AxisNames;
        public static IList<string> AxisStates => ItemLists.AxisStates;

        private SetAxisState(SetAxisState copyMe) : this() {
            CopyMetaData(copyMe);
        }

        public override async Task Execute(IProgress<ApplicationStatus> progress, CancellationToken ct) {
            string url;

            if (!await Utilities.Pwi4CheckMountConnected(Pwi4IpAddress, Pwi4Port, ct) && ConnectMount) {
                try {
                    url = "/mount/connect";
                    var response = await Utilities.HttpRequestAsync(Pwi4IpAddress, Pwi4Port, url, HttpMethod.Get, string.Empty, ct);

                    if (!response.IsSuccessStatusCode) {
                        throw new SequenceEntityFailedException($"PWI4 returned status {response.StatusCode} for {url}");
                    }
                } catch {
                    throw;
                }
            }

            url = "/mount";

            switch (AxisState) {
                case 0:
                    url += "/disable";
                    break;

                case 1:
                    url += "/enable";
                    break;
            }

            url += $"?axis={axis}";

            try {
                var response = await Utilities.HttpRequestAsync(Pwi4IpAddress, Pwi4Port, url, HttpMethod.Get, string.Empty, ct);

                if (!response.IsSuccessStatusCode) {
                    throw new SequenceEntityFailedException($"PWI4 returned status {response.StatusCode} for {url}");
                }

                await Task.Delay(TimeSpan.FromSeconds(1), ct);
            } catch {
                throw;
            }

            return;
        }

        public override object Clone() {
            return new SetAxisState(this) {
                Axis = Axis,
                AxisState = AxisState,
                ConnectMount = ConnectMount,
            };
        }

        public override string ToString() {
            return $"Category: {Category}, Item: {Name}, Axis: {AxisNames[Axis]}, AxisState: {AxisStates[AxisState]}, ConnectMount: {ConnectMount}";
        }

        public IList<string> Issues { get; set; } = new ObservableCollection<string>();

        public bool Validate() {
            var i = new List<string>();
            var status = new Dictionary<string, string>();

            Task.Run(async () => {
                try {
                    status = await Utilities.Pwi4GetStatus(Pwi4IpAddress, Pwi4Port, CancellationToken.None);
                } catch (HttpRequestException) {
                    i.Add("Could not communicate with PWI4");
                } catch (Exception ex) {
                    i.Add($"{ex.Message}");
                }
            }).Wait();

            if (i.Count > 0) {
                goto end;
            }

            if (!status.ContainsKey("mount.is_connected")) {
                i.Add("Unable to determine mount connection status");
                goto end;
            }

            if (!Utilities.Pwi4BoolStringToBoolean(status["mount.is_connected"]) && !ConnectMount) {
                i.Add("PWI4 is not connected to the mount");
                goto end;
            }

        end:
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