using System;

namespace Seatbelt
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var sb = (new Seatbelt(args));
                sb.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unhandled terminating exception: {e}");
            }
        }
    }
}
