/********************************************************************************
 *  DI2008
 * 
 *  Sample code for configuring a DI2008 data acquisition system and converting
 *  the streaming results.  Uses the DI2008 in CDC mode to send and receive 
 *  data over a serial port. 
 *  
 *  More on the DATAQ device protocol available here:
 *  https://www.dataq.com/resources/pdfs/misc/di-2008%20protocol.pdf
 *  
 */
using System.IO.Ports;

namespace di2008
{
    internal class Program
    {
        public const string COMPORT = "COM7";
        public const int SERIALDELAY = 250;
        public const int NUMCHAN = 3;


        static double TCFromADC(Int16 adcvalue, double m, double b)
        {
            return (double)adcvalue * m + b;
        }

        static double JTypeTCFromADC(Int16 adcvalue)
        {
            return TCFromADC(adcvalue, 0.021515, 495);
        }

        static double KTypeTCFromADC(Int16 adcvalue)
        {
            return TCFromADC(adcvalue, 0.023987, 586);
        }

        static void SendCommand(SerialPort sp, string cmd)
        {
            Console.WriteLine(cmd);
            sp.Write(cmd + "\r");
            Thread.Sleep(SERIALDELAY);
        }

        static void Main(string[] args)
        {
            UInt16 cfg;

            // open a clean serial port for communications to the daq device
            SerialPort sp = new SerialPort(COMPORT, 9600);
            sp.Open();
            Thread.Sleep(SERIALDELAY);
            sp.DiscardInBuffer();

            // stop the device so we can start over cleanly 
            SendCommand(sp, "reset");
            sp.DiscardInBuffer();

            // stop the device so we can start over cleanly 
            SendCommand(sp, "stop");
            sp.DiscardInBuffer();

            // identify the device 
            SendCommand(sp, "info");
            Console.WriteLine(sp.ReadExisting());

            // configure the channels be read
            cfg = 0x1200;
            SendCommand(sp, $"slist 0 {cfg}");
            SendCommand(sp, $"filter 0 0");

            // configure the channels be read
            cfg = 0x1201;
            SendCommand(sp, $"slist 1 {cfg}");
            SendCommand(sp, $"filter 1 0");

            // configure the channels be read
            cfg = 0x1202;
            SendCommand(sp, $"slist 2 {cfg}");
            SendCommand(sp, $"filter 2 0");

            // configure the sample rate to about 1/sec
            SendCommand(sp, "srate 100 2");
            sp.DiscardInBuffer();

            sp.Write("start\r");

            int b = 0;
            int c = 0;
            int adc = 0;

            while (true)
            {
                while (sp.BytesToRead > 0)
                {
                    int ch = sp.ReadByte();
                    Console.Write($"{ch:X2} ");
                    if (b == 0)
                    {
                        adc = ch;
                        b = 1;
                    }
                    else
                    {
                        adc = (adc + (ch << 8));
                        double tempC = JTypeTCFromADC((Int16)adc);
                        Console.WriteLine($"{c} {adc} = {tempC}\r");
                        b = 0;
                        c = c + 1;
                        if (c >= NUMCHAN)
                        {
                            c = 0;
                        }
                    }
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
