using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Common.LoggingUtils
{
    public class ErrorMessages
    {
        public const string APPLICATION_ALREADY_EXIST = "Name of {0} is being used by other applications. Please choose a different name and try again.";
        public const string MARKETPLACE_OFFER_ALREADY_EXIST = "Id of {0} is being used by other Azure marketplace offers. Please choose a different name and try again.";
        public const string MARKETPLACE_PLAN_ALREADY_EXIST = "Id of {0} is being used by other Azure marketplace plans in offer {1}. Please choose a different name and try again.";
        public const string API_ALREADY_EXIST = "Name of {0} is being used by other APIs in current application {1}. Please choose a different name and try again.";
        public const string API_VERSION_ALREADY_EXIST = "Name of {0} is being used by other versions in current API {1}. Please choose a different name and try again.";
        public const string PARTNER_SERVICE_ALREADY_EXIST = "Name of {0} is being used by other partner services. Please choose a different name and try again.";
        public const string APPLICATION_DOES_NOT_EXIST = "Application {0} does not exist or you do not have permission to access it.";
        public const string MARKETPLACE_OFFER_DOES_NOT_EXIST = "Azure marketplace offer {0} does not exist or you do not have permission to access it.";
        public const string MARKETPLACE_PLAN_DOES_NOT_EXIST = "Azure marketplace plan {0} in offer {1} does not exist or you do not have permission to access it.";
        public const string API_DOES_NOT_EXIST = "API {0} in application {1} does not exist or you do not have permission to access it.";
        public const string API_VERSION_DOES_NOT_EXIST = "API version {0} in API {1} does not exist or you do not have permission to access it.";
        public const string PARTNER_SERVICE_DOES_NOT_EXIST = "Partner service {0} does not exist or you do not have permission to access it.";
        public const string APPLICATION_KEY_DOES_NOT_EXIST = "The application key {0} does not exist or you do not have permission to access it.";
        public const string OPERATION_DOES_NOT_EXIST = "The operation {0} does not exist or you do not have permission to access it.";
        public const string OPERATION_ID_DOES_NOT_EXIST = "The operation with id {0} does not exist or you do not have permission to access it.";
        public const string MISSING_PARAMETER = "The parameter {0} is required.";
        public const string VALUE_NOT_UPDATABLE = "The value of parameter {0} is not updatable.";
        public const string CAN_NOT_DELETE_APPLICATION_WITH_APIS = "Can not delete the application {0} unless all APIs in the current application is deleted.";
        public const string CAN_NOT_DELETE_API_WITH_VERSIONS = "Can not delete the api {0} unless all version in the current api is deleted.";
        public const string API_TYPE_NOT_SUPPORTED = "API type {0} is not supported.";
        public const string VERSION_TYPE_NOT_SUPPORTED = "Version type {0} of API type {1} is not supported.";
        public const string MISSING_QUERY_PARAMETER = "Query parameter '{0}' is required.";
        public const string INVALID_QUERY_PARAMETER_VALUE = "Value of query parameter '{0}' is invalid.";
        public const string CAN_NOT_UPDATE_PARTNER_SERVICE_TYPE = "Can not update partner service type. The current type is {0}.";
        public const string CAN_NOT_CONNECT_TO_PARTNER_SERVICE = "Can not connect to partner service {0}.";
        public const string CAN_NOT_PERFORM_OPERATION = "Can not perform the operation.";
        public const string CAN_NOT_REMOVE_YOUR_OWN_ACCOUNT_FROM_ADMN = "Removing your own account from SystemAdmin is not supported";
        public const string CAN_NOT_CANCEL_EXECUTION = "Can not cancel operation with id {0}. The operation status is {1}.";
        public const string CAN_NOT_GET_OUTPUT = "Can not get operation output with id {0}. The operation status is {1}.";
        public const string EVENT_STORE_DOES_NOT_EXIST = "Event store {0} does not exist";
        public const string EVENT_TYPE_IS_NOT_SUPPORTED = "Event type {0} is not support in event store {1}";
        public const string SUBSCIRPTION_ALREADY_EXIST = "Subscription with name {0} already exist for application {1}. Pleaes choose a different name and try again.";
        public const string SUBSCIRPTION_DOES_NOT_EXIST = "The subsciption {0} does not exist or you do not have permission to access it.";
        public const string SUBSCIRPTION_OWNER_ALREADY_EXIST = "User {0} is already a owner of subscription {1}.";
        public const string SUBSCIRPTION_OWNER_DOES_NOT_EXIST = "User {0} is not a a owner of subscription {1}.";
        public const string KEY_NAME_NOT_SUPPORTED = "Regenerate key with name {0} is not supported.";
        public const string MISSING_REQUEST_BODY = "Request boddy is required.";
        public const string INVALID_KEY = "The subscription key is invalid.";
        public const string STRING_PROPERTY_VALUE_TOO_LONG = "The value of property {0} is too long. Allow max length is {1}";
        public const string INVALID_PROPERTY_VALUE = "The value of property {0} is invalid";
        public const string STRING_PROPERTY_NOT_VALID_HTTPS_URL = "The value of property {0} is not a valid https url.";
        public const string INVALID_ID_PROPERTY = "The value of property '{0}' is invalid. It allow only lower case characters or numbers.";
        public const string INVALID_INPUT = "The request body is invalid.";
        public const string INVALID_ML_COMPONENT_TYPE = "The partner service {0} does not host machine learning components with type {1}";
        public const string INVALID_TENANT_OR_CLIENT_ID = "The tenant id or client id is invalid.";
        public const string INVALID_CLIENT_SECRET = "The client secret is invalid.";
        public const string INVALID_MARKETPLACE_TOKEN = "The Azure marketplace token is invalid.";
        public const string MARKETPLACE_SUBSCRIPTION_DOES_NOT_EXIST = "The Azure marketplace subscription {0} does not exist or you don't have permission to access it.";
        public const string MARKETPLACE_OFFER_NAME_DOES_NOT_MATCH = "The offer id {0} in request path does not match the offer id in request body {1}.";
        public const string MARKETPLACE_PLAN_NAME_DOES_NOT_MATCH = "The plan id {0} in request path does not match the plan id in request body {1}.";
        public const string MARKETPLACE_SUBSCRIPTION_CAN_NOT_BE_ACTIVATED = "Can not activate Azure marketplace subscription {0}. The subscription is in {1} state.";
    }
}
