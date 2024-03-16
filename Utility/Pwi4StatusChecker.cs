#region "copyright"

/*
    Copyright (c) 2024 Dale Ghent <daleg@elemental.org>

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/
*/

#endregion "copyright"

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Settings = DaleGhent.NINA.PlaneWaveTools.Properties.Settings;

namespace DaleGhent.NINA.PlaneWaveTools.Utility {

    internal static class Pwi4StatusChecker {
        private static readonly PeriodicTimer timer;
        private static readonly CancellationTokenSource pollCts;
        private static readonly Task pollTask;

        static Pwi4StatusChecker() {
            timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
            pollCts = new CancellationTokenSource();
            pollTask = Task.Run(() => Poll(pollCts.Token));
        }

        // Common items that any instruction might ask about

        public static Version Pwi4Version { get; private set; } = null;
        public static bool Pwi4IsRunning { get; private set; } = false;
        public static bool IsConnected { get; private set; } = false;
        public static string NotConnectedReason { get; private set; } = string.Empty;

        private static Dictionary<string, string> Status { get; set; } = null;

        private static async Task Poll(CancellationToken token) {
            while (await timer.WaitForNextTickAsync(token) && !token.IsCancellationRequested) {
                try {
                    var pwi4 = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(Settings.Default.Pwi4ExePath));

                    if (pwi4.Length == 0) {
                        Pwi4IsRunning = IsConnected = false;
                        NotConnectedReason = "PWI4 is not running";
                        continue;
                    } else {
                        Pwi4IsRunning = true;
                        NotConnectedReason = string.Empty;
                    }

                    Status = await Utilities.Pwi4GetStatus(Settings.Default.Pwi4IpAddress, Settings.Default.Pwi4Port, token);

                    if (Status.TryGetValue("mount.is_connected", out var mountIsConnected)) {
                        if (Utilities.Pwi4BoolStringToBoolean(mountIsConnected)) {
                            IsConnected = true;
                            NotConnectedReason = string.Empty;
                        } else {
                            IsConnected = false;
                            NotConnectedReason = "PWI4 is not connected to the mount";
                        }
                    } else {
                        NotConnectedReason = "Unable to determine mount connection status";
                    }

                    try {
                        if (Status.TryGetValue("pwi4.version", out var pwi4Version)) {
                            Pwi4Version = Version.Parse(pwi4Version);
                        }
                    } catch { }
                } catch (HttpRequestException) {
                    NotConnectedReason = "Could not communicate with PWI4";
                    IsConnected = false;
                    Pwi4Version = null;
                } catch (Exception ex) {
                    NotConnectedReason = ex.Message;
                    IsConnected = false;
                    Pwi4Version = null;
                }
            }
        }

        public static bool? GetBool(string key) {
            return Status.TryGetValue(key, out var val) ? Utilities.Pwi4BoolStringToBoolean(val) : null;
        }

#nullable enable

        public static string? GetString(string key) {
            return Status.TryGetValue(key, out var val) ? val : null;
        }

#nullable disable

        public static int? GetInt(string key) {
            if (Status.TryGetValue(key, out var val)) {
                if (int.TryParse(val, CultureInfo.InvariantCulture, out var i)) {
                    return i;
                }
            }

            return null;
        }

        public static short? GetShort(string key) {
            if (Status.TryGetValue(key, out var val)) {
                if (short.TryParse(val, CultureInfo.InvariantCulture, out var i)) {
                    return i;
                }
            }

            return null;
        }

        public static double? GetDouble(string key) {
            if (Status.TryGetValue(key, out var val)) {
                if (double.TryParse(val, CultureInfo.InvariantCulture, out var d)) {
                    return d;
                }
            }

            return null;
        }

        public static void Shutdown() {
            try {
                pollCts?.Cancel();
            } catch { }
        }
    }
}