$path = "HKLM:\SOFTWARE\Microsoft\MSCRM"
New-ItemProperty -Path $path -Name "AzureMLAnalyticsOnPremiseAllowed" -Value 1 -PropertyType "DWORD"
New-ItemProperty -Path $path -Name "AzureMLAnalyticsServiceIsMocked" -Value 1 -PropertyType "DWORD"
New-ItemProperty -Path $path -Name "ignoreSchedulerInitialization" -Value 1 -PropertyType "DWORD"
