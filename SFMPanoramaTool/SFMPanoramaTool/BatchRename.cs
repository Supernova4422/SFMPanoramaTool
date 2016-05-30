using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AForge.Video.FFMPEG;
using System.Drawing;


namespace SFMPanoramaTool
{
    class BatchRename
    {
        public void GetFilesVariables(bool ConvertToVideo)
        {
            //First we Get the framerate
            int Framerate = 0;

            while (Framerate == 0)
            {
                Console.WriteLine("What is the video's frame rate?");
                string framerate = Console.ReadLine();
                Int32.TryParse(framerate, out Framerate);
                Console.WriteLine("The value is: {0}", Framerate.ToString());
            }

            //We get the number of camera angles used, if the value entered is invalid, we don't stop asking until we get a valid value
            int CameraAnglesInt = 0;

            while (CameraAnglesInt == 0)
            {

                Console.WriteLine("How Many different camera angles were used?");

                string Cameraangles = Console.ReadLine();

                Int32.TryParse(Cameraangles, out CameraAnglesInt);

            }
            bool running = true;

            while (running)
            {
                Console.WriteLine("Press any key to begin selecting the first File");
                Console.ReadKey();
                OpenFileDialog openFileDialog1 = new OpenFileDialog();

                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    Console.WriteLine(openFileDialog1.FileName);
                    Console.WriteLine("Press any key to begin selecting the second File");
                    Console.ReadKey();
                    OpenFileDialog openFileDialog2 = new OpenFileDialog();

                    if (openFileDialog2.ShowDialog() == DialogResult.OK && (System.IO.Path.GetDirectoryName(openFileDialog2.FileName) == System.IO.Path.GetDirectoryName(openFileDialog1.FileName)))
                    {
                        Console.WriteLine("Conversion Process Beginning");
                        GetFiles(ConvertToVideo, openFileDialog1.FileName, openFileDialog2.FileName, Framerate, CameraAnglesInt);
                        Console.WriteLine("Conversion Process Completed");
                        running = false;
                    }
                    else {
                        Console.WriteLine("Either you hit cancel or both Files selected are from different directories! Ensure they are in the same, then try again");
                    }
                }
                else
                {
                    Console.WriteLine("An error occured selecting the first file.");
                }
            }




        }

        public void GetFiles(bool ConvertToVideo, string openFileDialog1, string openFileDialog2, int framerate = 24, Int32 CameraAnglesInt = 6)
        {
            string FirstFile = System.IO.Path.GetFileNameWithoutExtension(openFileDialog1);
            string SecondFile = System.IO.Path.GetFileNameWithoutExtension(openFileDialog2);
            string FilePath = System.IO.Path.GetDirectoryName(openFileDialog2);
            string extension = System.IO.Path.GetExtension(openFileDialog2);

            //Run a for statement that'll check each character in both filenames until they're different
            for (int i = 0; i < FirstFile.Length; i++)
            {
                //If the character we're currently on is different in both files, this code begins:
                if (!(FirstFile.Substring(i, 1).Equals(SecondFile.Substring(i, 1))))
                {

                    string FileName = SecondFile.Substring(0, i); //We get the filename by splitting the string before the different character

                    int ImagesInSequence = Int32.Parse(SecondFile.Substring(i)) + 1; //We get the amount of images, by splitting on and after the different character

                    int digits = Convert.ToInt32(Math.Floor(Math.Log10(ImagesInSequence) + 1)); //We get the amount of digits in the value of the images in sequence, so we can apply padding later

                    Console.WriteLine("Camera Angles present: {0}. If anything seems wrong, close this window, if not, press Enter when ready to process", CameraAnglesInt);

                    Console.ReadKey();

                    int CurrentPicture = 0; //When we go through each file in the folder sequentially, this keeps track of what file we're up to.

                    int InitialPicturesPerFile = (ImagesInSequence / CameraAnglesInt); //The amount of frames per video/folder
                    int PicturesRemainingPerFile = InitialPicturesPerFile; //The amount of frames per video/folder, this will be incremented
                    int foldertouse = 0;
                    string[] FramesForvideo = new string[InitialPicturesPerFile]; //Create a new array we will store images to make into video
                    string[] FileLocations = new string[InitialPicturesPerFile];

                    while (ImagesInSequence + 1 != 0)
                    {
                        if (PicturesRemainingPerFile == 0)
                        {
                            if (ConvertToVideo)
                            {
                                MakeAVIFile(foldertouse.ToString(), FramesForvideo, FilePath, framerate);
                            }
                            else
                            {
                                TransferFiles(FileLocations, FilePath, foldertouse, FileName, extension);
                            }

                            foldertouse++; //Once a folder/video is filled out, it'll create a new folder/video to store the next set of images in
                            PicturesRemainingPerFile = InitialPicturesPerFile; //Resets the value to what it was initially set as
                            FramesForvideo = new string[InitialPicturesPerFile + 1]; //Wipes the array
                        }

                        FileLocations[InitialPicturesPerFile - PicturesRemainingPerFile] = (FileName + CurrentPicture.ToString().PadLeft(digits, '0') + extension);

                        FramesForvideo[InitialPicturesPerFile - PicturesRemainingPerFile] = FilePath + "/" + FileName + CurrentPicture.ToString().PadLeft(digits, '0') + extension; //Add image to the array

                        ImagesInSequence--; //We lower it so we can get the TOTAL amount of images remaining
                        PicturesRemainingPerFile--; //We lower it so we later check if we've ran out of images for the file we're writing
                        CurrentPicture++; //As we're moving to the next image, we tell this
                    }
                    Console.WriteLine("Task completed");
                    break;
                }
            };
        }

        public void MakeAVIFile(string filename, string[] FramesForVideo, string filepath, int Framerate = 24)
        {

            VideoFileWriter writer = new VideoFileWriter();
            Image ImageToStudy = Image.FromFile(FramesForVideo[0]);
            int width = ImageToStudy.Width;
            int height = ImageToStudy.Height;
            Rectangle Rectangle = new Rectangle(((width - height) / 2), 0, height, height);

            writer.Open(filepath + "/" + filename + ".avi", height, height, Framerate, VideoCodec.Raw);
            foreach (string FrameLocation in FramesForVideo)
            {


                if (FrameLocation != null)
                {
                    Bitmap Frame = new Bitmap(FrameLocation);
                    Bitmap NewFrame = Frame.Clone(Rectangle, Frame.PixelFormat);
                    writer.WriteVideoFrame(NewFrame);
                    Frame.Dispose();
                    NewFrame.Dispose();
                }
                else
                {
                    Console.WriteLine(FrameLocation);
                    Console.WriteLine("One Video Done, please wait");
                }
            }
            writer.Close();
            writer.Dispose();

        }
        public void TransferFiles(string[] ImagesToUse, string FilePath, int foldertouse, string filename, string extension)
        {
            int digits = Convert.ToInt32(Math.Floor(Math.Log10(ImagesToUse.Length) + 1)); //We get the amount of digits in the value of the images in sequence, so we can apply padding later

            int i = 0;
            foreach (string frame in ImagesToUse)
            {
                Console.WriteLine("Move {0} to: {1}", FilePath + "/" + frame, FilePath + "/" + foldertouse + "/" + filename + i.ToString().PadLeft(digits, '0') + extension);
                System.IO.Directory.CreateDirectory(FilePath + "/" + foldertouse);
                //Check if the path about to write to exists, if it does, exit execution.
                if (System.IO.File.Exists(FilePath + "/" + foldertouse + "/" + frame)) //Before writing the file, it checks if they exist, if it does, it breaks
                {
                    Console.WriteLine("Files already exist within subfolders! Remove these files then try again");
                    break;
                }
                else
                {
                    System.IO.File.Copy(FilePath + "/" + frame, FilePath + "/" + foldertouse + "/" + filename + i.ToString().PadLeft(digits, '0') + extension);
                }
                i++;
            }
        }
    }
}



