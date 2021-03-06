
function Run-CESql($ceDllPath, $connectionString, $sqlString)
{
    [Reflection.Assembly]::LoadFile($ceDllPath) | out-null
    $cn = new-object "System.Data.SqlServerCe.SqlCeConnection" $connString

    # create the command 
    $cmd = new-object "System.Data.SqlServerCe.SqlCeCommand" 
    $cmd.CommandType = [System.Data.CommandType]"Text" 
    $cmd.CommandText = $sqlCmd
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
    #$connString = "Data Source=C:\dev\Sage Cloud\CloudConnector\Main\Runtime Files\Documents and Settings\All Users\Application Data\Sage\CRE\HostingFramework\Sage.CloudConnector.SCA.Service.1.0\ConfigurationStore.sdf"
    #$connString = "Data Source=C:\ProgramData\SAGE\CRE\HostingFramework\Sage.CloudConnector.SCA.Service.1.0\ConfigurationStore.sdf"
    $connString = "Data Source=ConfigurationStore.sdf"
    
    return $connString
}

function Set-ConnectorConcurentExecutions()
{
    $cePath = Get-CEPath
    $connString = Get-ConnectionString

    $sqlCmd = "Update PremiseConfigurations SET BackOfficeAllowableConcurrentExecutions = '5'"

    Run-CESql $cePath $connString $sqlCmd 
}

function Get-ConnectorConfigurations()
{
    $cePath = Get-CEPath
    $connString = Get-ConnectionString
    
    $sqlCmd = "SELECT * FROM PremiseConfigurations"

    Run-CESql $cePath $connString $sqlCmd 
}


function KeepConsoleOpenIfNeeded
{
    if((get-host).name -eq "ConsoleHost"){ 
        powershell.exe -noexit -nologo
    }
}

Set-ConnectorConcurentExecutions

Get-ConnectorConfigurations | Out-GridView

KeepConsoleOpenifNeeded