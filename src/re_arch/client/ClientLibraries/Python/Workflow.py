from requests import api
from WorkflowExecution import WorkflowExecution
import threading
import json
import time

class CustomError(Exception):
    pass

class Workflow():
    def __init__(self, api_operations, user_input, api_version, api_client):
        self.api_operations = api_operations
        self.user_input = user_input
        self.api_version = api_version
        self.api_client = api_client


    def validate(self):
        for api_operation in self.api_operations:
            if api_operation not in self.api_client.operations:
                raise CustomError("Operation does not exist, check spelling.")

        return True


    def run(self):
        self.validate()
        input = self.user_input
        result = input
        for api_operation in self.api_operations:
            api_call = getattr(self.api_client, api_operation)
            result = api_call(input)
            input = json.dumps(result)

        return result


    def run_async(self):
        self.validate()
        self.exec = WorkflowExecution()
        threaded_exec = threading.Thread(target=self.theaded_workflow)
        threaded_exec.start()
        return self.exec    


    def theaded_workflow(self):
        input = self.user_input

        for operation in self.api_operations:
            api_call = getattr(self.api_client, operation)
            current_operation = api_call(input)
            self.exec.operations.append(current_operation)
            current_operation.wait_for_completion()

            if current_operation.get_status() == "Completed":
                input = json.dumps(current_operation.get_output())

        return current_operation


    def wait_for_completion(self, time_between_calls = 30):
        while len(self.exec.operations) != len(self.api_operations):
            time.sleep(time_between_calls)
        while self.exec.get_status() in ["Scheduled","Running"]:
            time.sleep(time_between_calls)
        return
