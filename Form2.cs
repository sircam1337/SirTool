using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SirTool
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Click(object sender, EventArgs e)
        {

        }

        private void guna2PictureBox1_Click(object sender, EventArgs e)
        {
            guna2PictureBox1.Visible = false;
            guna2PictureBox2.Visible = true;
        }

        private void guna2PictureBox2_Click(object sender, EventArgs e)
        {
            guna2PictureBox2.Visible = false;
            guna2PictureBox3.Visible = true;
        }

        private void guna2PictureBox3_Click(object sender, EventArgs e)
        {
            guna2PictureBox3.Visible = false;
            guna2PictureBox4.Visible = true;
        }

        private void guna2PictureBox5_Click(object sender, EventArgs e)
        {
            guna2PictureBox5.Visible = false;
            guna2PictureBox1.Visible = true;
        }

        private void guna2PictureBox4_Click(object sender, EventArgs e)
        {
            guna2PictureBox4.Visible = false;
            guna2PictureBox5.Visible = true;
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape) this.Close();
            bool res = base.ProcessCmdKey(ref msg, keyData);
            return res;
        }

        private void Form2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)27)
                this.Hide();
        }

        private void Form2_KeyUp(object sender, KeyEventArgs e)
        {

            if (e.KeyCode == Keys.Escape)
                this.Hide();

        }
    }
}
