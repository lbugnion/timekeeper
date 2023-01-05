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

        public EditContext CurrentEditContext
        {
            get;
            set;
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

        [Parameter]
        public ManagePollsView Parent
        {
            get;
            set;
        }

        public string PollClass => CurrentPoll.IsPublished ? (CurrentPoll.IsVotingOpen ? "poll-published" : "poll-closed") : "poll-unpublished";

        public MarkupString TitleMarkup { get; set; }

        private async void CurrentPollOnEdit(object sender, EventArgs e)
        {
            Log.LogTrace("-> CurrentPollOnEdit");
            await JSRuntime.InvokeVoidAsync("host.observeFocusAndSelect", "question");
            Log.LogTrace("CurrentPollOnEdit ->");
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (CurrentPoll != null)
            {
                CurrentPoll.OnEdit += CurrentPollOnEdit;
            }

            if (firstRender)
            {
                await JSRuntime.InvokeVoidAsync("branding.setTitle", $"{Branding.WindowTitle} : Polls");
            }
        }

        public void Dispose()
        {
            if (CurrentPoll != null)
            {
                CurrentPoll.OnEdit -= CurrentPollOnEdit;
            }
        }

        public MarkupString GetMarkup(string html)
        {
            return new MarkupString(html);
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
    }
}