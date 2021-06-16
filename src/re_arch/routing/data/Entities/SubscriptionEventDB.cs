using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Routing.Data
{
    public class ProcessedEventDB
    {
        public long Id { get; set; }

        public string EventStoreName { get; set; }

        public long LastAppliedEventId { get; set; }
    }
}
