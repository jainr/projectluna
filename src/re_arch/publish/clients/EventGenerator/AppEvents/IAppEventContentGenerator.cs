using Luna.Publish.Public.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Luna.Publish.Clients
{
    /// <summary>
    /// The client interface to generate publishing events
    /// </summary>
    public interface IAppEventContentGenerator
    {
        /// <summary>
        /// Generate create Luna application event and convert to JSON string
        /// </summary>
        /// <param name="name">The application name</param>
        /// <param name="properties">The application properties</param>
        /// <returns>The event content JSON string</returns>
        string GenerateCreateLunaApplicationEventContent(
            string name, 
            LunaApplicationProp properties);


        /// <summary>
        /// Generate update Luna application event and convert to JSON string
        /// </summary>
        /// <param name="name">The application name</param>
        /// <param name="properties">The application properties</param>
        /// <returns>The event content JSON string</returns>
        string GenerateUpdateLunaApplicationEventContent(
            string name,
            LunaApplicationProp properties);


        /// <summary>
        /// Generate delete application event and convert to JSON string
        /// </summary>
        /// <param name="name">The application name</param>
        /// <returns>The event content JSON string</returns>
        string GenerateDeleteLunaApplicationEventContent(string name);


        /// <summary>
        /// Generate publish application event and convert to JSON string
        /// </summary>
        /// <param name="name">The application name</param>
        /// <returns>The event content JSON string</returns>
        string GeneratePublishLunaApplicationEventContent(string name, string publishingComment);


        /// <summary>
        /// Generate create Luna API event and convert to JSON string
        /// </summary>
        /// <param name="appName">The application name</param>
        /// <param name="name">The API name</param>
        /// <param name="properties">The API properties</param>
        /// <returns>The event content JSON string</returns>
        string GenerateCreateLunaAPIEventContent(
            string appName,
            string name,
            BaseLunaAPIProp properties);


        /// <summary>
        /// Generate update Luna API event and convert to JSON string
        /// </summary>
        /// <param name="appName">The application name</param>
        /// <param name="name">The API name</param>
        /// <param name="properties">The API properties</param>
        /// <returns>The event content JSON string</returns>
        string GenerateUpdateLunaAPIEventContent(
            string appName,
            string name,
            BaseLunaAPIProp properties);


        /// <summary>
        /// Generate delete API event and convert to JSON string
        /// </summary>
        /// <param name="appName">The application name</param>
        /// <param name="name">The application name</param>
        /// <returns>The event content JSON string</returns>
        string GenerateDeleteLunaAPIEventContent(string appName, string name);

        /// <summary>
        /// Generate create Luna API version event and convert to JSON string
        /// </summary>
        /// <param name="appName">The application name</param>
        /// <param name="apiName">The API name</param>
        /// <param name="name">The version name</param>
        /// <param name="properties">The API properties</param>
        /// <returns>The event content JSON string</returns>
        string GenerateCreateLunaAPIVersionEventContent(
            string appName,
            string apiName,
            string name,
            BaseAPIVersionProp properties);


        /// <summary>
        /// Generate update Luna API version event and convert to JSON string
        /// </summary>
        /// <param name="appName">The application name</param>
        /// <param name="apiName">The API name</param>
        /// <param name="name">The version name</param>
        /// <param name="properties">The API properties</param>
        /// <returns>The event content JSON string</returns>
        string GenerateUpdateLunaAPIVersionEventContent(
            string appName,
            string apiName,
            string name,
            BaseAPIVersionProp properties);


        /// <summary>
        /// Generate delete API version event and convert to JSON string
        /// </summary>
        /// <param name="appName">The application name</param>
        /// <param name="apiName">The API name</param>
        /// <param name="name">The version name</param>
        /// <returns>The event content JSON string</returns>
        string GenerateDeleteLunaAPIVersionEventContent(string appName, string apiName, string name);
    }
}
