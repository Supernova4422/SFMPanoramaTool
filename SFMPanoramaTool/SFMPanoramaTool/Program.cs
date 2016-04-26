using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AForge.Video.FFMPEG;
using System.Windows.Forms;

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
            const string DMXConvert = "dmx";
            bool Exit = true;

            Dictionary<string, string> HelpCommand = new Dictionary<string, string>();
            HelpCommand.Add(CloseCommand, "Closes the software");
            HelpCommand.Add(ImageCommand, "Splits an entire image sequence into a chosen number of Folders (typically used to make one for each angle) Saved in the source directory and deletes the original files");
            HelpCommand.Add(VideoCommand, "Splits an entire image sequence into a chosen number of AVI files (typically used ot make one for each angle), with a defined framerate and cropped to be 1:1 as defined by the height. Saved in the source directory");
            HelpCommand.Add(DMXConvert, "Modifies a DMX file to have 6 different camera angles which can be later spliced as a panoramic video");

            //Giving Credits
            Console.WriteLine("CREDITS:");
            Console.WriteLine("This program adapts Logan McCloud's 360 degree method. Check out his stuff at: http://hyperchaotixanimation.tumblr.com/");
            Console.WriteLine("");
            Console.WriteLine("");
            //Legal Notices
            Console.WriteLine("About this software");
            Console.WriteLine("");
            Console.WriteLine("This software uses libraries from the FFmpeg project under the LGPLv2. Furhtermore this software does not own the FFmpeg libarary, and its owners can be found at: https://www.ffmpeg.org");
            Console.WriteLine("");
            Console.WriteLine("This software uses the accord-framework licensed under the LGPLv2.1. Furthermore this software's owners  do not own the accord-framework and the owners can be found at: http://accord-framework.net/");
            Console.WriteLine("");
            Console.WriteLine("The FFMPEG library and Accord-Framework libraries were downloaded as packages using nuget and are available at: https://www.nuget.org/packages/Accord.Video.FFMPEG/ ");
            Console.WriteLine("");
            Console.WriteLine("This project uses Artfunkel's Datamodel library available at: https://github.com/Artfunkel/Datamodel.NET");

            //Print Useful Stuff
            Console.WriteLine("");
            Console.WriteLine("Welcome to the SFM panorama toolkit alpha! There are likely to be some errors, so please be sure to report all of them.");
            Console.WriteLine("To get started, type: /help");

            while (Exit)
            {
                switch (Console.ReadLine().ToLowerInvariant()) //We'll return everything lower case, just to ensure case insensitvity
                {
                    case "/help":
                        foreach (KeyValuePair<string, string> Entry in HelpCommand)
                        {
                            Console.WriteLine();
                            Console.WriteLine("{0}: {1}", Entry.Key, Entry.Value);
                        }
                        break;
                    case DMXConvert:
                        OpenFileDialog openFileDialog1 = new OpenFileDialog();
                        if (openFileDialog1.ShowDialog() == DialogResult.OK)
                        {
                            DMXTool DMXToolClass = new DMXTool();
                            DMXToolClass.TestDMX(openFileDialog1.FileName);
                        }
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
                        Exit = false;
                        break;
                }
            }
        }

    }
}
