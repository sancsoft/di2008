using System.IO.Ports;
using System.Threading;

namespace di2008
{
    internal class Program
    {
        public const int SERIALDELAY = 1000;

        static void Main(string[] args)
        {
            // open a clean serial port for communications to the daq device
            SerialPort sp = new SerialPort("COM7", 9600);
            sp.Open();
            Thread.Sleep(SERIALDELAY);
            sp.DiscardInBuffer();

            // stop the device so we can start over cleanly 
            sp.Write("stop\r");
            Thread.Sleep(SERIALDELAY);
            sp.DiscardInBuffer();

            // identify the device 
            sp.Write("info\r");
            Thread.Sleep(SERIALDELAY);
            Console.WriteLine(sp.ReadExisting());

            // configure the channels be read
            sp.Write("slist 0 4608\r");
            Thread.Sleep(SERIALDELAY);

            sp.Write("filter 0 0\r");
            Thread.Sleep(SERIALDELAY);

            // configure the sample rate to about 1/sec
            sp.Write("srate 400 2\r");
            Thread.Sleep(SERIALDELAY);
            Thread.Sleep(SERIALDELAY);

            sp.DiscardInBuffer();

            sp.Write("start\r");

            while (true)
            {
                while (sp.BytesToRead > 0)
                {
                    int ch = sp.ReadByte();
                    Console.Write($"{ch:X2} ");
                }
                if (Console.KeyAvailable)
                {
                    if (Console.ReadKey().Key == ConsoleKey.Escape) 
                    {
                        break;
                    }
                }
            }
            sp.Write("stop\r");

            sp.Close();

        }
    }
}
