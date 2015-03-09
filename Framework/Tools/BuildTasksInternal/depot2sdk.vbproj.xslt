<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="1.0"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:msbuild="http://schemas.microsoft.com/developer/msbuild/2003"
  exclude-result-prefixes="msbuild #default">
  <xsl:output omit-xml-declaration="yes" method="xml"/>

  <xsl:param name="TransformType" />
  <xsl:param name="ToolsVersion" />
  <xsl:param name="MajorVersion" />
  <xsl:param name="MinorVersion" />
  <xsl:param name="BuildNumber" />
  <xsl:param name="RevisionNumber" />
  <xsl:param name="ShortVersion" />
  <xsl:param name="FullVersion" />

  <xsl:template match="/msbuild:Project/@DefaultTargets">
    <xsl:attribute name="DefaultTargets">Build</xsl:attribute>
  </xsl:template>

  <!--These are properties that will be removed-->
  <xsl:template match="/msbuild:Project/msbuild:PropertyGroup/msbuild:SDKProject"/>
  <xsl:template match="/msbuild:Project/msbuild:PropertyGroup/msbuild:EmulatorExe"/>
  <xsl:template match="/msbuild:Project/msbuild:PropertyGroup/msbuild:ComponentGuid"/>
  <xsl:template match="/msbuild:Project/msbuild:PropertyGroup/msbuild:ComponentGuidGAC"/>
  <xsl:template match="/msbuild:Project/msbuild:PropertyGroup/msbuild:DirectoryRef"/>
  <xsl:template match="/msbuild:Project/msbuild:PropertyGroup/msbuild:TransformProject"/>
  <xsl:template match="/msbuild:Project/msbuild:PropertyGroup/msbuild:TinyCLR_Flavor"/>
  <xsl:template match="/msbuild:Project/msbuild:PropertyGroup/msbuild:TinyCLR_Platform"/>
  <xsl:template match="/msbuild:Project/msbuild:PropertyGroup/msbuild:TinyCLR_ClientOnly"/>
  <xsl:template match="/msbuild:Project/msbuild:PropertyGroup/msbuild:TinyCLR_ServerOnly"/>
  <xsl:template match="/msbuild:Project/msbuild:PropertyGroup/msbuild:TransformType"/>
  <xsl:template match="/msbuild:Project/msbuild:PropertyGroup/msbuild:PreTransform"/>
  <xsl:template match="/msbuild:Project/msbuild:PropertyGroup/msbuild:PostTransform"/>
  <xsl:template match="/msbuild:Project/msbuild:ItemGroup/msbuild:Reference/msbuild:HintPath"/>
  <xsl:template match="/msbuild:Project/msbuild:ItemGroup/msbuild:MMP_DAT_CreateDatabase"/>
  <xsl:template match="/msbuild:Project/msbuild:ItemGroup/msbuild:MMP_XML_Load"/>
  <xsl:template match="/msbuild:Project/msbuild:PropertyGroup/msbuild:MMP_PE_NoBitmapCompression"/>
  <xsl:template match="/msbuild:Project/msbuild:PropertyGroup/msbuild:MMP_DAT_SKIP"/>
  <xsl:template match="/msbuild:Project/msbuild:PropertyGroup/msbuild:MMP_DAT_CreateDatabaseFile"/>
  <xsl:template match="/msbuild:Project/msbuild:PropertyGroup/msbuild:MMP_XML_SKIP"/>
  <xsl:template match="/msbuild:Project/msbuild:PropertyGroup/msbuild:MMP_XML_GenerateDependency"/>
  <xsl:template match="/msbuild:Project/msbuild:PropertyGroup/msbuild:TinyCLR_WiXPlatform"/>
  <xsl:template match="/msbuild:Project/msbuild:ItemGroup/msbuild:WiXComponentIncludeFile"/>
  <xsl:template match="/msbuild:Project/msbuild:ItemGroup/msbuild:WiXFragmentIncludeFile"/>
  <xsl:template match="/msbuild:Project/msbuild:ItemGroup/msbuild:WiXComponentFiles"/>
  <xsl:template match="/msbuild:Project/msbuild:PropertyGroup/msbuild:MSDNETMFBuildExtensionsPath"/>
  <!--End of properties that will be removed-->

  <xsl:template match="/msbuild:Project/msbuild:PropertyGroup[position()=1]">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
      <xsl:if test="string-length(./msbuild:TargetFrameworkVersion/text()) = 0">
        <!-- The TargetFrameworkVersion needs to be set differently depending on Client/Server (and ToolVersion on Server)-->
        <xsl:choose>
          <xsl:when test="$TransformType='Server'"><TargetFrameworkVersion>v<xsl:copy-of select="$ToolsVersion"/></TargetFrameworkVersion></xsl:when>
          <xsl:otherwise><TargetFrameworkVersion>v<xsl:copy-of select="$ShortVersion"/></TargetFrameworkVersion></xsl:otherwise>
        </xsl:choose>
      </xsl:if>
      <xsl:if test="string-length(./msbuild:NetMfTargetsBaseDir/text()) = 0">
        <NetMfTargetsBaseDir Condition="'$(NetMfTargetsBaseDir)'==''">$(MSBuildExtensionsPath32)\Microsoft\.NET Micro Framework\</NetMfTargetsBaseDir>
      </xsl:if>
    </xsl:copy>

    <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' ">
      <DebugSymbols>true</DebugSymbols>
      <DebugType>full</DebugType>
      <Optimize>true</Optimize>
      <OutputPath>bin\Debug\</OutputPath>
      <DefineDebug>true</DefineDebug>
      <DefineTrace>true</DefineTrace>
      <ErrorReport>prompt</ErrorReport>
      <WarningLevel>4</WarningLevel>
      <StartupObject>Sub Main</StartupObject>
    </PropertyGroup>
    <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Release|AnyCPU' ">
      <DebugType>pdbonly</DebugType>
      <Optimize>true</Optimize>
      <OutputPath>bin\Release\</OutputPath>
      <DefineTrace>true</DefineTrace>
      <ErrorReport>prompt</ErrorReport>
      <WarningLevel>4</WarningLevel>
      <StartupObject>Sub Main</StartupObject>
    </PropertyGroup>

  </xsl:template>

  <xsl:template match="/msbuild:Project/msbuild:Import">

    <xsl:choose>
      <!-- Samples like SPITemperatureDevice could use the standard Microsoft.CSharp.Targets instead of Emulator.targets, but I don't think it
           hurts anything.
      -->
      
      <xsl:when test="@Project = '$(SPOCLIENT)\tools\Targets\Microsoft.SPOT.VisualBasic.Targets' or
                      @Project = '$(SPOCLIENT)\tools\Targets\Microsoft.SPOT.Test.VisualBasic.Targets' or
                      @Project = '$(SPOCLIENT)\Framework\IDE\Targets\v{$ShortVersion}\VisualBasic.Targets'">
        <xsl:copy>
          <xsl:attribute name="Project">$(NetMfTargetsBaseDir)$(TargetFrameworkVersion)\VisualBasic.Targets</xsl:attribute>
        </xsl:copy>
      </xsl:when>
      
      <xsl:when test="@Project = '$(SPOCLIENT)\tools\Targets\Microsoft.SPOT.VisualBasic.Host.Targets' or
                      @Project = '$(SPOCLIENT)\tools\Targets\Microsoft.SPOT.Test.VisualBasic.Host.Targets'">
        <xsl:copy>
          <xsl:attribute name="Project">$(MSBuildToolsPath)\Microsoft.VisualBasic.targets</xsl:attribute>
        </xsl:copy>
        <Import Project="$(NetMfTargetsBaseDir)\v{$ShortVersion}\Emulator.Targets" />
      </xsl:when>
      
      <xsl:when test="@Project = '$(SPOCLIENT)\Framework\IDE\Targets\v{$ShortVersion}\Emulator.targets'">
        <xsl:copy>
          <xsl:attribute name="Project">$(NetMfTargetsBaseDir)\v<xsl:copy-of select="$ShortVersion" />\Emulator.Targets</xsl:attribute>
        </xsl:copy>
      </xsl:when>
      <xsl:otherwise>
        <xsl:copy>
          <xsl:apply-templates select="@*|node()"/>
        </xsl:copy>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>

  <xsl:template match="/msbuild:Project/msbuild:ItemGroup/msbuild:Reference[@Include='Microsoft.SPOT.Emulator']">
    <Reference Include="Microsoft.SPOT.Emulator, Version={$FullVersion}, Culture=neutral, PublicKeyToken=2670f5f21e7f4192, processorArchitecture=x86" />
  </xsl:template>

  <xsl:template match="@*|node()">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
    </xsl:copy>
  </xsl:template>
</xsl:stylesheet>
