using Luna.Common.Utils;
using Luna.Publish.Data;
using Luna.Publish.Public.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Publish.Clients
{
    public class PublishingEventProcessor : IPublishingEventProcessor
    {
        /// <summary>
        /// Get Luna application from a snapshot and events
        /// </summary>
        /// <param name="name">The name of the application</param>
        /// <param name="events">The events</param>
        /// <param name="snapshot">The snapshot</param>
        /// <returns></returns>
        public LunaApplication GetLunaApplication(
            string appName,
            List<BaseLunaPublishingEvent> events,
            ApplicationSnapshotDB snapshot)
        {
            LunaApplication result = null;

            if (snapshot != null)
            {
                result = (LunaApplication)JsonConvert.DeserializeObject(snapshot.SnapshotContent, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                });
            }
            else if (events[0].EventType != PublishingEventType.CreateLunaApplication)
            {
                throw new LunaServerException($"The snapshot of Luna application {appName} is null.");
            }

            foreach (var ev in events)
            {
                switch (ev.EventType)
                {
                    case PublishingEventType.CreateLunaApplication:
                        result = new LunaApplication(appName, ApplicationStatus.Draft, ((CreateLunaApplicationEvent)ev).Properties);
                        break;
                    case PublishingEventType.UpdateLunaApplication:
                        result.Properties.Update(((UpdateLunaApplicationEvent)ev).Properties);
                        break;
                    case PublishingEventType.DeleteLunaApplication:
                        result.Status = ApplicationStatus.Deleted.ToString();
                        return null;
                    case PublishingEventType.PublishLunaApplication:
                        result.Status = ApplicationStatus.Published.ToString();
                        break;
                    case PublishingEventType.CreateLunaAPI:
                        result.APIs.Add(new LunaAPI(((CreateLunaAPIEvent)ev).Name, 
                            ((CreateLunaAPIEvent)ev).Properties));
                        break;
                    case PublishingEventType.UpdateLunaAPI:
                        result.APIs.Find(x => x.Name == ((UpdateLunaAPIEvent)ev).Name).
                            Properties.Update(((UpdateLunaAPIEvent)ev).Properties);
                        break;
                    case PublishingEventType.DeleteLunaAPI:
                        result.APIs.RemoveAll(x => x.Name == ((DeleteLunaAPIEvent)ev).Name);
                        break;
                    case PublishingEventType.CreateLunaAPIVersion:
                        result.APIs.Find(x => x.Name == ((CreateLunaAPIVersionEvent)ev).APIName).
                            Versions.Add(new APIVersion(((CreateLunaAPIVersionEvent)ev).Name,
                            ((CreateLunaAPIVersionEvent)ev).Properties));
                        break;
                    case PublishingEventType.UpdateLunaAPIVersion:
                        result.APIs.Find(x => x.Name == ((UpdateLunaAPIVersionEvent)ev).APIName).
                            Versions.Find(x => x.Name == ((UpdateLunaAPIVersionEvent)ev).Name).
                            Properties.Update(((UpdateLunaAPIVersionEvent)ev).Properties);
                        break;
                    case PublishingEventType.DeleteLunaAPIVersion:
                        result.APIs.Find(x => x.Name == ((DeleteLunaAPIVersionEvent)ev).APIName).
                            Versions.RemoveAll(x => x.Name == ((DeleteLunaAPIVersionEvent)ev).Name);
                        break;
                    default:
                        throw new LunaServerException($"Unknown event type {ev.EventType.ToString()}.");
                }
            }

            return result;
        }

        /// <summary>
        /// Get Luna application in JSON string from a snapshot and events
        /// </summary>
        /// <param name="name">The name of the application</param>
        /// <param name="events">The events</param>
        /// <param name="snapshot">The snapshot</param>
        /// <returns></returns>
        public string GetLunaApplicationJSONString(
            string name,
            List<BaseLunaPublishingEvent> events,
            ApplicationSnapshotDB snapshot = null)
        {
            LunaApplication app = this.GetLunaApplication(name, events, snapshot);

            return JsonConvert.SerializeObject(app, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });
        }
    }
}
