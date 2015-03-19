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

namespace WhiteRoom
{
    public partial class frmStatistics : Form
    {
        private frmMain parent;

        public frmStatistics()
        {
            InitializeComponent();
        }

        public frmStatistics(frmMain pParent)
        {
            parent = pParent;
            InitializeComponent();
        }

        private void frmStatistics_Load(object sender, EventArgs e)
        {
            lblWords.Text = parent.Statistics(0).ToString();
            lblCharNoSpaces.Text = parent.Statistics(1).ToString();
            lblCharSpaces.Text = parent.Statistics(2).ToString();
            lblLines.Text = parent.Statistics(3).ToString();
        }
    }
}