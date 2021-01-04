using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Data.DataContracts.Luna.AI
{
    public class MLModelArtifact
    {
        public string Name { get; set; }
        public int Version { get; set; }
        public string Framework { get; set; }
        public string FrameworkVersion { get; set; }
    }
}