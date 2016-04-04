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

                OpenFileDialog openFileDialog2 = new OpenFileDialog();

                if (openFileDialog2.ShowDialog() == DialogResult.OK && (System.IO.Path.GetDirectoryName(openFileDialog2.FileName) == System.IO.Path.GetDirectoryName(openFileDialog1.FileName)))
                {
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

                            Console.WriteLine("The filename is {0} within the filepath {1} and the amount of files is: {2}. We also can say that {2} is a {3} digit number", FileName, FilePath, increments, Math.Floor(Math.Log10(increments) + 1));
                            break;
                        }

                    };

                }
                Console.WriteLine("Either you hit cancel or both Files selected are from different directories! Ensure they are in the same, then try again");

            }
            else
            {
                return;
            }

        }
        public void RenameFiles()
        {

        }
    }
}

