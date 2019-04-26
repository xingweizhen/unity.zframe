#include "CLZF2.h"
#include <iostream>

CT_BYTE *CCLZF2::Compress(CT_BYTE *inputBytes,CT_INT32 &iLen)
{
	// Starting guess, increase it later if needed
	CT_INT32 outputByteCountGuess = iLen * 2;
	CT_BYTE *tempBuffer = new CT_BYTE[outputByteCountGuess];
	CT_INT32 byteCount = lzf_compress(inputBytes, tempBuffer,iLen,outputByteCountGuess);



	// If byteCount is 0, then increase buffer and try again
	while (byteCount == 0)
	{
		delete[] tempBuffer;
		tempBuffer=0;

		outputByteCountGuess *= 2;
		tempBuffer = new CT_BYTE[outputByteCountGuess];
		byteCount = lzf_compress(inputBytes, tempBuffer,iLen,outputByteCountGuess);
	}



	CT_BYTE *outputBytes = new CT_BYTE[byteCount];
	memcpy(outputBytes,tempBuffer,byteCount);
	iLen=byteCount;

	return outputBytes;

}





CT_BYTE *CCLZF2::Decompress(CT_BYTE *inputBytes,CT_INT32 &iLen)
{

	// Starting guess, increase it later if needed

	CT_INT32 outputByteCountGuess = iLen * 2;
	CT_BYTE *tempBuffer = new CT_BYTE[outputByteCountGuess];
	CT_INT32 byteCount = lzf_decompress(inputBytes,tempBuffer,iLen,outputByteCountGuess);



	// If byteCount is 0, then increase buffer and try again

	while (byteCount == 0)
	{
		delete[] tempBuffer;
		tempBuffer=0;

		outputByteCountGuess *= 2;
		tempBuffer = new CT_BYTE[outputByteCountGuess];
		byteCount = lzf_decompress(inputBytes, tempBuffer,iLen,outputByteCountGuess);

	}



	CT_BYTE *outputBytes = new CT_BYTE[byteCount];
	memcpy(outputBytes,tempBuffer,byteCount);
	iLen=byteCount;
	return outputBytes;

}

CT_INT32 CCLZF2::lzf_compress(CT_BYTE *input,CT_BYTE *output,CT_INT32 iInLen,CT_INT32 iOutLen)
{
	CT_INT32 inputLength = iInLen;
	CT_INT32 outputLength = iOutLen;
    CT_INT64 HashTable[HSIZE] = {0};
    
	memset(HashTable,0,sizeof(HashTable));
	memset(output,0,iOutLen);

	CT_INT64 hslot;
	CT_UINT32 iidx = 0;
	CT_UINT32 oidx = 0;
	CT_INT64 reference;
	CT_UINT32 hval = (CT_UINT32)(((input[iidx]) << 8) | input[iidx + 1]); // FRST(in_data, iidx);
	CT_INT64 off;
	CT_INT32 lit = 0;

	for (; ; )
	{
		if (iidx < inputLength - 2)
		{
			hval = (hval << 8) | input[iidx + 2];
			hslot = ((hval ^ (hval << 5)) >> (CT_INT32)(((3 * 8 - HLOG)) - hval * 5) & (HSIZE - 1));
			reference = HashTable[hslot];
			HashTable[hslot] = (CT_INT64)iidx;

			if ((off = iidx - reference - 1) < MAX_OFF
				&& iidx + 4 < inputLength
				&& reference > 0
				&& input[reference + 0] == input[iidx + 0]
			&& input[reference + 1] == input[iidx + 1]
			&& input[reference + 2] == input[iidx + 2]
			)
			{

				/* match found at *reference++ */

				CT_UINT32 len = 2;
				CT_UINT32 maxlen = (CT_UINT32)inputLength - iidx - len;
				maxlen = (maxlen > MAX_REF ? MAX_REF : maxlen);

				if (oidx + lit + 1 + 3 >= outputLength)
					return 0;

				do
				{
					len++;
				}
				while (len < maxlen && input[reference + len] == input[iidx + len]);

				if (lit != 0)
				{
					output[oidx++] = (CT_BYTE)(lit - 1);
					lit = -lit;
					do
					{
						output[oidx++] = input[iidx + lit];
					}
					while ((++lit) != 0);

				}

				len -= 2;
				iidx++;

				if (len < 7)
				{
					output[oidx++] = (CT_BYTE)((off >> 8) + (len << 5));
				}
				else
				{
					output[oidx++] = (CT_BYTE)((off >> 8) + (7 << 5));
					output[oidx++] = (CT_BYTE)(len - 7);
				}

				output[oidx++] = (CT_BYTE)off;
				iidx += len - 1;
				hval = (CT_UINT32)(((input[iidx]) << 8) | input[iidx + 1]);
				hval = (hval << 8) | input[iidx + 2];

				HashTable[((hval ^ (hval << 5)) >> (CT_INT32)(((3 * 8 - HLOG)) - hval * 5) & (HSIZE - 1))] = iidx;
				iidx++;

				hval = (hval << 8) | input[iidx + 2];
				HashTable[((hval ^ (hval << 5)) >> (CT_INT32)(((3 * 8 - HLOG)) - hval * 5) & (HSIZE - 1))] = iidx;
				iidx++;

				continue;
			}
		}

		else if (iidx == inputLength)
			break;

		/* one more literal byte we must copy */
		lit++;
		iidx++;

		if (lit == MAX_LIT)
		{
			if (oidx + 1 + MAX_LIT >= outputLength)
				return 0;

			output[oidx++] = (CT_BYTE)(MAX_LIT - 1);
			lit = -lit;

			do
			{
				output[oidx++] = input[iidx + lit];
			}
			while ((++lit) != 0);
		}
	}

	if (lit != 0)
	{
		if (oidx + lit + 1 >= outputLength)
			return 0;

		output[oidx++] = (CT_BYTE)(lit - 1);
		lit = -lit;

		do
		{
			output[oidx++] = input[iidx + lit];
		}
		while ((++lit) != 0);

	}



	return (CT_INT32)oidx;

}

CT_INT32 CCLZF2::lzf_decompress(CT_BYTE *input, CT_BYTE *output,CT_INT32 iInLen,CT_INT32 iOutLen)
{

	 CT_INT32 inputLength = iInLen;
	 CT_INT32 outputLength = iOutLen;
	CT_UINT32 iidx = 0;
	CT_UINT32 oidx = 0;

	memset(output,0,iOutLen);

	do
	{
		CT_UINT32 ctrl = (CT_BYTE)input[iidx++];

		if (ctrl < (1 << 5)) /* literal run */
		{
			ctrl++;
			if (oidx + ctrl > outputLength)
			{
				//SET_ERRNO (E2BIG);
				return 0;
			}

			do
			{
				output[oidx++] = input[iidx++];
			}
			while ((--ctrl) != 0);

		}

		else /* back reference */
		{
			CT_UINT32 len = ctrl >> 5;
			CT_INT32 reference = (int)(oidx - ((ctrl & 0x1f) << 8) - 1);

			if (len == 7)
				len += input[iidx++];

			reference -= input[iidx++];

			if (oidx + len + 2 > outputLength)
			{
				//SET_ERRNO (E2BIG);
				return 0;
			}

			if (reference < 0)
			{
				//SET_ERRNO (EINVAL);
				return 0;

			}

			output[oidx++] = output[reference++];
			output[oidx++] = output[reference++];

			do
			{
				output[oidx++] = output[reference++];
			}
			while ((--len) != 0);
		}

	}

	while (iidx < inputLength);

	return (CT_INT32)oidx;

}

int activation;

int LZF_Compress(int **inbytes, int inLen, int **outbytes, int outlen)
{
    if (activation != ACTIVATION) return 0;
    return CCLZF2::lzf_compress((unsigned char *)*inbytes, (unsigned char *)*outbytes, inLen, outlen);
}
int LZF_Decompress(int **inbytes, int inLen, int **outbytes, int outlen)
{
    if (activation != ACTIVATION) return 0;
    return CCLZF2::lzf_decompress((unsigned char *)*inbytes, (unsigned char *)*outbytes, inLen, outlen);
}

void Encrypt(char bytes[], int length)
{
    if (activation != ACTIVATION) return;
    if (bytes == NULL || length == 0) return;
    int mask = ~length;
    int index = 4;
    int limit = length - sizeof(int);
    while (index < limit) {
        char *p = bytes + index - 1;
        int origin = *(int *)p;
        *(int *)p = origin ^ mask;
        index *= 2;
    }
}

void Decrypt(char bytes[], int length)
{
    if (activation != ACTIVATION) {
        if (bytes == NULL) {
            activation = length;
        } else {
            activation <<= length;
        }
    } else {
        if (bytes != NULL && length != 0) {
            int mask = ~length;
            int index = 4;
            int limit = length - sizeof(int);
            while (index < limit) {
                char *p = bytes + index - 1;
                int origin = *(int *)p;
                *(int *)p = origin ^ mask;
                index *= 2;
            }
        }
    }
}
