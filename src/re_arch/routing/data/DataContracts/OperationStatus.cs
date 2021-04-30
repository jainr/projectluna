using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Routing.Data.DataContracts
{
    public class OperationStatus
    {
        public string OperationId { get; set; }

        public string OperationName { get; set; }

        public string Status { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public Dictionary<string, string> ExtendedProperties { get; set; }

    }
}
