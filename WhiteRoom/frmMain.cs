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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Printing;
using System.Text.RegularExpressions;

namespace WhiteRoom
{
    public partial class frmMain : Form
    {
        public string FileName = "";
        public int ScrollAction;
        private Form[] f = new Form[Screen.AllScreens.Length];
        private RichTextBoxFinds FindOptions;
        private string SearchString = "";
        private Int32 intCurrentChar = 0;

        private int highlight;
        private int highlightText;
        private uint blinkRate;
        private FormWindowState lastWindowState;
        private Size lastWindowSize;

        public frmMain()
        {
            InitializeComponent();
            ToolStripManager.Renderer = new CustomMenuRenderer();
            //mnuMenuStrip.Renderer = new CustomMenuRenderer();
        }

        #region "Helper Functions"

        private int getWin32Color(Color color, int shift)
        {
            int R = 0; int G = 0; int B = 0;

            if (shift != 0)
            {
                R = color.R + shift;
                if (R < 0) { R = 0; } else if (R > 255) { R = 255; }

                G = color.G + shift;
                if (G < 0) { G = 0; } else if (G > 255) { G = 255; }

                B = color.B + shift;
                if (B < 0) { B = 0; } else if (B > 255) { B = 255; }
            }

            return System.Drawing.ColorTranslator.ToWin32(Color.FromArgb(R, G, B));
        }

        public Boolean getModified()
        {
            return txtPage.Modified;
        }

        public void setModified(Boolean modified)
        {
            txtPage.Modified = modified;
        }

        private void Cut()
        {
            txtPage.Cut();
            try
            {
                if (Clipboard.GetDataObject().GetData(DataFormats.UnicodeText) != null)
                {
                    Clipboard.SetDataObject(Clipboard.GetDataObject().GetData(DataFormats.UnicodeText));
                }
            }
            catch(Exception e)
            {
                MessageBox.Show("An error occurred while cutting text: " + e.Message, "WhiteRoom - Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void Copy()
        {
            txtPage.Copy();
            try
            {
                if (Clipboard.GetDataObject().GetData(DataFormats.UnicodeText) != null)
                {
                    Clipboard.SetDataObject(Clipboard.GetDataObject().GetData(DataFormats.UnicodeText));
                }
            }
            catch(Exception e)
            {
                MessageBox.Show("An error occurred while copying text: " + e.Message, "WhiteRoom - Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void Paste()
        {
            try
            {
                if (Clipboard.GetDataObject().GetData(DataFormats.UnicodeText) != null)
                {
                    Clipboard.SetDataObject(Clipboard.GetDataObject().GetData(DataFormats.UnicodeText));
                }

                txtPage.SelectedText = Clipboard.GetDataObject().GetData(DataFormats.UnicodeText).ToString();
            }
            catch(Exception e)
            {
                MessageBox.Show("An error occurred while pasting content: " + e.Message, "WhiteRoom - Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
 
            //txtPage.Paste(); //(DataFormats.GetFormat(DataFormats.UnicodeText));
        }

        public int Statistics(int stat)
        {
            int statistic = 0;

            switch (stat)
            {
                case 0: // word count
                    statistic = new Regex(@"\s+").Replace(txtPage.Text.Trim()," ").Split(' ').Length;
                    break;
                case 1: // characters - no spaces
                    statistic = txtPage.Text.Replace(" ", "").Length;
                    break;
                case 2: // charactesr - w/ spaces
                    statistic = txtPage.Text.Length;
                    break;
                case 3: // lines
                    statistic = txtPage.Lines.Length;
                    break;
            }

            return statistic;
        }

        public string StatsText()
        {
            return Statistics(0).ToString() + " words | " + DateTime.Now.ToShortTimeString().ToString();
        }

        public void Find(string pSearchString, RichTextBoxFinds pFindOptions)
        {
            SearchString = pSearchString;
            FindOptions = pFindOptions;
            Find();
        }
        
        public void Find()
        {
            int pos;
            pos = txtPage.Find(SearchString, txtPage.SelectionStart + txtPage.SelectionLength, FindOptions);
            if (pos == -1 & txtPage.SelectionStart > 0)
            {
                if (MessageBox.Show("You have reached the end of the document, start search at the beginning of the document?", "WriteRoom - Find: End of Document", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    txtPage.SelectionStart = 0;
                    Find();
                }
            }
            else if (pos == -1)
            {
                MessageBox.Show("Search string not found.","WriteRoom - Search");
            }
            else
            {
                txtPage.SelectionStart = pos;
            }
        }

        public void UpdateEditMenu()
        {
            undoToolStripMenuItem.Enabled = txtPage.CanUndo;
            redoToolStripMenuItem.Enabled = txtPage.CanRedo;
        }

        private void ToggleState()
        {
            //avoid double call to ReScale();
            DisableResizeEvents();

            //save current window state
            FormWindowState currentWindowState = this.WindowState;

            //bugfix for : if window was manually maximized beforehand by user
            if (this.WindowState == FormWindowState.Maximized && this.FormBorderStyle != System.Windows.Forms.FormBorderStyle.None)
            {
                this.WindowState = FormWindowState.Normal;
            }

            if (this.FormBorderStyle == System.Windows.Forms.FormBorderStyle.None)
            {
                mnuMenuStrip.Show();
                pnlPage.Top = mnuMenuStrip.Height;
                this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
                this.WindowState = this.lastWindowState;
                if (Properties.Settings.Default.MultipleMonitors)
                {
                    HandleAdditionalScreens(0);
                }
            }
            else
            {
                mnuMenuStrip.Hide();
                pnlPage.Top = 0;
                this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                this.lastWindowState = currentWindowState;
                this.WindowState = FormWindowState.Maximized;
                if (Properties.Settings.Default.MultipleMonitors)
                {
                    HandleAdditionalScreens(1);
                }
            }
            ReScale();
            txtPage.Focus();

            //Re-enable Resize events;
            SetResizeEvents();
        }

        public void HandleAdditionalScreens(int mode)
        {
            Screen[] s = Screen.AllScreens;
            for (int i = 0; i < Screen.AllScreens.Length; i++)
            {
                if (f[i] != null)
                {
                    f[i].Visible = false;
                }
                if (!((this.Left >= s[i].Bounds.X & this.Left < (s[i].Bounds.X + s[i].Bounds.Width))) | !((this.Top >= s[i].Bounds.Y & this.Top < (s[i].Bounds.Y + s[i].Bounds.Height))))
                {
                    if (f[i] == null)
                    {
                        f[i] = new Form();
                        f[i].FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                        f[i].BackColor = Properties.Settings.Default.BackgroundColor;
                        f[i].Show();
                        f[i].Bounds = s[i].Bounds;
                        f[i].WindowState = FormWindowState.Maximized;
                        f[i].Opacity = Properties.Settings.Default.Opacity / 100.0;
                    }
                    else
                    {
                        if (mode == 1)
                        {
                            f[i].BackColor = Properties.Settings.Default.BackgroundColor;
                            f[i].Visible = true;
                            f[i].Opacity = Properties.Settings.Default.Opacity / 100.0;
                        }
                        else
                        {
                            f[i].Visible = false;
                        }
                    }
                }
            }
        }

        private DialogResult ConfirmOverwrite()
        {
            return MessageBox.Show("Your document has changed since the last save. If you do not save now, all progress will be lost. \n\nWould you like to save?", "WriteRoom - Warning", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
        }

        #endregion

        #region "Sync Functions" and "Styling Misc"

        public void Sync()
        {
            //Better minimum height, based on pnlNav height
            int delta = this.Height - this.ClientRectangle.Height;
            this.MinimumSize = new Size(this.MinimumSize.Width, delta + pnlNav.Height + mnuMenuStrip.Height + Properties.Settings.Default.PageTopOffset);

            //DoubleBuffered window
            this.DoubleBuffered = Properties.Settings.Default.DoubleBuffered;

            this.BackColor = Properties.Settings.Default.BackgroundColor;
            if (Properties.Settings.Default.BackImageEnable)
            {
                string BackImg = Properties.Settings.Default.BackImage.Trim();
                this.BackgroundImage = (File.Exists(BackImg)) ? new Bitmap(BackImg) : null;
            }
            else
            {
                this.BackgroundImage = null;
            }
            this.Opacity = (float)(Properties.Settings.Default.Opacity / 100.0);
            pnlPage.BackColor = Properties.Settings.Default.PageColor;
            pnlPage.BorderStyle = (Properties.Settings.Default.ShowPageBorder) ? BorderStyle.FixedSingle : BorderStyle.None;
            txtPage.BackColor = Properties.Settings.Default.PageColor;
            txtPage.Font = new Font(Properties.Settings.Default.Font.FontFamily, Properties.Settings.Default.Font.Size, txtPage.Font.Style);
            txtPage.ForeColor = Properties.Settings.Default.ForegroundColor;

            pnlNav.Visible = !(Properties.Settings.Default.HideNavigation);
            pnlNav.BackColor = Properties.Settings.Default.PageColor;

            btnPageUp.ForeColor = Properties.Settings.Default.ForegroundColor;
            btnLineUp.ForeColor = Properties.Settings.Default.ForegroundColor;
            btnLineDown.ForeColor = Properties.Settings.Default.ForegroundColor;
            btnPageDown.ForeColor = Properties.Settings.Default.ForegroundColor;

            mnuMenuStrip.ForeColor = Properties.Settings.Default.ForegroundColor;
            mnuMenuStrip.BackColor = Properties.Settings.Default.PageColor;

            txtPage.TabsToSpaces = Properties.Settings.Default.TabToSpaces;
            txtPage.AutoIndent = Properties.Settings.Default.AutoIndent;

            ScrollWin9xProgressBar.ForeColor = Properties.Settings.Default.ForegroundColor;
            ScrollWin9xProgressBar.BackColor = Properties.Settings.Default.PageColor;
            
            updateScrollPercent();

            if (Properties.Settings.Default.MultipleMonitors)
            {
                HandleAdditionalScreens((this.WindowState == FormWindowState.Normal) ? 0 : 1);
            }
            SetResizeEvents();
        }

        private class CustomMenuRenderer : ToolStripProfessionalRenderer
        {
            public CustomMenuRenderer() : base(new CustomMenuColors()) { }

            // http://stackoverflow.com/a/32313929/883015
            protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
            {
                // Here set e.TextFont, e.TextColor and so on, For example:
                if (e.Item.Selected)
                {
                    if ( (e.Item.OwnerItem != null) || (e.Item.Owner.Name == "contextMenuPage") )
                    {
                        e.TextColor = Properties.Settings.Default.PageColor;
                        //e.TextFont = new Font(e.Item.Font, FontStyle.Italic | FontStyle.Bold);
                    }
                }
                else {
                    e.TextColor = Properties.Settings.Default.ForegroundColor;
                }
                base.OnRenderItemText(e);
            }
            
            //// https://msdn.microsoft.com/en-us/library/system.windows.forms.toolstripprofessionalrenderer.aspx
            //protected override void OnRenderItemBackground(ToolStripItemRenderEventArgs e) {
            //    e.Item.BackColor = Properties.Settings.Default.PageColor;
            //}
        }

        private class CustomMenuColors : ProfessionalColorTable
        {
            // http://stackoverflow.com/a/9260827/883015
            Color ForeColor = Properties.Settings.Default.ForegroundColor;
            Color BackColor = Properties.Settings.Default.PageColor;
            
            public override Color MenuItemSelectedGradientBegin { get { return this.BackColor; } }
            public override Color MenuItemSelectedGradientEnd { get { return this.BackColor; } }
            public override Color MenuItemPressedGradientBegin { get { return this.BackColor; } }
            public override Color MenuItemPressedGradientEnd { get { return this.BackColor; } }
            public override Color ToolStripDropDownBackground { get { return this.BackColor; } }

            public override Color MenuBorder { get { return this.ForeColor; } }
            public override Color MenuItemSelected { get { return this.ForeColor; } }
            public override Color MenuItemBorder { get { return this.ForeColor; } }
            public override Color SeparatorDark { get { return this.ForeColor; } }
            public override Color SeparatorLight { get { return this.ForeColor; } }

            /* The following seem to have no effect
            public override Color ToolStripBorder { get { return Color.Orange; } }
            public override Color ToolStripContentPanelGradientBegin { get { return Color.Orange; } }
            public override Color ToolStripContentPanelGradientEnd { get { return Color.Orange; } }
            public override Color MenuStripGradientBegin { get { return Color.Orange; } }
            public override Color MenuStripGradientEnd { get { return Color.Orange; } }
            public override Color ToolStripGradientBegin { get { return Color.Orange; } }
            public override Color ToolStripGradientEnd { get { return Color.Orange; } }
            public override Color ToolStripPanelGradientBegin { get { return Color.Orange; } }
            public override Color ToolStripPanelGradientEnd { get { return Color.Orange; } }
            */
        }

        public void SetRescaleOnMouseRelease(bool enableIt)
        {
            DisableResizeEvents();

            if (enableIt)
            {
                this.Resize += frmMain_ResizeMinMax;
                this.ResizeBegin += frmMain_ResizeBegin;
                this.ResizeEnd += frmMain_ResizeSpecial;
            }
            else
            {
                this.Resize += frmMain_Resize;
            }
        }

        public void DisableResizeEvents()
        {
            this.Resize -= frmMain_Resize;
            this.Resize -= frmMain_ResizeMinMax;
            this.ResizeBegin -= frmMain_ResizeBegin;
            this.ResizeEnd -= frmMain_Resize;
        }

        public void SetResizeEvents()
        {
            SetRescaleOnMouseRelease(Properties.Settings.Default.RescaleOnMouseRelease);
        }

        public void ReScale()
        {
            if (Properties.Settings.Default.PageWidth > 0)
            {
                pnlPage.Width = Properties.Settings.Default.PageWidth;
            }
            else
            {
                pnlPage.Width = this.ClientSize.Width;
            }
            if (pnlPage.Width > this.ClientSize.Width)
            {
                pnlPage.Width = this.ClientSize.Width;
            }
            if (pnlNav.Visible)
            {
                pnlPage.Width -= pnlNav.Width;
            }

            pnlPage.Left = (this.ClientSize.Width - (pnlPage.Width + (pnlNav.Visible ? pnlNav.Width : 0))) / 2;

            if (Properties.Settings.Default.PageHeight > 0)
            {
                pnlPage.Height = Properties.Settings.Default.PageHeight;
            }
            else
            {
                pnlPage.Height = this.ClientSize.Height;
            }
            if (pnlPage.Height > this.ClientSize.Height)
            {
                pnlPage.Height = this.ClientSize.Height;
            }

            if (mnuMenuStrip.Visible)
            {
                pnlPage.Top = mnuMenuStrip.Height;
                pnlPage.Height -= mnuMenuStrip.Height;
            }
            else
            {
                pnlPage.Top = 0;
            }

            if (Properties.Settings.Default.PageTopOffset > 0)
            {
                pnlPage.Top += Properties.Settings.Default.PageTopOffset;
                pnlPage.Height -= Properties.Settings.Default.PageTopOffset;
            }
            txtPage.Width = pnlPage.Width - (Properties.Settings.Default.PagePadding * 2);
            txtPage.Top = Properties.Settings.Default.PagePadding;
            txtPage.Left = Properties.Settings.Default.PagePadding;
            //change the '2' for a '1' in order to remove the bottom margin
            txtPage.Height = pnlPage.Height - (Properties.Settings.Default.PagePadding * 2);
            pnlNav.Top = pnlPage.Top;
            pnlNav.Left = pnlPage.Left + pnlPage.Width;

            updateScrollPercent();
        }

        #endregion

        #region "File Handling"

        private void OpenFile()
        {
            dlgOpen.FileName = "";
            if (dlgOpen.ShowDialog(this) != DialogResult.Cancel)
            {
                FileName = dlgOpen.FileName;
                OpenFile(FileName);
            }
        }

        private void OpenFile(string file)
        {
            FileName = file;
            this.Text = Path.GetFileName(FileName) + " - WhiteRoom";
            try
            {
                if (txtPage.FormattingEnabled && Path.GetExtension(FileName) == ".rtf")
                {
                    txtPage.LoadFile(FileName, RichTextBoxStreamType.RichText);
                }
                else
                {
                    txtPage.LoadFile(FileName, RichTextBoxStreamType.PlainText);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            Sync();
            txtPage.Modified = false;
        }

        private string RecallCache()
        {
            if (Properties.Settings.Default.LocalCacheFile)
            {
                return RecallCacheFile();
            }
            else
            {
                return Properties.Settings.Default.Content;
            }
        }

        private string RecallCacheFile()
        {
            string cache = "";
            try
            {
                string fName = Application.StartupPath + "\\CachedContent.dat";
                txtPage.LoadFile(fName);
                cache = txtPage.Text;

                txtPage.Modified = false;
            }
            catch (FileNotFoundException ex)
            {
                if (Eclectic.DebugMode)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            return cache;
        }

        private bool CacheFile()
        {
            if (Properties.Settings.Default.LocalCacheFile)
            {
                SaveFile(Application.StartupPath + "\\CachedContent.dat");
                return true;
            }
            return false;
        }

        private bool SaveFile(string fName)
        {
            if (txtPage.FormattingEnabled && Path.GetExtension(fName) == ".rtf")
            {
                txtPage.SaveFile(fName, RichTextBoxStreamType.RichText);
            }
            else
            {
                txtPage.SaveFile(fName, RichTextBoxStreamType.PlainText);
            }

            this.Text = Path.GetFileName(fName) + " - WhiteRoom";
            
            txtPage.Modified = false;

            return true;
        }

        private bool SaveFile()
        {
            if (FileName == "")
            {
                dlgSave.FileName = "untitled." + ((txtPage.FormattingEnabled) ? "rtf" : "txt");
                if (dlgSave.ShowDialog(this) != DialogResult.Cancel)
                {
                    FileName = dlgSave.FileName;
                }
                else
                {
                    return false;
                }
            }

            SaveFile(FileName);

            return true;
        }

        #endregion

        #region "Initialization"

        private void Init()
        {
            Int32 lStyle;
            lStyle = (int) Eclectic.TextMode.TM_RICHTEXT | (int) Eclectic.TextMode.TM_MULTILEVELUNDO | (int) Eclectic.TextMode.TM_MULTICODEPAGE;
            Eclectic.SendMessage(txtPage.Handle, Eclectic.EM_SETTEXTMODE, lStyle, 0);

            Sync();

            switch (Properties.Settings.Default.DataRecoveryMode)
            {
                case (int) Eclectic.DataRecoveryModes.BUFFER:
                    txtPage.Text = RecallCache();
                    break;
                case (int) Eclectic.DataRecoveryModes.LAST_FILE:
                    OpenFile(Properties.Settings.Default.LastFileName);
                    break;
            }

            txtPage.Select(0, 0);
            txtPage.ContextMenuStrip = contextMenuPage;
            txtPage.DetectUrls = txtPage.FormattingEnabled;
            txtPage.AutoIndent = Properties.Settings.Default.AutoIndent;
            txtPage.TabsToSpaces = Properties.Settings.Default.TabToSpaces;
            txtPage.ClearUndo();
            txtPage.Modified = false;

            dlgOpen.Filter = "Text Documents (*.txt)|*.txt|All files (*.*)|*.*";
            dlgSave.Filter = "Text Documents (*.txt)|*.txt|All files (*.*)|*.*";

            if (txtPage.FormattingEnabled)
            {
                dlgOpen.Filter = "Rich Text Format (*.rtf)|*.rtf|" + dlgOpen.Filter;
                dlgSave.Filter = "Rich Text Format (*.rtf)|*.rft|" + dlgSave.Filter;
            }
            
            dlgSave.AddExtension = true;
            preferencesToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+,";
            statisticsToolStripMenuItem.ShortcutKeyDisplayString = "Ctrl+/";

            ReScale();
        }

        #endregion

        private void frmMain_Load(object sender, EventArgs e)
        {
            Init();
            if (Properties.Settings.Default.LaunchFullscreen)
            {
                ToggleState();
            }
            tmrAutosave.Start();
            if ((Environment.GetCommandLineArgs()).Length > 1)
            {
                string arg = (Environment.GetCommandLineArgs())[1];
                string filename = arg.Replace("\"", "");
                OpenFile(filename);
            }
        }

        private void frmMain_ResizeMinMax(object sender, EventArgs e)
        {
            if (this.WindowState != this.lastWindowState)
            {
                this.lastWindowState = this.WindowState;
                ReScale();
            }
        }

        private void frmMain_ResizeSpecial(object sender, EventArgs e)
        {
            ResumeLayout();
            if (MouseButtons == MouseButtons.Left)
                return;
            if (this.lastWindowSize == this.Size)
                return;
            this.lastWindowSize = this.Size;
            ReScale();
        }

        private void frmMain_Resize(object sender, EventArgs e)
        {
            ReScale();
        }

        private void frmMain_ResizeBegin(object sender, EventArgs e)
        {
            //see http://stackoverflow.com/a/7759951/883015
            SuspendLayout();
        }

        private void frmMain_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Escape)
            {
                ToggleState();
            }
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult UserResponse = new DialogResult();
            if (txtPage.Modified)
            {
                UserResponse = MessageBox.Show("Your document has changed since the last save. \n\n Would you like to save?", "WriteRoom - Warning", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            }
            if (txtPage.Modified & UserResponse == DialogResult.Cancel)
            {
                e.Cancel = true;
            }
            else if (txtPage.Modified & UserResponse == DialogResult.Yes)
            {
                if (!(SaveFile()))
                {
                    e.Cancel = true;
                }
            }
            Properties.Settings.Default.Content = txtPage.Text;
            if (FileName != "")
            {
                Properties.Settings.Default.LastFileName = FileName;
            }
            Properties.Settings.Default.Save();
            CacheFile();
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult UserResponse = new DialogResult();
            if (txtPage.Modified)
            {
                UserResponse = ConfirmOverwrite();
            }
            if (txtPage.Modified & UserResponse == DialogResult.Cancel)
            {
                return;
            }
            else if (txtPage.Modified & UserResponse == DialogResult.Yes)
            {
                SaveFile();
            }
            txtPage.Clear();
            FileName = "";
            this.Text = "untitled - WhiteRoom";
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult UserResponse = new DialogResult();
            if (txtPage.Modified)
            {
                UserResponse = ConfirmOverwrite();
            }
            if (txtPage.Modified & UserResponse == DialogResult.Cancel)
            {
                return;
            }
            else if (txtPage.Modified & UserResponse == DialogResult.Yes)
            {
                SaveFile();
            }
            OpenFile();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFile();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string temp = FileName;
            FileName = "";
            if (!(SaveFile()))
            {
                FileName = temp;
            }
        }

        private void pageSetupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (PrinterSettings.InstalledPrinters.Count > 0)
            {
                if (FileName != "")
                {
                    prtDoc.DocumentName = FileName;
                }
                dlgPageSetup.Document = prtDoc;
                dlgPageSetup.ShowDialog();
            }
            else
            {
                MessageBox.Show("No printers were detected on this machine.", "WriteRoom - Error: No Printer", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FileName != "")
            {
                prtDoc.DocumentName = FileName;
            }
            dlgPrint.Document = prtDoc;
            dlgPrint.AllowPrintToFile = true;
            if (dlgPrint.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    prtDoc.Print();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "WriteRoom - Print Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (txtPage.CanUndo)
            {
                txtPage.Undo();
            }
            UpdateEditMenu();
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (txtPage.CanRedo)
            {
                txtPage.Redo();
            }
            UpdateEditMenu();
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Cut();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Copy();
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Paste();
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult UserResponse = new DialogResult();
            if (txtPage.Modified)
            {
                UserResponse = MessageBox.Show("Would you like to clear the contents of your documents? This action cannot be undone.", "WriteRoom - Warning", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            }
            if (txtPage.Modified & UserResponse == DialogResult.Cancel)
            {
                return;
            }
            else if (txtPage.Modified & UserResponse == DialogResult.Yes)
            {
                txtPage.Clear();
            }
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            txtPage.SelectAll();
        }

        private void findToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmFindReplace find = new frmFindReplace(this);
            find.ShowDialog();
        }

        private void findNextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Find();
        }

        private void preferencesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmPreferences preferences = new frmPreferences(this);
            preferences.ShowDialog();
        }

        private void fullscreenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToggleState();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmAbout about = new frmAbout();
            about.ShowDialog();
        }

        private void undoToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            txtPage.Undo();
        }

        private void redoToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            txtPage.Redo();
        }

        private void cutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Cut();
        }

        private void copyToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Copy();
        }

        private void pasteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Paste();
        }

        private void selectAllToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            txtPage.SelectAll();
        }

        private void preferencesToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            frmPreferences preferences = new frmPreferences(this);
            preferences.ShowDialog();
        }

        private void tmrAutosave_Tick(object sender, EventArgs e)
        {
            Properties.Settings.Default.Content = txtPage.Text;
            Properties.Settings.Default.Save();
            CacheFile();
            if (Properties.Settings.Default.Autosave & FileName != "")
            {
                SaveFile();
            }
            //lblStats.Text = StatsText();
        }

        private void txtPage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Shift & e.KeyCode == Keys.Insert)
            {
                txtPage.Paste(DataFormats.GetFormat(DataFormats.UnicodeText));
                e.Handled = true;
            }
        }

        private void txtPage_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Escape)
            {
                ToggleState();
            }
        }

        private void txtPage_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0)
            {
                Eclectic.SendMessage(txtPage.Handle, Eclectic.WM_VSCROLL, (int) Eclectic.ScrollBarActions.SB_LINEUP, 0);
            }
            else
            {
                Eclectic.SendMessage(txtPage.Handle, Eclectic.WM_VSCROLL, (int) Eclectic.ScrollBarActions.SB_LINEDOWN, 0);
            }
        }

        private void txtPage_TextChanged(object sender, EventArgs e)
        {
            UpdateEditMenu();
            updateScrollPercent();
            //lblStats.Text = StatsText();
        }

        private void txtPage_GotFocus(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.NeutralHighlight)
            {
                highlight = Eclectic.GetSysColor(Eclectic.COLOR_HIGHLIGHT);
                highlightText = Eclectic.GetSysColor(Eclectic.COLOR_HIGHLIGHTTEXT);

                int[] elements = { Eclectic.COLOR_HIGHLIGHT, Eclectic.COLOR_HIGHLIGHTTEXT };
                int[] colors = { 0, 0 };

                if (txtPage.ForeColor.R > 150 | txtPage.ForeColor.G > 150 | txtPage.ForeColor.B > 150)
                {
                    colors[0] = getWin32Color(txtPage.ForeColor, -100);
                }
                else
                {
                    colors[0] = getWin32Color(txtPage.ForeColor, 100);
                }

                colors[1] = System.Drawing.ColorTranslator.ToWin32(txtPage.ForeColor);

                Eclectic.SetSysColors(elements.Length, elements, colors);
            }

            blinkRate = Eclectic.GetCaretBlinkTime();
            Eclectic.SetCaretBlinkTime(Properties.Settings.Default.CaretBlinkRate);
        }

        private void txtPage_LostFocus(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.NeutralHighlight)
            {
                int[] elements = { Eclectic.COLOR_HIGHLIGHT, Eclectic.COLOR_HIGHLIGHTTEXT };
                int[] colors = { highlight, highlightText };

                Eclectic.SetSysColors(elements.Length, elements, colors);
            }

            Eclectic.SetCaretBlinkTime(blinkRate);
        }

        private void txtPage_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false) == true)
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                DialogResult UserResponse = new DialogResult();
                if (txtPage.Modified)
                {
                    UserResponse = ConfirmOverwrite();
                }
                if (txtPage.Modified & UserResponse == DialogResult.Cancel)
                {
                    return;
                }
                else if (txtPage.Modified & UserResponse == DialogResult.Yes)
                {
                    SaveFile();
                }

                OpenFile(files[0]);

                try
                {
                    e.Data.SetData(null);
                }
                catch { 
                    /* Do nothing */
                }
            }
        }

        private void btnPageUp_Click(object sender, EventArgs e)
        {
            ScrollAction = (int) Eclectic.ScrollBarActions.SB_PAGEUP;
            Eclectic.SendMessage(txtPage.Handle, Eclectic.WM_VSCROLL, (int) Eclectic.ScrollBarActions.SB_PAGEUP, 0);
            txtPage.Focus();
        }

        private void btnLineUp_MouseDown(object sender, MouseEventArgs e)
        {
            ScrollAction = (int) Eclectic.ScrollBarActions.SB_LINEUP;
            tmrScroll.Enabled = true;
            Eclectic.SendMessage(txtPage.Handle, Eclectic.WM_VSCROLL, (int) Eclectic.ScrollBarActions.SB_LINEUP, 0);
        }

        private void btnLineUp_MouseUp(object sender, MouseEventArgs e)
        {
            tmrScroll.Enabled = false;
            txtPage.Focus();
        }

        private void btnLineDown_MouseDown(object sender, MouseEventArgs e)
        {
            ScrollAction = (int) Eclectic.ScrollBarActions.SB_LINEDOWN;
            tmrScroll.Enabled = true;
            Eclectic.SendMessage(txtPage.Handle, Eclectic.WM_VSCROLL, (int) Eclectic.ScrollBarActions.SB_LINEDOWN, 0);
            txtPage.Focus();
        }

        private void btnLineDown_MouseUp(object sender, MouseEventArgs e)
        {
            tmrScroll.Enabled = false;
            txtPage.Focus();
        }

        private void btnPageDown_Click(object sender, EventArgs e)
        {
            ScrollAction = (int) Eclectic.ScrollBarActions.SB_PAGEDOWN;
            Eclectic.SendMessage(txtPage.Handle, Eclectic.WM_VSCROLL, (int) Eclectic.ScrollBarActions.SB_PAGEDOWN, 0);
            txtPage.Focus();
        }

        private void pnlNavBtns_MouseEnter(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            b.ForeColor = Properties.Settings.Default.PageColor;
            b.BackColor = Properties.Settings.Default.ForegroundColor;
        }

        private void pnlNavBtns_MouseLeave(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            b.ForeColor = Properties.Settings.Default.ForegroundColor;
            b.BackColor = Properties.Settings.Default.PageColor;
        }

        private void tmrScroll_Tick(object sender, EventArgs e)
        {
            switch(ScrollAction) {
                case (int) Eclectic.ScrollBarActions.SB_LINEDOWN:
                    Eclectic.SendMessage(txtPage.Handle, Eclectic.WM_VSCROLL, (int)Eclectic.ScrollBarActions.SB_LINEDOWN, 0);
                    break;
                case (int) Eclectic.ScrollBarActions.SB_PAGEDOWN:
                    Eclectic.SendMessage(txtPage.Handle, Eclectic.WM_VSCROLL, (int)Eclectic.ScrollBarActions.SB_PAGEDOWN, 0);
                    break;
                case (int) Eclectic.ScrollBarActions.SB_LINEUP:
                    Eclectic.SendMessage(txtPage.Handle, Eclectic.WM_VSCROLL, (int)Eclectic.ScrollBarActions.SB_LINEUP, 0);
                    break;
                case (int) Eclectic.ScrollBarActions.SB_PAGEUP:
                    Eclectic.SendMessage(txtPage.Handle, Eclectic.WM_VSCROLL, (int)Eclectic.ScrollBarActions.SB_PAGEUP, 0);
                    break;
            }
        }

        private void txtPage_PageScrolled(object sender, EventArgs e)
        {
            updateScrollPercent();
        }

        public void updateScrollPercent() {
            int a = txtPage.getScrollYPos();
            //int b = txtPage.getScrollYPosMax();
            int b = txtPage.getVScrollMax();
            int p = 0;
            if (b > 0) {
                //p = (int)Math.Floor(((double)a / (double)b) * 100.0);
                p = (int)Math.Round(((double)a / (double)b) * 100.0);
            }
            if (a > b)
                p = 100;

            ScrollWin9xProgressBar.Value = p;

            //scrollpercentLbl.Text = a.ToString() + " | " + b.ToString();
            //scrollpercentLbl.Text = p.ToString() + "%";
        }

        private void prtDoc_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            
            Font font = txtPage.Font;
            Int32 PrintAreaHeight, PrintAreaWidth, marginLeft, marginTop;

            PrintAreaHeight = prtDoc.DefaultPageSettings.PaperSize.Height - prtDoc.DefaultPageSettings.Margins.Top - prtDoc.DefaultPageSettings.Margins.Bottom;
            PrintAreaWidth = prtDoc.DefaultPageSettings.PaperSize.Width - prtDoc.DefaultPageSettings.Margins.Left - prtDoc.DefaultPageSettings.Margins.Right;
            marginLeft = prtDoc.DefaultPageSettings.Margins.Left;
            marginTop = prtDoc.DefaultPageSettings.Margins.Top;

            if (prtDoc.DefaultPageSettings.Landscape)
            {
                Int32 intTemp;
                intTemp = PrintAreaHeight;
                PrintAreaHeight = PrintAreaWidth;
                PrintAreaWidth = intTemp;
            }

            Int32 intLineCount = System.Convert.ToInt32(PrintAreaHeight / font.Height);
            RectangleF rectPrintingArea = new RectangleF(marginLeft, marginTop, PrintAreaWidth, PrintAreaHeight);
            StringFormat fmt = new StringFormat(StringFormatFlags.LineLimit);
            Int32 intLinesFilled;
            Int32 intCharsFitted;
            e.Graphics.MeasureString(Eclectic.Mid(txtPage.Text, intCurrentChar), font, new SizeF(PrintAreaWidth, PrintAreaHeight), fmt, out intCharsFitted, out intLinesFilled);
            e.Graphics.DrawString(Eclectic.Mid(txtPage.Text, intCurrentChar), font, Brushes.Black, rectPrintingArea, fmt);
            intCurrentChar += intCharsFitted;
            if (intCurrentChar < (txtPage.Text.Length - 1))
            {
                e.HasMorePages = true;
            }
            else
            {
                e.HasMorePages = false;
                intCurrentChar = 0;
            }
            
        }

        private void statisticsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmStatistics statistics = new frmStatistics(this);
            statistics.ShowDialog();
        }

        private void WhiteRoomWebsiteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(Properties.Settings.Default.WhiteRoomURL);
        }

        private void checkForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string latest = Eclectic.CheckForUpdate();

            if (latest.Trim() != "") //show nothing if errors occurred
            {
                if (latest == System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString())
                {
                    MessageBox.Show("Your version of WhiteRoom is up to date.", "WriteRoom - Update", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    DialogResult response = MessageBox.Show("A new version (" + latest + ") of WhiteRoom is available.\n\nWould you like to download it?", "WhiteRoom New Version Available", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (response == DialogResult.Yes)
                    {
                        System.Diagnostics.Process.Start(Properties.Settings.Default.WhiteRoomURL);
                    }
                }
            }
        }
    }
}
