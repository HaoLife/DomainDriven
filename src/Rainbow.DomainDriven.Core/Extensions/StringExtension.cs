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

        private static Byte[] _hostNameGuid;

        private static Byte[] HostNameGuid
        {
            get
            {
                if (_hostNameGuid == null || _hostNameGuid.Length != 7)
                {

                    var temp = System.Net.Dns.GetHostName().ToGuid().ToByteArray();
                    _hostNameGuid = temp.Take(7).ToArray();
                }
                return _hostNameGuid;
            }
        }
        private static long _seq = 0;
        private static long[] _seqs = new long[2];

        public static Guid ToSeqGuid()
        {
            var t = HostNameGuid;
            var m = BitConverter.GetBytes(DateTime.Now.Ticks);
            var index = DateTime.Now.Millisecond % 2;
            var e = (_seqs[index]++) % 128;
            var s = (byte)(e & 0xFF);
            byte[] bytes = new byte[16];
            t.CopyTo(bytes, 0);
            m.CopyTo(bytes, 7);
            bytes[15] = s;
            //重制另一个为0
            _seqs[(index + 1) % 2] = 0;
            return new Guid(bytes);

        }
    }
}