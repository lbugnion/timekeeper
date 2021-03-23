using System;
using Timekeeper.DataModel;

namespace Timekeeper.Client.Model
{
    public class Clock
    {
        public event EventHandler CountdownFinished;
        public event EventHandler<bool> SelectionChanged;

        public const string DefaultAlmostDoneColor = "#FF6B77";
        public const string DefaultBackgroundColor = "#EEEEEE";
        public const string DefaultClockDisplay = "00:00:00";
        public const string DefaultPayAttentionColor = "#FFFB91";
        public const string DefaultRunningColor = "#3AFFA9";
        public static readonly TimeSpan DefaultAlmostDone = TimeSpan.FromSeconds(30);
        public static readonly TimeSpan DefaultCountDown = TimeSpan.FromMinutes(5);
        public static readonly TimeSpan DefaultPayAttention = TimeSpan.FromMinutes(2);

        public string ClockDisplay
        {
            get;
            set;
        }

        public string CurrentBackgroundColor
        {
            get;
            set;
        }

        public bool IsClockRunning
        {
            get;
            set;
        }

        public bool IsSelected
        {
            get;
            set;
        }

        public void ToggleSelect()
        {
            IsSelected = !IsSelected;
            SelectionChanged?.Invoke(this, IsSelected);
        }

        public bool IsConfigDisabled
        {
            get;
            internal set;
        }

        public bool IsDeleteDisabled
        {
            get;
            internal set;
        }

        public bool IsNudgeDisabled
        {
            get;
            internal set;
        }

        public StartClockMessage Message
        {
            get;
            private set;
        }

        public TimeSpan Remains
        {
            get
            {
                var elapsed = DateTime.Now - Message.ServerTime;
                var remains = Message.CountDown - elapsed;
                return remains;
            }
        }

        public Clock(StartClockMessage message)
            : base()
        {
            Message = message;
        }

        public Clock()
        {
            CurrentBackgroundColor = DefaultBackgroundColor;

            Message = new StartClockMessage
            {
                ClockId = Guid.NewGuid().ToString(),
                AlmostDone = DefaultAlmostDone,
                PayAttention = DefaultPayAttention,
                CountDown = DefaultCountDown,
                AlmostDoneColor = DefaultAlmostDoneColor,
                PayAttentionColor = DefaultPayAttentionColor,
                RunningColor = DefaultRunningColor,
                Label = "New clock"
            };

            ResetDisplay();
        }

        public void RaiseCountdownFinished()
        {
            CountdownFinished?.Invoke(this, EventArgs.Empty);
        }

        public void Reset()
        {
            ResetDisplay();
            Message.ServerTime = DateTime.Now;
        }

        public void ResetDisplay()
        {
            ClockDisplay = Message.CountDown.ToString("c");
            CurrentBackgroundColor = DefaultBackgroundColor;
        }

        public void Restore(Clock clockInSavedSession)
        {
            Message = clockInSavedSession.Message;
        }
    }
}