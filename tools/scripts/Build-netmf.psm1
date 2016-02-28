<#
.SYNOPSIS

Function to extract the files contained in a zip file provided as a stream typically from an Invoke-WebRequest

.EXAMPLE

Invoke-WebRequest -Uri "$packSourceURLBase/$packFileName" | Expand-Stream
#>
# 
Function Expand-Stream
{
    [CmdletBinding()]
    Param ( [parameter(ValueFromPipeline=$True,Mandatory=$True,ValueFromPipelineByPropertyName=$True)] [System.IO.Stream]$RawContentStream
          , [parameter(Mandatory=$False)] [String]$Destination
          )
    Begin
    {
        [System.Reflection.Assembly]::LoadWithPartialName("System.IO.Compression") | Out-Null
        [System.Reflection.Assembly]::LoadWithPartialName("System.IO.Compression.FileSystem") | Out-Null
        if( [String]::IsNullOrWhiteSpace( $Destination ) )
        {
            $rootPath = Get-Location
        }
        else
        {
            $rootPath = $Destination
        }
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

export-modulemember -function Expand-Stream