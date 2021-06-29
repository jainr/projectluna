import json
import time
from locust import HttpUser, task, between
from locust.user.wait_time import constant

class FunctionalTests(HttpUser):
    ### Functional Tests for Luna Publish Service Functions
    # https://azure.github.io/projectluna/swagger/publish/

    with open("src/Luna.Test/luna_locust_config.json", "r") as jsonfile: 
        data = json.load(jsonfile)
    HttpUser.host = data['gateway_host']
    wait_time = constant(60)

    def on_start(self):
        self.service_name = "Publish Service"

        self.tenant_id = self.data['tenant_id']
        self.aml_spn_client_id = self.data['aml_spn_client_id']
        self.aml_spn_client_secret = self.data['aml_spn_client_secret']
        self.resourceId = self.data['resourceId']
        self.region = self.data['region']
        self.luna_user_id = self.data['luna_user_id']
        self.application_owner_id = self.data['application_owner_id']

        self.headerData = { "Luna-User-Id": self.luna_user_id }
        self.application_name = "functionaltestapp"
        self.interval = 3

    @task
    def Publishservice(self):
        ################################################
        #### Review Settings (CRUD)
        crud_object_name = "webhooktest"
        uri = "/api/reviewsettings/webhooks/" + crud_object_name
        body = {"Name": "responsible-ai-scan","Description": "Check responsible AI","WebhookUrl": "https://aka.ms/lunaresponsibleai","IsEnabled": True,"CreatedTime": None,"LastUpdatedTime": None }
        # self._test("put","success", uri, self.headerData, body)
        # self._test("patch","success", uri, self.headerData, body)
        # self._test("get","success", uri, self.headerData, body)
        # self._test("delete","success", uri, self.headerData)
        

        # ################################################
        # #### Application Review
        # uri = "/api/reviewsettings/webhooks"
        # self._test("get","success", uri, self.headerData)


        # ################################################
        # #### Azure Marketplace
        # # Offers (list)
        # uri = "/api/marketplace/offers"
        # self._test("get","success", uri, self.headerData)

        # # Offers (CRUD)
        # crud_offer_id = "offeridtest"
        # body = {"MarketplaceOfferId": "mytestoffer","DisplayName": "Test Offer","Description": "This is my test offer","Status": "Draft","CreatedTime": "2021-06-09T17:23:13.1352800-07:00","LastUpdatedTime": "2021-06-09T17:23:13.1352800-07:00","DeletedTime": None,"IsManualActivation": True,"Plans": []}
        # uri = "/api/marketplace/offers/" + crud_offer_id
        # self._test("put","success", uri, self.headerData, body)
        # self._test("patch","success", uri, self.headerData, body)
        # self._test("get","success", uri, self.headerData, body)
        # self._test("post","success", uri + "/publish", self.headerData, body)

        # # Plans (list)
        # uri = "/api/marketplace/offers/" + crud_offer_id + "/plans"
        # self._assert_success(self.client.get(uri, headers=self.headerData))

        # # Plans (CRUD)
        # crud_plan_id = "planidtest"
        # body = {"MarketplaceOfferId": "myoffer","MarketplacePlanId": "myplan","Description": "This is my plan","IsLocalDeployment": True,"ManagementKitDownloadUrl": "https://aka.ms/lunamgmtkit","ManagementKitDownloadUrlSecretName": None,"CreatedTime": "0001-01-01T00:00:00.0000000-08:00","LastUpdatedTime": "0001-01-01T00:00:00.0000000-08:00"}
        # uri = "/api/marketplace/offers/" + crud_offer_id + "/plans/" + crud_plan_id
        # self._test("put","success", uri, self.headerData, body)
        # self._test("patch","success", uri, self.headerData, body)
        # self._test("get","success", uri, self.headerData, body)
        # self._test("delete","success", uri, self.headerData, body)

        # uri = "/api/marketplace/offers/" + crud_offer_id
        # self._test("delete","success", uri, self.headerData)


        ################################################
        #### Application
        # (list)
        uri = "/api/applications"
        self._test("get","success", uri, self.headerData)

        # # (CRUD)
        # crud_app_name = "testapplication"
        # uri = "/api/applications/" + crud_app_name
        # body = {"OwnerUserId": self.application_owner_id,"DisplayName": "My App","Description": "This is my application","DocumentationUrl": "https://aka.ms/lunadoc","LogoImageUrl": "https://aka.ms/lunalogo.png","Publisher": "Microsoft","Tags": [{"Key": "Department","Value": "HR"}],"CreatedBy": None,"PrimaryMasterKeySecretName": None,"SecondaryMasterKeySecretName": None}
        # self._test("put","success", uri, self.headerData, body)
        # self._test("patch","success", uri, self.headerData, body)
        # self._test("get","success", uri, self.headerData, body)
        # self._test("post","success", uri + "/publish", self.headerData, body)
        # self._test("post","success", uri + "/regenerateMasterKeys?key-name=testkeys", self.headerData, body)
        # self._test("get","success", uri + "/masterkeys", self.headerData, body)


        # ################################################
        # #### API
        # crud_api_name = "testapplication"
        # uri = "/api/applications/" + crud_app_name + "/apis/" + crud_api_name
        # body = {"DisplayName": "sentimentanalysis","Type": "Pipeline","Description": "Sentiment analysis API","AdvancedSettings": None}
        # self._test("put","success", uri, self.headerData, body)
        # self._test("patch","success", uri, self.headerData, body)

        # ################################################
        # #### API Version
        # crud_version_name = "testversion"
        # uri = "/api/applications/" + crud_app_name + "/apis/" + crud_api_name + "/versions/" + crud_version_name
        # body = {"Type": "AzureML","Description": "API version from Azure ML workspace","AdvancedSettings": None}
        # self._test("put","success", uri, self.headerData, body)
        # self._test("patch","success", uri, self.headerData, body)
        # self._test("delete","success", uri, self.headerData)

        # ################################################
        # #### API
        # # (delete)
        # uri = "/api/applications/" + crud_app_name + "/apis/" + crud_api_name
        # self._test("delete","success", uri, self.headerData)


        # ################################################
        # #### Application
        # # (delete)
        # uri = "/api/applications/" + crud_app_name
        # self._test("delete","success", uri, self.headerData)





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



