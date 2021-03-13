using System;
using System.Collections.Generic;

namespace Timekeeper.Client.Model
{
    public class ClockDefinition
    {
        /// <summary>
        /// Color
        /// </summary>
        public string C
        {
            get;
            set;
        }

        /// <summary>
        /// Time
        /// </summary>
        public TimeSpan T
        {
            get;
            set;
        }
    }

    public class ClockForTemplate
    {
        /// <summary>
        /// Almost done
        /// </summary>
        public ClockDefinition A
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
        /// Default clock
        /// </summary>
        public bool D
        {
            get;
            set;
        }

        /// <summary>
        /// Label
        /// </summary>
        public string L
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
    }

    public class ClockTemplate
    {
        /// <summary>
        /// Clocks
        /// </summary>
        public IList<ClockForTemplate> CK
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
        /// Session name
        /// </summary>
        public string SN
        {
            get;
            set;
        }
    }
}