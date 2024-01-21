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
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DaleGhent.NINA.PlaneWaveTools.Fans {

    [ExportMetadata("Name", "Fan Control (PWI3)")]
    [ExportMetadata("Description", "Turns OTA cooling fans on or off via PWI3")]
    [ExportMetadata("Icon", "FanControl_SVG")]
    [ExportMetadata("Category", "PlaneWave Tools")]
    [Export(typeof(ISequenceItem))]
    [JsonObject(MemberSerialization.OptIn)]
    public class FanControl : SequenceItem, IValidatable, INotifyPropertyChanged {
        private bool fanState = false;
        private readonly string pwi3UrlBase = "/?device=fans";

        [ImportingConstructor]
        public FanControl() {
            Pwi3IpAddress = Properties.Settings.Default.Pwi3IpAddress;
            Pwi3Port = Properties.Settings.Default.Pwi3Port;
            Pwi3ClientId = Properties.Settings.Default.Pwi3ClientId;

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

        private FanControl(FanControl copyMe) : this() {
            CopyMetaData(copyMe);
        }

        public override async Task Execute(IProgress<ApplicationStatus> progress, CancellationToken ct) {
            string url = $"{pwi3UrlBase}&clientId={Pwi3ClientId}";
            var fanStateString = FanState ? "normal" : "off";
            url += $"&mode={fanStateString}";

            try {
                await Utilities.HttpRequestAsync(Pwi3IpAddress, Pwi3Port, url, HttpMethod.Get, string.Empty, ct);
                byte attempts = 0;

                do {
                    if (attempts > 10) {
                        throw new SequenceEntityFailedException($"Waited too long for fans to change state to {fanStateString}");
                    }

                    await Task.Delay(TimeSpan.FromSeconds(1), ct);

                    var pwi3Status = await Utilities.Pwi3GetStatus(Pwi3IpAddress, Pwi3Port, ct);

                    if (string.IsNullOrEmpty(pwi3Status.Status.Fans)) {
                        throw new SequenceEntityFailedException($"Attempted to set fans to {fanStateString} but no fans appear to be connected in PWI3");
                    }

                    if (pwi3Status.Status.Fans.Equals(fanStateString)) {
                        Logger.Info($"PWI3 fans have been set to {fanStateString}");
                        break;
                    }

                    Logger.Debug($"PWI3 fan state: Requested = {fanStateString}, PWI3 = {pwi3Status.Status.Fans}");
                    attempts++;
                } while (true);
            } catch {
                throw;
            }

            return;
        }

        public override object Clone() {
            return new FanControl(this) {
                FanState = FanState,
            };
        }

        public override string ToString() {
            return $"Category: {Category}, Item: {Name}, FanState: {FanState}";
        }

        public IList<string> Issues { get; set; } = new ObservableCollection<string>();

        public bool Validate() {
            var i = new List<string>();
            var status = new Pwi3Status.System();

            Task.Run(async () => {
                try {
                    var status = await Utilities.Pwi3GetStatus(Pwi3IpAddress, Pwi3Port, CancellationToken.None);
                } catch (HttpRequestException) {
                    i.Add("Could not communicate with PWI3");
                } catch (Exception ex) {
                    i.Add($"{ex.Message}");
                }
            }).Wait();

            if (i.Count > 0) {
                goto end;
            }

        end:

            if (i != Issues) {
                Issues = i;
                RaisePropertyChanged(nameof(Issues));
            }

            return i.Count == 0;
        }

        private string Pwi3ClientId { get; set; }
        private string Pwi3IpAddress { get; set; }
        private ushort Pwi3Port { get; set; }

        private void SettingsChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case "Pwi3ClientId":
                    Pwi3ClientId = Properties.Settings.Default.Pwi3ClientId;
                    break;

                case "Pwi3IpAddress":
                    Pwi3IpAddress = Properties.Settings.Default.Pwi3IpAddress;
                    break;

                case "Pwi3Port":
                    Pwi3Port = Properties.Settings.Default.Pwi3Port;
                    break;
            }
        }
    }
}