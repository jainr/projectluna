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
    public class AppEventProcessor : IAppEventProcessor
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
            List<BaseLunaAppEvent> events,
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
            else if (events[0].EventType != LunaAppEventType.CreateLunaApplication)
            {
                throw new LunaServerException($"The snapshot of Luna application {appName} is null.");
            }

            foreach (var ev in events)
            {
                switch (ev.EventType)
                {
                    case LunaAppEventType.CreateLunaApplication:
                        result = new LunaApplication(appName, ApplicationStatus.Draft, ((CreateLunaApplicationEvent)ev).Properties);
                        break;
                    case LunaAppEventType.UpdateLunaApplication:
                        result.Properties.Update(((UpdateLunaApplicationEvent)ev).Properties);
                        break;
                    case LunaAppEventType.DeleteLunaApplication:
                        result.Status = ApplicationStatus.Deleted.ToString();
                        return null;
                    case LunaAppEventType.PublishLunaApplication:
                        result.Status = ApplicationStatus.Published.ToString();
                        break;
                    case LunaAppEventType.CreateLunaAPI:
                        result.APIs.Add(new LunaAPI(((CreateLunaAPIEvent)ev).Name, 
                            ((CreateLunaAPIEvent)ev).Properties));
                        break;
                    case LunaAppEventType.UpdateLunaAPI:
                        result.APIs.Find(x => x.Name == ((UpdateLunaAPIEvent)ev).Name).
                            Properties.Update(((UpdateLunaAPIEvent)ev).Properties);
                        break;
                    case LunaAppEventType.DeleteLunaAPI:
                        result.APIs.RemoveAll(x => x.Name == ((DeleteLunaAPIEvent)ev).Name);
                        break;
                    case LunaAppEventType.CreateLunaAPIVersion:
                        result.APIs.Find(x => x.Name == ((CreateLunaAPIVersionEvent)ev).APIName).
                            Versions.Add(new APIVersion(((CreateLunaAPIVersionEvent)ev).Name,
                            ((CreateLunaAPIVersionEvent)ev).Properties));
                        break;
                    case LunaAppEventType.UpdateLunaAPIVersion:
                        result.APIs.Find(x => x.Name == ((UpdateLunaAPIVersionEvent)ev).APIName).
                            Versions.Find(x => x.Name == ((UpdateLunaAPIVersionEvent)ev).Name).
                            Properties.Update(((UpdateLunaAPIVersionEvent)ev).Properties);
                        break;
                    case LunaAppEventType.DeleteLunaAPIVersion:
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
            List<BaseLunaAppEvent> events,
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
