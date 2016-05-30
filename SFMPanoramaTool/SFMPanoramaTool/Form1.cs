using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SFMPanoramaTool
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
        
        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox2.Text = openFileDialog1.FileName;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if ((openFileDialog1.ShowDialog() == DialogResult.OK) && ((System.IO.Path.GetDirectoryName(openFileDialog1.FileName) == System.IO.Path.GetDirectoryName(textBox2.Text))))
            {
                textBox3.Text = openFileDialog1.FileName;
            }
        }

        private void Process_Click(object sender, EventArgs e)
        {
            BatchRename BatchRenameClassVideo = new BatchRename();
            int ParseFramerate = 24;
            int Parsecameraangles = 6;
            Int32.TryParse(textBox11.Text, out ParseFramerate);
            Int32.TryParse(textBox9.Text, out Parsecameraangles);
            BatchRenameClassVideo.GetFiles(radioButton2.Checked,textBox2.Text,textBox3.Text, ParseFramerate, Parsecameraangles);            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox6.Text = openFileDialog1.FileName;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if ((textBox6.Text != null) && (textBox5.Text != null))
            {
                DMXTool DMXToolClass = new DMXTool();
                DMXToolClass.TestDMX(textBox6.Text,textBox5.Text);
            }
        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.Filter = "Dmx files (*.dmx)|*.dmx|All files (*.*)|*.*";
            saveFileDialog1.FilterIndex = 2;
            saveFileDialog1.RestoreDirectory = true;

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox5.Text = saveFileDialog1.FileName;
            }
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void textBox8_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox11_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox9_TextChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
