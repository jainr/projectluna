using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Publish.Data
{
    public enum PublishingEventType
    {
        CreateLunaApplication,
        UpdateLunaApplication,
        DeleteLunaApplication,
        PublishLunaApplication,
        CreateLunaAPI,
        UpdateLunaAPI,
        DeleteLunaAPI,
        CreateLunaAPIVersion,
        UpdateLunaAPIVersion,
        DeleteLunaAPIVersion
    }
}
