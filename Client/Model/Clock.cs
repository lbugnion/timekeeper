using System;
using Timekeeper.DataModel;

namespace Timekeeper.Client.Model
{
    public class Clock
    {
        public const string DefaultBackgroundColor = "#FFFFFF";
        public const string DefaultClockDisplay = "00:00:00";
        public static readonly string DefaultClockId = Guid.Empty.ToString();
        public const string DefaultRunningColor = "#3AFFA9";
        public const string DefaultPayAttentionColor = "#FFFB91";
        public const string DefaultAlmostDoneColor = "#FF6B77";

        public event EventHandler CountdownFinished;

        public bool IsStartDisabled
        {
            get;
            internal set;
        }

        public bool IsStopDisabled
        {
            get;
            internal set;
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

        public StartClockMessage Message
        {
            get;
            private set;
        }

        public Clock(StartClockMessage message)
            : base()
        {
            if (message.ClockId == DefaultClockId)
            {
                Message.AlmostDone = message.AlmostDone;
                Message.CountDown = message.CountDown;
                Message.Label = message.Label;
                Message.PayAttention = message.PayAttention;
                Message.RunningColor = message.RunningColor;
                Message.ServerTime = message.ServerTime;
            }
            else
            {
                Message = message;
            }
        }

        public Clock()
        {
            IsStopDisabled = true;
            CurrentBackgroundColor = DefaultBackgroundColor;

            Message = new StartClockMessage
            {
                ClockId = DefaultClockId, // Default ID
                AlmostDone = TimeSpan.FromSeconds(30),
                PayAttention = TimeSpan.FromMinutes(2),
                CountDown = TimeSpan.FromMinutes(5),
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
            CurrentBackgroundColor = DefaultBackgroundColor;
            Message.ServerTime = DateTime.Now;
        }

        public void ResetDisplay()
        {
            ClockDisplay = Message.CountDown.ToString("c");
        }

        public void RaiseCountdownFinished()
        {
            CountdownFinished?.Invoke(this, EventArgs.Empty);
        }
    }
}
