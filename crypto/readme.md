#### .NET Micro Framework Cryptographic libraries
The source of the crypto libraries owned by Micrsoft is not available publicly due to Microsoft security policies regarding Cryptographic technology.
The available components are not checked into the public repository. Instead they are made available as a separate binary download of pre-compiled
libraries. Developers can link to the libraries provided without requiring the source or, if they so choose, can replace them with their own private,
but compatible source implementations if desired. The build support in  the .NET Microframework will automatically select the cryptographic stub libraries
if the binary libraries are not installed.

The current release of the binaries is located on our [Github Releases page](https://github.com/NETMF/netmf-interpreter/releases)

##### Installing the libraries  
1. Download the MSI from the Github releases  
1. Double click on the downloaded MSI from Windows Explorer to start the installation
  When prompted for an installation location select the root of your NETMF source tree (e.g. the folder containing the setenv_xxx.cmd stripts )


