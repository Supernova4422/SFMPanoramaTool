using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMPanoramaTool
{
    class Program
    {
        static void Main(string[] args)
        {
            const string CloseCommand = "close";
            Console.WriteLine("Welcome to the SFM panorama toolkit alpha! There are likely to be some errors, so please be sure to report all of them.");
            Console.WriteLine("To get started, type: /Help");
            bool Exit = false;
            do {
                switch (Console.ReadLine().ToLowerInvariant()) //We'll return everything lower case, just to ensure case insensitvity
                {
                    case "/help":
                        Console.WriteLine("{0} is able to close this window, that's pretty much it", CloseCommand);
                        break;
                    case CloseCommand:
                        Exit = true;
                        break;
                }
            }while(!Exit);
        }
        void GetFiles()
        {
            Console.WriteLine("Open the first file in the sequence please");

            Console.WriteLine("Open the final file in the sequence please");
        }
    }
}
