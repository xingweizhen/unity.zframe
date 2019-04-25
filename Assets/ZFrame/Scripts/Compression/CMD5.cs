using System;
using System.IO;
using System.Security.Cryptography;

public class CMD5
{

    public static string Token(string message, string secret)
    {
        if (secret == null) secret = string.Empty;

        var encoding = new System.Text.UTF8Encoding();
        var keyBytes = encoding.GetBytes(secret);
        using (var hmac = new HMACSHA256(keyBytes)) {
            var msgBytes = encoding.GetBytes(message);
            var hashBytes = hmac.ComputeHash(msgBytes);
            return Convert.ToBase64String(hashBytes);
        }
    }

    /// <summary>
    /// 计算文件的 MD5 值
    /// </summary>
    /// <param name="fileName">要计算 MD5 值的文件名和路径</param>
    /// <returns>MD5 值16进制字符串</returns>
    public static string MD5File(string fileName)
    {
        return HashFile(fileName, "md5");
    }

    /// <summary>
    /// 计算字符串的 MD5 值
    /// </summary>
    /// <param name="str">要计算的字符串</param>
    /// <returns>MD5 值16进制字符串</returns>
    public static string MD5String(string str)
    {
        return MD5Data(System.Text.Encoding.UTF8.GetBytes(str));
    }

    /// <summary>
	/// 计算数据的 MD5 值
    /// </summary>
    /// <param name="data">要计算的二进制数据</param>
    /// <returns>MD5 值16进制字符串</returns>
    public static string MD5Data(byte[] data)
    {
        return ByteArrayToHexString(HashData(data, "md5"));
    }

    public static string MD5Stream(Stream stream)
    {
        return ByteArrayToHexString(HashData(stream, "md5"));
    }

    /// <summary>
    /// 计算文件的哈希值
    /// </summary>
    /// <param name="fileName">要计算哈希值的文件名和路径</param>
    /// <param name="algName">算法:sha1,md5</param>
    /// <returns>哈希值16进制字符串</returns>
    public static string HashFile(string fileName, string algName)
    {
        if (!System.IO.File.Exists(fileName))
            return string.Empty;

        FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
        byte[] hashBytes = HashData(fs, algName);
        fs.Close();
        return ByteArrayToHexString(hashBytes);
    }

    /// <summary>
    /// 得到相应的哈希算法
    /// </summary>
    /// <param name="algName"></param>
    /// <returns></returns>
    static HashAlgorithm getAlgorithm(string algName)
    {
        HashAlgorithm algorithm;
        if (algName == null) {
            throw new ArgumentNullException("algName 不能为 null");
        }
        if (string.Compare(algName, "sha1", true) == 0) {
            algorithm = SHA1.Create();
        } else {
            if (string.Compare(algName, "md5", true) != 0) {
                throw new Exception("algName 只能使用 sha1 或 md5");
            }
            algorithm = MD5.Create();
        }
        return algorithm;
    }

    /// <summary>
    /// 计算哈希值
    /// </summary>
    /// <param name="stream">要计算哈希值的 Stream</param>
    /// <param name="algName">算法:sha1,md5</param>
    /// <returns>哈希值字节数组</returns>
    public static byte[] HashData(Stream stream, string algName)
    {
        return getAlgorithm(algName).ComputeHash(stream);
    }

    /// <summary>
    /// 计算哈希值
    /// </summary>
    /// <param name="data">要计算哈希值的数据</param>
    /// <param name="algName">算法:sha1,md5</param>
    /// <returns>哈希值字节数组</returns>
    public static byte[] HashData(byte[] data, string algName)
    {
        return getAlgorithm(algName).ComputeHash(data);
    }

    public static string ByteArrayToHexString(byte[] nbytes)
    {
        string ret = "";
        for (int i = 0; i < nbytes.Length; ++i) {
            ret += nbytes[i].ToString("x2");
        }
        return ret;
    }
}
