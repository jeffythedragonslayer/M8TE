﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace M8TE
{
    public partial class frmSave3 : Form
    {
        public frmSave3()
        {
            InitializeComponent();
        }

        public static int start_val, end_val;
        public static bool skipTextChange = false;

        private void Form4_FormClosed(object sender, FormClosedEventArgs e)
        {
            frmMain.close_it4();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (skipTextChange == true) return;

            string str = textBox1.Text;
            if (str == "") return;


            skipTextChange = true;

            int value = 0;
            int.TryParse(str, out value);
            if (value > 959) value = 959; // max value
            if (value < 0) value = 0; // min value
            str = value.ToString();
            textBox1.Text = str;
            start_val = value;
            skipTextChange = false;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (skipTextChange == true) return;

            string str = textBox2.Text;
            if (str == "") return;

            skipTextChange = true;

            int value = 0;
            int.TryParse(str, out value);
            if (value > 959) value = 959; // max value
            if (value < 0) value = 0; // min value
            str = value.ToString();
            textBox2.Text = str;
            end_val = value;
            skipTextChange = false;
        }

        private void button1_Click(object sender, EventArgs e)
        { // save mode 3 tiles in a range

            // allow, even if in mode 7

            if ((start_val < 0) || (start_val > 959))
            {
                MessageBox.Show("Error. Start value is out of range.");
                return;
            }
            if ((end_val < 0) || (end_val > 959))
            {
                MessageBox.Show("Error. End value is out of range.");
                return;
            }
            if(start_val > end_val)
            {
                MessageBox.Show("Error. Start value > end value.");
                return;
            }
            if((end_val - start_val) > 511)
            {
                MessageBox.Show("Sorry. Max file size of 512 tiles.");
                return;
            }

            // 64 bytes per tile x 512 possible tiles = 32768
            byte[] out_array = new byte[32768];
            int out_size = 0;
            int end_val2 = end_val + 1;
            int out_index = 0;
            int[] bit1 = new int[8]; // bit planes
            int[] bit2 = new int[8];
            int[] bit3 = new int[8];
            int[] bit4 = new int[8];
            int[] bit5 = new int[8];
            int[] bit6 = new int[8];
            int[] bit7 = new int[8];
            int[] bit8 = new int[8];
            int temp;

            out_size = (end_val2 - start_val) * 64; // 64 bytes per tile

            for (int i = start_val; i < end_val2; i++)
            {
                int z = i * 64; // 64 bytes per tile // start of current tile
                for (int y = 0; y < 8; y++)
                {
                    for (int x = 0; x < 8; x++)
                    {
                        temp = Tiles.Tile_Arrays[z + (y * 8) + x];
                        bit1[y] = (bit1[y] << 1) + (temp & 1);
                        bit2[y] = (bit2[y] << 1) + ((temp >> 1) & 1); // NOTE, this was changed
                        bit3[y] = (bit3[y] << 1) + ((temp >> 2) & 1);
                        bit4[y] = (bit4[y] << 1) + ((temp >> 3) & 1);
                        bit5[y] = (bit5[y] << 1) + ((temp >> 4) & 1);
                        bit6[y] = (bit6[y] << 1) + ((temp >> 5) & 1);
                        bit7[y] = (bit7[y] << 1) + ((temp >> 6) & 1);
                        bit8[y] = (bit8[y] << 1) + ((temp >> 7) & 1);
                    }
                }
                for (int j = 0; j < 8; j++)
                {
                    out_array[out_index++] = (byte)(bit1[j]);
                    out_array[out_index++] = (byte)(bit2[j]);
                }
                for (int j = 0; j < 8; j++)
                {
                    out_array[out_index++] = (byte)(bit3[j]);
                    out_array[out_index++] = (byte)(bit4[j]);
                }
                for (int j = 0; j < 8; j++)
                {
                    out_array[out_index++] = (byte)(bit5[j]);
                    out_array[out_index++] = (byte)(bit6[j]);
                }
                for (int j = 0; j < 8; j++)
                {
                    out_array[out_index++] = (byte)(bit7[j]);
                    out_array[out_index++] = (byte)(bit8[j]);
                }

            }

            // now save it
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Tileset (*.chr)|*.chr|RLE File (*.rle)|*.rle";
            saveFileDialog1.Title = "Save Tiles in Range, Mode 3";
            //saveFileDialog1.ShowDialog();

            frmMain f = (this.Owner as frmMain);

            if ((saveFileDialog1.ShowDialog() == DialogResult.OK) && (saveFileDialog1.FileName != ""))
            {
                System.IO.FileStream fs = (System.IO.FileStream)saveFileDialog1.OpenFile();
                string ext = System.IO.Path.GetExtension(saveFileDialog1.FileName);
                if (ext == ".chr")
                {
                    for (int j = 0; j < out_size; j++)
                    {
                        fs.WriteByte(out_array[j]);
                    }

                    fs.Close();
                }
                else if (ext == ".rle")
                {
                    int rle_length = f.convert_RLE(out_array, out_size);
                    // global rle_array[] now has our compressed data
                    for (int i = 0; i < rle_length; i++)
                    {
                        fs.WriteByte(frmMain.rle_array[i]);
                    }

                    float percent = (float)rle_length / out_size;
                    fs.Close();

                    MessageBox.Show(String.Format("RLE size is {0}, or {1:P2}", rle_length, percent));
                }
                else
                { // something went wrong.
                    fs.Close();
                }

                // close this form, only if result = ok
                this.Close();
            }

        }

        private void Form4_Load(object sender, EventArgs e)
        {
            start_val = 0;
            end_val = 255;

            textBox1.Text = start_val.ToString();
            textBox2.Text = end_val.ToString();
        }


    }
}
