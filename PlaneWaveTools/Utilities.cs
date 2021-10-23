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
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace DaleGhent.NINA.PlaneWaveTools {

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

            if ((int)response.StatusCode < 400) {
                success = true;
            }

            response.Dispose();
            client.Dispose();

            return success;
        }
    }
}