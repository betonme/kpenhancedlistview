namespace KPEnhancedListview
{
    partial class AboutForm
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.m_bannerImage = new System.Windows.Forms.PictureBox();
            this.m_lblCopyright = new System.Windows.Forms.Label();
            this.m_lblDescription = new System.Windows.Forms.Label();
            this.m_lblGpl = new System.Windows.Forms.Label();
            this.m_linkHomepage = new System.Windows.Forms.LinkLabel();
            this.m_linkDonate = new System.Windows.Forms.LinkLabel();
            this.m_btnOK = new System.Windows.Forms.Button();
            this.m_lvComponents = new KeePass.UI.CustomListViewEx();
            ((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).BeginInit();
            this.SuspendLayout();
            // 
            // m_bannerImage
            // 
            this.m_bannerImage.Dock = System.Windows.Forms.DockStyle.Top;
            this.m_bannerImage.Location = new System.Drawing.Point(0, 0);
            this.m_bannerImage.Name = "m_bannerImage";
            this.m_bannerImage.Size = new System.Drawing.Size(461, 60);
            this.m_bannerImage.TabIndex = 0;
            this.m_bannerImage.TabStop = false;
            // 
            // m_lblCopyright
            // 
            this.m_lblCopyright.Location = new System.Drawing.Point(10, 72);
            this.m_lblCopyright.Name = "m_lblCopyright";
            this.m_lblCopyright.Size = new System.Drawing.Size(402, 15);
            this.m_lblCopyright.TabIndex = 1;
            // 
            // m_lblDescription
            // 
            this.m_lblDescription.Location = new System.Drawing.Point(10, 96);
            this.m_lblDescription.Name = "m_lblDescription";
            this.m_lblDescription.Size = new System.Drawing.Size(402, 14);
            this.m_lblDescription.TabIndex = 2;
            // 
            // m_lblGpl
            // 
            this.m_lblGpl.Location = new System.Drawing.Point(10, 119);
            this.m_lblGpl.Name = "m_lblGpl";
            this.m_lblGpl.Size = new System.Drawing.Size(439, 36);
            this.m_lblGpl.TabIndex = 3;
            this.m_lblGpl.Text = "The program is distributed under the terms of the GNU General Public License v2 o" +
                "r later.";
            // 
            // m_linkHomepage
            // 
            this.m_linkHomepage.AutoSize = true;
            this.m_linkHomepage.Location = new System.Drawing.Point(10, 155);
            this.m_linkHomepage.Name = "m_linkHomepage";
            this.m_linkHomepage.Size = new System.Drawing.Size(147, 13);
            this.m_linkHomepage.TabIndex = 4;
            this.m_linkHomepage.TabStop = true;
            this.m_linkHomepage.Text = "Google Code Project Website";
            this.m_linkHomepage.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnLinkHomepage);
            // 
            // m_linkDonate
            // 
            this.m_linkDonate.AutoSize = true;
            this.m_linkDonate.Location = new System.Drawing.Point(195, 155);
            this.m_linkDonate.Name = "m_linkDonate";
            this.m_linkDonate.Size = new System.Drawing.Size(42, 13);
            this.m_linkDonate.TabIndex = 8;
            this.m_linkDonate.TabStop = true;
            this.m_linkDonate.Text = "Donate";
            this.m_linkDonate.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnLinkDonate);
            // 
            // m_btnOK
            // 
            this.m_btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.m_btnOK.Location = new System.Drawing.Point(374, 153);
            this.m_btnOK.Name = "m_btnOK";
            this.m_btnOK.Size = new System.Drawing.Size(75, 23);
            this.m_btnOK.TabIndex = 0;
            this.m_btnOK.Text = "&OK";
            this.m_btnOK.UseVisualStyleBackColor = true;
            // 
            // m_lvComponents
            // 
            this.m_lvComponents.Location = new System.Drawing.Point(0, 0);
            this.m_lvComponents.Name = "m_lvComponents";
            this.m_lvComponents.Size = new System.Drawing.Size(121, 97);
            this.m_lvComponents.TabIndex = 0;
            this.m_lvComponents.UseCompatibleStateImageBehavior = false;
            // 
            // AboutForm
            // 
            this.AcceptButton = this.m_btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.m_btnOK;
            this.ClientSize = new System.Drawing.Size(461, 187);
            this.Controls.Add(this.m_btnOK);
            this.Controls.Add(this.m_linkDonate);
            this.Controls.Add(this.m_linkHomepage);
            this.Controls.Add(this.m_lblGpl);
            this.Controls.Add(this.m_lblDescription);
            this.Controls.Add(this.m_lblCopyright);
            this.Controls.Add(this.m_bannerImage);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutForm";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "About KPEnhancedListview";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OnFormClosed);
            this.Load += new System.EventHandler(this.OnFormLoad);
            ((System.ComponentModel.ISupportInitialize)(this.m_bannerImage)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox m_bannerImage;
        private System.Windows.Forms.Label m_lblCopyright;
        private System.Windows.Forms.Label m_lblDescription;
        private System.Windows.Forms.Label m_lblGpl;
        private System.Windows.Forms.LinkLabel m_linkHomepage;
        private System.Windows.Forms.LinkLabel m_linkDonate;
        private System.Windows.Forms.Button m_btnOK;
        private KeePass.UI.CustomListViewEx m_lvComponents;
    }
}