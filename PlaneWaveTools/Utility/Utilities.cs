#region "copyright"

/*
    Copyright Dale Ghent <daleg@elemental.org>

    This Source Code Form is subject to the terms of the Mozilla Public
    License, v. 2.0. If a copy of the MPL was not distributed with this
    file, You can obtain one at http://mozilla.org/MPL/2.0/
*/

#endregion "copyright"

using NINA.Astrometry;
using NINA.Core.Enum;
using NINA.Core.Model;
using NINA.Core.Utility;
using NINA.Sequencer.Container;
using NINA.Sequencer.SequenceItem;
using NINA.Sequencer.Validations;
using System;
using System.ComponentModel;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace DaleGhent.NINA.PlaneWaveTools.Utility {

    public class Utilities {

        public static async Task<bool> HttpGetRequestAsync(string host, ushort port, string url, CancellationToken ct) {
            bool success = false;
            var uri = new Uri($"http://{host}:{port}{url}");

            if (!uri.IsWellFormedOriginalString()) {
                throw new SequenceEntityFailedException($"Invalid or malformed URL: {uri}");
            }

            var client = new HttpClient();
            var response = new HttpResponseMessage();

            Logger.Debug($"Querying: {uri}");
            response = await client.GetAsync(uri, ct);
            Logger.Debug($"Response: {response}");

            success = (int)response.StatusCode < 400
                ? true
                : throw new SequenceEntityFailedException($"API server returned error code {(int)response.StatusCode}");

            response.Dispose();
            client.Dispose();

            return success;
        }

        public static async Task<bool> TestTcpPort(string host, ushort port) {
            bool success = false;
            Socket s = null;

            try {
                s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                await s.ConnectAsync(host, port);
                s.Disconnect(true);
                success = true;
            } catch {
                throw;
            } finally {
                s.Close();
                s.Dispose();
            }

            return success;
        }
    }
}