﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;

namespace FileManager
{
    internal class ClsTreeListView
    {
        private enum SHGFI
        {
            SmallIcon = 0x00000001,
            LargeIcon = 0x00000000,
            Icon = 0x00000100,
            DisplayName = 0x00000200,
            Typename = 0x00000400,
            SysIconIndex = 0x00004000,
            UseFileAttributes = 0x00000010
        }

        [DllImport("Shell32.dll")]
        private static extern IntPtr SHGetFileInfo
        (
            string pszPath,
            uint dwFileAttributes,
            out SHFILEINFO psfi,
            uint cbfileInfo,
            SHGFI uFlags
        );


        [StructLayout(LayoutKind.Sequential)]
        private struct SHFILEINFO
        {
            public SHFILEINFO(bool b)
            {
                hIcon = IntPtr.Zero; iIcon = 0; dwAttributes = 0; szDisplayName = ""; szTypeName = "";
            }
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.LPStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.LPStr, SizeConst = 80)]
            public string szTypeName;
        };

        public static Icon GetDirectoryIcon(string dirName, bool largeIcon)
        {
            SHFILEINFO _SHFILEINFO = new SHFILEINFO();
            int cbFileInfo = Marshal.SizeOf(_SHFILEINFO);
            SHGFI flags = SHGFI.Icon;
            if (largeIcon)
                flags |= SHGFI.LargeIcon;
            else
                flags |= SHGFI.SmallIcon;

            IntPtr IconIntPtr = SHGetFileInfo(dirName, 0, out _SHFILEINFO, (uint)cbFileInfo, flags);
            if (IconIntPtr.Equals(IntPtr.Zero))
                return null;
            Icon _Icon = System.Drawing.Icon.FromHandle(_SHFILEINFO.hIcon);
            return _Icon;
        }

        #region Create Tree View
        public void CreateTreeView(TreeView treeView)
        {
            TreeNode ThisPC;
            TreeNode Tag;
            //Create ThisPC node
            ThisPC = new TreeNode("This PC");
            ThisPC.Name = "This PC";

            //Create Favorite node
            Tag = new TreeNode("Tag");
            Tag.Name = "Tag";

            //Delete treeView
            treeView.Nodes.Clear();

            Icon icon;

            //Add Tag node                       PENDING...
            icon = Properties.Resources.Computer;
            Tag.Tag = new CustomTreeView.TreeNodeTag(icon, false);
            treeView.Nodes.Add(Tag);

            //Add ThisPC node
            icon = Properties.Resources.Computer;
            ThisPC.Tag = new CustomTreeView.TreeNodeTag(icon, true);
            treeView.Nodes.Add(ThisPC);          

            //Get list of disk in ThisPC
            ManagementObjectSearcher query = new ManagementObjectSearcher("SELECT * FROM Win32_LogicalDisk");
            ManagementObjectCollection collectionQuery = query.Get();

            foreach (ManagementObject item in collectionQuery)
            {
                //Create tree node for each disk
                TreeNode diskTreeNode = new TreeNode(item["Name"].ToString() + "\\");

                //Add tree node's tag
                diskTreeNode.Tag = GetTreeNodeTag(diskTreeNode.Text);
                ThisPC.Nodes.Add(diskTreeNode);
            }
        }

        public CustomTreeView.TreeNodeTag GetTreeNodeTag(string path)
        {
            Icon icon = GetDirectoryIcon(path, false);
            bool hasChild = false;
            if(new DirectoryInfo(path).GetDirectories().Length!=0)
                hasChild = true;
            return  new CustomTreeView.TreeNodeTag(icon, hasChild);
        }
        #endregion

        #region Show Folder Tree
        public bool ShowFolderTree(TreeView treeView, ListView listView, TreeNode currentNode)
        {
            if (currentNode.Name != "This PC")
            {
                try
                {
                    //Check the path
                    if (!Directory.Exists(GetFullPath(currentNode.FullPath)))
                    {
                        MessageBox.Show("'" + GetFullPath(currentNode.FullPath) + "' doesn't exist.", "File Manager", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }

                    //Handle display folder
                    else
                    {
                        string[] strDirectories = Directory.GetDirectories(GetFullPath(currentNode.FullPath));
                        foreach (string strDirectory in strDirectories)
                        {
                            //Check if the dir is unauthorized or not
                            if (CheckUnauthoried(strDirectory)) continue;
                            string strName = GetName(strDirectory);
                            TreeNode dirNode = new TreeNode(strName);
                            dirNode.Tag = GetTreeNodeTag(strDirectory);
                            currentNode.Nodes.Add(dirNode);
                         }
                        currentNode.Checked = true;

                        ShowContent(listView, currentNode);
                    }
                    return true;
                }
                catch (IOException)
                {
                    MessageBox.Show("'" + GetFullPath(currentNode.FullPath) + "' doesn't exist.", "File Manager", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                catch (UnauthorizedAccessException)
                {
                    MessageBox.Show("You don't currently have permission to access this folder.", "Administrator", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
            return false;
        }

        //Check if the dir is unauthorized or not
        public bool CheckUnauthoried(string strPath)
        {
            try { new DirectoryInfo(strPath).GetDirectories(); }
            catch (UnauthorizedAccessException) { return true; }
            return false;
        }

        //Return the full path of a tree node after handling
        static public string GetFullPath(string strPath)
        {
            return strPath.Replace(@"This PC\", "").Replace(@"\\", @"\");
        }

        //Get the name of a dirctory from its full path
        public string GetName(string strPath)
        {
            string[] strPlit = strPath.Split('\\');
            int maxIndex = strPlit.Length;
            return strPlit[maxIndex - 1];
        }
        #endregion

        #region Show Content
        //Show content of directory selected in Tree View
        public bool ShowContent(ListView listView, TreeNode currentNode)
        {
            try
            {
                if (currentNode.Name != "This PC")
                {  //Delete all items in List View
                    listView.Items.Clear();

                    DirectoryInfo directoryInfo = new DirectoryInfo(GetFullPath(currentNode.FullPath));

                    //Information of directories
                    foreach (DirectoryInfo dir in directoryInfo.GetDirectories())
                        listView.Items.Add(GetLVItems(dir));

                    //Information of files
                    foreach (FileInfo file in directoryInfo.GetFiles())
                        listView.Items.Add(GetLVItems(file, listView));
                }
                else
                {
                    listView.Items.Clear();
                    foreach (TreeNode node in currentNode.Nodes)
                    {
                        string[] item = new string[5];
                        item[0] = node.Text;
                        switch (node.ImageIndex)
                        {
                            case 1:
                                item[2] = "Removable Disk";
                                break;
                            case 2:
                                item[2] = "Local Disk";
                                break;
                            case 3:
                                item[2] = "Network Disk";
                                break;
                            case 4:
                                item[2] = "CD Disk";
                                break;
                            default:
                                item[2] = "File folder";
                                break;
                        }
                        item[4] = node.Text;
                        ListViewItem listViewItem = new ListViewItem(item, node.ImageIndex - 1);
                        listView.Items.Add(listViewItem);
                    }
                }
                return true;
            }
            catch (IOException)
            {
                MessageBox.Show("'" + GetFullPath(currentNode.FullPath) + "' doesn't exist.", "File Manager", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("You don't currently have permission to access this folder.", "Administrator", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            return false;
        }
    

        //Get List View item having info from folder
        public ListViewItem GetLVItems(DirectoryInfo folder)
        {
            string[] item = new string[5];
            item[0] = folder.Name;
            item[1] = folder.LastWriteTime.ToString();
            item[2] = "File folder";
            item[4] = folder.FullName;
            ListViewItem listViewItem = new ListViewItem(item);
            listViewItem.ImageIndex = 4;
            return listViewItem;
        }

        //Get List View item having info from file
        public ListViewItem GetLVItems(FileInfo file, ListView listView)
        {
            string[] item = new string[5];
            item[0] = file.Name;
            item[1] = file.LastWriteTime.ToString();
            item[2] = (file.Extension.Equals("")) ? "File" : file.Extension;
            item[3] = Math.Ceiling(file.Length / (1024 * 1.0)).ToString("###,###") + " KB";
            item[4] = file.FullName;

            //Set a default icon for the file
            Icon iconForFile = SystemIcons.WinLogo;
            ListViewItem listViewItem = new ListViewItem(item, 1);


            string key = file.Extension;
            if (key == ".exe")
                key = file.FullName;
            else if (key == "")
            {
                listViewItem.ImageIndex = 5;
                return listViewItem;
            }

            //Check to see if the image collection contains an image for this extension, using the extension as a key
            if (!listView.SmallImageList.Images.ContainsKey(file.Extension))
            {
                //If not, add the image to the image list
                iconForFile = Icon.ExtractAssociatedIcon(file.FullName);
                listView.SmallImageList.Images.Add(key, iconForFile);
                listView.LargeImageList.Images.Add(key, iconForFile);
            }

            listViewItem.ImageKey = key;
            return listViewItem;
        }

        public void ShowContent(ListView listView, string path)
        {
            try
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(path);
                if (directoryInfo.Exists)
                {
                    listView.Items.Clear();

                    //Information of directories
                    foreach (DirectoryInfo dir in directoryInfo.GetDirectories())
                        listView.Items.Add(GetLVItems(dir));

                    //Information of files
                    foreach (FileInfo file in directoryInfo.GetFiles())
                        listView.Items.Add(GetLVItems(file, listView));
                }
                else
                    MessageBox.Show("Can't find '" + path + "'. Check the spelling and try again", "File Manager", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (IOException)
            {
                MessageBox.Show("Can't find '" + path + "'. Check the spelling and try again", "File Manager", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("You don't currently have permission to access this folder.", "Administrator", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        #endregion


        public bool ClickItem(ListView listView, ListViewItem item)
        {
            try
            {
                string path = item.SubItems[4].Text;
                FileInfo file = new FileInfo(path);
                if (file.Exists)
                {
                    Process.Start(path);
                }
                else
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(path);
                    if (!directoryInfo.Exists)
                    {
                        MessageBox.Show("'" + path + "' doesn't exist.", "File Manager", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;

                    }

                    listView.Items.Clear();
                    foreach (DirectoryInfo directoryInfoTemp in directoryInfo.GetDirectories())
                        listView.Items.Add(GetLVItems(directoryInfoTemp));

                    foreach (FileInfo fileInfoTemp in directoryInfo.GetFiles())
                        listView.Items.Add(GetLVItems(fileInfoTemp, listView));
                }
                return true;
            }
            catch (IOException)
            {
                MessageBox.Show("'" + GetFullPath(item.SubItems[4].Text) + "' doesn't exist.", "File Manager", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("You don't currently have permission to access this folder.", "Administrator", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            return false;
        }
    }
}


