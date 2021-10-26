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
using System.Diagnostics;
using System.Linq;
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
        private DeltaTHeatersEnum deltaTHeater = DeltaTHeatersEnum.Primary;
        private DeltaTHeaterModesEnum deltaTHeaterMode = DeltaTHeaterModesEnum.Off;
        private readonly string pwi3UrlBase = "/?device=heater";

        [ImportingConstructor]
        public DeltaTControl() {
            Pwi3IpAddress = Properties.Settings.Default.Pwi3IpAddress;
            Pwi3Port = Properties.Settings.Default.Pwi3Port;
            Pwi3ClientId = Properties.Settings.Default.Pwi3ClientId;

            Properties.Settings.Default.PropertyChanged += SettingsChanged;
        }

        [JsonProperty]
        public DeltaTHeatersEnum DeltaTHeater {
            get => deltaTHeater;
            set {
                deltaTHeater = value;
                RaisePropertyChanged();
            }
        }

        [JsonProperty]
        public DeltaTHeaterModesEnum DeltaTHeaterMode {
            get => deltaTHeaterMode;
            set {
                deltaTHeaterMode = value;
                RaisePropertyChanged();
            }
        }

        private DeltaTControl(DeltaTControl copyMe) : this() {
            CopyMetaData(copyMe);
        }

        public override async Task Execute(IProgress<ApplicationStatus> progress, CancellationToken token) {
            string mode = "off";
            string url = $"{pwi3UrlBase}&clientId={Pwi3ClientId}";

            switch (DeltaTHeaterMode) {
                case DeltaTHeaterModesEnum.On:
                    mode = "on";
                    break;

                case DeltaTHeaterModesEnum.Control:
                    mode = "control";
                    break;

                case DeltaTHeaterModesEnum.OnWhenLessThan:
                    mode = "on_when_less_than";
                    break;
            }

            url += $"&index={(int)DeltaTHeater}&mode={mode}";

            try {
                await Utilities.HttpGetRequestAsync(Pwi3IpAddress, Pwi3Port, url, token);
            } catch {
                throw;
            }

            return;
        }

        public override object Clone() {
            return new DeltaTControl(this) {
                Icon = Icon,
                Name = Name,
                Category = Category,
                Description = Description,
                DeltaTHeater = DeltaTHeater,
                DeltaTHeaterMode = DeltaTHeaterMode,
            };
        }

        public override string ToString() {
            return $"Category: {Category}, Item: {nameof(DeltaTControl)}";
        }

        public IList<string> Issues { get; set; } = new ObservableCollection<string>();

        public bool Validate() {
            return true;
        }

        private string Pwi3ClientId { get; set; }
        private string Pwi3IpAddress { get; set; }
        private ushort Pwi3Port { get; set; }

        void SettingsChanged(object sender, PropertyChangedEventArgs e) {
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