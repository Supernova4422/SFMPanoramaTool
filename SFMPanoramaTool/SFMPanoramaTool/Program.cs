using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AForge.Video.FFMPEG;

namespace SFMPanoramaTool
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            //These all need to be lowercase
            const string CloseCommand = "close";
            const string ImageCommand = "batchimage";
            const string VideoCommand = "batchvideo";
            bool Exit = false;

            Console.WriteLine("Welcome to the SFM panorama toolkit alpha! There are likely to be some errors, so please be sure to report all of them.");
            Console.WriteLine("To get started, type: /Help");

            do {
               switch (Console.ReadLine().ToLowerInvariant()) //We'll return everything lower case, just to ensure case insensitvity
                {
                    case "/help":
                        Console.WriteLine("{0} is able to close this window. {1} Is able to batch rename an exported set of sequential images, into proportionatally smaller sequential images, {2} is similar, but saves as uncompressed AVI files", CloseCommand,ImageCommand,VideoCommand);
                        break;
                    case "dataread":
                        DMXTool DMXToolClass = new DMXTool();
                        DMXToolClass.TestDMX();
                        break;
                    case ImageCommand:
                        BatchRename BatchRenameClass = new BatchRename();
                        BatchRenameClass.GetFiles(false);
                        break;
                    case VideoCommand:
                        BatchRename BatchRenameClassVideo = new BatchRename();
                        BatchRenameClassVideo.GetFiles(true);
                        break;
                    case CloseCommand:
                        Exit = true;
                        break;
                    case "/debugtest":
                        VideoFileWriter writer = new VideoFileWriter();
                        break;
                }
            }while(!Exit);
        }
        
    }
}
