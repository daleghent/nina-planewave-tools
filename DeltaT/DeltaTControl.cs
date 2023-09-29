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

namespace DaleGhent.NINA.PlaneWaveTools.DeltaT {

    [ExportMetadata("Name", "DeltaT Control")]
    [ExportMetadata("Description", "Sets PlaneWave DeltaT heater state")]
    [ExportMetadata("Icon", "DeltaT_SVG")]
    [ExportMetadata("Category", "PlaneWave Tools")]
    [Export(typeof(ISequenceItem))]
    [JsonObject(MemberSerialization.OptIn)]
    public class DeltaTControl : SequenceItem, IValidatable, INotifyPropertyChanged {
        private int deltaTHeater = 0;
        private int deltaTHeaterMode = 0;
        private readonly string pwi3UrlBase = "/?device=heater";

        [ImportingConstructor]
        public DeltaTControl() {
            Pwi3IpAddress = Properties.Settings.Default.Pwi3IpAddress;
            Pwi3Port = Properties.Settings.Default.Pwi3Port;
            Pwi3ClientId = Properties.Settings.Default.Pwi3ClientId;

            Properties.Settings.Default.PropertyChanged += SettingsChanged;
        }

        [JsonProperty]
        public int DeltaTHeater {
            get => deltaTHeater;
            set {
                deltaTHeater = value;
                RaisePropertyChanged();
            }
        }

        [JsonProperty]
        public int DeltaTHeaterMode {
            get => deltaTHeaterMode;
            set {
                deltaTHeaterMode = value;
                RaisePropertyChanged();
            }
        }

        public IList<string> DeltaTHeaters => ItemLists.DeltaTHeaters;
        public IList<string> DeltaTHeaterModes => ItemLists.DeltaTHeaterModes;

        private DeltaTControl(DeltaTControl copyMe) : this() {
            CopyMetaData(copyMe);
        }

        public override async Task Execute(IProgress<ApplicationStatus> progress, CancellationToken ct) {
            string mode = "off";
            string url = $"{pwi3UrlBase}&clientId={Pwi3ClientId}";

            switch (DeltaTHeaterMode) {
                case 1:
                    mode = "on";
                    break;

                case 2:
                    mode = "control";
                    break;

                case 3:
                    mode = "on_when_less_than";
                    break;
            }

            url += $"&index={DeltaTHeater}&mode={mode}";

            try {
                await Utilities.HttpRequestAsync(Pwi3IpAddress, Pwi3Port, url, HttpMethod.Get, string.Empty, ct);
                await Task.Delay(TimeSpan.FromSeconds(5), ct);
            } catch {
                throw;
            }

            return;
        }

        public override object Clone() {
            return new DeltaTControl(this) {
                DeltaTHeater = DeltaTHeater,
                DeltaTHeaterMode = DeltaTHeaterMode,
            };
        }

        public override string ToString() {
            return $"Category: {Category}, Item: {nameof(DeltaTControl)}, DeltaTHeater: {DeltaTHeaters[DeltaTHeater]}, DeltaTHeaterMode: {DeltaTHeaterModes[DeltaTHeaterMode]}";
        }

        public IList<string> Issues { get; set; } = new ObservableCollection<string>();

        public bool Validate() {
            var i = new List<string>();
            var status = new Pwi3Status.System();

            Task.Run(async () => {
                try {
                    status = await Utilities.Pwi3GetStatus(Pwi3IpAddress, Pwi3Port, CancellationToken.None);
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