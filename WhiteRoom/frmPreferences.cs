/*
 * WhiteRoom
 * Copyright (c) Joe DF (2015), Jeffrey Fuller (2010)
 *
 * NOTICE OF LICENSE
 *
 * All project source files are subject to the Open Software License (OSL 3.0)
 * that is included with this applciation in the file LICENSE.txt.
 * The license is also available online at the following URL:
 * http://opensource.org/licenses/osl-3.0.php
 *
 */

using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Configuration;
using System.Xml;
using Microsoft.Win32;

namespace WhiteRoom
{
    public partial class frmPreferences : Form
    {
        private frmMain parent;
        private Boolean modified;

        public frmPreferences()
        {
            InitializeComponent();
        }

        public frmPreferences(frmMain pParent)
        {
            parent = pParent;
            InitializeComponent();
        }

        #region "Helper Functions"

        private void CacheSettings() {
            XmlDocument doc = new XmlDocument();
            doc.Load(Application.StartupPath + "\\WhiteRoom.exe.config");

            XmlNodeList nodes;
            nodes = doc.SelectNodes("//setting");

            foreach (XmlNode node in nodes)
            {
                if (node.Attributes["name"].InnerText == "Contents")
                {
                    // suppress this option
                }
                else if (node.Attributes["name"].InnerText.Contains("Color"))
                {
                    Color tmp = (Color)Properties.Settings.Default[node.Attributes["name"].InnerText];
                    if (tmp.IsNamedColor)
                    {
                        node.ChildNodes[0].InnerText = tmp.Name;
                    }
                    else
                    {
                        node.ChildNodes[0].InnerText = tmp.R + ", " + tmp.G + ", " + tmp.B;
                    }
                }
                else if (node.Attributes["name"].InnerText.Contains("Font"))
                {
                    Font tmp = (Font)Properties.Settings.Default[node.Attributes["name"].InnerText];
                    node.ChildNodes[0].InnerText = tmp.Name + ", " + tmp.SizeInPoints + "pt";
                }
                else
                {
                    node.ChildNodes[0].InnerText = Properties.Settings.Default[node.Attributes["name"].InnerText].ToString(); ;
                }
            }

            try
            {
                doc.Save(Application.StartupPath + "\\WhiteRoom.exe.config");
            }
            catch (UnauthorizedAccessException e)
            {
                MessageBox.Show("Config Cache Error: " + e.Message);
            }
        }

        private bool IsValidNumber(string val)
        {
            bool value = true;
            
            try
            {
                Int32.Parse(val);
            }
            catch
            {
                return false;
            }

            return value;
        }

        #endregion

        #region "Initialization"

        private void Init()
        {
            // was text modified
            modified = parent.getModified();

            // formatting settings
            btnFontColor.BackColor = Properties.Settings.Default.ForegroundColor;
            txtFont.Text = Properties.Settings.Default.Font.Name + ", " + Math.Round(Properties.Settings.Default.Font.SizeInPoints) + "pt";
            txtTabstoSpaces.Text = Properties.Settings.Default.TabToSpaces.ToString();
            chkTabToSpaces.Checked = (Properties.Settings.Default.TabToSpaces != 0);
            txtTabstoSpaces.Enabled = chkTabToSpaces.Checked;
            chkAudoIndent.Checked = Properties.Settings.Default.AutoIndent;

            // page settings
            txtPageWidth.Text = Properties.Settings.Default.PageWidth.ToString();
            chkPageWidth.Checked = (Properties.Settings.Default.PageWidth == 0);
            txtPageHeight.Text = Properties.Settings.Default.PageHeight.ToString();
            chkPageHeight.Checked = (Properties.Settings.Default.PageHeight == 0);
            txtPageMargin.Text = Properties.Settings.Default.PagePadding.ToString();
            txtPageTopOffset.Text = Properties.Settings.Default.PageTopOffset.ToString();
            btnPageColor.BackColor = Properties.Settings.Default.PageColor;
            chkPageShowBorder.Checked = Properties.Settings.Default.ShowPageBorder;

            // general (interface) assignments
            btnBackColor.BackColor = Properties.Settings.Default.BackgroundColor;
            string BackImg = Properties.Settings.Default.BackImage.Trim();
            btnBackImage.Text = (File.Exists(BackImg)) ? BackImg : "none";
            chkBackImage.Checked = Properties.Settings.Default.BackImageEnable;
            btnBackImage.Enabled = Properties.Settings.Default.BackImageEnable;
            trcOpacity.Value = Properties.Settings.Default.Opacity;
            chkMultipleMonitor.Checked = Properties.Settings.Default.MultipleMonitors;
            chkNav.Checked = Properties.Settings.Default.HideNavigation;
            chkNeutralHighlighting.Checked = Properties.Settings.Default.NeutralHighlight;

            // file settings
            chkAutosave.Checked = Properties.Settings.Default.Autosave;
            chkLaunchFullscreen.Checked = Properties.Settings.Default.LaunchFullscreen;

            // data recovery settings
            switch (Properties.Settings.Default.DataRecoveryMode)
            {
                case (int)Eclectic.DataRecoveryModes.BUFFER:
                    rdoLoadBuffer.Checked = true;
                    break;
                case (int)Eclectic.DataRecoveryModes.CLEAN:
                    rdoLoadClean.Checked = true;
                    break;
                case (int)Eclectic.DataRecoveryModes.LAST_FILE:
                    rdoLoadLast.Checked = true;
                    break;
            }

            // advanced settings
            chkRescale.Checked = Properties.Settings.Default.RescaleOnMouseRelease;
            chkDoubleBuffered.Checked = Properties.Settings.Default.DoubleBuffered;
            chkContextMenu.Checked = Properties.Settings.Default.OpenWithContextMenu;
            chkLocalCacheFile.Checked = Properties.Settings.Default.LocalCacheFile;
            txtCursorBlinkTime.Text = Properties.Settings.Default.CaretBlinkRate.ToString();
        }

        #endregion

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            Properties.Settings.Default.Save();
            CacheSettings();

            parent.setModified(modified);

            this.Close();
        }

        private void frmPreferences_Load(object sender, EventArgs e)
        {
            Init();
        }

        private void btnFontColor_Click(object sender, EventArgs e)
        {
            clrPicker.Color = btnFontColor.BackColor;
            if (clrPicker.ShowDialog() != DialogResult.Cancel)
            {
                Properties.Settings.Default.ForegroundColor = clrPicker.Color;
                Properties.Settings.Default.Save();
                btnFontColor.BackColor = clrPicker.Color;
                parent.Sync();
            }
        }

        private void btnFont_Click(object sender, EventArgs e)
        {
            fntPicker.Font = Properties.Settings.Default.Font;
            if (fntPicker.ShowDialog() != DialogResult.Cancel)
            {
                txtFont.Text = fntPicker.Font.Name + ", " + Math.Round(fntPicker.Font.SizeInPoints) + "pt";
                Properties.Settings.Default.Font = fntPicker.Font;
                Properties.Settings.Default.Save();
                parent.Sync();
            }
        }

        private void txtPageWidth_TextChanged(object sender, EventArgs e)
        {
            if (txtPageWidth.Text.Trim() != "" & IsValidNumber(txtPageWidth.Text))
            {
                if ((System.Convert.ToInt32(txtPageWidth.Text) > 100 | System.Convert.ToInt32(txtPageWidth.Text) == 0))
                {
                    Properties.Settings.Default.PageWidth = Int32.Parse(txtPageWidth.Text);
                    Properties.Settings.Default.Save();
                    parent.ReScale();
                }
            }
            else
            {
                MessageBox.Show("Invalid Page Width. You may only use numeric characters.");
            }
        }

        private void chkPageWidth_CheckedChanged(object sender, EventArgs e)
        {
            txtPageWidth.Enabled = !(chkPageWidth.Checked);
            if (chkPageWidth.Checked)
            {
                txtPageWidth.Text = "0";
            }
        }

        private void txtPageHeight_TextChanged(object sender, EventArgs e)
        {
            if (txtPageWidth.Text.Trim() != "" & IsValidNumber(txtPageHeight.Text))
            {
                if ((System.Convert.ToInt32(txtPageHeight.Text) > 100 | System.Convert.ToInt32(txtPageHeight.Text) == 0))
                {
                    Properties.Settings.Default.PageHeight = Int32.Parse(txtPageHeight.Text);
                    Properties.Settings.Default.Save();
                    parent.ReScale();
                }
            }
            else
            {
                MessageBox.Show("Invalid Page Height. You may only use numeric characters.");
            }
        }

        private void chkPageHeight_CheckedChanged(object sender, EventArgs e)
        {
            txtPageHeight.Enabled = !(chkPageHeight.Checked);
            if (chkPageHeight.Checked)
            {
                txtPageHeight.Text = "0";
            }
        }

        private void txtPageMargin_TextChanged(object sender, EventArgs e)
        {
            if (txtPageMargin.Text.Trim() != "" & IsValidNumber(txtPageMargin.Text))
            {
                if (System.Convert.ToInt32(txtPageMargin.Text) > 0 & (System.Convert.ToInt32(txtPageMargin.Text) < 100)) // arbitrarily assign the max to 100 pixels
                {
                    Properties.Settings.Default.PagePadding = Int32.Parse(txtPageMargin.Text);
                    Properties.Settings.Default.Save();
                    parent.ReScale();
                }
                else
                {
                    MessageBox.Show("Invalid Page Margin. You may only use numeric characters totalling less than 100.");
                }
            }
        }

        private void btnPageColor_Click(object sender, EventArgs e)
        {
            clrPicker.Color = btnPageColor.BackColor;
            if (clrPicker.ShowDialog() != DialogResult.Cancel)
            {
                btnPageColor.BackColor = clrPicker.Color;
                Properties.Settings.Default.PageColor = clrPicker.Color;
                Properties.Settings.Default.Save();
                parent.Sync();
            }
        }

        private void btnBackColor_Click(object sender, EventArgs e)
        {
            clrPicker.Color = btnBackColor.BackColor;
            if (clrPicker.ShowDialog() != DialogResult.Cancel)
            {
                btnBackColor.BackColor = clrPicker.Color;
                Properties.Settings.Default.BackgroundColor = clrPicker.Color;
                Properties.Settings.Default.Save();
                parent.Sync();
            }
        }

        private void trcOpacity_Scroll(object sender, EventArgs e)
        {
            Properties.Settings.Default.Opacity = trcOpacity.Value;
            Properties.Settings.Default.Save();

            parent.Sync();
        }

        private void chkNav_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.HideNavigation = chkNav.Checked;
            Properties.Settings.Default.Save();

            parent.Sync();
            parent.ReScale();
        }

        private void chkMultipleMonitor_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.MultipleMonitors = chkMultipleMonitor.Checked;
            Properties.Settings.Default.Save();
        }

        private void rdoLoadBuffer_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoLoadBuffer.Checked)
            {
                Properties.Settings.Default.DataRecoveryMode = (int)Eclectic.DataRecoveryModes.BUFFER;
                Properties.Settings.Default.Save();
            }
        }

        private void rdoLoadLast_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoLoadLast.Checked)
            {
                Properties.Settings.Default.DataRecoveryMode = (int)Eclectic.DataRecoveryModes.LAST_FILE;
                Properties.Settings.Default.Save();
            }
        }

        private void rdoLoadClean_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoLoadClean.Checked)
            {
                Properties.Settings.Default.DataRecoveryMode = (int)Eclectic.DataRecoveryModes.CLEAN;
                Properties.Settings.Default.Save();
            }
        }

        private void chkAutosave_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.Autosave = chkAutosave.Checked;
            Properties.Settings.Default.Save();
        }

        private void chkLaunchFullscreen_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.LaunchFullscreen = chkLaunchFullscreen.Checked;
            Properties.Settings.Default.Save();
        }

        private void chkContextMenu_CheckedChanged(object sender, EventArgs e)
        {
            RegistryKey regKey;
            if (chkContextMenu.Checked)
            {
                regKey = Registry.ClassesRoot.OpenSubKey("*\\Shell", true);
                try
                {
                    regKey.GetValue("");
                }
                catch
                {
                    regKey = Registry.ClassesRoot.OpenSubKey("*\\", true);
                    regKey.CreateSubKey("Shell");
                    regKey.Close();
                    regKey = Registry.ClassesRoot.OpenSubKey("*\\Shell", true);
                }
                regKey.CreateSubKey("Open with WhiteRoom");
                regKey.Close();
                regKey = Registry.ClassesRoot.OpenSubKey("*\\Shell\\Open with WhiteRoom", true);
                regKey.SetValue("", "&Open with WhiteRoom");
                regKey.CreateSubKey("command");
                regKey.Close();
                regKey = Registry.ClassesRoot.OpenSubKey("*\\Shell\\Open with WhiteRoom\\Command", true);
                regKey.SetValue("", Application.StartupPath + "\\WhiteRoom.exe %1");
                regKey.Close();
            }
            else
            {
                regKey = Registry.ClassesRoot.OpenSubKey("*\\Shell", true);
                regKey.DeleteSubKeyTree("Open with WhiteRoom");
                regKey.Close();
            }
            Properties.Settings.Default.OpenWithContextMenu = chkContextMenu.Checked;
            Properties.Settings.Default.Save();
        }

        private void chkLocalCacheFile_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.LocalCacheFile = chkLocalCacheFile.Checked;
            Properties.Settings.Default.Save();
        }

        private void chkTabToSpaces_CheckedChanged(object sender, EventArgs e)
        {
            txtTabstoSpaces.Enabled = chkTabToSpaces.Checked;
            if (chkTabToSpaces.Checked)
            {
                txtTabstoSpaces.Text = (Properties.Settings.Default.TabToSpaces != 0) ? Properties.Settings.Default.TabToSpaces.ToString() : "3";
            }
            else
            {
                txtTabstoSpaces.Text = "0";
            }
        }

        private void txtTabstoSpaces_TextChanged(object sender, EventArgs e)
        {
            if (txtTabstoSpaces.Text.Trim() != "" & IsValidNumber(txtTabstoSpaces.Text))
            {
                Properties.Settings.Default.TabToSpaces = Int32.Parse(txtTabstoSpaces.Text);
                Properties.Settings.Default.Save();
                parent.Sync();
            }
        }

        private void chkAudoIndent_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.AutoIndent = chkAudoIndent.Checked;
            Properties.Settings.Default.Save();
            parent.Sync();
        }

        private void txtCursorBlinkTime_TextChanged(object sender, EventArgs e)
        {
            if (txtCursorBlinkTime.Text.Trim() != "" & IsValidNumber(txtCursorBlinkTime.Text))
            {
                if (System.Convert.ToInt32(txtCursorBlinkTime.Text) >= 0)
                {
                    Properties.Settings.Default.CaretBlinkRate = (uint)Int32.Parse(txtCursorBlinkTime.Text);
                    Properties.Settings.Default.Save();
                }
                else
                {
                    MessageBox.Show("Invalid Page Margin. You may only use numeric characters totalling less than 100.");
                }
            }
        }

        private void chkNeutralHighlighting_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.NeutralHighlight = chkNeutralHighlighting.Checked;
            Properties.Settings.Default.Save();
        }

        private void chkPageShowBorder_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.ShowPageBorder = chkPageShowBorder.Checked;
            Properties.Settings.Default.Save();
            parent.Sync();
        }

        private void btnBackImage_Click(object sender, EventArgs e)
        {
            string currentImagePath = btnBackImage.Text.Trim();
            if (currentImagePath != "" & File.Exists(currentImagePath))
                imgPicker.FileName = currentImagePath;
            if (imgPicker.ShowDialog() != DialogResult.Cancel)
            {
                string newImagePath = imgPicker.FileName.Trim();
                btnBackImage.Text = newImagePath;
                Properties.Settings.Default.BackImage = newImagePath;
                Properties.Settings.Default.Save();
                parent.Sync();
            }
        }

        private void chkRescale_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.RescaleOnMouseRelease = chkRescale.Checked;
            Properties.Settings.Default.Save();
            parent.Sync();
        }

        private void chkBackImage_CheckedChanged(object sender, EventArgs e)
        {
            btnBackImage.Enabled = chkBackImage.Checked;
            Properties.Settings.Default.BackImageEnable = chkBackImage.Checked;
            Properties.Settings.Default.Save();
            parent.Sync();
        }

        private void txtPageTopOffset_TextChanged(object sender, EventArgs e)
        {
            if (txtPageTopOffset.Text.Trim() != "" & IsValidNumber(txtPageTopOffset.Text))
            {
                if (System.Convert.ToInt32(txtPageTopOffset.Text) >= 0 & (System.Convert.ToInt32(txtPageTopOffset.Text) <= 100)) // arbitrarily assign the max to 100 pixels
                {
                    Properties.Settings.Default.PageTopOffset = Int32.Parse(txtPageTopOffset.Text);
                    Properties.Settings.Default.Save();
                    parent.ReScale();
                }
                else
                {
                    MessageBox.Show("Invalid Page Top offset. You may only use numeric characters totalling 100 or less.");
                }
            }
        }

        private void chkDoubleBuffered_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.DoubleBuffered = chkDoubleBuffered.Checked;
            Properties.Settings.Default.Save();
            parent.Sync();
        }
    }
}