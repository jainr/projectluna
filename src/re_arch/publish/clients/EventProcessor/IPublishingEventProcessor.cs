using Luna.Publish.Data;
using Luna.Publish.Public.Client;
using System.Collections.Generic;

namespace Luna.Publish.Clients
{
    public interface IPublishingEventProcessor
    {
        /// <summary>
        /// Get Luna application from a snapshot and events
        /// </summary>
        /// <param name="name">The name of the application</param>
        /// <param name="events">The events</param>
        /// <param name="snapshot">The snapshot</param>
        /// <returns></returns>
        LunaApplication GetLunaApplication(
            string name, 
            List<BaseLunaPublishingEvent> events,
            ApplicationSnapshotDB snapshot = null);

        /// <summary>
        /// Get Luna application in JSON string from a snapshot and events
        /// </summary>
        /// <param name="name">The name of the application</param>
        /// <param name="events">The events</param>
        /// <param name="snapshot">The snapshot</param>
        /// <returns></returns>
        string GetLunaApplicationJSONString(
            string name,
            List<BaseLunaPublishingEvent> events,
            ApplicationSnapshotDB snapshot = null);
    }
}
