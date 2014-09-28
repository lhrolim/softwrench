using System;
using softWrench.Mobile.Data;
using softWrench.Mobile.Metadata.Parsing;

namespace softWrench.iOS.Applications.WorkOrder
{
    internal class TimerData
    {
        /// <summary>
        ///     The string used as key in the <seealso cref="DataMap.CustomFields"/>
        ///     dictionary for storing timer-related data.
        /// </summary>
        /// <remarks>
        ///     The key is serialized as Json, so make sure
        ///     it follows the casing convention according
        ///     to <seealso cref="JsonParser.SerializerSettings"/>.
        /// </remarks>
        public const string CustomFieldKey = "timerData";

        private readonly DateTime _utcStart;

        public TimerData(DateTime utcStart)
        {
            _utcStart = utcStart;
        }

        public DateTime UtcStart
        {
            get { return _utcStart; }
        }
    }
}