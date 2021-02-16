class UserErrorMessage(object):
    OPERATION_NOT_SUPPORTED = "Operation is not supported."
    NO_MODEL_PUBLISHED = "No model published for the current API."
    NO_ENDPOINT_PUBLISHED = "No service endpoint published in the current API."
    NO_OPERATION_PUBLISHED = "No operation published in the current API."
    CAN_NOT_CONNECT_TO_MODEL_REPO =  "Can not connect to the model repository. Contact the publisher to correct the error."
    NOT_IMPLEMENTED = "{} is not supported."
    OPERATION_NOT_IN_STATUS = "Operation {} is not in {} status."
    INVALID_CERT = 'Invalid certificate.'
    INVALID_API_KEY = 'The api key is invalid.'
    API_NOT_EXIST = 'The API {} in application {} does not exist or you do not have permission to access it.'
    SUBSCRIPTION_NOT_EXIST = "The subscription {} doesn't exist or api key is invalid."
    API_VERSION_NOT_EXIST = "The specified API or API version does not exist or you do not have permission to access it."
    API_VERSION_REQUIRED = "The api-version query parameter is required."
    AAD_TOKEN_REQUIRED = "AAD token is required."
    INTERNAL_SERVER_ERROR = "The server encountered an internal error and was unable to complete your request."
