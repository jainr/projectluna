import json
import time
import uuid
import os
import requests
from locust import HttpUser, task, between
from locust.user.wait_time import constant

class ScenarioTest(HttpUser):
    ### Scenario Test for Publishing a new LunaAPI Realtime Endpoint

    with open("luna_locust_config.json", "r") as jsonfile: 
        data = json.load(jsonfile)
    HttpUser.host = os.environ['GATEWAY_URL']
    wait_time = constant(60)
    
    def on_start(self):
        self.service_name = "Create & Call API Endpoint Scenario"

        
        self.app_url = "/api/manage/applications/"
        self.partnerServices_url = "/api/manage/partnerServices/azureml/"
        
        self.routing_url = os.environ['ROUTING_URL']
        self.host_url = os.environ['GATEWAY_URL']
        self.tenant_id = os.environ['TENANT_ID']
        self.aml_spn_client_id = os.environ['AML_SPN_CLIENT_ID']
        self.aml_spn_client_secret = os.environ['AML_SPN_CLIENT_SECRET']
        self.resourceId = os.environ['AML_RESOURCE_ID']
        self.region = os.environ['AML_REGION']
        self.admin_spn_client_id = os.environ['ADMIN_SPN_CLIENT_ID']
        self.luna_app_client_id = os.environ['LUNA_APP_CLIENT_ID']
        self.luna_app_client_secret = os.environ['ADMIN_SPN_CLIENT_SECRET']
        
        data = {
            "grant_type": "client_credentials",
            "client_id": self.admin_spn_client_id,
            "resource": self.luna_app_client_id,
            "client_secret": self.luna_app_client_secret
        }
        
        headers = {'Content-Type': 'application/x-www-form-urlencoded'}
        
        url = "https://login.microsoftonline.com/" + self.tenant_id + "/oauth2/token"
        
        response = requests.post(url, data=data, headers=headers)
        
        result = response.json()
        
        access_token = result["access_token"]

        self.headerData = { "Authorization": "Bearer " + access_token }

    @task
    def create_and_call_realtime_endpoint(self):
        resource_name = "test" + str(uuid.uuid1())

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

        time.sleep(15)

        # 6.	Create Subscription to Application [GET]
        uri = self.host_url + "/api/gallery/applications/" + resource_name + "/subscriptions/sub" + resource_name
        response = self.client.put(uri, headers=self.headerData)
        self._assert_success(response)
        subscriptionKey = response.json()['PrimaryKey']

        # # Endpoint Tests
        # #############################################
        time.sleep(15)

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

