﻿using HunterPie.Integrations.Poogie.Common;
using HunterPie.Integrations.Poogie.Common.Models;
using HunterPie.Integrations.Poogie.Settings.Models;
using System.Threading.Tasks;

namespace HunterPie.Integrations.Poogie.Settings;
internal class PoogieClientSettingsConnector
{
    private readonly IPoogieClient _client;

    public PoogieClientSettingsConnector(IPoogieClient client)
    {
        _client = client;
    }

    private const string CLIENT_SETTINGS_ENDPOINT = "/v1/account/client/settings";

    public async Task<PoogieResult<ClientSettingsResponse>> UploadClientSettings(ClientSettingsRequest request) =>
        await _client.Patch<ClientSettingsRequest, ClientSettingsResponse>(CLIENT_SETTINGS_ENDPOINT, request);

    public async Task<PoogieResult<ClientSettingsResponse>> GetClientSettings() =>
        await _client.Get<ClientSettingsResponse>(CLIENT_SETTINGS_ENDPOINT);
}