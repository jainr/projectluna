using Luna.Publish.Data;
using Luna.Publish.Public.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Luna.Publish.Clients
{
    /// <summary>
    /// The client class to generate event content
    /// </summary>
    public class AppEventContentGenerator : IAppEventContentGenerator
    {

        public AppEventContentGenerator()
        {
        }

        /// <summary>
        /// Generate create Luna application event and convert to JSON string
        /// </summary>
        /// <param name="name">The application name</param>
        /// <param name="properties">The application properties</param>
        /// <returns>The event content JSON string</returns>
        public string GenerateCreateLunaApplicationEventContent(
            string name,
            LunaApplicationProp properties)
        {
            var ev = new CreateLunaApplicationEvent()
            {
                Properties = properties,
                Name = name
            };

            return this.ConvertToJSONWithAllTypeNames(ev);
        }

        /// <summary>
        /// Generate update Luna application event and convert to JSON string
        /// </summary>
        /// <param name="name">The application name</param>
        /// <param name="properties">The application properties</param>
        /// <returns>The event content JSON string</returns>
        public string GenerateUpdateLunaApplicationEventContent(
            string name,
            LunaApplicationProp properties)
        {
            var ev = new UpdateLunaApplicationEvent()
            {
                Properties = properties,
                Name = name
            };

            return this.ConvertToJSONWithAllTypeNames(ev);
        }


        /// <summary>
        /// Generate delete application event and convert to JSON string
        /// </summary>
        /// <param name="name">The application name</param>
        /// <returns>The event content JSON string</returns>
        public string GenerateDeleteLunaApplicationEventContent(string name)
        {
            var ev = new DeleteLunaApplicationEvent()
            {
                Name = name
            };

            return this.ConvertToJSONWithAllTypeNames(ev);
        }


        /// <summary>
        /// Generate publish application event and convert to JSON string
        /// </summary>
        /// <param name="name">The application name</param>
        /// <returns>The event content JSON string</returns>
        public string GeneratePublishLunaApplicationEventContent(string name, string publishingComment)
        {
            var ev = new PublishLunaApplicationEvent()
            {
                Name = name,
                PublishingComments = publishingComment
            };

            return this.ConvertToJSONWithAllTypeNames(ev);
        }

        /// <summary>
        /// Generate create Luna API event and convert to JSON string
        /// </summary>
        /// <param name="appName">The application name</param>
        /// <param name="name">The API name</param>
        /// <param name="properties">The API properties</param>
        /// <returns>The event content JSON string</returns>
        public string GenerateCreateLunaAPIEventContent(
            string appName,
            string name,
            BaseLunaAPIProp properties)
        {
            var ev = new CreateLunaAPIEvent()
            {
                ApplicationName = name,
                Name = name,
                Properties = properties
            };

            return this.ConvertToJSONWithAllTypeNames(ev);
        }


        /// <summary>
        /// Generate update Luna API event and convert to JSON string
        /// </summary>
        /// <param name="appName">The application name</param>
        /// <param name="name">The API name</param>
        /// <param name="properties">The API properties</param>
        /// <returns>The event content JSON string</returns>
        public string GenerateUpdateLunaAPIEventContent(
            string appName,
            string name,
            BaseLunaAPIProp properties)
        {
            var ev = new UpdateLunaAPIEvent()
            {
                ApplicationName = name,
                Name = name,
                Properties = properties
            };

            return this.ConvertToJSONWithAllTypeNames(ev);
        }


        /// <summary>
        /// Generate delete API event and convert to JSON string
        /// </summary>
        /// <param name="appName">The application name</param>
        /// <param name="name">The application name</param>
        /// <returns>The event content JSON string</returns>
        public string GenerateDeleteLunaAPIEventContent(string appName, string name)
        {
            var ev = new DeleteLunaAPIEvent()
            {
                ApplicationName = name,
                Name = name
            };

            return this.ConvertToJSONWithAllTypeNames(ev);
        }

        /// <summary>
        /// Generate create Luna API version event and convert to JSON string
        /// </summary>
        /// <param name="appName">The application name</param>
        /// <param name="apiName">The API name</param>
        /// <param name="name">The version name</param>
        /// <param name="properties">The API properties</param>
        /// <returns>The event content JSON string</returns>
        public string GenerateCreateLunaAPIVersionEventContent(
            string appName,
            string apiName,
            string name,
            BaseAPIVersionProp properties)
        {
            var ev = new CreateLunaAPIVersionEvent()
            {
                ApplicationName = appName,
                APIName = apiName,
                Name = name,
                Properties = properties
            };

            return this.ConvertToJSONWithAllTypeNames(ev);
        }


        /// <summary>
        /// Generate update Luna API version event and convert to JSON string
        /// </summary>
        /// <param name="appName">The application name</param>
        /// <param name="apiName">The API name</param>
        /// <param name="name">The version name</param>
        /// <param name="properties">The API properties</param>
        /// <returns>The event content JSON string</returns>
        public string GenerateUpdateLunaAPIVersionEventContent(
            string appName,
            string apiName,
            string name,
            BaseAPIVersionProp properties)
        {
            var ev = new UpdateLunaAPIVersionEvent()
            {
                ApplicationName = appName,
                APIName = apiName,
                Name = name,
                Properties = properties
            };

            return this.ConvertToJSONWithAllTypeNames(ev);
        }


        /// <summary>
        /// Generate delete API version event and convert to JSON string
        /// </summary>
        /// <param name="appName">The application name</param>
        /// <param name="apiName">The API name</param>
        /// <param name="name">The version name</param>
        /// <returns>The event content JSON string</returns>
        public string GenerateDeleteLunaAPIVersionEventContent(string appName, string apiName, string name)
        {
            var ev = new DeleteLunaAPIVersionEvent()
            {
                ApplicationName = appName,
                APIName = apiName,
                Name = name
            };

            return this.ConvertToJSONWithAllTypeNames(ev);
        }

        /// <summary>
        /// Convert an event to JSON string with all type names
        /// </summary>
        /// <typeparam name="T">The type of the event</typeparam>
        /// <param name="ev">The event</param>
        /// <returns>The JSON string</returns>
        private string ConvertToJSONWithAllTypeNames<T>(T ev)
        {
            var evString = JsonConvert.SerializeObject(ev, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });

            return evString;
        }

    }
}
