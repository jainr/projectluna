import json
import time
from locust import HttpUser, task, between
from locust.user.wait_time import constant

class ScenarioTest(HttpUser):
    ### Scenario Test for Publishing a new LunaAPI Realtime Endpoint

    with open("src/Luna.Test/luna_locust_config.json", "r") as jsonfile: 
        data = json.load(jsonfile)
    HttpUser.host = data['gateway_host']
    wait_time = constant(60)
    
    def on_start(self):
        self.service_name = "Create & Call API Endpoint Scenario"

        self.app_url = self.data['base_url'] + "/applications/"
        self.partnerServices_url = self.data['base_url'] + "/partnerServices/azureml/"
        self.routing_url = self.data['routing_host']
        self.host_url = self.data['gateway_host']
        self.tenant_id = self.data['tenant_id']
        self.aml_spn_client_id = self.data['aml_spn_client_id']
        self.aml_spn_client_secret = self.data['aml_spn_client_secret']
        self.resourceId = self.data['resourceId']
        self.region = self.data['region']
        self.luna_user_id = self.data['luna_user_id']

        self.headerData = { "Luna-User-Id": self.luna_user_id }

    @task
    def create_and_call_realtime_endpoint(self):
        resource_name = "testscenario0628"

        # Resource Creation Tests
        ##############################################

        # 1.	Create Partner Service [PUT]
        uri = self.partnerServices_url + resource_name
        body = {
                    "displayName":resource_name,
                    "description": resource_name + " workspace",
                    "type": "AzureML",
                    "region": self.region,
                    "resourceId": self.resourceId,
                    "tenantId": self.tenant_id,
                    "clientId": self.aml_spn_client_id,
                    "clientSecret": self.aml_spn_client_secret,
                    "tags": ""
                }
        self._assert_success(self.client.put(uri, headers=self.headerData, data=json.dumps(body)))

        # 2.	Create Luna Application [PUT]
        uri = self.app_url + resource_name
        body = {
                "displayName": resource_name,
                "description": resource_name + " workspace",
                "documentationUrl": "https://aka.ms/lunaai",
                "logoImageUrl": "https://aka.ms/lunaai",
                "publisher": "ACE",
                "tags":[
                    {
                        "key": "myKey",
                        "value": "myValue"
                    }
                ]
            }
        self._assert_success(self.client.put(uri, headers=self.headerData, data=json.dumps(body)))

        # 3.	Create Luna API [PUT]
        uri = self.app_url + resource_name + "/apis/myapi"
        body = {
                    "displayName": resource_name + " api",
                    "description": resource_name + " api",
                    "type": "Realtime",
                    "advancedSettings": resource_name + " settings"
                }
        self._assert_success(self.client.put(uri, headers=self.headerData, data=json.dumps(body)))

        # 4.	Create Luna API Version [PUT]
        uri = self.app_url + resource_name + "/apis/myapi/versions/v1"
        body = {
                    "AzureMLWorkspaceName":"amlworkspace",
                    "description": "my version lalala",
                    "type": "AzureML",
                    "endpoints": [
                        {
                            "endpointName": "bostonhousing",
                            "operationName": "predict"
                        }
                    ],
                    "advancedSettings": "this is my settings"
                }
        self._assert_success(self.client.put(uri, headers=self.headerData, data=json.dumps(body)))

        # 5.	Publish Luna Application [POST]
        uri = self.app_url + resource_name + "/publish"
        self._assert_success(self.client.post(uri, headers=self.headerData, data=json.dumps(body)))

        time.sleep(4)

        # 6.	Create Subscription to Application [GET]
        uri = self.host_url + "/api/gallery/applications/" + resource_name + "/subscriptions/sub" + resource_name
        response = self.client.put(uri, headers=self.headerData)
        self._assert_success(response)
        subscriptionKey = response.json()['PrimaryKey']

        # # Endpoint Tests
        # #############################################
        time.sleep(5)

        # # 7.	Call Realtime Endpoint [POST]
        uri = self.routing_url + "/api/" + resource_name + "/myapi/predict?api-version=v1"
        body = {"data":[[1,2,3,4,5,6,7,8,9,0,1,2,3]]}
        endPointHeaderData = { "api-key": subscriptionKey }
        response = self.client.post(uri, headers=endPointHeaderData, data=json.dumps(body))
        self._assert_success(response)
        print("Endpoint Results: " + str(response.content))

        # Cleanup Test Resources
        ##############################################

        # 6b.	Delete Subscription [DELETE]
        self._assert_success(self.client.delete(self.host_url + "/api/gallery/applications/" + resource_name + "/subscriptions/sub" + resource_name, headers=self.headerData))

        # 4b.	Delete Luna API Version [DELETE]
        self._assert_success(self.client.delete(self.app_url + resource_name + "/apis/myapi/versions/v1", headers=self.headerData))

        # 3b.	Delete Luna API [DELETE]
        self._assert_success(self.client.delete(self.app_url + resource_name + "/apis/myapi", headers=self.headerData))

        # 2b.	Delete Luna Application [DELETE]
        self._assert_success(self.client.delete(self.app_url + resource_name, headers=self.headerData))

        # 1b.   Delete Partner Service [DELETE]
        self._assert_success(self.client.delete(self.partnerServices_url + resource_name, headers=self.headerData))


    ### Test Assertion Functions

    def _assert_success(self, response):
        if (response.status_code not in {200, 201, 204}): 
            errorMsg = "Error: " + str(response.status_code) + " " + str(response.request.method) + " " + str(response.url) + " " + str(response.text)
            print("##vso[task.logissue type=error;]" + errorMsg)
            print("##vso[task.complete result=Failed;]" + self.service_name)

    def _assert_failure(self, response):
        if (response.status_code not in {400, 401, 401, 403, 404, 408}): 
            errorMsg = "Error: " + str(response.status_code) + " " + str(response.request.method) + " " + str(response.url) + " " + str(response.text)
            print("##vso[task.logissue type=error;]" + errorMsg)
            print("##vso[task.complete result=Failed;]" + self.service_name)

