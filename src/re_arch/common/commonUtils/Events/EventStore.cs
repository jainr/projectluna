using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Common.Utils.Events
{
    public class EventStore
    {
        public EventStore(string name, string connectionString, DateTime validThrough)
        {
            this.Name = name;
            this.ConnectionString = connectionString;
            this.ConnectionStringValidThroughUtc = validThrough;
            this.ValidEventTypes = new List<string>();
        }

        public string Name { get; set; }

        public string ConnectionString { get; set; }

        public List<string> ValidEventTypes { get; set; }

        public DateTime ConnectionStringValidThroughUtc { get; set; }
    }
}
