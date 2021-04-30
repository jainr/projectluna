param (
    [Alias("s")]
    [string[]]$services = @('gateway','partner','publish','rbac','routing'), 
	
    [Alias("n")]
	[switch] $deployNew = $false,
	
    [Alias("c")]
	[switch] $cleanUp = $false
)

$config = ([xml](Get-Content build.config)).config

if ($cleanUp){
	$keyVaultName = $config.namePrefix + "-keyvault"
	az group delete -n $config.resourceGroupName --subscription $config.subscriptionId --yes
	az keyvault purge --hsm-name $keyVaultName --subscription $config.subscriptionId
	exit 1
}

foreach ($service in $services) {
	
	$filePath = $service + "\functions"
	$zipFilePath = "..\..\testbuild\" + $service + "fx.zip"
	
	push-location $filePath
	
	dotnet clean --configuration Release /property:GenerateFullPaths=true /consoleloggerparameters:NoSummary
	dotnet publish --configuration Release /property:GenerateFullPaths=true /consoleloggerparameters:NoSummary

	Compress-Archive -Path .\bin\Release\netcoreapp3.1\publish\* -DestinationPath $zipFilePath -Force
	
	pop-location
	
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
	
	./deployment.sh -s $config.subscriptionId -r $config.resourceGroupName -l $config.region -n $config.namePrefix -q $config.sqlUserName -p $config.sqlPassword -t $config.tenantId -c $config.clientId -x $config.clientSecret -a $config.adminUserId -u $config.adminUserName -w $config.createNewResource

	pop-location
	
}
