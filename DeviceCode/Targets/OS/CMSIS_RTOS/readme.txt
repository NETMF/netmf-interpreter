CMSIS_RTOS tree

Code in this part of the build tree is specific to the CMSIS-RTOS API specification but *must not* have
any dependency on any particular implementation of the API. That is, any code here should compile and 
run "as-is" without additional modifications when used with different implementations of the CMSIS-RTOS
API spec. The default implementation used for testing is the CMSIS-RTX implementation from ARM, however
the intent is to support additional implementations for any devices which have an alternative already
in place.