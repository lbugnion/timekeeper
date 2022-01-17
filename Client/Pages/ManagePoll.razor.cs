using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Linq;
using System.Threading.Tasks;
using Timekeeper.Client.Model;
using Timekeeper.DataModel;

namespace Timekeeper.Client.Pages
{
    public partial class ManagePoll : IDisposable
    {
        private Poll _currentPoll;

        [Parameter]
        public string Role { get; set; }

        [Parameter]
        public ManagePollsView Parent
        {
            get;
            set;
        }

        public MarkupString TitleMarkup { get; set; }

        public string PollClass => CurrentPoll.IsPublished ? (CurrentPoll.IsVotingOpen ? "poll-published" : "poll-closed") : "poll-unpublished";

        public EditContext CurrentEditContext
        {
            get;
            set;
        }

        public MarkupString GetMarkup(string html)
        {
            return new MarkupString(html);
        }

        [Parameter]
        public Poll CurrentPoll
        {
            get => _currentPoll;
            set
            {
                CurrentEditContext = null;
                _currentPoll = value;

                if (_currentPoll != null)
                {
                    CurrentEditContext = new EditContext(_currentPoll);
                }
            }
        }

        public string CorrectAnswer
        {
            get
            {
                if (CurrentPoll == null)
                {
                    return string.Empty;
                }

                var correctAnswer = CurrentPoll.Answers
                    .FirstOrDefault(a => a.IsCorrect);

                if (correctAnswer == null)
                {
                    return "None";
                }

                return correctAnswer.Letter;
            }

            set
            {
                if (CurrentPoll == null)
                {
                    return;
                }

                foreach (var answer in CurrentPoll.Answers)
                {
                    answer.IsCorrect = false;
                }

                if (value == "None")
                {
                    return;
                }

                var correctAnswer = CurrentPoll.Answers
                    .FirstOrDefault(a => a.Letter == value);

                if (correctAnswer == null)
                {
                    return;
                }

                correctAnswer.IsCorrect = true;
            }
        }

        public async Task SaveCurrentPoll()
        {
            if (CurrentPoll.IsEdited)
            {
                if (CurrentEditContext != null)
                {
                    var isValid = CurrentEditContext.Validate();
                    if (isValid)
                    {
                        CurrentPoll.MustSave = true;
                        await Parent.ToggleEditPoll(CurrentPoll);
                    }
                }
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            Log.LogTrace("-> OnAfterRenderAsync");

            if (CurrentPoll != null)
            {
                CurrentPoll.OnEdit += CurrentPollOnEdit;
            }

            await JSRuntime.InvokeVoidAsync("branding.setTitle", $"{Branding.WindowTitle} : Polls");
            Log.LogTrace("OnAfterRenderAsync ->");
        }

        private async void CurrentPollOnEdit(object sender, EventArgs e)
        {
            Log.LogTrace("-> CurrentPollOnEdit");
            await JSRuntime.InvokeVoidAsync("host.observeFocusAndSelect", "question");
            Log.LogTrace("CurrentPollOnEdit ->");
        }

        public void Dispose()
        {
            if (CurrentPoll != null)
            {
                CurrentPoll.OnEdit -= CurrentPollOnEdit;
            }
        }
    }
}
