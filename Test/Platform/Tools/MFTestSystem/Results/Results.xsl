<?xml version="1.0" encoding="UTF-8"?>
<!-- saved from url=(0023)https://www.microsoft.com/ -->
<!--<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:fo="http://www.w3.org/1999/XSL/Format">-->
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:lxslt="http://xml.apache.org/xslt"
                xmlns:result="http://www.example.com/results"
                extension-element-prefixes="result"
                version="1.0">
<xsl:template match="/SPOT_Platform_Test">
<xsl:variable name="initializeName1">init<xsl:value-of select="TestLog/@Test"/></xsl:variable>
<xsl:variable name="testName1">test<xsl:value-of select="TestLog/@Test"/></xsl:variable>
<xsl:variable name="cleanUpName1">cleanUp<xsl:value-of select="TestLog/@Test"/></xsl:variable>
<xsl:variable name="resultName1">result<xsl:value-of select="TestLog/@Test"/></xsl:variable>
<xsl:variable name="imgTestName1">imgTest<xsl:value-of select="TestLog/@Test"/></xsl:variable>
<xsl:variable name="imgResultName1">imgResult<xsl:value-of select="TestLog/@Test"/></xsl:variable>		
<xsl:variable name="srcFileName"><xsl:value-of select="SourceFile"/></xsl:variable>

<html>	
<body>
<script type="text/javascript" >
	<![CDATA[
		function toggle(id)   
		{   
		    var tr = document.getElementById(id);   
		    if (tr==null) { return; }   
		    var bExpand = tr.style.display == '';   
		    tr.style.display = (bExpand ? 'none' : ''); 		    
		 } 		
		 		
		function changeImage(id)
		{   			
			var sExpand = 'Images/expand.gif';
			var sCollapse = 'Images/collapse.gif';
		    var img = document.getElementById(id);   
		    if (img!=null)   
		    {   
			    var bExpand = img.src.indexOf(sCollapse) < 0;

			    if (!bExpand)   
		            img.src = sExpand;   
		        else   
		            img.src = sCollapse;	    
		    }   
		} 
		
		function displayImage(id, sImage)
		{
			var img = document.getElementById(id); 			  
		    if (img!=null)   
		    {
		    	img.src = sImage;
		    }
		}
	
		function collapse(id)
		{
			var tr = document.getElementById(id);			
			if (tr==null) { return; }   			
			tr.style.display = 'none';
		}
	
		function expand(id)
		{
			var tr = document.getElementById(id);			
			if (tr==null) { return; }   			
			tr.style.display = '';
		}		
	]]>		
</script>
</body>
</html>		
		<!-- Start Comment Section -->	
		<xsl:for-each select="Comment">
			<Table border="2" width="100%" bordercolor="black" bgcolor="#EAEAEA" align="center">							
				<tr>
					<td colspan="4" align="center" bordercolor="#FFCC99">
						<b><font size="2" color="red" face="Verdana"><xsl:value-of select="Text/AdditionalInformation"/></font></b>
						<b><font size="2" color="red" face="Verdana"><xsl:value-of select="AdditionalInformation"/></font></b>
					</td>										
				</tr>				
				<tr>
					<td colspan="4" align="center" bordercolor="#FF8080">
								<xsl:variable name="Type" select="@type" />
								<xsl:if test="$Type=0">
									<tr>
										<td bordercolor="#C0C0C0" bgcolor="#FFFFCC" width="10%"><xsl:value-of select="Date"/></td>
										<td bordercolor="#C0C0C0" bgcolor="#FFFFCC" width="13%"><xsl:value-of select="Time"/></td>
										<td bordercolor="#C0C0C0" bgcolor="#FFFFCC" width="11%"><b>Comment</b></td>
										<td bordercolor="#C0C0C0" bgcolor="#FFFFCC" width="66%"><xsl:value-of select="Text"/></td>
									</tr>
								</xsl:if>
								<xsl:if test="$Type=1">
									<tr>
										<td colspan="4" align="center" bgcolor="white">
											<b><font size="5" face="Verdana" color="#912F00">Exception</font></b>
										</td>
									</tr>
									<tr>
										<td bordercolor="#C0C0C0" bgcolor="#FF8080" width="10%"><xsl:value-of select="Date"/></td>
										<td bordercolor="#C0C0C0" bgcolor="#FF8080" width="13%"><xsl:value-of select="Time"/></td>
										<td bordercolor="#C0C0C0" bgcolor="#FF8080" width="11%"><b>Exception</b></td>
										<td bordercolor="#C0C0C0" bgcolor="#FF8080" width="66%"><xsl:value-of select="Text"/></td>
									</tr>
								</xsl:if>	
								<xsl:if test="$Type=99">
									<tr>
										<td bordercolor="#C0C0C0" bgcolor="#FFFFCC" width="15%"><b>Additional Information</b></td>
										<td bordercolor="#C0C0C0" bgcolor="#FFFFCC" width="85%"><xsl:value-of select="Text"/></td>
									</tr>
								</xsl:if>
					</td>
				</tr>								
			</Table>												
		</xsl:for-each>
		<!-- End Comment Section -->	

		<!-- Start Exception Section -->
		<xsl:for-each select="Test_Exception">
			<Table border="2" width="100%" bordercolor="black" bgcolor="#EAEAEA" align="center">	
				<tr>
					<td colspan="4" align="center" bordercolor="#FFCC99">
						<b><font size="5" face="Verdana" color="#912F00">Test Exception</font></b>
					</td>
				</tr>
			</Table>
			<Table border="2" width="100%" bordercolor="black" bgcolor="#EAEAEA" align="center">					
				<tr>
					<td colspan="4" align="center" bordercolor="#FF8080">
						<xsl:value-of select="."/>
					</td>
				</tr>			
			</Table>
		</xsl:for-each>
		<!-- End Exception Section -->
		
		<!-- Start TestLog Section -->
		<xsl:for-each select="TestLog">
			<Table border="2" width="100%" bordercolor="black" bgcolor="#EAEAEA" align="center">
			
			<!-- Start Variable Declaration -->
			<xsl:variable name="initializeName">init<xsl:value-of select="@Test"/><xsl:value-of select="generate-id()"/></xsl:variable>
			<xsl:variable name="imgInitializeName">imgInit<xsl:value-of select="@Test"/><xsl:value-of select="generate-id()"/></xsl:variable>			
			<xsl:variable name="testName">test<xsl:value-of select="@Test"/><xsl:value-of select="generate-id()"/></xsl:variable>
			<xsl:variable name="imgTestName">imgTest<xsl:value-of select="@Test"/><xsl:value-of select="generate-id()"/></xsl:variable>
			<xsl:variable name="cleanUpName">cleanUp<xsl:value-of select="@Test"/><xsl:value-of select="generate-id()"/></xsl:variable>
			<xsl:variable name="imgCleanUpName">imgCleanUp<xsl:value-of select="@Test"/><xsl:value-of select="generate-id()"/></xsl:variable>
			<xsl:variable name="resultName">result<xsl:value-of select="@Test"/><xsl:value-of select="generate-id()"/></xsl:variable>
			<xsl:variable name="imgResultName">imgResult<xsl:value-of select="@Test"/><xsl:value-of select="generate-id()"/></xsl:variable>			
			<!-- End Variable Declaration -->
			
			<tr>
			<td>
				<!-- Start Headers Section -->
				<tr>
					<td colspan="4" align="center" bgcolor="white">
						<table bordercolor="white" bgcolor="#CDCD9C" border="1">
							<tr>
								<td colspan="4" align="center" bordercolor="#FFCC99">
									<b><font size="5" face="Verdana" color="#912F00">Test Log</font></b>
								</td>
							</tr>
							<tr>
								<td colspan="4" align="center" bordercolor="#FFCC99">
									<b><font size="6" face="Verdana"><xsl:value-of select="@Test"/></font></b>
								</td>
							</tr>	
							<tr>
								<td colspan="4" align="center" bordercolor="#FFCC99">
									<b>
									      <font size="2" color="blue" face="Verdana">
										<a href="{$srcFileName}">
											<xsl:value-of select = "$srcFileName" />
											</a>
									      </font>
									</b>
								</td>
							</tr>	
							<tr>
								<td colspan="4" align="center" bordercolor="#FFCC99" id="TestResult">
									<b>
										<font size="5" face="Verdana">
										<xsl:choose>
											<xsl:when test="Results/FailCount/Text='0'">
												<xsl:choose>
													<xsl:when test="Results/PassCount/Text='0'">
                            													<xsl:choose>
															<xsl:when test="Results/SkipCount/Text='0'">
																<xsl:choose>
																	<xsl:when test="Results/KnownFailureCount/Text='0'">
																		<font color="red">Fail</font>
																	</xsl:when>
																	<xsl:otherwise>
																		<font color="green">Pass</font>
																	</xsl:otherwise>
																</xsl:choose>												
															</xsl:when>
															<xsl:otherwise>
																<font color="#0F9FFF">Skip</font>
															</xsl:otherwise>
														</xsl:choose>
													</xsl:when>
													<xsl:otherwise>
														<font color="green">Pass</font>
													</xsl:otherwise>
												</xsl:choose>
											</xsl:when>
											<xsl:otherwise>									
												<font color="red">Fail</font>
											</xsl:otherwise>
										</xsl:choose>
										</font>
									</b>
								</td>
							</tr>						
						</table>	
					</td>
				</tr>				
				<!-- End Headers Section -->								
						
				<!-- Start Initialize Section -->
				<td bordercolor="white" bgcolor="white"><br /></td>
				<tr>										
					<td colspan="4" align="center">								
						<table bordercolor="white" border="1" width="100%">							
							<tr>										
								<td colspan="4" align="left" bordercolor="#C0C0C0" bgcolor="white">											
									<font size="2" face="Verdana">
										<a href="javascript:toggle('{$initializeName}');changeImage('{$imgInitializeName}');">
											<img border="0" src="Images/expand.gif" id="{$imgInitializeName}"/>
										</a>
										<b>   Initialize</b>
									</font>
								</td>								
							</tr>
							<tr>
							<td>
							<table id="{$initializeName}" width="100%" style="display:none">
							<tr>
							<td>
								<xsl:for-each select="../AdditionalInformation">
									<tr>
										<td bordercolor="#C0C0C0" bgcolor="#FFFFCC" width="11%" colspan="2"><b>Additional Information (from debug output)</b></td>
										<td bordercolor="#C0C0C0" bgcolor="#FFFFCC" width="66%" colspan="2"><xsl:value-of select="."/></td>
									</tr>
								</xsl:for-each>	
								<xsl:for-each select="Initialize/Comment">
									<tr style="display:''">									
										<td bordercolor="#C0C0C0" bgcolor="#FFFFCC" width="10%"><xsl:value-of select="Date"/></td>
										<td bordercolor="#C0C0C0" bgcolor="#FFFFCC" width="13%"><xsl:value-of select="Time"/></td>
										<td bordercolor="#C0C0C0" bgcolor="#FFFFCC" width="11%"><b>Comment</b></td>
										<td bordercolor="#C0C0C0" bgcolor="#FFFFCC" width="66%"><xsl:value-of select="Text"/></td>									
									</tr>
								</xsl:for-each>
							</td>
							</tr>							
							</table>							
							</td>
							</tr>
							<!--PASS Section placeholder
							<tr>
							</tr>
							-->
							<!--Fail Section placeholder
							<tr>
							</tr>
							-->						
						</table>
					</td>
				</tr>
				<!-- End Initialize Section -->

				<tr bordercolor="white" bgcolor="white"><td colspan="4"><hr color="white"></hr></td></tr>
				
				<!-- Start TestMethod Section -->
				<tr>
				<td colspan="4" align="left" bordercolor="black">
				<table width="100%" border="1" bordercolor="white">
					<tr>
					<td colspan="4" bordercolor="#C0C0C0" bgcolor="white">
					<a href="javascript:toggle('{$testName}');changeImage('{$imgTestName}');">
						<img border="0" src="Images/expand.gif" id="{$imgTestName}"/>
					</a>
					<b>   TestMethod Section</b>
					</td>
					</tr>
				</table>
				<table width="95%" align="center" id="{$testName}" style="display:none">
				<xsl:for-each select="TestMethod">
					<tr>
						<td colspan="4" align="center">
							<table bordercolor="white" border="1" width="100%">
								<tr>
									<td colspan="4" align="left" bordercolor="#C0C0C0" bgcolor="#F0FFF0">
         							 <xsl:variable name="testMethodName"><xsl:value-of select="concat($testName,@name)"/></xsl:variable>
         							 <xsl:variable name="imgTestMethodName"><xsl:value-of select="concat('img',$testMethodName)"/></xsl:variable>
										<a href="javascript:toggle('{$testMethodName}');changeImage('{$imgTestMethodName}');">
											<img border="0" src="Images/expand.gif" id="{$imgTestMethodName}"/>											
										</a>
										TestMethod: 
										<b><font size="2" face="Verdana"><xsl:value-of select="@name"/></font></b>										
									</td>
								</tr>
								<tr>
								<td>
								<table id="{$testMethodName}" width="100%" style="display:none;WORD-BREAK:BREAK-ALL;">
								  <xsl:for-each select="Comment">				
									<xsl:variable name="Type" select="@type" />
									<xsl:choose>
									<xsl:when test="$Type=0">
										<tr>
											<td bordercolor="#C0C0C0" bgcolor="#FFFFCC" width="10%"><xsl:value-of select="Date"/></td>
											<td bordercolor="#C0C0C0" bgcolor="#FFFFCC" width="13%"><xsl:value-of select="Time"/></td>
											<td bordercolor="#C0C0C0" bgcolor="#FFFFCC" width="11%"><b>Comment</b></td>
											<td bordercolor="#C0C0C0" bgcolor="#FFFFCC" width="66%"><xsl:value-of select="Text"/></td>
										</tr>
									</xsl:when>
									<xsl:when test="$Type=1">
										<tr>
											<td bordercolor="#C0C0C0" bgcolor="#FF8080" width="10%"><xsl:value-of select="Date"/></td>
											<td bordercolor="#C0C0C0" bgcolor="#FF8080" width="13%"><xsl:value-of select="Time"/></td>
											<td bordercolor="#C0C0C0" bgcolor="#FF8080" width="11%"><b>Exception</b></td>
											<td bordercolor="#C0C0C0" bgcolor="#FF8080" width="66%"><pre><xsl:value-of select="Text"/></pre></td>
										</tr>
									</xsl:when>
									<xsl:when test="$Type=99">
										<tr>
											<td bordercolor="#C0C0C0" bgcolor="#FFFFCC" width="10%"><xsl:value-of select="Date"/></td>
											<td bordercolor="#C0C0C0" bgcolor="#FFFFCC" width="13%"><xsl:value-of select="Time"/></td>
											<td bordercolor="#C0C0C0" bgcolor="#FFFFCC" width="11%"><b>Additional Information</b></td>
											<td bordercolor="#C0C0C0" bgcolor="#FFFFCC" width="66%"><xsl:value-of select="Text"/></td>
										</tr>
									</xsl:when>
									</xsl:choose>
									</xsl:for-each>													
														
									<xsl:for-each select="TestMethodResult">																	
										<xsl:choose>
											<xsl:when test="@Result='Pass'">
												<tr>
													<td bordercolor="#C0C0C0" bgcolor="#93FF93" width="10%"><xsl:value-of select="Date"/></td>
													<td bordercolor="#C0C0C0" bgcolor="#93FF93" width="13%"><xsl:value-of select="Time"/></td>
													<td bordercolor="#C0C0C0" bgcolor="#93FF93" width="11%"><b><xsl:value-of select="@Result"/></b></td>
													<td bordercolor="#C0C0C0" bgcolor="#93FF93" width="66%"><xsl:value-of select="Text"/></td>
												</tr>
											</xsl:when>
											<xsl:when test="@Result='Skip'">
												<tr>
													<td bordercolor="#C0C0C0" bgcolor="#00CCFF" width="10%"><xsl:value-of select="Date"/></td>
													<td bordercolor="#C0C0C0" bgcolor="#00CCFF" width="13%"><xsl:value-of select="Time"/></td>
													<td bordercolor="#C0C0C0" bgcolor="#00CCFF" width="11%"><b><xsl:value-of select="@Result"/></b></td>
													<td bordercolor="#C0C0C0" bgcolor="#00CCFF" width="66%"><xsl:value-of select="Text"/></td>
												</tr>
											</xsl:when>
											<xsl:when test="@Result='KnownFailure'">
												<tr>
													<td bordercolor="#C0C0C0" bgcolor="#FFCCCC" width="10%"><xsl:value-of select="Date"/></td>
													<td bordercolor="#C0C0C0" bgcolor="#FFCCCC" width="13%"><xsl:value-of select="Time"/></td>
													<td bordercolor="#C0C0C0" bgcolor="#FFCCCC" width="11%"><b><xsl:value-of select="@Result"/></b></td>
													<td bordercolor="#C0C0C0" bgcolor="#FFCCCC" width="66%"><xsl:value-of select="Text"/></td>
												</tr>
											</xsl:when>										
											<xsl:otherwise>
												<tr bordercolor="#C0C0C0" bgcolor="red">
													<td width="10%"><xsl:value-of select="Date"/></td>
													<td width="13%"><xsl:value-of select="Time"/></td>
													<td width="10%"><b><xsl:value-of select="@Result"/></b></td>
													<td width="67%" align="left"><xsl:value-of select="Text"/></td>													
													<script>expand('<xsl:value-of select = "$testName" />');</script>
													<script>expand('<xsl:value-of select = "$testMethodName" />');</script>
													<script>displayImage('<xsl:value-of select = "$imgTestName" />', 'Images/collapse.gif');</script>
													<script>displayImage('<xsl:value-of select = "$imgTestMethodName" />', 'Images/collapse.gif');</script>												
												</tr>										
											</xsl:otherwise>
										</xsl:choose>									
										<tr bordercolor="white">
											<td class="dotted" colspan="4">
												<hr color="black" size="1"  />
												<!-- <p style="border-bottom: 2px dotted #000000;"></p> -->
											</td>
										</tr>									
								  </xsl:for-each>	
								</table>
								</td>
								</tr>								
							</table>
						</td>
					</tr>
				</xsl:for-each>
				</table>
				</td>
				</tr>
				<!-- End TestMethod Section -->
				
				<tr bordercolor="white" bgcolor="white"><td colspan="4"><hr color="white"></hr></td></tr>
				
				<!-- Start CleanUp Section -->
				<tr>
					<td colspan="4" align="center">
						<table bordercolor="white" border="1" width="100%">
							<tr>
								<td colspan="4" align="left" bordercolor="#C0C0C0" bgcolor="white">
									<a href="javascript:toggle('{$cleanUpName}');changeImage('{$imgCleanUpName}');">
										<img border="0" src="Images/expand.gif" id="{$imgCleanUpName}"/>
									</a>
									<b><font size="2" face="Verdana">   Cleanup</font></b>
								</td>
							</tr>	
							<tr>
							<td>
							<table id="{$cleanUpName}" width="100%" style="display:none">
							<tr>
							<td>					
							<xsl:for-each select="CleanUp/Comment">
								<tr bordercolor="#C0C0C0" bgcolor="#FFFFCC">
									<td width="10%"><xsl:value-of select="Date"/></td>
									<td width="13%"><xsl:value-of select="Time"/></td>
									<td width="10%"><b>Comment</b></td>
									<td width="67%"><xsl:value-of select="Text"/></td>
								</tr>
							</xsl:for-each>
							</td>
							</tr>
							</table>
							</td>
							</tr>
							<!--PASS Section placeholder
							<tr>
							</tr>
							-->
							<!--Fail Section placeholder
							<tr>
							</tr>
							-->
						</table>
					</td>
				</tr>				
				<!-- End CleanUp Section -->
				
				<tr bordercolor="white" bgcolor="white"><td colspan="4"><hr color="white"></hr></td></tr>
								
				<!-- Start Results Section -->
				<tr>
					<td colspan="4" align="center">
						<table bordercolor="white" border="1" width="100%">
							<tr>
								<td colspan="4" align="left" bordercolor="#C0C0C0" bgcolor="white">
									<a href="javascript:toggle('{$resultName}');changeImage('{$imgResultName}');">
										<img border="0" src="Images/expand.gif" id="{$imgResultName}"/>
									</a>								
									<b><font size="2" face="Verdana">    Test Result</font></b>
								</td>
							</tr>
							<tr>
							<td>
							<table id="{$resultName}" width="100%" style="display:none">
							<tr>
							<td>	
							<tr bordercolor="black">
								<xsl:choose>
									<xsl:when test="Results/PassCount/Text='0'">
										<td colspan="4" align="center" bgcolor="red">
											<b>
												Total Tests Passed: <xsl:value-of select="Results/PassCount/Text"/>
											</b>
										</td>																															
										<script>expand('<xsl:value-of select = "$resultName" />');</script>
									</xsl:when>
									<xsl:otherwise>
										<td colspan="4" align="center" bgcolor="#93FF93">
											Total Tests Passed: <xsl:value-of select="Results/PassCount/Text"/>
										</td>																															
										<script>expand('<xsl:value-of select = "$resultName" />');</script>
									</xsl:otherwise>
								</xsl:choose>
							</tr>
							<tr bordercolor="black">
								<xsl:choose>
									<xsl:when test="Results/FailCount/Text='0'">
										<td colspan="4" align="center" bgcolor="#93FF93">
											Total Tests Failed: <xsl:value-of select="Results/FailCount/Text"/>
										</td>
										<script>expand('<xsl:value-of select = "$resultName" />');</script>	
										<script>changeImage('<xsl:value-of select = "$imgResultName" />');</script>	
									</xsl:when>
									<xsl:otherwise>
										<td colspan="4" align="center" bgcolor="red">
											<b>
												Total Tests Failed: <xsl:value-of select="Results/FailCount/Text"/>
											</b>
										</td>		
										<script>expand('<xsl:value-of select = "$resultName" />');</script>	
										<script>changeImage('<xsl:value-of select = "$imgResultName" />');</script>								
									</xsl:otherwise>
								</xsl:choose>
							</tr>
							<tr bordercolor="black">
								<xsl:choose>
									<xsl:when test="Results/SkipCount/Text='0'">
										<td colspan="4" align="center" bgcolor="#93FF93">
												Total Tests Skipped: <xsl:value-of select="Results/SkipCount/Text"/>
										</td>																															
										<script>expand('<xsl:value-of select = "$resultName" />');</script>
									</xsl:when>
									<xsl:otherwise>
										<td colspan="4" align="center" bgcolor="#00CCFF">
											<b>
												Total Tests Skipped: <xsl:value-of select="Results/SkipCount/Text"/>
											</b>
										</td>																															
										<script>expand('<xsl:value-of select = "$resultName" />');</script>
									</xsl:otherwise>
								</xsl:choose>
							</tr>
							<tr bordercolor="black">
								<xsl:choose>
									<xsl:when test="Results/KnownFailureCount/Text='0'">
										<td colspan="4" align="center" bgcolor="#93FF93">
											Total Tests with Known Failures: <xsl:value-of select="Results/KnownFailureCount/Text"/>
										</td>																															
										<script>expand('<xsl:value-of select = "$resultName" />');</script>
									</xsl:when>
									<xsl:otherwise>
										<td colspan="4" align="center" bgcolor="#FFCCCC">
											<b>
												Total Tests with Known Failures: <xsl:value-of select="Results/KnownFailureCount/Text"/>
											</b>
										</td>																															
										<script>expand('<xsl:value-of select = "$resultName" />');</script>
									</xsl:otherwise>
								</xsl:choose>
							</tr>
							</td>
							</tr>
							</table>
							</td>
							</tr>														
						</table>
					</td>
				</tr>
				<!-- End Results Section -->
				
			</td>
			</tr>
			</Table>
			<br />
			<br />
		</xsl:for-each>

		<!-- Start Device Status -->	
		<xsl:for-each select="DeviceStatus">
			<Table border="2" width="100%" bordercolor="black" bgcolor="#EAEAEA" align="center">							
				<tr>
					<td colspan="4" align="center" bordercolor="#FF8080">
						<tr>
							<td bordercolor="#C0C0C0" bgcolor="#FFFFCC" width="15%"><b>Device Status</b></td>
							<td bordercolor="#C0C0C0" bgcolor="#FFFFCC" width="85%"><b><xsl:value-of select="Text"/></b></td>
						</tr>
					</td>
				</tr>								
			</Table>												
		</xsl:for-each>
		<!-- End Device Status Section -->	

		<!--Log the Additional Information at the end-->
		<xsl:choose>
			<xsl:when test="count(AdditionalInformation)='0'">
			</xsl:when>
			<xsl:otherwise>
				<Table border="2" width="100%" bordercolor="black" bgcolor="#EAEAEA" align="center">
					<tr>				
						<td colspan="4" align="center" bgcolor="white">
							<b><font size="4" face="Verdana" color="#912F00">Additional Information</font></b>
						</td>
					</tr>		
				</Table>
				<xsl:for-each select="AdditionalInformation">
					<Table border="2" width="100%" bordercolor="black" bgcolor="#EAEAEA" align="center">
						<tr>				
							<td colspan="4" align="center" bgcolor="white">
								<b><font size="2" face="Verdana" color="#912F00"><xsl:value-of select="."/></font></b>
							</td>
						</tr>		
					</Table>
				</xsl:for-each>
			</xsl:otherwise>
		</xsl:choose>

		<!-- End TestLog Section -->
					
	</xsl:template>
</xsl:stylesheet>