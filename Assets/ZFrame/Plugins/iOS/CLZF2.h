#ifndef CCLZF2_H_
#define CCLZF2_H_
#include "Export.h"
typedef unsigned char CT_BYTE;
typedef unsigned int CT_UINT32;
typedef int CT_INT32;
typedef long long CT_INT64;

const CT_UINT32 HLOG=		14;
const CT_UINT32 HSIZE=		(1 << 14);
const CT_UINT32 MAX_LIT	=	(1 << 5);
const CT_UINT32 MAX_OFF	=	(1 << 13);
const CT_UINT32 MAX_REF	=	((1 << 8) + (1 << 3));
class CCLZF2
{

public:
	static CT_BYTE *Compress(CT_BYTE *inputBytes,CT_INT32 &iLen);
	//
	static CT_BYTE *Decompress(CT_BYTE *inputBytes,CT_INT32 &iLen);
	//
    static CT_INT32 lzf_compress(CT_BYTE *input,CT_BYTE *output,CT_INT32 iInLen,CT_INT32 iOutLen);
	//
	static CT_INT32 lzf_decompress(CT_BYTE *input, CT_BYTE *output,CT_INT32 iInLen,CT_INT32 iOutLen);

};

SM_EXPORTS int LZF_Compress(int **inbytes, int inLen, int **outbytes, int outlen);
SM_EXPORTS int LZF_Decompress(int **inbytes, int inLen, int **outbytes, int outlen);

SM_EXPORTS void Encrypt(char bytes[], int length);
SM_EXPORTS void Decrypt(char bytes[], int length);

#endif
