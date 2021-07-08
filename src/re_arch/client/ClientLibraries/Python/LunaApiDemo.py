from LunaAPI import LunaAPI
from Operations import Operation
from Workflow import Workflow
import time

baseUrl = "https://lunatest-routing.azurewebsites.net/api/lunanlp"
subscription = "1ac11120805b4caeba19858c95f5a8ca"
version = "v1"

fakeNewsPayload = "{\"data\":[{\"titletext\": \"Perry became the king of United States.\"}]}"


# #################################################
# Realtime Operation Demo
# #################################################

print()
print("*** Realtime API ...")
realtimeApi = LunaAPI(baseUrl, "realtime", subscription, version)
print(realtimeApi.list_operations())

print()
print("*** Pipeline API ...")
pipelineApi = LunaAPI(baseUrl, "pipeline", subscription, version)
print(pipelineApi.list_operations())




# #################################################
# Realtime Operation Demo
# #################################################
print()
print()
print("*** Calling Realtime 'FakeNews' Operation...")
realtimeResult = realtimeApi.fakenews(fakeNewsPayload, version)
print(realtimeResult)




# #################################################
# Realtime Workflow Demo
# #################################################
print()
print()
print("*** Creating Realtime Workflow...")
workflow = realtimeApi.create_workflow(["fakenews"], fakeNewsPayload, version)

print("Is Workflow Valid: " + str(workflow.validate()))

print("Realtime Workflow Output: ")
realtimeWorkflowResult = workflow.run()
print(realtimeWorkflowResult)






# #################################################
# Async/Pipeline Opertation Demo
# #################################################
print()
print()
print("*** Calling Async API Operation...")
operation = pipelineApi.outputtest("{}")

print("Async Opertation Status: ")
print(operation.get_status())
print(pipelineApi.get_operation_status(operation))
print(pipelineApi.get_operation_status(operation.operation_id))

print("Waiting for Async Operation to Complete...")
operation.wait_for_completion()

print("Async Opertation Output: ")
print(operation.get_output())
print(pipelineApi.get_operation_output(operation))
print(pipelineApi.get_operation_output(operation.operation_id))




# #################################################
# Async/Pipeline Workflow Demo
# #################################################
print()
print()
print("*** Creating Async Workflow...")
workflow = pipelineApi.create_workflow(["outputtest"], "", "v1")

print("Is Workflow Valid: " + str(workflow.validate()))

print("Running Async Workflow...")
asyncWorkflowTask = workflow.run_async()

print("Async Workflow Status...")
print(asyncWorkflowTask.get_status())
time.sleep(3)
print(asyncWorkflowTask.get_status())


print("Waiting for Workflow to Complete...")
workflow.wait_for_completion()

print(asyncWorkflowTask.get_output())

