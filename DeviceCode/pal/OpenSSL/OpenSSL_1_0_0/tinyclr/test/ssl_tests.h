#ifndef SSL_TESTS_H
#define SSL_TESTS_H

int ssl_test_bf(int, char**);
int ssl_test_bn(int, char**);
int ssl_test_exp(int, char**);
int ssl_test_cast(int, char**);
int ssl_test_des(int, char**);
int ssl_test_dh(int, char**);
int ssl_test_dsa(int, char**);
int ssl_test_ec(int, char**);
int ssl_test_ecdh(int, char**);
int ssl_test_ecdsa(int, char**);
int ssl_test_engine(int, char**);
int ssl_test_evp(int, char**);
int ssl_test_hmac(int, char**);
int ssl_test_idea(int, char**);
//int ssl_test_lhash(int, char**);
int ssl_test_md2(int, char**);
int ssl_test_md4(int, char**);
int ssl_test_md5(int, char**);
int ssl_test_pqueue(int, char**);
int ssl_test_rand(int, char**);
int ssl_test_rc2(int, char**);
int ssl_test_rc4(int, char**);
int ssl_test_rc5(int, char**);
int ssl_test_ripemd(int, char**);
int ssl_test_rsa(int, char**);
int ssl_test_sha(int, char**);
int ssl_test_sha1(int, char**);
int ssl_test_sha256(int, char**);
int ssl_test_sha512(int, char**);
int ssl_test_whrlpool(int, char**);
int ssl_test_x509v3(int, char**);

#endif
