using Blazored.LocalStorage;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Timekeeper.DataModel;

namespace Timekeeper.Client.Model.Polls
{
    public class PollGuest : SignalRGuestBase
    {
        protected override string SessionKey => "PollGuestSession";

        public string Role { get; set; }

        private async Task ReceiveVote(string pollJson)
        {
            if (Role != Constants.RolePresenter)
            {
                return; // We will display the votes when the poll is closed
            }

            Poll receivedPoll;

            try
            {
                receivedPoll = JsonConvert.DeserializeObject<Poll>(pollJson);
            }
            catch
            {
                _log.LogTrace("Error with received poll");
                return;
            }

            var poll = CurrentSession.Polls.FirstOrDefault(p => p.Uid == receivedPoll.Uid);

            if (poll == null)
            {
                _log.LogDebug($"Poll doesn't exist: {receivedPoll.Uid}");
                return;
            }

            if (!poll.IsVotingOpen)
            {
                _log.LogDebug($"Voting is closed for poll {receivedPoll.Uid}");
                return;
            }

            var chosenAnswer = poll.Answers
                .FirstOrDefault(a => a.Letter == receivedPoll.GivenAnswer);

            if (chosenAnswer == null)
            {
                _log.LogDebug($"No such question {receivedPoll.Uid} / receivedPoll.GivenAnswer");
                return;
            }

            chosenAnswer.Count++;

            double totalCount = 0;

            foreach (var answer in poll.Answers)
            {
                totalCount += answer.Count;
            }

            foreach (var answer in poll.Answers)
            {
                answer.Ratio = (answer.Count / totalCount);
            }

            poll.GivenAnswer = null;
            await SaveSessionToStorage();
            RaiseUpdateEvent();
        }

        public PollGuest(
            IConfiguration config,
            ILocalStorageService localStorage,
            ILogger log,
            HttpClient http,
            string sessionId,
            SessionHandler session) : base(config, localStorage, log, http, sessionId, session)
        {
            _log.LogInformation("-> PollGuest()");
        }

        public override async Task Connect()
        {
            _log.LogInformation("-> PollGuest.Connect");

            IsBusy = true;
            IsInError = false;

            var ok = await InitializeSession(_sessionId)
                && await InitializePeerInfo()
                && await UnregisterFromPreviousGroup(_unregisterFromGroup)
                && await CreateConnection();

            if (ok)
            {
                _connection.On<string>(Constants.PublishPollMessage, async p => await ReceivePublishUnpublishPoll(p, true));
                _connection.On<string>(Constants.UnpublishPollMessage, async p => await ReceivePublishUnpublishPoll(p, false));
                _connection.On<string>(Constants.ReceivePollsMessage, ReceiveAllPublishedPolls);
                _connection.On<string>(Constants.VotePollMessage, async p => await ReceiveVote(p));
                _connection.On<string>(Constants.ResetPollMessage, async p => await ResetPoll(p));

                ok = await StartConnection();

                if (ok)
                {
                    // Ask for existing polls

                    _log.LogTrace("Asking for existing polls");

                    var pollsUrl = $"{_hostNameFree}/polls";
                    var httpRequest = new HttpRequestMessage(HttpMethod.Get, pollsUrl);
                    httpRequest.Headers.Add(Constants.GroupIdHeaderKey, CurrentSession.SessionId);

                    var response = await _http.SendAsync(httpRequest);

                    if (!response.IsSuccessStatusCode)
                    {
                        IsConnected = false;
                        ErrorStatus = "Error";
                        IsInError = true;
                        _log.LogError($"Error when asking for existing polls: {response.ReasonPhrase}");
                    }
                    else
                    {
                        IsConnected = true;
                        Status = "Ready";
                        _log.LogTrace("Done asking for existing polls");
                    }
                }
                else
                {
                    IsConnected = false;
                    ErrorStatus = "Error";
                    IsInError = true;
                    _log.LogError($"Error when starting connection");
                }
            }
            else
            {
                IsConnected = false;
                ErrorStatus = "Error";
                IsInError = true;
                _log.LogError($"Error when connecting");
            }

            IsBusy = false;
            RaiseUpdateEvent();
            _log.LogInformation("SignalRGuest.Connect ->");
        }

        private async Task ResetPoll(string pollJson)
        {
            Poll receivedPoll;

            try
            {
                receivedPoll = JsonConvert.DeserializeObject<Poll>(pollJson);
            }
            catch
            {
                _log.LogTrace("Error with received poll");
                return;
            }

            var poll = CurrentSession.Polls.FirstOrDefault(p => p.Uid == receivedPoll.Uid);

            if (poll == null)
            {
                _log.LogDebug($"Poll doesn't exist: {receivedPoll.Uid}");
                return;
            }

            poll.GivenAnswer = null;
            await SaveSessionToStorage();
            RaiseUpdateEvent();
        }

        private async Task ReceiveAllPublishedPolls(string json)
        {
            _log.LogTrace("-> ReceiveAllPublishedPolls");
            _log.LogDebug($"JSON: {json}");

            try
            {
                var list = JsonConvert.DeserializeObject<ListOfPolls>(json);

                foreach (var poll in list.Polls)
                {
                    await ReceivePublishUnpublishPoll(poll, poll.IsPublished);
                }
            }
            catch
            {
                ErrorStatus = "Error receiving polls";
                IsInError = true;
            }

            RaiseUpdateEvent();
        }

        private async Task ReceivePublishUnpublishPoll(Poll poll, bool mustPublish)
        {
            _log.LogTrace("-> ReceivePublishUnpublishPoll");
            _log.LogDebug($"Received poll: {poll.Uid}");
            _log.LogDebug($"Must publish: {mustPublish}");

            poll.IsPublished = mustPublish;

            if (!string.IsNullOrEmpty(poll.SessionName))
            {
                CurrentSession.SessionName = poll.SessionName;
            }

            var existingPoll = CurrentSession.Polls
                .FirstOrDefault(p => p.Uid == poll.Uid);

            if (existingPoll != null)
            {
                _log.LogDebug($"Found poll: {existingPoll.Uid}");
                _log.LogTrace("Updating existing poll");
                existingPoll.Update(poll);

                if (!mustPublish)
                {
                    _log.LogTrace("Removing existing poll");
                    CurrentSession.Polls.Remove(existingPoll);
                    await SaveSessionToStorage();
                }

                RaiseUpdateEvent();
                return;
            }

            if (mustPublish)
            {
                _log.LogTrace("Add new poll");
                CurrentSession.Polls.Insert(0, poll);
                RaiseUpdateEvent();
            }
        }

        private async Task ReceivePublishUnpublishPoll(string pollJson, bool mustPublish)
        {
            _log.LogTrace("-> ReceivePublishUnpublishPoll");

            Poll receivedPoll;

            try
            {
                receivedPoll = JsonConvert.DeserializeObject<Poll>(pollJson);
            }
            catch
            {
                _log.LogError($"Cannot deserialize Poll: {pollJson}");
                return;
            }

            await ReceivePublishUnpublishPoll(receivedPoll, mustPublish);
        }

        public async Task SelectAnswer(string pollId, string answerLetter)
        {
            _log.LogTrace("HIGHLIGHT---> SelectAnswer");

            var poll = CurrentSession.Polls.FirstOrDefault(p => p.Uid == pollId);

            if (poll == null)
            {
                _log.LogWarning($"Poll not found: {pollId}");
                return;
            }

            poll.GivenAnswer = answerLetter;
            poll.IsAnswered = true;

            var voteUrl = $"{_hostNameFree}/vote-poll";
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, voteUrl);
            httpRequest.Headers.Add(Constants.GroupIdHeaderKey, CurrentSession.SessionId);

            var json = JsonConvert.SerializeObject(poll);
            httpRequest.Content = new StringContent(json);

            var response = await _http.SendAsync(httpRequest);

            if (response.IsSuccessStatusCode)
            {
                _log.LogTrace("Saving to local storage");
                await SaveSessionToStorage();
            }
            else
            {
                poll.IsAnswered = false;
                poll.GivenAnswer = null;
                ErrorStatus = "Error when voting, try again";
                _log.LogError($"Error when voting: {pollId} / {answerLetter}");
            }

            RaiseUpdateEvent();
            _log.LogTrace("HIGHLIGHT--SelectAnswer ->");
        }

        public async Task<bool> InitializeSession(string sessionId)
        {
            _log.LogInformation("HIGHLIGHT---> InitializeSession");
            _log.LogDebug($"sessionId: {sessionId}");

            var guestSession = await _session.GetFromStorage(SessionKey, _log);

            if (guestSession != null)
            {
                _unregisterFromGroup = guestSession.SessionId;
            }

            if (guestSession == null
                || guestSession.SessionId != sessionId)
            {
                CurrentSession = new SessionBase
                {
                    SessionId = sessionId,
                    SessionName = Branding.PollsPageTitle
                };

                await _session.SaveToStorage(CurrentSession, SessionKey, _log);
            }
            else
            {
                CurrentSession = guestSession;
            }

            _log.LogTrace("Session saved to storage");
            _log.LogInformation("InitializeSession ->");
            return true;
        }

        public IList<Poll> PublishedPolls
        {
            get;
            private set;
        }

        public async Task SaveSessionToStorage()
        {
            await _session.SaveToStorage(CurrentSession, SessionKey, _log);
        }
    }
}
