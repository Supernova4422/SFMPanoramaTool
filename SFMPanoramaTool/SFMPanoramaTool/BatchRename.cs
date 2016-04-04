using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SFMPanoramaTool
{
    class BatchRename
    {
        public void GetFiles()
        {

            Console.WriteLine("Press enter to begin searching for the first file in the sequence");
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Console.WriteLine(openFileDialog1.FileName);
                OpenFileDialog openFileDialog2 = new OpenFileDialog();

                if (openFileDialog2.ShowDialog() == DialogResult.OK && (System.IO.Path.GetDirectoryName(openFileDialog2.FileName) == System.IO.Path.GetDirectoryName(openFileDialog1.FileName)))
                {
                    Console.WriteLine(openFileDialog2.FileName);

                    string FirstFile = System.IO.Path.GetFileNameWithoutExtension(openFileDialog1.FileName);
                    string SecondFile = System.IO.Path.GetFileNameWithoutExtension(openFileDialog2.FileName);
                    string FilePath = System.IO.Path.GetDirectoryName(openFileDialog2.FileName);
                    string extension = System.IO.Path.GetExtension(openFileDialog2.FileName);

                    for (int i = 0; i < FirstFile.Length; i++)
                    {
                        if (!(FirstFile.Substring(i, 1).Equals(SecondFile.Substring(i, 1))))
                        {
                            string FileName = SecondFile.Substring(0, i);

                            int increments = Int32.Parse(SecondFile.Substring(i));
                            
                            int timeslooped = 0;

                            int digits = Convert.ToInt32(Math.Floor(Math.Log10(increments) + 1));

                            Console.WriteLine("How Many different camera angles were used?");

                            string Cameraangles = Console.ReadLine();

                            int CameraAnglesInt;

                            Int32.TryParse(Cameraangles, out CameraAnglesInt);

                            if (CameraAnglesInt == 0)
                            {
                                Console.WriteLine("Error, That value was not expected");
                                break;
                            }


                            Console.WriteLine("The file path is: {0}, the file name is {1} and the extension is: {2}. The files are {3} - {4}. There are {5} files total, and that number has {6} digits", FilePath, FileName, extension, FirstFile, SecondFile, increments, digits);
                            Console.WriteLine("Camera Angles present: {0}. If anything seems wrong, close this window, if not, press Enter when ready to process", CameraAnglesInt);

                            Console.ReadKey();

                            int InitialPicturesPerFile = (increments / CameraAnglesInt);
                            int PicturesPerFile = InitialPicturesPerFile;
                            int foldertouse = 0;

                            while (increments + 1 != 0)
                            {
                                if (PicturesPerFile == 0)
                                {
                                    foldertouse++;
                                    PicturesPerFile = InitialPicturesPerFile;
                                }
                                Console.WriteLine("Move {0} to: {1}", FilePath + "/" + FileName + timeslooped.ToString().PadLeft(digits, '0'), FilePath + "/" + foldertouse + "/" + FileName + "_" + (InitialPicturesPerFile - PicturesPerFile));

                                System.IO.Directory.CreateDirectory(FilePath + "/" + foldertouse );
                                if (System.IO.File.Exists(FilePath + "/" + foldertouse + "/" + FileName + "_" + (InitialPicturesPerFile - PicturesPerFile) + extension))
                                {
                                    Console.WriteLine("Files already exist within subfolders! Remove these files then try again");
                                    break;
                                }
                                System.IO.File.Copy(FilePath + "/" + FileName + timeslooped.ToString().PadLeft(digits, '0') + extension , FilePath + "/" + foldertouse + "/" + FileName + "_" + (InitialPicturesPerFile - PicturesPerFile) + extension);

                                Console.WriteLine();
                                increments--;
                                PicturesPerFile--;
                                timeslooped++;
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
        public void RenameFiles()
        {

        }
    }
}

