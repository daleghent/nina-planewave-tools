#region "copyright"

/*
    Copyright Dale Ghent <daleg@elemental.org>

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/
*/

#endregion "copyright"

using NINA.Core.Utility;
using NINA.Plugin;
using NINA.Plugin.Interfaces;
using NINA.Profile.Interfaces;
using System;
using System.IO;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace DaleGhent.NINA.PlaneWaveTools {
    [Export(typeof(IPluginManifest))]
    public class PlaneWaveTools : PluginBase, ISettings, INotifyPropertyChanged {

        [ImportingConstructor]
        public PlaneWaveTools() {
            if (Properties.Settings.Default.UpgradeSettings) {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeSettings = false;
                Properties.Settings.Default.Save();
            }

            if (string.IsNullOrEmpty(Pwi3ClientId) || string.IsNullOrWhiteSpace(Pwi3ClientId)) {
                Pwi3ClientId = Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 8);
            }

            Pwi3ExePathDialogCommand = new RelayCommand(OpenPwi3ExePathDialog);
        }

        public string Pwi3ExePath {
            get => Properties.Settings.Default.Pwi3ExePath;
            set {
                Properties.Settings.Default.Pwi3ExePath = value;
                Properties.Settings.Default.Save();
                RaisePropertyChanged();
            }
        }

        public string Pwi3IpAddress {
            get => Properties.Settings.Default.Pwi3IpAddress;
            set {
                Properties.Settings.Default.Pwi3IpAddress = value;
                Properties.Settings.Default.Save();
                RaisePropertyChanged();
            }
        }

        public ushort Pwi3Port {
            get => Properties.Settings.Default.Pwi3Port;
            set {
                Properties.Settings.Default.Pwi3Port = value;
                Properties.Settings.Default.Save();
                RaisePropertyChanged();
            }
        }

        public string Pwi3ClientId {
            get => Properties.Settings.Default.Pwi3ClientId;
            set {
                Properties.Settings.Default.Pwi3ClientId = value;
                Properties.Settings.Default.Save();
                RaisePropertyChanged();
            }
        }

        private void OpenPwi3ExePathDialog(object obj) {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog {
                FileName = string.Empty,
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                Filter = "Any Program|*.exe"
            };

            if (dialog.ShowDialog() == true) {
                Pwi3ExePath = dialog.FileName;
            }
        }

        public ICommand Pwi3ExePathDialogCommand { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}