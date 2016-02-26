The WinPcap_Eth driver interface is dependent on library files from the
WinPcap Developer Pack. These files are not included in the NETMF source
distribution. In order to sucesfully compile and use the WinPCAP Ethernet
driver for NETMF you first need to download the WinPcap Developer Pack
from:

http://www.winpcap.org/devel.htm

Unzip the contents and copy the "WpdPack" folder and its subfolders to:
"$(SPOCLIENT)\DeviceCode\Targets\OS\Win32\DeviceCode\WinPcap_Eth\Dependencies\"

The final directory structure should look like this:

$(SPOCLIENT)\DeviceCode\Targets\OS\Win32\DeviceCode\WinPcap_Eth\Dependencies\WpdPack\
|-- docs
|-- Examples-pcap
|-- Examples-remote
|-- Include
|-- Lib