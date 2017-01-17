using System;

namespace Sweetshot.Library.Models.Requests.Common
{
    public class OffsetLimitFields
    {
        public OffsetLimitFields(string offset = "", int limit = 0)
        {
            Offset = offset;
            Limit = limit;
        }

        public string Offset { get; private set; }
        public int Limit { get; private set; }
    }
}