$acrName = $env:acrName
$acr =$acrName + "azurecr.io"
$fulltag = $acr +"/httpschedulerrunner:" + $(Get-Date -format "yyyy-MM-dd-HH-mm")


docker build -f KL.HttpScheduler.Runner/Dockerfile -t $fulltag .

az acr login --name $acrName

docker push $fulltag