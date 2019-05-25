using System;
using System.Diagnostics;

namespace NetUtils
{
    public static class DateTimeOffsetUtils
    {
        public static DateTimeOffset AddSafe(this DateTimeOffset dateTimeOffset, TimeSpan timeSpan)
        {
            try
            {
                return dateTimeOffset + timeSpan;
            }
            catch (ArgumentOutOfRangeException e)
            {
                Trace.TraceError(e.ToString());
                return timeSpan > TimeSpan.Zero ? DateTimeOffset.MaxValue : DateTimeOffset.MinValue;
            }
        }
    }
}
