﻿param (
    [Parameter(Mandatory=$true)]
    [string]$uniqueName = "default", 

    [Parameter(Mandatory=$true)]
    [string]$location = "centralus",

    [string]$publisherMicrosoftid = "default",

    [string]$tenantId = "default",

    [string]$accountId = "default",
      
    [string]$lunaServiceSubscriptionId = "default",
    
    [string]$userApplicationSubscriptionId = "default", 

    [string]$resourceGroupName = "default", 
    
    [string]$keyVaultName = "default",
    
    [string]$sqlServerName = "default",
    
    [string]$sqlDatabaseName = "default",
    
    [string]$StorageName = "default",
    
    [string]$appServicePlanName = "default",
    
    [string]$isvWebAppName = "default",
    
    [string]$enduserWebAppName = "default",

    [string]$controllerWebAppResourceGroupName = "default",
    
    [string]$controllerWebAppServicePlanName = "default",

    [string]$controllerWebAppName = "default",

    [string]$userPortalWebAppName = "default",
    
    [string]$apiWebAppName = "default",

    [string]$apiWebJobName = "default",
    
    [string]$apiWebAppInsightsName = "default",

    [string]$apimName = "default",

    [string]$apimTier = "Developer",

    [string]$apimCapacity = 1,

    [string]$amlWorkspaceName = "default",

    [string]$amlWorkspaceSku = "Basic",

    [string]$azureMarketplaceAADApplicationName = "default",

    [string]$azureMarketplaceAADApplicationId = "00000000-0000-0000-0000-000000000000",

    [string]$azureResourceManagerAADApplicationName = "default",

    [string]$azureResourceManagerAADApplicationId = "00000000-0000-0000-0000-000000000000",

    [string]$webAppAADApplicationName = "default",

    [string]$webAppAADApplicationId = "00000000-0000-0000-0000-000000000000",

    [string]$sqlServerAdminUsername = "cloudsa",

    [int]$keyExpirationByMonth = 12,

    [string]$firewallStartIpAddress = "clientIp",

    [string]$firewallEndIpAddress = "clientIp",

    [string]$buildLocation = "default",

    [string]$sqlScriptFileLocation = "default",

    [string]$companyName = "Microsoft",

    [string]$headerBackgroundColor = "#004578",

    [string]$enableV1 = "true",

    [string]$enableV2 = "true",

    [string]$adminTenantId = "common",

    [string]$adminAccounts = "default"

)

Clear-AzContext -Force

if($tenantId -ne "default"){
    Connect-AzureAD -TenantId $tenantId
    
    Connect-AzAccount -Tenant $tenantId
}
else{
    Connect-AzureAD
    Connect-AzAccount
}

az login --only-show-errors

function GetPassword{
    $psw = ("#%0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ_abcdefghijklmnopqrstuvwxyz".tochararray() | Sort-Object {Get-Random})[0..21] -join ''
    return $psw + "3Fd"
}

function GetNameForAzureResources{
    param($uniqueName, $defaultName, $resourceTypeSuffix)
    if ($defaultName -ne "default"){
        return $defaultName
    }

    return $uniqueName + $resourceTypeSuffix
}

function Create-AzureADApplication{
    param($appName, $appId, $keyVaultName, $replyURLs, $secretName="none", $multiTenant=$False)
    
    $filter = "AppId eq '"+$appId+"'"
    $azureadapp = Get-AzureADApplication -Filter $filter

    ## Create new AAD application if the appId is not specified or application doesn't exist
    if ($azureadapp.Count -eq 0){
        $azureadapp = New-AzureADApplication -DisplayName $appName -AvailableToOtherTenants $multiTenant -ReplyUrls $replyURLs


        $requiredResourceAccess = [Microsoft.Open.AzureAD.Model.RequiredResourceAccess]@{
        ResourceAppId="00000003-0000-0000-c000-000000000000";
        ResourceAccess=[Microsoft.Open.AzureAD.Model.ResourceAccess]@{
            Id = "e1fe6dd8-ba31-4d61-89e7-88639da4683d" ;
            Type = "Scope"
            }
        }
        Set-AzureADApplication -ObjectId $azureadapp.ObjectId -RequiredResourceAccess $requiredResourceAccess
    }
    
    $startDate = Get-Date

    if($secretName -ne "none"){
        $secret = New-AzureADApplicationPasswordCredential -ObjectId $azureadapp.ObjectId -StartDate $startDate -EndDate $startDate.AddMonths($keyExpirationByMonth) -CustomKeyIdentifier "keyfortoken"
        $secretvalue = ConvertTo-SecureString $secret.Value -AsPlainText -Force
        $secretObj = Set-AzKeyVaultSecret -VaultName $keyVaultName -Name $secretName -SecretValue $secretvalue
    }

    return $azureadapp.AppId
}

function GrantKeyVaultAccessToWebApp{
    param($resourceGroupName, $keyVaultName, $webAppName)
    $webapp = Get-AzWebApp -ResourceGroupName $resourceGroupname -Name $webAppName
    Set-AzKeyVaultAccessPolicy -VaultName $keyVaultName -ObjectId $webapp.Identity.PrincipalId -PermissionsToSecrets list,get,set,delete
}

function Get-PublishingProfileCredentials($resourceGroupName, $webAppName){

    $resourceType = "Microsoft.Web/sites/config"

    $resourceName = "$webAppName/publishingcredentials"
    $publishingCredentials = Invoke-AzResourceAction -ResourceGroupName $resourceGroupName -ResourceType $resourceType -ResourceName $resourceName -Action list -ApiVersion "2015-08-01" -Force
    return $publishingCredentials
}

#Pulling authorization access token :
function Get-KuduApiAuthorisationHeaderValue($resourceGroupName, $webAppName){
    $publishingCredentials = Get-PublishingProfileCredentials $resourceGroupName $webAppName
    $publishingCredentials
    return ("Basic {0}" -f [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(("{0}:{1}" -f $publishingCredentials.Properties.PublishingUserName, $publishingCredentials.Properties.PublishingPassword))))
}

function Deploy-WebJob($resourceGroupName, $webAppName, $webJobName, $webJobZipPath){
    $accessToken = (Get-KuduApiAuthorisationHeaderValue $resourceGroupName $webAppName)[-1]

    $tempFileName = "webjob"+(Get-Date).ToString("yyyyMMdd-hhmmss")+".zip"
    $contentDisposition = "attachment; filename="+$tempFileName
    $Header = @{
        "Authorization"=$accessToken
        "Content-Disposition"=$contentDisposition
    }

    $tempFile = "$env:temp\"+$tempFileName

    Invoke-WebRequest -Uri $webJobZipPath -OutFile $tempFile

$apiUrl = "https://" + $webAppName + ".scm.azurewebsites.net/api/triggeredwebjobs/" + $webJobName
$result = Invoke-RestMethod -Uri $apiUrl -Headers $Header -Method put -InFile $tempFile -ContentType 'application/zip' 

}

function UpdateScriptConfigFile($resourceGroupName, $webAppName, $configuration){
    $accessToken = (Get-KuduApiAuthorisationHeaderValue $resourceGroupName $webAppName)[-1]

    $tempFileName = "config.js"
    $Header = @{
        "Authorization"=$accessToken
        "If-Match"="*"
    }

    $tempFilePath = "$env:temp\"+$tempFileName

    $configuration | Out-File $tempFilePath

    $apiUrl = "https://" + $webAppName + ".scm.azurewebsites.net/api/vfs/site/wwwroot/"+$tempFileName

    Invoke-RestMethod -Uri $apiUrl `
                        -Headers $Header `
                        -Method PUT `
                        -InFile $tempFilePath `
                        -ContentType "multipart/form-data"

}

Function NewAzureRoleAssignment($scope, $objectId, $retryCount) {
    $totalRetries = $retryCount
    $retryCount
    While ($True) {
        try {
        
            $assignments = Get-AzRoleAssignment -ObjectId $objectId
            if ($assignments.Count -eq 0){
                New-AzRoleAssignment -ObjectId $objectId -RoleDefinitionName 'Contributor' -Scope $scope -ErrorAction Stop
            }
            return
        }
        catch {
            Write-Host "catch catch" $retryCount
            # The principal could not be found. Maybe it was just created.
            If ($retryCount -eq 0) {
                Write-Error "An error occurred: $($_.Exception)`n$($_.ScriptStackTrace)"
                throw "The principal '$objectId' cannot be granted Contributor role on the subscription. Please make sure the principal exists and try again later."
            }
            $retryCount--
            Write-Warning "  The principal '$objectId' cannot be granted Contributor role on the subscription. Trying again (attempt $($totalRetries - $retryCount)/$totalRetries) after 30 seconds."
            Start-Sleep 30
        }
    }
}


if($lunaServiceSubscriptionId -ne "default"){
    Write-Host $lunaServiceSubscriptionId
    Write-Host $tenantId
    if($tenantId -ne "default"){
        Set-AzContext -Subscription $lunaServiceSubscriptionId -Tenant $tenantId
    }
    else{
        Set-AzContext -Subscription $lunaServiceSubscriptionId
    }
}

$resourceGroupName = GetNameForAzureResources -defaultName $resourceGroupName -resourceTypeSuffix "-rg" -uniqueName $uniqueName
$keyVaultName = GetNameForAzureResources -defaultName $keyVaultName -resourceTypeSuffix "-keyvault" -uniqueName $uniqueName
$sqlServerName = GetNameForAzureResources -defaultName $sqlServerName -resourceTypeSuffix "-sqlserver" -uniqueName $uniqueName
$sqlDatabaseName = GetNameForAzureResources -defaultName $sqlDatabaseName -resourceTypeSuffix "-sqldb" -uniqueName $uniqueName
$StorageName = GetNameForAzureResources -defaultName $StorageName -resourceTypeSuffix "storage" -uniqueName $uniqueName
$appServicePlanName = GetNameForAzureResources -defaultName $appServicePlanName -resourceTypeSuffix "-appsvrplan" -uniqueName $uniqueName
$isvWebAppName = GetNameForAzureResources -defaultName $isvWebAppName -resourceTypeSuffix "-isvapp" -uniqueName $uniqueName
$enduserWebAppName = GetNameForAzureResources -defaultName $enduserWebAppName -resourceTypeSuffix "-userapp" -uniqueName $uniqueName
$apiWebAppName = GetNameForAzureResources -defaultName $apiWebAppName -resourceTypeSuffix "-apiapp" -uniqueName $uniqueName
$apiWebJobName = GetNameForAzureResources -defaultName $apiWebJobName -resourceTypeSuffix "-apiwebjob" -uniqueName $uniqueName
$apiWebAppInsightsName = GetNameForAzureResources -defaultName $apiWebAppInsightsName -resourceTypeSuffix "-apiappinsights" -uniqueName $uniqueName
$apimName = GetNameForAzureResources -defaultName $apimName -resourceTypeSuffix "-apim" -uniqueName $uniqueName
$amlWorkspaceName = GetNameForAzureResources -defaultName $amlWorkspaceName -resourceTypeSuffix "-aml" -uniqueName $uniqueName
$userPortalWebAppName = GetNameForAzureResources -defaultName $userPortalWebAppName -resourceTypeSuffix "-portal" -uniqueName $uniqueName

$controllerWebAppServicePlanName = GetNameForAzureResources -defaultName $controllerWebAppServicePlanName -resourceTypeSuffix "-crlsvrplan" -uniqueName $uniqueName

$controllerWebAppName = GetNameForAzureResources -defaultName $controllerWebAppName -resourceTypeSuffix "-api" -uniqueName $uniqueName

$controllerWebAppResourceGroupName = GetNameForAzureResources -defaultName $controllerWebAppResourceGroupName -resourceTypeSuffix "-linuxrg" -uniqueName $uniqueName

$azureMarketplaceAADApplicationName = GetNameForAzureResources -defaultName $azureMarketplaceAADApplicationName -resourceTypeSuffix "-azuremarketplace-aad" -uniqueName $uniqueName
$azureResourceManagerAADApplicationName = GetNameForAzureResources -defaultName $azureResourceManagerAADApplicationName -resourceTypeSuffix "-azureresourcemanager-aad" -uniqueName $uniqueName
$webAppAADApplicationName = GetNameForAzureResources -defaultName $webAppAADApplicationName -resourceTypeSuffix "-apiapp-aad" -uniqueName $uniqueName

$gatewayId = (New-Guid).ToString()

add-type -AssemblyName System.Web

$sqlServerAdminPasswordRaw = GetPassword
$sqlServerAdminPassword = ConvertTo-SecureString $sqlServerAdminPasswordRaw.ToString() -AsPlainText -Force

if ($buildLocation -eq "default"){
    $buildLocation = "https://github.com/Azure/projectluna/raw/main/Resources/Builds/latest"
}

if ($sqlScriptFileLocation -eq "default"){
    $sqlScriptFileLocation = ".\SqlScripts\latest\db_provisioning.sql"
}

$currentContext = Get-AzContext
if ($accountId -eq "default"){
    $accountId = $currentContext.Account.Id
}

if ($tenantId -eq "default"){
    $tenantId = $currentContext.Tenant.Id
}

if ($userApplicationSubscriptionId -eq "default"){
    $userApplicationSubscriptionId = $currentContext.Subscription.Id
}

$currentUser = Get-AzADUser -UserPrincipalName $accountId


if ($adminAccounts -eq "default"){
    $adminAccounts = $accountId
}

$objectId = $currentUser.Id

Write-Host "Create resource group" $resourceGroupName
New-AzResourceGroup -Name $resourceGroupName -Location $location

Write-Host "Create resource group" $controllerWebAppResourceGroupName
New-AzResourceGroup -Name $controllerWebAppResourceGroupName -Location $location

Write-Host "Deploy ARM template in resource group" $resourceGroupName
$deployAPIM = $enableV2 -eq 'true'
$deployAML = $enableV2 -eq 'true'
New-AzResourceGroupDeployment -ResourceGroupName $resourceGroupName `
                              -TemplateFile .\main.json `
                              -keyVaultName $keyVaultName `
                              -sqlServerName $sqlServerName `
                              -sqlDatabaseName $sqlDatabaseName `
                              -storageAccountName $StorageName `
                              -appServicePlanName $appServicePlanName `
                              -isvWebAppName $isvWebAppName `
                              -enduserWebAppName $enduserWebAppName `
                              -apiWebAppName $apiWebAppName `
                              -apiWebAppInsightsName $apiWebAppInsightsName `
                              -location $location `
                              -sqlAdministratorLoginPassword $sqlServerAdminPassword `
                              -sqlAdministratorUsername $sqlServerAdminUsername `
                              -tenantId $tenantId `
                              -objectId $objectId `
                              -buildLocation $buildLocation `
                              -apimAdminEmail $accountId `
                              -orgName $companyName `
                              -apimName $apimName `
                              -apimTier $apimTier `
                              -apimCapacity $apimCapacity `
                              -deployAPIM $deployAPIM `
                              -workspaceName $amlWorkspaceName `
                              -workspaceSku $amlWorkspaceSku `
                              -deployAML $deployAML `


$filter = "AppId eq '"+$webAppAADApplicationId+"'"
$azureadapp = Get-AzureADApplication -Filter $filter
$isNewApp = $azureadapp.Count -eq 0

Write-Host "Create AAD application for webapp authentication."
$replyUrls = New-Object System.Collections.Generic.List[System.String]
#create AAD application for ISV App
$replyUrl = "https://" + $userPortalWebAppName + ".azurewebsites.net";
$replyUrls.Add($replyUrl)
$replyUrl = "https://" + $isvWebAppName + ".azurewebsites.net";
$replyUrls.Add($replyUrl)
$replyUrl = "https://" + $isvWebAppName + ".azurewebsites.net/Offers";
$replyUrls.Add($replyUrl)
$replyUrl = "https://" + $isvWebAppName + ".azurewebsites.net/Products";
$replyUrls.Add($replyUrl)
$replyUrl = "https://" + $isvWebAppName + ".azurewebsites.net/Subscriptions";
$replyUrls.Add($replyUrl)
$replyUrl = "https://" + $enduserWebAppName + ".azurewebsites.net/LandingPage";
$replyUrls.Add($replyUrl)
$replyUrl = "https://" + $enduserWebAppName + ".azurewebsites.net/Subscriptions";
$replyUrls.Add($replyUrl)

#create AAD application for the API app
$appId = Create-AzureADApplication -appName $webAppAADApplicationName -appId $webAppAADApplicationId -keyVaultName $keyVaultName -secretName "api-app-key" -multiTenant $True -replyURLs $replyUrls
$webAppAADApplicationId = $appId
Write-Host "AAD application created with id" $webAppAADApplicationId

if ($isNewApp){

    Start-Sleep 15

    Write-Host "Creating Service Principal for webapp AAD application."
    New-AzureADServicePrincipal -AppId $webAppAADApplicationId

    $principalId = (Get-AzADServicePrincipal -ApplicationId $webAppAADApplicationId).id
    Write-Host "Service Principal created with id " $principalId
    
    $filter = "AppId eq '"+$webAppAADApplicationId+"'"
    $azureadapp = Get-AzureADApplication -Filter $filter
    
    $identifierUri = 'api://'+$webAppAADApplicationId
    
    Set-AzureADApplication -ObjectId $azureadapp.ObjectId `
    -Oauth2AllowImplicitFlow $True `
    -IdentifierUris $identifierUri
}

#create AAD application for Azure Marketplace auth
Write-Host "Create AAD Application for Azure Marketplace authentication."
$appId = Create-AzureADApplication -appName $azureMarketplaceAADApplicationName -appId $azureMarketplaceAADApplicationId -keyVaultName $keyVaultName -secretName "amp-app-key" -multiTenant $False
$azureMarketplaceAADApplicationId = $appId
Write-Host "AAD application created with id " $azureMarketplaceAADApplicationId

#create AAD application for ARM auth

$filter = "AppId eq '"+$azureResourceManagerAADApplicationId+"'"
$azureadapp = Get-AzureADApplication -Filter $filter
$isNewApp = $azureadapp.Count -eq 0

Write-Host "Creating AAD Application for Azure Resource Manager authentication."
$appId = Create-AzureADApplication -appName $azureResourceManagerAADApplicationName -appId $azureResourceManagerAADApplicationId -keyVaultName $keyVaultName -secretName "arm-app-key" -multiTenant $False
$azureResourceManagerAADApplicationId = $appId
Write-Host "AAD application created with id" $azureResourceManagerAADApplicationId

if ($isNewApp){
    Start-Sleep 15
    
    Write-Host "Creating Service Principal for Azure Resource Manager AAD application."
    New-AzureADServicePrincipal -AppId $azureResourceManagerAADApplicationId
    
    $principalId = (Get-AzADServicePrincipal -ApplicationId $azureResourceManagerAADApplicationId).id
    Write-Host "Service principal is created with id " $principalId
    
    Write-Host "Assign subscription contribution role to the service principal."
    $scope = '/subscriptions/'+$userApplicationSubscriptionId
    NewAzureRoleAssignment -objectId $principalId -scope $scope -retryCount 10
    
    Write-Host "Assign contribution role on the AML workspace to the service principal."
    $scope = '/subscriptions/'+$userApplicationSubscriptionId+'/resourceGroups/'+$resourceGroupName+'/providers/Microsoft.MachineLearningServices/workspaces/'+$amlWorkspaceName
    NewAzureRoleAssignment -objectId $principalId -scope $scope -retryCount 10
}


Write-Host "Adding client ip to the SQL Server firewall rule"
if ($firewallStartIpAddress -ne "clientIp" -or $firewallEndIpAddress -ne "clientIp"){

    $firewallRuleName = "deploymentClientVPN"+(Get-Date).ToString("yyyyMMdd-hhmmss")
    New-AzSqlServerFirewallRule -ResourceGroupName $resourceGroupName -ServerName $sqlServerName -FirewallRuleName $firewallRuleName -StartIpAddress $firewallStartIpAddress -EndIpAddress $firewallEndIpAddress

}

$clientIp = (Invoke-WebRequest -uri "http://ifconfig.me/ip").Content
$firewallStartIpAddress = $clientIp
$firewallEndIpAddress = $clientIp

$firewallRuleName = "deploymentClient"+(Get-Date).ToString("yyyyMMdd-hhmmss")

New-AzSqlServerFirewallRule -ResourceGroupName $resourceGroupName -ServerName $sqlServerName -FirewallRuleName $firewallRuleName -StartIpAddress $firewallStartIpAddress -EndIpAddress $firewallEndIpAddress

Write-Host "Execute SQL script to create database user and default gateway entry."
$sqlDatabaseUserName = "lunauser" + $uniqueName
$sqlDatabaseUsernameVar = "username='" + $sqlDatabaseUserName + "'"

$sqlDatabasePassword = GetPassword
$sqlDatabasePasswordVar = "password='" + $sqlDatabasePassword + "'"
$publisherIdVar = "publisherId='" + $publisherId + "'"

$endpointUrl = "https://"+ $controllerWebAppName +".azurewebsites.net"
$endpointUrlVar = "endpointUrl='" + $endpointUrl + "'"
$gatewayIdVar = "gatewayId='" + $gatewayId + "'"

$accountIdVar = "gatewayOwner='" + $accountId + "'"

$variables = $sqlDatabaseUsernameVar, $sqlDatabasePasswordVar, $endpointUrlVar, $gatewayIdVar, $accountIdVar

Write-Host $variables

$sqlServerInstanceName = $sqlServerName + ".database.windows.net"
$sqlServerInstanceName
Invoke-Sqlcmd -ServerInstance $sqlServerInstanceName -Username $sqlServerAdminUsername -Password $sqlServerAdminPasswordRaw -Database $sqlDatabaseName -Variable $variables -InputFile $sqlScriptFileLocation

Write-Host "Store storage account key to Azure Key Vault."
$key = (Get-AzStorageAccountKey -ResourceGroupName $resourceGroupName -Name $StorageName)| Where-Object {$_.KeyName -eq "key1"}

$secretvalue = ConvertTo-SecureString $key.Value -AsPlainText -Force
Set-AzKeyVaultSecret -VaultName $keyVaultName -Name 'storage-key' -SecretValue $secretvalue

Write-Host "Store SQL connection string to Azure key vault"
$connectionString = "Server=tcp:" + $sqlServerInstanceName + ",1433;Initial Catalog=" + $sqlDatabaseName + ";Persist Security Info=False;User ID=" + $sqlDatabaseUserName + ";Password='" + $sqlDatabasePassword + "';MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

$odbcConnectionString = "mssql+pyodbc://" + $sqlDatabaseUserName + ":" + $sqlDatabasePassword + "@" + $sqlServerInstanceName + ":1433/" + $sqlDatabaseName + "?driver=ODBC+Driver+17+for+SQL+Server"
Write-Host $odbcConnectionString

$secretvalue = ConvertTo-SecureString $connectionString -AsPlainText -Force
Set-AzKeyVaultSecret -VaultName $keyVaultName -Name 'connection-string' -SecretValue $secretvalue

$apimTenantAccessId = 'integration'
$controllerBaseUrl = "https://"+ $controllerWebAppName +".azurewebsites.net"

Write-Host "Update app settings"
$appsettings = @{}
$appsettings["SecuredCredentials:VaultName"] = $keyVaultName;
$appsettings["SecuredCredentials:StorageAccount:Config:AccountName"] = $StorageName;
$appsettings["SecuredCredentials:StorageAccount:Config:VaultName"] = $keyVaultName;
$appsettings["SecuredCredentials:Database:DatabaseName"] = $sqlDatabaseName;
$appsettings["SecuredCredentials:ResourceManager:AzureActiveDirectory:VaultName"] = $keyVaultName;
$appsettings["SecuredCredentials:ResourceManager:AzureActiveDirectory:ClientId"] = $azureResourceManagerAADApplicationId;
$appsettings["SecuredCredentials:ResourceManager:AzureActiveDirectory:TenantId"] = $tenantId;
$appsettings["SecuredCredentials:Marketplace:AzureActiveDirectory:VaultName"] = $keyVaultName;
$appsettings["SecuredCredentials:Marketplace:AzureActiveDirectory:ClientId"] = $azureMarketplaceAADApplicationId;
$appsettings["SecuredCredentials:Marketplace:AzureActiveDirectory:TenantId"] = $tenantId;
$appsettings["AzureAD:ClientId"] = $webAppAADApplicationId;
$appsettings["AzureAD:TenantId"] = $tenantId;
$appsettings["ISVPortal:AdminAccounts"] = $adminAccounts;
$appsettings["ISVPortal:AdminTenant"] = $adminTenantId;

$appsettings["SecuredCredentials:Azure:Config:VaultName"] = $keyVaultName;
$appsettings["SecuredCredentials:Azure:Config:ControllerBaseUrl"] = $controllerBaseUrl;

$appInsightsApp = Get-AzApplicationInsights -ResourceGroupName $resourceGroupName -name $apiWebAppInsightsName
$appsettings["ApplicationInsights:InstrumentationKey"] = $appInsightsApp.InstrumentationKey;
$appsettings["WebJob:APIServiceUrl"] = "https://" + $apiWebAppName + ".azurewebsites.net/api";
$appsettings["LunaClient:BaseUri"]="https://" + $apiWebAppName + ".azurewebsites.net/api";

Set-AzWebApp -ResourceGroupName $resourceGroupName -Name $apiWebAppName -AppSettings $appsettings

$setting = "SecuredCredentials:VaultName=" +  $keyVaultName;
az webapp config appsettings set --resource-group $resourceGroupName --name $apiWebAppName --settings $setting

$setting = "SecuredCredentials:StorageAccount:Config:AccountName=" +  $StorageName;
az webapp config appsettings set --resource-group $resourceGroupName --name $apiWebAppName --settings $setting

$setting = "SecuredCredentials:StorageAccount:Config:VaultName=" +  $keyVaultName;
az webapp config appsettings set --resource-group $resourceGroupName --name $apiWebAppName --settings $setting

$setting = "SecuredCredentials:Database:DatabaseName=" +  $sqlDatabaseName;
az webapp config appsettings set --resource-group $resourceGroupName --name $apiWebAppName --settings $setting

$setting = "SecuredCredentials:ResourceManager:AzureActiveDirectory:VaultName=" +  $keyVaultName;
az webapp config appsettings set --resource-group $resourceGroupName --name $apiWebAppName --settings $setting

$setting = "SecuredCredentials:ResourceManager:AzureActiveDirectory:ClientId=" +  $azureResourceManagerAADApplicationId;
az webapp config appsettings set --resource-group $resourceGroupName --name $apiWebAppName --settings $setting

$setting = "SecuredCredentials:ResourceManager:AzureActiveDirectory:TenantId=" +  $tenantId;
az webapp config appsettings set --resource-group $resourceGroupName --name $apiWebAppName --settings $setting

$setting = "SecuredCredentials:Marketplace:AzureActiveDirectory:VaultName=" +  $keyVaultName;
az webapp config appsettings set --resource-group $resourceGroupName --name $apiWebAppName --settings $setting

$setting = "SecuredCredentials:Marketplace:AzureActiveDirectory:ClientId=" +  $azureMarketplaceAADApplicationId;
az webapp config appsettings set --resource-group $resourceGroupName --name $apiWebAppName --settings $setting

$setting = "SecuredCredentials:Marketplace:AzureActiveDirectory:TenantId=" +  $tenantId;
az webapp config appsettings set --resource-group $resourceGroupName --name $apiWebAppName --settings $setting

$setting = "AzureAD:ClientId=" +  $webAppAADApplicationId;
az webapp config appsettings set --resource-group $resourceGroupName --name $apiWebAppName --settings $setting

$setting = "AzureAD:TenantId=" +  $tenantId;
az webapp config appsettings set --resource-group $resourceGroupName --name $apiWebAppName --settings $setting

$setting = "ISVPortal:AdminAccounts=" +  $adminAccounts;
az webapp config appsettings set --resource-group $resourceGroupName --name $apiWebAppName --settings $setting

$setting = "ISVPortal:AdminTenant=" +  $adminTenantId;
az webapp config appsettings set --resource-group $resourceGroupName --name $apiWebAppName --settings $setting

$setting = "SecuredCredentials:Azure:Config:VaultName=" +  $keyVaultName;
az webapp config appsettings set --resource-group $resourceGroupName --name $apiWebAppName --settings $setting

$setting = "SecuredCredentials:Azure:Config:ControllerBaseUrl=" +  $controllerBaseUrl;
az webapp config appsettings set --resource-group $resourceGroupName --name $apiWebAppName --settings $setting

$setting = "ApplicationInsights:InstrumentationKey=" +  $appInsightsApp.InstrumentationKey;
az webapp config appsettings set --resource-group $resourceGroupName --name $apiWebAppName --settings $setting

$setting = "WebJob:APIServiceUrl=" +  "https://" + $apiWebAppName + ".azurewebsites.net/api";
az webapp config appsettings set --resource-group $resourceGroupName --name $apiWebAppName --settings $setting

$setting = "LunaClient:BaseUri=" +  "https://" + $apiWebAppName + ".azurewebsites.net/api";
az webapp config appsettings set --resource-group $resourceGroupName --name $apiWebAppName --settings $setting

$config = 'var Configs = {
    API_ENDPOINT: "https://'+ $apiWebAppName +'.azurewebsites.net/api/",
    ISV_NAME: "'+$companyName+'",
    AAD_APPID: "'+$webAppAADApplicationId+'",
    AAD_ENDPOINT: "https://'+$isvWebAppName+'.azurewebsites.net",
    HEADER_BACKGROUND_COLOR: "'+$headerBackgroundColor+'",
    ENABLE_V1: "'+$enableV1+'",
    ENABLE_V2: "'+$enableV2+'"
}'

UpdateScriptConfigFile -resourceGroupName $resourceGroupName -webAppName $isvWebAppName -configuration $config

$config = 'var Configs = {
    API_ENDPOINT: "https://'+ $apiWebAppName +'.azurewebsites.net/api/",
    ISV_NAME: "'+$companyName+'",
    AAD_APPID: "'+$webAppAADApplicationId+'",
    AAD_ENDPOINT: "https://'+$enduserWebAppName+'.azurewebsites.net",
    HEADER_BACKGROUND_COLOR: "'+$headerBackgroundColor+'",
    ENABLE_V1: "'+$enableV1+'",
    ENABLE_V2: "'+$enableV2+'"
}'

UpdateScriptConfigFile -resourceGroupName $resourceGroupName -webAppName $enduserWebAppName -configuration $config


Write-Host "Deploy webjob."
$webjobZipPath = $buildLocation + "/webjob.zip"
Deploy-WebJob -resourceGroupName $resourceGroupName -webAppName $apiWebAppName -webJobName $apiWebJobName -webJobZipPath $webjobZipPath

Write-Host "Create Linux app plan"
az account set -s $lunaServiceSubscriptionId

az appservice plan create -n $controllerWebAppServicePlanName -g $controllerWebAppResourceGroupName -l $location --is-linux --sku p1v2

Write-Host "Deploy portal app."

$buildFolder = "tempBuild"
$zipFileName = "tempBuild.zip"

$userPortalZipPath = $buildLocation + "/portalApp.zip"

Invoke-WebRequest -Uri $userPortalZipPath -OutFile $zipFileName
Expand-Archive -LiteralPath $zipFileName -DestinationPath $buildFolder

Push-Location -Path $buildFolder

$config = 'var BASE_URL = "https://'+$apiWebAppName+'.azurewebsites.net/api";
var HEADER_HEX_COLOR = "'+$headerBackgroundColor+'";
var SITE_TITLE = "'+$companyName+'";
var MSAL_CONFIG = {
  appId: "'+$webAppAADApplicationId+'",
  redirectUri: "https://'+$userPortalWebAppName+'.azurewebsites.net/",
  scopes: [
    "user.read",
    "User.ReadBasic.All"
  ]
};'

$config | Out-File "appConfig.js"

az webapp create -n $userPortalWebAppName -p $controllerWebAppServicePlanName -g $controllerWebAppResourceGroupName --runtime 'node"|"10.14'

az webapp config set -n $userPortalWebAppName -g $controllerWebAppResourceGroupName --startup-file "start.sh"

az webapp up -n $userPortalWebAppName -p $controllerWebAppServicePlanName -g $controllerWebAppResourceGroupName -l $location --only-show-errors

Pop-Location

Remove-Item $zipFileName -Force -ErrorAction SilentlyContinue
Remove-Item $buildFolder -Force -Recurse -ErrorAction SilentlyContinue


Write-Host "Deploy controller web app"

Push-Location -Path '..\..\src\luna.Agent\luna.agent'

az webapp up -n $controllerWebAppName -p $controllerWebAppServicePlanName -g $controllerWebAppResourceGroupName -l $location --only-show-errors

az webapp config set -n $controllerWebAppName --startup-file startup.sh

Write-Host "enable managed identity"
az webapp identity assign -g $controllerWebAppResourceGroupName -n $controllerWebAppName

$setting = 'KEY_VAULT_NAME=' + $keyVaultName 
az webapp config appsettings set -n $controllerWebAppName --settings $setting
$setting = 'AGENT_MODE=SAAS' 
az webapp config appsettings set -n $controllerWebAppName --settings $setting
$setting = 'ODBC_CONNECTION_STRING="'+$odbcConnectionString+'"'
az webapp config appsettings set -n $controllerWebAppName --settings $setting
$setting = 'AGENT_API_ENDPOINT=' + "https://"+ $controllerWebAppName +".azurewebsites.net"
az webapp config appsettings set -n $controllerWebAppName --settings $setting
$setting = 'AAD_VALID_AUDIENCES=' + $webAppAADApplicationId
az webapp config appsettings set -n $controllerWebAppName --settings $setting
$setting = 'AAD_TOKEN_ISSUER=https://login.microsoftonline.com/' + $tenantId + "/v2.0"
az webapp config appsettings set -n $controllerWebAppName --settings $setting

$setting = 'APPINSIGHTS_INSTRUMENTATIONKEY='+$appInsightsApp.InstrumentationKey
az webapp config appsettings set -n $controllerWebAppName --settings $setting

Pop-Location

Write-Host "Update CORS"

$corsUrl = "https://" + $isvWebAppName + ".azurewebsites.net"
az webapp cors add -g $resourceGroupName -n $apiWebAppName --allowed-origins $corsUrl
$corsUrl = "https://" + $enduserWebAppName + ".azurewebsites.net"
az webapp cors add -g $resourceGroupName -n $apiWebAppName --allowed-origins $corsUrl
$corsUrl = "https://" + $userPortalWebAppName + ".azurewebsites.net"
az webapp cors add -g $resourceGroupName -n $apiWebAppName --allowed-origins $corsUrl

az webapp cors add -g $controllerWebAppResourceGroupName -n $controllerWebAppName --allowed-origins "*"

Pop-Location

#restart the API app
Start-Sleep 30
Restart-AzWebApp -ResourceGroupName $resourceGroupName -Name $apiWebAppName

#grant key vault access to API app
Start-Sleep 30
Write-Host "Grant key vault access to API app"
GrantKeyVaultAccessToWebApp -resourceGroupName $resourceGroupName -keyVaultName $keyVaultName -webAppName $apiWebAppName
GrantKeyVaultAccessToWebApp -resourceGroupName $controllerWebAppResourceGroupName -keyVaultName $keyVaultName -webAppName $controllerWebAppName

Write-Host "Deployment finished successfully."

Write-Host "You will need the following information when creating a SaaS offer in Azure Partner Center:"
$landingPageUrl = "https://" + $enduserWebAppName + ".azurewebsites.net/LandingPage";
Write-Host "Landing page URL: " $landingPageUrl
$connectionWebhook = "https://"+ $apiWebAppName +".azurewebsites.net/Webhook"
Write-Host "Connection Webhook: " $connectionWebhook
Write-Host "Azure Active Directory tenant ID: " $tenantId
Write-Host "Azure Active Directory application ID: " $azureMarketplaceAADApplicationId
