from enum import Enum

class EndpointAuthType(Enum):
    API_KEY = 1
    SERVICE_PRINCIPAL = 2
    QUERY_ARAMETER = 3

class ComputeType(Enum):
    AML = 1
    ADB = 2

class APIType(Enum):
    model = 1
    endpoint = 2
    mlproject = 3
    pipeline = 4
    dataset = 5

class AMLOperationStatus(Enum):
    Completed = 1
    
class ADBOperationStatus(Enum):
    FINISHED = 1

class OutputType(Enum):
    file = 1
    json = 2