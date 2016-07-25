#include "e_os.h"
#include <tinyhal.h>
#include "ssl_types.h"
#include <tinyclr/ssl_functions.h>
#include <openssl/asn1.h>
#include <openssl/pem.h>
#include <openssl/err.h>
#include <openssl/pkcs12.h>

#if defined(DEBUG) || defined(_DEBUG)
static const char *mon[12] =
{ 
    "Jan","Feb","Mar","Apr","May","Jun",
    "Jul","Aug","Sep","Oct","Nov","Dec"
};
#endif

static int ssl_get_ASN1_UTCTIME(const ASN1_UTCTIME *tm, DATE_TIME_INFO *dti);

static int password_cb(char *buf, int size, int rwflag, void *userdata)
{
    const char* password = (const char*)userdata;
    int res = TINYCLR_SSL_STRLEN(password);

    if (res > size)
        res = size;
    
    TINYCLR_SSL_MEMCPY(buf, password, res);
    
    return res;
}

X509* ssl_parse_certificate(void* pCert, size_t certLen, LPCSTR pwd, EVP_PKEY** privateKey)
{
    X509 *x=NULL;
    PKCS12* p12 = NULL;

    if(privateKey != NULL)
    {
        *privateKey = NULL;
    }
    
    //check for PEM BASE64 header
    BIO *certBio = NULL;
    if ((certBio=BIO_new(BIO_s_mem())) != NULL)
    {
        BIO_write(certBio, (const char*)pCert, certLen);
    
        x = PEM_read_bio_X509(certBio, NULL, 0, NULL);
        
        if(x != NULL)
        {
            BIO_reset(certBio);
            BIO_write(certBio, (const char*)pCert, certLen);

            if(privateKey != NULL)
            {
                *privateKey = PEM_read_bio_PrivateKey(certBio, NULL, password_cb, (void*)pwd);
            }
        }
    
        if(x == NULL)
        {
            BIO_reset(certBio);
            BIO_write(certBio, (const char*)pCert, certLen);
    
            p12 = d2i_PKCS12_bio(certBio, NULL);
    
            if(p12 != NULL)
            {
                if(privateKey == NULL)
                {
                    PKCS12_parse(p12, pwd, NULL, &x, NULL); 
                }
                else
                {
                    PKCS12_parse(p12, pwd, privateKey, &x, NULL); 
                }
    
                PKCS12_free(p12);
            }
        }
    
        BIO_free(certBio);
    }
    
    if(x == NULL)
    {
        const UINT8* tmp = (const UINT8*)pCert;
        
        x = d2i_X509(NULL, &tmp, certLen);

        if(x != NULL && privateKey != NULL)
        {
            X509_PKEY* pKey;
            
            tmp = (const UINT8*)pCert;
            
            pKey = d2i_X509_PKEY(NULL, &tmp, certLen);            

            if(pKey != NULL)
            {
                *privateKey = pKey->dec_pkey;
            }
        }
    }
    
    return x;
    
}

BOOL ssl_parse_certificate_internal(void * bytes, size_t size, void* pwd, void* x509CertData)
{
    char *name,*subject;
    X509CertData* x509 = (X509CertData*)x509CertData;
    X509* x = ssl_parse_certificate(bytes, size, (LPCSTR)pwd, NULL);

    if (x == NULL)
    {
        TINYCLR_SSL_PRINTF("Unable to load certificate\n");
        ERR_print_errors_fp(OPENSSL_TYPE__FILE_STDERR);
        return FALSE;
    }
    
    name=X509_NAME_oneline(X509_get_issuer_name(x),NULL,0);
    subject=X509_NAME_oneline(X509_get_subject_name(x),NULL,0);

    TINYCLR_SSL_STRNCPY(x509->Issuer, name, TINYCLR_SSL_STRLEN(name));
    TINYCLR_SSL_STRNCPY(x509->Subject, subject, TINYCLR_SSL_STRLEN(subject));

    ssl_get_ASN1_UTCTIME(X509_get_notBefore(x), &x509->EffectiveDate);
    ssl_get_ASN1_UTCTIME(X509_get_notAfter(x), &x509->ExpirationDate);

#if defined(DEBUG) || defined(_DEBUG)
    TINYCLR_SSL_PRINTF("\n        Issuer: ");
    TINYCLR_SSL_PRINTF(name);
    TINYCLR_SSL_PRINTF("\n");
    TINYCLR_SSL_PRINTF("        Validity\n");
    TINYCLR_SSL_PRINTF("            Not Before: ");
    TINYCLR_SSL_PRINTF("%s %2d %02d:%02d:%02d %d%s",
        mon[x509->EffectiveDate.month-1],
        x509->EffectiveDate.day,
        x509->EffectiveDate.hour,
        x509->EffectiveDate.minute,
        x509->EffectiveDate.second,
        x509->EffectiveDate.year,
        (x509->EffectiveDate.tzOffset)?" GMT":"");
    TINYCLR_SSL_PRINTF("\n            Not After : ");
    TINYCLR_SSL_PRINTF("%s %2d %02d:%02d:%02d %d%s",
        mon[x509->ExpirationDate.month-1],
        x509->ExpirationDate.day,
        x509->ExpirationDate.hour,
        x509->ExpirationDate.minute,
        x509->ExpirationDate.second,
        x509->ExpirationDate.year,
        (x509->ExpirationDate.tzOffset)?" GMT":"");
    TINYCLR_SSL_PRINTF("\n");
    TINYCLR_SSL_PRINTF("        Subject: ");
    TINYCLR_SSL_PRINTF(subject);
    TINYCLR_SSL_PRINTF("\n");
#endif

    OPENSSL_free(name);
    OPENSSL_free(subject);

    X509_free(x);

    return TRUE;
}

//MS: copied decoding algo from get_ASN1_UTCTIME of asn1_openssl.lib
//MS: populate DATE_TIME_INFO struct with year,month, day,hour,minute,second,etc
static int ssl_get_ASN1_UTCTIME(const ASN1_UTCTIME *tm, DATE_TIME_INFO *dti)
{
    const char *v;
    int gmt=0;
    int i;
    int y=0,M=0,d=0,h=0,m=0,s=0;

    i=SSL_LONG_LITTLE_ENDIAN(tm->length);
    v=(const char *)tm->data;

    if (i < 10) { goto err; }
    if (v[i-1] == 'Z') gmt=1;
    for (i=0; i<10; i++)
        if ((v[i] > '9') || (v[i] < '0')) { goto err; }
    y= (v[0]-'0')*10+(v[1]-'0');
    if (y < 50) y+=100;
    M= (v[2]-'0')*10+(v[3]-'0');
    if ((M > 12) || (M < 1)) { goto err; }
    d= (v[4]-'0')*10+(v[5]-'0');
    h= (v[6]-'0')*10+(v[7]-'0');
    m=  (v[8]-'0')*10+(v[9]-'0');
    if (tm->length >=12 &&
        (v[10] >= '0') && (v[10] <= '9') &&
        (v[11] >= '0') && (v[11] <= '9'))
        s=  (v[10]-'0')*10+(v[11]-'0');

    dti->year = y+1900;
    dti->month = M;
    dti->day = d;
    dti->hour = h;
    dti->minute = m;
    dti->second = s;
    dti->dlsTime = 0; //TODO:HOW to find
    dti->tzOffset = gmt; //TODO:How to find

    return(1);

    
err:
    TINYCLR_SSL_PRINTF("Bad time value\r\n");
    return(0);
}
