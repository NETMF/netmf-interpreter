this is an example of how to use the signature facilities in the metadata processor to be uploaded to the device with the tinybooter
the bin file must be signed and upload must use the hex file 

the public key is heardcoded in the tinybooter and must match the private key

of course a decent Key management strategy generates the private key only once and does not keep it in the source tree 

use the sign_file script to sign files after building them
remember to sign the binary files and to upload the corresponding hex files
if the signature files are placed in the same directory where the hex files are and if they have the same name plus the ".sig" extension, than FlashLiteClient will load the signature automatically 

MetaDataProcessor.exe 
-dump_key tinybooter_public_key.bin 
-dump_key tinybooter_private_key.bin 
-sign_file %SPOCLIENT%_BUILD\arm\FLASH\release\AUXD\bin\tinyclr.bin\ER_DAT tinybooter_private_key.bin %SPOCLIENT%_BUILD\arm\FLASH\release\AUXD\bin\tinyclr.hex\ER_DAT.sig 
-verify_signature %SPOCLIENT%_BUILD\arm\FLASH\release\AUXD\bin\tinyclr.bin\ER_DAT tinybooter_public_key.bin %SPOCLIENT%_BUILD\arm\FLASH\release\AUXD\bin\tinyclr.hex\ER_DAT.sig
