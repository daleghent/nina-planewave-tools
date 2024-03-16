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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace DaleGhent.NINA.PlaneWaveTools.ShutterControl {

    [ExportMetadata("Name", "Shutter Control")]
    [ExportMetadata("Description", "Controls the PlaneWave mirror shutter")]
    [ExportMetadata("Icon", "BahtinovSVG")]
    [ExportMetadata("Category", "PlaneWave Tools")]
    [Export(typeof(ISequenceItem))]
    [JsonObject(MemberSerialization.OptIn)]
    public partial class ShutterControl : SequenceItem, IValidatable, INotifyPropertyChanged {
        private int shutterAction = 0;
        private bool launchShutterController = true;

        [ImportingConstructor]
        public ShutterControl() {
            PwiShutterCtlAddress = Properties.Settings.Default.PwscIpAddress;
            PwiShutterCtlPort = Properties.Settings.Default.PwscPort;
            PwiShutterCtlPath = Properties.Settings.Default.PwscExePath;

            Properties.Settings.Default.PropertyChanged += SettingsChanged;
        }

        [JsonProperty]
        public int ShutterAction {
            get => shutterAction;
            set {
                shutterAction = value;
                RaisePropertyChanged();
            }
        }

        [JsonProperty]
        public bool LaunchShutterController {
            get => launchShutterController;
            set {
                launchShutterController = value;
                RaisePropertyChanged();
            }
        }

        public static IList<string> ShutterActions => ItemLists.ShutterActions;
        public string ShutterActionName => ItemLists.ShutterActions[shutterAction];

        public override async Task Execute(IProgress<ApplicationStatus> progress, CancellationToken ct) {
            var shutterCtrlProc = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(PwiShutterCtlPath)).Length;

            if (shutterCtrlProc == 0 && launchShutterController == true) {
                StartShutterControllerApp();
                await Task.Delay(TimeSpan.FromSeconds(5), ct);
                Validate();
            } else if (shutterCtrlProc == 0 && launchShutterController == false) {
                throw new SequenceEntityFailedException("PlaneWave Shutter Controller is not running");
            }

            await SendShutterCtrlCommand("connect", ct);

            if (!await GetShutterConnection(ct)) {
                throw new SequenceEntityFailedException("PlaneWave Shutter Controller could not connect to the hardware");
            }

            var shutterStatus = await GetShutterStatus(ct);

            if (shutterStatus == ShutterStatusEnum.Errored || shutterStatus == ShutterStatusEnum.StatusError) {
                throw new SequenceEntityFailedException("Shutter is in an errored state");
            }

            ShutterStatusEnum wantedState;

            if (shutterAction == 0) {
                wantedState = ShutterStatusEnum.Open;

                if (shutterStatus != wantedState) {
                    await SendShutterCtrlCommand("stop", ct);
                    await SendShutterCtrlCommand("beginopen", ct);
                }
            } else {
                wantedState = ShutterStatusEnum.Closed;

                if (shutterStatus != wantedState) {
                    await SendShutterCtrlCommand("stop", ct);
                    await SendShutterCtrlCommand("beginclose", ct);
                }
            }

            do {
                if (shutterStatus == ShutterStatusEnum.Errored) {
                    progress?.Report(new ApplicationStatus { Status = string.Empty });
                    throw new SequenceEntityFailedException("Shutter is in an errored state");
                }

                await Task.Delay(TimeSpan.FromSeconds(1), ct);

                shutterStatus = await GetShutterStatus(ct);
                progress?.Report(new ApplicationStatus { Status = $"Shutter status: {shutterStatus}" });
            } while (shutterStatus != wantedState);

            progress?.Report(new ApplicationStatus { Status = string.Empty });

            return;
        }

        private ShutterControl(ShutterControl copyMe) : this() {
            CopyMetaData(copyMe);
        }

        public override object Clone() {
            return new ShutterControl(this) {
                ShutterAction = ShutterAction,
                LaunchShutterController = LaunchShutterController,
            };
        }

        public override string ToString() {
            return $"Category: {Category}, Item: {Name}, ShutterAction: {ItemLists.ShutterActions[ShutterAction]}";
        }

        public IList<string> Issues { get; set; } = new ObservableCollection<string>();

        public bool Validate() {
            var i = new List<string>();

            var shutterCtrlProc = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(PwiShutterCtlPath)).Length;

            // Skip validations if PWSC is not yet running but it is set to be started by the instruction.
            if (shutterCtrlProc == 0 && launchShutterController == true) {
                goto end;
            }

            if (shutterCtrlProc == 0 && launchShutterController == false) {
                i.Add("PlaneWave Shutter Controller is not running");
                goto end;
            }

            Task.Run(async () => {
                try {
                    if (!await GetShutterConnection(CancellationToken.None)) {
                        i.Add("PlaneWave Shutter Controller is not connected to the hardware");
                    }
                } catch (Exception ex) {
                    i.Add($"{ex.Message}");
                }
            }).Wait();

            if (i.Count > 0) {
                goto end;
            }

            ShutterStatusEnum shutterStatus = ShutterStatusEnum.StatusError;

            Task.Run(async () => {
                try {
                    shutterStatus = await GetShutterStatus(CancellationToken.None);
                } catch (Exception ex) {
                    i.Add($"{ex.Message}");
                }
            }).Wait();

            if (i.Count > 0) {
                goto end;
            }

            if (shutterStatus == ShutterStatusEnum.StatusError) {
                i.Add("PlaneWave Shutter Controller encontered an internal error");
                goto end;
            }

            if (shutterStatus == ShutterStatusEnum.Errored) {
                i.Add("Shutter is in an errored state. Possible hardware issue.");
            }

        end:
            if (i != Issues) {
                Issues = i;
                RaisePropertyChanged(nameof(Issues));
            }

            return i.Count == 0;
        }

        private string PwiShutterCtlAddress { get; set; }
        private ushort PwiShutterCtlPort { get; set; }
        private string PwiShutterCtlPath { get; set; }

        private async Task<ShutterStatusEnum> GetShutterStatus(CancellationToken ct) {
            ShutterStatusEnum statusCode = ShutterStatusEnum.StatusError;

            try {
                var response = await SendShutterCtrlCommand("shutterstate", ct);
                var regex = StatusCodeRegex();
                var status = regex.Match(response);
                statusCode = (ShutterStatusEnum)short.Parse(status.Value, CultureInfo.InvariantCulture);
                Logger.Debug($"shutterstate = {statusCode}");
            } catch {
            }

            return statusCode;
        }

        private async Task<bool> GetShutterConnection(CancellationToken ct) {
            bool isConnected = false;

            var response = await SendShutterCtrlCommand("isconnected", ct);
            var regex = StatusCodeRegex();
            var status = regex.Match(response);

            if (status.Value.Trim().Equals("0")) {
                isConnected = true;
            }

            Logger.Debug($"isconnected = {isConnected}");

            return isConnected;
        }

        private async Task<string> SendShutterCtrlCommand(string command, CancellationToken ct) {
            string response = string.Empty;

            try {
                var tcpClient = new TcpClient();
                await tcpClient.ConnectAsync(PwiShutterCtlAddress, PwiShutterCtlPort, ct);

                var ns = tcpClient.GetStream();
                var sw = new StreamWriter(ns) {
                    AutoFlush = true,
                };

                if (tcpClient.Connected) {
                    Logger.Trace($"Shutter Controller command: {command}");

                    var sb = new StringBuilder(command.Trim());
                    sb.Append('\n');

                    await sw.WriteAsync(sb, ct);

                    response = await new StreamReader(ns).ReadLineAsync(ct);
                    Logger.Trace($"Shutter Controller response: {response}");
                }
            } catch {
            }

            return response.Trim();
        }

        private void StartShutterControllerApp() {
            var shctlr = new ProcessStartInfo(PwiShutterCtlPath) {
                Arguments = "connect"
            };

            try {
                var cmd = Process.Start(shctlr);
                Logger.Info($"{PwiShutterCtlPath} started with PID {cmd.Id}");
            } catch (Exception ex) {
                throw new SequenceEntityFailedException(ex.Message);
            }

            return;
        }

        private void SettingsChanged(object sender, PropertyChangedEventArgs e) {
            switch (e.PropertyName) {
                case nameof(PwiShutterCtlAddress):
                    PwiShutterCtlAddress = Properties.Settings.Default.PwscIpAddress;
                    break;

                case nameof(PwiShutterCtlPort):
                    PwiShutterCtlPort = Properties.Settings.Default.PwscPort;
                    break;

                case nameof(PwiShutterCtlPath):
                    PwiShutterCtlPath = Properties.Settings.Default.PwscExePath;
                    break;
            }
        }

        [GeneratedRegex("^\\d+")]
        private static partial Regex StatusCodeRegex();
    }
}