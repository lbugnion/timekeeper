﻿using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Timekeeper.Client.Model
{
    public abstract class SignalRHostBase : SignalRHandler
    {
        protected NavigationManager _nav;
        public const string HostSessionKey = "HostSessionKey";

        protected override string PeerKey => "HostPeer";

        protected override string SessionKey => HostSessionKey;

        public bool? IsAuthorized
        {
            get;
            protected set;
        }

        public SignalRHostBase(
            IConfiguration config,
            ILocalStorageService localStorage,
            ILogger log,
            HttpClient http,
            NavigationManager nav,
            SessionHandler session) : base(config, localStorage, log, http, session)
        {
            _nav = nav;
        }

        public async Task CheckAuthorize()
        {
            _log.LogInformation("-> CheckAuthorize");

            var versionUrl = $"{_hostName}/version";
            _log.LogDebug($"versionUrl: {versionUrl}");

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, versionUrl);
            HttpResponseMessage response;

            try
            {
                response = await _http.SendAsync(httpRequest);
            }
            catch (Exception ex)
            {
                _log.LogError($"Connection refused: {ex.Message}");
                IsInError = true;
                IsConnected = false;
                IsBusy = false;
                IsAuthorized = false;
                Status = "Cannot communicate with functions";
                RaiseUpdateEvent();
                return;
            }

            _log.LogDebug($"Response code: {response.StatusCode}");

            switch (response.StatusCode)
            {
                case System.Net.HttpStatusCode.OK:
                    _log.LogTrace("All ok");
                    IsAuthorized = true;
                    Status = "Ready...";
                    RaiseUpdateEvent();
                    break;

                case System.Net.HttpStatusCode.Forbidden:
                    _log.LogTrace("Unauthorized");
                    IsInError = true;
                    IsConnected = false;
                    IsBusy = false;
                    IsAuthorized = false;
                    Status = "Unauthorized";
                    RaiseUpdateEvent();
                    break;

                default:
                    _log.LogTrace("Other error code");
                    IsInError = true;
                    IsConnected = false;
                    IsBusy = false;
                    IsAuthorized = false;
                    Status = "Cannot communicate with functions";
                    _log.LogError($"Cannot communicate with functions: {response.StatusCode}");
                    RaiseUpdateEvent();
                    break;
            }
        }

        public async Task<bool> SaveSession()
        {
            return await _session.Save(CurrentSession, SessionKey, _log);
        }
    }
}