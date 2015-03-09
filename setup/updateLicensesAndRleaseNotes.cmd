REM 
REM USAGE:  updateLicensesAndRleaseNotes.cmd <open license for sources> <release notes for every installer> <binaries license for crytpto and tcp binaries> 
REM 


copy %3 "PKProduct\Firmware License.rtf"
copy %3 "PKProduct\License.rtf"
copy %2 "PKProduct\ReleaseNotes.txt"
copy %4 "PKProduct\PKWelcome.rtf"

copy %3 "PKProductCrypto\Firmware License (Crypto).rtf"
copy %3 "PKProductCrypto\License (Crypto).rtf"
copy %2 "PKProductCrypto\ReleaseNotesCrypto.txt"
copy %4 "PKProductCrypto\PKCryptoWelcome.rtf"

copy %3 "PKProductNetARM\Firmware License (ARM Network).rtf"
copy %3 "PKProductNetARM\License (ARM Network).rtf"
copy %2 "PKProductNetARM\ReleaseNotesArmNetwork.txt"
copy %4 "PKProductNetARM\PKNetArmWelcome.rtf"

copy %3 "PKProductNetThumb\Firmware License (Thumb Network).rtf"
copy %3 "PKProductNetThumb\License (Thumb Network).rtf"
copy %2 "PKProductNetThumb\ReleaseNotesThumbNetwork.txt"
copy %4 "PKProductNetThumb\PKNetThumbWelcome.rtf"

copy %3 "PKProductNetThumb2\Firmware License (Thumb2 Network).rtf"
copy %3 "PKProductNetThumb2\License (Thumb2 Network).rtf"
copy %2 "PKProductNetThumb2\ReleaseNotesThumb2Network.txt"
copy %4 "PKProductNetThumb2\PKNetThumb2Welcome.rtf"

copy %1 "PKProductNoLibs\Firmware License.rtf"
copy %1 "PKProductNoLibs\License.rtf"
copy %2 "PKProductNoLibs\ReleaseNotes.txt"
copy %4 "PKProductNoLibs\PKWelcome.rtf"

copy %1 "ProductSDK\License.rtf"


