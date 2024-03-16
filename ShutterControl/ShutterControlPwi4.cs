#region "copyright"

/*
    Copyright (c) 2024 Dale Ghent <daleg@elemental.org>

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/
*/

#endregion "copyright"

using DaleGhent.NINA.PlaneWaveTools.Enum;
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
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DaleGhent.NINA.PlaneWaveTools.ShutterControl {

    [ExportMetadata("Name", "Shutter Control (PWI4)")]
    [ExportMetadata("Description", "Controls the PlaneWave mirror shutter via PWI4")]
    [ExportMetadata("Icon", "BahtinovSVG")]
    [ExportMetadata("Category", "PlaneWave Tools")]
    [Export(typeof(ISequenceItem))]
    [JsonObject(MemberSerialization.OptIn)]
    public partial class ShutterControlPwi4 : SequenceItem, IValidatable, INotifyPropertyChanged {
        private readonly Version minPwi4Version = Version.Parse("4.1.3");
        private ShutterAction shutterAction = ShutterAction.Open;

        [ImportingConstructor]
        public ShutterControlPwi4() {
            Pwi4IpAddress = Properties.Settings.Default.Pwi4IpAddress;
            Pwi4Port = Properties.Settings.Default.Pwi4Port;

            Properties.Settings.Default.PropertyChanged += SettingsChanged;
        }

        [JsonProperty]
        public ShutterAction ShutterAction {
            get => shutterAction;
            set {
                shutterAction = value;
                RaisePropertyChanged();
            }
        }

        public string ShutterActionByName => ShutterAction.GetDescriptionAttr();

        public override async Task Execute(IProgress<ApplicationStatus> progress, CancellationToken ct) {
            int waitSeconds = 1;
            int waitAttempts = 120;
            int i = 0;

            try {
                var shutterStatus = GetShutterStatus();

                if (shutterStatus == ShutterStatusEnum.Errored) {
                    throw new SequenceEntityFailedException("Shutter is in an errored state");
                }

                if (shutterStatus == (ShutterStatusEnum)shutterAction) {
                    Logger.Info($"Shutter is already at position {shutterAction}");
                    return;
                }

                await SendShutterCommand(shutterAction, ct);
                await Task.Delay(TimeSpan.FromSeconds(waitSeconds), ct);
                shutterStatus = GetShutterStatus();

                do {
                    if (i >= waitAttempts) {
                        throw new SequenceEntityFailedException($"{Name} waited too long to set shutter to {(ShutterStatusEnum)shutterAction}. Verify that the shutter is operating correctly");
                    }

                    if (shutterStatus == ShutterStatusEnum.Errored) {
                        throw new SequenceEntityFailedException("Shutter is in an errored state");
                    }

                    progress?.Report(new ApplicationStatus { Status = $"Shutter status: {shutterStatus}" });

                    await Task.Delay(TimeSpan.FromSeconds(waitSeconds), ct);

                    shutterStatus = GetShutterStatus();
                    ++i;
                } while (shutterStatus != (ShutterStatusEnum)shutterAction && !ct.IsCancellationRequested);
            } catch {
                throw;
            } finally {
                progress?.Report(new ApplicationStatus() { Status = string.Empty });
            }

            return;
        }

        private ShutterControlPwi4(ShutterControlPwi4 copyMe) : this() {
            CopyMetaData(copyMe);
        }

        public override object Clone() {
            return new ShutterControlPwi4(this) {
                ShutterAction = ShutterAction,
            };
        }

        public override string ToString() {
            return $"Category: {Category}, Item: {Name}, ShutterAction: {ShutterAction}";
        }

        public IList<string> Issues { get; set; } = new ObservableCollection<string>();

        public bool Validate() {
            var i = new List<string>();

            if (!Pwi4StatusChecker.Pwi4IsRunning) {
                i.Add(Pwi4StatusChecker.NotConnectedReason);
                goto end;
            }

            if (Pwi4StatusChecker.Pwi4Version < minPwi4Version) {
                i.Add($"PWI4 version is too old for this function. Please update to at least {minPwi4Version}");
                goto end;
            }

            if (!ShutterConnected()) {
                i.Add("PWI4 is not connected to a shutter controller");
                goto end;
            }

            if (GetShutterStatus() == ShutterStatusEnum.Errored) {
                i.Add("Shutter is in an errored state. Possible hardware issue.");
            }

        end:
            if (i != Issues) {
                Issues = i;
                RaisePropertyChanged(nameof(Issues));
            }

            return i.Count == 0;
        }

        private async Task SendShutterCommand(ShutterAction shutterAction, CancellationToken ct) {
            const string baseUrl = "/mirrorcover";
            string url = shutterAction == ShutterAction.Open ? $"{baseUrl}/open" : $"{baseUrl}/close";

            var response = await Utilities.HttpRequestAsync(Pwi4IpAddress, Pwi4Port, url, HttpMethod.Get, string.Empty, ct);

            if (!response.IsSuccessStatusCode) {
                throw new SequenceEntityFailedException($"PWI4 returned status {response.StatusCode} for {url}");
            }
        }

        private static ShutterStatusEnum GetShutterStatus() {
            return (ShutterStatusEnum)Pwi4StatusChecker.GetShort("mirrorcover.overall_state");
        }

        private static bool ShutterConnected() {
            return Pwi4StatusChecker.GetBool("mirrorcover.is_connected") ?? throw new SequenceEntityFailedException("Could not determine if shutter is connected");
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