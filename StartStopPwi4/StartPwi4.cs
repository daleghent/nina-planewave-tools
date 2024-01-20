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

namespace DaleGhent.NINA.PlaneWaveTools.StartStopPwi4 {

    [ExportMetadata("Name", "Start PWI4")]
    [ExportMetadata("Description", "Starts PWI4")]
    [ExportMetadata("Icon", "PWI4_SVG")]
    [ExportMetadata("Category", "PlaneWave Tools")]
    [Export(typeof(ISequenceItem))]
    [JsonObject(MemberSerialization.OptIn)]
    public class StartPwi4 : SequenceItem, IValidatable, INotifyPropertyChanged {

        [ImportingConstructor]
        public StartPwi4() {
            Pwi4IpAddress = Properties.Settings.Default.Pwi4IpAddress;
            Pwi4Port = Properties.Settings.Default.Pwi4Port;
            Pwi4ExePath = Properties.Settings.Default.Pwi4ExePath;

            Properties.Settings.Default.PropertyChanged += SettingsChanged;
        }

        private StartPwi4(StartPwi4 copyMe) : this() {
            CopyMetaData(copyMe);
        }

        public override async Task Execute(IProgress<ApplicationStatus> progress, CancellationToken ct) {
            if (Process.GetProcessesByName("PWI4").Length == 0) {
                RunPwi4();
            } else {
                Logger.Info("PWI4 is already running!");
                return;
            }

            short timeout = 30;

            for (int i = 0; i < timeout; i++) {
                try {
                    if (await Utilities.TestTcpPort(Pwi4IpAddress, Pwi4Port)) {
                        Logger.Debug($"Was able to connect to PWI4 after {i} seconds");
                        break;
                    }
                } catch (SocketException e) {
                    Logger.Debug($"Failed to connect to PWI4: {e.Message}");
                } catch (Exception e) {
                    throw new SequenceEntityFailedException($"Failure when attempting to connect to PWI4: {e.Message}");
                }

                if (i == timeout - 1) {
                    throw new SequenceEntityFailedException("Timed out trying to connect to PWI4");
                }

                if (ct.IsCancellationRequested) { return; }

                await Task.Delay(TimeSpan.FromSeconds(1), ct);
            }

            // Wait 5 seconds for PWI4 to connect to equipment
            await Task.Delay(TimeSpan.FromSeconds(5), ct);
        }

        public override object Clone() {
            return new StartPwi4(this);
        }

        public override string ToString() {
            return $"Category: {Category}, Item: {nameof(StartPwi4)}";
        }

        public IList<string> Issues { get; set; } = new ObservableCollection<string>();

        public bool Validate() {
            var i = new List<string>();

            if (string.IsNullOrEmpty(Pwi4ExePath) || !File.Exists(Pwi4ExePath)) {
                i.Add("Invalid location for PWI4.exe");
            }

            if (i != Issues) {
                Issues = i;
                RaisePropertyChanged(nameof(Issues));
            }

            return i.Count == 0;
        }

        private string Pwi4ExePath { get; set; }
        private string Pwi4IpAddress { get; set; }
        private ushort Pwi4Port { get; set; }

        private void RunPwi4() {
            var appm = new ProcessStartInfo(Pwi4ExePath);

            try {
                var cmd = Process.Start(appm);
                Logger.Info($"{Pwi4ExePath} started with PID {cmd.Id}");
            } catch (Exception ex) {
                throw new SequenceEntityFailedException(ex.Message);
            }

            return;
        }

        private void SettingsChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(Pwi4ExePath):
                    Pwi4ExePath = Properties.Settings.Default.Pwi4ExePath;
                    break;

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