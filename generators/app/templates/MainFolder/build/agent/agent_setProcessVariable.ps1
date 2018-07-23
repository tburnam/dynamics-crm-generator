param(
		[string]$varName = "ProcessVariable",
		[string]$varValue = "ProcessValue"
)

if ($varName -ne '') {
	Write-Host("##vso[task.setvariable variable=" + $varName + ";]" + $varValue)
	"Process variable '" + $varName + "' has been set to '" + $varValue + "'"
}