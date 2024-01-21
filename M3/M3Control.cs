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

namespace DaleGhent.NINA.PlaneWaveTools.M3 {

    [ExportMetadata("Name", "M3 Control")]
    [ExportMetadata("Description", "Switches Nasmyth port via PWI4")]
    [ExportMetadata("Icon", "BahtinovSVG")]
    [ExportMetadata("Category", "PlaneWave Tools")]
    [Export(typeof(ISequenceItem))]
    [JsonObject(MemberSerialization.OptIn)]
    public class M3Control : SequenceItem, IValidatable, INotifyPropertyChanged {
        private short m3Port = 1;

        [ImportingConstructor]
        public M3Control() {
            Pwi4IpAddress = Properties.Settings.Default.Pwi4IpAddress;
            Pwi4Port = Properties.Settings.Default.Pwi4Port;

            Properties.Settings.Default.PropertyChanged += SettingsChanged;
        }

        [JsonProperty]
        public short M3Port {
            get => m3Port;
            set {
                m3Port = value;
                RaisePropertyChanged();
            }
        }

        public override async Task Execute(IProgress<ApplicationStatus> progress, CancellationToken ct) {
            try {
                short portStatus = GetM3PortStatus(ct);

                if (portStatus == 0) {
                    throw new SequenceEntityFailedException($"M3 was asked to move to port {M3Port} but it either doesn't exist or is already in motion!");
                }

                if (portStatus == M3Port) {
                    Logger.Info($"M3 port is already at position {M3Port}");
                    return;
                }

                // Send API call to change the active M3 Nasmyth port
                string url = $"/m3/goto?port={M3Port}";

                progress?.Report(new ApplicationStatus() { Status = $"Moving M3 to port {M3Port}" });
                var response = await Utilities.HttpRequestAsync(Pwi4IpAddress, Pwi4Port, url, HttpMethod.Get, string.Empty, ct);

                if (!response.IsSuccessStatusCode) {
                    throw new SequenceEntityFailedException($"PWI4 returned status {response.StatusCode} for {url}");
                }

                int waitSeconds = 2;
                int waitAttempts = 30;
                int i = 0;

                do {
                    if (i >= waitAttempts) {
                        throw new SequenceEntityFailedException($"{this.Name} waited too long to set M3 port to {M3Port}. Verify that the M3 unit is operating correctly");
                    }

                    await Task.Delay(TimeSpan.FromSeconds(waitSeconds), ct);
                    portStatus = GetM3PortStatus(ct);

                    i++;
                } while (portStatus != M3Port);
            } catch {
                throw;
            } finally {
                progress?.Report(new ApplicationStatus() { Status = string.Empty });
            }

            return;
        }

        public static IList<short> M3Ports => ItemLists.M3Ports;

        private M3Control(M3Control copyMe) : this() {
            CopyMetaData(copyMe);
        }

        public override object Clone() {
            return new M3Control(this) {
                M3Port = M3Port,
            };
        }

        public override string ToString() {
            return $"Category: {Category}, Item: {Name}, M3Port: {M3Port}";
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

            if (!Utilities.Pwi4BoolStringToBoolean(status["mount.is_connected"])) {
                i.Add("PWI4 is not connected to the mount");
                goto end;
            }

            if (!M3PortExists(status)) {
                i.Add("M3 ports do not exist on this system");
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

        private short GetM3PortStatus(CancellationToken ct) {
            Dictionary<string, string> status = [];

            Task.Run(async () => {
                status = await Utilities.Pwi4GetStatus(Pwi4IpAddress, Pwi4Port, ct);
            }, ct).Wait(ct);

            return !short.TryParse(status["m3.port"], CultureInfo.InvariantCulture, out short port)
                ? throw new SequenceEntityFailedException("Unable to determine M3 port status")
                : port;
        }

        private static bool M3PortExists(Dictionary<string, string> status) {
            return short.Parse(status["m3.exists"], CultureInfo.InvariantCulture) != 0;
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