using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Es.Throttle;

namespace ConsoleTest
{
    public class RateLimterTest
    {
        private IOptions<MemoryDistributedCacheOptions> options =
             new OptionsWrapper<MemoryDistributedCacheOptions>(new MemoryDistributedCacheOptions());

        [Test]
        public void Example()
        {
            var rateLimiter = new RateLimiter(new RateLimitStore(new MemoryDistributedCache(options)));
            string identity = "identity";
            var rnd = new Random(Guid.NewGuid().GetHashCode());
            RateLimitResult result;
            var rateQuota = new RateQuota(100, 50, RateLimitPeriod.Min);
            for (int i = 0; i < 50; i++)
            {
                result = rateLimiter.RateLimit(rateQuota, identity, 30);
                Console.WriteLine(result);
                Thread.Sleep(rnd.Next(1000));
            }
        }

        [Test]
        public void StreamCopy()
        {
            var rateLimiter = new RateLimiter(new RateLimitStore(new MemoryDistributedCache(options)));
            string identity = "identity";
            var rnd = new Random(Guid.NewGuid().GetHashCode());

            var mem1 = new MemoryStream();
            mem1.SetLength(1 << 30);
            mem1.Seek(0, SeekOrigin.Current);

            var max = 1024 * 1024 * 1;

            var rateQuota = new RateQuota(max, max, RateLimitPeriod.Sec);

            var mem2 = new MemoryStream();

            var buffer = new byte[4096];
            Stopwatch sw = Stopwatch.StartNew();
            CancellationTokenSource _cts = new CancellationTokenSource();
            Console.WriteLine("Src:" + mem1.Length);
            int read;
            while ((read = mem1.Read(buffer, 0, buffer.Length)) != 0)
            {
                var ret = rateLimiter.RateLimit(rateQuota, identity, buffer.Length);
                if (ret)
                {
                    Console.WriteLine(ret);
                    _cts.Token.WaitHandle.WaitOne(ret.RetryAfter);
                }

                mem2.Write(buffer, 0, read);
            }

            Console.WriteLine("Dest:" + mem2.Length + " Time:" + sw.Elapsed);
        }
    }
}