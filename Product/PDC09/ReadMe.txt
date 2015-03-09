Azure Account Info:
*****************************
http://www.microsoft.com/windowsazure/account/

UserName: NetMF_PDC09@live.com
Password: Azure4.NetMF

.Net Services Account
*****************************
(use live account above)
https://portal.ex.azure.microsoft.com/view.aspx


Intallation items:
*****************************
1) VS2008 SP1
2) Silverlight 3 SDK
3) Silverlight 3 Tools for Visual Studio
4) Silverlight 3 TookKit (oct 2009)
5) Windows Azure SDK
6) Windows Azure Tools for Visual Studio (CTP)
7) Expression Studio 3
8) .Net Services SDK (july 2009 CPTP)

Enable IIS
*****************************
1) Control Panel -> Programs and Features -> Turn Windows Features on or off
2) Internet Information Services -> WWW services -> Application Development Features 
    Select Asp, Asp.net and CGI

Running the Demo
*****************************
1) Load the WineMonditorDemo.sln 
2) Set the WineMonitorService project as the startup project
3) In the "Web" tab of the project properties page for the WineMonitorService project select "Specific Page" radio and select SilverlightWineMonitorAppTestPage.aspx
4) Press F5 
5) Open the MF solution WineCabinetDevice.sln
6) Press F5 



Changing the WCF contracts
*****************************

1) Generate wsdl files for Device
	svcutil.exe /target:Metadata http://localhost:52305/WineMonitorUpdate.svc?wsdl
	svcutil.exe /target:Metadata http://localhost:56152/WineMonitorDevice.svc?wsdl

	copy *.wsdl Device\WineSensorDevice\
	copy *.xsd  Device\WineSensorDevice\

2) Generate Device code from wsdl

	MFSvcUtil.exe *.wsdl *.xsd /D:<DIR> /O:<outFileName> /P:Microframework /V

3) Generate service proxy code for device service

	svcutil.exe /ser:XmlSerializer *.wsdl *.xsd /out:<NAME>Client.cs /config:Client_app.config

	eg. svcUtil.exe /ser:XmlSerializer localhost.WineMonitorDevice.wsdl localhost.WineMonitorDevice.xsd localhost.WineMonitorService.xsd schemas.microsoft.com.2003.10.Serialization.Arrays.xsd schemas.microsoft.com.2003.10.Serialization.xsd WineMonitorDevice.xsd WineMonitorService.xsd /out:localhost.WineMonitorDevice.cs  /config:client_app.config


	copy <Name>Client.cs Server\WineMontitorService\

4) Update the silverligth application by right clicking on "Service References\WineMonitorService" node and selecting "Update Service Reference"


5) build everything


**** Optional for WPF desktop app ***
6) Create and copy service proxy for WPF client application

 	svcutil.exe http://localhost:52305/WineMonitorService.svc?wsdl

	copy WineMonitorClient.cs Desktop\WineMonitorApp\


