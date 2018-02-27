using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Sandbox
{
    class Program
    {
        static void Main(string[] args)
        {
            NAudio.Wave.WaveFileReader wave = new NAudio.Wave.WaveFileReader(@"C:\Users\Christoph Royer\Documents\Visual Studio 2017\Projects\Artsy Stuff\Audioframe\bin\Debug\Rick Astley - Never Gonna Give You Up.wav");
            wave.Skip(30);
            for (int i = 0; i < 5000; i++)
            {
                float[] frame = wave.ReadNextSampleFrame();
                Console.WriteLine(frame[0]);
            }
            Console.ReadLine();
        }
    }
}
