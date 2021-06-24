using System;

namespace Timekeeper.DataModel
{
    public class Clock
    {
        public event EventHandler<bool> SelectionChanged;

        public const string DefaultAlmostDoneColor = "#FF6B77";
        public const string DefaultBackgroundColor = "#EEEEEE";
        public const string DefaultForegroundColor = "#000000";
        public const string OvertimeForegroundColor = "#FF0000";
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

        public string CurrentForegroundColor
        {
            get;
            set;
        }

        public string CurrentBackgroundColor
        {
            get;
            set;
        }

        public string CurrentLabel
        {
            get;
            set;
        }

        public bool IsClockRunning
        {
            get;
            set;
        }

        public bool IsConfigDisabled
        {
            get;
            set;
        }

        public bool IsNudgeDisabled
        {
            get;
            set;
        }

        public bool IsPlayStopDisabled
        {
            get;
            set;
        }

        public bool IsSelected
        {
            get;
            set;
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
                var remains = Message.CountDown + Message.Nudge - elapsed;
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

        public void Reset()
        {
            ResetDisplay();
            Message.ServerTime = DateTime.Now;
        }

        public void ResetDisplay()
        {
            ClockDisplay = (Message.CountDown + Message.Nudge).ToString("c");
            CurrentBackgroundColor = DefaultBackgroundColor;
            CurrentForegroundColor = DefaultForegroundColor;
            CurrentLabel = Message.Label;
        }

        public void ToggleSelect()
        {
            IsSelected = !IsSelected;
            SelectionChanged?.Invoke(this, IsSelected);
        }

        public void Update(
            StartClockMessage model,
            bool copyIdToo)
        {
            Message.AlmostDone = model.AlmostDone;
            Message.AlmostDoneColor = model.AlmostDoneColor;
            Message.CountDown = model.CountDown;
            Message.Nudge = model.Nudge;
            Message.Label = model.Label;
            Message.PayAttention = model.PayAttention;
            Message.PayAttentionColor = model.PayAttentionColor;
            Message.Position = model.Position;
            Message.RunningColor = model.RunningColor;
            Message.ServerTime = model.ServerTime;
            Message.Position = model.Position;

            if (copyIdToo)
            {
                Message.ClockId = model.ClockId;
                ResetDisplay();
            }
        }
    }
}