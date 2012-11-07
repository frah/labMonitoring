using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Twitterizer;

namespace LabMonitoring
{
    public partial class OAuthForm : Form
    {
        private OAuthTokenResponse res;

        public OAuthForm()
        {
            InitializeComponent();
        }

        private void authButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(keyTextBox.Text) || string.IsNullOrWhiteSpace(secretTextBox.Text))
            {
                MessageBox.Show("Please input consumer key & secret", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            res = OAuthUtility.GetRequestToken(keyTextBox.Text, secretTextBox.Text, "oob");
            Uri uri = OAuthUtility.BuildAuthorizationUri(res.Token);
            webBrowser.Navigate(uri);
            keyTextBox.Enabled = false;
            secretTextBox.Enabled = false;
            authButton.Enabled = false;
            pinTextBox.Enabled = true;
            okButton.Enabled = true;
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            OAuthTokenResponse res2 = OAuthUtility.GetAccessToken(keyTextBox.Text, secretTextBox.Text, res.Token, pinTextBox.Text);
            Properties.Settings.Default.consumerKey = keyTextBox.Text;
            Properties.Settings.Default.consumerSecret = secretTextBox.Text;
            Properties.Settings.Default.accessToken = res2.Token;
            Properties.Settings.Default.accessTokenSecret = res2.TokenSecret;

            Properties.Settings.Default.Save();

            this.Dispose();
        }

        private void OAuthForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Abort;
        }
    }
}
