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
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace DaleGhent.NINA.PlaneWaveTools.StartStopPwi3 {

    [ExportMetadata("Name", "Start PWI3")]
    [ExportMetadata("Description", "Starts PWI3")]
    [ExportMetadata("Icon", "PWI3_SVG")]
    [ExportMetadata("Category", "PlaneWave Tools")]
    [Export(typeof(ISequenceItem))]
    [JsonObject(MemberSerialization.OptIn)]
    public class StartPwi3 : SequenceItem, IValidatable, INotifyPropertyChanged {

        [ImportingConstructor]
        public StartPwi3() {
            Pwi3IpAddress = Properties.Settings.Default.Pwi3IpAddress;
            Pwi3Port = Properties.Settings.Default.Pwi3Port;
            Pwi3ExePath = Properties.Settings.Default.Pwi3ExePath;

            Properties.Settings.Default.PropertyChanged += SettingsChanged;
        }

        private StartPwi3(StartPwi3 copyMe) : this() {
            CopyMetaData(copyMe);
        }

        public override async Task Execute(IProgress<ApplicationStatus> progress, CancellationToken ct) {
            if (Process.GetProcessesByName("PWI3").Length == 0) {
                RunPwi3();
            } else {
                Logger.Info("PWI3 is already running!");
                return;
            }

            short timeout = 30;

            for (int i = 0; i < timeout; i++) {
                try {
                    if (await Utilities.TestTcpPort(Pwi3IpAddress, Pwi3Port)) {
                        Logger.Debug($"Was able to connect to PWI3 after {i} seconds");
                        break;
                    }
                } catch (SocketException e) {
                    Logger.Debug($"Failed to connect to PWI3: {e.Message}");
                } catch (Exception e) {
                    throw new SequenceEntityFailedException($"Failure when attempting to connect to PWI3: {e.Message}");
                }

                if (i == timeout - 1) {
                    throw new SequenceEntityFailedException("Timed out trying to connect to PWI3");
                }

                if (ct.IsCancellationRequested) { return; }

                await Task.Delay(TimeSpan.FromSeconds(1), ct);
            }

            // Wait 5 seconds for PWI3 to connect to eqipment
            await Task.Delay(TimeSpan.FromSeconds(5), ct);
        }

        public override object Clone() {
            return new StartPwi3(this);
        }

        public override string ToString() {
            return $"Category: {Category}, Item: {Name}";
        }

        public IList<string> Issues { get; set; } = new ObservableCollection<string>();

        public bool Validate() {
            var i = new List<string>();

            if (string.IsNullOrEmpty(Pwi3ExePath) || !File.Exists(Pwi3ExePath)) {
                i.Add("Invalid location for PWI3.exe");
            }

            if (i != Issues) {
                Issues = i;
                RaisePropertyChanged(nameof(Issues));
            }

            return i.Count == 0;
        }

        private string Pwi3ExePath { get; set; }
        private string Pwi3IpAddress { get; set; }
        private ushort Pwi3Port { get; set; }

        private void RunPwi3() {
            var appm = new ProcessStartInfo(Pwi3ExePath);

            try {
                var cmd = Process.Start(appm);
                Logger.Info($"{Pwi3ExePath} started with PID {cmd.Id}");
            } catch (Exception ex) {
                throw new SequenceEntityFailedException(ex.Message);
            }

            return;
        }

        private void SettingsChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case "Pwi3ExePath":
                    Pwi3ExePath = Properties.Settings.Default.Pwi3ExePath;
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