﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using FileManager.Properties;

namespace FileManager
{
    public partial class FileManager : Form
    {
        private ClsTreeListView clsTreeListView = new ClsTreeListView(); //Generate a ClsTreeListView object
        private string currentAddr;
        private Point mouseLocation; //Use for dragging the form

        private void FMIntialize()
        {
            Theme dark = new Theme(Color.FromArgb(20, 20, 20));
            currentTheme = dark;
            InitializeComponent();
            reloadTheme();
            currentAddr = "C:/";            

        }


        private void reloadTheme()
        {
            //Change Back Color
            foreach (Panel p in this.Controls.OfType<Panel>())
            {

            }
            //Form
            this.BackColor = currentTheme.lighterMain;

            //Navigation
            this.NavigationTablePanel.BackColor = currentTheme.darkerMain;
            foreach (Button b in new Button[] {BtnBack, BtnForward, BtnRecent, BtnParentFolder}) {
                b.BackColor = currentTheme.darkerMain;
                b.FlatAppearance.MouseOverBackColor = currentTheme.lighterMain;
            }
            foreach (TextBox t in new TextBox[] {TxtBxAddress, TxtBxSearch })
            {
                t.BackColor = currentTheme.darkerMain;

                t.ForeColor = currentTheme.text;
            }
            this.TxtBxAddress.BackColor = currentTheme.darkerMain;
            //Display
            this.listView.BackColor = currentTheme.main;
            this.treeView.BackColor = currentTheme.main;
        }


        //Functions For Dragging the Form (Open)
        private void HeaderTablePanel_MouseDown(object sender, MouseEventArgs e)
        {
            mouseLocation = new Point(-e.X, -e.Y);
        }

        private void HeaderTablePanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Point mousePose = Control.MousePosition;
                mousePose.Offset(mouseLocation.X, mouseLocation.Y);
                Location = mousePose;
                if (mouseLocation.Y < 100)
                {

                }
            }
        }
    }
    //Functions For Dragging the Form (Close)

}
