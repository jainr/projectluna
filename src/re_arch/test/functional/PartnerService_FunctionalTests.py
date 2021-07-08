import json
import time
from locust import HttpUser, task, between
from locust.user.wait_time import constant

class FunctionalTests(HttpUser):
    ### Functional Tests for Luna Partner Service Functions
    # https://azure.github.io/projectluna/swagger/partner/
    
    with open("src/Luna.Test/luna_locust_config.json", "r") as jsonfile: 
        data = json.load(jsonfile)

    HttpUser.host = data['gateway_host']
    wait_time = constant(60)

    def on_start(self):
        self.service_name = "Partner Service"
        self.service = "/partnerServices/"
        self.tenant_id = self.data['tenant_id']
        self.aml_spn_client_id = self.data['aml_spn_client_id']
        self.aml_spn_client_secret = self.data['aml_spn_client_secret']
        self.resourceId = self.data['resourceId']
        self.region = self.data['region']
        self.luna_user_id = self.data['luna_user_id']
        self.base_url = self.data['base_url'] + self.service
        self.interval = 3
        self.headerData = { "Luna-User-Id": self.luna_user_id }

    @task
    def partner_service_list(self):
        ################################################
        ### List Partner Services
        uri = self.base_url
        self._test("get","success", uri, self.headerData)

        ################################################
        ### Partner Service CRUD operations
        crud_obj_name = "functionaltest1"
        uri = self.base_url + "azureml/" + crud_obj_name
        body = {"displayName": crud_obj_name + " workspace", "description": "this is " + crud_obj_name, "type": "AzureML", "region": self.region, "resourceId": self.resourceId, "tenantId": self.tenant_id, "clientId": self.aml_spn_client_id, "clientSecret": self.aml_spn_client_secret, "tags": "" }    
        self._test("put","success", uri, self.headerData, body)
        self._test("get","success", uri, self.headerData, body)
        self._test("patch","success", uri, self.headerData, body)
        self._test("delete","success", uri, self.headerData, body)

        ################################################
        ### Partner Service metadata operations
        self._test("get","success", "metadata/computeservicetypes", self.headerData)
        self._test("get","success", "metadata/AzureML/mlcomponenttypes", self.headerData)
        self._test("get","success", "metadata/hostservicetypes", self.headerData)
        self._test("get","success", "amlworkspace/mlcomponents/Realtime", self.headerData)
        self._test("get","success", "amlworkspace/mlcomponents/Pipeline", self.headerData)


        ################################################
        #### Negative Test Cases

        ### Enforcing Naming Convention
        crud_obj_name = "CapitolLetters"
        uri = self.base_url + "azureml/" + crud_obj_name
        body = {"displayName": crud_obj_name + " workspace", "description": "this is " + crud_obj_name, "type": "AzureML", "region": self.region, "resourceId": self.resourceId, "tenantId": self.tenant_id, "clientId": self.aml_spn_client_id, "clientSecret": self.aml_spn_client_secret, "tags": "" }    
        self._test("put","failure", uri, self.headerData, body)

    ### Test Assertion Functions
    def _test(self, method, outcome, uri, headers, body = None):
        if (method == "put"):
            response = self.client.put(uri, headers=headers, data=json.dumps(body))
            time.sleep(self.interval)
        elif (method == "patch"):
            response = self.client.patch(uri, headers=headers, data=json.dumps(body))
            time.sleep(self.interval)
        elif (method == "post"):
            response = self.client.post(uri, headers=headers, data=json.dumps(body))
            time.sleep(self.interval)
        elif (method == "get"):
            response = self.client.get(uri, headers=headers)
        elif (method == "delete"):
            response = self.client.delete(uri, headers=headers)
        
        if (outcome == "success"):
            self._assert_success(response, uri)
        elif (outcome == "failure"):
            self._assert_failure(response, uri)
        
    def _assert_success(self, response, uri):
        if (response.status_code not in {200, 201, 204}): 
            errorMsg = "Error: " + str(response.status_code) + " " + str(response.request.method) + " " + uri + " " + str(response.text)
            print("##vso[task.logissue type=error;]" + errorMsg)
            print("##vso[task.complete result=Failed;]" + self.service_name)

    def _assert_failure(self, response, uri):
        if (response.status_code not in {400, 401, 401, 403, 404, 408}): 
            errorMsg = "Error: " + str(response.status_code) + " " + str(response.request.method) + " " + uri + " " + str(response.text)
            print("##vso[task.logissue type=error;]" + errorMsg)
            print("##vso[task.complete result=Failed;]" + self.service_name)

            