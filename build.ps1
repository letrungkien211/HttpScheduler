param(
 [Parameter(Mandatory=$True)]
 [string]
 $acrName
)


$acr =$acrName + ".azurecr.io"
$fulltag = $acr +"/httpschedulerrunner:" + $(Get-Date -format "yyyy-MM-dd-HH-mm")


docker build -f KL.HttpScheduler.Runner/Dockerfile -t $fulltag .

docker push $fulltag