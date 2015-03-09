Part of the source in the crypto libraries is not available publicly. The available components are checked in in the SPOT source depot

Programmers won't normally need to recompile the libraries; they can link to the binaries available in lib\arm for the arm code and lib\x86 for the Windows version.

To compile the crypto libraries you need access to the ENIGMA source depot server. 
To get access you need to get in touch with the LCA group.

If you have access to the ENIGMA source depot, you need to create a directory named private_crypto under the Crypto directory and
create a sd.ini file there, pointing SD to ENIGMA

Use the sd client command to map //depot/spot/netmf_v4_0_dev to %SPOCLIENT%\crypto\private_crypto, and do a full sync.

run build_crypto.cmd to rebuild all of the libraries.

