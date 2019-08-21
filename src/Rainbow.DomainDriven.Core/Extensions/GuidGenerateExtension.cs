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

        private static Random rm = new Random();
        private readonly static int _size = 5;
        private static Byte[] _hostNameBytes;

        private static Byte[] HostNameBytes
        {
            get
            {
                if (_hostNameBytes == null || _hostNameBytes.Length != _size)
                {
                    //生成一个随机跳过位 32- size -1
                    var skip = rm.Next(24);
                    var temp = System.Net.Dns.GetHostName().ToGuid().ToByteArray();
                    _hostNameBytes = temp.Skip(skip).Take(_size).ToArray();
                }
                return _hostNameBytes;
            }
        }
        private static long[] _seqs = new long[3];


        public static Guid ToSequenceGuid(this DateTime time)
        {
            var timeBytes = BitConverter.GetBytes(time.Ticks);
            //为运算 1111 1111 获取0-65535
            int rx = rm.Next(65535);
            byte[] bytes = new byte[16];

            //"03 02 01 00 - 05 04 - 07 06 - 08 09 - 0a 0b 0c 0d 0e 0f"
            var timeReveseBytes = timeBytes.Skip(4).Take(4)
                .Union(timeBytes.Skip(2).Take(2))
                .Union(timeBytes.Take(2))
                .ToArray();

            timeReveseBytes.CopyTo(bytes, 0);
            HostNameBytes.CopyTo(bytes, 8);
            bytes[13] = (byte)((_seqs[0]++) & 0xFF);
            bytes[14] = (byte)(rx & 0xFF);
            bytes[15] = (byte)((rx >> 8) & 0xFF);
            return new Guid(bytes);
        }
    }
}