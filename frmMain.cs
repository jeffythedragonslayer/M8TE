﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace M8TE
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
            this.Location = new Point(Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y);

        }
        static frmTileEditor newChild = null;
        static frmOptions newChild3 = null;
        static frmSave3 newChild4 = null;
        static frmSave7 newChild5 = null;

        public static void close_it()
        {
            newChild = null;
        }

        public static void close_it3()
        {
            newChild3 = null;
        }

        public static void close_it4()
        {
            newChild4 = null;
        }

        public static void close_it5()
        {
            newChild5 = null;
        }

        //globals
        public static Bitmap image_map = new Bitmap(256, 256);
        public static Bitmap image_tiles = new Bitmap(128, 128);
        public static Bitmap image_pal = new Bitmap(256, 256);
        public static Bitmap image_map_local = new Bitmap(256, 256);
        public static Bitmap temp_bmp = new Bitmap(256, 256); //double size
        public static Bitmap temp_bmp2 = new Bitmap(512, 512); //double size
        public static Bitmap temp_bmp3 = new Bitmap(256, 256); //double size
        public static Bitmap cool_bmp = new Bitmap(256, 256); //import
        public static int pal_x, pal_y, tile_x, tile_y, tile_num, tile_set;
        public static int bg_mode, active_map_x, active_map_y, active_map_index;
        public static int map_height = 28;
        public static int last_tile_x, last_tile_y;
        public static int brushsize;
        public const int BRUSH1x1 = 0;
        public const int BRUSH3x3 = 1;
        public const int BRUSH5x5 = 2;
        public const int BRUSHNEXT = 3;
        public const int BRUSH_CLONE_T = 4;
        public const int BRUSH_CLONE_M = 5;
        public const int BRUSH_FILL = 6;
        public const int BG_MODE_3 = 0;
        public const int BG_MODE_7 = 1;
        public const int BG_MODE_7P = 2;
        public static int pal_r_copy, pal_g_copy, pal_b_copy;
        public static byte[] rle_array = new byte[65536];
        public static int rle_index, rle_index2, rle_count;
        public static int map_clone_x, map_clone_y, clone_start_x, clone_start_y;
        public static int disable_map_click;

        public static int[] R_Array = new int[65536];
        public static int[] G_Array = new int[65536];
        public static int[] B_Array = new int[65536];
        public static int[] Count_Array = new int[65536]; // count each color
        public static int[] c256ColorIndexes = new int[256];
        public static int[] c256ColorsAdded = new int[256];
        //public static int[] SortedColorIndexes = new int[16];
        public static int color_count; // how many total different colors
        public static int r_val, g_val, b_val, diff_val;
        public static int c_offset, c_offset2;
        public static int image_width, image_height;
        public static int[] needy_chr_array = new int[65536]; // temp store color values of imported image

        public static int dither_factor = 0;
        public static int dither_adjust = 0;
        public static double dither_db = 0.0;
        public static bool f3_cb1 = false, f3_cb2 = false;
        public static bool flip_h = false, flip_v = false;
        public static int max_import_color = 256;

        public static bool undo_ready = false;
        public static int which_map, which_map_x, which_map_y, which_map_backup;

        public readonly int[,] BAYER_MATRIX =
        {
            { 0,48,12,60,3,51,15,63 },
            { 32,16,44,28,35,19,47,31 },
            { 8,56,4,52,11,59,7,55 },
            { 40,24,36,20,43,27,39,23 },
            { 2,50,14,62,1,49,13,61 },
            { 34,18,46,30,33,17,45,29 },
            { 10,58,6,54,9,57,5,53 },
            { 42,26,38,22,41,25,37,21 }
        }; // 1/64 times this

        public readonly int[] H_FLIP_TABLE =
        {
            7,6,5,4,3,2,1,0,
            15,14,13,12,11,10,9,8,
            23,22,21,20,19,18,17,16,
            31,30,29,28,27,26,25,24,
            39,38,37,36,35,34,33,32,
            47,46,45,44,43,42,41,40,
            55,54,53,52,51,50,49,48,
            63,62,61,60,59,58,57,56
        };

        public readonly int[] V_FLIP_TABLE =
        {
            56,57,58,59,60,61,62,63,
            48,49,50,51,52,53,54,55,
            40,41,42,43,44,45,46,47,
            32,33,34,35,36,37,38,39,
            24,25,26,27,28,29,30,31,
            16,17,18,19,20,21,22,23,
            8,9,10,11,12,13,14,15,
            0,1,2,3,4,5,6,7
        };

        public readonly int[] HV_FLIP_TABLE =
        {
            63,62,61,60,59,58,57,56,
            55,54,53,52,51,50,49,48,
            47,46,45,44,43,42,41,40,
            39,38,37,36,35,34,33,32,
            31,30,29,28,27,26,25,24,
            23,22,21,20,19,18,17,16,
            15,14,13,12,11,10,9,8,
            7,6,5,4,3,2,1,0
        };


        private void Form1_Load(object sender, EventArgs e)
        {
            update_palette();
            update_tile_image();
            update_tilemap();
            label5.Focus();
            this.ActiveControl = label5;
        }


        public void Checkpoint()
        {
            // backup for undo function
            undo_ready = true;
            which_map_backup = which_map;

            int offset = which_map_backup * 1024;
            // only save the current map
            for (int i = 0; i < 1024; ++i)
            { // copy maps
                MapsU.tile[i] = Maps.tile[offset];
                MapsU.palette[i] = Maps.palette[offset];
                MapsU.h_flip[i] = Maps.h_flip[offset];
                MapsU.v_flip[i] = Maps.v_flip[offset];
                MapsU.priority[i] = Maps.priority[offset];
                offset++;
            }

            for(int i = 0; i < 65536; ++i)
            { // copy tilesets
                TilesU.Tile_Arrays[i] = Tiles.Tile_Arrays[i];
            }
        }

        public void Do_Undo()
        {
            if (undo_ready == false) return;

            int offset = which_map_backup * 1024;
            for (int i = 0; i < 1024; ++i)
            { // restore maps
                Maps.tile[offset] = MapsU.tile[i];
                Maps.palette[offset] = MapsU.palette[i];
                Maps.h_flip[offset] = MapsU.h_flip[i];
                Maps.v_flip[offset] = MapsU.v_flip[i];
                Maps.priority[offset] = MapsU.priority[i];
                offset++;
            }

            for (int i = 0; i < 65536; ++i)
            { // restore tilesets
                Tiles.Tile_Arrays[i] = TilesU.Tile_Arrays[i];
            }

            active_map_index = active_map_x + (active_map_y * 32) + (32 * 32 * which_map);
            
            if (Maps.h_flip[active_map_index] == 0)
            {
                checkBox1.Checked = false;
            }
            else
            {
                checkBox1.Checked = true;
            }
            if (Maps.v_flip[active_map_index] == 0)
            {
                checkBox2.Checked = false;
            }
            else
            {
                checkBox2.Checked = true;
            }
            

            common_update2();

            undo_ready = false;
        }


        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Do_Undo();
        }


        // 16x16 grid
        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            //update_tilemap();
            common_update2();
            label5.Focus();
        }



        public void update_tilemap() // the big box on the left, 32x32
        {
            //default BG, draw color 0 all over the BG
            int r = Palettes.pal_r[0];
            int g = Palettes.pal_g[0];
            int b = Palettes.pal_b[0];
            int offset = 0;
            int temp_tile = 0;
            

            for (int y = 0; y < 256; y++) // fill with the 0th color first
            {
                for (int x = 0; x < 256; x++)
                {
                    image_map_local.SetPixel(x, y, Color.FromArgb(r, g, b));
                }
            }

            if (bg_mode == BG_MODE_7P) // preview mode
            {
                // a zoomed out view of the mode 7 map
                draw_preview();
                return;
            }

            else // bg_mode 0 or 1 
            {
                int z = which_map * 32 * 32; // offset for current map
                for (int y = 0; y < map_height; y++)
                {
                    for (int x = 0; x < 32; x++)
                    {
                        offset = z + (y * 32) + x; // offset to current tile on the map

                        temp_tile = (Maps.tile[offset] * 8 * 8); // base offset for tile
                        
                        big_sub(offset, x, y, temp_tile);
                    }
                }
            }

            // draw a checkerboard pattern over the unused portion of the map
            if (map_height < 32)
            {
                Color checker_color = Color.SlateGray;
                for (int y = map_height * 8; y < 256; y++)
                {
                    for (int x = 0; x < 256; x++)
                    {
                        if ((y & 4) == 0)
                        {
                            if ((x & 4) == 0)
                            {
                                checker_color = Color.SlateGray;
                            }
                            else
                            {
                                checker_color = Color.LightSlateGray;
                            }
                        }
                        else
                        {
                            if ((x & 4) == 0)
                            {
                                checker_color = Color.LightSlateGray;
                            }
                            else
                            {
                                checker_color = Color.SlateGray;
                            }
                        }

                        image_map_local.SetPixel(x, y, checker_color);
                    }
                }
            }

            //Bitmap temp_bmp2 = new Bitmap(512, 512); //resize double size
            using (Graphics g2 = Graphics.FromImage(temp_bmp2))
            {
                g2.InterpolationMode = InterpolationMode.NearestNeighbor;
                g2.PixelOffsetMode = PixelOffsetMode.Half; // fix bug, missing half a pixel on top and left
                g2.DrawImage(image_map_local, 0, 0, 512, 512);
            } // standard resize of bmp was blurry, this makes it sharp

            //draw a box around the active tile
            if (bg_mode < 2)
            {

                //draw grid here
                if (checkBox4.Checked == true)
                {
                    //draw horizontal lines at each 16
                    for (int i = 31; i < (map_height * 15); i += 32)
                    {
                        for (int j = 0; j < 510; j += 2)
                        {
                            temp_bmp2.SetPixel(j, i, Color.Black);
                            temp_bmp2.SetPixel(j + 1, i, Color.White);
                        }
                    }
                    //draw vertical lines at each 16
                    for (int j = 31; j < 511; j += 32)
                    {
                        for (int i = 0; i < (map_height * 16) - 2; i += 2)
                        {
                            temp_bmp2.SetPixel(j, i + 1, Color.Black);
                            temp_bmp2.SetPixel(j, i, Color.White);
                        }
                    }
                }
                // draw box around current selection
                int x2 = (active_map_x * 16);
                
                if (active_map_y >= map_height) active_map_y = map_height - 1;
                int y2 = (active_map_y * 16);
                
                for (int i = 0; i < 16; i++)
                {
                    temp_bmp2.SetPixel(x2 + i, y2, Color.White);
                    temp_bmp2.SetPixel(x2, y2 + i, Color.White);
                    temp_bmp2.SetPixel(x2 + i, y2 + 15, Color.White);
                    temp_bmp2.SetPixel(x2 + 15, y2 + i, Color.White);
                }
            }

            pictureBox1.Image = temp_bmp2;
            pictureBox1.Refresh();
        }
        // END UPDATE TILEMAP


        private void draw_preview()
        { // draw the mode 7 preview
            // start over, blank the temp_bmp2 (512,512)

            int r = Palettes.pal_r[0];
            int g = Palettes.pal_g[0];
            int b = Palettes.pal_b[0];
            int map_offset = 0;
            int screen_x = 0;
            int screen_y = 0;

            for (int y = 0; y < 512; y++) // fill with the 0th color first
            {
                for (int x = 0; x < 512; x++)
                {
                    temp_bmp2.SetPixel(x, y, Color.FromArgb(r, g, b));
                }
            }
            

            for(int map_y = 0; map_y < 4; map_y++)
            {
                for(int map_x = 0; map_x < 4; map_x++)
                {
                    // screen x, screen y, tile offset
                    for(int yy = 0; yy < 32; yy++)
                    {
                        for (int xx = 0; xx < 32; xx++)
                        {
                            map_offset = ((map_y * 4) + map_x) * 1024; // 1024 bytes per map
                            map_offset += (yy * 32) + xx; // exact tile
                            int tile_num = Maps.tile[map_offset];
                            //
                            screen_x = (map_x * 128) + (xx * 4);
                            screen_y = (map_y * 128) + (yy * 4);
                            //
                            draw_preview_sub(tile_num, screen_x, screen_y);
                        }
                    }
                }
            }

            // skip grid or selection

            // put in on screen now

            pictureBox1.Image = temp_bmp2;
            pictureBox1.Refresh();
        }


        private void draw_preview_sub(int tile_num, int screen_x, int screen_y)
        { // draw 1 tile, only 4x4 (zoomed out), to temp_bmp2
            int color, final_x, final_y, x2, y2;
            int tile_offset; // t = map_offset;

            for(int y = 0; y < 8; y += 2)
            {
                y2 = y / 2;
                for(int x = 0; x < 8; x += 2)
                {
                    x2 = x / 2;
                    tile_offset = tile_num * 64;
                    tile_offset += (y * 8) + x;
                    //draw each tile
                    
                    color = Tiles.Tile_Arrays[tile_offset];
                    final_x = screen_x + x2;
                    final_y = screen_y + y2;
                    

                    temp_bmp2.SetPixel(final_x, final_y,
                            Color.FromArgb(Palettes.pal_r[color], Palettes.pal_g[color], Palettes.pal_b[color]));
                    
                }
            }
        }




        //for drawing the tile map
        private void big_sub(int offset, int x, int y, int temp_tile)
        {
            // in 8bpp modes, there is only one palette
            int color = 0;
            int temp_h = Maps.h_flip[offset];
            int temp_v = Maps.v_flip[offset];
            if(bg_mode == BG_MODE_7)
            {
                temp_h = temp_v = 0; // no flipping in mode 7
                temp_tile = temp_tile & 0x3fff; // only use first tileset
            }

            if (temp_h == 0) // plain
            {
                if (temp_v == 0) // plain
                {
                    int index = temp_tile;
                    int x8 = (x * 8);
                    int y8 = (y * 8);
                    for (int tile_y = 0; tile_y < 8; tile_y++)
                    {
                        for (int tile_x = 0; tile_x < 8; tile_x++)
                        {
                            color = Tiles.Tile_Arrays[index++];
                            
                            image_map_local.SetPixel(x8 + tile_x, y8 + tile_y,
                                    Color.FromArgb(Palettes.pal_r[color], Palettes.pal_g[color], Palettes.pal_b[color]));
                        }
                    }
                }
                else // v flipped
                {
                    int index = temp_tile;
                    int x8 = (x * 8);
                    int y8 = (y * 8);
                    for (int tile_y = 0; tile_y < 8; tile_y++)
                    {
                        for (int tile_x = 0; tile_x < 8; tile_x++)
                        {
                            color = Tiles.Tile_Arrays[index++];
                            
                            image_map_local.SetPixel(x8 + tile_x, y8 + (7 - tile_y),
                                    Color.FromArgb(Palettes.pal_r[color], Palettes.pal_g[color], Palettes.pal_b[color]));
                        }
                    }
                }
            }
            else // h flipped
            {
                if (temp_v == 0) // just h
                {
                    int index = temp_tile;
                    int x8 = (x * 8);
                    int y8 = (y * 8);
                    for (int tile_y = 0; tile_y < 8; tile_y++)
                    {
                        for (int tile_x = 0; tile_x < 8; tile_x++)
                        {
                            color = Tiles.Tile_Arrays[index++];
                            
                            image_map_local.SetPixel(x8 + (7 - tile_x), y8 + tile_y,
                                    Color.FromArgb(Palettes.pal_r[color], Palettes.pal_g[color], Palettes.pal_b[color]));
                        }
                    }
                }
                else // both flipped
                {
                    int index = temp_tile;
                    int x8 = (x * 8);
                    int y8 = (y * 8);
                    for (int tile_y = 0; tile_y < 8; tile_y++)
                    {
                        for (int tile_x = 0; tile_x < 8; tile_x++)
                        {
                            color = Tiles.Tile_Arrays[index++];
                            
                            image_map_local.SetPixel(x8 + (7 - tile_x), y8 + (7 - tile_y),
                                    Color.FromArgb(Palettes.pal_r[color], Palettes.pal_g[color], Palettes.pal_b[color]));
                        }
                    }
                }
            }
        }
        // END TILEMAP SUB



        private int check_num(string str) // make sure string is number
        {
            int value = 0;

            int.TryParse(str, out value);
            if (value > 255) value = 255; // max value
            if (value < 0) value = 0; // min value
            value = value & 0xf8;
            
            return (value);
        }



        public void rebuild_pal_boxes()
        {
            int selection = pal_x + (pal_y * 16);

            int red = Palettes.pal_r[selection];
            textBox1.Text = red.ToString();
            trackBar1.Value = red / 8;

            int green = Palettes.pal_g[selection];
            textBox2.Text = green.ToString();
            trackBar2.Value = green / 8;

            int blue = Palettes.pal_b[selection];
            textBox3.Text = blue.ToString();
            trackBar3.Value = blue / 8;

            update_box4();
        }



        private void update_box4() // when boxes 1,2,or 3 changed
        {
            int value_red, value_green, value_blue;
            int sum;
            int selection = get_selection();

            value_red = Palettes.pal_r[selection];
            value_green = Palettes.pal_g[selection];
            value_blue = Palettes.pal_b[selection];


            sum = ((value_red & 0xf8) >> 3) + ((value_green & 0xf8) << 2) + ((value_blue & 0xf8) << 7);
            string hexValue = sum.ToString("X");
            // may have to append zeros to beginning


            if (hexValue.Length == 3) hexValue = String.Concat("0", hexValue);
            else if (hexValue.Length == 2) hexValue = String.Concat("00", hexValue);
            else if (hexValue.Length == 1) hexValue = String.Concat("000", hexValue);
            else if (hexValue.Length == 0) hexValue = "0000";

            textBox4.Text = hexValue;
        }



        private bool is_hex(char ch1)
        {
            if ((ch1 >= '0') && (ch1 <= '9')) return true;
            if ((ch1 >= 'A') && (ch1 <= 'F')) return true;
            //should be upper case letters
            return false;
        }



        private string check_hex(string str) //str.Length should be exacly 4
        {
            if ((!is_hex(str[0])) ||
                (!is_hex(str[1])) ||
                (!is_hex(str[2])) ||
                (!is_hex(str[3])))
            {
                //something isn't a hex string
                return "Z";
            }

            //make sure the high byte is 0-7
            if (str[0] > '7')
            {
                char[] letters = str.ToCharArray();
                char letter;
                switch (letters[0])
                {
                    case 'F':
                        letter = '7'; break;
                    case 'E':
                        letter = '6'; break;
                    case 'D':
                        letter = '5'; break;
                    case 'C':
                        letter = '4'; break;
                    case 'B':
                        letter = '3'; break;
                    case 'A':
                        letter = '2'; break;
                    case '9':
                        letter = '1'; break;
                    case '8':
                    default:
                        letter = '0'; break;
                }
                letters[0] = letter;
                return string.Join("", letters);
            }
            return str;
        }



        private int hex_val(char chr) // convert single hex digit to int value
        {
            switch (chr)
            {
                case 'F':
                    return 15;
                case 'E':
                    return 14;
                case 'D':
                    return 13;
                case 'C':
                    return 12;
                case 'B':
                    return 11;
                case 'A':
                    return 10;
                case '9':
                    return 9;
                case '8':
                    return 8;
                case '7':
                    return 7;
                case '6':
                    return 6;
                case '5':
                    return 5;
                case '4':
                    return 4;
                case '3':
                    return 3;
                case '2':
                    return 2;
                case '1':
                    return 1;
                case '0':
                default:
                    return 0;
            }
        }



        private int get_selection() // palette
        {
            int selection = pal_x + (pal_y * 16);
            
            return selection;
        }



        private void update_rgb() // when r g or b boxes change
        {
            string str = textBox1.Text;
            int value = check_num(str);
            textBox1.Text = value.ToString();
            trackBar1.Value = value / 8;

            int selection = get_selection();
            Palettes.pal_r[selection] = (byte)value;

            str = textBox2.Text;
            value = check_num(str);
            textBox2.Text = value.ToString();
            trackBar2.Value = value / 8;

            Palettes.pal_g[selection] = (byte)value;

            str = textBox3.Text;
            value = check_num(str);
            textBox3.Text = value.ToString();
            trackBar3.Value = value / 8;

            Palettes.pal_b[selection] = (byte)value;
        }



        private void textBox1_KeyPress(object sender, KeyPressEventArgs e) //Red
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                update_rgb();
                update_box4();

                update_palette();

                common_update2();

                e.Handled = true; // prevent ding on return press
            }
        }



        private void textBox2_KeyPress(object sender, KeyPressEventArgs e) //Green
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                update_rgb();
                update_box4();

                update_palette();

                common_update2();

                e.Handled = true; // prevent ding on return press
            }
        }



        private void textBox3_KeyPress(object sender, KeyPressEventArgs e) //Blue
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                update_rgb();
                update_box4();

                update_palette();

                common_update2();

                e.Handled = true; // prevent ding on return press
            }
        }



        private void textBox4_KeyPress(object sender, KeyPressEventArgs e) //Hex
        {
            if (e.KeyChar == (char)Keys.Return)
            {
                string str = textBox4.Text;
                str = str.Trim(); // remove spaces
                int[] value = new int[4];
                int temp;

                if (str.Length < 4)
                {
                    str = str.PadLeft(4, '0');
                }

                if (str.Length != 4) return;
                str = str.ToUpper();
                str = check_hex(str); //returns "Z" if fail
                if (str == "Z") return;

                textBox4.Text = str;

                value[0] = hex_val(str[0]); //get int value, 0-15
                value[1] = hex_val(str[1]);
                value[2] = hex_val(str[2]);
                value[3] = hex_val(str[3]);

                //pass values to the other boxes
                temp = ((value[3] & 0x0f) << 3) + ((value[2] & 0x01) << 7); // red, 5 bits
                textBox1.Text = temp.ToString();
                temp = ((value[2] & 0x0e) << 2) + ((value[1] & 0x03) << 6); // green, 5 bits
                textBox2.Text = temp.ToString();
                temp = ((value[1] & 0x0c) << 1) + ((value[0] & 0x07) << 5); // blue, 5 bits
                textBox3.Text = temp.ToString();

                update_rgb();
                update_palette();
                common_update2();

                e.Handled = true; // prevent ding on return press
            }
        }



        private string hex_char(int value)
        {
            switch (value)
            {
                case 15:
                    return "F";
                case 14:
                    return "E";
                case 13:
                    return "D";
                case 12:
                    return "C";
                case 11:
                    return "B";
                case 10:
                    return "A";
                case 9:
                    return "9";
                case 8:
                    return "8";
                case 7:
                    return "7";
                case 6:
                    return "6";
                case 5:
                    return "5";
                case 4:
                    return "4";
                case 3:
                    return "3";
                case 2:
                    return "2";
                case 1:
                    return "1";
                case 0:
                default:
                    return "0";
            }
        }



        public void update_tile_image() // redraw the visible tileset
        {
            Color temp_color;
            int temp_tile_num = 0;
            for (int i = 0; i < 16; i++) //tile row = y
            {
                for (int j = 0; j < 16; j++) //tile column = x
                {
                    temp_tile_num = (i * 16) + j;
                    for (int k = 0; k < 8; k++) // pixel row = y
                    {
                        for (int m = 0; m < 8; m++) // pixel column = x
                        {
                            int color = 0;
                            int index = (frmMain.tile_set * 256 * 8 * 8) + (temp_tile_num * 8 * 8) + (k * 8) + m;
                            int pal_index = Tiles.Tile_Arrays[index]; // pixel in tile array
                            
                            
                            color = pal_index;
                            temp_color = Color.FromArgb(Palettes.pal_r[color], Palettes.pal_g[color], Palettes.pal_b[color]);
                            image_tiles.SetPixel((j * 8) + m, (i * 8) + k, temp_color);
                        }
                    }
                }
            }


            // disable the last 64 tiles (4 rows) in the last tileset

            // draw a checkerboard pattern over the unused portion of the map
            if (tile_set == 3)
            {
                Color checker_color = Color.SlateGray;
                for (int y = 96; y < 128; y++)
                {
                    for (int x = 0; x < 128; x++)
                    {
                        if ((y & 4) == 0)
                        {
                            if ((x & 4) == 0)
                            {
                                checker_color = Color.SlateGray;
                            }
                            else
                            {
                                checker_color = Color.LightSlateGray;
                            }
                        }
                        else
                        {
                            if ((x & 4) == 0)
                            {
                                checker_color = Color.LightSlateGray;
                            }
                            else
                            {
                                checker_color = Color.SlateGray;
                            }
                        }

                        image_tiles.SetPixel(x, y, checker_color);
                    }
                }
            }




            //Bitmap temp_bmp = new Bitmap(256, 256); //resize double size
            using (Graphics g = Graphics.FromImage(temp_bmp))
            {
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.Half; // fix bug, missing half a pixel on top and left
                g.DrawImage(image_tiles, 0, 0, 256, 256);
            } // standard resize of bmp was blurry, this makes it sharp

            //draw grid here
            if (checkBox4.Checked == true)
            {
                //draw horizontal lines at each 16
                for (int i = 31; i < 255; i += 32)
                {
                    for (int j = 0; j < 255; j += 2)
                    {
                        temp_bmp.SetPixel(j, i, Color.Black);
                        temp_bmp.SetPixel(j + 1, i, Color.LightGray);
                    }
                }
                //draw vertical lines at each 16
                for (int j = 31; j < 255; j += 32)
                {
                    for (int i = 0; i < 255; i += 2)
                    {
                        temp_bmp.SetPixel(j, i + 1, Color.Black);
                        temp_bmp.SetPixel(j, i, Color.LightGray);
                    }
                }
            }

            //put a white box around the selected tile
            int pos_x = 0; int pos_y = 0;
            for (int i = 0; i < 16; i++)
            {
                pos_y = (tile_y * 16) - 1; // it's doing a weird off by 1 thing
                if (pos_y < 0) pos_y = 0; // so have to adjust by 1, and not == -1
                pos_x = (tile_x * 16) - 1;
                if (pos_x < 0) pos_x = 0;
                temp_bmp.SetPixel(pos_x + i, pos_y, Color.White);
                temp_bmp.SetPixel(pos_x, pos_y + i, Color.White);
                temp_bmp.SetPixel(pos_x + i, pos_y + 15, Color.White);
                temp_bmp.SetPixel(pos_x + 15, pos_y + i, Color.White);
            }
            pictureBox2.Image = temp_bmp;
            pictureBox2.Refresh();
            
        }
        // END REDRAW TILESET



        public void tile_show_num() // top right, above tileset
        {
            string str = "";
            int dec_num = (tile_y * 16) + tile_x + ((tile_set & 3) * 256);
            str = hex_char(tile_y) + hex_char(tile_x) + "   " + dec_num.ToString();
            label9.Text = str;
        }



        private void pictureBox2_Click(object sender, EventArgs e)
        { // tiles
            if (bg_mode == BG_MODE_7P)
            {
                MessageBox.Show("Editing is disabled in Preview Mode.");
                return;
            }

            //change the label to tile number, in hex
            tile_x = 0; tile_y = 0; tile_num = 0; //globals

            var mouseEventArgs = e as MouseEventArgs;
            if (mouseEventArgs != null)
            {

                tile_x = mouseEventArgs.X >> 4;
                tile_y = mouseEventArgs.Y >> 4;
            }
            if (tile_x < 0) tile_x = 0;
            if (tile_y < 0) tile_y = 0;
            if (tile_x > 15) tile_x = 15;
            if (tile_y > 15) tile_y = 15;
            tile_num = (tile_y * 16) + tile_x;

            if((tile_num >= 192) && (tile_set == 3)) // don't allow clicking the last few tiles
            {
                tile_x = 0;
                tile_y = 0;
                tile_num = 0;
            }

            tile_show_num();

            //last
            if (newChild != null)
            {
                newChild.BringToFront();
                newChild.update_tile_box();
            }
            else
            {
                newChild = new frmTileEditor();
                newChild.Owner = this;
                int xx = Screen.PrimaryScreen.Bounds.Width;
                if (this.Location.X + 970 < xx) // set new form location
                {
                    newChild.Location = new Point(this.Location.X + 800, this.Location.Y + 80);
                }
                else
                {
                    newChild.Location = new Point(xx - 170, this.Location.Y);
                }

                newChild.Show();
                //update
            }

            update_tile_image();
            label5.Focus();
        } // END CLICKED ON TILES



        private void picbox1_sub() // place a tile on the map
        {
            // apply the tile now
            int temp_y, temp_x, start_x, loop_x, loop_y;
            int z = which_map * 32 * 32;
            int offset, temp_tile;
            int next_count = 0;
            int[] next_tiles = new int[5]; // actually 4

            // which tile is selected on right.
            int temp_set = tile_set & 3; //0-3
            if(bg_mode == BG_MODE_7)
            {
                temp_set = 0;
            }
            int tile_num2 = tile_x + (tile_y * 16) + (256 * temp_set); // 0-1023

            if (brushsize == BRUSH1x1)
            {
                start_x = temp_x = active_map_x;
                temp_y = active_map_y;
                loop_x = 1;
                loop_y = 1;
            }
            else if (brushsize == BRUSH3x3)
            {
                start_x = temp_x = active_map_x - 1;
                temp_y = active_map_y - 1;
                loop_x = 3;
                loop_y = 3;
            }
            else if (brushsize == BRUSH5x5)
            {
                start_x = temp_x = active_map_x - 2;
                temp_y = active_map_y - 2;
                loop_x = 5;
                loop_y = 5;
            }
            else if (brushsize == BRUSHNEXT)
            {
                start_x = temp_x = active_map_x;
                temp_y = active_map_y;
                loop_x = 2;
                loop_y = 2;
                if (checkBox5.Checked == false) // h flip no
                {
                    if (checkBox6.Checked == false) // v flip no
                    {
                        next_tiles[0] = tile_num2;
                        next_tiles[1] = (tile_num2 + 1) & 0x3ff;
                        next_tiles[2] = (tile_num2 + 16) & 0x3ff;
                        next_tiles[3] = (tile_num2 + 17) & 0x3ff;
                    }
                    else // v flip yes
                    {
                        next_tiles[0] = (tile_num2 + 16) & 0x3ff;
                        next_tiles[1] = (tile_num2 + 17) & 0x3ff;
                        next_tiles[2] = tile_num2;
                        next_tiles[3] = (tile_num2 + 1) & 0x3ff;
                    }
                }
                else // h flip yes
                {
                    if (checkBox6.Checked == false) // v flip no
                    {
                        next_tiles[0] = (tile_num2 + 1) & 0x3ff;
                        next_tiles[1] = tile_num2;
                        next_tiles[2] = (tile_num2 + 17) & 0x3ff;
                        next_tiles[3] = (tile_num2 + 16) & 0x3ff;
                    }
                    else // v flip yes, both flipped
                    {
                        next_tiles[0] = (tile_num2 + 17) & 0x3ff;
                        next_tiles[1] = (tile_num2 + 16) & 0x3ff;
                        next_tiles[2] = (tile_num2 + 1) & 0x3ff;
                        next_tiles[3] = tile_num2;
                    }
                }
                if(bg_mode == BG_MODE_7)
                {
                    next_tiles[0] = next_tiles[0] & 0xff; // always use tile set 0
                    next_tiles[1] = next_tiles[1] & 0xff;
                    next_tiles[2] = next_tiles[2] & 0xff;
                    next_tiles[3] = next_tiles[3] & 0xff;
                }

                tile_num2 = next_tiles[0];
            }
            else if ((brushsize == BRUSH_CLONE_T) || (brushsize == BRUSH_CLONE_M))
            {

                loop_x = 1;
                loop_y = 1;

                if (brushsize == BRUSH_CLONE_T)
                { // clone from tileset
                    // get distance in tiles 
                    temp_x = (active_map_x - clone_start_x) + tile_x;
                    if ((temp_x < 0) || (temp_x > 15)) return;

                    temp_y = (active_map_y - clone_start_y) + tile_y;
                    if ((temp_y < 0) || (temp_y > 15)) return;

                    tile_num2 = temp_x + (temp_y * 16) + (256 * temp_set); // 0-1023

                    start_x = temp_x = active_map_x;
                    temp_y = active_map_y;
                }
                else // clone from map
                {
                    int temp_x2, temp_y2, active_map_index2;

                    temp_x2 = (active_map_x - clone_start_x) + map_clone_x;
                    if ((temp_x2 < 0) || (temp_x2 > 31)) return;

                    temp_y2 = (active_map_y - clone_start_y) + map_clone_y;
                    if ((temp_y2 < 0) || (temp_y2 >= map_height)) return;
                    
                    temp_x = active_map_x;
                    temp_y = active_map_y;


                    active_map_index = temp_x + (temp_y * 32) + (32 * 32 * which_map);
                    active_map_index2 = temp_x2 + (temp_y2 * 32) + (32 * 32 * which_map);
                    Maps.tile[active_map_index] = Maps.tile[active_map_index2];
                    Maps.palette[active_map_index] = Maps.palette[active_map_index2];
                    Maps.h_flip[active_map_index] = Maps.h_flip[active_map_index2];
                    Maps.v_flip[active_map_index] = Maps.v_flip[active_map_index2];
                    Maps.priority[active_map_index] = Maps.priority[active_map_index2];



                    // draw 1 tile, but first fill with zero color
                    int temp_x3 = temp_x * 8;
                    int temp_y3 = temp_y * 8;
                    Color zero_color = Color.FromArgb(Palettes.pal_r[0], Palettes.pal_g[0], Palettes.pal_b[0]);

                    for (int i = 0; i < 8; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            image_map_local.SetPixel(temp_x3 + j, temp_y3 + i, zero_color);
                        }
                    }

                    
                    offset = z + (temp_y * 32) + temp_x;
                    
                    temp_tile = (Maps.tile[offset] * 8 * 8); // base offset for tile
                    
                    big_sub(offset, temp_x, temp_y, temp_tile);

                    //Bitmap temp_bmp2 = new Bitmap(512, 512); //resize double size
                    using (Graphics g2 = Graphics.FromImage(temp_bmp2))
                    {
                        g2.InterpolationMode = InterpolationMode.NearestNeighbor;
                        g2.PixelOffsetMode = PixelOffsetMode.Half; // fix bug, missing..
                                                                   // half a pixel on  top and left
                        g2.DrawImage(image_map_local, 0, 0, 512, 512);
                    } // standard resize of bmp was blurry, this makes it sharp

                    //draw grid here
                    if (checkBox4.Checked == true)
                    {
                        //draw horizontal lines at each 16
                        for (int i = 31; i < (map_height * 15); i += 32)
                        {
                            for (int j = 0; j < 510; j += 2)
                            {
                                temp_bmp2.SetPixel(j, i, Color.Black);
                                temp_bmp2.SetPixel(j + 1, i, Color.White);
                            }
                        }
                        //draw vertical lines at each 16
                        for (int j = 31; j < 511; j += 32)
                        {
                            for (int i = 0; i < (map_height * 16) - 2; i += 2)
                            {
                                temp_bmp2.SetPixel(j, i + 1, Color.Black);
                                temp_bmp2.SetPixel(j, i, Color.White);
                            }
                        }
                    }

                    pictureBox1.Image = temp_bmp2;
                    pictureBox1.Refresh();

                    return; // must return when done.
                }
            }
            else // brush = fill screen
            {
                start_x = temp_x = 0;
                temp_y = 0;
                loop_x = 32;
                loop_y = map_height;
            }





            // nested loop of tile changes, per brush size.
            for (int yy = 0; yy < loop_y; yy++)
            {
                for (int xx = 0; xx < loop_x; xx++)
                {
                    //tile change temp_y < map_height
                    if ((temp_y >= 0) && (temp_x >= 0) &&
                        (temp_y < map_height) && (temp_x < 32))
                    {
                        active_map_index = temp_x + (temp_y * 32) + (32 * 32 * which_map);

                        // palette irrelavent in 8bpp mode
                        if(tile_num2 > 959)
                        {
                            tile_num2 -= 960; // we are skipping the last few tiles.
                        }

                        Maps.tile[active_map_index] = tile_num2;
                        if (checkBox5.Checked == false)
                        {
                            Maps.h_flip[active_map_index] = 0;
                            checkBox1.Checked = false;
                        }
                        else
                        {
                            Maps.h_flip[active_map_index] = 1;
                            checkBox1.Checked = true;
                        }
                        if (checkBox6.Checked == false)
                        {
                            Maps.v_flip[active_map_index] = 0;
                            checkBox2.Checked = false;
                        }
                        else
                        {
                            Maps.v_flip[active_map_index] = 1;
                            checkBox2.Checked = true;
                        }


                        // draw 1 tile, but first fill with zero color
                        int temp_x2 = temp_x * 8;
                        int temp_y2 = temp_y * 8;
                        Color zero_color = Color.FromArgb(Palettes.pal_r[0], Palettes.pal_g[0], Palettes.pal_b[0]);

                        for (int i = 0; i < 8; i++)
                        {
                            for (int j = 0; j < 8; j++)
                            {
                                image_map_local.SetPixel(temp_x2 + j, temp_y2 + i, zero_color);
                            }
                        }

                        
                        offset = z + (temp_y * 32) + temp_x;
                        
                        temp_tile = (Maps.tile[offset] * 8 * 8); // base offset for tile
                        
                        big_sub(offset, temp_x, temp_y, temp_tile);


                    } // end of tile change 

                    if (brushsize == BRUSHNEXT)
                    {
                        
                        next_count++;
                        tile_num2 = next_tiles[next_count];
                    }
                    temp_x++;
                }

                temp_x = start_x;
                temp_y++;
            }

            //Bitmap temp_bmp2 = new Bitmap(512, 512); //resize double size
            using (Graphics g2 = Graphics.FromImage(temp_bmp2))
            {
                g2.InterpolationMode = InterpolationMode.NearestNeighbor;
                g2.PixelOffsetMode = PixelOffsetMode.Half; // fix bug, missing..
                                                           // half a pixel on  top and left
                g2.DrawImage(image_map_local, 0, 0, 512, 512);
            } // standard resize of bmp was blurry, this makes it sharp

            //draw grid here
            if (checkBox4.Checked == true)
            {
                //draw horizontal lines at each 16
                for (int i = 31; i < (map_height * 15); i += 32)
                {
                    for (int j = 0; j < 510; j += 2)
                    {
                        temp_bmp2.SetPixel(j, i, Color.Black);
                        temp_bmp2.SetPixel(j + 1, i, Color.White);
                    }
                }
                //draw vertical lines at each 16
                for (int j = 31; j < 511; j += 32)
                {
                    for (int i = 0; i < (map_height * 16) - 2; i += 2)
                    {
                        temp_bmp2.SetPixel(j, i + 1, Color.Black);
                        temp_bmp2.SetPixel(j, i, Color.White);
                    }
                }
            }

            pictureBox1.Image = temp_bmp2;
            pictureBox1.Refresh();


            //update_tilemap(); // moved
        }


        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (disable_map_click == 1) // click dialog boxes were causing mouse clicks on map
            {
                disable_map_click = 0;
                return;
            }

            if (brushsize == BRUSH_CLONE_M)
            {
                var mouseEventArgs = e as MouseEventArgs;
                if (mouseEventArgs == null) return;
                if (e.Button == MouseButtons.Left)
                {
                    active_map_x = map_clone_x;
                    active_map_y = map_clone_y;
                    label12.Text = "X = " + map_clone_x.ToString(); // change the numbers at the top
                    label13.Text = "Y = " + map_clone_y.ToString();
                    update_palette();
                    common_update2();
                }
            }

            update_tilemap();
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        { // tilemap
            disable_map_click = 0;
            if (bg_mode == BG_MODE_7P)
            {
                MessageBox.Show("Editing is disabled in Preview Mode.");
                return;
            }

            //Checkpoint();

            active_map_x = 0; active_map_y = 0;
            var mouseEventArgs = e as MouseEventArgs;
            if (mouseEventArgs != null)
            {
                active_map_x = mouseEventArgs.X >> 4;
                active_map_y = mouseEventArgs.Y >> 4;
            }
            if (active_map_x < 0) active_map_x = 0;
            if (active_map_x > 31) active_map_x = 31;
            if (active_map_y < 0) active_map_y = 0;
            if (active_map_y >= map_height)
            {
                active_map_y = map_height - 1;
                return;
            }
            label12.Text = "X = " + active_map_x.ToString(); // change the numbers at the top
            label13.Text = "Y = " + active_map_y.ToString();

            if (e.Button == MouseButtons.Left)
            {
                Checkpoint(); // only if left click

                clone_start_x = active_map_x;
                clone_start_y = active_map_y;


                last_tile_x = active_map_x; // to speed up the app
                last_tile_y = active_map_y; // see mouse move event
                picbox1_sub(); // place the tile and redraw the map
                //update_tilemap();
            }
            else if (e.Button == MouseButtons.Right) // get the tile, tileset, and properties
            {
                map_clone_x = active_map_x;
                map_clone_y = active_map_y;

                int tile = (which_map * 32 * 32) + (32 * active_map_y) + active_map_x;
                
                if (Maps.h_flip[tile] == 0) checkBox1.Checked = false;
                else checkBox1.Checked = true;
                if (Maps.v_flip[tile] == 0) checkBox2.Checked = false;
                else checkBox2.Checked = true;
                int set = (Maps.tile[tile] & 0x300) >> 8;
                if(bg_mode == BG_MODE_7)
                {
                    set = 0;
                }
                int tile2 = Maps.tile[tile] & 0xff;

                // don't allow last few tiles to be selected
                if((tile2 >= 192) && (set == 3))
                {
                    tile2 = 0;
                }

                tile_x = tile2 & 0x0f;
                tile_y = (tile2 >> 4) & 0x0f;
                tile_num = (tile_y * 16) + tile_x;
                tile_show_num();

                set1ToolStripMenuItem.Checked = false; // set them all to false
                set2ToolStripMenuItem.Checked = false;
                set3ToolStripMenuItem.Checked = false;
                set4ToolStripMenuItem.Checked = false;
                
                if (set == 0)
                {
                    label10.Text = "1";
                    tile_set = 0;
                    set1ToolStripMenuItem.Checked = true;
                }
                else if (set == 1)
                {
                    label10.Text = "2";
                    tile_set = 1;
                    set2ToolStripMenuItem.Checked = true;
                }
                else if (set == 2)
                {
                    label10.Text = "3";
                    tile_set = 2;
                    set3ToolStripMenuItem.Checked = true;
                }
                else
                {
                    label10.Text = "4";
                    tile_set = 3;
                    set4ToolStripMenuItem.Checked = true;
                }
                
                

                update_palette();
                common_update2();
            }

        } // end tilemap

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        { // TILEMAP

        } // END CLICKED ON TILEMAP



        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (bg_mode == BG_MODE_7P) return;
            if (disable_map_click == 1) return;

            if (e.Button == MouseButtons.Left)
            {
                active_map_x = 0; active_map_y = 0;
                var mouseEventArgs = e as MouseEventArgs;
                if (mouseEventArgs != null)
                {

                    active_map_x = mouseEventArgs.X >> 4;
                    active_map_y = mouseEventArgs.Y >> 4;
                }
                if (active_map_x < 0) active_map_x = 0;
                if (active_map_x > 31) active_map_x = 31;
                if (active_map_y < 0) active_map_y = 0;
                if (active_map_y >= map_height)
                {
                    active_map_y = map_height - 1;
                    return;
                }
                label12.Text = "X = " + active_map_x.ToString();
                label13.Text = "Y = " + active_map_y.ToString();

                if ((last_tile_x != active_map_x) || (last_tile_y != active_map_y))
                {
                    // only update if the tile under mouse has changed.
                    last_tile_x = active_map_x;
                    last_tile_y = active_map_y;
                    picbox1_sub();
                    //update_tilemap();
                }

            }
        }
        // END MOUSE DOWN MOVE ON TILEMAP




        //capure key presses on the tiles, focus is redirected to label 5
        private void label5_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            int selection = pal_x + (pal_y * 16);

            if (e.KeyCode == Keys.Left)
            {
                Checkpoint();
                e.IsInputKey = true;
                Tiles.shift_left();
            }
            else if (e.KeyCode == Keys.Up)
            {
                Checkpoint();
                e.IsInputKey = true;
                Tiles.shift_up();
            }
            else if (e.KeyCode == Keys.Right)
            {
                Checkpoint();
                e.IsInputKey = true;
                Tiles.shift_right();
            }
            else if (e.KeyCode == Keys.Down)
            {
                Checkpoint();
                e.IsInputKey = true;
                Tiles.shift_down();
            }

            else if (e.KeyCode == Keys.NumPad2)
            {
                if (tile_y < 15) tile_y++;
                tile_num = (tile_y * 16) + tile_x;
            }
            else if (e.KeyCode == Keys.NumPad4)
            {
                if (tile_x > 0) tile_x--;
                tile_num = (tile_y * 16) + tile_x;
            }
            else if (e.KeyCode == Keys.NumPad6)
            {
                if (tile_x < 15) tile_x++;
                tile_num = (tile_y * 16) + tile_x;
            }
            else if (e.KeyCode == Keys.NumPad8)
            {
                if (tile_y > 0) tile_y--;
                tile_num = (tile_y * 16) + tile_x;
            }
            else if (e.KeyCode == Keys.H)
            {
                Checkpoint();
                Tiles.tile_h_flip();
            }
            else if (e.KeyCode == Keys.V)
            {
                Checkpoint();
                Tiles.tile_v_flip();
            }
            else if (e.KeyCode == Keys.R)
            {
                Checkpoint();
                Tiles.tile_rot_cw();
            }
            else if (e.KeyCode == Keys.L)
            {
                Checkpoint();
                Tiles.tile_rot_ccw();
            }
            else if (e.KeyCode == Keys.Delete)
            {
                Checkpoint();
                Tiles.tile_delete();
            }
            else if (e.KeyCode == Keys.C)
            {
                Tiles.tile_copy();
            }
            else if (e.KeyCode == Keys.P)
            {
                Checkpoint();
                Tiles.tile_paste();
            }
            else if (e.KeyCode == Keys.F)
            {
                Checkpoint();
                Tiles.tile_fill();
            }

            else if (e.KeyCode == Keys.Q)
            { // palette copy selected color
                pal_r_copy = Palettes.pal_r[selection];
                pal_g_copy = Palettes.pal_g[selection];
                pal_b_copy = Palettes.pal_b[selection];
            }
            else if (e.KeyCode == Keys.W)
            { // palette paste selected to color
                Palettes.pal_r[selection] = (byte)pal_r_copy;
                Palettes.pal_g[selection] = (byte)pal_g_copy;
                Palettes.pal_b[selection] = (byte)pal_b_copy;
                update_palette();
                rebuild_pal_boxes();
            }
            else if (e.KeyCode == Keys.E)
            { // palette clear selected to color
                Palettes.pal_r[selection] = 0;
                Palettes.pal_g[selection] = 0;
                Palettes.pal_b[selection] = 0;
                update_palette();
                rebuild_pal_boxes();
            }

            else if (e.KeyCode == Keys.D1) // number buttons
            {
                set1_change(); // change the tileset
            }
            else if (e.KeyCode == Keys.D2)
            {
                set2_change();
            }
            else if (e.KeyCode == Keys.D3)
            {
                set3_change();
            }
            else if (e.KeyCode == Keys.D4)
            {
                set4_change();
            }
            
            else if (e.KeyCode == Keys.Z)
            {
                Do_Undo();
            }

            common_update2();
            // prevent change in focus
            label5.Focus();
        }







        private void common_update2()
        {
            if (newChild != null)
            {
                newChild.update_tile_box();
            }

            update_tile_image();
            update_tilemap();
        }

        private void x1ToolStripMenuItem_Click(object sender, EventArgs e)
        { // brush size
            brushsize = BRUSH1x1;
            x1ToolStripMenuItem.Checked = true;
            x3ToolStripMenuItem.Checked = false;
            x5ToolStripMenuItem.Checked = false;
            x2NextToolStripMenuItem.Checked = false;
            cloneFromTilesetToolStripMenuItem.Checked = false;
            cloneFromMapToolStripMenuItem.Checked = false;
            fillScreenToolStripMenuItem.Checked = false;
        }

        private void x3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            brushsize = BRUSH3x3;
            x1ToolStripMenuItem.Checked = false;
            x3ToolStripMenuItem.Checked = true;
            x5ToolStripMenuItem.Checked = false;
            x2NextToolStripMenuItem.Checked = false;
            cloneFromTilesetToolStripMenuItem.Checked = false;
            cloneFromMapToolStripMenuItem.Checked = false;
            fillScreenToolStripMenuItem.Checked = false;
        }

        private void x5ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            brushsize = BRUSH5x5;
            x1ToolStripMenuItem.Checked = false;
            x3ToolStripMenuItem.Checked = false;
            x5ToolStripMenuItem.Checked = true;
            x2NextToolStripMenuItem.Checked = false;
            cloneFromTilesetToolStripMenuItem.Checked = false;
            cloneFromMapToolStripMenuItem.Checked = false;
            fillScreenToolStripMenuItem.Checked = false;
        }

        // this should be in menuclicks.cs
        private void fillTopRowWithColorsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Checkpoint();

            int save_num = tile_num;
            int save_pal = pal_x;
            tile_num = 0;
            pal_x = 0;
            for (int a = 0; a < 16; a++)
            {
                Tiles.tile_fill();
                tile_num++;
                pal_x++;
            }

            tile_num = save_num;
            pal_x = save_pal;
            common_update2();
        }

        private void trackBar1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int val = trackBar1.Value * 8;
                textBox1.Text = val.ToString();

                update_rgb();
                update_box4();

                update_palette();

                common_update2();
            }
        }

        private void trackBar2_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int val = trackBar2.Value * 8;
                textBox2.Text = val.ToString();

                update_rgb();
                update_box4();

                update_palette();

                common_update2();
            }
        }

        private void trackBar3_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int val = trackBar3.Value * 8;
                textBox3.Text = val.ToString();

                update_rgb();
                update_box4();

                update_palette();

                common_update2();
            }
        }

        private void fillScreenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //BRUSH_FILL
            brushsize = BRUSH_FILL;
            x1ToolStripMenuItem.Checked = false;
            x3ToolStripMenuItem.Checked = false;
            x5ToolStripMenuItem.Checked = false;
            x2NextToolStripMenuItem.Checked = false;
            cloneFromTilesetToolStripMenuItem.Checked = false;
            cloneFromMapToolStripMenuItem.Checked = false;
            fillScreenToolStripMenuItem.Checked = true;
        }

        private void trackBar1_MouseUp(object sender, MouseEventArgs e)
        {
            label5.Focus();
        }

        private void trackBar2_MouseUp(object sender, MouseEventArgs e)
        {
            label5.Focus();
        }

        private void removeDuplicateTilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Checkpoint();

            RemoveDuplicateTiles();
            common_update2();
        }

        public void RemoveDuplicateTiles()
        {
            // this checks each tileset 1-4 / 5-8 and removes duplicates
            // this goes through each map and reorders the tile #
            // also flipped versions of a tile are removed

            // the 8bpp tilesets (1-4)
            int tile_so_far = 1;
            int max_tile = 960; // mode 3, skip the last few tiles
            if (bg_mode > 0) max_tile = 256; // mode 7, don't check past 1st set

            for (int tile = 1; tile < max_tile; tile++) // higher val
            {
                // compare to all previous tiles
                bool match_found = false;
                int match_index = 0;
                for (int tile2 = 0; tile2 < tile_so_far; tile2++) // lower val
                {
                    match_found = Compare2Tiles(tile, tile2);
                    if (match_found == true)
                    {
                        match_index = tile2; // the lower val, keeper
                        break;
                    }
                }

                if (match_found == true)
                {
                    // zeros out the higher tile chr
                    DeleteTile(tile);
                    // search map1 for the bad tile, replace with good
                    ReplaceTile(tile, match_index, 0); // bad, good, start offset
                    // search other maps
                    ReplaceTile(tile, match_index, 1024 * 1); // map2
                    ReplaceTile(tile, match_index, 1024 * 2); // map3
                    ReplaceTile(tile, match_index, 1024 * 3);
                    ReplaceTile(tile, match_index, 1024 * 4);
                    ReplaceTile(tile, match_index, 1024 * 5);
                    ReplaceTile(tile, match_index, 1024 * 6);
                    ReplaceTile(tile, match_index, 1024 * 7);
                    ReplaceTile(tile, match_index, 1024 * 8);
                    ReplaceTile(tile, match_index, 1024 * 9);
                    ReplaceTile(tile, match_index, 1024 * 10);
                    ReplaceTile(tile, match_index, 1024 * 11);
                    ReplaceTile(tile, match_index, 1024 * 12);
                    ReplaceTile(tile, match_index, 1024 * 13);
                    ReplaceTile(tile, match_index, 1024 * 14);
                    ReplaceTile(tile, match_index, 1024 * 15); // map 16
                }
                else // no match found
                {
                    // shift the tile down, maybe
                    if (tile != tile_so_far)
                    {
                        // (bad, good, start offset)
                        ReplaceTile(tile, tile_so_far, 0); // search map1
                        ReplaceTile(tile, tile_so_far, 1024 * 1); // search map2
                        ReplaceTile(tile, tile_so_far, 1024 * 2); // map 3
                        ReplaceTile(tile, tile_so_far, 1024 * 3);
                        ReplaceTile(tile, tile_so_far, 1024 * 4);
                        ReplaceTile(tile, tile_so_far, 1024 * 5);
                        ReplaceTile(tile, tile_so_far, 1024 * 6);
                        ReplaceTile(tile, tile_so_far, 1024 * 7);
                        ReplaceTile(tile, tile_so_far, 1024 * 8);
                        ReplaceTile(tile, tile_so_far, 1024 * 9);
                        ReplaceTile(tile, tile_so_far, 1024 * 10);
                        ReplaceTile(tile, tile_so_far, 1024 * 11);
                        ReplaceTile(tile, tile_so_far, 1024 * 12);
                        ReplaceTile(tile, tile_so_far, 1024 * 13);
                        ReplaceTile(tile, tile_so_far, 1024 * 14);
                        ReplaceTile(tile, tile_so_far, 1024 * 15); // map 16

                        CopyTile(tile, tile_so_far); // bad, good, start offset
                        DeleteTile(tile); // zero out the higher tile chr
                    }

                    tile_so_far++;
                }
            }


            //common_update2(); // moved
        }

        public bool Compare2Tiles(int tile, int tile2)
        { // tilset should be either 0 or 65536 (for 4bpp vs 2bpp tiles)
            // also check for flipped version of the tile
            
            int offset1 = (tile * 64);
            int offset2 = (tile2 * 64);
            flip_h = false;
            flip_v = false;
            bool same = true;

            // check unflipped
            for (int i = 0; i < 64; i++)
            { // 64 pixels per tile
                if (Tiles.Tile_Arrays[offset1] != Tiles.Tile_Arrays[offset2])
                {
                    same = false;
                    break;
                }

                offset1++;
                offset2++;
            }
            if (same == true) return true;

            if(bg_mode > 0) // mode 7 can't flip tiles
            {
                return false;
            }

            // check H flip version
            same = true;
            offset1 = (tile * 64);
            offset2 = (tile2 * 64);
            //int offset3 = 0;
            for (int i = 0; i < 64; i++)
            { // 64 pixels per tile
                if (Tiles.Tile_Arrays[offset1] != Tiles.Tile_Arrays[offset2 + H_FLIP_TABLE[i]])
                {
                    same = false;
                    break;
                }

                offset1++;
            }
            if (same == true)
            {
                flip_h = true;
                return true;
            }

            // check V flip version
            same = true;
            offset1 = (tile * 64);
            offset2 = (tile2 * 64);
            //int offset3 = 0;
            for (int i = 0; i < 64; i++)
            { // 64 pixels per tile
                if (Tiles.Tile_Arrays[offset1] != Tiles.Tile_Arrays[offset2 + V_FLIP_TABLE[i]])
                {
                    same = false;
                    break;
                }

                offset1++;
            }
            if (same == true)
            {
                flip_v = true;
                return true;
            }

            // check HV flip version
            same = true;
            offset1 = (tile * 64);
            offset2 = (tile2 * 64);
            //int offset3 = 0;
            for (int i = 0; i < 64; i++)
            { // 64 pixels per tile
                if (Tiles.Tile_Arrays[offset1] != Tiles.Tile_Arrays[offset2 + HV_FLIP_TABLE[i]])
                {
                    same = false;
                    break;
                }

                offset1++;
            }
            if (same == true)
            {
                flip_h = true;
                flip_v = true;
                return true;
            }

            return false;
        }

        // for the remove duplicate tile code
        public void DeleteTile(int tile)
        { // just put a zero there
            int offset3 = (tile * 64);
            for (int i = 0; i < 64; i++)
            {
                Tiles.Tile_Arrays[offset3++] = 0;
            }
        }


        public void CopyTile(int tile, int tile2) // high, low
        { // shift the tile down 
            
            int offset4 = (tile * 64); 
            int offset5 = (tile2 * 64);
            for (int i = 0; i < 64; i++)
            {
                Tiles.Tile_Arrays[offset5] = Tiles.Tile_Arrays[offset4];
                offset4++;
                offset5++;
            }
        }


        

        private void checkBox5_Click(object sender, EventArgs e)
        {
            label5.Focus();
        }

        private void checkBox6_Click(object sender, EventArgs e)
        {
            label5.Focus();
        }

        


        public void MapMenuSwitch()
        {
            map1ToolStripMenuItem.Checked = false;
            map2ToolStripMenuItem.Checked = false;
            map3ToolStripMenuItem.Checked = false;
            map4ToolStripMenuItem.Checked = false;
            map5ToolStripMenuItem.Checked = false;
            map6ToolStripMenuItem.Checked = false;
            map7ToolStripMenuItem.Checked = false;
            map8ToolStripMenuItem.Checked = false;
            map9ToolStripMenuItem.Checked = false;
            map10ToolStripMenuItem.Checked = false;
            map11ToolStripMenuItem.Checked = false;
            map12ToolStripMenuItem.Checked = false;
            map13ToolStripMenuItem.Checked = false;
            map14ToolStripMenuItem.Checked = false;
            map15ToolStripMenuItem.Checked = false;
            map16ToolStripMenuItem.Checked = false;

            if(bg_mode == BG_MODE_3)
            {
                which_map = 0;
            }

            switch (which_map)
            {
                default:
                case 0:
                    which_map_x = 0;
                    label8.Text = "Map X = 0";
                    which_map_y = 0;
                    label14.Text = "Map Y = 0";
                    map1ToolStripMenuItem.Checked = true;
                    break;
                case 1:
                    which_map_x = 1;
                    label8.Text = "Map X = 1";
                    which_map_y = 0;
                    label14.Text = "Map Y = 0";
                    map2ToolStripMenuItem.Checked = true;
                    break;
                case 2:
                    which_map_x = 2;
                    label8.Text = "Map X = 2";
                    which_map_y = 0;
                    label14.Text = "Map Y = 0";
                    map3ToolStripMenuItem.Checked = true;
                    break;
                case 3:
                    which_map_x = 3;
                    label8.Text = "Map X = 3";
                    which_map_y = 0;
                    label14.Text = "Map Y = 0";
                    map4ToolStripMenuItem.Checked = true;
                    break;
                case 4:
                    which_map_x = 0;
                    label8.Text = "Map X = 0";
                    which_map_y = 1;
                    label14.Text = "Map Y = 1";
                    map5ToolStripMenuItem.Checked = true;
                    break;
                case 5:
                    which_map_x = 1;
                    label8.Text = "Map X = 1";
                    which_map_y = 1;
                    label14.Text = "Map Y = 1";
                    map6ToolStripMenuItem.Checked = true;
                    break;
                case 6:
                    which_map_x = 2;
                    label8.Text = "Map X = 2";
                    which_map_y = 1;
                    label14.Text = "Map Y = 1";
                    map7ToolStripMenuItem.Checked = true;
                    break;
                case 7:
                    which_map_x = 3;
                    label8.Text = "Map X = 3";
                    which_map_y = 1;
                    label14.Text = "Map Y = 1";
                    map8ToolStripMenuItem.Checked = true;
                    break;
                case 8:
                    which_map_x = 0;
                    label8.Text = "Map X = 0";
                    which_map_y = 2;
                    label14.Text = "Map Y = 2";
                    map9ToolStripMenuItem.Checked = true;
                    break;
                case 9:
                    which_map_x = 1;
                    label8.Text = "Map X = 1";
                    which_map_y = 2;
                    label14.Text = "Map Y = 2";
                    map10ToolStripMenuItem.Checked = true;
                    break;
                case 10:
                    which_map_x = 2;
                    label8.Text = "Map X = 2";
                    which_map_y = 2;
                    label14.Text = "Map Y = 2";
                    map11ToolStripMenuItem.Checked = true;
                    break;
                case 11:
                    which_map_x = 3;
                    label8.Text = "Map X = 3";
                    which_map_y = 2;
                    label14.Text = "Map Y = 2";
                    map12ToolStripMenuItem.Checked = true;
                    break;
                case 12:
                    which_map_x = 0;
                    label8.Text = "Map X = 0";
                    which_map_y = 3;
                    label14.Text = "Map Y = 3";
                    map13ToolStripMenuItem.Checked = true;
                    break;
                case 13:
                    which_map_x = 1;
                    label8.Text = "Map X = 1";
                    which_map_y = 3;
                    label14.Text = "Map Y = 3";
                    map14ToolStripMenuItem.Checked = true;
                    break;
                case 14:
                    which_map_x = 2;
                    label8.Text = "Map X = 2";
                    which_map_y = 3;
                    label14.Text = "Map Y = 3";
                    map15ToolStripMenuItem.Checked = true;
                    break;
                case 15:
                    which_map_x = 3;
                    label8.Text = "Map X = 3";
                    which_map_y = 3;
                    label14.Text = "Map Y = 3";
                    map16ToolStripMenuItem.Checked = true;
                    break;

            }
        }

        

        private void checkBox7_Click(object sender, EventArgs e)
        {
            label5.Focus();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        

        public void ReplaceTile(int bad_tile, int good_tile, int map_offset)
        { // mapset should be 0, 1024, or 2048 (map1, map2, map3, etc)
            for (int i = 0; i < 1024; i++)
            {
                if(Maps.tile[map_offset] == bad_tile)
                {
                    Maps.tile[map_offset] = good_tile;
                    if(flip_h == true)
                    {
                        Maps.h_flip[map_offset] = Maps.h_flip[map_offset] ^ 1;
                        // bitwise XOR, 1 -> 0, 0 -> 1
                    }
                    if (flip_v == true)
                    {
                        Maps.v_flip[map_offset] = Maps.v_flip[map_offset] ^ 1;
                        // bitwise XOR, 1 -> 0, 0 -> 1
                    }

                }
                map_offset++;
            }
        }

        private void trackBar3_MouseUp(object sender, MouseEventArgs e)
        {
            label5.Focus();
        }

        private void getPaletteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // generate a palette from the image
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "Open Image";
                dlg.Filter = "Image Files .png .jpg .bmp .gif)|*.png;*.jpg;*.bmp;*.gif|" + "All Files (*.*)|*.*";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    Bitmap import_bmp = new Bitmap(dlg.FileName);

                    if ((import_bmp.Height < 8) || (import_bmp.Width < 8))
                    {
                        MessageBox.Show("Error. File too small?");
                        import_bmp.Dispose();
                        return;
                    }
                    if (bg_mode > 0) // mode 7
                    {
                        if ((import_bmp.Height > 128) || (import_bmp.Width > 128))
                        {
                            MessageBox.Show("Error. File too large. 128x128 max for mode 7.");
                            import_bmp.Dispose();
                            return;
                        }
                    }
                    if ((import_bmp.Height > 240) || (import_bmp.Width > 256))
                    {
                        MessageBox.Show("Error. File too large. 256x240 max for mode 3.");
                        import_bmp.Dispose();
                        return;
                    }


                    int num_col_to_find, start_offset;

                    if (max_import_color < 2) max_import_color = 2;
                    if (max_import_color > 256) max_import_color = 256;
                    num_col_to_find = max_import_color; //256;
                    start_offset = 0;


                    image_height = import_bmp.Height;
                    image_width = import_bmp.Width;
                    Color temp_color;
                    // copy the bitmap, crop but don't resize
                    // copy pixel by pixel
                    for (int xx = 0; xx < 256; xx++)
                    {
                        for (int yy = 0; yy < 256; yy++)
                        {
                            if ((xx < image_width) && (yy < image_height))
                            {
                                temp_color = import_bmp.GetPixel(xx, yy);
                            }
                            else
                            {
                                temp_color = Color.Gray;
                            }
                            cool_bmp.SetPixel(xx, yy, temp_color);
                        }
                    }



                    int color_found = 0;
                    int red = 0, blue = 0, green = 0;
                    int temp_var, closest_cnt, added;

                    // default colors

                    // blank the arrays
                    for (int i = 0; i < 65536; i++)
                    {
                        R_Array[i] = 0;
                        G_Array[i] = 0;
                        B_Array[i] = 0;
                        Count_Array[i] = 0;
                    }
                    color_count = 0;

                    Color tempcolor = Color.Black;

                    // read all possible colors from the orig image
                    // removing duplicates, keep track of how many
                    for (int yy = 0; yy < image_height; yy++)
                    {
                        for (int xx = 0; xx < image_width; xx++)
                        {
                            tempcolor = cool_bmp.GetPixel(xx, yy);
                            // speed it up, narrow the possibilities.
                            red = tempcolor.R & 0xf8;
                            blue = tempcolor.G & 0xf8;
                            green = tempcolor.B & 0xf8;
                            tempcolor = Color.FromArgb(red, blue, green);

                            // compare to all other colors, add if not present
                            if (color_count == 0)
                            {
                                Add_Color(tempcolor);
                                continue;
                            }

                            color_found = 0;
                            for (int i = 0; i < color_count; i++)
                            {
                                if ((tempcolor.R == R_Array[i] &&
                                    tempcolor.G == G_Array[i] &&
                                    tempcolor.B == B_Array[i]))
                                { // color match found
                                    Count_Array[i] = Count_Array[i] + 1;
                                    color_found = 1;
                                    break;
                                }
                            }
                            // no color match found
                            if (color_found == 0)
                            {
                                Add_Color(tempcolor);
                            }

                        }
                    }
                    
                    // this mid point algorithm tends avoid extremes
                    // give extra weight to the lowest value and the highest value
                    // first find the darkest and lightest colors
                    int darkest = 999;
                    int darkest_index = 0;
                    int lightest = 0;
                    int lightest_index = 0;
                    for (int i = 0; i < color_count; i++)
                    {
                        added = R_Array[i] + G_Array[i] + B_Array[i];
                        if (added < darkest)
                        {
                            darkest = added;
                            darkest_index = i;
                        }
                        if (added > lightest)
                        {
                            lightest = added;
                            lightest_index = i;
                        }
                    }
                    // give more count to them
                    temp_var = image_width * image_height / 8; // 8 is magic
                    Count_Array[darkest_index] += temp_var;
                    Count_Array[lightest_index] += temp_var;

                    // then reduce to 256 colors, using a mid point merge with
                    // the closest neighbor color

                    int color_count2 = color_count;
                    while (color_count2 > num_col_to_find)
                    {
                        //find the least count
                        int least_index = 0;
                        int least_cnt = 99999;
                        for (int i = 0; i < color_count; i++)
                        {
                            if (Count_Array[i] == 0) continue;
                            if (Count_Array[i] < least_cnt)
                            {
                                least_cnt = Count_Array[i];
                                least_index = i;
                            }
                        }
                        // delete itself
                        Count_Array[least_index] = 0;

                        int closest_index = 0;
                        int closest_val = 999999;
                        r_val = R_Array[least_index];
                        g_val = G_Array[least_index];
                        b_val = B_Array[least_index];
                        int dR = 0, dG = 0, dB = 0;

                        // find the closest to that one
                        for (int i = 0; i < color_count; i++)
                        {
                            if (Count_Array[i] == 0) continue;
                            dR = r_val - R_Array[i];
                            dG = g_val - G_Array[i];
                            dB = b_val - B_Array[i];
                            diff_val = ((dR * dR) + (dG * dG) + (dB * dB));

                            
                            if (diff_val < closest_val)
                            {
                                closest_val = diff_val;
                                closest_index = i;
                            }
                        }

                        closest_cnt = Count_Array[closest_index];

                        // merge closet index with least index, mid point
                        temp_var = (closest_cnt + least_cnt);
                        // the algorithm was (color1 + color2) / 2
                        // but now, multiplied each by their count, div by both counts
                        r_val = (R_Array[least_index] * least_cnt) + (R_Array[closest_index] * closest_cnt);
                        r_val = (int)Math.Round((double)r_val / temp_var);
                        g_val = (G_Array[least_index] * least_cnt) + (G_Array[closest_index] * closest_cnt);
                        g_val = (int)Math.Round((double)g_val / temp_var);
                        b_val = (B_Array[least_index] * least_cnt) + (B_Array[closest_index] * closest_cnt);
                        b_val = (int)Math.Round((double)b_val / temp_var);
                        R_Array[closest_index] = r_val;
                        G_Array[closest_index] = g_val;
                        B_Array[closest_index] = b_val;
                        Count_Array[closest_index] = closest_cnt + least_cnt;

                        color_count2--;

                    }

                    // always palette zero
                    // zero fill the palette, before filling (black)
                    for (int i = 0; i < 256; i++)
                    {
                        int j = start_offset + i;
                        Palettes.pal_r[j] = 0;
                        Palettes.pal_g[j] = 0;
                        Palettes.pal_b[j] = 0;
                    }
                    // then go through the array and pull out numbers
                    int findindex = 0;
                    int color_count3 = 0;
                    while (color_count3 < color_count2)
                    {
                        if(Count_Array[findindex] != 0)
                        {
                            c256ColorIndexes[color_count3] = findindex;
                            color_count3++;
                        }

                        findindex++;
                        if (findindex >= 65536) break;

                    }

                    // then sort by darkness
                    for(int i = 0; i < 256; i++) // zero them
                    {
                        c256ColorsAdded[i] = 0;
                    }
                    for (int i = 0; i < color_count2; i++) // add them up (rough brightness)
                    {
                        c256ColorsAdded[i] += R_Array[c256ColorIndexes[i]];
                        c256ColorsAdded[i] += G_Array[c256ColorIndexes[i]];
                        c256ColorsAdded[i] += B_Array[c256ColorIndexes[i]];
                    }
                    int temp_val;
                    while (true)
                    {
                        bool sorted = true;
                        for (int i = 0; i < color_count2-1; i++) // add them up (rough brightness)
                        {
                            if(c256ColorsAdded[i] > c256ColorsAdded[i+1])
                            {
                                sorted = false;
                                // swap them
                                temp_val = c256ColorsAdded[i];
                                c256ColorsAdded[i] = c256ColorsAdded[i + 1];
                                c256ColorsAdded[i + 1] = temp_val;
                                temp_val = c256ColorIndexes[i];
                                c256ColorIndexes[i] = c256ColorIndexes[i + 1];
                                c256ColorIndexes[i + 1] = temp_val;
                            }
                        }
                        if (sorted == true) break;
                    }
                    

                    // then fill the palette with the colors
                    for (int i = 0; i < color_count2; i++)
                    {
                        int j = start_offset + i;
                        Palettes.pal_r[j] = (byte)(R_Array[c256ColorIndexes[i]] & 0xf8);
                        Palettes.pal_g[j] = (byte)(G_Array[c256ColorIndexes[i]] & 0xf8);
                        Palettes.pal_b[j] = (byte)(B_Array[c256ColorIndexes[i]] & 0xf8);
                    }



                    // then update the palette image
                    update_palette();

                    //update the boxes
                    rebuild_pal_boxes();

                    common_update2();
                    import_bmp.Dispose();
                }
            }
        }

        public void Add_Color(Color tempcolor)
        {
            R_Array[color_count] = tempcolor.R;
            G_Array[color_count] = tempcolor.G;
            B_Array[color_count] = tempcolor.B;
            Count_Array[color_count] = 1;

            color_count++;
        }

        private void imageToCHRToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // import an image, generate CHR based on existing palette
            // also fill current map with it.
            
            
            // load image, generate CHR from it
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "Open Image";
                dlg.Filter = "Image Files .png .jpg .bmp .gif)|*.png;*.jpg;*.bmp;*.gif|" + "All Files (*.*)|*.*";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    undo_ready = false;
                    
                    Bitmap import_bmp = new Bitmap(dlg.FileName);

                    if ((import_bmp.Height < 8) || (import_bmp.Width < 8))
                    {
                        MessageBox.Show("Error. File too small?");
                        import_bmp.Dispose();
                        return;
                    }
                    if (bg_mode > 0) // mode 7
                    {
                        if ((import_bmp.Height > 128) || (import_bmp.Width > 128))
                        {
                            MessageBox.Show("Error. File too large. 128x128 max for mode 7.");
                            import_bmp.Dispose();
                            return;
                        }
                    }
                    if ((import_bmp.Height > 240) || (import_bmp.Width > 256))
                    {
                        MessageBox.Show("Error. File too large. 256x240 max for mode 3.");
                        import_bmp.Dispose();
                        return;
                    }

                    int num_col, start_offset;

                    num_col = 256;
                    start_offset = 0;


                    image_height = import_bmp.Height;
                    image_width = import_bmp.Width;

                    Color temp_color;
                    // copy the bitmap, crop but don't resize
                    // copy pixel by pixel
                    for (int xx = 0; xx < 256; xx++)
                    {
                        for (int yy = 0; yy < 256; yy++)
                        {
                            if ((xx < image_width) && (yy < image_height))
                            {
                                temp_color = import_bmp.GetPixel(xx, yy);
                            }
                            else
                            {
                                temp_color = Color.Gray;
                            }
                            cool_bmp.SetPixel(xx, yy, temp_color);
                        }
                    }
                    

                    int final_y, final_x, best_index, chr_index, tile_num, pixel_num;
                    //int temp_set = 0;
                    int count = 0;

                    // get best color for each pixel
                    // copied to int array, needy_chr_array

                    
                    dither_db = dither_factor / 25.0; // ??

                    dither_adjust = (int)(dither_db * 32.0);
                    int red, green, blue, bayer_val;

                    for (int y = 0; y < 256; y++)
                    {
                        for (int x = 0; x < 256; x++)
                        {
                            if((x >= image_width) || (y >= image_height))
                            {
                                needy_chr_array[count] = 0;
                            }
                            else
                            {
                                // get the pixel and find its best color
                                temp_color = cool_bmp.GetPixel(x, y);

                                if(dither_factor != 0)
                                {
                                    // add dithering
                                    red = temp_color.R - dither_adjust; // keep it from lightening
                                    green = temp_color.G - dither_adjust;
                                    blue = temp_color.B - dither_adjust;
                                    bayer_val = BAYER_MATRIX[x % 8, y % 8];
                                    bayer_val = (int)((double)bayer_val * dither_db);
                                    red += bayer_val;
                                    red = Math.Max(0, red); // clamp min max
                                    red = Math.Min(255, red);
                                    green += bayer_val;
                                    green = Math.Max(0, green);
                                    green = Math.Min(255, green);
                                    blue += bayer_val;
                                    blue = Math.Max(0, blue);
                                    blue = Math.Min(255, blue);
                                    temp_color = Color.FromArgb(red, green, blue);
                                }

                                best_index = Best_Color(temp_color, num_col, start_offset);
                                needy_chr_array[count] = best_index;
                            }
                            
                            count++;
                        }
                    }

                    // copy image to CHR
                    // do in 128x128 segments so it looks pretty
                    // working through each tile, one at a time
                    // this is a bit of a mess. ugh.
                    // the tile system goes tile by tile, y*8 + x
                    tile_num = 0;
                    

                    for (int segment_y = 0; segment_y < 256; segment_y += 128)
                    {
                        for (int segment_x = 0; segment_x < 256; segment_x += 128)
                        {
                            for (int y1 = 0; y1 < 128; y1 += 8) // 16 tiles of 8x8
                            {
                                for (int x1 = 0; x1 < 128; x1 += 8) // ditto
                                {
                                    for (int y2 = 0; y2 < 8; y2++) // 8 pixels tall
                                    {
                                        for (int x2 = 0; x2 < 8; x2++) // 8 pixels wide
                                        {
                                            final_x = segment_x + x1 + x2;
                                            final_y = segment_y + y1 + y2;
                                            
                                            pixel_num = (final_y * 256) + final_x;
                                            // 64 bytes per tile

                                            chr_index = (tile_num * 64) + (y2 * 8) + x2;
                                            
                                            Tiles.Tile_Arrays[chr_index] = needy_chr_array[pixel_num];
                                        }
                                    }
                                    tile_num++;
                                }
                            }

                        }
                    }



                    // fix for 240 px tall max
                    // shift the 4th tileset up to the 3rd
                    // (2 * 256 * 8 * 8) + (224 * 8 * 8) = 32768 + 14336 = 47104
                    
                    int forward = 0;
                    for (int i = 47104; i < 63488; i++)
                    {
                        forward = i + 2048;
                        Tiles.Tile_Arrays[i] = Tiles.Tile_Arrays[forward];
                    }
                    // blank the end
                    for (int i = 63488; i < 65536; i++)
                    {
                        Tiles.Tile_Arrays[i] = 0;
                    }



                    
                    for(int i = 0; i < 1024; i++) // fill attributes with 0
                    {
                        Maps.palette[i] = 0; // pal_sel;
                        Maps.h_flip[i] = 0;
                        Maps.v_flip[i] = 0;
                        Maps.priority[i] = 0;
                        Maps.tile[i] = 0;
                    }
                    // each 128x128 block separately with tiles
                    tile_num = 0;
                    int map_offset2 = 0;
                    for (int y = 0; y < 16; y++)
                    {
                        for(int x = 0; x < 16; x++)
                        {
                            map_offset2 = (y * 32) + x;
                            Maps.tile[map_offset2] = tile_num++;
                        }
                    }
                    if(bg_mode == BG_MODE_3)
                    {
                        for (int y = 0; y < 16; y++)
                        {
                            for (int x = 16; x < 32; x++)
                            {
                                map_offset2 = (y * 32) + x;
                                Maps.tile[map_offset2] = tile_num++;
                            }
                        }
                        // y height max 30 now
                        for (int y = 16; y < 30; y++)
                        {
                            for (int x = 0; x < 16; x++)
                            {
                                map_offset2 = (y * 32) + x;
                                Maps.tile[map_offset2] = tile_num++;
                            }
                        }
                        // y height max 30 now
                        for (int y = 16; y < 30; y++)
                        {
                            for (int x = 16; x < 32; x++)
                            {
                                map_offset2 = (y * 32) + x;
                                Maps.tile[map_offset2] = tile_num++;
                            }
                        }
                    }
                    // skip if mode 7, not enough unique tiles

                    

                    // change map height and box
                    image_height = (image_height + 7) & 0x1f8;
                    if (image_height > 256) image_height = 256;
                    if (image_height < 8) image_height = 8;
                    map_height = image_height / 8; //1-32
                    //mode 3
                    textBox6.Text = map_height.ToString();
                    if (bg_mode > 0) // mode 7
                    {
                        map_height = 32;
                        textBox6.Text = "128";
                    }

                    // remove duplicates
                    if (f3_cb2 == true)
                    {
                        RemoveDuplicateTiles();
                    }

                    // redraw everything
                    common_update2();

                    import_bmp.Dispose();
                }
            }
        }

        public int Best_Color(Color temp_color, int num_col, int start_offset)
        {
            int best_index = 0;
            int best_count = 99999;

            for(int i = 0; i < num_col; i++)
            {
                int i2 = start_offset + i;
                int red = Palettes.pal_r[i2] - temp_color.R;
                red = Math.Abs(red);
                int green = Palettes.pal_g[i2] - temp_color.G;
                green = Math.Abs(green);
                int blue = Palettes.pal_b[i2] - temp_color.B;
                blue = Math.Abs(blue);
                int sum = red + green + blue;
                if(sum < best_count)
                {
                    best_count = sum;
                    best_index = i;
                }
            }

            return best_index;
        }

        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // open Form3, Options for importing images
            if (newChild3 != null)
            {
                newChild3.BringToFront();
            }
            else
            {
                newChild3 = new frmOptions();
                newChild3.Owner = this;
                
                newChild3.Top = this.Top + 100;
                newChild3.Left = this.Left + 300;

                newChild3.Show();
                
            }
        }

        private void x2NextToolStripMenuItem_Click(object sender, EventArgs e)
        { // drop current tile and it's neighbors in a 16x16 box
            brushsize = BRUSHNEXT;
            x1ToolStripMenuItem.Checked = false;
            x3ToolStripMenuItem.Checked = false;
            x5ToolStripMenuItem.Checked = false;
            x2NextToolStripMenuItem.Checked = true;
            cloneFromTilesetToolStripMenuItem.Checked = false;
            cloneFromMapToolStripMenuItem.Checked = false;
            fillScreenToolStripMenuItem.Checked = false;
        }

        private void cloneFromTilesetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            brushsize = BRUSH_CLONE_T;
            x1ToolStripMenuItem.Checked = false;
            x3ToolStripMenuItem.Checked = false;
            x5ToolStripMenuItem.Checked = false;
            x2NextToolStripMenuItem.Checked = false;
            cloneFromTilesetToolStripMenuItem.Checked = true;
            cloneFromMapToolStripMenuItem.Checked = false;
            fillScreenToolStripMenuItem.Checked = false;
        }

        private void cloneFromMapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            brushsize = BRUSH_CLONE_M;
            x1ToolStripMenuItem.Checked = false;
            x3ToolStripMenuItem.Checked = false;
            x5ToolStripMenuItem.Checked = false;
            x2NextToolStripMenuItem.Checked = false;
            cloneFromTilesetToolStripMenuItem.Checked = false;
            cloneFromMapToolStripMenuItem.Checked = true;
            fillScreenToolStripMenuItem.Checked = false;
        }

        private void button2_Click(object sender, EventArgs e)
        { // shift map left
            int temp_offset, temp_tile, temp_palette, temp_h_flip, temp_v_flip, temp_priority;
            int temp_offset2;
            if (bg_mode == BG_MODE_7P) return;
            temp_offset = 32 * 32 * which_map;

            Checkpoint();

            for(int yy = 0; yy < 32; yy++)
            {
                temp_offset2 = temp_offset + (yy * 32);
                // save left most column
                temp_tile = Maps.tile[temp_offset2];
                temp_palette = Maps.palette[temp_offset2];
                temp_h_flip = Maps.h_flip[temp_offset2];
                temp_v_flip = Maps.v_flip[temp_offset2];
                temp_priority = Maps.priority[temp_offset2];

                for (int xx = 0; xx < 31; xx++)
                {
                    // shift every tile left 1
                    Maps.tile[temp_offset2 + xx] = Maps.tile[temp_offset2 + xx + 1];
                    Maps.palette[temp_offset2 + xx] = Maps.palette[temp_offset2 + xx + 1];
                    Maps.h_flip[temp_offset2 + xx] = Maps.h_flip[temp_offset2 + xx + 1];
                    Maps.v_flip[temp_offset2 + xx] = Maps.v_flip[temp_offset2 + xx + 1];
                    Maps.priority[temp_offset2 + xx] = Maps.priority[temp_offset2 + xx + 1];
                }
                // put left most column on right
                Maps.tile[temp_offset2+31] = temp_tile;
                Maps.palette[temp_offset2+31] = temp_palette;
                Maps.h_flip[temp_offset2+31] = temp_h_flip;
                Maps.v_flip[temp_offset2+31] = temp_v_flip;
                Maps.priority[temp_offset2+31] = temp_priority;
            }
            common_update2();
            label5.Focus();
        }

        private void button3_Click(object sender, EventArgs e)
        { // shift map right
            int temp_offset, temp_tile, temp_palette, temp_h_flip, temp_v_flip, temp_priority;
            int temp_offset2;
            if (bg_mode == BG_MODE_7P) return;
            temp_offset = 32 * 32 * which_map;

            Checkpoint();

            for (int yy = 0; yy < 32; yy++)
            {
                temp_offset2 = temp_offset + (yy * 32);
                // save right most column
                temp_tile = Maps.tile[temp_offset2 + 31];
                temp_palette = Maps.palette[temp_offset2 + 31];
                temp_h_flip = Maps.h_flip[temp_offset2 + 31];
                temp_v_flip = Maps.v_flip[temp_offset2 + 31];
                temp_priority = Maps.priority[temp_offset2 + 31];

                for (int xx = 30; xx >= 0; xx--)
                {
                    // shift every tile right 1
                    Maps.tile[temp_offset2 + xx + 1] = Maps.tile[temp_offset2 + xx];
                    Maps.palette[temp_offset2 + xx + 1] = Maps.palette[temp_offset2 + xx];
                    Maps.h_flip[temp_offset2 + xx + 1] = Maps.h_flip[temp_offset2 + xx];
                    Maps.v_flip[temp_offset2 + xx + 1] = Maps.v_flip[temp_offset2 + xx];
                    Maps.priority[temp_offset2 + xx + 1] = Maps.priority[temp_offset2 + xx];
                }
                // put right most column on left
                Maps.tile[temp_offset2] = temp_tile;
                Maps.palette[temp_offset2] = temp_palette;
                Maps.h_flip[temp_offset2] = temp_h_flip;
                Maps.v_flip[temp_offset2] = temp_v_flip;
                Maps.priority[temp_offset2] = temp_priority;
            }
            common_update2();
            label5.Focus();
        }

        private void button4_Click(object sender, EventArgs e)
        { // shift map up
            int temp_offset, temp_tile, temp_palette, temp_h_flip, temp_v_flip, temp_priority;
            int temp_offset2, temp_offset3;
            if (bg_mode == BG_MODE_7P) return;
            temp_offset = 32 * 32 * which_map;

            Checkpoint();

            for (int xx = 0; xx < 32; xx++)
            {
                temp_offset2 = temp_offset + xx;
                // save top most row
                temp_tile = Maps.tile[temp_offset2];
                temp_palette = Maps.palette[temp_offset2];
                temp_h_flip = Maps.h_flip[temp_offset2];
                temp_v_flip = Maps.v_flip[temp_offset2];
                temp_priority = Maps.priority[temp_offset2];

                for (int yy = 0; yy < 31; yy++)
                {
                    // shift every tile up 1
                    temp_offset3 = temp_offset2 + (yy * 32);
                    Maps.tile[temp_offset3] = Maps.tile[temp_offset3 + 32];
                    Maps.palette[temp_offset3] = Maps.palette[temp_offset3 + 32];
                    Maps.h_flip[temp_offset3] = Maps.h_flip[temp_offset3 + 32];
                    Maps.v_flip[temp_offset3] = Maps.v_flip[temp_offset3 + 32];
                    Maps.priority[temp_offset3] = Maps.priority[temp_offset3 + 32];
                }
                // put top most row on bottom
                Maps.tile[temp_offset2 + (31 * 32)] = temp_tile;
                Maps.palette[temp_offset2 + (31 * 32)] = temp_palette;
                Maps.h_flip[temp_offset2 + (31 * 32)] = temp_h_flip;
                Maps.v_flip[temp_offset2 + (31 * 32)] = temp_v_flip;
                Maps.priority[temp_offset2 + (31 * 32)] = temp_priority;
            }
            common_update2();
            label5.Focus();
        }

        private void button5_Click(object sender, EventArgs e)
        { // shift map down
            int temp_offset, temp_tile, temp_palette, temp_h_flip, temp_v_flip, temp_priority;
            int temp_offset2, temp_offset3;
            if (bg_mode == BG_MODE_7P) return;
            temp_offset = 32 * 32 * which_map;

            Checkpoint();

            for (int xx = 0; xx < 32; xx++)
            {
                temp_offset2 = temp_offset + xx;
                // save bottom most row
                temp_tile = Maps.tile[temp_offset2 + (31 * 32)];
                temp_palette = Maps.palette[temp_offset2 + (31 * 32)];
                temp_h_flip = Maps.h_flip[temp_offset2 + (31 * 32)];
                temp_v_flip = Maps.v_flip[temp_offset2 + (31 * 32)];
                temp_priority = Maps.priority[temp_offset2 + (31 * 32)];

                for (int yy = 30; yy >= 0; yy--)
                {
                    // shift every tile down 1
                    temp_offset3 = temp_offset2 + (yy * 32);
                    Maps.tile[temp_offset3 + 32] = Maps.tile[temp_offset3];
                    Maps.palette[temp_offset3 + 32] = Maps.palette[temp_offset3];
                    Maps.h_flip[temp_offset3 + 32] = Maps.h_flip[temp_offset3];
                    Maps.v_flip[temp_offset3 + 32] = Maps.v_flip[temp_offset3];
                    Maps.priority[temp_offset3 + 32] = Maps.priority[temp_offset3];
                }
                // put bottom most row on top
                Maps.tile[temp_offset2] = temp_tile;
                Maps.palette[temp_offset2] = temp_palette;
                Maps.h_flip[temp_offset2] = temp_h_flip;
                Maps.v_flip[temp_offset2] = temp_v_flip;
                Maps.priority[temp_offset2] = temp_priority;
            }
            common_update2();
            label5.Focus();
        }

        

        private void button1_Click(object sender, EventArgs e)
        { // color picker
            if(colorDialog1.ShowDialog() == DialogResult.OK)
            {
                Color tempcolor = colorDialog1.Color;
                int red = tempcolor.R & 0xf8;
                int green = tempcolor.G & 0xf8;
                int blue = tempcolor.B & 0xf8;
                textBox1.Text = red.ToString();
                textBox2.Text = green.ToString();
                textBox3.Text = blue.ToString();
                
                update_rgb(); //updates trackbars too
                update_box4();
                update_palette();
                common_update2();
            }
        }



        



        private void textBox6_KeyPress(object sender, KeyPressEventArgs e)
        { // the map height box, 1-32
            if (e.KeyChar == (char)Keys.Return)
            {
                if(bg_mode != 0)
                {
                    map_height = 32;
                    textBox6.Text = "128";
                    e.Handled = true; // prevent ding on return press
                    label5.Focus();
                    return;
                }
                
                
                string str = textBox6.Text;
                int value = 32; //default
                int.TryParse(str, out value);
                if (value > 32) value = 32; // max value
                if (value < 1) value = 32; // just use default if error
                str = value.ToString();
                textBox6.Text = str;

                map_height = value; //1-32

                update_tilemap();
                e.Handled = true; // prevent ding on return press
                label5.Focus();
            }
        }



        private void checkBox1_Click(object sender, EventArgs e)
        { // h flip
            if (bg_mode == BG_MODE_7P) return;

            Checkpoint();

            active_map_index = active_map_x + (active_map_y * 32) + (32 * 32 * bg_mode);
            if (checkBox1.Checked == false)
            {
                Maps.h_flip[active_map_index] = 0;
            }
            else
            {
                Maps.h_flip[active_map_index] = 1;
            }

            update_tilemap();
            label5.Focus();
        }



        private void checkBox2_Click(object sender, EventArgs e)
        { // v flip
            if (bg_mode == BG_MODE_7P) return;

            Checkpoint();

            active_map_index = active_map_x + (active_map_y * 32) + (32 * 32 * bg_mode);
            if (checkBox2.Checked == false)
            {
                Maps.v_flip[active_map_index] = 0;
            }
            else
            {
                Maps.v_flip[active_map_index] = 1;
            }

            update_tilemap();
            label5.Focus();
        }

        

        public static void draw_palettes() // sub routine of update palette
        {
            int count = 0;
            //Bitmap temp_bm = new Bitmap(256, 256); // very small, will zoom it later
            SolidBrush temp_brush = new SolidBrush(Color.White);

            for (int i = 0; i < 256; i += 16) //each row
            {
                for (int j = 0; j < 256; j += 16) //each box in the row
                {
                    // draw a rectangle
                    using (Graphics g = Graphics.FromImage(temp_bmp3))
                    {
                        temp_brush.Color = Color.FromArgb(Palettes.pal_r[count], Palettes.pal_g[count], Palettes.pal_b[count]);
                        g.FillRectangle(temp_brush, j, i, 16, 16);
                    }
                    count++;
                }
            }


            image_pal = temp_bmp3;
            temp_brush.Dispose();
        } // END DRAW PALETTES



        public void update_palette() 
        {
            
            // which palette square
            int xx = pal_x * 16;
            int yy = pal_y * 16;

            draw_palettes();

            // draw a square on selected box
            for(int i = 0; i < 16; i++)
            {
                image_pal.SetPixel(xx + i, yy, Color.White); //top line
                image_pal.SetPixel(xx, yy + i, Color.White); //left line
                
                image_pal.SetPixel(xx + i, yy + 15, Color.White); //bottom line
                image_pal.SetPixel(xx + 15, yy + i, Color.White); //right line

                if (i == 15) continue;
                image_pal.SetPixel(xx + 14, yy + i, Color.Black); //black right line
                image_pal.SetPixel(xx + i, yy + 14, Color.Black); //black bottom line
            }

            pictureBox3.Image = image_pal;
            pictureBox3.Refresh();
        }



        private void pictureBox3_Click(object sender, EventArgs e)
        { // palettes            

            //int selection;
            var mouseEventArgs = e as MouseEventArgs;
            if (mouseEventArgs != null)
            {
                pal_x = (mouseEventArgs.X & 0xf0) >> 4;
                pal_y = (mouseEventArgs.Y & 0xf0) >> 4;

                if (pal_x < 0) pal_x = 0;
                if (pal_y < 0) pal_y = 0;
                if (pal_x > 15) pal_x = 15;
                if (pal_y > 15) pal_y = 15;

                //selection = pal_x + (pal_y * 16);
                
                update_palette();

                //update the boxes
                rebuild_pal_boxes();
            }

            common_update2();
            label5.Focus();
        }


    }
}
