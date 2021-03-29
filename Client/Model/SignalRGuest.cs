using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Timekeeper.DataModel;

namespace Timekeeper.Client.Model
{
    public class SignalRGuest : SignalRHandler
    {
        private string _session;

        public Guest GuestInfo
        {
            get;
            private set;
        }

        protected override string SessionKey => "GuestSession";

        public SignalRGuest(
            IConfiguration config,
            ILocalStorageService localStorage,
            ILogger log,
            HttpClient http,
            string session) : base(config, localStorage, log, http)
        {
            _log.LogInformation("> SignalRGuest()");

            _session = session;
            Guest.SetLocalStorage(localStorage, log);
        }

        private async void ClockCountdownFinished(object sender, EventArgs e)
        {
            _log.LogInformation("-> ClockCountdownFinished");

            var clock = sender as Clock;

            if (clock == null)
            {
                return;
            }

            clock.CountdownFinished -= ClockCountdownFinished;
            await DeleteLocalClock(clock.Message.ClockId);
        }

        private void ReceiveStartClock(string message)
        {
            _log.LogInformation("-> SignalRGuest.ReceiveStartClock");

            _log.LogDebug(message);

            IList<StartClockMessage> clockMessages;

            try
            {
                clockMessages = JsonConvert.DeserializeObject<IList<StartClockMessage>>(message);
            }
            catch
            {
                _log.LogWarning("Not a list of clocks");
                return;
            }

            var clockStarted = 0;
            var newList = new List<Clock>();

            foreach (var clockMessage in clockMessages)
            {
                _log.LogDebug($"clockID: {clockMessage.ClockId}");
                _log.LogDebug($"AlmostDoneColor: {clockMessage.AlmostDoneColor}");
                _log.LogDebug($"PayAttentionColor: {clockMessage.PayAttentionColor}");

                var existingClock = CurrentSession.Clocks
                    .FirstOrDefault(c => c.Message.ClockId == clockMessage.ClockId);

                if (existingClock == null)
                {
                    _log.LogTrace($"No found clock, adding");
                    existingClock = new Clock(clockMessage);
                    clockStarted++;
                }
                else
                {
                    _log.LogDebug($"Found clock {existingClock.Message.Label}, updating");
                    existingClock.Message.Label = clockMessage.Label;
                    existingClock.Message.CountDown = clockMessage.CountDown;
                    existingClock.Message.AlmostDone = clockMessage.AlmostDone;
                    existingClock.Message.PayAttention = clockMessage.PayAttention;
                    existingClock.Message.AlmostDoneColor = clockMessage.AlmostDoneColor;
                    existingClock.Message.PayAttentionColor = clockMessage.PayAttentionColor;
                    existingClock.Message.RunningColor = clockMessage.RunningColor;
                    existingClock.Message.ServerTime = clockMessage.ServerTime;
                    existingClock.Message.Position = clockMessage.Position;
                }

                _log.LogDebug($"Clock {existingClock.Message.Label} remains {existingClock.Remains}");

                if (existingClock.Remains.TotalSeconds > 0)
                {
                    // Clock hasn't expired yet
                    newList.Add(existingClock);
                }
            }

            if (clockStarted > 0)
            {
                Status = $"{clockStarted} clock(s) started";
            }

            CurrentSession.Clocks = newList.OrderBy(c => c.Message.Position).ToList();

            foreach (Clock clock in CurrentSession.Clocks)
            {
                RunClock(clock);
            }

            RaiseUpdateEvent();
            _log.LogInformation("SignalRGuest.ReceiveStartClock ->");
        }

        protected void DisplayReceivedMessage(string message)
        {
            base.DisplayMessage(message, false);
            Status = "Received host message";
        }

        public async Task<bool> AnnounceName()
        {
            _log.LogInformation($"-> {nameof(AnnounceName)}");
            _log.LogDebug($"UserId: {CurrentSession.UserId}");
            _log.LogDebug($"GuestId: {GuestInfo.Message.GuestId}");

            var json = JsonConvert.SerializeObject(GuestInfo.Message);
            _log.LogDebug($"json: {json}");

            var content = new StringContent(json);

            var functionKey = _config.GetValue<string>(AnnounceGuestKeyKey);
            _log.LogDebug($"functionKey: {functionKey}");

            var announceUrl = $"{_hostNameFree}/announce";
            _log.LogDebug($"announceUrl: {announceUrl}");

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, announceUrl);
            httpRequest.Headers.Add(FunctionCodeHeaderKey, functionKey);
            httpRequest.Headers.Add(Constants.GroupIdHeaderKey, CurrentSession.SessionId);
            httpRequest.Content = content;

            var response = await _http.SendAsync(httpRequest);

            if (!response.IsSuccessStatusCode)
            {
                _log.LogError($"Cannot send message: {response.ReasonPhrase}");
                _log.LogInformation($"{nameof(AnnounceName)} ->");
                return false;
            }

            _log.LogInformation($"{nameof(AnnounceName)} ->");
            return true;
        }

        public override async Task Connect(
            string templateName = null)
        {
            _log.LogInformation("-> SignalRGuest.Connect");

            IsBusy = true;

            var ok = await InitializeSession(_session)
                && await InitializeGuestInfo()
                && await CreateConnection();

            if (ok)
            {
                _connection.On<string>(Constants.StartClockMessageName, ReceiveStartClock);
                _connection.On<string>(Constants.HostToGuestMessageName, DisplayReceivedMessage);
                _connection.On(Constants.HostToGuestRequestAnnounceMessageName, AnnounceName);
                _connection.On<string>(Constants.StopClockMessage, s => StopLocalClock(s, false));
                _connection.On<string>(Constants.DeleteClockMessage, DeleteLocalClock);

                ok = await StartConnection();

                if (ok)
                {
                    IsConnected = true;
                    DisplayMessage("Ready", false);

                    _log.LogTrace($"Name is {GuestInfo.Message.DisplayName}");

                    if (!string.IsNullOrEmpty(GuestInfo.Message.CustomName))
                    {
                        _log.LogTrace($"Sending name {GuestInfo.Message.CustomName}");

                        ok = await AnnounceName();

                        if (!ok)
                        {
                            IsConnected = false;
                            DisplayMessage("Error", true);
                        }
                    }
                }
                else
                {
                    IsConnected = false;
                    DisplayMessage("Error", true);
                }
            }
            else
            {
                IsConnected = false;
                DisplayMessage("Error", true);
            }

            IsBusy = false;
            _log.LogInformation("SignalRGuest.Connect ->");
        }

        public async Task<bool> InitializeGuestInfo()
        {
            _log.LogInformation("-> InitializeGuestInfo");

            var message = await Guest.GetFromStorage();

            if (message == null)
            {
                _log.LogTrace("Saved GuestInfo is null");
                _log.LogDebug($"CurrentSession.UserId {CurrentSession.UserId}");
                GuestInfo = new Guest(CurrentSession.UserId);
                await GuestInfo.Save();
            }
            else
            {
                GuestInfo = new Guest(message.GuestId)
                {
                    Message = message
                };
            }

            if (GuestInfo.Message.GuestId != CurrentSession.UserId)
            {
                _log.LogTrace($"Fixing GuestId");
                _log.LogDebug($"GuestInfo.Message.GuestId {GuestInfo.Message.GuestId}");
                _log.LogDebug($"CurrentSession.UserId {CurrentSession.UserId}");
                GuestInfo.Message.GuestId = CurrentSession.UserId;
                await GuestInfo.Save();
            }

            _log.LogDebug($"name: {GuestInfo.Message.DisplayName}");
            _log.LogInformation("InitializeGuestInfo ->");

            return true;
        }

        public async Task<bool> InitializeSession(string sessionId)
        {
            _log.LogInformation("-> InitializeSession");
            _log.LogDebug($"sessionId: {sessionId}");

            var guestSession = await SessionBase.GetFromStorage(SessionKey, _log);

            if (guestSession == null)
            {
                guestSession = new SessionBase();
            }

            guestSession.SessionId = sessionId;
            guestSession.Clocks = new List<Clock>(); // Always reset the clocks

            _log.LogDebug($"UserID {guestSession.UserId}");
            _log.LogDebug($"UserName {guestSession.UserName}");

            CurrentSession = guestSession;
            await CurrentSession.Save(SessionKey, _log);
            _log.LogTrace("Session saved to storage");

            _log.LogInformation("InitializeSession ->");
            return true;
        }
    }
}