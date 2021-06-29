from os import O_APPEND
import time
import requests
import json
from requests import api
from requests.models import requote_uri
from Operations import Operation
from Workflow import Workflow

# Main LunaAPI class
class LunaAPI:
    def __make_func(self, name, version = None):
        base_url = self.base_url
        api_name = self.api_name
        subscription_key = self.subscription_key
        is_async = self.is_async
        lunaAPI = self
        default_api_version = self.default_api_version
        if version != None:
            default_api_version = version

        def _function(self, data='', version = None):
            url =  base_url + api_name + "/" + name + "?api-version=" + default_api_version
            headers = {'api-key': subscription_key}
            payload=data
            if is_async:
                reponse = requests.request("POST", url, headers=headers, data=payload)
                parsed_response = json.loads(reponse.text)
                result = Operation(name, parsed_response["operationId"], parsed_response["status"], parsed_response["startTime"], lunaAPI)
                return result
            else:
                response = requests.request("POST", url, headers=headers, data=payload)
                return response.text

        setattr(LunaAPI, name, _function)
        return _function


    def __get_operations_from_api(self, api_version = None):
        url = self.base_url + self.api_name + "/metadata"
        headers = {'api-key': self.subscription_key}

        if (api_version == None):
            api_version = self.default_api_version
        
        response = requests.request("GET", url, headers=headers)
        parsed_reponse = json.loads(response.text)

        self.api_type = parsed_reponse["apiType"]
        if parsed_reponse["apiType"] == "Pipeline":
            self.is_async = True
        else:
            self.is_async = False

        for version in parsed_reponse["versions"]:
            if version["name"] == api_version:
                return version["operations"]

        raise CustomError("Operations could not be found")
        

    def __init__(self, base_url, api_name, subscription_key, default_api_version = None):
        if base_url[-1] != '/':
            base_url = base_url + '/'

        if api_name[-1] == '/':
            api_name = api_name[:-1]

        self.base_url = base_url
        self.subscription_key = subscription_key
        self.api_name = api_name
        self.default_api_version = default_api_version
        self.operations = self.__get_operations_from_api()
        self.functions = []
        self.headers = {'api-key': self.subscription_key}

        for operation in self.operations:
            self.functions.append(self.__make_func(operation))


    def list_operations(self, api_version = None):
        if api_version == None:
            api_version = self.default_api_version

        api_operations = self.operations
        if (api_version != self.default_api_version):
            api_operations = self.__get_operations_from_api(api_version)

        return {
            "type": self.api_type,
            "versions": [
                {
                    "name": api_version,
                    "operations": api_operations
                }
            ]
        }


    def create_workflow(self, operations, user_input, api_version):
        workflow = Workflow(operations, user_input, api_version, self)
        return workflow


    def get_operation_status(self, operation):
        if hasattr(operation, "operation_id"): 
            operation_id = operation.operation_id
        else:
            operation_id = operation

        url = self.base_url + self.api_name + "/" + "operations/" + operation_id + "?api-version=" + self.default_api_version
        response = requests.request("GET", url, headers=self.headers)
        parsed_reponse = json.loads(response.text)

        if response.status_code == 404:
            time.sleep(3)
            response = requests.request("GET", url, headers=self.headers)
            parsed_reponse = json.loads(response.text)

        if response.status_code != 200:
            raise CustomError(response)

        return parsed_reponse["status"]


    def get_operation_output(self, operation):
        if hasattr(operation, "operation_id"): 
            operation_id = operation.operation_id
        else:
            operation_id = operation

        if self.get_operation_status(operation_id) != "Completed":
            raise CustomError("Operation not complete. Output Unavailable")

        url = self.base_url + self.api_name + "/" + "operations/" + operation_id + "/output" + "?api-version=" + self.default_api_version
        response = requests.request("GET", url, headers=self.headers)
        parsed_reponse = json.loads(response.text)

        return parsed_reponse



class CustomError(Exception):
    pass

