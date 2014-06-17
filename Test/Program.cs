using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Single.Core;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch sw = new Stopwatch();
            Rom r = new Rom(@"D:\Hacking\Romhacking\Ressources\Smaragd\original\emer.gba");
            for (int i = 0; i < 10; ++i)
            {
                sw.Start();
                int[] array =
                r.FindByteSequence(new byte[] {0xDE, 0xAD, 0xC0, 0xDE}).ToArray();
                sw.Stop();
                Console.WriteLine("New Algorithm: {0}", sw.ElapsedMilliseconds);
                sw.Reset();
                sw.Start();
                array = 
                    r.FindByteSequenceOld(new byte[] { 0xDE, 0xAD, 0xC0, 0xDE }).ToArray();
                sw.Stop();
                Console.WriteLine("Old Algorithm: {0}", sw.ElapsedMilliseconds);
                sw.Reset();
            }
            Console.ReadLine();
        }
    }
}
