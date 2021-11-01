using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Agent.Data
{
    public class ProvisioningJobDb
    {
        public long Id { get; set; }

        public Guid SubscriptionId { get; set; }

        public Guid JobId { get; set; }

        public string Status { get; set; }

        public string Mode { get; set; }

        public string JobType { get; set; }

        public int CurrentJobStepIndex { get; set; }

        public bool IsSynchronizedStep { get; set; }

        public string CurrentJobStepStatus { get; set; }

        public string ParametersSecretName { get; set; }

        public string ProvisionStepsSecretName { get; set; }

        public bool IsActive { get; set; }

        public int RetryCount { get; set; }

        public string LastErrorMessage { get; set; }

        public long CreatedByEventId { get; set; }

        public string ProvisionSteps { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime LastUpdatedTime { get; set; }

        public DateTime? CompletedTime { get; set; }
    }
}
