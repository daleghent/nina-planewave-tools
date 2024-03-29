﻿#region "copyright"

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
            int waitSeconds = 1;
            int waitAttempts = 90;
            int i = 0;

            try {
                var m3Postion = GetM3Position();

                if (m3Postion == 0) {
                    throw new SequenceEntityFailedException($"M3 was asked to move to port {M3Port} but it either doesn't exist or is already in motion!");
                }

                if (m3Postion == M3Port) {
                    Logger.Info($"M3 port is already at position {M3Port}");
                    return;
                }

                await SendM3Command(ct);

                do {
                    if (i >= waitAttempts) {
                        throw new SequenceEntityFailedException($"{Name} waited too long to set M3 port to {M3Port}. Verify that the M3 unit is operating correctly");
                    }

                    progress?.Report(new ApplicationStatus { Status = $"Moving M3 to port {M3Port}" });
                    await Task.Delay(TimeSpan.FromSeconds(waitSeconds), ct);

                    m3Postion = GetM3Position();
                    i++;
                } while (m3Postion != M3Port && !ct.IsCancellationRequested);
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

            if (!Pwi4StatusChecker.Pwi4IsRunning) {
                i.Add(Pwi4StatusChecker.NotConnectedReason);
                goto end;
            }

            if (!Pwi4StatusChecker.IsConnected) {
                i.Add(Pwi4StatusChecker.NotConnectedReason);
            } else {
                if (!M3Present()) {
                    i.Add("M3 ports do not exist on this system");
                }
            }

        end:
            if (i != Issues) {
                Issues = i;
                RaisePropertyChanged(nameof(Issues));
            }

            return i.Count == 0;
        }

        private async Task SendM3Command(CancellationToken ct) {
            string url = $"/m3/goto?port={M3Port}";
            var response = await Utilities.HttpRequestAsync(Pwi4IpAddress, Pwi4Port, url, HttpMethod.Get, string.Empty, ct);

            if (!response.IsSuccessStatusCode) {
                throw new SequenceEntityFailedException($"PWI4 returned status {response.StatusCode} for {url}");
            }
        }

        private static short GetM3Position() {
            return Pwi4StatusChecker.GetShort("m3.port") ?? throw new SequenceEntityFailedException("Could not determine M3 port position");
        }

        private static bool M3Present() {
            return Pwi4StatusChecker.GetBool("m3.exists") ?? throw new SequenceEntityFailedException("Could not determine M3 presence");
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