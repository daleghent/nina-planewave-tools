#region "copyright"

/*
    Copyright (c) 2024 Dale Ghent <daleg@elemental.org>

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/
*/

#endregion "copyright"

using NINA.Core.Model;
using NINA.Core.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace DaleGhent.NINA.PlaneWaveTools.Utility {

    public partial class Utilities {

        public static async Task<HttpResponseMessage> HttpRequestAsync(string host, ushort port, string url, HttpMethod method, string body, CancellationToken ct) {
            var uri = new Uri($"http://{host}:{port}{url}");

            if (!uri.IsWellFormedOriginalString()) {
                throw new SequenceEntityFailedException($"Invalid or malformed URL: {uri}");
            }

            var request = new HttpRequestMessage(method, uri);

            if (!string.IsNullOrEmpty(body)) {
                request.Content = new StringContent(body, Encoding.UTF8, "application/json");
            }

            Logger.Debug($"Request URL: {request.Method} {request.RequestUri}");
            if (request.Method != HttpMethod.Get
                    && request.Method != HttpMethod.Delete
                    && request.Method != HttpMethod.Trace
                    && request.Method != HttpMethod.Options
                    && request.Method != HttpMethod.Head) {
                var requestContent = await (request.Content?.ReadAsStringAsync(ct) ?? Task.FromResult(string.Empty));
                Logger.Trace($"Request body:{Environment.NewLine}{requestContent}");
            }

            using var client = new HttpClient();
            HttpResponseMessage response = null;
            int i = 1;

            while (i < 4) {
                try {
                    response = await client.SendAsync(request, ct);
                    break;
                } catch (WebException ex) {
                    Logger.Error($"HTTP request to {request.RequestUri} failed: {ex.Message}. Retry attempt {i}");
                    i++;
                    if (i < 4) {
                        await Task.Delay(TimeSpan.FromSeconds(1), ct);
                    }
                }
            }

            var responseContent = await (response.Content?.ReadAsStringAsync(ct) ?? Task.FromResult(string.Empty));
            Logger.Debug($"Response status code: {response.StatusCode}");
            Logger.Trace($"Response body:{Environment.NewLine}{responseContent}");

            return response;
        }

        public static async Task<Pwi3Status.System> Pwi3GetStatus(string host, ushort port, CancellationToken ct) {
            if (Process.GetProcessesByName("PWI3").Length < 1) {
                throw new Exception("PWI3 is not running");
            }

            var response = await HttpRequestAsync(host, port, "/", HttpMethod.Get, string.Empty, ct);
            var pwi3Status = new Pwi3Status();

            return pwi3Status.DeserializeStatus(await response.Content.ReadAsStringAsync(ct));
        }

        public static async Task<Dictionary<string, string>> Pwi4GetStatus(string host, ushort port, CancellationToken ct) {
            if (Process.GetProcessesByName("PWI4").Length < 1) {
                throw new Exception("PWI4 is not running");
            }

            var response = await HttpRequestAsync(host, port, "/status", HttpMethod.Get, string.Empty, ct);
            var status = await response.Content.ReadAsStringAsync(ct);

            var dic = StatusRegex()
                .Matches(status)
                .Cast<Match>()
                .ToDictionary(x =>
                    x.Groups[1].Value.Trim(), x => x.Groups[2].Value.Trim()
                );

            Logger.Trace(string.Join(Environment.NewLine, dic.Select(pair => $"{pair.Key} => {pair.Value}")));

            return dic;
        }

        public static async Task<bool> Pwi4CheckMountConnected(string host, ushort port, CancellationToken ct) {
            var status = await Task.Run(() => Pwi4GetStatus(host, port, ct));

            return Pwi4BoolStringToBoolean(status["mount.is_connected"]);
        }

        public static bool Pwi4BoolStringToBoolean(string pwi4BoolString) {
            return pwi4BoolString.Equals("true", StringComparison.OrdinalIgnoreCase) || pwi4BoolString.Equals("1");
        }

        public static async Task<bool> TestTcpPort(string host, ushort port) {
            bool success = false;

            using var s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try {
                await s.ConnectAsync(host, port);
                s.Disconnect(true);
                success = true;
            } catch {
                throw;
            } finally {
                s.Close();
            }

            return success;
        }

        [GeneratedRegex("^(.*)=(.*)$", RegexOptions.Multiline)]
        private static partial Regex StatusRegex();
    }
}