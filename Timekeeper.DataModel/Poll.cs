using Markdig;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Timekeeper.DataModel
{
    public class Poll
    {
        public event EventHandler OnEdit;

        private bool _isEdited;

        public string ExplanationMarkdown { get; set; }

        public string ExplanationHtml { get; set; }

        public bool MustSave { get; set; }

        public string Uid { get; set; }

        [Required(ErrorMessage = "You must enter a question")]
        public string QuestionMarkdown { get; set; }

        public IList<Answer> Answers { get; set; } = new List<Answer>();

        public bool IsPublished { get; set; }

        public bool IsVotingOpen { get; set; }

        public int TotalAnswers => Answers.Select(a => a.Count).Sum();

        public bool IsEdited
        {
            get => _isEdited;

            set
            {
                _isEdited = value;

                if (_isEdited)
                {
                    OnEdit?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    if (string.IsNullOrEmpty(QuestionMarkdown))
                    {
                        QuestionHtml = string.Empty;
                    }
                    else
                    {
                        QuestionHtml = Markdown.ToHtml(QuestionMarkdown);
                    }

                    if (string.IsNullOrEmpty(ExplanationMarkdown))
                    {
                        ExplanationHtml = string.Empty;
                    }
                    else
                    {
                        ExplanationHtml = Markdown.ToHtml(ExplanationMarkdown);
                    }

                    foreach (var answer in Answers)
                    {
                        if (string.IsNullOrEmpty(answer.TitleMarkdown))
                        {
                            answer.TitleHtml = string.Empty;
                        }
                        else
                        {
                            answer.TitleHtml = Markdown.ToHtml($"{answer.Letter}: {answer.TitleMarkdown}");
                        }
                    }
                }
            }
        }

        public void Reset()
        {
            foreach (var answer in Answers)
            {
                answer.Reset();
            }
        }

        public string QuestionHtml { get; set; }

        public void SplitAnswers(string allAnswers)
        {
            Answers = allAnswers
                .Replace("\r\n", "\n")
                .Split('\n')
                .Where(s => !string.IsNullOrEmpty(s))
                .Select(s => new Answer
                {
                    TitleMarkdown = s
                })
                .ToList();

            for (var index = 0; index < Answers.Count; index++)
            {
                Answers[index].Letter = ((char)('A' + index)).ToString();
            }
        }

        [JsonIgnore]
        [Required(ErrorMessage = "You must enter at least one answer")]
        public string AllAnswers
        {
            get => string.Join("\n", Answers.Select(a => a.TitleMarkdown));
            set => SplitAnswers(value);
        }

        public bool IsBroadcasting { get; set; }

        public void Update(Poll poll)
        {
            Answers.Clear();

            foreach (var answer in poll.Answers)
            {
                Answers.Add(answer);
            }

            IsPublished = poll.IsPublished;
            IsVotingOpen = poll.IsVotingOpen;
            QuestionMarkdown = poll.QuestionMarkdown;
            ExplanationMarkdown = poll.ExplanationMarkdown;
        }

        public bool IsAnswered
        {
            get;
            set;
        }

        public string GivenAnswer
        {
            get;
            set;
        }
    }
}