#include <tinyhal.h>
#include <mfupdate_decl.h>


static const IUpdatePackage s_UpdatePackages[] =
{
    {
        "NetMF",
        NULL,
        NULL,
        NULL,
        NULL,
    }, 
};

const IUpdatePackage* g_UpdatePackages     = s_UpdatePackages;
const INT32           g_UpdatePackageCount = ARRAYSIZE(s_UpdatePackages);


