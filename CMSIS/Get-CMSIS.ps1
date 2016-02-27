<#
.SYNOPSIS

Retrieves and extracts the CMSIS pack supported by NETMF from the official GitHub release.

.EXAMPLE

Get-CMSIS
#>

# Function to extract the files contained in a zip file provided as a stream
Function Extract-PackStream
{
    [CmdletBinding()]
    Param ( [parameter(ValueFromPipeline=$True,Mandatory=$True,ValueFromPipelineByPropertyName=$True)] [System.IO.Stream]$RawContentStream )
    Begin
    {
        [System.Reflection.Assembly]::LoadWithPartialName("System.IO.Compression") | Out-Null
        [System.Reflection.Assembly]::LoadWithPartialName("System.IO.Compression.FileSystem") | Out-Null
        $rootPath = Get-Location
    } 
    Process
    {
        $pack = New-Object -TypeName System.IO.Compression.ZipArchive -ArgumentList $RawContentStream
        $i = 1
        Foreach( $entry in $pack.Entries )
        {
            $targetPath = [System.IO.Path]::Combine( $rootPath, $entry.FullName )
            $targetDir = [System.IO.Path]::GetDirectoryName( $targetPath )
            if( $entry.FullName.EndsWith('/') )
            {
                if( -not [System.IO.Directory]::Exists( $targetDir ) )
                {
                    [System.IO.Directory]::CreateDirectory( $targetDir )
                }
            }
            else
            {
                Write-Progress -Activity "Extracting" -Status $entry.FullName -PercentComplete ( $i / $pack.Entries.Count * 100 )
                [System.IO.Compression.ZipFileExtensions]::ExtractToFile( $entry, $targetPath, $true )
            }
            $i = $i + 1
        }
        Write-Host "Done Extracting $i files"
    }
}

# officially supported version
$packVersion = "4.3.0"

# FUll versioned pack file name to download
$packFileName = "ARM.CMSIS.$packVersion.pack"

# base URL to download the pack file from
$packSourceURLBase = "https://github.com/ARM-software/CMSIS/releases/download/v$packVersion"

# download the pack and extract the files into the curent directory 
Invoke-WebRequest -Uri "$packSourceURLBase/$packFileName" | Extract-PackStream