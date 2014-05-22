<# 
.SYNOPSIS 
    This script applies an Read/Execute privilege for Network Service to a file. 
    
.DESCRIPTION 
    This script applies an Read/Execute privilege for Network Service to a file.
    The Synchronize privilage will implicitly be added.
        
.NOTES 
    File Name  : Grant-NetworkServiceExecuteAccess.ps1 
    Requires   : PowerShell Version 2.0
    
.PARAMETER FilePath

The path to the file being opened

#> 
param ( 
    [Parameter(Mandatory = $true)] [string] $FilePath
    )

if (-not (Test-Path -Path $FilePath -PathType Leaf)) {
    Write-Error "No file called $FilePath exists"
    return
}


$acl = Get-Acl $FilePath
$networkService = New-Object System.Security.Principal.SecurityIdentifier @([System.Security.Principal.WellKnownSidType]::NetworkServiceSid, $null)
$grant = New-Object System.Security.AccessControl.FileSystemAccessRule ($networkService, [System.Security.AccessControl.FileSystemRights]::ReadAndExecute,[System.Security.AccessControl.AccessControlType]::Allow)
$acl.AddAccessRule($grant)
Set-Acl -Path $FilePath -AclObject $acl 

