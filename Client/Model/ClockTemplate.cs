using System;
using System.Collections.Generic;

namespace Timekeeper.Client.Model
{
    public class ClockTemplate
    {
        /// <summary>
        /// Session name
        /// </summary>
        public string SN
        {
            get;
            set;
        }

        /// <summary>
        /// Session ID
        /// </summary>
        public string SessionId
        {
            get;
            set;
        }

        /// <summary>
        /// Clocks
        /// </summary>
        public IList<ClockForTemplate> CK
        {
            get;
            set;
        }
    }

    public class ClockForTemplate
    {
        /// <summary>
        /// Label
        /// </summary>
        public string L
        {
            get;
            set;
        }

        /// <summary>
        /// CountDown
        /// </summary>
        public ClockDefinition C
        {
            get;
            set;
        }

        /// <summary>
        /// Pay attention
        /// </summary>
        public ClockDefinition P
        {
            get;
            set;
        }

        /// <summary>
        /// Almost done
        /// </summary>
        public ClockDefinition A
        {
            get;
            set;
        }

        /// <summary>
        /// Default clock
        /// </summary>
        public bool D
        {
            get;
            set;
        }
    }

    public class ClockDefinition
    {
        /// <summary>
        /// Time
        /// </summary>
        public TimeSpan T
        {
            get;
            set;
        }

        /// <summary>
        /// Color
        /// </summary>
        public string C
        {
            get;
            set;
        }
    }
}
