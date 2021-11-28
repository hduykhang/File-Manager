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

namespace FileManager
{
    public partial class MainForm : Form
    {
        private ClsTreeListView clsTreeListView = new ClsTreeListView(); //Generate a ClsTreeListView object
        public MainForm()
        {
            FMIntialize();//Just like Initailize Components
        }

        //Create tree view when File Manger loads
        private void FileManager_Load(object sender, EventArgs e)
        {
            this.clsTreeListView.CreateTreeView(treeView);
        }

        //Handle event when a tree node is selected
        private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Name == "This PC")
                clsTreeListView.ShowContent(listView, e.Node);
                clsTreeListView.ShowFolderTree(treeView, listView, e.Node);
        }

        private void MainTablePanel_Paint(object sender, PaintEventArgs e)
        {

        }

        private void listView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            clsTreeListView.ClickItem(this.listView, this.listView.FocusedItem);
        }

        private void listView_KeyPress(object sender, KeyPressEventArgs e)
        {
            if(e.KeyChar ==(int)Keys.Enter)
                clsTreeListView.ClickItem(this.listView, this.listView.FocusedItem);
        }

    }
}
