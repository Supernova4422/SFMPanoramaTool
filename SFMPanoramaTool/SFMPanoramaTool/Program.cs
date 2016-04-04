using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SFMPanoramaTool
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            //These all need to be lowercase
            const string CloseCommand = "close";
            const string RenameCommand = "rename";
            bool Exit = false;

            Console.WriteLine("Welcome to the SFM panorama toolkit alpha! There are likely to be some errors, so please be sure to report all of them.");
            Console.WriteLine("To get started, type: /Help");

            do {
               switch (Console.ReadLine().ToLowerInvariant()) //We'll return everything lower case, just to ensure case insensitvity
                {
                    case "/help":
                        Console.WriteLine("{0} is able to close this window, that's pretty much it", CloseCommand);
                        break;
                    case RenameCommand:
                        BatchRename BatchRenameClass = new BatchRename();
                        BatchRenameClass.GetFiles();
                        break;
                    case CloseCommand:
                        Exit = true;
                        break;
                }
            }while(!Exit);
        }
        
    }
}
