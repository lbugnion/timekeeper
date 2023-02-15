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
        private string _explanationMarkdown;
        private string _questionMarkdown;

        [JsonIgnore]
        [Required(ErrorMessage = "You must enter at least one answer")]
        public string AllAnswers
        {
            get => string.Join("\n", Answers.Select(a => a.TitleMarkdown));
            set => SplitAnswers(value);
        }

        public IList<string> AlreadyVotedIds { get; set; } = new List<string>();

        public IList<Answer> Answers { get; set; } = new List<Answer>();

        public string ExplanationHtml { get; set; }

        public string ExplanationMarkdown
        {
            get => _explanationMarkdown;
            set
            {
                _explanationMarkdown = value;

                if (string.IsNullOrEmpty(value))
                {
                    ExplanationHtml = string.Empty;
                }
                else
                {
                    ExplanationHtml = Markdown.ToHtml(_explanationMarkdown);
                }
            }
        }

        public string GivenAnswer
        {
            get;
            set;
        }

        public bool IsAnswered
        {
            get;
            set;
        }

        public bool IsBroadcasting { get; set; }

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
                        // TODO Do we need to pass the HTML in the JSON, can we not
                        // just generate the HTML whenever ExplanationMarkdown is set?
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

        public bool IsPublished { get; set; }

        public bool IsVotingOpen { get; set; }

        public bool MustSave { get; set; }

        public string QuestionHtml { get; set; }

        [Required(ErrorMessage = "You must enter a question")]
        public string QuestionMarkdown
        {
            get => _questionMarkdown;
            
            set
            {
                _questionMarkdown = value;

                if (string.IsNullOrEmpty(_questionMarkdown))
                {
                    QuestionHtml = string.Empty;
                }
                else
                {
                    QuestionHtml = Markdown.ToHtml(value);
                }
            }
        }

        public string SessionName { get; set; }

        public int TotalAnswers => Answers.Select(a => a.Count).Sum();

        public string Uid { get; set; }

        public string VoterId { get; set; }

        public Poll GetSafeCopy()
        {
            var copy = new Poll
            {
                ExplanationMarkdown = ExplanationMarkdown,
                ExplanationHtml = ExplanationHtml,
                QuestionHtml = QuestionHtml,
                QuestionMarkdown = QuestionMarkdown,
                SessionName = SessionName,
                Uid = Uid,
                IsVotingOpen = IsVotingOpen,
                IsPublished = IsPublished
            };

            foreach (var answer in Answers)
            {
                copy.Answers.Add(new Answer
                {
                    Letter = answer.Letter,
                    Count = answer.Count,
                    Ratio = answer.Ratio,
                    TitleHtml = answer.TitleHtml,
                    TitleMarkdown = answer.TitleMarkdown,
                });
            }

            return copy;
        }

        public void Reset()
        {
            foreach (var answer in Answers)
            {
                answer.Reset();
            }

            GivenAnswer = null;
            AlreadyVotedIds = new List<string>();
        }

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
    }
}