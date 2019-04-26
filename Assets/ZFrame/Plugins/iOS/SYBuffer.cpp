// SYBuffer.cpp : 定义 DLL 应用程序的导出函数。
//
#if _MSC_VER // this is defined when compiling with Visual Studio
#define EXPORT_API __declspec(dllexport) // Visual Studio needs annotating exported functions with this
#else
#define EXPORT_API // XCode does not need annotating exported functions, so define is empty
#endif

#include "Export.h"

extern int activation;

int VER2LEN[] = {0,1,2,4};

extern "C" {
	int EXPORT_API getVer() {
		return 3;
	}

	void EXPORT_API putU32(unsigned char buf[], int* pPos, int value){
        if (activation != ACTIVATION) return;
        
		char h = 0x00;
		if (value < 0)
		{
			h = 0x40;
			value *= -1;
		}
		if (value < 0x00000040)
		{
			buf[(*pPos)++] = (value & 0x3F) | h;
			return;
		}
		h |= 0x80;
		if (value < 0x00002000)
		{
			buf[(*pPos)++] = (value & 0x3F) | h;
			buf[(*pPos)++] = (value >> 6) & 0x7F;
		}
		else if (value < 0x00100000)
		{
			buf[(*pPos)++] = (value & 0x3F) | h;
			buf[(*pPos)++] = (value >> 6) | 0x80;
			buf[(*pPos)++] = (value >> 13) & 0x7F;
		}
		else if (value < 0x08000000)
		{
			buf[(*pPos)++] = (value & 0x3F) | h;
			buf[(*pPos)++] = (value >> 6) | 0x80;
			buf[(*pPos)++] = (value >> 13) | 0x80;
			buf[(*pPos)++] = (value >> 20) & 0x7F;
		}
		else
		{
			buf[(*pPos)++] = (value & 0x3F) | h;
			buf[(*pPos)++] = (value >> 6) | 0x80;
			buf[(*pPos)++] = (value >> 13) | 0x80;
			buf[(*pPos)++] = (value >> 20) | 0x80;
			buf[(*pPos)++] = (value >> 27) & 0x7F;
		}
	}

	void EXPORT_API putU64(unsigned char buf[], int* pos, long long value)
    {
        if (activation != ACTIVATION) return;
        
		putU32(buf,pos,(int)(value >> 32));
		putU32(buf,pos,(int)value);
	}

	int EXPORT_API getU32(unsigned char buf[], int* pPos)
    {
        if (activation != ACTIVATION) return 0;
        
		int result = buf[(*pPos)++];
		bool fat = 0 < (result & 0x00000040);
		if (0 < (result & 0x00000080))
		{
			result = (result & 0x0000003f) | buf[(*pPos)++] << 6;
			if (0 < (result & 0x00002000))
			{
				result = (result & 0x00001fff) | buf[(*pPos)++] << 13;
				if (0 < (result & 0x00100000))
				{
					result = (result & 0x000fffff) | buf[(*pPos)++] << 20;
					if (0 < (result & 0x08000000))
					{
						result = (result & 0x07ffffff) | buf[(*pPos)++] << 27;
					}
				}
			}
		}
		else
		{
			result = result & 0x0000003f;
		}
		return fat ? -result : result;
	}

	long long EXPORT_API getU64(unsigned char buf[], int* pPos)
    {
        if (activation != ACTIVATION) return 0;
        
		long long value = getU32(buf,pPos);
		value = value << 32;
		value = value | (getU32(buf,pPos) & 0xFFFFFFFFL);
		return value;
	}

	char EXPORT_API readHead(unsigned char buf[], int* len)
	{
		if (activation != ACTIVATION) return 0;
		char ver = buf[1] & 0x03;
		*len = VER2LEN[ver];
		return ver;
	}

	int EXPORT_API readLen(unsigned char buf[], char ver)
	{
		if (activation != ACTIVATION) return 0;
		int n = 0;
		if (ver == 1){
			n = buf[0];
		}
		else if (ver == 2){
			n = buf[0];
			n = (n << 8) | buf[1];
		}
		else if (ver == 3){
			n = buf[0];
			n = (n << 8) | buf[1];
			n = (n << 8) | buf[2];
			n = (n << 8) | buf[3];
		}
		return n;
	}

	int EXPORT_API putHead(unsigned char buf[], unsigned char cheker, int len, int flag){
		if (activation != ACTIVATION) return 0;
		int offset = 0;
		int ver = 3;
		if(len < 127)
		{
			offset = 3;
			ver = 1;
			buf[offset+2] = len;
		}
		else if(len < 32767){
			offset = 2;
			ver = 2;
			buf[offset+2] = len>>8;
			buf[offset+3] = len;
		}
		else{
			buf[offset+2] = len>>24;
			buf[offset+3] = len>>16;
			buf[offset+4] = len>>8;
			buf[offset+5] = len;
		}

		for (int i = offset + 2; i < len + 6; i++){
			cheker ^= buf[i];
		}

		//len = (ver << 6) | (len & 0x3F);
		buf[offset + 1] = (flag << 2) | ver;
		cheker ^= buf[offset + 1];
		buf[offset] = cheker;
		return offset;
	}


	int EXPORT_API putHands(unsigned char buf[], int len) {
		unsigned char cheker = 0x70;

		for (int i = 1; i < len; i++) {
			cheker ^= buf[i];
		}
		buf[0] = cheker;
		return len;
	}
}
