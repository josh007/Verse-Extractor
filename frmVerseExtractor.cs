using System;
using System.Windows.Forms;

namespace VerseExtractor
{
    public partial class frmMain : Form
    {
        public Proceessor Processor { get; set; }

        public frmMain()
        {
            InitializeComponent();
        }
        private void frmMain_Load(object sender, EventArgs e)
        {
            lblWarning.Text = "";
        }

        private void btnProcessPass1_Click(object sender, EventArgs e)
        {
            try
            {
                if (txtContent.Text.Trim() == "")
                {
                    MessageBox.Show("No data to process", "Verse Extractor", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                Processor = new Proceessor();
                Processor.ExtractVerseAndProcessRaw(txtContent.Text);
                txtContent.Text = Processor.OutPutText;
                btnProcessPass2.Enabled = true;

                lblWarning.Text = Processor.Warning;
            }

            catch (Exception ex)
            {
                MessageBox.Show("You did something WRONG!!! Fix it and Re-Run", "Verse Extractor", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnProcessPass2_Click(object sender, EventArgs e)
        {
            try
            {
                if (txtContent.Text.Trim() == "")
                {
                    MessageBox.Show("No data to process", "Verse Extractor", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                Processor.MergeProcessedVerseWithVerseText(txtContent.Text);
                txtContent.Text = Processor.OutPutText;

                lblWarning.Text = Processor.Warning;
            }

            catch (Exception ex)
            {
                MessageBox.Show("You did something WRONG!!! Fix it and Re-Run", "Verse Extractor", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtContent.Text = "";
            txtContent.Focus();
        }
        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            txtContent.SelectAll();
            txtContent.Focus();
        }
        private void btnCopy_Click(object sender, EventArgs e)
        {
            txtContent.SelectAll();
            txtContent.Focus();
            txtContent.Copy();
        }

        private void btnAbout_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Verse Extractor!\r\nCreated By : Josh July 2014 \r\nCopyright © JoshSoft Inc. 2014\r\nVerse Extractr extracts WT study article from JW site/Merge with epub verse list for consumption",
                "Verse Extractor", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void txtContent_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
                txtContent.SelectAll();
        }
    }
}
