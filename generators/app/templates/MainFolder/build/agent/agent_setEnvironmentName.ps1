param(
		[string]$targetMachineName = "xrmtest"
)

$targetMachineName = $targetMachineName.ToLower()

switch ($targetMachineName) 
{ 
		"xrmtest241" { $environmentName = "XRMTest_2017-02-02-1309-30" }
		"xrmtest249" { $environmentName = "XRMTest_2017-02-02-0843-36" }
		"xrmtest258" { $environmentName = "XRMTest_2017-05-02-1650-28" }
		"xrmtest275" { $environmentName = "XRMTest_2016-11-23-1329-13" }
		"xrmtest280" { $environmentName = "XRMTest_2017-01-31-1621-14" }
		"xrmtest298" { $environmentName = "XRMTest_2017-02-04-0612-22" }
		"xrmtest302" { $environmentName = "XRMTest_2016-11-23-1328-34" }
		"xrmtest346" { $environmentName = "XRMTest_2017-02-01-0945-09" }
		"xrmtest450" { $environmentName = "XRMTest_2017-02-09-1301-29" }
		"xrmtest480" { $environmentName = "XRMTest_2016-11-23-1328-47" }
		"xrmtest499" { $environmentName = "XRMTest_2017-05-05-0921-01" }
		"xrmtest557" { $environmentName = "XRMTest_2017-05-06-2014-45" }
		"xrmtest580" { $environmentName = "XRMTest_2017-05-02-1027-56" }
		"xrmtest586" { $environmentName = "XRMTest_2016-11-23-1328-58" }
		"xrmtest622" { $environmentName = "XRMTest_2016-11-23-1329-06" }
		"xrmtest689" { $environmentName = "XRMTest_2017-01-20-1048-53" }
		"xrmtest693" { $environmentName = "XRMTest_2016-11-23-1328-29" }
		"xrmtest733" { $environmentName = "XRMTest_2016-11-23-1328-53" }
		"xrmtest738" { $environmentName = "XRMTest_2017-05-05-0945-45" }
		"xrmtest811" { $environmentName = "XRMTest_2017-02-01-1300-43" }
		"xrmtest847" { $environmentName = "XRMTest_2016-11-23-1328-16" }
		"xrmtest858" { $environmentName = "XRMTest_2017-02-01-1129-56" }
		"xrmtest874" { $environmentName = "XRMTest_2017-05-06-2015-04" }
		"xrmtest881" { $environmentName = "XRMTest_2016-11-23-1328-40" }
		"xrmtest910" { $environmentName = "XRMTest_2017-02-11-0631-27" }
		"xrmtest968" { $environmentName = "XRMTest_2016-11-23-1328-22" }
		"xrmtest993" { $environmentName = "XRMTest_2017-02-11-0631-56" }
		default { $environmentName = "XRMTest_" }
}

Write-Host("##vso[task.setvariable variable=Environment;]" + $environmentName)
"Enviroment variable has been set to '" + $environmentName + "'" 