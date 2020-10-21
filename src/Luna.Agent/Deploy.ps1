param (
    [Parameter(Mandatory=$true)]
    [string]$name = "default", 

    [Parameter(Mandatory=$true)]
    [string]$location = "West US 2",

    [string]$folder = "default",
    
    [string]$webAppServicePlanName = "default",

    [string]$webAppServicePlanSku = "p1v2",

    [string]$agentApiWebAppName = "default",
    
    [string]$agentPortalWebAppName = "default",

    [string]$agentApiAADApplicationId = "default",
    
    [string]$keyVaultName = "default",
    [string]$sqlServerName = "default",
    [string]$sqlDbName = "default",
    
    [string]$resourceGroupName = "default",

    [string]$sqlAdminUser = "cloudsa",

    [string]$sqlAdminPassword = "default",

    [string]$sqlDatabaseUserName = "lunaagentuser",

    [string]$sqlDatabaseUserPassword = "default",

    [string]$sqlFirewallRuleStartIp = "default",

    [string]$sqlFirewallRuleEndIp = "default",

    [string]$headerBackgroundColor = "#3376CD",

    [string]$homepageTitle = "Luna AI Agent"
)

function GetNameForAzureResources{
    param($uniqueName, $defaultName, $resourceTypeSuffix)
    if ($defaultName -ne "default"){
        return $defaultName
    }

    return $uniqueName + $resourceTypeSuffix
}

function GetPassword{
    $psw = ("#%0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ_abcdefghijklmnopqrstuvwxyz".tochararray() | Sort-Object {Get-Random})[0..21] -join ''
    return $psw + "3Fd"
}

if ($folder -ne "default"){
    $apiFolder = $folder + "/api"
    Push-Location -Path $apiFolder
}
else{
    Push-Location "Luna.Agent"
}

$resourceGroupName = GetNameForAzureResources -defaultName $resourceGroupName -resourceTypeSuffix "-rg" -uniqueName $name
$webAppServicePlanName = GetNameForAzureResources -defaultName $webAppServicePlanName -resourceTypeSuffix "-serviceplan" -uniqueName $name
$agentApiWebAppName = GetNameForAzureResources -defaultName $agentApiWebAppName -resourceTypeSuffix "-api" -uniqueName $name
$agentPortalWebAppName = GetNameForAzureResources -defaultName $agentPortalWebAppName -resourceTypeSuffix "-portal" -uniqueName $name
$keyVaultName = GetNameForAzureResources -defaultName $keyVaultName -resourceTypeSuffix "-keyvault" -uniqueName $name
$sqlServerName = GetNameForAzureResources -defaultName $sqlServerName -resourceTypeSuffix "-sqlserver" -uniqueName $name
$sqlDbName = GetNameForAzureResources -defaultName $sqlDbName -resourceTypeSuffix "-sqldb" -uniqueName $name

if ($sqlAdminPassword -eq 'default'){
    $sqlAdminPassword = GetPassword
    $quotedSqlAdminPassword = "'" + $sqlAdminPassword + "'"
    $sqlAdminPassword
}
if ($sqlDatabaseUserPassword -eq 'default'){
    $sqlDatabaseUserPassword = GetPassword
    $quotedqlDatabaseUserPassword = "'" + $sqlDatabaseUserPassword + "'"
    $sqlDatabaseUserPassword
}

#create AAD application

if ($agentApiAADApplicationId -eq 'default'){

    $resouceAccesses = '[{\"resourceAppId\":\"00000002-0000-0000-c000-000000000000\",\"resourceAccess\":[{\"id\":\"a42657d6-7f20-40e3-b6f0-cee03008a62a\",\"type\":\"Scope\"}]}]'
    $appDisplayName = $name + '-aadapplication-apiapp'
    $replyUrls = "https://"+ $agentPortalWebAppName +".azurewebsites.net"

    $aadApp = az ad app create --display-name $appDisplayName --reply-urls $replyUrls --available-to-other-tenants false --required-resource-accesses $resouceAccesses | ConvertFrom-Json

    $agentApiAADApplicationId = $aadApp.appId
}


#create resource group
az group create --location $location --name $resourceGroupName

#create sql server and database
az sql server create --resource-group $resourceGroupName --location $location --name $sqlServerName --admin-user $sqlAdminUser --admin-password $sqlAdminPassword

az sql server firewall-rule create --resource-group $resourceGroupName --server $sqlServerName -n "AllowAzureServiceAccess" --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0

if ($sqlFirewallRuleStartIp -ne 'default' -and $sqlFirewallRuleEndIp -ne 'default'){
    az sql server firewall-rule create --resource-group $resourceGroupName --server $sqlServerName -n "AllowClientAccess" --start-ip-address $sqlFirewallRuleStartIp --end-ip-address $sqlFirewallRuleEndIp
}

az sql db create --resource-group $resourceGroupName --server $sqlServerName --name $sqlDbName --service-objective S0

$sqlDatabaseUsernameVar = "username='" + $sqlDatabaseUserName + "'"
$sqlDatabasePasswordVar = "password='" + $sqlDatabaseUserPassword + "'"
$adminObjectId =  (az ad signed-in-user show | ConvertFrom-Json).objectId
$adminUserName = (az ad signed-in-user show | ConvertFrom-Json).userPrincipalName
$adminObjectIdVar = "adminAADObjectId='" + $adminObjectId + "'"
$adminUserNameVar = "adminUserName='" + $adminUserName + "'"

$variables = $sqlDatabaseUsernameVar, $sqlDatabasePasswordVar, $adminObjectIdVar, $adminUserNameVar

$sqlServerInstanceName = $sqlServerName + ".database.windows.net"

Invoke-Sqlcmd -ServerInstance $sqlServerInstanceName -Username $sqlAdminUser -Password $sqlAdminPassword -Database $sqlDbName -Variable $variables -InputFile "init.sql"


#create key vault
az keyvault create --resource-group $resourceGroupName --location $location --name $keyVaultName --enable-soft-delete false

az appservice plan create -n $webAppServicePlanName -g $resourceGroupName -l $location --is-linux --sku $webAppServicePlanSku


az webapp up -n $agentApiWebAppName -p $webAppServicePlanName -g $resourceGroupName -l $location

az webapp config set -n $agentApiWebAppName --startup-file startup.sh

Write-Host "enable managed identity"
az webapp identity assign -g $resourceGroupName -n $agentApiWebAppName

$setting = 'KEY_VAULT_NAME=' + $keyVaultName 
az webapp config appsettings set -n $agentApiWebAppName --settings $setting

$setting = 'AGENT_MODE=LOCAL' 
az webapp config appsettings set -n $agentApiWebAppName --settings $setting

$odbcConnectionString = "mssql+pyodbc://" + $sqlDatabaseUserName + ":" + $sqlDatabaseUserPassword + "@" + $sqlServerInstanceName + ":1433/" + $sqlDbName + "?driver=ODBC+Driver+17+for+SQL+Server"
$setting = 'ODBC_CONNECTION_STRING="'+$odbcConnectionString+'"'
az webapp config appsettings set -n $agentApiWebAppName --settings $setting

$agentId = (New-Guid).toString()
$setting = 'AGENT_ID='+$agentId
az webapp config appsettings set -n $agentApiWebAppName --settings $setting

$agentKey = GetPassword
$setting = 'AGENT_KEY="'+$agentKey+'"'
az webapp config appsettings set -n $agentApiWebAppName --settings $setting

$setting = 'AGENT_API_ENDPOINT=' + "https://"+ $agentApiWebAppName +".azurewebsites.net"
az webapp config appsettings set -n $agentApiWebAppName --settings $setting

$setting = 'AAD_VALID_AUDIENCES=' + $agentApiAADApplicationId
az webapp config appsettings set -n $agentApiWebAppName --settings $setting

$tenantId = (az account show | ConvertFrom-Json).tenantId
$setting = 'AAD_TOKEN_ISSUER=https://login.microsoftonline.com/' + $tenantId + "/v2.0"

az webapp config appsettings set -n $agentApiWebAppName --settings $setting

$webappIdentity = az webapp identity show --name $agentApiWebAppName --resource-group $resourceGroupName | ConvertFrom-Json

az keyvault set-policy --name $keyVaultName --secret-permissions get list set delete --object-id $webappIdentity.principalId

if ($folder -ne "default"){
    Pop-Location
    $portalFolder = $folder + "/portal"
    Push-Location -Path $portalFolder
}
else{
    Pop-Location
    Push-Location "/Luna.Agent.Portal/build"
}

az webapp create -n $agentPortalWebAppName -p $webAppServicePlanName -g $resourceGroupName --runtime "node|10.14"

az webapp config set -n $agentPortalWebAppName -g $resourceGroupName --startup-file "pm2 serve /home/site/wwwroot --no-daemon –spa"

az webapp up -n $agentPortalWebAppName -p $webAppServicePlanName -g $resourceGroupName


$config = 'var BASE_URL = "https://'+$agentApiWebAppName+'.azurewebsites.net/api/management";
var HEADER_HEX_COLOR = "'+$headerBackgroundColor+'";
var SITE_TITLE = "'+$homepageTitle+'";
var MSAL_CONFIG = {
  appId: "'+$agentApiAADApplicationId+'",
  redirectUri: "https://'+$agentPortalWebAppName+'.azurewebsites.net/",
  scopes: [
    "user.read",
    "User.ReadBasic.All"
  ]
};'


$config | Out-File "appConfig.js"

if ($folder -ne "default"){
    Pop-Location
    $zipFileName = $folder+".zip"
    Remove-Item $zipFileName -Force -ErrorAction SilentlyContinue
    Remove-Item $folder -Force -Recurse -ErrorAction SilentlyContinue
}