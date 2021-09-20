import json
import time
import uuid
import os
import requests
from locust import HttpUser, task, between
from locust.user.wait_time import constant
from azure.identity import EnvironmentCredential

class ScenarioTest(HttpUser):
    ### Scenario Test for Publishing a new LunaAPI Realtime Endpoint

    HttpUser.host = os.environ['GATEWAY_URL']
    wait_time = constant(60)
    
    def on_start(self):
        self.service_name = "Create & Call API Endpoint Scenario"

        self.app_url = "/api/manage/applications/"
        self.partnerServices_url = "/api/manage/partnerServices/"
        self.rbac_url = "/api/manage/rbac/"
        
        self.routing_url = os.environ['ROUTING_URL']
        self.host_url = os.environ['GATEWAY_URL']
        self.tenant_id = os.environ['AZURE_TENANT_ID']
        self.azure_client_secret = os.environ['AZURE_CLIENT_SECRET']
        self.aml_spn_client_id = os.environ['AML_SPN_CLIENT_ID']
        self.aml_spn_client_secret = os.environ['AML_SPN_CLIENT_SECRET']
        self.resourceId = os.environ['AML_RESOURCE_ID']
        self.region = os.environ['AML_REGION']
        self.admin_spn_client_id = os.environ['ADMIN_SPN_CLIENT_ID']
        self.luna_app_client_id = os.environ['LUNA_APP_CLIENT_ID']
        self.luna_app_client_secret = os.environ['ADMIN_SPN_CLIENT_SECRET']
        self.aml_endpoint_name = os.environ['AML_ENDPOINT_NAME']
        self.aml_endpoint_input = os.environ['AML_ENDPOINT_INPUT']
        
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

        azure_cred = EnvironmentCredential()
        self.azure_headers = { "Authorization": "Bearer " + azure_cred.get_token("https://management.azure.com/.default").token, "Content-Type": "application/json"}

    @task
    def create_and_call_realtime_endpoint(self):
        resource_name = "test" + str(uuid.uuid1())
        uid = str(uuid.uuid1())

        uri = "https://management.azure.com/subscriptions/a6c2a7cc-d67e-4a1a-b765-983f08c0423a/resourcegroups/xiwutest/providers/Microsoft.Resources/deployments/MarketplaceSaaS_" +str(uuid.uuid1())+"?api-version=2020-06-01"
        
        with open('CreateSaaSSubscription.json') as f:
            create_saas_subscription_payload = f.read()

        create_saas_subscription_payload = create_saas_subscription_payload.replace("<subscription-name>", str(uuid.uuid1()));

        self._assert_success(self.client.put(uri, headers=self.azure_headers, data=create_saas_subscription_payload))

        # RBAC Tests
        ##############################################

        #1. Add and remove admin
        uri = self.rbac_url + "roleassignments/Add"
        body = {
                "uid": uid,
                "userName": resource_name,
                "role": "SystemAdmin"
                }
        
        self._assert_success(self.client.post(uri, headers=self.headerData, data=json.dumps(body)))

        uri = self.rbac_url + "roleassignments/Remove"
        self._assert_success(self.client.post(uri, headers=self.headerData, data=json.dumps(body)))
        
        #2. Add and remove publisher
        uri = self.rbac_url + "roleassignments/Add"
        body = {
                "uid": uid,
                "userName": resource_name,
                "role": "Publisher"
                }
        
        self._assert_success(self.client.post(uri, headers=self.headerData, data=json.dumps(body)))

        uri = self.rbac_url + "roleassignments/Remove"
        self._assert_success(self.client.post(uri, headers=self.headerData, data=json.dumps(body)))

        #3. List role assignment
        uri = self.rbac_url + "roleassignments"
        self._assert_success(self.client.get(uri, headers=self.headerData))

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
        self._assert_success(self.client.patch(uri, headers=self.headerData, data=json.dumps(body)))
        self._assert_success(self.client.get(uri, headers=self.headerData))
        self._assert_success(self.client.get(self.partnerServices_url, headers=self.headerData))

        # 2.	Create Luna Application [PUT]
        uri = self.app_url + resource_name
        body = {
                "displayName": resource_name,
                "description": resource_name + " application",
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
        self._assert_success(self.client.patch(uri, headers=self.headerData, data=json.dumps(body)))
        self._assert_success(self.client.get(uri, headers=self.headerData))
        self._assert_success(self.client.get(self.app_url, headers=self.headerData))

        # 3.	Create Luna API [PUT]
        uri = self.app_url + resource_name + "/apis/myapi"
        body = {
                    "displayName": resource_name + " api",
                    "description": resource_name + " api",
                    "type": "Realtime",
                    "advancedSettings": resource_name + " settings"
                }
        self._assert_success(self.client.put(uri, headers=self.headerData, data=json.dumps(body)))
        self._assert_success(self.client.patch(uri, headers=self.headerData, data=json.dumps(body)))
        self._assert_success(self.client.get(uri, headers=self.headerData))

        # 4.	Create Luna API Version [PUT]
        uri = self.app_url + resource_name + "/apis/myapi/versions/v1"
        body = {
                    "AzureMLWorkspaceName":resource_name,
                    "description": "my version",
                    "type": "AzureML",
                    "endpoints": [
                        {
                            "endpointName": self.aml_endpoint_name,
                            "operationName": "predict"
                        }
                    ],
                    "advancedSettings": "this is my settings"
                }
        self._assert_success(self.client.put(uri, headers=self.headerData, data=json.dumps(body)))
        self._assert_success(self.client.patch(uri, headers=self.headerData, data=json.dumps(body)))

        # 5.	Publish Luna Application [POST]
        uri = self.app_url + resource_name + "/publish"
        self._assert_success(self.client.post(uri, headers=self.headerData, data=json.dumps(body)))
        
        # 6.	Get application master key [GET]
        uri = self.app_url + resource_name + "/masterkeys"
        response = self.client.get(uri, headers=self.headerData, data=json.dumps(body))
        self._assert_success(response)
        masterKey = response.json()['PrimaryMasterKey']

        ## long wait time for cold start up
        time.sleep(20)
        
        # 7.	Create Subscription to Application [PUT]
        uri = self.host_url + "/api/gallery/applications/" + resource_name + "/subscriptions/sub" + resource_name
        response = self.client.put(uri, headers=self.headerData)
        self._assert_success(response)
        subscriptionKey = response.json()['PrimaryKey']
        subscriptionId = response.json()['SubscriptionId']
        self._assert_success(self.client.get(uri, headers=self.headerData))
        uri = self.host_url + "/api/gallery/applications/" + resource_name + "/subscriptions/" + subscriptionId
        self._assert_success(self.client.get(uri, headers=self.headerData))

        # # Endpoint Tests with application master key
        # #############################################

        ## long wait time for cold start up
        time.sleep(20)

        # 1.	Call Realtime Endpoint [POST]
        uri = self.routing_url + "/api/" + resource_name + "/myapi/predict?api-version=v1"
        body = json.loads(self.aml_endpoint_input)
        endPointHeaderData = { "Luna-Application-Master-Key": masterKey }
        response = self.client.post(uri, headers=endPointHeaderData, data=json.dumps(body))
        self._assert_success(response)
        print("Endpoint Results: " + str(response.content))

        # 2.	regenerate application master key [POST]
        uri = self.app_url + resource_name + "/regeneratemasterkeys?key-name=primaryKey"
        response = self.client.post(uri, headers=self.headerData, data=json.dumps(body))
        self._assert_success(response)
        masterKey = response.json()['PrimaryMasterKey']
        
        time.sleep(10)
        
        # 3.	Call Realtime Endpoint with the new application master key[POST]
        uri = self.routing_url + "/api/" + resource_name + "/myapi/predict?api-version=v1"
        body = json.loads(self.aml_endpoint_input)
        endPointHeaderData = { "Luna-Application-Master-Key": masterKey }
        response = self.client.post(uri, headers=endPointHeaderData, data=json.dumps(body))
        self._assert_success(response)
        print("Endpoint Results: " + str(response.content))
        
        # # Endpoint Tests with subscription key
        # #############################################

        # 1.	Call Realtime Endpoint [POST]
        uri = self.routing_url + "/api/" + resource_name + "/myapi/predict?api-version=v1"
        body = json.loads(self.aml_endpoint_input)
        endPointHeaderData = { "api-key": subscriptionKey }
        response = self.client.post(uri, headers=endPointHeaderData, data=json.dumps(body))
        self._assert_success(response)
        print("Endpoint Results: " + str(response.content))
        
        # 2.	Regenerate subscription key [PUT]
        uri = self.host_url + "/api/gallery/applications/" + resource_name + "/subscriptions/sub" + resource_name + "/regeneratekey?key-name=PrimaryKey"
        response = self.client.post(uri, headers=self.headerData)
        self._assert_success(response)
        subscriptionKey = response.json()['PrimaryKey']
        time.sleep(10)
        
        # 3.	Call Realtime Endpoint with regenerated key [POST]
        uri = self.routing_url + "/api/" + resource_name + "/myapi/predict?api-version=v1"
        body = json.loads(self.aml_endpoint_input)
        endPointHeaderData = { "api-key": subscriptionKey }
        response = self.client.post(uri, headers=endPointHeaderData, data=json.dumps(body))
        self._assert_success(response)
        print("Endpoint Results: " + str(response.content))

        # Cleanup Test Resources
        ##############################################
        
        # 5b.   Delete subscription [DELETE]
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

