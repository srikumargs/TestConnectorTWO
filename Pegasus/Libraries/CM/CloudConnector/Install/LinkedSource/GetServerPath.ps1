function get-tfs (
    [string] $serverName = $(Throw 'serverName is required'),
    [string] $version = $(Throw 'version is required')
)
{
    # load the required dll
	if($version -eq '10.0.0.0')
	{
		Add-Type -AssemblyName ('Microsoft.TeamFoundation.Client, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a')
		Add-Type -AssemblyName ('Microsoft.TeamFoundation.VersionControl.Client, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a')
		Add-Type -AssemblyName ('Microsoft.TeamFoundation.WorkItemTracking.Client, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a')
		Add-Type -AssemblyName ('Microsoft.TeamFoundation.Build.Common, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a')
		Add-Type -AssemblyName ('Microsoft.TeamFoundation, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a')
	}

	if($version -eq '11.0.0.0')
	{
		Add-Type -AssemblyName ('Microsoft.TeamFoundation.Client, Version=11.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a')
		Add-Type -AssemblyName ('Microsoft.TeamFoundation.VersionControl.Client, Version=11.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a')
		Add-Type -AssemblyName ('Microsoft.TeamFoundation.WorkItemTracking.Client, Version=11.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a')
		Add-Type -AssemblyName ('Microsoft.TeamFoundation.Build.Common, Version=11.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a')
		Add-Type -AssemblyName ('Microsoft.TeamFoundation, Version=11.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a')
	}

    $propertiesToAdd = (
        ('VCS', 'Microsoft.TeamFoundation.VersionControl.Client', 'Microsoft.TeamFoundation.VersionControl.Client.VersionControlServer'),
        ('WIT', 'Microsoft.TeamFoundation.WorkItemTracking.Client', 'Microsoft.TeamFoundation.WorkItemTracking.Client.WorkItemStore'),
        ('BS', 'Microsoft.TeamFoundation.Build.Common', 'Microsoft.TeamFoundation.Build.Proxy.BuildStore'),
        ('CSS', 'Microsoft.TeamFoundation', 'Microsoft.TeamFoundation.Server.ICommonStructureService'),
        ('GSS', 'Microsoft.TeamFoundation', 'Microsoft.TeamFoundation.Server.IGroupSecurityService')
    )

    # fetch the TFS instance, but add some useful properties to make life easier
    # Make sure to "promote" it to a psobject now to make later modification easier
    [psobject] $tfs = [Microsoft.TeamFoundation.Client.TeamFoundationServerFactory]::GetServer($serverName)
    foreach ($entry in $propertiesToAdd) {
        $scriptBlock = '
            $this.GetService([{1}])
        ' -f $entry[1],$entry[2]
        $tfs | add-member scriptproperty $entry[0] $ExecutionContext.InvokeCommand.NewScriptBlock($scriptBlock)
    }
    return $tfs
}




$tfs = get-tfs $args[0] $args[3]
$workspace = $tfs.VCS.GetWorkspace($args[1])
[System.IO.File]::WriteAllText($args[2], $workspace.GetServerItemForLocalItem($args[1]))