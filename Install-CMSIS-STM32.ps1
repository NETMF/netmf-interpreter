<#
.SYNOPSIS
Downloads and and extracts STM32CubeMX CMSIS pack.

.DESCRIPTION
The script downloads and and extracts an STM32CubeMX CMSIS support pack for STM32 series from ST website.
Accepts as parameters the series name and the pack version.


.EXAMPLE
Install-CMSIS-STM32.ps1 F4 1.11.00

.PARAMETER seriesName
STM32 series name. Valid series are: F0, F1, F2, F3, F4, F7, L0, L1 and L4.

.PARAMETER packVersion
The pack version to donwload. Format is N.N.N. Example: 1.1.0

#>

[CmdletBinding()]
Param(
    [Parameter(Mandatory=$True,Position=1)]
    [ValidateNotNullOrEmpty()]
    [string]$seriesName,
	
    [Parameter(Mandatory=$True,Position=2)]
    [ValidateNotNullOrEmpty()]
    [string]$packVersion
)

# check running path 
$spotClientPath = Get-Location
if($spotClientPath.Path.Contains("Solutions"))
{
    # path includes 'Solutions' so presume that this is being called from a solution project, repository home must be two levels up
    $spotClientPath = [System.IO.Path]::GetFullPath( [System.IO.Path]::Combine( $spotClientPath, "..","..") )
}

# validate pack version
if(-not ($packVersion -match "\d{1}.\d{1,2}.\d{1}$"))
{
    # path includes 'Solutions' so presume that this is being called from a solution project, repository home must be two levels up
    throw "Pack version is invalid. Must have format N.N.N"
}

# validate series name
$seriesName = $seriesName.ToUpper()
if(-not ($seriesName -match "(^F0$|^F1$|^F2$|^F3$|^F4$|^F7$|^L0$|^L1$|^L4$)"))
{
    throw "Unsupported series. Valid series are: F0, F1, F2, F3, F4, F7, L0, L1 and L4."
}

Write-Host "Installing STM32Cube CMSIS pack v$packVersion for STM32$seriesName series"

# check if this a patch
$PatchVersion = $packVersion.Substring($packVersion.LastIndexOf('.') + 1)

$IsPatch = $false
if( [system.int32]::Parse($PatchVersion) -gt 0)
{
    # this is patch, meaning that the base package needs to be downloaded and then the patch applied on top of it
    
    # update patch flag
    $IsPatch = $true

    # get the base package name
    $packVersion = $packVersion.Substring(0, $packVersion.LastIndexOf('.')) + ".0"
}


# pack file name
$packFileName = "stm32cube_fw_" + $seriesName.ToLower() +  "_v" + ($packVersion -replace "[.]","") + ".zip"

# zip folder name
$zipPackFileName = "STM32Cube_FW_" + $seriesName + "_V" + $packVersion

# directory for the SMT32Cube 
$stmCubePath = [System.IO.Path]::Combine( $spotClientPath, "STM32Cube" )

# directory for the series under STM32Cube folder
$seriesPath = [System.IO.Path]::Combine( $stmCubePath, $seriesName )

# make sure the destination directory is empty (if it exists at all)
if( Test-Path $seriesPath )
{
    Remove-Item -Path $seriesPath -Force -Recurse -ErrorAction Ignore | Out-Null
}
else
{
    # need to create destination directory first
    New-Item -Path $seriesPath -Force -ItemType directory | Out-Null
}

# check is pack file is already there
if(-not (Test-Path ([System.IO.Path]::Combine( $stmCubePath , $packFileName ))) ) 
{
    # base URL to download the pack file from
    $packSourceURL = "http://www.st.com/resource/en/firmware2/" + $packFileName 

    # download the pack...  
    Write-Host "Downloading pack from ST web site. This will take a while, be patient..."

    # must use WebClient because the pack file is huge
    $webclient = New-Object System.Net.WebClient
    $webclient.DownloadFile( $packSourceURL , [System.IO.Path]::Combine( $stmCubePath , $packFileName ) )
}
else
{
    # pack is available...  
    Write-Host "Pack already available, skipping download"
}

# ... and extract the files into the series directory
Write-Host "Extracting pack..."

# must load this type to open Zip files
Add-Type -assembly  System.IO.Compression.FileSystem

$zipArchive = [IO.Compression.ZipFile]::OpenRead( [System.IO.Path]::Combine( $stmCubePath , $packFileName ) )

try
{
    # source folders and files that are to be copied
    $cmsisDeviceFolder = $zipPackFileName + "/Drivers/CMSIS/Device/ST/"
    $halDriverIncFolder = $zipPackFileName + "/Drivers/STM32" + $seriesName + "xx_HAL_Driver/Inc/"
    $halDriverSrcFolder = $zipPackFileName + "/Drivers/STM32" + $seriesName + "xx_HAL_Driver/Src/"
    $halDriverReleaseNotes = $zipPackFileName + "/Drivers/STM32" + $seriesName + "xx_HAL_Driver/Release_Notes.html"
    $cmsisUSBClientLib = $zipPackFileName + "/Middlewares/ST/STM32_USB_Device_Library/"

    foreach($zipItem in $zipArchive.Entries)
    {

        if($zipItem.FullName.StartsWith( $cmsisDeviceFolder ) -Or
            $zipItem.FullName.StartsWith( $halDriverIncFolder ) -Or
            $zipItem.FullName.StartsWith( $halDriverSrcFolder ) -Or
            $zipItem.FullName.StartsWith( $halDriverReleaseNotes ) -Or
            $zipItem.FullName.StartsWith( $cmsisUSBClientLib ))
        {
            $destinationFile   = [System.IO.Path]::Combine( $seriesPath, $zipItem.FullName.Replace($zipPackFileName, "").Replace("/", "\").SubString(1))
            $parentDir = [System.IO.Path]::GetDirectoryName( $destinationFile )

            # create directory if it doesn't exist
            if((-not [System.IO.Path]::HasExtension($parentDir)) -And (-not (Test-Path -Path $parentDir)) )
            {
                New-Item -Path $parentDir -Force -ItemType directory | Out-Null
            }
            else
            {
                [IO.Compression.ZipFileExtensions]::ExtractToFile($zipItem, $destinationFile, $true)
            }
        }
    }
}
finally
{
    # free file
    $zipArchive.Dispose()
}

# move CMSIS folder for this series from STM32Cube folder to the respective Device folder in CMSIS folder
# first clear the destination directory before copying as we don't want to mix versions
Write-Host "Copying CMSIS driver..."

$cmsisPathForSeries = [System.IO.Path]::Combine($spotClientPath , "CMSIS\Device\ST")
Remove-Item -Path $cmsisPathForSeries -Force -Recurse -ErrorAction Ignore
New-Item -Path $cmsisPathForSeries -Force -ItemType directory | Out-Null
Move-Item -Path ( [System.IO.Path]::Combine($seriesPath, "Drivers\CMSIS\Device\ST\*") ) -Destination $cmsisPathForSeries -Force

# move HAL drivers for this series from STM32Cube folder to the respective DeviceCode folder 
# first clear the destination directory before copying as we don't want to mix versions
Write-Host "Copying HAL driver..."

$deviceCodePathForSeries = [System.IO.Path]::Combine( $spotClientPath, "DeviceCode\Targets\Native\STM32" + $seriesName + "xx\HAL_Driver")
Remove-Item -Path $deviceCodePathForSeries -Force -Recurse -ErrorAction Ignore
New-Item -Path $deviceCodePathForSeries -Force -ItemType directory | Out-Null
Move-Item -Path ( [System.IO.Path]::Combine($seriesPath, "Drivers\STM32" + $seriesName + "xx_HAL_Driver\*") ) -Destination $deviceCodePathForSeries -Force 

# move USB device client library from STM32Cube folder to USB DeviceCode folder 
# first clear the destination directory before copying as we don't want to mix versions
Write-Host "Copying USB device client library..."

$usbClientLibPath = [System.IO.Path]::Combine( $spotClientPath, "DeviceCode\Targets\Native\STM32" + $seriesName + "xx\USB\STM32_USB_Device_Library")
Remove-Item -Path $usbClientLibPath -Force -Recurse -ErrorAction Ignore
New-Item -Path $usbClientLibPath -Force -ItemType directory | Out-Null
Move-Item -Path ( [System.IO.Path]::Combine($seriesPath, "Middlewares\ST\STM32_USB_Device_Library\Core\*") ) -Destination $usbClientLibPath -Force 

# rename the USB device client library source files to cpp extension so they get compiled as CPP code
Get-childItem -Path ( [System.IO.Path]::Combine($usbClientLibPath, "Src") ) *.c | rename-item -newname { $_.name -replace '\.c','.cpp' }

# delete the template files from USB device client library destination folder 
Remove-Item -Path ( [System.IO.Path]::Combine($usbClientLibPath, "Src\usbd_conf_template.cpp") ) -Force
Remove-Item -Path ( [System.IO.Path]::Combine($usbClientLibPath, "Inc\usbd_conf_template.h") ) -Force

# delete source folder in STM32Cube as they aren't needed it anymore
Remove-Item -Path $seriesPath -Force -Recurse -ErrorAction Ignore

# if this a patch them go through all the above replacing the existing files
if($IsPatch)
{
    # get the patch package name
    $packVersion = $packVersion.Substring(0, $packVersion.LastIndexOf('.')) + "." + $PatchVersion

    # pack file name
    $packFileName = "stm32cube_fw_" + $seriesName.ToLower() +  "_v" + ($packVersion -replace "[.]","") + ".zip"

    # zip folder name
    $zipPackFileName = "STM32Cube_FW_" + $seriesName + "_V" + $packVersion

    # directory for the SMT32Cube 
    $stmCubePath = [System.IO.Path]::Combine( $spotClientPath, "STM32Cube" )

    # directory for the series under STM32Cube folder
    $seriesPath = [System.IO.Path]::Combine( $stmCubePath, $seriesName )

    # check is pack file is already there
    if(-not (Test-Path ([System.IO.Path]::Combine( $stmCubePath , $packFileName ))) ) 
    {
        # base URL to download the pack file from
        $packSourceURL = "http://www.st.com/resource/en/firmware2/" + $packFileName 

        # download the pack...  
        Write-Host "Downloading pack from ST web site. This will take a while, be patient..."

        # must use WebClient because the pack file is huge
        $webclient = New-Object System.Net.WebClient
        $webclient.DownloadFile( $packSourceURL , [System.IO.Path]::Combine( $stmCubePath , $packFileName ) )
    }
    else
    {
        # pack is available...  
        Write-Host "Pack already available, skipping download"
    }

    # ... and extract the files into the series directory
    Write-Host "Extracting pack..."

    # must load this type to open Zip files
    Add-Type -assembly  System.IO.Compression.FileSystem

    $zipArchive = [IO.Compression.ZipFile]::OpenRead( [System.IO.Path]::Combine( $stmCubePath , $packFileName ) )

    try
    {
        # source folders and files that are to be copied
        $cmsisDeviceFolder = $zipPackFileName + "/Drivers/CMSIS/Device/ST/"
        $halDriverIncFolder = $zipPackFileName + "/Drivers/STM32" + $seriesName + "xx_HAL_Driver/Inc/"
        $halDriverSrcFolder = $zipPackFileName + "/Drivers/STM32" + $seriesName + "xx_HAL_Driver/Src/"
        $halDriverReleaseNotes = $zipPackFileName + "/Drivers/STM32" + $seriesName + "xx_HAL_Driver/Release_Notes.html"
        $cmsisUSBClientLib = $zipPackFileName + "/Middlewares/ST/STM32_USB_Device_Library/"

        foreach($zipItem in $zipArchive.Entries)
        {

            if($zipItem.FullName.StartsWith( $cmsisDeviceFolder ) -Or
                $zipItem.FullName.StartsWith( $halDriverIncFolder ) -Or
                $zipItem.FullName.StartsWith( $halDriverSrcFolder ) -Or
                $zipItem.FullName.StartsWith( $halDriverReleaseNotes ) -Or
                $zipItem.FullName.StartsWith( $cmsisUSBClientLib ))
            {
                $destinationFile   = [System.IO.Path]::Combine( $seriesPath, $zipItem.FullName.Replace($zipPackFileName, "").Replace("/", "\").SubString(1))
                $parentDir = [System.IO.Path]::GetDirectoryName( $destinationFile )

                # create directory if it doesn't exist
                if((-not [System.IO.Path]::HasExtension($parentDir)) -And (-not (Test-Path -Path $parentDir)) )
                {
                    New-Item -Path $parentDir -Force -ItemType directory | Out-Null
                }
                else
                {
                    [IO.Compression.ZipFileExtensions]::ExtractToFile($zipItem, $destinationFile, $true)
                }
            }
        }
    }
    finally
    {
        # free file
        $zipArchive.Dispose()
    }

    # move CMSIS folder for this series from STM32Cube folder to the respective Device folder in CMSIS folder
    # first clear the destination directory before copying as we don't want to mix versions
    Write-Host "Copying CMSIS driver..."

    $cmsisPathForSeries = [System.IO.Path]::Combine($spotClientPath , "CMSIS\Device\ST")
    New-Item -Path $cmsisPathForSeries -Force -ItemType directory -ErrorAction Ignore | Out-Null
    Move-Item -Path ( [System.IO.Path]::Combine($seriesPath, "Drivers\CMSIS\Device\ST\*") ) -Destination $cmsisPathForSeries -Force -ErrorAction Ignore

    # move HAL drivers for this series from STM32Cube folder to the respective DeviceCode folder 
    # first clear the destination directory before copying as we don't want to mix versions
    Write-Host "Copying HAL driver..."

    $deviceCodePathForSeries = [System.IO.Path]::Combine( $spotClientPath, "DeviceCode\Targets\Native\STM32" + $seriesName + "xx\HAL_Driver")
    New-Item -Path $deviceCodePathForSeries -Force -ItemType directory  -ErrorAction Ignore | Out-Null
    Move-Item -Path ( [System.IO.Path]::Combine($seriesPath, "Drivers\STM32" + $seriesName + "xx_HAL_Driver\*") ) -Destination $deviceCodePathForSeries -Force -ErrorAction Ignore

    # move USB device client library from STM32Cube folder to USB DeviceCode folder 
    # first clear the destination directory before copying as we don't want to mix versions
    Write-Host "Copying USB device client library..."

    $usbClientLibPath = [System.IO.Path]::Combine( $spotClientPath, "DeviceCode\Targets\Native\STM32" + $seriesName + "xx\USB\STM32_USB_Device_Library")
    New-Item -Path $usbClientLibPath -Force -ItemType directory  -ErrorAction Ignore | Out-Null
    Move-Item -Path ( [System.IO.Path]::Combine($seriesPath, "Middlewares\ST\STM32_USB_Device_Library\Core\*") ) -Destination $usbClientLibPath -Force -ErrorAction Ignore

    # rename the USB device client library source files to cpp extension so they get compiled as CPP code
    Get-childItem -Path ( [System.IO.Path]::Combine($usbClientLibPath, "Src") ) *.c | rename-item -newname { $_.name -replace '\.c','.cpp' }

    # delete the template files from USB device client library destination folder 
    Remove-Item -Path ( [System.IO.Path]::Combine($usbClientLibPath, "Src\usbd_conf_template.cpp") ) -Force  -ErrorAction Ignore
    Remove-Item -Path ( [System.IO.Path]::Combine($usbClientLibPath, "Inc\usbd_conf_template.h") ) -Force  -ErrorAction Ignore

    # delete source folder in STM32Cube as they aren't needed it anymore
    Remove-Item -Path $seriesPath -Force -Recurse -ErrorAction Ignore
}

# done here
Write-Host "Installation of STM32Cube CMSIS pack v$packVersion for STM32$seriesName series completed"
