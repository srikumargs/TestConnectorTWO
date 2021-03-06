
function Run-CESql($ceDllPath, $connectionString, $sqlString)
{
    [Reflection.Assembly]::LoadFile($ceDllPath) | out-null
    $cn = new-object "System.Data.SqlServerCe.SqlCeConnection" $connectionString

    # create the command 
    $cmd = new-object "System.Data.SqlServerCe.SqlCeCommand" 
    $cmd.CommandType = [System.Data.CommandType]"Text" 
    $cmd.CommandText = $sqlString
    $cmd.Connection = $cn

    #get the data 
    $dt = new-object "System.Data.DataTable"

    $cn.Open() 
    $rdr = $cmd.ExecuteReader()
    $dt.Load($rdr) 
    $cn.Close()

	if ($dt.Rows.Count -eq 0)
    {
        "No configurations to display."
    }
    else 
    {
    	$dt
    }
}


function Run-CESqlCommand($ceDllPath, $connectionString, $sqlString)
{
    #only outputs if there is data that comes back
    [Reflection.Assembly]::LoadFile($ceDllPath) | out-null
    $cn = new-object "System.Data.SqlServerCe.SqlCeConnection" $connectionString

    # create the command 
    $cmd = new-object "System.Data.SqlServerCe.SqlCeCommand" 
    $cmd.CommandType = [System.Data.CommandType]"Text" 
    $cmd.CommandText = $sqlString
    $cmd.Connection = $cn

    #get the data 
    $dt = new-object "System.Data.DataTable"

    $cn.Open() 
    $rdr = $cmd.ExecuteReader()
    $dt.Load($rdr) 
    $cn.Close()

	if ($dt.Rows.Count -ne 0)
    {
    	$dt
    }
}

function Get-CEPath()
{
    $cePath = "\Microsoft SQL Server Compact Edition\v4.0\Desktop\System.Data.SqlServerCe.dll”
    if ($ENV:PROCESSOR_ARCHITECTURE -eq "x86")
    {
        $cePath = $ENV:ProgramFiles + $cePath
    }
    else
    {
    	$cePath = ${ENV:ProgramFiles(x86)} + $cePath
    }
    
    return $cePath
}

function Get-ConnectionString()
{
    #$connString = "Data Source=C:\dev\Sage Cloud\CloudConnector\Main\Runtime Files\Documents and Settings\All Users\Application Data\Sage\CRE\HostingFramework\Sage.CloudConnector.STO.Service.1.0\ConfigurationStore.sdf"
    #$connString = "Data Source=C:\ProgramData\SAGE\CRE\HostingFramework\Sage.Skyfire.STOConnector.Service.1.0\ConfigurationStore.sdf"
    $workingPath = Get-Location
    $connString = "Data Source=$workingPath\ConfigurationStore.sdf"
    
    return $connString
}

function Set-ConnectorConfigurationsForS300CRE()
{
    $targetShim = 'Sage.CRE.CloudConnector.PluginShim40.exe'
    $matchShim = 'Sage.CRE.CloudConnector.PluginShim35.exe'
    
    $targetPlugin = 'Sage.STO.CloudIntegration.dll'
    $matchPlugin1 = 'Sage.CRE.CloudConnector.Integration.MockServicer.dll'
    $matchPlugin2 = 'Sage.STO.ReportAccessor.dll'
    
    $sqlCmd = "Update PremiseConfigurations
                SET 
                BackOfficeProductPluginShimExePath = REPLACE(BackOfficeProductPluginShimExePath, '$matchShim', '$targetShim'),
                BackOfficeProductPluginPath = REPLACE(REPLACE(BackOfficeProductPluginPath, '$matchPlugin1', '$targetPlugin'), '$matchPlugin2', '$targetPlugin')
                "

    $cePath = Get-CEPath
    $connString = Get-ConnectionString

    Run-CESqlCommand $cePath $connString $sqlCmd 
}

function Set-ConnectorConfigurationsForSTO98
{
    $targetShim = 'Sage.CRE.CloudConnector.PluginShim35.exe'
    $matchShim = 'Sage.CRE.CloudConnector.PluginShim40.exe'
    
    $targetPlugin = 'Sage.STO.ReportAccessor.dll'
    $matchPlugin1 = 'Sage.CRE.CloudConnector.Integration.MockServicer.dll'
    $matchPlugin2 = 'Sage.STO.CloudIntegration.dll'
    
    $sqlCmd = "Update PremiseConfigurations
                SET 
                BackOfficeProductPluginShimExePath = REPLACE(BackOfficeProductPluginShimExePath, '$matchShim', '$targetShim'),
                BackOfficeProductPluginPath = REPLACE(REPLACE(BackOfficeProductPluginPath, '$matchPlugin1', '$targetPlugin'), '$matchPlugin2', '$targetPlugin')
                "

    $cePath = Get-CEPath
    $connString = Get-ConnectionString

    Run-CESqlCommand $cePath $connString $sqlCmd 
}

function Set-ConnectorConfigurationsForMock
{
    $targetShim = 'Sage.CRE.CloudConnector.PluginShim35.exe'
    $matchShim = 'Sage.CRE.CloudConnector.PluginShim40.exe'
    
    $targetPlugin = 'Sage.CRE.CloudConnector.Integration.MockServicer.dll'
    $matchPlugin1 = 'Sage.STO.ReportAccessor.dll'
    $matchPlugin2 = 'Sage.STO.CloudIntegration.dll'
    
    $sqlCmd = "Update PremiseConfigurations
                SET 
                BackOfficeProductPluginShimExePath = REPLACE(BackOfficeProductPluginShimExePath, '$matchShim', '$targetShim'),
                BackOfficeProductPluginPath = REPLACE(REPLACE(BackOfficeProductPluginPath, '$matchPlugin1', '$targetPlugin'), '$matchPlugin2', '$targetPlugin')
                "

    $cePath = Get-CEPath
    $connString = Get-ConnectionString

    Run-CESqlCommand $cePath $connString $sqlCmd 
}


function Get-ConnectorSTORegistryPath
{
    if ($ENV:PROCESSOR_ARCHITECTURE -eq "x86")
    {
        $regPath = 'HKLM:\software\Sage\SageConnector\STO'
    }
    else
    {
    	$regPath = 'HKLM:\software\Wow6432Node\Sage\SageConnector\STO'
    }
    
    return $regPath
}

function Set-ConnectorRegsitryForS300CRE
{
    $regPath = Get-ConnectorSTORegistryPath
    
    set-itemproperty -path $regPath -name 'ProductPluginRelativeLocation' -value 'Sage.STO.CloudIntegration.dll'
    set-itemproperty -path $regPath -name 'ProductPluginShimExe' -value 'Sage.CRE.CloudConnector.PluginShim40.exe'
    set-itemproperty -path $regPath -name 'PluggedInProductName' -value 'Sage 300 Construction and Real Estate'
}

function Set-ConnectorRegsitryForSTO98
{
    $regPath = Get-ConnectorSTORegistryPath

    set-itemproperty -path $regPath -name 'ProductPluginRelativeLocation' -value 'Sage.STO.ReportAccessor.dll'
    set-itemproperty -path $regPath -name 'ProductPluginShimExe' -value 'Sage.CRE.CloudConnector.PluginShim35.exe'
    set-itemproperty -path $regPath -name 'PluggedInProductName' -value 'Sage Timberline Office'
}

function Set-ConnectorRegsitryForMock
{
    $regPath = Get-ConnectorSTORegistryPath
    
    set-itemproperty -path $regPath -name 'ProductPluginRelativeLocation' -value 'Sage.CRE.CloudConnector.Integration.MockServicer.dll'
    set-itemproperty -path $regPath -name 'ProductPluginShimExe' -value 'Sage.CRE.CloudConnector.PluginShim35.exe'
    set-itemproperty -path $regPath -name 'PluggedInProductName' -value 'Sage Timberline Office'

    #get-itemproperty -path $regPath -name ProductPluginRelativeLocation 
    #get-itemproperty -path $regPath -name ProductPluginShimExe 
    #get-itemproperty -path $regPath -name PluggedInProductName 
}

function Set-ConnectorForS300CRE
{
    Set-ConnectorRegsitryForS300CRE
    Set-ConnectorConfigurationsForS300CRE
}

function Set-ConnectorForSTO98
{
    Set-ConnectorRegsitryForSTO98
    Set-ConnectorConfigurationsForSTO98
}

function Set-ConnectorForMock
{
    Set-ConnectorRegsitryForMock
    Set-ConnectorConfigurationsForMock
}


function Get-ConnectorConfigurations()
{
    $cePath = Get-CEPath
    $connString = Get-ConnectionString
    
    $sqlCmd = "SELECT * FROM PremiseConfigurations"

    Run-CESql $cePath $connString $sqlCmd 
}

function AllowOnlyAdminUser
{

    $isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")
    if(-Not $isAdmin)
    {
        Write-warning "You must be an administrator to run this script successfully."
        break
    }
}

function KeepConsoleOpenIfNeeded
{
    if((get-host).name -eq "ConsoleHost"){ 
        powershell.exe -noexit -nologo
    }
}

AllowOnlyAdminUser

Set-ConnectorForS300CRE

Get-ConnectorConfigurations | Out-GridView

KeepConsoleOpenifNeeded