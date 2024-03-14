using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

        public static bool IsConnected { get; private set; }
        public static string NotConnectedReason { get; private set; }
        public static bool M3PortExists { get; private set; }

        private static async Task Poll(CancellationToken token) {
            while (await timer.WaitForNextTickAsync() && !token.IsCancellationRequested) {
                try {
                    var status = await Utilities.Pwi4GetStatus(Properties.Settings.Default.Pwi4IpAddress, Properties.Settings.Default.Pwi4Port, token);

                    if (status.ContainsKey("mount.is_connected")) {
                        if (Utilities.Pwi4BoolStringToBoolean(status["mount.is_connected"])) {
                            IsConnected = true;
                            NotConnectedReason = "";
                        } else {
                            IsConnected = false;
                            NotConnectedReason = "PWI4 is not connected to the mount";
                        }
                    } else {
                        NotConnectedReason = "Unable to determine mount connection status";
                    }

                    if (status.ContainsKey("m3.exists")) {
                        M3PortExists = short.Parse(status["m3.exists"], CultureInfo.InvariantCulture) != 0;
                    } else {
                        M3PortExists = false;
                    }
                } catch (HttpRequestException) {
                    NotConnectedReason = "Could not communicate with PWI4";
                    IsConnected = false;
                    M3PortExists = false;
                } catch (Exception ex) {
                    NotConnectedReason = ex.Message;
                    IsConnected = false;
                    M3PortExists = false;
                }
            }
        }

        public static void Shutdown() {
            try {
                pollCts?.Cancel();
            } catch { }
        }
    }
}