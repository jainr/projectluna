class WorkflowExecution():
    def __init__(self):
        self.operations = []


    def get_status(self):
        if len(self.operations) > 0:
            return self.operations[-1].get_status()
        return "Pending"


    def get_output(self):
        if len(self.operations) > 0:
            return self.operations[-1].get_output()
        return None
