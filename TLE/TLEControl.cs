#region "copyright"

/*
    Copyright Dale Ghent <daleg@elemental.org>

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/
*/

#endregion "copyright"

using DaleGhent.NINA.PlaneWaveTools.Utility;
using Newtonsoft.Json;
using NINA.Core.Model;
using NINA.Core.Utility;
using NINA.Sequencer.SequenceItem;
using NINA.Sequencer.Validations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Primitives;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DaleGhent.NINA.PlaneWaveTools.TLE {

    [ExportMetadata("Name", "TLE Follow")]
    [ExportMetadata("Description", "Start following a satellite")]
    [ExportMetadata("Icon", "PlatesolveAndRotateSVG")]
    [ExportMetadata("Category", "PlaneWave Tools")]
    [Export(typeof(ISequenceItem))]
    [JsonObject(MemberSerialization.OptIn)]
    public class TLEControl : SequenceItem, IValidatable, INotifyPropertyChanged {

        [ImportingConstructor]
        public TLEControl() {
            Pwi4IpAddress = Properties.Settings.Default.Pwi4IpAddress;
            Pwi4Port = Properties.Settings.Default.Pwi4Port;

            Properties.Settings.Default.PropertyChanged += SettingsChanged;
        }

        private string _line1 = string.Empty;

        [JsonProperty]
        public string Line1 {
            get => _line1;
            set {
                _line1 = value;
                RaisePropertyChanged();
            }
        }

        private string _line2 = string.Empty;

        [JsonProperty]
        public string Line2 {
            get => _line2;
            set {
                _line2 = value;
                RaisePropertyChanged();
            }
        }

        private double _axis0Distance = 0d;

        [JsonProperty]
        public double Axis0Distance {
            get => Math.Round(_axis0Distance, 2);
            set {
                _axis0Distance = value;
                RaisePropertyChanged();
            }
        }

        private double _axis1Distance = 0d;

        [JsonProperty]
        public double Axis1Distance {
            get => Math.Round(_axis1Distance, 2);
            set {
                _axis1Distance = value;
                RaisePropertyChanged();
            }
        }

        public override async Task Execute(IProgress<ApplicationStatus> progress, CancellationToken ct) {
            try {
                // Send API call to start tracking following the TLE
                string url = $"/mount/follow_tle?line1=target&line2={Uri.EscapeDataString(Line1)}&line3={Uri.EscapeDataString(Line2)}";

                progress?.Report(new ApplicationStatus() { Status = $"Moving scope to track satellite" });
                var response = await Utilities.HttpRequestAsync(Pwi4IpAddress, Pwi4Port, url, HttpMethod.Get, string.Empty, ct);

                if (!response.IsSuccessStatusCode) {
                    throw new SequenceEntityFailedException($"PWI4 returned status {response.StatusCode} for {url}");
                }

                DateTime _tracking;
                DateTime _timeout;

                _tracking = _timeout = DateTime.Now;

                do {
                    Dictionary<string, string> status = new();

                    Task.Run(async () => {
                        status = await Utilities.Pwi4GetStatus(Pwi4IpAddress, Pwi4Port, ct);
                    }, ct).Wait(ct);

                    // Check altitude and stop below 5 degrees
                    _ = double.TryParse(status["mount.altitude_degs"], CultureInfo.InvariantCulture, out double altitude);

                    if (altitude < 5) {
                        throw new SequenceEntityFailedException("Target below horizon");
                    }

                    _ = double.TryParse(status["mount.axis0.dist_to_target_arcsec"], CultureInfo.InvariantCulture, out double axis0Distance);
                    _ = double.TryParse(status["mount.axis1.dist_to_target_arcsec"], CultureInfo.InvariantCulture, out double axis1Distance);
                    Axis0Distance = axis0Distance;
                    Axis1Distance = axis1Distance;

                    progress?.Report(new ApplicationStatus() { Status = $"Distance to target: {axis0Distance}-{axis1Distance}" });
                    await CoreUtil.Delay(500, ct);

                    if (Math.Abs(axis0Distance) > 5 || Math.Abs(axis1Distance) > 5)
                        _tracking = DateTime.Now;
                    if (_timeout.AddMinutes(1) < DateTime.Now)
                        throw new SequenceEntityFailedException("Timed out after 1 minute trying to follow the target.");
                } while (_tracking.AddSeconds(3) > DateTime.Now);
            } catch {
                // Stop tracking for safety
                await StopTracking(progress, ct);
                throw;
            } finally {
                progress?.Report(new ApplicationStatus() { Status = string.Empty });
            }

            return;
        }

        private TLEControl(TLEControl copyMe) : this() {
            CopyMetaData(copyMe);
        }

        public override object Clone() {
            return new TLEControl(this) {
                Line1 = Line1,
                Line2 = Line2,
            };
        }

        public override string ToString() {
            return $"Category: {Category}, Item: {Name}";
        }

        public IList<string> Issues { get; set; } = new ObservableCollection<string>();

        public bool Validate() {
            var i = new List<string>();
            var status = new Dictionary<string, string>();

            if (string.IsNullOrEmpty(Line1) || string.IsNullOrEmpty(Line2)) {
                i.Add($"TLE information is incomplete");
            }

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

            if (!Utilities.Pwi4BoolStringToBoolean(status["mount.is_connected"])) {
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

        private async Task<bool> StopTracking(IProgress<ApplicationStatus> progress, CancellationToken ct) {
            // Send API call to stop tracking
            string url = "/mount/tracking_off";

            progress?.Report(new ApplicationStatus() { Status = $"Stop tracking satellite" });
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