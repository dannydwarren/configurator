$vmName="ConfiguratorTestbed"
New-VM -Name $vmName -MemoryStartupBytes 8GB -Generation 2 -NewVHDPath "C:\VirtualMachines\$vmName.vhdx" -NewVHDSizeBytes 60GB
Set-VM -Name $vmName -ProcessorCount 12
Add-VMDvdDrive -VMName $vmName -Path "D:\Restoration Tools\MSDN\Windows\Win 11\en-us_windows_11_business_editions_version_23h2_x64_dvd_a9092734.iso"
Get-VMDvdDrive -VMName $vmName
Set-VMKeyProtector -VMName $vmName -NewLocalKeyProtector
Enable-VMTPM -VMName $vmName