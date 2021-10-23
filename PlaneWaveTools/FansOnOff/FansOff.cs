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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace DaleGhent.NINA.PlaneWaveTools.FansOnOff {

    [ExportMetadata("Name", "Fans off")]
    [ExportMetadata("Description", "Turns OTA cooling fans off via PWI3")]
    [ExportMetadata("Icon", "FanOn_SVG")]
    [ExportMetadata("Category", "PlaneWave Tools")]
    [Export(typeof(ISequenceItem))]
    [JsonObject(MemberSerialization.OptIn)]
    public class FansOff : SequenceItem, IValidatable, INotifyPropertyChanged {

        [ImportingConstructor]
        public FansOff() {
            Pwi3IpAddress = Properties.Settings.Default.Pwi3IpAddress;
            Pwi3Port = Properties.Settings.Default.Pwi3Port;
            Pwi3ClientId = Properties.Settings.Default.Pwi3ClientId;
             
            Properties.Settings.Default.PropertyChanged += SettingsChanged;
        }

        private FansOff(FansOff copyMe) : this() {
            CopyMetaData(copyMe);
        }

        public override async Task Execute(IProgress<ApplicationStatus> progress, CancellationToken token) {
            string url = $"/?device=fans&mode=off&clientId={Pwi3ClientId}";

            try {
                await Utilities.HttpGetRequestAsync(Pwi3IpAddress, Pwi3Port, url, token);
            } catch {
                throw;
            }

            return;
        }

        public override object Clone() {
            return new FansOff(this);
        }

        public override string ToString() {
            return $"Category: {Category}, Item: {nameof(FansOff)}";
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