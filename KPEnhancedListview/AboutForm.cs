using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

using KeePass;
using KeePass.UI;
using KeePass.Util;

using KeePassLib;

namespace KPEnhancedListview
{
    public partial class AboutForm : Form, IGwmWindow
    {
        public bool CanCloseWithoutDataLoss { get { return true; } }

        public AboutForm()
        {
            InitializeComponent();
            Program.Translation.ApplyTo(this);
        }

        private void OnFormLoad(object sender, EventArgs e)
        {
            GlobalWindowManager.AddWindow(this, this);

            Type clsType = typeof(KPEnhancedListviewExt);
            Assembly assy = clsType.Assembly;

            string strTitle = ProductName;
            AssemblyTitleAttribute adAttrTitle =
                (AssemblyTitleAttribute)Attribute.GetCustomAttribute(
                assy, typeof(AssemblyTitleAttribute));
            if (adAttrTitle != null)
                strTitle = adAttrTitle.Title;
                
            string strVersion = "Version: " + this.ProductVersion;

            m_lblDescription.Text = "";
            AssemblyDescriptionAttribute adAttrDescription =
                (AssemblyDescriptionAttribute)Attribute.GetCustomAttribute(
                assy, typeof(AssemblyDescriptionAttribute));
            if (adAttrDescription != null)
                m_lblDescription.Text = adAttrDescription.Description + ".";
                
            m_lblCopyright.Text = "";
            AssemblyCopyrightAttribute adAttrCopyright =
                (AssemblyCopyrightAttribute)Attribute.GetCustomAttribute(
                assy, typeof(AssemblyCopyrightAttribute));
            if (adAttrCopyright != null)
                m_lblCopyright.Text = adAttrCopyright.Copyright + ".";
                
            Icon icoNew = new Icon(Properties.Resources.KeePass, 48, 48);

            BannerFactory.CreateBannerEx(this, m_bannerImage, icoNew.ToBitmap(),
                strTitle, strVersion);
            this.Icon = Properties.Resources.KeePass;
        }

        private void OnFormClosed(object sender, FormClosedEventArgs e)
        {
            GlobalWindowManager.RemoveWindow(this);
        }

        private void OnLinkHomepage(object sender, LinkLabelLinkClickedEventArgs e)
        {
            const string HomepageUrl = "http://code.google.com/p/kpenhancedlistview/";
            WinUtil.OpenUrl(HomepageUrl, null);
            this.Close();
        }

        private void OnLinkDonate(object sender, LinkLabelLinkClickedEventArgs e)
        {
            const string HomepageUrl = "http://bit.ly/kpelpaypal";
            WinUtil.OpenUrl(HomepageUrl, null);
            this.Close();
        }

    }
}