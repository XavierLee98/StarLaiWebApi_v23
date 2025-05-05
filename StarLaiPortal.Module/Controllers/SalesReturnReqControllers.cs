using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Layout;
using DevExpress.ExpressApp.Model.NodeGenerators;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.ExpressApp.Templates;
using DevExpress.ExpressApp.Utils;
using DevExpress.ExpressApp.Web.Editors.ASPx;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.Validation;
using StarLaiPortal.Module.BusinessObjects;
using StarLaiPortal.Module.BusinessObjects.Delivery_Order;
using StarLaiPortal.Module.BusinessObjects.Item_Inquiry;
using StarLaiPortal.Module.BusinessObjects.Sales_Order;
using StarLaiPortal.Module.BusinessObjects.Sales_Return;
using StarLaiPortal.Module.BusinessObjects.Setup;
using StarLaiPortal.Module.BusinessObjects.View;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

// 2023-08-16 - add stock 3 and stock 4 - ver 1.0.8
// 2023-10-05 add payment method for sales return ver 1.0.10
// 2024-06-12 - e-invoice - ver 1.0.18
// 2024-07-18 - add basedoc - ver 1.0.19
// 2025-02-04 - not allow submit if posting period locked - ver 1.0.22

namespace StarLaiPortal.Module.Controllers
{
    // For more typical usage scenarios, be sure to check out https://documentation.devexpress.com/eXpressAppFramework/clsDevExpressExpressAppViewControllertopic.aspx.
    public partial class SalesReturnReqControllers : ViewController
    {
        GeneralControllers genCon;
        public SalesReturnReqControllers()
        {
            InitializeComponent();
            // Target required Views (via the TargetXXX properties) and create their Actions.
        }
        protected override void OnActivated()
        {
            base.OnActivated();
            // Perform various tasks depending on the target View.
            this.SRRCopyFromInv.Active.SetItemValue("Enabled", false);
            this.SRRInquiryItem.Active.SetItemValue("Enabled", false);
            this.SubmitSRR.Active.SetItemValue("Enabled", false);
            this.CancelSRR.Active.SetItemValue("Enabled", false);
            this.PreviewSRR.Active.SetItemValue("Enabled", false);
            this.ReviewAppSRR.Active.SetItemValue("Enabled", false);
            this.ApproveAppSRR.Active.SetItemValue("Enabled", false);
            this.RejectAppSRR.Active.SetItemValue("Enabled", false);
            this.SRRPrintLabel.Active.SetItemValue("Enabled", false);
            this.SRRCopyToSR.Active.SetItemValue("Enabled", false);
            this.ApproveAppSRR_Pop.Active.SetItemValue("Enabled", false);
        }
        protected override void OnViewControlsCreated()
        {
            base.OnViewControlsCreated();
            // Access and customize the target View control.
            genCon = Frame.GetController<GeneralControllers>();

            if (View.Id == "SalesReturnRequests_DetailView")
            {
                if (((DetailView)View).ViewEditMode == ViewEditMode.View)
                {
                    this.SubmitSRR.Active.SetItemValue("Enabled", true);
                    this.CancelSRR.Active.SetItemValue("Enabled", true);
                    //this.PreviewSRR.Active.SetItemValue("Enabled", true);
                    //this.SRRPrintLabel.Active.SetItemValue("Enabled", true);
                    this.SRRCopyToSR.Active.SetItemValue("Enabled", true);
                }
                else
                {
                    this.SubmitSRR.Active.SetItemValue("Enabled", false);
                    this.CancelSRR.Active.SetItemValue("Enabled", false);
                    this.PreviewSRR.Active.SetItemValue("Enabled", false);
                    this.SRRPrintLabel.Active.SetItemValue("Enabled", false);
                    this.SRRCopyToSR.Active.SetItemValue("Enabled", false);
                }

                if (((DetailView)View).ViewEditMode == ViewEditMode.Edit)
                {
                    this.SRRCopyFromInv.Active.SetItemValue("Enabled", true);
                    this.SRRInquiryItem.Active.SetItemValue("Enabled", true);
                }
                else
                {
                    this.SRRCopyFromInv.Active.SetItemValue("Enabled", false);
                    this.SRRInquiryItem.Active.SetItemValue("Enabled", false);
                }
            }
            else if (View.Id == "SalesReturnRequests_ListView_Approval")
            {
                //this.ReviewAppSRR.Active.SetItemValue("Enabled", true);
                //this.ReviewAppSRR.SelectionDependencyType = DevExpress.ExpressApp.Actions.SelectionDependencyType.RequireSingleObject;
                //this.ApproveAppSRR.Active.SetItemValue("Enabled", true);
                //this.ApproveAppSRR.SelectionDependencyType = DevExpress.ExpressApp.Actions.SelectionDependencyType.RequireSingleObject;
                //this.RejectAppSRR.Active.SetItemValue("Enabled", true);
                //this.RejectAppSRR.SelectionDependencyType = DevExpress.ExpressApp.Actions.SelectionDependencyType.RequireSingleObject;
                this.ApproveAppSRR_Pop.Active.SetItemValue("Enabled", true);
            }
            else if (View.Id == "SalesReturnRequests_DetailView_Approval")
            {
                if (((DetailView)View).ViewEditMode == ViewEditMode.View)
                {
                    this.ApproveAppSRR_Pop.Active.SetItemValue("Enabled", true);
                }
                else
                {
                    this.ApproveAppSRR_Pop.Active.SetItemValue("Enabled", false);
                }
            }
            else
            {
                this.SRRCopyFromInv.Active.SetItemValue("Enabled", false);
                this.SRRInquiryItem.Active.SetItemValue("Enabled", false);
                this.SubmitSRR.Active.SetItemValue("Enabled", false);
                this.CancelSRR.Active.SetItemValue("Enabled", false);
                this.PreviewSRR.Active.SetItemValue("Enabled", false);
                this.ReviewAppSRR.Active.SetItemValue("Enabled", false);
                this.ApproveAppSRR.Active.SetItemValue("Enabled", false);
                this.RejectAppSRR.Active.SetItemValue("Enabled", false);
                this.SRRPrintLabel.Active.SetItemValue("Enabled", false);
                this.SRRCopyToSR.Active.SetItemValue("Enabled", false);
                this.ApproveAppSRR_Pop.Active.SetItemValue("Enabled", false);
            }

            if (View.Id == "SalesReturnRequests_SalesReturnRequestDetails_ListView")
            {
                ((ASPxGridListEditor)((ListView)View).Editor).Grid.RowUpdating += new DevExpress.Web.Data.ASPxDataUpdatingEventHandler(Grid_RowUpdating);
            }
        }
        protected override void OnDeactivated()
        {
            // Unsubscribe from previously subscribed events and release other references and resources.
            base.OnDeactivated();
        }

        private void Grid_RowUpdating(object sender, DevExpress.Web.Data.ASPxDataUpdatingEventArgs e)
        {
            ASPxGridListEditor listEditor = ((ListView)View).Editor as ASPxGridListEditor;
            if (listEditor != null)
            {
                object currentObject = listEditor.Grid.GetRow(listEditor.Grid.EditingRowVisibleIndex);
                if (currentObject != null)
                {
                    //object validation = currentObject.GetType().GetProperty("IsValid").GetValue(currentObject);

                    //if ((bool)validation == true)
                    //{
                    //    showMsg("Error", "Price cannot zero.", InformationType.Error);
                    //}

                    object validation1 = currentObject.GetType().GetProperty("IsValid1").GetValue(currentObject);

                    if ((bool)validation1 == true)
                    {
                        showMsg("Error", "Unit Cost cannot zero.", InformationType.Error);
                    }
                }
            }
        }

        public void openNewView(IObjectSpace os, object target, ViewEditMode viewmode)
        {
            ShowViewParameters svp = new ShowViewParameters();
            DetailView dv = Application.CreateDetailView(os, target);
            dv.ViewEditMode = viewmode;
            dv.IsRoot = true;
            svp.CreatedView = dv;

            Application.ShowViewStrategy.ShowView(svp, new ShowViewSource(null, null));

        }
        public void showMsg(string caption, string msg, InformationType msgtype)
        {
            MessageOptions options = new MessageOptions();
            options.Duration = 3000;
            //options.Message = string.Format("{0} task(s) have been successfully updated!", e.SelectedObjects.Count);
            options.Message = string.Format("{0}", msg);
            options.Type = msgtype;
            options.Web.Position = InformationPosition.Right;
            options.Win.Caption = caption;
            options.Win.Type = WinMessageType.Flyout;
            Application.ShowViewStrategy.ShowMessage(options);
        }

        private void SRRCopyFromInv_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            if (e.PopupWindowViewSelectedObjects.Count > 0)
            {
                try
                {
                    SalesReturnRequests srr = (SalesReturnRequests)View.CurrentObject;

                    //if (srr.IsNew == true)
                    //{
                    //    IObjectSpace os = Application.CreateObjectSpace();
                    //    SalesReturnRequests newsrr = os.CreateObject<SalesReturnRequests>();

                    //    foreach (vwInvoice dtl in e.PopupWindowViewSelectedObjects)
                    //    {
                    //        if (dtl.CardCode != null)
                    //        {
                    //            newsrr.Customer = newsrr.Session.GetObjectByKey<vwBusniessPartner>(dtl.CardCode);
                    //        }

                    //        SalesReturnRequestDetails newsrritem = os.CreateObject<SalesReturnRequestDetails>();

                    //        newsrritem.ItemCode = newsrritem.Session.GetObjectByKey<vwItemMasters>(dtl.ItemCode);
                    //        newsrritem.ItemDesc = dtl.ItemDescrip;
                    //        if (dtl.WhsCode != null)
                    //        {
                    //            newsrritem.Warehouse = newsrritem.Session.GetObjectByKey<vwWarehouse>(dtl.WhsCode.WarehouseCode);
                    //        }
                    //        newsrritem.Quantity = dtl.OpenQty;
                    //        newsrritem.RtnQuantity = dtl.OpenQty;
                    //        newsrritem.Price = dtl.UnitPrice;
                    //        newsrritem.BaseDoc = dtl.BaseEntry.ToString();
                    //        newsrritem.BaseId = dtl.BaseLine.ToString();

                    //        newsrr.SalesReturnRequestDetails.Add(newsrritem);
                    //    }

                    //    ShowViewParameters svp = new ShowViewParameters();
                    //    DetailView dv = Application.CreateDetailView(os, newsrr);
                    //    dv.ViewEditMode = ViewEditMode.Edit;
                    //    dv.IsRoot = true;
                    //    svp.CreatedView = dv;

                    //    Application.ShowViewStrategy.ShowView(svp, new ShowViewSource(null, null));
                    //    showMsg("Success", "Copy Success.", InformationType.Success);
                    //}
                    //else
                    //{
                        string invoiceno = null;
                        string dupinv = null;
                        // Start ver 1.0.18
                        int count = 0;
                        // End ver 1.0.18
                        foreach (vwInvoice dtl in e.PopupWindowViewSelectedObjects)
                        {
                            if (dupinv != dtl.SAPDocNum)
                            {
                                if (invoiceno == null)
                                {
                                    invoiceno = dtl.SAPDocNum;
                                }
                                else
                                {
                                    invoiceno = invoiceno + ", " + dtl.SAPDocNum;
                                }

                                dupinv = dtl.SAPDocNum;
                            }

                            if (dtl.Salesperson != null)
                            {
                                srr.Salesperson = ObjectSpace.FindObject<vwSalesPerson>(CriteriaOperator.Parse("SlpName = ?", dtl.Salesperson));
                            }

                            // Start ver 1.0.18
                            if (dtl.PortalNum != null)
                            {
                                DeliveryOrder delivery = ObjectSpace.FindObject<DeliveryOrder>(CriteriaOperator.Parse("DocNum = ?", dtl.PortalNum));

                                if (delivery != null)
                                {
                                    // Buyer
                                    if (delivery.EIVConsolidate != null)
                                    {
                                        srr.EIVConsolidate = srr.Session.FindObject<vwYesNo>(CriteriaOperator.Parse("Code = ?", delivery.EIVConsolidate.Code));
                                    }
                                    if (delivery.EIVType != null)
                                    {
                                        srr.EIVType = srr.Session.FindObject<vwEIVType>(CriteriaOperator.Parse("Code = ?", delivery.EIVType.Code));
                                    }
                                    if (delivery.EIVFreqSync != null)
                                    {
                                        srr.EIVFreqSync = srr.Session.FindObject<vwEIVFreqSync>(CriteriaOperator.Parse("Code = ?", delivery.EIVFreqSync.Code));
                                    }
                                    srr.EIVBuyerName = delivery.EIVBuyerName;
                                    srr.EIVBuyerTIN = delivery.EIVBuyerTIN;
                                    srr.EIVBuyerRegNum = delivery.EIVBuyerRegNum;
                                    if (delivery.EIVBuyerRegTyp != null)
                                    {
                                        srr.EIVBuyerRegTyp = srr.Session.FindObject<vwEIVRegType>(CriteriaOperator.Parse("Code = ?", delivery.EIVBuyerRegTyp.Code));
                                    }
                                    srr.EIVBuyerSSTRegNum = delivery.EIVBuyerSSTRegNum;
                                    srr.EIVBuyerEmail = delivery.EIVBuyerEmail;
                                    srr.EIVBuyerContact = delivery.EIVBuyerContact;
                                    srr.EIVAddressLine1B = delivery.EIVAddressLine1B;
                                    srr.EIVAddressLine2B = delivery.EIVAddressLine2B;
                                    srr.EIVAddressLine3B = delivery.EIVAddressLine3B;
                                    srr.EIVPostalZoneB = delivery.EIVPostalZoneB;
                                    srr.EIVCityNameB = delivery.EIVCityNameB;
                                    if(delivery.EIVStateB != null)
                                    {
                                        srr.EIVStateB = srr.Session.FindObject<vwState>(CriteriaOperator.Parse("Code = ?", delivery.EIVStateB.Code));
                                    }
                                    if(delivery.EIVCountryB != null)
                                    {
                                        srr.EIVCountryB = srr.Session.FindObject<vwCountry>(CriteriaOperator.Parse("Code = ?", delivery.EIVCountryB.Code));
                                    }
                                    //Recipient
                                    srr.EIVShippingName = delivery.EIVShippingName;
                                    srr.EIVShippingTin = delivery.EIVShippingTin;
                                    srr.EIVShippingRegNum = delivery.EIVShippingRegNum;
                                    if (delivery.EIVShippingRegTyp != null)
                                    {
                                        srr.EIVShippingRegTyp = srr.Session.FindObject<vwEIVRegType>(CriteriaOperator.Parse("Code = ?", delivery.EIVShippingRegTyp.Code));
                                    }
                                    srr.EIVAddressLine1S = delivery.EIVAddressLine1S;
                                    srr.EIVAddressLine2S = delivery.EIVAddressLine2S;
                                    srr.EIVAddressLine3S = delivery.EIVAddressLine3S;
                                    srr.EIVPostalZoneS = delivery.EIVPostalZoneS;
                                    srr.EIVCityNameS = delivery.EIVCityNameS;
                                    if(delivery.EIVStateS != null)
                                    {
                                        srr.EIVStateS = srr.Session.FindObject<vwState>(CriteriaOperator.Parse("Code = ?", delivery.EIVStateS.Code));
                                    }
                                    if(delivery.EIVCountryS != null)
                                    {
                                        srr.EIVCountryS = srr.Session.FindObject<vwCountry>(CriteriaOperator.Parse("Code = ?", delivery.EIVCountryS.Code));
                                    }
                                }
                            }
                            // End ver 1.0.18

                            srr.CustomerName = dtl.CardName;
                            SalesReturnRequestDetails newsrritem = ObjectSpace.CreateObject<SalesReturnRequestDetails>();

                            newsrritem.ItemCode = newsrritem.Session.GetObjectByKey<vwItemMasters>(dtl.ItemCode);
                            newsrritem.ItemDesc = dtl.ItemDescrip;
                            if (dtl.WhsCode != null)
                            {
                                newsrritem.Warehouse = newsrritem.Session.GetObjectByKey<vwWarehouse>(dtl.WhsCode.WarehouseCode);
                            }
                            newsrritem.Quantity = dtl.OpenQty;
                            newsrritem.RtnQuantity = dtl.OpenQty;
                            newsrritem.Price = dtl.UnitPrice;
                            newsrritem.UnitCost = dtl.UnitCost;
                            newsrritem.BaseDoc = dtl.BaseEntry.ToString();
                            newsrritem.BaseId = dtl.BaseLine.ToString();
                            // Start ver 1.0.18
                            if (dtl.U_EIV_Classification != null)
                            {
                                newsrritem.EIVClassification = newsrritem.Session.FindObject<vwEIVClass>
                                (CriteriaOperator.Parse("Code = ?", dtl.U_EIV_Classification));
                            }
                            // End ver 1.0.18
                            srr.SalesReturnRequestDetails.Add(newsrritem);

                            showMsg("Success", "Copy Success.", InformationType.Success);
                        }

                        srr.Reference = invoiceno;
                        if (srr.DocNum == null)
                        {
                            string docprefix = genCon.GetDocPrefix();
                            srr.DocNum = genCon.GenerateDocNum(DocTypeList.SRR, ObjectSpace, TransferType.NA, 0, docprefix);
                        }

                        ObjectSpace.CommitChanges();
                        ObjectSpace.Refresh();
                    //}
                }
                catch (Exception)
                {
                    showMsg("Fail", "Copy Fail.", InformationType.Error);
                }
            }
        }

        private void SRRCopyFromInv_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            SalesReturnRequests srr = (SalesReturnRequests)View.CurrentObject;

            var os = Application.CreateObjectSpace();
            var viewId = Application.FindListViewId(typeof(vwInvoice));
            var cs = Application.CreateCollectionSource(os, typeof(vwInvoice), viewId);
            if (srr.Customer != null)
            {
                cs.Criteria["CardCode"] = new BinaryOperator("CardCode", srr.Customer.BPCode);
            }
            else
            {
                cs.Criteria["CardCode"] = new BinaryOperator("CardCode", "");
            }

            var lv1 = Application.CreateListView(viewId, cs, true);
            e.View = lv1;
        }

        private void SubmitSRR_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            SalesReturnRequests selectedObject = (SalesReturnRequests)e.CurrentObject;
            StringParameters p = (StringParameters)e.PopupWindow.View.CurrentObject;
            if (p.IsErr) return;
            SqlConnection conn = new SqlConnection(genCon.getConnectionString());

            if (selectedObject.IsValid3 == true)
            {
                showMsg("Error", "Unit Cost cannot zero.", InformationType.Error);
                return;
            }

            if (selectedObject.IsValid == true)
            {
                if (selectedObject.IsValid1 == false)
                {
                    if (selectedObject.IsValid2 == false)
                    {
                        selectedObject.Status = DocStatus.Submitted;
                        SalesReturnRequestDocTrail ds = ObjectSpace.CreateObject<SalesReturnRequestDocTrail>();
                        ds.DocStatus = DocStatus.Submitted;
                        ds.DocRemarks = p.ParamString;
                        selectedObject.SalesReturnRequestDocTrail.Add(ds);

                        ObjectSpace.CommitChanges();
                        ObjectSpace.Refresh();

                        #region Get approval
                        List<string> ToEmails = new List<string>();
                        string emailbody = "";
                        string emailsubject = "";
                        string emailaddress = "";
                        Guid emailuser;
                        DateTime emailtime = DateTime.Now;

                        string getapproval = "EXEC sp_GetApproval '" + selectedObject.CreateUser.Oid + "', '" + selectedObject.Oid + "', 'SalesReturnRequest'";
                        if (conn.State == ConnectionState.Open)
                        {
                            conn.Close();
                        }
                        conn.Open();
                        SqlCommand cmd = new SqlCommand(getapproval, conn);
                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            if (reader.GetString(1) != "")
                            {
                                emailbody = "Dear Sir/Madam, " + System.Environment.NewLine + System.Environment.NewLine +
                                       reader.GetString(3) + System.Environment.NewLine + GeneralSettings.appurl + reader.GetString(2) +
                                       System.Environment.NewLine + System.Environment.NewLine;

                                emailsubject = "Sales Return Request Approval";
                                emailaddress = reader.GetString(1);
                                emailuser = reader.GetGuid(0);

                                ToEmails.Add(emailaddress);
                            }

                        }
                        cmd.Dispose();
                        conn.Close();

                        if (ToEmails.Count > 0)
                        {
                            if (genCon.SendEmail(emailsubject, emailbody, ToEmails) == 1)
                            {
                            }
                        }

                        #endregion

                        IObjectSpace os = Application.CreateObjectSpace();
                        SalesReturnRequests trx = os.FindObject<SalesReturnRequests>(new BinaryOperator("Oid", selectedObject.Oid));
                        openNewView(os, trx, ViewEditMode.View);
                        showMsg("Successful", "Submit Done.", InformationType.Success);
                    }
                    else
                    {
                        showMsg("Error", "Return qty cannot be zero.", InformationType.Error);
                    }
                }
                else
                {
                    showMsg("Error", "Please fill in reason code.", InformationType.Error);
                }
            }
            else
            {
                showMsg("Error", "No Content.", InformationType.Error);
            }
        }

        private void SubmitSRR_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            var os = Application.CreateObjectSpace(typeof(StringParameters));
            StringParameters message = os.CreateObject<StringParameters>();

            DetailView dv = Application.CreateDetailView(os, message);
            dv.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;
            ((StringParameters)dv.CurrentObject).IsErr = false;
            ((StringParameters)dv.CurrentObject).ActionMessage = "Press OK to CONFIRM the action and SAVE, else press Cancel.";

            e.View = dv;
        }

        private void CancelSRR_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            SalesReturnRequests selectedObject = (SalesReturnRequests)e.CurrentObject;
            StringParameters p = (StringParameters)e.PopupWindow.View.CurrentObject;
            if (p.IsErr) return;

            selectedObject.Status = DocStatus.Cancelled;
            SalesReturnRequestDocTrail ds = ObjectSpace.CreateObject<SalesReturnRequestDocTrail>();
            ds.DocStatus = DocStatus.Cancelled;
            ds.DocRemarks = p.ParamString;
            selectedObject.SalesReturnRequestDocTrail.Add(ds);

            ObjectSpace.CommitChanges();
            ObjectSpace.Refresh();

            IObjectSpace os = Application.CreateObjectSpace();
            SalesReturnRequests trx = os.FindObject<SalesReturnRequests>(new BinaryOperator("Oid", selectedObject.Oid));
            openNewView(os, trx, ViewEditMode.View);
            showMsg("Successful", "Cancel Done.", InformationType.Success);
        }

        private void CancelSRR_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            var os = Application.CreateObjectSpace(typeof(StringParameters));
            StringParameters message = os.CreateObject<StringParameters>();

            DetailView dv = Application.CreateDetailView(os, message);
            dv.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;
            ((StringParameters)dv.CurrentObject).IsErr = false;
            ((StringParameters)dv.CurrentObject).ActionMessage = "Press OK to CONFIRM the action and SAVE, else press Cancel.";

            e.View = dv;
        }

        private void SRRPrintLabel_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {

        }

        private void SRRPrintLabel_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {

        }

        private void PreviewSRR_Execute(object sender, SimpleActionExecuteEventArgs e)
        {

        }

        private void SRRInquiryItem_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            ObjectSpace.CommitChanges();
            ObjectSpace.Refresh();
        }

        private void SRRInquiryItem_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            SalesReturnRequests trx = (SalesReturnRequests)View.CurrentObject;

            if (trx.DocNum == null)
            {
                string docprefix = genCon.GetDocPrefix();
                trx.DocNum = genCon.GenerateDocNum(DocTypeList.SRR, ObjectSpace, TransferType.NA, 0, docprefix);
            }

            ObjectSpace.CommitChanges();
            ObjectSpace.Refresh();

            IObjectSpace os = Application.CreateObjectSpace();
            SalesReturnRequests srr = os.FindObject<SalesReturnRequests>(new BinaryOperator("Oid", trx.Oid));

            IObjectSpace inqos = Application.CreateObjectSpace();
            ItemInquiry addnew = inqos.CreateObject<ItemInquiry>();

            DetailView dv = Application.CreateDetailView(inqos, addnew, true);
            dv.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;
            ((ItemInquiry)dv.CurrentObject).Cart = srr.DocNum;
            ((ItemInquiry)dv.CurrentObject).DocType = DocTypeList.SRR;
            ((ItemInquiry)dv.CurrentObject).CardCode = ((ItemInquiry)dv.CurrentObject).Session.GetObjectByKey<vwBusniessPartner>
                (trx.Customer.BPCode);

            ItemInquiryDefault defaultdata = inqos.FindObject<ItemInquiryDefault>(CriteriaOperator.Parse("DocType = ? and IsActive= ?",
                DocTypeList.SRR, "True"));

            if (defaultdata != null)
            {
                if (defaultdata.PriceList1 != null)
                {
                    ((ItemInquiry)dv.CurrentObject).PriceList1 = ((ItemInquiry)dv.CurrentObject).Session.GetObjectByKey<vwPriceList>
                        (defaultdata.PriceList1.ListNum);
                }
                if (defaultdata.PriceList2 != null)
                {
                    ((ItemInquiry)dv.CurrentObject).PriceList2 = ((ItemInquiry)dv.CurrentObject).Session.GetObjectByKey<vwPriceList>
                        (defaultdata.PriceList2.ListNum);
                }
                if (defaultdata.PriceList3 != null)
                {
                    ((ItemInquiry)dv.CurrentObject).PriceList3 = ((ItemInquiry)dv.CurrentObject).Session.GetObjectByKey<vwPriceList>
                        (defaultdata.PriceList3.ListNum);
                }
                if (defaultdata.PriceList4 != null)
                {
                    ((ItemInquiry)dv.CurrentObject).PriceList4 = ((ItemInquiry)dv.CurrentObject).Session.GetObjectByKey<vwPriceList>
                        (defaultdata.PriceList4.ListNum);
                }
                if (defaultdata.Stock1 != null)
                {
                    ((ItemInquiry)dv.CurrentObject).Stock1 = ((ItemInquiry)dv.CurrentObject).Session.GetObjectByKey<vwWarehouse>
                        (defaultdata.Stock1.WarehouseCode);
                }
                if (defaultdata.Stock2 != null)
                {
                    ((ItemInquiry)dv.CurrentObject).Stock2 = ((ItemInquiry)dv.CurrentObject).Session.GetObjectByKey<vwWarehouse>
                        (defaultdata.Stock2.WarehouseCode);
                }
                // Start ver 1.0.8
                if (defaultdata.Stock3 != null)
                {
                    ((ItemInquiry)dv.CurrentObject).Stock3 = ((ItemInquiry)dv.CurrentObject).Session.GetObjectByKey<vwWarehouse>
                        (defaultdata.Stock3.WarehouseCode);
                }
                if (defaultdata.Stock4 != null)
                {
                    ((ItemInquiry)dv.CurrentObject).Stock4 = ((ItemInquiry)dv.CurrentObject).Session.GetObjectByKey<vwWarehouse>
                        (defaultdata.Stock4.WarehouseCode);
                }
                // End ver 1.0.8
            }

            inqos.CommitChanges();
            inqos.Refresh();

            e.DialogController.CancelAction.Active["NothingToCancel"] = false;
            e.DialogController.AcceptAction.ActionMeaning = ActionMeaning.Unknown;
            e.Maximized = true;

            e.View = dv;
        }

        private void ReviewAppSRR_Execute(object sender, SimpleActionExecuteEventArgs e)
        {

        }

        private void ApproveAppSRR_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            SalesReturnRequests selectedObject = (SalesReturnRequests)e.CurrentObject;

            selectedObject.AppStatus = ApprovalStatusType.Approved;

            SalesReturnRequestDocTrail ds = ObjectSpace.CreateObject<SalesReturnRequestDocTrail>();
            ds.DocStatus = DocStatus.Submitted;
            ds.DocRemarks = "Approved";
            selectedObject.SalesReturnRequestDocTrail.Add(ds);

            SalesReturnRequestAppStatus apps = ObjectSpace.CreateObject<SalesReturnRequestAppStatus>();
            apps.AppStatus = ApprovalStatusType.Approved;
            apps.AppRemarks = "Approved";
            selectedObject.SalesReturnRequestAppStatus.Add(apps);

            ObjectSpace.CommitChanges();
            ObjectSpace.Refresh();

            showMsg("Successful", "Approve Done.", InformationType.Success);
        }

        private void RejectAppSRR_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            SalesReturnRequests selectedObject = (SalesReturnRequests)e.CurrentObject;

            selectedObject.AppStatus = ApprovalStatusType.Rejected;

            SalesReturnRequestDocTrail ds = ObjectSpace.CreateObject<SalesReturnRequestDocTrail>();
            ds.DocStatus = DocStatus.Submitted;
            ds.DocRemarks = "Rejected";
            selectedObject.SalesReturnRequestDocTrail.Add(ds);

            SalesReturnRequestAppStatus apps = ObjectSpace.CreateObject<SalesReturnRequestAppStatus>();
            apps.AppStatus = ApprovalStatusType.Rejected;
            apps.AppRemarks = "Rejected";
            selectedObject.SalesReturnRequestAppStatus.Add(apps);

            ObjectSpace.CommitChanges();
            ObjectSpace.Refresh();

            showMsg("Successful", "Reject Done.", InformationType.Success);
        }

        private void SRRCopyToSR_Execute(object sender, SimpleActionExecuteEventArgs e)
        {
            try
            {
                SalesReturnRequests srr = (SalesReturnRequests)View.CurrentObject;
                IObjectSpace os = Application.CreateObjectSpace();
                SalesReturns newsr = os.CreateObject<SalesReturns>();

                if (srr.Customer != null)
                {
                    newsr.Customer = newsr.Session.GetObjectByKey<vwBusniessPartner>(srr.Customer.BPCode);
                }
                newsr.CustomerName = srr.CustomerName;
                if (srr.Salesperson != null)
                {
                    newsr.Salesperson = newsr.Session.GetObjectByKey<vwSalesPerson>(srr.Salesperson.SlpCode);
                }
                if (srr.Transporter != null)
                {
                    newsr.Transporter = newsr.Session.GetObjectByKey<vwTransporter>(srr.Transporter.TransporterID);
                }
                if (srr.BillingAddress != null)
                {
                    newsr.BillingAddress = newsr.Session.GetObjectByKey<vwBillingAddress>(srr.BillingAddress.PriKey);
                }
                newsr.BillingAddressfield = srr.BillingAddressfield;
                if (srr.ShippingAddress != null)
                {
                    newsr.ShippingAddress = newsr.Session.GetObjectByKey<vwShippingAddress>(srr.ShippingAddress.PriKey);
                }
                newsr.ShippingAddressfield = srr.ShippingAddressfield;
                newsr.Reference = srr.Reference;
                newsr.Remarks = srr.Remarks;
                if (srr.ReasonCode != null)
                {
                    newsr.ReasonCode = newsr.Session.GetObjectByKey<vwReasonCode>(srr.ReasonCode.Prikey);
                }
                // Start ver 1.0.10
                newsr.PaymentMethod = srr.PaymentMethod;
                // End ver 1.0.10
                // Start ver 1.0.19
                newsr.BaseDoc = srr.DocNum;
                // End ver 1.0.19
                // Start ver 1.0.18
                // Buyer
                if (srr.EIVConsolidate != null)
                {
                    newsr.EIVConsolidate = newsr.Session.FindObject<vwYesNo>(CriteriaOperator.Parse("Code = ?", srr.EIVConsolidate.Code));
                }
                if (srr.EIVType != null)
                {
                    newsr.EIVType = newsr.Session.FindObject<vwEIVType>(CriteriaOperator.Parse("Code = ?", srr.EIVType.Code));
                }
                if (srr.EIVFreqSync != null)
                {
                    newsr.EIVFreqSync = newsr.Session.FindObject<vwEIVFreqSync>(CriteriaOperator.Parse("Code = ?", srr.EIVFreqSync.Code));
                }
                newsr.EIVBuyerName = srr.EIVBuyerName;
                newsr.EIVBuyerTIN = srr.EIVBuyerTIN;
                newsr.EIVBuyerRegNum = srr.EIVBuyerRegNum;
                if (srr.EIVBuyerRegTyp != null)
                {
                    newsr.EIVBuyerRegTyp = newsr.Session.FindObject<vwEIVRegType>(CriteriaOperator.Parse("Code = ?", srr.EIVBuyerRegTyp.Code));
                }
                newsr.EIVBuyerSSTRegNum = srr.EIVBuyerSSTRegNum;
                newsr.EIVBuyerEmail = srr.EIVBuyerEmail;
                newsr.EIVBuyerContact = srr.EIVBuyerContact;
                newsr.EIVAddressLine1B = srr.EIVAddressLine1B;
                newsr.EIVAddressLine2B = srr.EIVAddressLine2B;
                newsr.EIVAddressLine3B = srr.EIVAddressLine3B;
                newsr.EIVPostalZoneB = srr.EIVPostalZoneB;
                newsr.EIVCityNameB = srr.EIVCityNameB;
                if (srr.EIVStateB != null)
                {
                    newsr.EIVStateB = newsr.Session.FindObject<vwState>(CriteriaOperator.Parse("Code = ?", srr.EIVStateB.Code));
                }
                if (srr.EIVCountryB != null)
                {
                    newsr.EIVCountryB = newsr.Session.FindObject<vwCountry>(CriteriaOperator.Parse("Code = ?", srr.EIVCountryB.Code));
                }
                //Recipient
                newsr.EIVShippingName = srr.EIVShippingName;
                newsr.EIVShippingTin = srr.EIVShippingTin;
                newsr.EIVShippingRegNum = srr.EIVShippingRegNum;
                if (srr.EIVShippingRegTyp != null)
                {
                    newsr.EIVShippingRegTyp = newsr.Session.FindObject<vwEIVRegType>(CriteriaOperator.Parse("Code = ?", srr.EIVShippingRegTyp.Code));
                }
                newsr.EIVAddressLine1S = srr.EIVAddressLine1S;
                newsr.EIVAddressLine2S = srr.EIVAddressLine2S;
                newsr.EIVAddressLine3S = srr.EIVAddressLine3S;
                newsr.EIVPostalZoneS = srr.EIVPostalZoneS;
                newsr.EIVCityNameS = srr.EIVCityNameS;
                if (srr.EIVStateS != null)
                {
                    newsr.EIVStateS = newsr.Session.FindObject<vwState>(CriteriaOperator.Parse("Code = ?", srr.EIVStateS.Code));
                }
                if (srr.EIVCountryS != null)
                {
                    newsr.EIVCountryS = newsr.Session.FindObject<vwCountry>(CriteriaOperator.Parse("Code = ?", srr.EIVCountryS.Code));
                }
                // End ver 1.0.18

                foreach (SalesReturnRequestDetails dtl in srr.SalesReturnRequestDetails)
                {
                    SalesReturnDetails newsrdetails = os.CreateObject<SalesReturnDetails>();

                    newsrdetails.ItemCode = newsrdetails.Session.GetObjectByKey<vwItemMasters>(dtl.ItemCode.ItemCode);
                    newsrdetails.ItemDesc = dtl.ItemDesc;
                    newsrdetails.ItemDetails = dtl.ItemDetails;
                    newsrdetails.DefBarcode = dtl.DefBarcode;
                    newsrdetails.UOM = dtl.UOM;
                    if (dtl.ReasonCode != null)
                    {
                        newsrdetails.ReasonCode = newsrdetails.Session.GetObjectByKey<vwReasonCode>(dtl.ReasonCode.Prikey);
                    }
                    if (dtl.Warehouse != null)
                    {
                        newsrdetails.Warehouse = newsrdetails.Session.GetObjectByKey<vwWarehouse>(dtl.Warehouse.WarehouseCode);
                    }
                    if (dtl.Bin != null)
                    {
                        newsrdetails.Bin = newsrdetails.Session.GetObjectByKey<vwBin>(dtl.Bin.BinCode);
                    }
                    newsrdetails.Quantity = dtl.RtnQuantity;
                    newsrdetails.RtnQuantity = dtl.RtnQuantity;
                    newsrdetails.UnitCost = dtl.UnitCost;
                    newsrdetails.Price = dtl.Price;
                    newsrdetails.BaseDoc = srr.DocNum;
                    newsrdetails.BaseId = dtl.Oid.ToString();
                    newsrdetails.InvoiceDoc = dtl.BaseDoc;
                    // Start ver 1.0.18
                    if (dtl.EIVClassification != null)
                    {
                        newsrdetails.EIVClassification = newsrdetails.Session.FindObject<vwEIVClass>(CriteriaOperator.Parse("Code = ?", dtl.EIVClassification.Code));
                    }
                    // End ver 1.0.18
                    newsr.SalesReturnDetails.Add(newsrdetails);
                }

                ShowViewParameters svp = new ShowViewParameters();
                DetailView dv = Application.CreateDetailView(os, newsr);
                dv.ViewEditMode = ViewEditMode.Edit;
                dv.IsRoot = true;
                svp.CreatedView = dv;

                Application.ShowViewStrategy.ShowView(svp, new ShowViewSource(null, null));
                showMsg("Success", "Copy Success.", InformationType.Success);
            }
            catch (Exception)
            {
                showMsg("Fail", "Copy Fail.", InformationType.Error);
            }
        }

        private void ApproveAppSRR_Pop_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
        {
            if (e.SelectedObjects.Count > 1)
            {
                int totaldoc = 0;

                SqlConnection conn = new SqlConnection(genCon.getConnectionString());
                ApplicationUser user = (ApplicationUser)SecuritySystem.CurrentUser;
                ApprovalParameters p = (ApprovalParameters)e.PopupWindow.View.CurrentObject;

                if (p.IsValid == false)
                {
                    try
                    {
                        foreach (SalesReturnRequests dtl in e.SelectedObjects)
                        {
                            IObjectSpace sos = Application.CreateObjectSpace();
                            SalesReturnRequests srr = sos.FindObject<SalesReturnRequests>(new BinaryOperator("Oid", dtl.Oid));

                            // Start ver 1.0.22
                            bool periodlock = false;
                            string getpostingperiod = "SELECT * From vwPostingPeriod WHERE F_RefDate <= '" + srr.PostingDate.Date.ToString("yyyy-MM-dd") + "' " +
                                "AND T_RefDate >= '" + srr.PostingDate.Date.ToString("yyyy-MM-dd") + "'";
                            if (conn.State == ConnectionState.Open)
                            {
                                conn.Close();
                            }
                            conn.Open();
                            SqlCommand cmdperiod = new SqlCommand(getpostingperiod, conn);
                            SqlDataReader readerperiod = cmdperiod.ExecuteReader();
                            while (readerperiod.Read())
                            {
                                if (readerperiod.GetString(3) != "N")
                                {
                                    periodlock = true;
                                }
                            }
                            cmdperiod.Dispose();
                            conn.Close();
                            // End ver 1.0.22

                            // Start ver 1.0.22
                            if (periodlock == false)
                            {
                            // End ver 1.0.22
                                ApprovalStatusType appstatus = ApprovalStatusType.Required_Approval;

                                if (appstatus == ApprovalStatusType.Not_Applicable)
                                    appstatus = ApprovalStatusType.Required_Approval;

                                if (p.IsErr) return;
                                if (appstatus == ApprovalStatusType.Required_Approval && p.AppStatus == ApprovalActions.NA)
                                {
                                    showMsg("Failed", "Same Approval Status is not allowed.", InformationType.Error);
                                    return;
                                }
                                else if (appstatus == ApprovalStatusType.Approved && p.AppStatus == ApprovalActions.Yes)
                                {
                                    showMsg("Failed", "Same Approval Status is not allowed.", InformationType.Error);
                                    return;
                                }
                                else if (appstatus == ApprovalStatusType.Rejected && p.AppStatus == ApprovalActions.No)
                                {
                                    showMsg("Failed", "Same Approval Status is not allowed.", InformationType.Error);
                                    return;
                                }
                                if (p.AppStatus == ApprovalActions.NA)
                                {
                                    appstatus = ApprovalStatusType.Required_Approval;
                                }
                                if (p.AppStatus == ApprovalActions.Yes)
                                {
                                    appstatus = ApprovalStatusType.Approved;
                                }
                                if (p.AppStatus == ApprovalActions.No)
                                {
                                    appstatus = ApprovalStatusType.Rejected;
                                }

                                SalesReturnRequestAppStatus ds = sos.CreateObject<SalesReturnRequestAppStatus>();
                                ds.SalesReturnRequests = sos.GetObjectByKey<SalesReturnRequests>(srr.Oid);
                                ds.AppStatus = appstatus;
                                if (appstatus == ApprovalStatusType.Rejected)
                                {
                                    //sq.Status = DocStatus.New;
                                    ds.AppRemarks =
                                        System.Environment.NewLine + "(Reject User: " + user.Staff.StaffName + ")" +
                                        System.Environment.NewLine + "(Reason: Approval Rejected) - " + p.ParamString;
                                    ds.CreateUser = sos.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                                }
                                else
                                {
                                    ds.AppRemarks = System.Environment.NewLine + "(Approved User: " + user.Staff.StaffName + ") - " + p.ParamString;
                                }
                                srr.SalesReturnRequestAppStatus.Add(ds);

                                sos.CommitChanges();
                                sos.Refresh();

                                totaldoc++;

                                #region approval

                                List<string> ToEmails = new List<string>();
                                string emailbody = "";
                                string emailsubject = "";
                                string emailaddress = "";
                                Guid emailuser;
                                DateTime emailtime = DateTime.Now;

                                string getapproval = "EXEC sp_Approval '" + user.UserName + "', '" + srr.Oid + "', 'SalesReturnRequests', " + ((int)appstatus);
                                if (conn.State == ConnectionState.Open)
                                {
                                    conn.Close();
                                }
                                conn.Open();
                                SqlCommand cmd = new SqlCommand(getapproval, conn);
                                SqlDataReader reader = cmd.ExecuteReader();
                                while (reader.Read())
                                {
                                    if (reader.GetString(1) != "")
                                    {
                                        emailbody = "Dear Sir/Madam, " + System.Environment.NewLine + System.Environment.NewLine +
                                                  reader.GetString(3) + System.Environment.NewLine + GeneralSettings.appurl + reader.GetString(2) +
                                                  System.Environment.NewLine + System.Environment.NewLine;

                                        if (appstatus == ApprovalStatusType.Approved)
                                            emailsubject = "Sales Return Request Approval";
                                        else if (appstatus == ApprovalStatusType.Rejected)
                                            emailsubject = "Sales Return Request Approval Rejected";

                                        emailaddress = reader.GetString(1);
                                        emailuser = reader.GetGuid(0);

                                        ToEmails.Add(emailaddress);
                                    }
                                }
                                cmd.Dispose();
                                conn.Close();

                                if (ToEmails.Count > 0)
                                {
                                    if (genCon.SendEmail(emailsubject, emailbody, ToEmails) == 1)
                                    {
                                    }
                                }
                                #endregion

                            // Start ver 1.0.22
                            }
                            // End ver 1.0.22

                            //ObjectSpace.CommitChanges(); //This line persists created object(s).
                            //ObjectSpace.Refresh();
                        }

                        showMsg("Info", "Total Document : " + totaldoc + " Approval Done.", InformationType.Info);

                        ((DevExpress.ExpressApp.ListView)Frame.View).ObjectSpace.Refresh();
                    }
                    catch (Exception ex)
                    {
                        showMsg("Error", ex.Message, InformationType.Error);
                    }
                }
            }
            else if (e.SelectedObjects.Count == 1)
            {
                SqlConnection conn = new SqlConnection(genCon.getConnectionString());
                ApplicationUser user = (ApplicationUser)SecuritySystem.CurrentUser;
                ApprovalParameters p = (ApprovalParameters)e.PopupWindow.View.CurrentObject;

                if (p.IsValid == false)
                {
                    foreach (SalesReturnRequests dtl in e.SelectedObjects)
                    {
                        IObjectSpace sos = Application.CreateObjectSpace();
                        SalesReturnRequests srr = sos.FindObject<SalesReturnRequests>(new BinaryOperator("Oid", dtl.Oid));

                        // Start ver 1.0.22
                        string getpostingperiod = "SELECT * From vwPostingPeriod WHERE F_RefDate <= '" + srr.PostingDate.Date.ToString("yyyy-MM-dd") + "' " +
                            "AND T_RefDate >= '" + srr.PostingDate.Date.ToString("yyyy-MM-dd") + "'";
                        if (conn.State == ConnectionState.Open)
                        {
                            conn.Close();
                        }
                        conn.Open();
                        SqlCommand cmdperiod = new SqlCommand(getpostingperiod, conn);
                        SqlDataReader readerperiod = cmdperiod.ExecuteReader();
                        while (readerperiod.Read())
                        {
                            if (readerperiod.GetString(3) != "N")
                            {
                                showMsg("Error", "Posting period locked.", InformationType.Error);
                                return;
                            }
                        }
                        cmdperiod.Dispose();
                        conn.Close();
                        // End ver 1.0.22

                        ApprovalStatusType appstatus = ApprovalStatusType.Required_Approval;

                        if (appstatus == ApprovalStatusType.Not_Applicable)
                            appstatus = ApprovalStatusType.Required_Approval;


                        if (p.IsErr) return;
                        if (appstatus == ApprovalStatusType.Required_Approval && p.AppStatus == ApprovalActions.NA)
                        {
                            showMsg("Failed", "Same Approval Status is not allowed.", InformationType.Error);
                            return;
                        }
                        else if (appstatus == ApprovalStatusType.Approved && p.AppStatus == ApprovalActions.Yes)
                        {
                            showMsg("Failed", "Same Approval Status is not allowed.", InformationType.Error);
                            return;
                        }
                        else if (appstatus == ApprovalStatusType.Rejected && p.AppStatus == ApprovalActions.No)
                        {
                            showMsg("Failed", "Same Approval Status is not allowed.", InformationType.Error);
                            return;
                        }
                        if (p.AppStatus == ApprovalActions.NA)
                        {
                            appstatus = ApprovalStatusType.Required_Approval;
                        }
                        if (p.AppStatus == ApprovalActions.Yes)
                        {
                            appstatus = ApprovalStatusType.Approved;
                        }
                        if (p.AppStatus == ApprovalActions.No)
                        {
                            appstatus = ApprovalStatusType.Rejected;
                        }

                        SalesReturnRequestAppStatus ds = sos.CreateObject<SalesReturnRequestAppStatus>();
                        ds.SalesReturnRequests = sos.GetObjectByKey<SalesReturnRequests>(srr.Oid);
                        ds.AppStatus = appstatus;
                        if (appstatus == ApprovalStatusType.Rejected)
                        {
                            ds.AppRemarks =
                                System.Environment.NewLine + "(Reject User: " + user.Staff.StaffName + ")" +
                                System.Environment.NewLine + "(Reason: Approval Rejected) - " + p.ParamString;
                            ds.CreateUser = sos.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                        }
                        else
                        {
                            ds.AppRemarks = System.Environment.NewLine + "(Approved User: " + user.Staff.StaffName + ") - " + p.ParamString;
                        }
                        srr.SalesReturnRequestAppStatus.Add(ds);

                        sos.CommitChanges();
                        sos.Refresh();

                        #region approval

                        List<string> ToEmails = new List<string>();
                        string emailbody = "";
                        string emailsubject = "";
                        string emailaddress = "";
                        Guid emailuser;
                        DateTime emailtime = DateTime.Now;

                        string getapproval = "EXEC sp_Approval '" + user.UserName + "', '" + srr.Oid + "', 'SalesReturnRequests', " + ((int)appstatus);
                        if (conn.State == ConnectionState.Open)
                        {
                            conn.Close();
                        }
                        conn.Open();
                        SqlCommand cmd = new SqlCommand(getapproval, conn);
                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            if (reader.GetString(1) != "")
                            {
                                emailbody = "Dear Sir/Madam, " + System.Environment.NewLine + System.Environment.NewLine +
                                      reader.GetString(3) + System.Environment.NewLine + GeneralSettings.appurl + reader.GetString(2) +
                                      System.Environment.NewLine + System.Environment.NewLine;

                                if (appstatus == ApprovalStatusType.Approved)
                                    emailsubject = "Sales Return Request Approval";
                                else if (appstatus == ApprovalStatusType.Rejected)
                                    emailsubject = "Sales Return Request Rejected";

                                emailaddress = reader.GetString(1);
                                emailuser = reader.GetGuid(0);

                                ToEmails.Add(emailaddress);
                            }
                        }
                        cmd.Dispose();
                        conn.Close();

                        if (ToEmails.Count > 0)
                        {
                            if (genCon.SendEmail(emailsubject, emailbody, ToEmails) == 1)
                            {
                            }
                        }
                        #endregion

                        IObjectSpace tos = Application.CreateObjectSpace();
                        SalesReturnRequests trx = tos.FindObject<SalesReturnRequests>(new BinaryOperator("Oid", srr.Oid));
                        openNewView(tos, trx, ViewEditMode.View);
                        showMsg("Successful", "Approve Done.", InformationType.Success);
                    }
                }
            }
            else
            {
                showMsg("Fail", "No Sales Return Request selected.", InformationType.Error);
            }
        }

        private void ApproveAppSRR_Pop_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
        {
            bool err = false;

            ApprovalStatusType appstatus = ApprovalStatusType.Required_Approval;

            IObjectSpace os = Application.CreateObjectSpace();
            DetailView dv = Application.CreateDetailView(os, os.CreateObject<ApprovalParameters>(), true);
            dv.ViewEditMode = DevExpress.ExpressApp.Editors.ViewEditMode.Edit;
            switch (appstatus)
            {
                case ApprovalStatusType.Required_Approval:
                    ((ApprovalParameters)dv.CurrentObject).AppStatus = ApprovalActions.NA;
                    break;
                case ApprovalStatusType.Approved:
                    ((ApprovalParameters)dv.CurrentObject).AppStatus = ApprovalActions.Yes;
                    break;
                case ApprovalStatusType.Rejected:
                    ((ApprovalParameters)dv.CurrentObject).AppStatus = ApprovalActions.No;
                    break;
            }
            ((ApprovalParameters)dv.CurrentObject).IsErr = err;
            ((ApprovalParameters)dv.CurrentObject).ActionMessage = "Press choose from approval status 'Yes or No' and press OK to CONFIRM the action and SAVE, else press Cancel.";

            e.View = dv;
        }
    }
}
