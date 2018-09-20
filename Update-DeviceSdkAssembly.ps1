# copy 4 files to Sdk endian specific folder
# <assembly>.dll
# <assembly>.pdb
# <assembly>.pdbx
# <assembly>.pe
Function Update-SdkEndianBins
{
Param( [String] $assemblyName, [String] $endian )

    $BUILD_CLIENT_DLL = Join-Path ${env:BUILD_TREE_CLIENT} "dll"
    $BUILD_CLIENT_PE = Join-Path ${env:BUILD_TREE_CLIENT} "pe"
    $SdkInstallRoot = Join-Path ${env:ProgramFiles(x86)} "Microsoft .NET Micro Framework\v4.4"

    $endianName = Join-Path $endian $assemblyName
    Copy-Item (Join-Path $BUILD_CLIENT_DLL "$assemblyName.dll") (Join-Path $SdkInstallRoot "Assemblies\$endianName.dll")
    Copy-Item (Join-Path $BUILD_CLIENT_DLL "$assemblyName.pdb") (Join-Path $SdkInstallRoot "Assemblies\$endianName.pdb")
    Copy-Item (Join-Path $BUILD_CLIENT_PE "$endianName.pdbx") (Join-Path $SdkInstallRoot "Assemblies\$endianName.pdbx")
    Copy-Item (Join-Path $BUILD_CLIENT_PE "$endianName.pe") (Join-Path $SdkInstallRoot "Assemblies\$endianName.pe")
}

# copy 2 files into Reference Assemblies location
# <assembly>.pdb
# <assembly>.dll
Function Update-SdkRefAssembly
{
Param( [String] $assemblyName )

    $RefAssembliesDir = Join-Path ${env:ProgramFiles(x86)} "Reference Assemblies\Microsoft\Framework\.NETMicroFramework\v4.4"
    $BUILD_CLIENT_DLL = Join-Path ${env:BUILD_TREE_CLIENT} "dll"
    $BUILD_CLIENT_PE = Join-Path ${env:BUILD_TREE_CLIENT} "pe"
    Copy-Item (Join-Path $BUILD_CLIENT_DLL "$assemblyName.dll") (Join-Path $RefAssembliesDir "$assemblyName.dll")
    Copy-Item (Join-Path $BUILD_CLIENT_DLL "$assemblyName.pdb") (Join-Path $RefAssembliesDir "$assemblyName.pdb")
}

Function Update-SdkAssembly
{
Param( [String] $assemblyName )

    if( [String]::IsNullOrWhiteSpace( ${env:BUILD_TREE_CLIENT} ) )
    {
        Write-Error "You must run one of the setenv_xxx scripts to initialize the environment before using this command"
        return
    }

    Update-SdkRefAssembly $AssemblyName
    Update-SdkEndianBins $AssemblyName "le"
    Update-SdkEndianBins $AssemblyName "be"
}
