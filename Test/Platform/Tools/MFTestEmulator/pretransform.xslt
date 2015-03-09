<?xml version="1.0" encoding="UTF-8" ?>
<xsl:stylesheet version="1.0"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:msbuild="http://schemas.microsoft.com/developer/msbuild/2003"
  exclude-result-prefixes="msbuild #default">
  <xsl:output omit-xml-declaration="yes" method="xml"/>

  <xsl:template match="/msbuild:Project/msbuild:PropertyGroup/msbuild:AssemblyName">
    <AssemblyName>SampleEmulator</AssemblyName>
  </xsl:template>

  <xsl:template match="/msbuild:Project/msbuild:PropertyGroup/msbuild:EmulatorId">
    <EmulatorId>{7702411c-01e7-499a-a057-65f05fbb1c11}</EmulatorId>
  </xsl:template>

  <xsl:template match="@*|node()">
    <xsl:copy>
      <xsl:apply-templates select="@*|node()"/>
    </xsl:copy>
  </xsl:template>


</xsl:stylesheet>
