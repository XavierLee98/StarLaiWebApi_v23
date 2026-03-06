using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Layout;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Model.NodeGenerators;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Templates;
using DevExpress.ExpressApp.Utils;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace StarLaiPortal.Module.Controllers
{
    public partial class MyChangePasswordViewController : ViewController<DetailView>
    {
        public MyChangePasswordViewController()
        {
            InitializeComponent();
        }
        protected override void OnActivated()
        {
            base.OnActivated();

            if (View.Id == "ChangePasswordOnLogonParameters_DetailView")
            {
                SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ToString());

                string query = "SELECT ISNULL(PwdMinLen, 0), ISNULL(MinUppers, 0), ISNULL(MinLowCase, 0), " +
                 "ISNULL(MinDigits, 0), ISNULL(MinNonAlph, 0), ISNULL(PwdExample, '') FROM [" + ConfigurationManager.AppSettings["SAPDB"].ToString() + "]..OPPA";
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
                conn.Open();
                SqlCommand cmdsecurity = new SqlCommand(query, conn);
                SqlDataReader readersecurity = cmdsecurity.ExecuteReader();
                while (readersecurity.Read())
                {
                    if (View is DetailView detailView)
                    {
                        // 1. Create the model item for static text
                        IModelNode modelStaticText = detailView.Model.Items.GetNode(4);
                        if (modelStaticText != null)
                        {
                            modelStaticText.SetValue("Text", "Minimum password length: " + readersecurity.GetInt32(0) + " " + Environment.NewLine +
                                "Minimum number of uppercase character: " + readersecurity.GetInt32(1) + " " + Environment.NewLine +
                                "Minimum number of lowercase character: " + readersecurity.GetInt32(2) + " " + Environment.NewLine +
                                "Minimum number of digit: " + readersecurity.GetInt32(3) + " " + Environment.NewLine +
                                "Minimum number of non-alphanumeric character: " + readersecurity.GetInt32(4) + " " + Environment.NewLine +
                                "Password example: " + readersecurity.GetString(5));
                        }
                    }
                }
                cmdsecurity.Dispose();
                conn.Close();
            }

            if (View.Id == "ChangePasswordParameters_DetailView")
            {
                SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ToString());

                string query = "SELECT ISNULL(PwdMinLen, 0), ISNULL(MinUppers, 0), ISNULL(MinLowCase, 0), " +
                 "ISNULL(MinDigits, 0), ISNULL(MinNonAlph, 0), ISNULL(PwdExample, '') FROM [" + ConfigurationManager.AppSettings["SAPDB"].ToString() + "]..OPPA";
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
                conn.Open();
                SqlCommand cmdsecurity = new SqlCommand(query, conn);
                SqlDataReader readersecurity = cmdsecurity.ExecuteReader();
                while (readersecurity.Read())
                {
                    if (View is DetailView detailView)
                    {
                        // 1. Create the model item for static text
                        IModelNode modelStaticText = detailView.Model.Items.GetNode(5);
                        if (modelStaticText != null)
                        {
                            modelStaticText.SetValue("Text", "Minimum password length: " + readersecurity.GetInt32(0) + " " + Environment.NewLine +
                                "Minimum number of uppercase character: " + readersecurity.GetInt32(1) + " " + Environment.NewLine +
                                "Minimum number of lowercase character: " + readersecurity.GetInt32(2) + " " + Environment.NewLine +
                                "Minimum number of digit: " + readersecurity.GetInt32(3) + " " + Environment.NewLine +
                                "Minimum number of non-alphanumeric character: " + readersecurity.GetInt32(4) + " " + Environment.NewLine +
                                "Password example: " + readersecurity.GetString(5));
                        }
                    }
                }
                cmdsecurity.Dispose();
                conn.Close();
            }
        }
        protected override void OnViewControlsCreated()
        {
            base.OnViewControlsCreated();
        }
        protected override void OnDeactivated()
        {
            base.OnDeactivated();
        }
    }
}
