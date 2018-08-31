using System;
using System.Security.Cryptography;
using System.Text;
using System.Linq;

namespace Rainbow.DomainDriven.Core.Extensions
{
    public static class StringExtension
    {
        public static Guid ToGuid(this string source)
        {
            byte[] bytes;
            using (var md5 = MD5.Create())
            {
                bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(source));
            }
            var result = new StringBuilder();
            foreach (byte t in bytes)
            {
                result.Append(t.ToString("X2"));
            }

            return new Guid(result.ToString());
        }

        private static Byte[] _hostNameBytes;

        private static Byte[] HostNameBytes
        {
            get
            {
                if (_hostNameBytes == null || _hostNameBytes.Length != 7)
                {

                    var temp = System.Net.Dns.GetHostName().ToGuid().ToByteArray();
                    _hostNameBytes = temp.Take(7).ToArray();
                }
                return _hostNameBytes;
            }
        }
        private static long _seq = 0;
        private static long[] _seqs = new long[2];


        public static Guid ToSequenceGuid(this DateTime time)
        {
            var m = BitConverter.GetBytes(time.Ticks);
            //位运算 0 or 1 ,这里取ticks的末尾第二位作为随机数
            var index = (time.Ticks>>1) & 1;
            //为运算 1111 1111 获取0-255
            var e = (byte)((_seqs[index]++) & 0xFF);
            byte[] bytes = new byte[16];
            HostNameBytes.CopyTo(bytes, 0);
            m.CopyTo(bytes, 7);
            bytes[15] = e;
            //重制另一个为0
            _seqs[(index + 1) & 1] = 0;
            return new Guid(bytes);
        }
    }
}