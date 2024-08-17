#region "copyright"

/*
    Copyright (c) 2024 Dale Ghent <daleg@elemental.org>

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/
*/

#endregion "copyright"

using CommunityToolkit.Mvvm.Input;
using DaleGhent.NINA.PlaneWaveTools.Utility;
using Newtonsoft.Json;
using NINA.Core.Model;
using NINA.Core.Utility;
using NINA.Core.Utility.Notification;
using NINA.Sequencer.SequenceItem;
using NINA.Sequencer.Validations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace DaleGhent.NINA.PlaneWaveTools.ModelManagement {

    [ExportMetadata("Name", "Load Model")]
    [ExportMetadata("Description", "Loads a PointXP model file into PWI4")]
    [ExportMetadata("Icon", "PointXP_SVG")]
    [ExportMetadata("Category", "PlaneWave Tools")]
    [Export(typeof(ISequenceItem))]
    [JsonObject(MemberSerialization.OptIn)]
    public partial class LoadModel : SequenceItem, IValidatable, INotifyPropertyChanged {
        private string modelFile = string.Empty;

        [ImportingConstructor]
        public LoadModel() {
            Pwi4IpAddress = Properties.Settings.Default.Pwi4IpAddress;
            Pwi4Port = Properties.Settings.Default.Pwi4Port;

            Properties.Settings.Default.PropertyChanged += SettingsChanged;
        }

        [JsonProperty]
        public string ModelFile {
            get => modelFile;
            set {
                if (File.Exists(Path.Combine(ModelPath, value))) {
                    modelFile = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(ModelFileBaseName));
                    Validate();
                    Logger.Info($"Model file set to {modelFile}");
                }
            }
        }

        public string ModelFileBaseName => Path.GetFileName(modelFile);

        private static string ModelPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"PlaneWave Instruments", @"PWI4", @"Mount");

        private LoadModel(LoadModel copyMe) : this() {
            CopyMetaData(copyMe);
        }

        public override async Task Execute(IProgress<ApplicationStatus> progress, CancellationToken ct) {
            var url = $"/mount/model/load?filename={HttpUtility.UrlEncode(modelFile)}";

            var response = await Utilities.HttpRequestAsync(Pwi4IpAddress, Pwi4Port, url, HttpMethod.Get, string.Empty, ct);

            if (response.StatusCode != System.Net.HttpStatusCode.OK) {
                var error = await response.Content.ReadAsStringAsync(ct);
                throw new SequenceEntityFailedException($"Failed to load PointXP model {modelFile}: {error.Trim()}");
            }

            return;
        }

        public override object Clone() {
            return new LoadModel(this) {
                ModelFile = ModelFile,
            };
        }

        public override string ToString() {
            return $"Category: {Category}, Item: {Name}, ModelFile: {modelFile}";
        }

        public IList<string> Issues { get; set; } = new ObservableCollection<string>();

        public bool Validate() {
            var i = new List<string>();

            if (!Pwi4StatusChecker.Pwi4IsRunning) {
                i.Add(Pwi4StatusChecker.NotConnectedReason);
                goto end;
            }

            if (string.IsNullOrEmpty(modelFile) || !File.Exists(Path.Combine(ModelPath, modelFile))) {
                i.Add("Model file does not exist");
                goto end;
            }

            if (!ValidFileNameChars().IsMatch(ModelFileBaseName)) {
                i.Add("Model file name contains invalid characters");
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

        [RelayCommand]
        internal void OpenSelectModelFileDialog(object obj) {
            Microsoft.Win32.OpenFileDialog dialog = new() {
                FileName = string.Empty,
                InitialDirectory = ModelPath,
                Title = "Select PointXP model file",
                Filter = "PointXP Model|*.pxp|All files|*.*",
            };

            if (dialog.ShowDialog() == true) {
                if (Path.Exists(dialog.FileName) && Path.GetDirectoryName(dialog.FileName).Equals(ModelPath)) {
                    ModelFile = Path.GetFileName(dialog.FileName);
                } else {
                    Notification.ShowError($"PointXP model file must reside in {ModelPath}");
                }
            }
        }

        [GeneratedRegex(@"^[a-zA-Z0-9\._]+$")]
        private static partial Regex ValidFileNameChars();
    }
}