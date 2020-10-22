
Push-Location luna.api

$profileExists = (Test-Path "Properties\PublishProfiles\FolderProfile.pubxml")
if (-not $profileExists){
	mkdir Properties\PublishProfiles
	Copy-Item FolderProfile.pubxml.user.bak .\Properties\PublishProfiles\FolderProfile.pubxml.user
	Copy-Item FolderProfile.pubxml.bak .\Properties\PublishProfiles\FolderProfile.pubxml
}

Start-Process -FilePath "C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MSBuild.exe" -ArgumentList "Luna.API.csproj /p:DeployOnBuild=true /p:PublishProfile=FolderProfile /p:Configuration=Release" -NoNewWindow -Wait

Compress-Archive -Path .\bin\Release\netcoreapp3.1\publish\* -DestinationPath ..\..\Resources\Builds\latest\apiApp.zip -Force

Pop-Location

Push-Location luna.ui\isv_client

yarn install

yarn build

Compress-Archive -Path .\build\* -DestinationPath ..\..\..\Resources\Builds\latest\isvApp.zip -Force

Pop-Location

Push-Location luna.ui\enduser_client

yarn install

yarn build

Compress-Archive -Path .\build\* -DestinationPath ..\..\..\Resources\Builds\latest\userApp.zip -Force

Pop-Location

Push-Location luna.agent\luna.agent.ui

yarn install

yarn build

Pop-Location

Push-Location luna.agent

Remove-Item "build" -Recurse -ErrorAction Ignore

New-Item -Path . -Name "build" -ItemType "directory"

Copy-Item ".\Deploy.ps1" -Destination "build"

Copy-Item -Path ".\luna.agent.ui\build\*" -Destination "build\portal" -Recurse

New-Item -Path "build" -Name "api" -ItemType "directory"

$exclude = "Agent", "__pycache__", ".azure", "env", "mlruns", "obj"

Copy-Item -Path ".\luna.agent\*" -Destination "build\api" -Exclude $exclude

New-Item -Path "build\api" -Name "Agent" -ItemType "directory"

Copy-Item -Path ".\luna.agent\Agent\*" -Destination "build\api\Agent" -Recurse -Exclude "lunacode"

Compress-Archive -Path .\build\* -DestinationPath ..\..\Resources\Builds\latest\agentApp.zip -Force

Remove-Item "build" -Recurse -ErrorAction Ignore

Pop-Location


Push-Location luna.webjobs

$profileExists = (Test-Path "Properties\PublishProfiles\FolderProfile.pubxml")
if (-not $profileExists){
	mkdir Properties\PublishProfiles
	Copy-Item FolderProfile.pubxml.user.bak .\Properties\PublishProfiles\FolderProfile.pubxml.user
	Copy-Item FolderProfile.pubxml.bak .\Properties\PublishProfiles\FolderProfile.pubxml
}

Start-Process -FilePath "C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MSBuild.exe" -ArgumentList "Luna.webjobs.csproj /p:DeployOnBuild=true /p:PublishProfile=FolderProfile /p:Configuration=Release" -NoNewWindow -Wait

Compress-Archive -Path .\bin\Release\netcoreapp3.0\* -DestinationPath ..\..\Resources\Builds\latest\webjob.zip -Force

Pop-Location

