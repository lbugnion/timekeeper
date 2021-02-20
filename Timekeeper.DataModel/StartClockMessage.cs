using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace Timekeeper.DataModel
{
    public class StartClockMessage
    {
        private const string ColorErrorMessage = "Please enter a color in the form XXXXXX where X is between 0 and F";

        private string _almostDoneColor;
        private string _payAttentionColor;
        private string _runningColor;

        public string Label
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

        public TimeSpan CountDown
        {
            get;
            set;
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

        public TimeSpan AlmostDone
        {
            get;
            set;
        }

        [Required]
        [MinLength(4, ErrorMessage = ColorErrorMessage)]
        [MaxLength(7, ErrorMessage = ColorErrorMessage)]
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

                if (!value.StartsWith("#"))
                {
                    value = $"#{value}";
                }

                _almostDoneColor = value;
            }
        }

        public DateTime ServerTime
        {
            get;
            set;
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

        public TimeSpan PayAttention
        {
            get;
            set;
        }

        [Required]
        [MinLength(4, ErrorMessage = ColorErrorMessage)]
        [MaxLength(7, ErrorMessage = ColorErrorMessage)]
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

                if (!value.StartsWith("#"))
                {
                    value = $"#{value}";
                }

                _payAttentionColor = value;
            }
        }

        [Required]
        [MinLength(4, ErrorMessage = ColorErrorMessage)]
        [MaxLength(7, ErrorMessage = ColorErrorMessage)]
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

                if (!value.StartsWith("#"))
                {
                    value = $"#{value}";
                }

                _runningColor = value;
            }
        }

        public override string ToString()
        {
            return ServerTime.ToString();
        }

        public string ClockId
        {
            get;
            set;
        }
    }
}