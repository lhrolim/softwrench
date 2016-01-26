using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Security;

[assembly: AllowPartiallyTrustedCallers]

namespace softWrench.sW4.Controls.WebForms
{
    public interface iSasActiveX
    {
        string binaryData { get; }
        string asciiData { get; }
    }
    public partial class RichTextBox : UserControl, iSasActiveX
    {
        public RichTextBox()
        {
            InitializeComponent();
        }
        public string binaryData { get { return rTxtBox.Rtf; } }
        public string asciiData { get { return rTxtBox.Text; } }

        private void rTxtBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                rTxtBox.ContextMenu = new ContextMenu();
                rTxtBox.ContextMenu.MenuItems.Add(new MenuItem("Cut", new EventHandler(iCut)));
                rTxtBox.ContextMenu.MenuItems.Add(new MenuItem("Copy", new EventHandler(iCopy)));
                rTxtBox.ContextMenu.MenuItems.Add(new MenuItem("Paste", new EventHandler(iPaste)));
            }
        }

        void iCut(object sender, EventArgs e)
        {
            rTxtBox.Cut();
        }
        void iCopy(object sender, EventArgs e)
        {
            rTxtBox.Copy();
        }
        void iPaste(object sender, EventArgs e)
        {
            rTxtBox.Paste();
        }
    }
}
