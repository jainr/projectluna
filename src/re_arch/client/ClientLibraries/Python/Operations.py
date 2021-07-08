from asyncio.windows_events import NULL
import time

class CustomError(Exception):
    pass

class Operation:
    def __init__(self, name, id, status, startTime, api):
        self.name = name
        self.operation_id = id
        self.status = status
        self.startTime = startTime
        self.api = api
    

    def get_output(self):
        return self.api.get_operation_output(self.operation_id)
    

    def get_status(self):
        result = self.api.get_operation_status(self.operation_id)

        if result != None and result != "Unknown":
            self.status = result

        return self.status


    def wait_for_completion(self, time_between_calls = 30):
        while self.get_status() in ["Scheduled","Running"]:
            time.sleep(time_between_calls)
        return