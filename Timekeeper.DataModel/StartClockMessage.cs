using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace Timekeeper.DataModel
{
    public class StartClockMessage
    {
        private string _almostDoneColor;
        private string _payAttentionColor;
        private string _runningColor;

        public TimeSpan AlmostDone
        {
            get;
            set;
        }

        [Required]
        public string AlmostDoneColor
        {
            get => _almostDoneColor;
            set
            {
                if (value == null)
                {
                    _runningColor = value;
                    return;
                }

                _almostDoneColor = value;
            }
        }

        [JsonIgnore]
        [Range(0, 23, ErrorMessage = "Please select a value between 0 and 23 hours")]
        public int AlmostDoneHours
        {
            get
            {
                return AlmostDone.Hours;
            }
            set
            {
                if (value != AlmostDone.Hours
                    && value >= 0
                    && value < 24)
                {
                    AlmostDone = new TimeSpan(value, AlmostDone.Minutes, AlmostDone.Seconds);
                }
            }
        }

        [JsonIgnore]
        [Range(0, 59, ErrorMessage = "Please select a value between 0 and 59 minutes")]
        public int AlmostDoneMinutes
        {
            get
            {
                return AlmostDone.Minutes;
            }
            set
            {
                if (value != AlmostDone.Minutes
                    && value >= 0
                    && value < 60)
                {
                    AlmostDone = new TimeSpan(AlmostDone.Hours, value, AlmostDone.Seconds);
                }
            }
        }

        [JsonIgnore]
        [Range(0, 59, ErrorMessage = "Please select a value between 0 and 59 seconds")]
        public int AlmostDoneSeconds
        {
            get
            {
                return AlmostDone.Seconds;
            }
            set
            {
                if (value != AlmostDone.Seconds
                    && value >= 0
                    && value < 60)
                {
                    AlmostDone = new TimeSpan(AlmostDone.Hours, AlmostDone.Minutes, value);
                }
            }
        }

        public string ClockId
        {
            get;
            set;
        }

        public TimeSpan CountDown
        {
            get;
            set;
        }

        [JsonIgnore]
        [Range(0, 23, ErrorMessage = "Please select a value between 0 and 23 hours")]
        public int CountDownHours
        {
            get
            {
                return CountDown.Hours;
            }
            set
            {
                if (value != CountDown.Hours
                    && value >= 0
                    && value < 24)
                {
                    CountDown = new TimeSpan(value, CountDown.Minutes, CountDown.Seconds);
                }
            }
        }

        [JsonIgnore]
        [Range(0, 59, ErrorMessage = "Please select a value between 0 and 59 minutes")]
        public int CountDownMinutes
        {
            get
            {
                return CountDown.Minutes;
            }
            set
            {
                if (value != CountDown.Minutes
                    && value >= 0
                    && value < 60)
                {
                    CountDown = new TimeSpan(CountDown.Hours, value, CountDown.Seconds);
                }
            }
        }

        [JsonIgnore]
        [Range(0, 59, ErrorMessage = "Please select a value between 0 and 59 seconds")]
        public int CountDownSeconds
        {
            get
            {
                return CountDown.Seconds;
            }
            set
            {
                if (value != CountDown.Seconds
                    && value >= 0
                    && value < 60)
                {
                    CountDown = new TimeSpan(CountDown.Hours, CountDown.Minutes, value);
                }
            }
        }

        public string Label
        {
            get;
            set;
        }

        public TimeSpan Nudge
        {
            get;
            set;
        }

        public string OvertimeLabel
        {
            get;
            set;
        }

        public TimeSpan PayAttention
        {
            get;
            set;
        }

        [Required]
        public string PayAttentionColor
        {
            get => _payAttentionColor;
            set
            {
                if (value == null)
                {
                    _runningColor = value;
                    return;
                }

                _payAttentionColor = value;
            }
        }

        [JsonIgnore]
        [Range(0, 23, ErrorMessage = "Please select a value between 0 and 23 hours")]
        public int PayAttentionHours
        {
            get
            {
                return PayAttention.Hours;
            }
            set
            {
                if (value != PayAttention.Hours
                    && value >= 0
                    && value < 24)
                {
                    PayAttention = new TimeSpan(value, PayAttention.Minutes, PayAttention.Seconds);
                }
            }
        }

        [JsonIgnore]
        [Range(0, 59, ErrorMessage = "Please select a value between 0 and 59 minutes")]
        public int PayAttentionMinutes
        {
            get
            {
                return PayAttention.Minutes;
            }
            set
            {
                if (value != PayAttention.Minutes
                    && value >= 0
                    && value < 60)
                {
                    PayAttention = new TimeSpan(PayAttention.Hours, value, PayAttention.Seconds);
                }
            }
        }

        [JsonIgnore]
        [Range(0, 59, ErrorMessage = "Please select a value between 0 and 59 seconds")]
        public int PayAttentionSeconds
        {
            get
            {
                return PayAttention.Seconds;
            }
            set
            {
                if (value != PayAttention.Seconds
                    && value >= 0
                    && value < 60)
                {
                    PayAttention = new TimeSpan(PayAttention.Hours, PayAttention.Minutes, value);
                }
            }
        }

        public int Position
        {
            get;
            set;
        }

        [Required]
        public string RunningColor
        {
            get => _runningColor;
            set
            {
                if (value == null)
                {
                    _runningColor = value;
                    return;
                }

                _runningColor = value;
            }
        }

        public string SenderId
        {
            get;
            set;
        }

        public DateTime ServerTime
        {
            get;
            set;
        }

        public string SessionName { get; set; }

        [JsonIgnore]
        public bool WasDeleted
        {
            get;
            set;
        }

        public StartClockMessage()
        {
            OvertimeLabel = Clock.DefaultOvertimeLabel;
        }

        public override string ToString()
        {
            return ServerTime.ToString();
        }
    }
}