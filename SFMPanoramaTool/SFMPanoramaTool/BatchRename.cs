﻿using System;
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
        public void GetFiles()
        {
            
            //First we Get the framerate
            bool Correct = false;
            int Framerate = new int();
            while (!Correct)
            {
                Console.WriteLine("What is the video's frame rate?");

                string framerate = Console.ReadLine();
                Int32.TryParse(framerate, out Framerate);

                if (Framerate == 0)
                {
                    Console.WriteLine("Error, That value was not expected");
                    return;
                }
                Console.WriteLine("The value is: {0} is this correct? Y/N", Framerate.ToString());

                Correct = (Console.ReadLine() == "Y");
            }

            //Then we ask for the first and last image in the image sequence

            Console.WriteLine("Press enter to begin searching for the first file in the sequence");
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Console.WriteLine(openFileDialog1.FileName);
                OpenFileDialog openFileDialog2 = new OpenFileDialog();

                if (openFileDialog2.ShowDialog() == DialogResult.OK && (System.IO.Path.GetDirectoryName(openFileDialog2.FileName) == System.IO.Path.GetDirectoryName(openFileDialog1.FileName)))
                {
                    //Once we have the first and last images, we get the filenames, paths, and extensions

                    Console.WriteLine(openFileDialog2.FileName);

                    string FirstFile = System.IO.Path.GetFileNameWithoutExtension(openFileDialog1.FileName);
                    string SecondFile = System.IO.Path.GetFileNameWithoutExtension(openFileDialog2.FileName);
                    string FilePath = System.IO.Path.GetDirectoryName(openFileDialog2.FileName);
                    string extension = System.IO.Path.GetExtension(openFileDialog2.FileName);

                    //Run a for statement that'll check each character in both filenames until they're different
                    for (int i = 0; i < FirstFile.Length; i++)
                    {
                        //If the character we're currently on is different in both files, this code begins:
                        if (!(FirstFile.Substring(i, 1).Equals(SecondFile.Substring(i, 1))))
                        {
                            
                            string FileName = SecondFile.Substring(0, i); //We get the filename by splitting the string before the different character

                            int ImagesInSequence = Int32.Parse(SecondFile.Substring(i)); //We get the amount of images, by splitting on and after the different character
                            
                            

                            int digits = Convert.ToInt32(Math.Floor(Math.Log10(ImagesInSequence) + 1)); //We get the amount of digits in the value of the images in sequence, so we can apply padding later

                            //We get the number of camera angles used, if the value entered is invalid, we don't stop asking until we get a valid value
                            int CameraAnglesInt = 0;

                            while (CameraAnglesInt == 0)
                            {

                                Console.WriteLine("How Many different camera angles were used?");

                                string Cameraangles = Console.ReadLine();

                                Int32.TryParse(Cameraangles, out CameraAnglesInt);

                            }

                            Console.WriteLine("Camera Angles present: {0}. If anything seems wrong, close this window, if not, press Enter when ready to process", CameraAnglesInt);

                            Console.ReadKey();

                            int CurrentPicture = 0; //When we go through each file in the folder sequentially, this keeps track of what file we're up to.

                            int InitialPicturesPerFile = (ImagesInSequence / CameraAnglesInt); //The amount of frames per video/folder
                            int PicturesRemainingPerFile = InitialPicturesPerFile; //The amount of frames per video/folder, this will be incremented
                            int foldertouse = 0;
                            Image[] FramesForvideo = new Bitmap[InitialPicturesPerFile + 1]; //Create a new array we will store images to make into video

                            while (ImagesInSequence + 1 != 0)
                            {
                                if (PicturesRemainingPerFile == 0)
                                {
                                    MakeAVIFile(foldertouse.ToString(), FramesForvideo, Framerate);

                                    foldertouse++; //Once a folder/video is filled out, it'll create a new folder/video to store the next set of images in
                                    PicturesRemainingPerFile = InitialPicturesPerFile; //Resets the value to what it was initially set as
                                    FramesForvideo = new Bitmap[InitialPicturesPerFile + 1]; //Wipes the array
                                }
                                Console.WriteLine("Move {0} to: {1}", FilePath + "/" + FileName + CurrentPicture.ToString().PadLeft(digits, '0'), FilePath + "/" + foldertouse + "/" + FileName + "_" + (InitialPicturesPerFile - PicturesRemainingPerFile));

                                System.IO.Directory.CreateDirectory(FilePath + "/" + foldertouse );

                                //Check if the path about to write to exists, if it does, exit execution.
                                if (System.IO.File.Exists(FilePath + "/" + foldertouse + "/" + FileName + "_" + (InitialPicturesPerFile - PicturesRemainingPerFile) + extension)) //Before writing the file, it checks if they exist, if it does, it breaks
                                {
                                    Console.WriteLine("Files already exist within subfolders! Remove these files then try again");
                                    break;
                                }

                                FramesForvideo[InitialPicturesPerFile - PicturesRemainingPerFile] = Bitmap.FromFile(FilePath + "/" + FileName + CurrentPicture.ToString().PadLeft(digits, '0') + extension); //Add image to the array

                                System.IO.File.Copy(FilePath + "/" + FileName + CurrentPicture.ToString().PadLeft(digits, '0') + extension , FilePath + "/" + foldertouse + "/" + FileName + "_" + (InitialPicturesPerFile - PicturesRemainingPerFile) + extension);

                                Console.WriteLine();
                                ImagesInSequence--; //We lower it so we can get the TOTAL amount of images remaining
                                PicturesRemainingPerFile--; //We lower it so we later check if we've ran out of images for the file we're writing
                                CurrentPicture++; //As we're moving to the next image, we tell this
                            } 
                            Console.WriteLine("Task completed");

                            break;
                        }

                    };

                }
                else { 
                Console.WriteLine("Either you hit cancel or both Files selected are from different directories! Ensure they are in the same, then try again");
                }
            }
            else
            {
                Console.WriteLine("An error occured selecting the first file");
                return;
            }

        }

        public void MakeAVIFile(string filename , Image[] FramesForVideo, int Framerate)
        {
            
            VideoFileWriter writer = new VideoFileWriter();
            
            int width = FramesForVideo[0].Width;
            int height = FramesForVideo[0].Height;

            
            writer.Open(filename, width, height, Framerate, VideoCodec.Raw);
            foreach (Bitmap Frame in FramesForVideo)
            {
                writer.WriteVideoFrame(Frame);
            }
            writer.Close();

        }
        public void RenameFiles()
        {

        }
    }
}

