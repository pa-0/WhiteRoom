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
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Drawing;

using System.Runtime.InteropServices;

namespace WhiteRoom
{
    public partial class DarkTextBox : System.Windows.Forms.RichTextBox 
    {
        // TOGGLE MODE
        public bool FormattingEnabled = false;
        
        private const int TO_ADVANCEDTYPOGRAPHY = 1;
        private const int EM_SETTYPOGRAPHYOPTIONS = (Eclectic.WM_USER + 202);

        private int tabCount = 0;

        public bool AutoIndent = false;
        public int TabsToSpaces = 0;

        public DarkTextBox()
        {
            InitializeComponent();

            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DarkTextBox_KeyDown);
        }

        public DarkTextBox(IContainer container)
        {
            container.Add(this);

            InitializeComponent();

            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.DarkTextBox_KeyDown);
        }

        public int getScrollYPos()
        {
            // http://stackoverflow.com/a/22861913/883015
            var ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(Eclectic.POINT)));
            Marshal.StructureToPtr(new Eclectic.POINT(), ptr, false);
            Eclectic.SendMessage(this.Handle, Eclectic.EM_GETSCROLLPOS, (int)IntPtr.Zero, (int)ptr);
            var point = (Eclectic.POINT)Marshal.PtrToStructure(ptr, typeof(Eclectic.POINT));
            Marshal.FreeHGlobal(ptr);
            return point.Y;
        }

        public int getScrollYPosMax()
        {
            // Estimation method
            int offset = 74;
            int TotalLines = this.Lines.Length;
            int max_b = (this.FontHeight * TotalLines) - this.Size.Height - offset;
            return (max_b>0) ? max_b : 0;
        }

        public int getVScrollMax()
        {
            // Estimation method 2, More accurate and supports new lines caused by wordwrap
            // modified from http://stackoverflow.com/a/2986455/883015
            int offset = 17;
            if (this.TextLength == 0) return 0;
            var p1 = this.GetPositionFromCharIndex(0);
            var p2 = this.GetPositionFromCharIndex(this.TextLength - 1);

            int scrollPos = -p1.Y;
            int maxPos = p2.Y - p1.Y - this.ClientSize.Height + offset;

            return (maxPos > 0) ? maxPos : 0;
        }

        #region "Helper Functions"

        private string findIndentLevel()
        {
            int startPos;
            int endPos;
            string test;

            startPos = Text.LastIndexOf("\n") + 1;
            if (startPos == -1) startPos = 0;

            for (endPos = startPos; endPos < SelectionStart; endPos++)
            {
                test = Text.Substring(endPos, 1);
                if (test != " " & test != "\t")
                    break;
            }
            
            if (startPos != endPos) 
            {
                return Text.Substring(startPos, endPos - startPos);
            }

            return "";
        }

        #endregion

        private void DarkTextBox_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {

            if (e.Control) // handle control key functions
            {
                if (FormattingEnabled) // handle formating keys if formatting enabled
                {
                    if (e.KeyCode == Keys.B)
                    {
                        if (this.SelectionFont.Bold)
                        {
                            this.SelectionFont = new Font(this.SelectionFont, FontStyle.Regular);
                        }
                        else
                        {
                            this.SelectionFont = new Font(this.SelectionFont, FontStyle.Bold);
                        }
                    }
                    else if (e.KeyCode == Keys.J)
                    {
                        Eclectic.SendMessage(Handle, EM_SETTYPOGRAPHYOPTIONS, TO_ADVANCEDTYPOGRAPHY, TO_ADVANCEDTYPOGRAPHY);
                    }
                }
                else // supress formating if formatting disabled
                {
                    if (e.KeyCode == Keys.L || e.KeyCode == Keys.E || e.KeyCode == Keys.R || e.KeyCode == Keys.Oem2 || e.KeyCode == Keys.Oem1)
                    {
                        e.Handled = true;
                    }
                }
            }
            else // handle if control not pressed 
            {
                // handle new line
                if (e.KeyCode == Keys.Enter)
                {
                    // handle autoindent
                    if (AutoIndent & SelectionStart != 0)
                    {
                        SelectedText = "\n" + findIndentLevel();
                        e.Handled = true;
                    }
                }

                // handle tabs to spaces
                if (e.KeyCode == Keys.Tab)
                {
                    if (TabsToSpaces > 0)
                    {

                        SendKeys.Send("{BACKSPACE}");
                        for (int i = 0; i < TabsToSpaces; i++)
                            SendKeys.Send(" ");
                    }

                    tabCount++;
                }
            }
        }
    }
}
