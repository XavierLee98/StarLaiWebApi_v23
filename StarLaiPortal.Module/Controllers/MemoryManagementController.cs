using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Layout;
using DevExpress.ExpressApp.Model.NodeGenerators;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Templates;
using DevExpress.ExpressApp.Utils;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
using DevExpress.Persistent.Validation;
using StarLaiPortal.Module.BusinessObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StarLaiPortal.Module.Controllers
{
    // For more typical usage scenarios, be sure to check out https://documentation.devexpress.com/eXpressAppFramework/clsDevExpressExpressAppViewControllertopic.aspx.
    public partial class MemoryManagementController : ViewController
    {
        GeneralControllers genCon;
        public MemoryManagementController()
        {
            InitializeComponent();
            // Target required Views (via the TargetXXX properties) and create their Actions.
        }
        protected override void OnActivated()
        {
            base.OnActivated();
            // Perform various tasks depending on the target View.
            this.ForceReleaseMemory.Active.SetItemValue("Enabled", false);
            this.ForceFlushGC.Active.SetItemValue("Enabled", false);
        }
        protected override void OnViewControlsCreated()
        {
            base.OnViewControlsCreated();
            // Access and customize the target View control.
            genCon = Frame.GetController<GeneralControllers>();

            if (View.Id == "ApplicationUser_DetailView")
            {
                if (((DetailView)View).ViewEditMode == ViewEditMode.View)
                {
                    PermissionPolicyRole AdminRole = ObjectSpace.FindObject<PermissionPolicyRole>(CriteriaOperator.Parse("IsCurrentUserInRole('Administrators')"));

                    if (AdminRole != null)
                    {
                        ApplicationUser user = (ApplicationUser)SecuritySystem.CurrentUser;

                        BusinessObjects.ApplicationUser curruser = View.CurrentObject as BusinessObjects.ApplicationUser;

                        if (curruser.UserName == "Admin")
                        {
                            this.ForceReleaseMemory.Active.SetItemValue("Enabled", true);
                            this.ForceFlushGC.Active.SetItemValue("Enabled", true);
                        }
                    }
                }
                else
                {
                    this.ForceReleaseMemory.Active.SetItemValue("Enabled", false);
                }
            }
            else
            {
                this.ForceReleaseMemory.Active.SetItemValue("Enabled", false);
            }
        }
        protected override void OnDeactivated()
        {
            // Unsubscribe from previously subscribed events and release other references and resources.
            base.OnDeactivated();
        }

        private void ForceReleaseMemory_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            try
            {
                MemoryManagement.FlushMemory();
            }
            catch (Exception ex)
            {
                genCon.showMsg("Fail", ex.Message, InformationType.Error);
            }

            genCon.showMsg("Successful", "Memory Flush Successful.", InformationType.Success);
        }

        private void ForceFlushGC_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            try
            {
                MemoryManagement.FlushGCMemory();
            }
            catch (Exception ex)
            {
                genCon.showMsg("Fail", ex.Message, InformationType.Error);
            }

            genCon.showMsg("Successful", "Flush GC Successful.", InformationType.Success);
        }
    }
}
