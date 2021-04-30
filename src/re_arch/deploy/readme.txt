1. Create AAD application: follow the instruction here: https://docs.microsoft.com/en-us/azure/app-service/configure-authentication-provider-aad#-create-an-app-registration-in-azure-ad-for-your-app-service-app
2. Run deployment script in bash (Git Bash works)

./deployment.sh -s <azure-subscription-id> \
  -r <resource-group-name> \
  -l <region>\
  -n <resource-name-prefix> \
  -q <sql-admin-user-name> \
  -p <sql-admin-user-password> \
  -t <aad-application-tenant-id> \
  -c <aad-application-client-id> \
  -x <aad-application-secret> \
  -a <admin-aad-object-id> \
  -u <admin-user-display-name> \
  -w <y if created new azure service, configure only otherwise>

3. Import the Postman collection (management_test.json)

NOTE:
1. admin-user-display-name can not have space. ("xiwu@microsoft.com" is valid, "Xiaochen Wu" is not valid)
2. admin-aad-object-id is the object id of the user or service principal. It is different from the client id. See here for more details: https://docs.microsoft.com/en-us/azure/marketplace/find-tenant-object-id#find-user-object-id
3. avoid special characters other than "#%_" in sql password if possible
4. resource-name-prefix should contain only lower case letters and numbers, less than 12 characters
5. region should be a valid Azure region in format without spaces (for example, westus2)
6. the deployment script will print out the variable values for postman collection
7. before calling and management API, you need to get the access token. Run "Device authorization request", follow the message in response to authenticate, run "Get access token". You are good to go.


