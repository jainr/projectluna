param (
    [Alias("s")]
    [string[]]$services = @('gateway','partner','publish','rbac','routing','gallery','pubsub','provision'), 
	
    [Alias("n")]
	[switch] $deployNew = $false,
	
    [Alias("c")]
	[switch] $cleanUp = $false,
	
    [Alias("p")]
	[switch] $publishLocalSettings = $false
)

New-Item -ItemType Directory -Force -Path testbuild

$servicesWithSwaggerConfiged = @('partner','publish','rbac','gallery','pubsub','gateway')

$config = ([xml](Get-Content build.config)).config

$subscription = az account subscription show --id $config.subscriptionId --only-show-errors | ConvertFrom-Json

if (-not $subscription){
	az login --only-show-errors
}

if ($cleanUp){
	$keyVaultName = $config.namePrefix + "-keyvault"
	az group delete -n $config.resourceGroupName --subscription $config.subscriptionId --yes
	az keyvault purge --name $keyVaultName --subscription $config.subscriptionId
	exit 1
}

push-location "common\swagger"
dotnet clean --configuration Release /property:GenerateFullPaths=true /consoleloggerparameters:NoSummary
dotnet build --configuration Release /property:GenerateFullPaths=true /consoleloggerparameters:NoSummary
pop-location

foreach ($service in $services) {
	
	$filePath = $service + "\functions"
	$zipFilePath = "..\..\testbuild\" + $service + "fx.zip"
	
	push-location $filePath
	
	dotnet clean --configuration Release /property:GenerateFullPaths=true /consoleloggerparameters:NoSummary
	dotnet publish --configuration Release /property:GenerateFullPaths=true /consoleloggerparameters:NoSummary

	Remove-Item -Path $zipFilePath -Force
	Start-Sleep -s 1
	Compress-Archive -Path .\bin\Release\netcoreapp3.1\publish\* -DestinationPath $zipFilePath -Force
	
	pop-location
	
	if ($servicesWithSwaggerConfiged.Contains($service)){
		push-location "common\swagger\bin\Release\netcoreapp3.1"
		$argements = "-r -s " + $service
		& '.\SwaggerGenerator.exe' $argements.Split(" ")
		pop-location
	}
	
	if (-not $deployNew){
		$functionAppName = $config.namePrefix + "-" + $service
		$deploymentZipPath = "testbuild\" + $service + "fx.zip"
		az webapp deployment source config-zip -g $config.resourceGroupName -n $functionAppName -t 360 --src $deploymentZipPath --subscription $config.subscriptionId
	}
	
}

if ($deployNew) {
	copy .\deploy\jq.exe .\testbuild -Force
	copy .\deploy\deployment.sh .\testbuild -Force
	copy .\deploy\lunadb.sql .\testbuild -Force
	
	push-location testbuild
	
	./deployment.sh -s $config.subscriptionId -r $config.resourceGroupName -l $config.region -n $config.namePrefix -q $config.sqlUserName -p $config.sqlPassword -t $config.tenantId -c $config.clientId -x $config.clientSecret -a $config.adminUserId -u $config.adminUserName -w $config.createNewResource -m $config.useManagedIdentity -e $config.marketplaceTenantId -i $config.marketplaceClientId -y $config.marketplaceClientSecret

	pop-location
	
}

if ($publishLocalSettings) {
	
	$keyVaultName = $config.namePrefix + "-keyvault"
	
	$storageName = $config.namePrefix + "storage"
	$storageAccount = az storage account show-connection-string -g $config.resourceGroupName -n $storageName | ConvertFrom-Json
	$storageConnectionString = $storageAccount.connectionString
	
	$sqlConnectionSring = "Server=tcp:" + $config.namePrefix + "-sqlserver.database.windows.net,1433;Initial Catalog=" + $config.namePrefix + "-sqldb;Persist Security Info=False;User ID=" + $config.sqlUserName + ";Password=" + $config.sqlPassword + ";MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
	
	$gatewayFxAppName = $config.namePrefix + "-gateway"
	$settings = az functionapp config appsettings list  -g $config.resourceGroupName -n $gatewayFxAppName | ConvertFrom-Json
	$keySetting = $settings | where { $_.name -eq "ENCRYPTION_ASYMMETRIC_KEY"}
	$encryptionKey = $keySetting.value

	$rbacFxUrl = "https://" + $config.namePrefix + "-rbac.azurewebsites.net/api/"
	$publishFxUrl = "https://" + $config.namePrefix + "-publish.azurewebsites.net/api/"
	$partnerFxUrl = "https://" + $config.namePrefix + "-partner.azurewebsites.net/api/"
	$pubsubFxUrl = "https://" + $config.namePrefix + "-pubsub.azurewebsites.net/api/"
	$routingFxUrl = "https://" + $config.namePrefix + "-routing.azurewebsites.net/api/"
	$galleryFxUrl = "https://" + $config.namePrefix + "-gallery.azurewebsites.net/api/"

	$rbacFxAppName = $config.namePrefix + "-rbac"
	$rbacFx = az functionapp keys list -g $config.resourceGroupName -n $rbacFxAppName | ConvertFrom-Json
	$rbacFxKey = $rbacFx.functionKeys.default

	$publishFxAppName = $config.namePrefix + "-publish"
	$publishFx = az functionapp keys list -g $config.resourceGroupName -n $publishFxAppName | ConvertFrom-Json
	$publishFxKey = $publishFx.functionKeys.default

	$partnerFxAppName = $config.namePrefix + "-partner"
	$partnerFx = az functionapp keys list -g $config.resourceGroupName -n $partnerFxAppName | ConvertFrom-Json
	$partnerFxKey = $partnerFx.functionKeys.default

	$pubsubFxAppName = $config.namePrefix + "-pubsub"
	$pubsubFx = az functionapp keys list -g $config.resourceGroupName -n $pubsubFxAppName | ConvertFrom-Json
	$pubsubFxKey = $pubsubFx.functionKeys.default
	
	$galleryFxAppName = $config.namePrefix + "-gallery"
	$galleryFx = az functionapp keys list -g $config.resourceGroupName -n $galleryFxAppName | ConvertFrom-Json
	$galleryFxKey = $galleryFx.functionKeys.default
	
	$publishServiceConfig = Get-Content .\localSettingTemplate.json | Out-String | ConvertFrom-Json
	$publishServiceConfig.Values | add-member -name "AzureWebJobsStorage" -value $storageConnectionString -MemberType NoteProperty
	$publishServiceConfig.Values | add-member -name "PUBSUB_SERVICE_BASE_URL" -value $pubsubFxUrl -MemberType NoteProperty
	$publishServiceConfig.Values | add-member -name "PUBSUB_SERVICE_KEY" -value $pubsubFxKey -MemberType NoteProperty	
	$publishServiceConfig.Values | add-member -name "SQL_CONNECTION_STRING" -value $sqlConnectionSring -MemberType NoteProperty	
	$publishServiceConfig.Values | add-member -name "KEY_VAULT_NAME" -value $keyVaultName -MemberType NoteProperty	
	$publishServiceConfig | ConvertTo-Json -depth 3 | Out-File .\publish\functions\local.settings.json
	
	$gatewayServiceConfig = Get-Content .\localSettingTemplate.json | Out-String | ConvertFrom-Json
	$gatewayServiceConfig.Values | add-member -name "AzureWebJobsStorage" -value $storageConnectionString -MemberType NoteProperty
	$gatewayServiceConfig.Values | add-member -name "PUBSUB_SERVICE_BASE_URL" -value $pubsubFxUrl -MemberType NoteProperty
	$gatewayServiceConfig.Values | add-member -name "PUBSUB_SERVICE_KEY" -value $pubsubFxKey -MemberType NoteProperty	
	$gatewayServiceConfig.Values | add-member -name "RBAC_SERVICE_BASE_URL" -value $rbacFxUrl -MemberType NoteProperty
	$gatewayServiceConfig.Values | add-member -name "RBAC_SERVICE_KEY" -value $rbacFxKey -MemberType NoteProperty	
	$gatewayServiceConfig.Values | add-member -name "PUBLISH_SERVICE_BASE_URL" -value $publishFxUrl -MemberType NoteProperty
	$gatewayServiceConfig.Values | add-member -name "PUBLISH_SERVICE_KEY" -value $publishFxKey -MemberType NoteProperty	
	$gatewayServiceConfig.Values | add-member -name "PARTNER_SERVICE_BASE_URL" -value $partnerFxUrl -MemberType NoteProperty
	$gatewayServiceConfig.Values | add-member -name "PARTNER_SERVICE_KEY" -value $partnerFxKey -MemberType NoteProperty	
	$gatewayServiceConfig.Values | add-member -name "GALLERY_SERVICE_BASE_URL" -value $galleryFxUrl -MemberType NoteProperty
	$gatewayServiceConfig.Values | add-member -name "GALLERY_SERVICE_KEY" -value $galleryFxKey -MemberType NoteProperty	
	$gatewayServiceConfig.Values | add-member -name "ENCRYPTION_ASYMMETRIC_KEY" -value $encryptionKey -MemberType NoteProperty	
	$gatewayServiceConfig | ConvertTo-Json -depth 3 | Out-File .\gateway\functions\local.settings.json
	
	$partnerServiceConfig = Get-Content .\localSettingTemplate.json | Out-String | ConvertFrom-Json
	$partnerServiceConfig.Values | add-member -name "AzureWebJobsStorage" -value $storageConnectionString -MemberType NoteProperty
	$partnerServiceConfig.Values | add-member -name "SQL_CONNECTION_STRING" -value $sqlConnectionSring -MemberType NoteProperty		
	$partnerServiceConfig.Values | add-member -name "KEY_VAULT_NAME" -value $keyVaultName -MemberType NoteProperty	
	$partnerServiceConfig.Values | add-member -name "ENCRYPTION_ASYMMETRIC_KEY" -value $encryptionKey -MemberType NoteProperty	
	$partnerServiceConfig | ConvertTo-Json -depth 3 | Out-File .\partner\functions\local.settings.json
	
	$pubsubServiceConfig = Get-Content .\localSettingTemplate.json | Out-String | ConvertFrom-Json
	$pubsubServiceConfig.Values | add-member -name "AzureWebJobsStorage" -value $storageConnectionString -MemberType NoteProperty
	$pubsubServiceConfig.Values | add-member -name "STORAGE_ACCOUNT_CONNECTION_STRING" -value $storageConnectionString -MemberType NoteProperty
	$pubsubServiceConfig | ConvertTo-Json -depth 3 | Out-File .\pubsub\functions\local.settings.json
	
	$rbacServiceConfig = Get-Content .\localSettingTemplate.json | Out-String | ConvertFrom-Json
	$rbacServiceConfig.Values | add-member -name "AzureWebJobsStorage" -value $storageConnectionString -MemberType NoteProperty
	$rbacServiceConfig.Values | add-member -name "SQL_CONNECTION_STRING" -value $sqlConnectionSring -MemberType NoteProperty
	$rbacServiceConfig | ConvertTo-Json -depth 3 | Out-File .\rbac\functions\local.settings.json
	
	$routingServiceConfig = Get-Content .\localSettingTemplate.json | Out-String | ConvertFrom-Json
	$routingServiceConfig.Values | add-member -name "AzureWebJobsStorage" -value $storageConnectionString -MemberType NoteProperty
	$routingServiceConfig.Values | add-member -name "PUBSUB_SERVICE_BASE_URL" -value $pubsubFxUrl -MemberType NoteProperty
	$routingServiceConfig.Values | add-member -name "PUBSUB_SERVICE_KEY" -value $pubsubFxKey -MemberType NoteProperty	
	$routingServiceConfig.Values | add-member -name "SQL_CONNECTION_STRING" -value $sqlConnectionSring -MemberType NoteProperty	
	$routingServiceConfig.Values | add-member -name "KEY_VAULT_NAME" -value $keyVaultName -MemberType NoteProperty	
	$routingServiceConfig.Values | add-member -name "ENCRYPTION_ASYMMETRIC_KEY" -value $encryptionKey -MemberType NoteProperty	
	$routingServiceConfig | ConvertTo-Json -depth 3 | Out-File .\routing\functions\local.settings.json
	
	$galleryServiceConfig = Get-Content .\localSettingTemplate.json | Out-String | ConvertFrom-Json
	$galleryServiceConfig.Values | add-member -name "AzureWebJobsStorage" -value $storageConnectionString -MemberType NoteProperty
	$galleryServiceConfig.Values | add-member -name "ROUTING_SERVICE_BASE_URL" -value $routingFxUrl -MemberType NoteProperty
	$galleryServiceConfig.Values | add-member -name "PUBSUB_SERVICE_BASE_URL" -value $pubsubFxUrl -MemberType NoteProperty
	$galleryServiceConfig.Values | add-member -name "PUBSUB_SERVICE_KEY" -value $pubsubFxKey -MemberType NoteProperty	
	$galleryServiceConfig.Values | add-member -name "SQL_CONNECTION_STRING" -value $sqlConnectionSring -MemberType NoteProperty	
	$galleryServiceConfig.Values | add-member -name "MARKETPLACE_AUTH_TENANT_ID" -value $config.marketplaceTenantId -MemberType NoteProperty	
	$galleryServiceConfig.Values | add-member -name "MARKETPLACE_AUTH_CLIENT_ID" -value $config.marketplaceClientId -MemberType NoteProperty	
	$galleryServiceConfig.Values | add-member -name "MARKETPLACE_AUTH_CLIENT_SECRET" -value $config.marketplaceClientSecret -MemberType NoteProperty	
	$galleryServiceConfig.Values | add-member -name "KEY_VAULT_NAME" -value $keyVaultName -MemberType NoteProperty	
	$galleryServiceConfig | ConvertTo-Json -depth 3 | Out-File .\gallery\functions\local.settings.json
	
	$provisionServiceConfig = Get-Content .\localSettingTemplate.json | Out-String | ConvertFrom-Json
	$provisionServiceConfig.Values | add-member -name "AzureWebJobsStorage" -value $storageConnectionString -MemberType NoteProperty
	$provisionServiceConfig.Values | add-member -name "ROUTING_SERVICE_BASE_URL" -value $routingFxUrl -MemberType NoteProperty
	$provisionServiceConfig.Values | add-member -name "PUBSUB_SERVICE_BASE_URL" -value $pubsubFxUrl -MemberType NoteProperty
	$provisionServiceConfig.Values | add-member -name "PUBSUB_SERVICE_KEY" -value $pubsubFxKey -MemberType NoteProperty	
	$provisionServiceConfig.Values | add-member -name "SQL_CONNECTION_STRING" -value $sqlConnectionSring -MemberType NoteProperty	
	$provisionServiceConfig.Values | add-member -name "KEY_VAULT_NAME" -value $keyVaultName -MemberType NoteProperty	
	$provisionServiceConfig | ConvertTo-Json -depth 3 | Out-File .\provision\functions\local.settings.json
}
