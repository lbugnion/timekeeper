using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Timekeeper.Client.Model.Polls;
using Timekeeper.DataModel;

namespace Timekeeper.Client.Pages
{
    public partial class ManagePollsView : IDisposable
    {
        private PollHost _handler;

        [Parameter]
        public ManagePolls Parent { get; set; }

        [Parameter]
        public PollHost Handler
        {
            get => _handler;
            set
            {
                if (value == null)
                {
                    _handler.UpdateUi -= HandlerUpdateUi;
                }

                _handler = value;

                if (_handler != null)
                {
                    _handler.UpdateUi += HandlerUpdateUi;
                }
            }
        }

        private void HandlerUpdateUi(object sender, EventArgs e)
        {
            StateHasChanged();
        }

        [Parameter]
        public string Role { get; set; }

        public string SessionName
        {
            get
            {
                if (Handler.CurrentSession == null)
                {
                    return "No session";
                }

                return Handler.CurrentSession.SessionName;
            }
        }

        public bool IsAnyPollEdited
        {
            get;
            private set;
        }

        public bool IsAnyPollPublished
        {
            get => Handler.IsAnyPollPublished;
        }

        public async Task DeletePoll(Poll poll)
        {
            if (poll.IsPublished)
            {
                return;
            }

            if (Handler.CurrentSession.Polls.Contains(poll))
            {
                Handler.CurrentSession.Polls.Remove(poll);
                //await Handler.SaveSession();
                StateHasChanged();
            }
        }

        public async Task ToggleEditPoll(Poll poll)
        {
            Log.LogTrace(nameof(ToggleEditPoll));

            if (poll.IsPublished
                || poll.IsVotingOpen)
            {
                return;
            }

            poll.IsEdited = !poll.IsEdited;
            var isPollInCollection = false;

            foreach (var existingPoll in Handler.CurrentSession.Polls)
            {
                if (existingPoll.Uid == poll.Uid)
                {
                    isPollInCollection = true;
                }
                else
                {
                    existingPoll.IsEdited = false;
                }
            }

            if (!isPollInCollection
                && poll.MustSave)
            {
                Handler.CurrentSession.Polls.Add(poll);
                CurrentPoll = null;
            }

            if (poll.MustSave)
            {
                poll.MustSave = false;
                await Handler.SaveSession();
            }

            IsAnyPollEdited = Handler.CurrentSession.Polls.Any(p => p.IsEdited)
                || (CurrentPoll != null
                    && CurrentPoll.IsEdited);

            StateHasChanged();
        }

        public Poll CurrentPoll
        {
            get;
            set;
        }

        public async Task CreateNewPoll()
        {
            if (IsAnyPollEdited)
            {
                return;
            }

            if (Handler.CurrentSession == null)
            {
                return;
            }

            foreach (var poll in Handler.CurrentSession.Polls)
            {
                poll.IsEdited = false;
            }

            CurrentPoll = new Poll
            {
                Uid = Guid.NewGuid().ToString(),
            };

            await ToggleEditPoll(CurrentPoll);

            IsAnyPollEdited = Handler.CurrentSession.Polls.Any(p => p.IsEdited)
                || (CurrentPoll != null
                    && CurrentPoll.IsEdited);

            StateHasChanged();
        }

        public async Task PublishPoll(Poll poll, bool mustPublish)
        {
            await Handler.PublishUnpublishPoll(poll, mustPublish);
        }

        public async Task OpenClosePoll(Poll poll, bool mustOpen)
        {
            await Handler.OpenClosePoll(poll, mustOpen);
        }

        public void Dispose()
        {
            if (Handler != null)
            {
                Handler.UpdateUi -= HandlerUpdateUi;
            }
        }

        public async Task MovePollUp(string uid)
        {
            await Handler.MovePollUpDown(uid, true);
        }

        public async Task MovePollDown(string uid)
        {
            await Handler.MovePollUpDown(uid, false);
        }
    }
}
