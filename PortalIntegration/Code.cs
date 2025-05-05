using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using DevExpress.XtraSpellChecker.Parser;
using SAPbobsCOM;
using StarLaiPortal.Module.BusinessObjects;
using StarLaiPortal.Module.BusinessObjects.Advanced_Shipment_Notice;
using StarLaiPortal.Module.BusinessObjects.Credit_Notes_Cancellation;
using StarLaiPortal.Module.BusinessObjects.Delivery_Order;
using StarLaiPortal.Module.BusinessObjects.GRN;
using StarLaiPortal.Module.BusinessObjects.Load;
using StarLaiPortal.Module.BusinessObjects.Pack_List;
using StarLaiPortal.Module.BusinessObjects.Pick_List;
using StarLaiPortal.Module.BusinessObjects.Purchase_Order;
using StarLaiPortal.Module.BusinessObjects.Purchase_Return;
using StarLaiPortal.Module.BusinessObjects.Sales_Order;
using StarLaiPortal.Module.BusinessObjects.Sales_Order_Collection;
using StarLaiPortal.Module.BusinessObjects.Sales_Quotation;
using StarLaiPortal.Module.BusinessObjects.Sales_Refund;
using StarLaiPortal.Module.BusinessObjects.Sales_Return;
using StarLaiPortal.Module.BusinessObjects.Setup;
using StarLaiPortal.Module.BusinessObjects.Stock_Adjustment;
using StarLaiPortal.Module.BusinessObjects.Stock_Count;
using StarLaiPortal.Module.BusinessObjects.View;
using StarLaiPortal.Module.BusinessObjects.Warehouse_Transfer;
using StarLaiPortal.Module.Controllers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static DevExpress.Data.Helpers.ExpressiveSortInfo;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

// 2023-07-28 add GRPO Correction ver 1.0.7
// 2023-08-16 temporary fix glaccount ver 1.0.8
// 2023-08-22 add cancel and close button ver 1.0.9
// 2023-04-09 fix speed issue ver 1.0.8.1
// 2023-09-25 add sales return ver 1.0.10
// 2023-10-30 Post FOC UDF ver 1.0.12
// 2023-11-02 Add stock count ver 1.0.12
// 2023-11-29 Recreate missing SO and DO ver 1.0.13
// 2023-12-04 enhance posting with adjustment instead of check oimn ver 1.0.13
// 2024-06-12 e-invoice - ver 1.0.18
// 2024-07-18 GRN post system date to posting date - ver 1.0.19
// 2025-01-23 Update Posted in picking - ver 1.0.22
// 2025-03-24 Picking post with systedate instead if screen date - ver 1.0.22
// 2025-04-03 Consolidate same SO but different packing - ver 1.0.22

namespace PortalIntegration
{
    class Code
    {
        private SortedDictionary<string, List<string>> logs = new SortedDictionary<string, List<string>>();
        SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DataSourceConnectionString"].ToString());
        SqlConnection conn1 = new SqlConnection(ConfigurationManager.ConnectionStrings["DataSourceConnectionString"].ToString());
        public Code(SecurityStrategyComplex security, IObjectSpaceProvider ObjectSpaceProvider)
        {
            logs.Clear();
            WriteLog("[Log]", "--------------------------------------------------------------------------------");
            WriteLog("[Log]", "Post Begin:[" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss tt") + "]");

            #region Connect to SAP  
            SAPCompany sap = new SAPCompany();
            if (sap.connectSAP())
            {
                WriteLog("[Log]", "Connected to SAP:[" + sap.oCom.CompanyName + "] Time:[" + DateTime.Now.ToString("hh:mm:ss tt") + "]");
            }
            else
            {
                WriteLog("[Error]", "SAP Connection:[" + sap.oCom.CompanyDB + "] Message:[" + sap.errMsg + "] Time:[" + DateTime.Now.ToString("hh: mm:ss tt") + "]");
                sap.oCom = null;
                goto EndApplication;
            }
            #endregion

            try
            {
                string temp = "";
                IObjectSpace ListObjectSpace = ObjectSpaceProvider.CreateObjectSpace();
                IObjectSpace securedObjectSpace = ObjectSpaceProvider.CreateObjectSpace();

                temp = ConfigurationManager.AppSettings["SOPost"].ToString().ToUpper();
                if (temp == "Y" || temp == "YES" || temp == "TRUE" || temp == "1")
                {
                    WriteLog("[INFO]", "--SO Posting Start--");

                    #region SO 
                    IList<SalesOrder> solist = ListObjectSpace.GetObjects<SalesOrder>
                    (CriteriaOperator.Parse("Sap = ?", 0));

                    foreach (SalesOrder dtlso in solist)
                    {
                        try
                        {
                            IObjectSpace soos = ObjectSpaceProvider.CreateObjectSpace();
                            SalesOrder soobj = soos.GetObjectByKey<SalesOrder>(dtlso.Oid);

                            #region Post SO
                            if (!sap.oCom.InTransaction) sap.oCom.StartTransaction();

                            int temppo = 0;

                            temppo = PostSOtoSAP(soobj, ObjectSpaceProvider, sap);
                            if (temppo == 1)
                            {
                                if (sap.oCom.InTransaction)
                                    sap.oCom.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit);

                                //soobj.Status = DocStatus.Post;
                                soobj.Sap = true;

                                SalesOrderDocStatus ds = soos.CreateObject<SalesOrderDocStatus>();
                                ds.CreateUser = soos.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                                ds.CreateDate = DateTime.Now;
                                ds.DocStatus = DocStatus.Post;
                                ds.DocRemarks = "Posted SAP";
                                soobj.SalesOrderDocStatus.Add(ds);

                                GC.Collect();
                            }
                            else if (temppo <= 0)
                            {
                                if (sap.oCom.InTransaction)
                                    sap.oCom.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack);

                                GC.Collect();
                            }
                            #endregion

                            soos.CommitChanges();
                        }
                        catch (Exception ex)
                        {
                            IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                            SalesOrder obj = osupdate.GetObjectByKey<SalesOrder>(dtlso.Oid);

                            SalesOrderDocStatus ds = osupdate.CreateObject<SalesOrderDocStatus>();
                            ds.CreateUser = osupdate.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                            ds.CreateDate = DateTime.Now;
                            ds.DocStatus = DocStatus.PendPost;
                            ds.DocRemarks = "SAP Error:" + ex.Message;
                            obj.SalesOrderDocStatus.Add(ds);

                            osupdate.CommitChanges();

                            WriteLog("[Error]", "Message: POST SO Failed - OID : " + dtlso.Oid + " (" + ex.Message + ")");
                        }
                    }
                    #endregion

                    WriteLog("[INFO]", "--SO Posting End--");
                }

                temp = ConfigurationManager.AppSettings["POPost"].ToString().ToUpper();
                if (temp == "Y" || temp == "YES" || temp == "TRUE" || temp == "1")
                {
                    WriteLog("[INFO]", "--PO Posting Start--");

                    #region PO 
                    IList<PurchaseOrders> polist = ListObjectSpace.GetObjects<PurchaseOrders>
                    (CriteriaOperator.Parse("Sap = ? and Status = ?", 0, 4));

                    foreach (PurchaseOrders dtlpo in polist)
                    {
                        try
                        {
                            IObjectSpace poos = ObjectSpaceProvider.CreateObjectSpace();
                            PurchaseOrders poobj = poos.GetObjectByKey<PurchaseOrders>(dtlpo.Oid);

                            #region Post PO
                            if (!sap.oCom.InTransaction) sap.oCom.StartTransaction();

                            int temppo = 0;

                            temppo = PostPOtoSAP(poobj, ObjectSpaceProvider, sap);
                            if (temppo == 1)
                            {
                                if (sap.oCom.InTransaction)
                                    sap.oCom.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit);

                                poobj.Status = DocStatus.Post;
                                poobj.Sap = true;

                                PurchaseOrderDocTrail ds = poos.CreateObject<PurchaseOrderDocTrail>();
                                ds.CreateUser = poos.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                                ds.CreateDate = DateTime.Now;
                                ds.DocStatus = DocStatus.Post;
                                ds.DocRemarks = "Posted SAP";
                                poobj.PurchaseOrderDocTrail.Add(ds);

                                GC.Collect();
                            }
                            else if (temppo <= 0)
                            {
                                if (sap.oCom.InTransaction)
                                    sap.oCom.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack);

                                GC.Collect();
                            }
                            #endregion

                            poos.CommitChanges();
                        }
                        catch (Exception ex)
                        {
                            IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                            PurchaseOrders obj = osupdate.GetObjectByKey<PurchaseOrders>(dtlpo.Oid);

                            PurchaseOrderDocTrail ds = osupdate.CreateObject<PurchaseOrderDocTrail>();
                            ds.CreateUser = osupdate.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                            ds.CreateDate = DateTime.Now;
                            ds.DocStatus = DocStatus.PendPost;
                            ds.DocRemarks = "SAP Error:" + ex.Message;
                            obj.PurchaseOrderDocTrail.Add(ds);

                            osupdate.CommitChanges();

                            WriteLog("[Error]", "Message: POST PO Failed - OID : " + dtlpo.Oid + " (" + ex.Message + ")");
                        }
                    }
                    #endregion

                    WriteLog("[INFO]", "--PO Posting End--");
                }

                temp = ConfigurationManager.AppSettings["GRNPost"].ToString().ToUpper();
                if (temp == "Y" || temp == "YES" || temp == "TRUE" || temp == "1")
                {
                    WriteLog("[INFO]", "--GRN Posting Start--");

                    #region GRN
                    IList<GRN> grnlist = ListObjectSpace.GetObjects<GRN>
                        (CriteriaOperator.Parse("Sap = ? and Status = ?", 0, 1));

                    foreach (GRN dtlgrn in grnlist)
                    {
                        try
                        {
                            IObjectSpace grnos = ObjectSpaceProvider.CreateObjectSpace();
                            GRN grnobj = grnos.GetObjectByKey<GRN>(dtlgrn.Oid);

                            #region Post GRN
                            if (!sap.oCom.InTransaction) sap.oCom.StartTransaction();

                            int tempgrn = 0;

                            tempgrn = PostGRNtoSAP(grnobj, ObjectSpaceProvider, sap);
                            if (tempgrn == 1)
                            {
                                if (sap.oCom.InTransaction)
                                    sap.oCom.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit);

                                grnobj.Sap = true;
                                grnobj.Status = DocStatus.Post;

                                GRNDocTrail ds = grnos.CreateObject<GRNDocTrail>();
                                ds.CreateUser = grnos.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                                ds.CreateDate = DateTime.Now;
                                ds.DocStatus = DocStatus.Post;
                                ds.DocRemarks = "Posted SAP";
                                grnobj.GRNDocTrail.Add(ds);

                                GC.Collect();
                            }
                            else if (tempgrn <= 0)
                            {
                                if (sap.oCom.InTransaction)
                                    sap.oCom.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack);

                                GC.Collect();
                            }
                            #endregion

                            grnos.CommitChanges();
                        }
                        catch (Exception ex)
                        {
                            IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                            GRN obj = osupdate.GetObjectByKey<GRN>(dtlgrn.Oid);

                            GRNDocTrail ds = osupdate.CreateObject<GRNDocTrail>();
                            ds.CreateUser = osupdate.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                            ds.CreateDate = DateTime.Now;
                            ds.DocStatus = DocStatus.PendPost;
                            ds.DocRemarks = "SAP Error:" + ex.Message;
                            obj.GRNDocTrail.Add(ds);

                            osupdate.CommitChanges();

                            WriteLog("[Error]", "Message: GRN Post Failed - OID : " + dtlgrn.Oid + " (" + ex.Message + ")");
                        }
                    }
                    #endregion

                    WriteLog("[INFO]", "--GRN Posting End--");
                }

                temp = ConfigurationManager.AppSettings["PReturnPost"].ToString().ToUpper();
                if (temp == "Y" || temp == "YES" || temp == "TRUE" || temp == "1")
                {
                    WriteLog("[INFO]", "--Purchase Return Posting Start--");

                    #region Purchase Return
                    IList<PurchaseReturns> preturnlist = ListObjectSpace.GetObjects<PurchaseReturns>
                        (CriteriaOperator.Parse("Sap = ? and Status = ?", 0, 1));

                    foreach (PurchaseReturns dtlpreturn in preturnlist)
                    {
                        try
                        {
                            IObjectSpace pros = ObjectSpaceProvider.CreateObjectSpace();
                            PurchaseReturns probj = pros.GetObjectByKey<PurchaseReturns>(dtlpreturn.Oid);

                            #region Post Purchase Return
                            if (!sap.oCom.InTransaction) sap.oCom.StartTransaction();

                            int temppr = 0;

                            temppr = PostPReturntoSAP(probj, ObjectSpaceProvider, sap);
                            if (temppr == 1)
                            {
                                if (sap.oCom.InTransaction)
                                    sap.oCom.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit);

                                probj.Sap = true;
                                probj.Status = DocStatus.Post;

                                PurchaseReturnDocTrail ds = pros.CreateObject<PurchaseReturnDocTrail>();
                                ds.CreateUser = pros.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                                ds.CreateDate = DateTime.Now;
                                ds.DocStatus = DocStatus.Post;
                                ds.DocRemarks = "Posted SAP";
                                probj.PurchaseReturnDocTrail.Add(ds);

                                GC.Collect();
                            }
                            else if (temppr <= 0)
                            {
                                if (sap.oCom.InTransaction)
                                    sap.oCom.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack);

                                GC.Collect();
                            }
                            #endregion

                            pros.CommitChanges();
                        }
                        catch (Exception ex)
                        {
                            IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                            PurchaseReturns obj = osupdate.GetObjectByKey<PurchaseReturns>(dtlpreturn.Oid);

                            PurchaseReturnDocTrail ds = osupdate.CreateObject<PurchaseReturnDocTrail>();
                            ds.CreateUser = osupdate.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                            ds.CreateDate = DateTime.Now;
                            ds.DocStatus = DocStatus.PendPost;
                            ds.DocRemarks = "SAP Error:" + ex.Message;
                            obj.PurchaseReturnDocTrail.Add(ds);

                            osupdate.CommitChanges();

                            WriteLog("[Error]", "Message: Purchase Return Post Failed - OID : " + dtlpreturn.Oid + " (" + ex.Message + ")");
                        }
                    }
                    #endregion

                    WriteLog("[INFO]", "--Purchase Return Posting End--");
                }

                temp = ConfigurationManager.AppSettings["SReturnPost"].ToString().ToUpper();
                if (temp == "Y" || temp == "YES" || temp == "TRUE" || temp == "1")
                {
                    WriteLog("[INFO]", "--Sales Return Posting Start--");

                    #region Sales Return
                    IList<SalesReturns> sreturnlist = ListObjectSpace.GetObjects<SalesReturns>
                        (CriteriaOperator.Parse("Sap = ? and Status = ?", 0, 1));

                    foreach (SalesReturns dtlsreturn in sreturnlist)
                    {
                        try
                        {
                            IObjectSpace pros = ObjectSpaceProvider.CreateObjectSpace();
                            SalesReturns srobj = pros.GetObjectByKey<SalesReturns>(dtlsreturn.Oid);

                            #region Post Sales Return
                            if (!sap.oCom.InTransaction) sap.oCom.StartTransaction();

                            int tempsr = 0;

                            tempsr = PostSReturntoSAP(srobj, ObjectSpaceProvider, sap);
                            if (tempsr == 1)
                            {
                                if (sap.oCom.InTransaction)
                                    sap.oCom.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit);

                                srobj.Sap = true;
                                srobj.Status = DocStatus.Post;

                                SalesReturnDocTrail ds = pros.CreateObject<SalesReturnDocTrail>();
                                ds.CreateUser = pros.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                                ds.CreateDate = DateTime.Now;
                                ds.DocStatus = DocStatus.Post;
                                ds.DocRemarks = "Posted SAP";
                                srobj.SalesReturnDocTrail.Add(ds);

                                GC.Collect();
                            }
                            else if (tempsr <= 0)
                            {
                                if (sap.oCom.InTransaction)
                                    sap.oCom.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack);

                                GC.Collect();
                            }
                            #endregion

                            pros.CommitChanges();
                        }
                        catch (Exception ex)
                        {
                            IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                            SalesReturns obj = osupdate.GetObjectByKey<SalesReturns>(dtlsreturn.Oid);

                            SalesReturnDocTrail ds = osupdate.CreateObject<SalesReturnDocTrail>();
                            ds.CreateUser = osupdate.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                            ds.CreateDate = DateTime.Now;
                            ds.DocStatus = DocStatus.PendPost;
                            ds.DocRemarks = "SAP Error:" + ex.Message;
                            obj.SalesReturnDocTrail.Add(ds);

                            osupdate.CommitChanges();

                            WriteLog("[Error]", "Message: Sales Return Post Failed - OID : " + dtlsreturn.Oid + " (" + ex.Message + ")");
                        }
                    }
                    #endregion

                    WriteLog("[INFO]", "--Sales Return Posting End--");
                }

                temp = ConfigurationManager.AppSettings["SAPost"].ToString().ToUpper();
                if (temp == "Y" || temp == "YES" || temp == "TRUE" || temp == "1")
                {
                    WriteLog("[INFO]", "--Stock Adjustment Posting Start--");

                    #region Stock Adjustment

                    IList<StockAdjustments> salist = ListObjectSpace.GetObjects<StockAdjustments>
                        (CriteriaOperator.Parse("Sap = ? and Status = ?", 0, 1));

                    foreach (StockAdjustments dtlsa in salist)
                    {
                        try
                        {
                            IObjectSpace saos = ObjectSpaceProvider.CreateObjectSpace();
                            StockAdjustments saobj = saos.GetObjectByKey<StockAdjustments>(dtlsa.Oid);

                            bool postfail = false;
                            bool positive = true;
                            bool negatif = true;
                            bool postissue = true;


                            foreach (StockAdjustmentDetails dtl in saobj.StockAdjustmentDetails)
                            {
                                if (dtl.Quantity > 0 && dtl.Sap == false)
                                {
                                    positive = false;
                                }

                                if (dtl.Quantity < 0 && dtl.Sap == false)
                                {
                                    negatif = false;
                                }
                            }

                            if (negatif == false)
                            {
                                #region Post Goods Issue
                                if (!sap.oCom.InTransaction) sap.oCom.StartTransaction();

                                int tempccout = 0;

                                tempccout = PostSAOuttoSAP(saobj, ObjectSpaceProvider, sap);
                                if (tempccout == 1)
                                {
                                    if (sap.oCom.InTransaction)
                                        sap.oCom.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit);

                                    foreach (StockAdjustmentDetails dtl in saobj.StockAdjustmentDetails)
                                    {
                                        if (dtl.Quantity < 0)
                                        {
                                            dtl.Sap = true;
                                        }
                                    }

                                    GC.Collect();
                                }
                                else if (tempccout <= 0)
                                {
                                    if (sap.oCom.InTransaction)
                                        sap.oCom.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack);

                                    postfail = true;
                                    postissue = false;

                                    GC.Collect();
                                }
                                #endregion

                                saos.CommitChanges();
                            }

                            if (positive == false && postissue == true)
                            {
                                #region Post Goods Receipt
                                if (!sap.oCom.InTransaction) sap.oCom.StartTransaction();

                                int tempccin = 0;

                                tempccin = PostSAINtoSAP(saobj, ObjectSpaceProvider, sap);
                                if (tempccin == 1)
                                {
                                    if (sap.oCom.InTransaction)
                                        sap.oCom.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit);

                                    foreach (StockAdjustmentDetails dtl in saobj.StockAdjustmentDetails)
                                    {
                                        if (dtl.Quantity > 0)
                                        {
                                            dtl.Sap = true;
                                        }
                                    }

                                    GC.Collect();
                                }
                                else if (tempccin <= 0)
                                {
                                    if (sap.oCom.InTransaction)
                                        sap.oCom.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack);

                                    postfail = true;

                                    GC.Collect();
                                }
                                #endregion
                            }

                            if (postfail == false)
                            {
                                saobj.Sap = true;

                                StockAdjustmentDocTrail ds = saos.CreateObject<StockAdjustmentDocTrail>();
                                ds.CreateUser = saos.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                                ds.CreateDate = DateTime.Now;
                                ds.DocStatus = DocStatus.Post;
                                ds.DocRemarks = "Posted SAP";
                                saobj.StockAdjustmentDocTrail.Add(ds);

                                saos.CommitChanges();
                            }
                        }
                        catch (Exception ex)
                        {
                            IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                            StockAdjustments obj = osupdate.GetObjectByKey<StockAdjustments>(dtlsa.Oid);

                            StockAdjustmentDocTrail ds = osupdate.CreateObject<StockAdjustmentDocTrail>();
                            ds.CreateUser = osupdate.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                            ds.CreateDate = DateTime.Now;
                            ds.DocStatus = DocStatus.PendPost;
                            ds.DocRemarks = "SAP Error:" + ex.Message;
                            obj.StockAdjustmentDocTrail.Add(ds);

                            osupdate.CommitChanges();

                            WriteLog("[Error]", "Message: Stock Adjustment Post Failed - OID : " + dtlsa.Oid + " (" + ex.Message + ")");
                        }
                    }
                    #endregion

                    WriteLog("[INFO]", "--Stock Adjustment Posting End--");
                }

                temp = ConfigurationManager.AppSettings["CNPost"].ToString().ToUpper();
                if (temp == "Y" || temp == "YES" || temp == "TRUE" || temp == "1")
                {
                    WriteLog("[INFO]", "--Sales Refund Posting Start--");

                    #region Sales Refund
                    IList<SalesRefundRequests> cnlist = ListObjectSpace.GetObjects<SalesRefundRequests>
                        (CriteriaOperator.Parse("Sap = ? and ((Status = ? and AppStatus = ?) or (Status = ? and AppStatus = ?))", 0, 4, 0, 4, 1));

                    foreach (SalesRefundRequests dtlcn in cnlist)
                    {
                        try
                        {
                            IObjectSpace cnos = ObjectSpaceProvider.CreateObjectSpace();
                            SalesRefundRequests cnobj = cnos.GetObjectByKey<SalesRefundRequests>(dtlcn.Oid);

                            #region Post CN
                            if (!sap.oCom.InTransaction) sap.oCom.StartTransaction();

                            int tempto = 0;

                            tempto = PostCNtoSAP(cnobj, ObjectSpaceProvider, sap);
                            if (tempto == 1)
                            {
                                if (sap.oCom.InTransaction)
                                    sap.oCom.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit);

                                cnobj.Sap = true;
                                cnobj.Status = DocStatus.Post;

                                SalesRefundReqDocTrail ds = cnos.CreateObject<SalesRefundReqDocTrail>();
                                ds.CreateUser = cnos.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                                ds.CreateDate = DateTime.Now;
                                ds.DocStatus = DocStatus.Post;
                                ds.DocRemarks = "Posted SAP";
                                cnobj.SalesRefundReqDocTrail.Add(ds);

                                GC.Collect();
                            }
                            else if (tempto <= 0)
                            {
                                if (sap.oCom.InTransaction)
                                    sap.oCom.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack);

                                GC.Collect();
                            }
                            #endregion

                            cnos.CommitChanges();
                        }
                        catch (Exception ex)
                        {
                            IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                            SalesRefundRequests obj = osupdate.GetObjectByKey<SalesRefundRequests>(dtlcn.Oid);

                            SalesRefundReqDocTrail ds = osupdate.CreateObject<SalesRefundReqDocTrail>();
                            ds.CreateUser = osupdate.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                            ds.CreateDate = DateTime.Now;
                            ds.DocStatus = DocStatus.PendPost;
                            ds.DocRemarks = "SAP Error:" + ex.Message;
                            obj.SalesRefundReqDocTrail.Add(ds);

                            osupdate.CommitChanges();

                            WriteLog("[Error]", "Message: Sales Refund Post Failed - OID : " + dtlcn.Oid + " (" + ex.Message + ")");
                        }
                    }
                    #endregion

                    WriteLog("[INFO]", "--Sales Refund Posting End--");
                }

                temp = ConfigurationManager.AppSettings["ARDPPost"].ToString().ToUpper();
                if (temp == "Y" || temp == "YES" || temp == "TRUE" || temp == "1")
                {
                    WriteLog("[INFO]", "--AR Downpayment Posting Start--");

                    #region AR Downpayment
                    IList<SalesOrderCollection> dplist = ListObjectSpace.GetObjects<SalesOrderCollection>
                        (CriteriaOperator.Parse("Sap = ? and Status = ?", 0, 1));

                    foreach (SalesOrderCollection dtldp in dplist)
                    {
                        try
                        {
                            IObjectSpace dpos = ObjectSpaceProvider.CreateObjectSpace();
                            SalesOrderCollection dpobj = dpos.GetObjectByKey<SalesOrderCollection>(dtldp.Oid);
                            bool postfail = false;
                            bool postdp = false;
                            bool postpayment = false;
                            bool pendingsales = false;

                            // Start ver 1.0.10
                            if (dpobj.TotalPayment <= 0)
                            {
                                postdp = true;
                                postpayment = true;
                            }
                            // End ver 1.0.10

                            // Start ver 1.0.10
                            if (postdp == false && postpayment == false)
                            {
                            // End ver 1.0.10
                                foreach (SalesOrderCollectionDetails dtl in dpobj.SalesOrderCollectionDetails)
                                {
                                    if (dtl.Sap == false)
                                    {
                                        #region Post AR Downpayment
                                        if (!sap.oCom.InTransaction) sap.oCom.StartTransaction();

                                        int tempto = 0;

                                        tempto = PostDPtoSAP(dpobj, dtl.SalesOrder, ObjectSpaceProvider, sap);
                                        if (tempto == 1)
                                        {
                                            if (sap.oCom.InTransaction)
                                                sap.oCom.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit);

                                            dtl.Sap = true;
                                            postdp = true;

                                            dpos.CommitChanges();

                                            GC.Collect();
                                        }
                                        else if (tempto <= 0)
                                        {
                                            if (sap.oCom.InTransaction)
                                                sap.oCom.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack);

                                            postfail = true;

                                            GC.Collect();
                                        }
                                        else if (tempto == 2)
                                        {
                                            if (sap.oCom.InTransaction)
                                                sap.oCom.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack);

                                            postfail = true;
                                            pendingsales = true;
                                        }
                                        #endregion
                                    }
                                    else
                                    {
                                        postdp = true;
                                    }

                                    if (dtl.SapPayment == false)
                                    {
                                        int dpdocentry = 0;
                                        string getdpDocentry = "SELECT DocEntry FROM [" + ConfigurationManager.AppSettings["CompanyDB"].ToString() +
                                                "]..ODPI WHERE U_PortalDocNum = '" + dpobj.DocNum + "' AND U_SoDocNumber = '" + dtl.SalesOrder + "'";
                                        if (conn.State == ConnectionState.Open)
                                        {
                                            conn.Close();
                                        }
                                        conn.Open();
                                        SqlCommand cmd = new SqlCommand(getdpDocentry, conn);
                                        SqlDataReader reader = cmd.ExecuteReader();
                                        while (reader.Read())
                                        {
                                            dpdocentry = reader.GetInt32(0);
                                        }
                                        conn.Close();

                                        if (dtl.PaymentAmount > 0 && dpdocentry > 0)
                                        {
                                            #region Post AR Downpayment Payment
                                            if (!sap.oCom.InTransaction) sap.oCom.StartTransaction();

                                            int tempardp = 0;

                                            tempardp = PostDPPaymenttoSAP(dpobj, dtl, dtl.SalesOrder, ObjectSpaceProvider, sap, dpdocentry);
                                            if (tempardp == 1)
                                            {
                                                if (sap.oCom.InTransaction)
                                                    sap.oCom.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit);

                                                dtl.SapPayment = true;
                                                postpayment = true;

                                                dpos.CommitChanges();

                                                GC.Collect();
                                            }
                                            else if (tempardp <= 0)
                                            {
                                                if (sap.oCom.InTransaction)
                                                    sap.oCom.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack);

                                                postfail = true;

                                                GC.Collect();
                                            }
                                            #endregion
                                        }
                                        else
                                        {
                                            postpayment = true;
                                        }
                                    }
                                    else
                                    {
                                        postpayment = true;
                                    }
                                }
                            // Start ver 1.0.10
                            }
                            // End ver 1.0.10

                            if (postfail == false && postdp == true && postpayment == true)
                            {
                                dpobj.Status = DocStatus.Post;
                                dpobj.Sap = true;

                                SalesOrderCollectionDocStatus ds = dpos.CreateObject<SalesOrderCollectionDocStatus>();
                                ds.CreateUser = dpos.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                                ds.CreateDate = DateTime.Now;
                                ds.DocStatus = DocStatus.Post;
                                ds.DocRemarks = "Posted SAP";
                                dpobj.SalesOrderCollectionDocStatus.Add(ds);

                                dpos.CommitChanges();
                            }

                            if (pendingsales == true)
                            {
                                SalesOrderCollectionDocStatus ds = dpos.CreateObject<SalesOrderCollectionDocStatus>();
                                ds.CreateUser = dpos.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                                ds.CreateDate = DateTime.Now;
                                ds.DocStatus = DocStatus.Post;
                                ds.DocRemarks = "Pending Sales Posting.";
                                dpobj.SalesOrderCollectionDocStatus.Add(ds);

                                dpos.CommitChanges();
                            }
                        }
                        catch (Exception ex)
                        {
                            IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                            SalesOrderCollection obj = osupdate.GetObjectByKey<SalesOrderCollection>(dtldp.Oid);

                            SalesOrderCollectionDocStatus ds = osupdate.CreateObject<SalesOrderCollectionDocStatus>();
                            ds.CreateUser = osupdate.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                            ds.CreateDate = DateTime.Now;
                            ds.DocStatus = DocStatus.PendPost;
                            ds.DocRemarks = "SAP Error:" + ex.Message;
                            obj.SalesOrderCollectionDocStatus.Add(ds);

                            osupdate.CommitChanges();

                            WriteLog("[Error]", "Message: AR Downpayment Post Failed - OID : " + dtldp.Oid + " (" + ex.Message + ")");
                        }
                    }
                    #endregion

                    WriteLog("[INFO]", "--AR Downpayment Posting End--");
                }

                temp = ConfigurationManager.AppSettings["PickListPost"].ToString().ToUpper();
                if (temp == "Y" || temp == "YES" || temp == "TRUE" || temp == "1")
                {
                    WriteLog("[INFO]", "--Pick List Start--");

                    #region Pick List 
                    IList<PickList> pllist = ListObjectSpace.GetObjects<PickList>
                    (CriteriaOperator.Parse("Sap = ? and Status = ?", 0, 1));

                    foreach (PickList dtlpl in pllist)
                    {
                        try
                        {
                            IObjectSpace plos = ObjectSpaceProvider.CreateObjectSpace();
                            PickList plobj = plos.GetObjectByKey<PickList>(dtlpl.Oid);
                            bool post = true;

                            string getPLWhs = "SELECT Warehouse FROM PickListDetails WHERE PickList = " + plobj.Oid + " GROUP BY Warehouse";
                            if (conn.State == ConnectionState.Open)
                            {
                                conn.Close();
                            }
                            conn.Open();
                            SqlCommand cmd = new SqlCommand(getPLWhs, conn);
                            SqlDataReader reader = cmd.ExecuteReader();
                            while (reader.Read())
                            {
                                #region Post Pick List
                                if (!sap.oCom.InTransaction) sap.oCom.StartTransaction();

                                int temppo = 0;

                                temppo = PostPLtoSAP(plobj, reader.GetString(0), ObjectSpaceProvider, sap);
                                if (temppo == 1)
                                {
                                    if (sap.oCom.InTransaction)
                                        sap.oCom.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit);

                                    //plobj.Status = DocStatus.Post;

                                    foreach (PickListDetailsActual dtl in plobj.PickListDetailsActual)
                                    {
                                        if (dtl.Warehouse.WarehouseCode == reader.GetString(0))
                                        {
                                            dtl.Sap = true;
                                        }
                                    }

                                    plos.CommitChanges();

                                    GC.Collect();
                                }
                                else if (temppo <= 0)
                                {
                                    if (sap.oCom.InTransaction)
                                        sap.oCom.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack);

                                    GC.Collect();
                                }
                                else if (temppo == 2)
                                {
                                    if (sap.oCom.InTransaction)
                                        sap.oCom.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack);

                                    foreach (PickListDetailsActual dtl in plobj.PickListDetailsActual)
                                    {
                                        if (dtl.Warehouse.WarehouseCode == reader.GetString(0))
                                        {
                                            dtl.Sap = true;
                                        }
                                    }

                                    plos.CommitChanges();

                                    GC.Collect();
                                }
                                #endregion
                            }
                            conn.Close();

                            foreach (PickListDetailsActual dtl2 in plobj.PickListDetailsActual)
                            {
                                if (dtl2.Sap == false)
                                {
                                    post = false;
                                }
                            }

                            if (post == true)
                            {
                                // Start ver 1.0.22
                                plobj.Status = DocStatus.Post;
                                // End ver 1.0.22
                                plobj.Sap = true;

                                PickListDocTrail ds = plos.CreateObject<PickListDocTrail>();
                                ds.CreateUser = plos.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                                ds.CreateDate = DateTime.Now;
                                ds.DocStatus = DocStatus.Post;
                                ds.DocRemarks = "Posted SAP";
                                plobj.PickListDocTrail.Add(ds);
                            }

                            plos.CommitChanges();
                        }
                        catch (Exception ex)
                        {
                            IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                            PickList obj = osupdate.GetObjectByKey<PickList>(dtlpl.Oid);

                            PickListDocTrail ds = osupdate.CreateObject<PickListDocTrail>();
                            ds.CreateUser = osupdate.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                            ds.CreateDate = DateTime.Now;
                            ds.DocStatus = DocStatus.PendPost;
                            ds.DocRemarks = "SAP Error:" + ex.Message;
                            obj.PickListDocTrail.Add(ds);

                            osupdate.CommitChanges();

                            WriteLog("[Error]", "Message: POST Pick List Failed - OID : " + dtlpl.Oid + " (" + ex.Message + ")");
                        }
                    }
                    #endregion

                    WriteLog("[INFO]", "--Pick List End--");
                }

                temp = ConfigurationManager.AppSettings["WhsTransferPost"].ToString().ToUpper();
                if (temp == "Y" || temp == "YES" || temp == "TRUE" || temp == "1")
                {
                    WriteLog("[INFO]", "--Warehouse Transfer Posting Start--");

                    #region Warehouse Transfer
                    IList<WarehouseTransfers> wtlist = ListObjectSpace.GetObjects<WarehouseTransfers>
                        (CriteriaOperator.Parse("Sap = ? and Status = ?", 0, 1));

                    foreach (WarehouseTransfers dtlwt in wtlist)
                    {
                        try
                        {
                            IObjectSpace wtos = ObjectSpaceProvider.CreateObjectSpace();
                            WarehouseTransfers wtobj = wtos.GetObjectByKey<WarehouseTransfers>(dtlwt.Oid);

                            #region Post Warehouse Transfer
                            if (!sap.oCom.InTransaction) sap.oCom.StartTransaction();

                            int tempto = 0;

                            tempto = PostWTIntoSAP(wtobj, ObjectSpaceProvider, sap);
                            if (tempto == 1)
                            {
                                if (sap.oCom.InTransaction)
                                    sap.oCom.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit);

                                wtobj.Sap = true;
                                wtobj.Status = DocStatus.Post;

                                WarehouseTransfersDocTrail ds = wtos.CreateObject<WarehouseTransfersDocTrail>();
                                ds.CreateUser = wtos.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                                ds.CreateDate = DateTime.Now;
                                ds.DocStatus = DocStatus.Post;
                                ds.DocRemarks = "Posted SAP";
                                wtobj.WarehouseTransfersDocTrail.Add(ds);

                                GC.Collect();
                            }
                            else if (tempto <= 0)
                            {
                                if (sap.oCom.InTransaction)
                                    sap.oCom.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack);

                                GC.Collect();
                            }
                            #endregion

                            wtos.CommitChanges();
                        }
                        catch (Exception ex)
                        {
                            IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                            WarehouseTransfers obj = osupdate.GetObjectByKey<WarehouseTransfers>(dtlwt.Oid);

                            WarehouseTransfersDocTrail ds = osupdate.CreateObject<WarehouseTransfersDocTrail>();
                            ds.CreateUser = osupdate.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                            ds.CreateDate = DateTime.Now;
                            ds.DocStatus = DocStatus.PendPost;
                            ds.DocRemarks = "SAP Error:" + ex.Message;
                            obj.WarehouseTransfersDocTrail.Add(ds);

                            osupdate.CommitChanges();

                            WriteLog("[Error]", "Message: Warehouse Transfer Post Failed - OID : " + dtlwt.Oid + " (" + ex.Message + ")");
                        }
                    }
                    #endregion

                    WriteLog("[INFO]", "--Warehouse Transfer Posting End--");
                }

                temp = ConfigurationManager.AppSettings["DOPost"].ToString().ToUpper();
                if (temp == "Y" || temp == "YES" || temp == "TRUE" || temp == "1")
                {
                    WriteLog("[INFO]", "--Delivery Order Posting Start--");

                    #region Delivery Order
                    IList<DeliveryOrder> dolist = ListObjectSpace.GetObjects<DeliveryOrder>
                        (CriteriaOperator.Parse("Sap = ? and Status = ?", 0, 1));

                    foreach (DeliveryOrder dtldo in dolist)
                    {
                        try
                        {
                            IObjectSpace doos = ObjectSpaceProvider.CreateObjectSpace();
                            DeliveryOrder doobj = doos.GetObjectByKey<DeliveryOrder>(dtldo.Oid);

                            if (doobj.SapDO == false)
                            {
                                #region Post DO
                                if (!sap.oCom.InTransaction) sap.oCom.StartTransaction();

                                int tempdo = 0;

                                tempdo = PostARDOtoSAP(doobj, ObjectSpaceProvider, sap);
                                if (tempdo == 1)
                                {
                                    if (sap.oCom.InTransaction)
                                        sap.oCom.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit);

                                    doobj.SapDO = true;
                                    doos.CommitChanges();

                                    GC.Collect();
                                }
                                else if (tempdo <= 0)
                                {
                                    if (sap.oCom.InTransaction)
                                        sap.oCom.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack);

                                    GC.Collect();
                                }
                                #endregion
                            }

                            if (doobj.SapINV == false && doobj.SapDO == true)
                            {
                                #region Post INV
                                if (!sap.oCom.InTransaction) sap.oCom.StartTransaction();

                                int tempdo = 0;

                                tempdo = PostDOtoSAP(doobj, ObjectSpaceProvider, sap);
                                if (tempdo == 1)
                                {
                                    if (sap.oCom.InTransaction)
                                        sap.oCom.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit);

                                    doobj.SapINV = true;
                                    doos.CommitChanges();

                                    GC.Collect();
                                }
                                else if (tempdo <= 0)
                                {
                                    if (sap.oCom.InTransaction)
                                        sap.oCom.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack);

                                    GC.Collect();
                                }
                                #endregion
                            }

                            if (doobj.SapDO == true && doobj.SapINV == true)
                            {
                                doobj.Status = DocStatus.Post;
                                doobj.Sap = true;

                                DeliveryOrderDocTrail ds = doos.CreateObject<DeliveryOrderDocTrail>();
                                ds.CreateUser = doos.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                                ds.CreateDate = DateTime.Now;
                                ds.DocStatus = DocStatus.Post;
                                ds.DocRemarks = "Posted SAP";
                                doobj.DeliveryOrderDocTrail.Add(ds);

                                doos.CommitChanges();
                            }
                        }
                        catch (Exception ex)
                        {
                            IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                            DeliveryOrder obj = osupdate.GetObjectByKey<DeliveryOrder>(dtldo.Oid);

                            DeliveryOrderDocTrail ds = osupdate.CreateObject<DeliveryOrderDocTrail>();
                            ds.CreateUser = osupdate.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                            ds.CreateDate = DateTime.Now;
                            ds.DocStatus = DocStatus.PendPost;
                            ds.DocRemarks = "SAP Error:" + ex.Message;
                            obj.DeliveryOrderDocTrail.Add(ds);

                            osupdate.CommitChanges();

                            WriteLog("[Error]", "Message: Delivery Order Post Failed - OID : " + dtldo.Oid + " (" + ex.Message + ")");
                        }
                    }
                    #endregion

                    WriteLog("[INFO]", "--Delivery Order Posting End--");
                }

                // Start ver 1.0.7
                temp = ConfigurationManager.AppSettings["CNCancelPost"].ToString().ToUpper();
                if (temp == "Y" || temp == "YES" || temp == "TRUE" || temp == "1")
                {
                    WriteLog("[INFO]", "--Downpayment Cancellation Posting Start--");

                    #region Downpayment Cancellation
                    IList<ARDownpaymentCancel> cnlist = ListObjectSpace.GetObjects<ARDownpaymentCancel>
                        (CriteriaOperator.Parse("Sap = ? and ((Status = ? and AppStatus = ?) or (Status = ? and AppStatus = ?))", 0, 1, 0, 1, 1));

                    foreach (ARDownpaymentCancel dtlcn in cnlist)
                    {
                        try
                        {
                            IObjectSpace cnos = ObjectSpaceProvider.CreateObjectSpace();
                            ARDownpaymentCancel cnobj = cnos.GetObjectByKey<ARDownpaymentCancel>(dtlcn.Oid);

                            string sonum = null;
                            bool fail = false;
                            foreach (ARDownpaymentCancelDetails dtl in cnobj.ARDownpaymentCancelDetails)
                            {
                                if (dtl.BaseDoc != null)
                                {
                                    if (dtl.BaseDoc != sonum)
                                    {
                                        string getdoDocentry = "SELECT DocNum FROM [" +
                                            ConfigurationManager.AppSettings["CompanyDB"].ToString() + "]..RCT2 WHERE InvType = 203 AND DocEntry = "
                                            + dtl.BaseDoc;
                                        if (conn.State == ConnectionState.Open)
                                        {
                                            conn.Close();
                                        }
                                        conn.Open();
                                        SqlCommand cmd1 = new SqlCommand(getdoDocentry, conn);
                                        SqlDataReader reader1 = cmd1.ExecuteReader();
                                        while (reader1.Read())
                                        {
                                            #region Cancel DP
                                            if (!sap.oCom.InTransaction) sap.oCom.StartTransaction();

                                            int temppayment = 0;

                                            temppayment = PostCancelPaymenttoSAP(cnobj, reader1.GetInt32(0), ObjectSpaceProvider, sap);
                                            if (temppayment == 1)
                                            {
                                                if (sap.oCom.InTransaction)
                                                    sap.oCom.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit);

                                                dtl.SapPayment = true;
                                                cnos.CommitChanges();

                                                GC.Collect();
                                            }
                                            else if (temppayment <= 0)
                                            {
                                                if (sap.oCom.InTransaction)
                                                    sap.oCom.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack);

                                                fail = true;
                                                GC.Collect();
                                            }
                                            else if (temppayment == 2)
                                            {
                                                sap.oCom.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack);

                                                dtl.SapPayment = true;
                                                cnos.CommitChanges();

                                                GC.Collect();
                                            }
                                            #endregion
                                        }
                                        conn.Close();

                                        sonum = dtl.BaseDoc;
                                    }
                                }
                            }

                            if (fail == false)
                            {
                                #region Post CN
                                if (!sap.oCom.InTransaction) sap.oCom.StartTransaction();

                                int tempto = 0;

                                tempto = PostCNCanceltoSAP(cnobj, ObjectSpaceProvider, sap);
                                if (tempto == 1)
                                {
                                    if (sap.oCom.InTransaction)
                                        sap.oCom.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit);

                                    cnobj.Status = DocStatus.Post;
                                    cnobj.Sap = true;

                                    ARDownpaymentCancellationDocTrail ds = cnos.CreateObject<ARDownpaymentCancellationDocTrail>();
                                    ds.CreateUser = cnos.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                                    ds.CreateDate = DateTime.Now;
                                    ds.DocStatus = DocStatus.Post;
                                    ds.DocRemarks = "Posted SAP";
                                    cnobj.ARDownpaymentCancellationDocTrail.Add(ds);

                                    GC.Collect();
                                }
                                else if (tempto <= 0)
                                {
                                    if (sap.oCom.InTransaction)
                                        sap.oCom.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack);

                                    GC.Collect();
                                }
                                #endregion
                            }

                            cnos.CommitChanges();
                        }
                        catch (Exception ex)
                        {
                            IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                            ARDownpaymentCancel obj = osupdate.GetObjectByKey<ARDownpaymentCancel>(dtlcn.Oid);

                            ARDownpaymentCancellationDocTrail ds = osupdate.CreateObject<ARDownpaymentCancellationDocTrail>();
                            ds.CreateUser = osupdate.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                            ds.CreateDate = DateTime.Now;
                            ds.DocStatus = DocStatus.PendPost;
                            ds.DocRemarks = "SAP Error:" + ex.Message;
                            obj.ARDownpaymentCancellationDocTrail.Add(ds);

                            osupdate.CommitChanges();

                            WriteLog("[Error]", "Message: Sales Refund Post Failed - OID : " + dtlcn.Oid + " (" + ex.Message + ")");
                        }
                    }
                    #endregion

                    WriteLog("[INFO]", "--Downpayment Cancellation Posting End--");
                }
                // End ver 1.0.7

                // Start ver 1.0.9
                temp = ConfigurationManager.AppSettings["SOCancel"].ToString().ToUpper();
                if (temp == "Y" || temp == "YES" || temp == "TRUE" || temp == "1")
                {
                    WriteLog("[INFO]", "--SO Cancel Start--");

                    #region SO Cancel
                    IList<SalesOrder> soclist = ListObjectSpace.GetObjects<SalesOrder>
                    (CriteriaOperator.Parse("PendingCancel = ? and SapCancel = ?", 1, 0));

                    foreach (SalesOrder dtlsoc in soclist)
                    {
                        int basedocentry = 0;
                        try
                        {
                            IObjectSpace socos = ObjectSpaceProvider.CreateObjectSpace();
                            SalesOrder socobj = socos.GetObjectByKey<SalesOrder>(dtlsoc.Oid);

                            foreach (SalesOrderDetails detail in socobj.SalesOrderDetails)
                            {
                                basedocentry = detail.SAPDocEntry;
                                break;
                            }

                            if (basedocentry > 0)
                            {
                                #region Cancel SO
                                if (!sap.oCom.InTransaction) sap.oCom.StartTransaction();

                                int tempso = 0;

                                tempso = CancelSOtoSAP(socobj, basedocentry, ObjectSpaceProvider, sap);
                                if (tempso == 1)
                                {
                                    if (sap.oCom.InTransaction)
                                        sap.oCom.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit);

                                    socobj.SapCancel = true;

                                    SalesOrderDocStatus ds = socos.CreateObject<SalesOrderDocStatus>();
                                    ds.CreateUser = socos.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                                    ds.CreateDate = DateTime.Now;
                                    ds.DocStatus = DocStatus.Cancelled;
                                    ds.DocRemarks = "Cancel SAP";
                                    socobj.SalesOrderDocStatus.Add(ds);

                                    socos.CommitChanges();

                                    GC.Collect();
                                }
                                else if (tempso <= 0)
                                {
                                    if (sap.oCom.InTransaction)
                                        sap.oCom.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack);

                                    GC.Collect();
                                }
                                else if (tempso == 2)
                                {
                                    sap.oCom.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack);

                                    socobj.SapCancel = true;
                                    socos.CommitChanges();

                                    GC.Collect();
                                }
                                #endregion
                            }
                        }
                        catch (Exception ex)
                        {
                            IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                            SalesOrder obj = osupdate.GetObjectByKey<SalesOrder>(dtlsoc.Oid);

                            SalesOrderDocStatus ds = osupdate.CreateObject<SalesOrderDocStatus>();
                            ds.CreateUser = osupdate.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                            ds.CreateDate = DateTime.Now;
                            ds.DocStatus = DocStatus.PendPost;
                            ds.DocRemarks = "SAP Error:" + ex.Message;
                            obj.SalesOrderDocStatus.Add(ds);

                            osupdate.CommitChanges();

                            WriteLog("[Error]", "Message: Cancel SO Failed - OID : " + dtlsoc.Oid + " (" + ex.Message + ")");
                        }
                    }
                    #endregion

                    WriteLog("[INFO]", "--SO Cancel End--");
                }

                temp = ConfigurationManager.AppSettings["SOClosed"].ToString().ToUpper();
                if (temp == "Y" || temp == "YES" || temp == "TRUE" || temp == "1")
                {
                    WriteLog("[INFO]", "--SO Closed Start--");

                    #region SO Closed
                    IList<SalesOrder> soclist = ListObjectSpace.GetObjects<SalesOrder>
                    (CriteriaOperator.Parse("PendingClose = ? and SapClose = ?", 1, 0));

                    foreach (SalesOrder dtlsoc in soclist)
                    {
                        int basedocentry = 0;
                        try
                        {
                            IObjectSpace socos = ObjectSpaceProvider.CreateObjectSpace();
                            SalesOrder socobj = socos.GetObjectByKey<SalesOrder>(dtlsoc.Oid);

                            foreach (SalesOrderDetails detail in socobj.SalesOrderDetails)
                            {
                                basedocentry = detail.SAPDocEntry;
                                break;
                            }

                            if (basedocentry > 0)
                            {
                                #region Closed SO
                                if (!sap.oCom.InTransaction) sap.oCom.StartTransaction();

                                int tempso = 0;

                                tempso = ClosedSOtoSAP(socobj, basedocentry, ObjectSpaceProvider, sap);
                                if (tempso == 1)
                                {
                                    if (sap.oCom.InTransaction)
                                        sap.oCom.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit);

                                    socobj.SapClose = true;

                                    SalesOrderDocStatus ds = socos.CreateObject<SalesOrderDocStatus>();
                                    ds.CreateUser = socos.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                                    ds.CreateDate = DateTime.Now;
                                    ds.DocStatus = DocStatus.Closed;
                                    ds.DocRemarks = "Closed SAP";
                                    socobj.SalesOrderDocStatus.Add(ds);

                                    socos.CommitChanges();

                                    GC.Collect();
                                }
                                else if (tempso <= 0)
                                {
                                    if (sap.oCom.InTransaction)
                                        sap.oCom.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack);

                                    GC.Collect();
                                }
                                else if (tempso == 2)
                                {
                                    sap.oCom.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack);

                                    socobj.SapClose = true;
                                    socos.CommitChanges();

                                    GC.Collect();
                                }
                                #endregion
                            }
                        }
                        catch (Exception ex)
                        {
                            IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                            SalesOrder obj = osupdate.GetObjectByKey<SalesOrder>(dtlsoc.Oid);

                            SalesOrderDocStatus ds = osupdate.CreateObject<SalesOrderDocStatus>();
                            ds.CreateUser = osupdate.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                            ds.CreateDate = DateTime.Now;
                            ds.DocStatus = DocStatus.PendPost;
                            ds.DocRemarks = "SAP Error:" + ex.Message;
                            obj.SalesOrderDocStatus.Add(ds);

                            osupdate.CommitChanges();

                            WriteLog("[Error]", "Message: Closed SO Failed - OID : " + dtlsoc.Oid + " (" + ex.Message + ")");
                        }
                    }
                    #endregion

                    WriteLog("[INFO]", "--SO Closed End--");
                }
                // End ver 1.0.9

                // Start ver 1.0.12
                temp = ConfigurationManager.AppSettings["StockCount"].ToString().ToUpper();
                if (temp == "Y" || temp == "YES" || temp == "TRUE" || temp == "1")
                {
                    WriteLog("[INFO]", "--Stock Count Posting Start--");

                    #region Stock Count 

                    IList<StockCountConfirm> sclist = ListObjectSpace.GetObjects<StockCountConfirm>
                    (CriteriaOperator.Parse("Sap = ? and Status = ?", 0, 1));

                    foreach (StockCountConfirm dtlsc in sclist)
                    {
                        try
                        {
                            IObjectSpace scos = ObjectSpaceProvider.CreateObjectSpace();
                            StockCountConfirm scobj = scos.GetObjectByKey<StockCountConfirm>(dtlsc.Oid);

                            bool postfail = false;
                            bool positive = false;
                            bool negatif = false;
                            bool postissue = true;

                            if (scobj.GISap == true)
                            {
                                negatif = true;
                            }

                            if (scobj.GRSap == true)
                            {
                                positive = true;
                            }

                            if (negatif == false)
                            {
                                #region Post Goods Issue
                                if (!sap.oCom.InTransaction) sap.oCom.StartTransaction();

                                int tempscout = 0;

                                tempscout = PostSCOuttoSAP(scobj, ObjectSpaceProvider, sap);
                                if (tempscout == 1)
                                {
                                    if (sap.oCom.InTransaction)
                                        sap.oCom.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit);

                                    scobj.GISap = true;

                                    GC.Collect();
                                }
                                else if (tempscout <= 0)
                                {
                                    if (sap.oCom.InTransaction)
                                        sap.oCom.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack);

                                    postfail = true;
                                    postissue = false;

                                    GC.Collect();
                                }
                                else if (tempscout == 2)
                                {
                                    if (sap.oCom.InTransaction)
                                        sap.oCom.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack);

                                    scobj.GISap = true;
                                    GC.Collect();
                                }
                                #endregion

                                scos.CommitChanges();
                            }

                            if (positive == false && postissue == true)
                            {
                                #region Post Goods Receipt
                                if (!sap.oCom.InTransaction) sap.oCom.StartTransaction();

                                int tempscin = 0;

                                tempscin = PostSCINtoSAP(scobj, ObjectSpaceProvider, sap);
                                if (tempscin == 1)
                                {
                                    if (sap.oCom.InTransaction)
                                        sap.oCom.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_Commit);

                                    scobj.GRSap = true;

                                    GC.Collect();
                                }
                                else if (tempscin <= 0)
                                {
                                    if (sap.oCom.InTransaction)
                                        sap.oCom.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack);

                                    postfail = true;

                                    GC.Collect();
                                }
                                else if (tempscin == 2)
                                {
                                    if (sap.oCom.InTransaction)
                                        sap.oCom.EndTransaction(SAPbobsCOM.BoWfTransOpt.wf_RollBack);

                                    scobj.GRSap = true;

                                    GC.Collect();
                                }
                                #endregion
                            }

                            if (postfail == false)
                            {
                                scobj.Sap = true;
                                scobj.Status = DocStatus.Post;

                                StockCountConfirmDocTrail ds = scos.CreateObject<StockCountConfirmDocTrail>();
                                ds.CreateUser = scos.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                                ds.CreateDate = DateTime.Now;
                                ds.DocStatus = DocStatus.Post;
                                ds.DocRemarks = "Posted SAP";
                                scobj.StockCountConfirmDocTrail.Add(ds);

                                scos.CommitChanges();
                            }

                        }
                        catch (Exception ex)
                        {
                            IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                            StockCountConfirm obj = osupdate.GetObjectByKey<StockCountConfirm>(dtlsc.Oid);

                            StockCountConfirmDocTrail ds = osupdate.CreateObject<StockCountConfirmDocTrail>();
                            ds.CreateUser = osupdate.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                            ds.CreateDate = DateTime.Now;
                            ds.DocStatus = DocStatus.PendPost;
                            ds.DocRemarks = "SAP Error:" + ex.Message;
                            obj.StockCountConfirmDocTrail.Add(ds);

                            osupdate.CommitChanges();

                            WriteLog("[Error]", "Message: Stock Count Post Failed - OID : " + dtlsc.Oid + " (" + ex.Message + ")");
                        }
                    }
                    #endregion
                    
                    WriteLog("[INFO]", "--Stock Count Posting End--");
                }
                // End ver 1.0.12

                #region Update DocNum
                    SqlCommand TransactionNotification = new SqlCommand("", conn);
                TransactionNotification.CommandTimeout = 600;

                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }

                try
                {
                    conn.Open();

                    TransactionNotification.CommandText = "EXEC sp_UpdSapDocNum '" + ConfigurationManager.AppSettings["CompanyDB"].ToString() + "'";

                    SqlDataReader reader = TransactionNotification.ExecuteReader();
                    //DataTable dt = new DataTable();
                    //da.Fill(dt);
                    //da.Dispose();
                    conn.Close();
                }
                catch (Exception ex)
                {
                    WriteLog("[Error]", "Message:" + ex.Message);
                    conn.Close();
                }
                #endregion

                // Start ver 1.0.8.1
                temp = ConfigurationManager.AppSettings["UpdNonPersistent"].ToString().ToUpper();
                if (temp == "Y" || temp == "YES" || temp == "TRUE" || temp == "1")
                {
                    #region Update NonPersistent
                    int result = UpdNonPersistent(ObjectSpaceProvider);
                    #endregion
                }
                // End ver 1.0.8.1

                // Start ver 1.0.13
                temp = ConfigurationManager.AppSettings["RecreateSO"].ToString().ToUpper();
                if (temp == "Y" || temp == "YES" || temp == "TRUE" || temp == "1")
                {
                    WriteLog("[INFO]", "--Recreate SO Start--");

                    #region Recreate SO

                    string getSQDoc = "SELECT T0.DocNum " +
                        "FROM SalesQuotation T0 " +
                        "LEFT JOIN SalesOrder T1 on T0.DocNum = T1.SQNumber " +
                        "WHERE T1.OID is null AND T0.Status = 1 " +
                        "AND (T0.AppStatus <> 3 and T0.AppStatus <> 2) AND CAST(T0.UpdateDate as date) = CAST(GETDATE() as date) " +
                        "AND GETDATE() >= DATEADD(MINUTE, 10, T0.UpdateDate)";
                    if (conn.State == ConnectionState.Open)
                    {
                        conn.Close();
                    }
                    conn.Open();
                    SqlCommand cmdsq = new SqlCommand(getSQDoc, conn);
                    SqlDataReader readersq = cmdsq.ExecuteReader();
                    while (readersq.Read())
                    {
                        IObjectSpace sqos = ObjectSpaceProvider.CreateObjectSpace();
                        SalesQuotation trx = sqos.FindObject<SalesQuotation>(new BinaryOperator("DocNum", readersq.GetString(0)));

                        if (trx != null)
                        {
                            #region Add SO
                            IObjectSpace salesos = ObjectSpaceProvider.CreateObjectSpace();
                            SalesOrder newSO = salesos.CreateObject<SalesOrder>();

                            GeneralControllers genCon = new GeneralControllers();
                            string docprefix = GetDocPrefix();
                            newSO.DocNum = genCon.GenerateDocNum(DocTypeList.SO, salesos, TransferType.NA, 0, docprefix);

                            if (trx.Customer != null)
                            {
                                newSO.Customer = newSO.Session.GetObjectByKey<vwBusniessPartner>(trx.Customer.BPCode);
                            }
                            newSO.CustomerName = trx.CustomerName;
                            if (trx.Transporter != null)
                            {
                                newSO.Transporter = newSO.Session.GetObjectByKey<vwTransporter>(trx.Transporter.TransporterID);
                            }
                            newSO.ContactNo = trx.ContactNo;
                            if (trx.ContactPerson != null)
                            {
                                newSO.ContactPerson = newSO.Session.GetObjectByKey<vwSalesPerson>(trx.ContactPerson.SlpCode);
                            }
                            if (trx.PaymentTerm != null)
                            {
                                newSO.PaymentTerm = newSO.Session.GetObjectByKey<vwPaymentTerm>(trx.PaymentTerm.GroupNum);
                            }
                            if (trx.Series != null)
                            {
                                newSO.Series = newSO.Session.GetObjectByKey<vwSeries>(trx.Series.Series);
                            }
                            if (trx.Priority != null)
                            {
                                newSO.Priority = newSO.Session.GetObjectByKey<PriorityType>(trx.Priority.Oid);
                            }
                            if (trx.BillingAddress != null)
                            {
                                newSO.BillingAddress = newSO.Session.GetObjectByKey<vwBillingAddress>(trx.BillingAddress.PriKey);
                            }
                            newSO.BillingAddressfield = trx.BillingAddressfield;
                            if (trx.ShippingAddress != null)
                            {
                                newSO.ShippingAddress = newSO.Session.GetObjectByKey<vwShippingAddress>(trx.ShippingAddress.PriKey);
                            }
                            newSO.ShippingAddressfield = trx.ShippingAddressfield;
                            newSO.Remarks = trx.Remarks;
                            newSO.Attn = trx.Attn;
                            newSO.RefNo = trx.RefNo;
                            // Start ver 1.0.8.1
                            newSO.SQNumber = trx.DocNum;
                            // End ver 1.0.8.1
                            // Start ver 1.0.18
                            // Buyer
                            if (trx.EIVConsolidate != null)
                            {
                                newSO.EIVConsolidate = newSO.Session.FindObject<vwYesNo>(CriteriaOperator.Parse("Code = ?", trx.EIVConsolidate.Code));
                            }
                            if (trx.EIVType != null)
                            {
                                newSO.EIVType = newSO.Session.FindObject<vwEIVType>(CriteriaOperator.Parse("Code = ?", trx.EIVType.Code));
                            }
                            if (trx.EIVFreqSync != null)
                            {
                                newSO.EIVFreqSync = newSO.Session.FindObject<vwEIVFreqSync>(CriteriaOperator.Parse("Code = ?", trx.EIVFreqSync.Code));
                            }
                            newSO.EIVBuyerName = trx.EIVBuyerName;
                            newSO.EIVBuyerTIN = trx.EIVBuyerTIN;
                            newSO.EIVBuyerRegNum = trx.EIVBuyerRegNum;
                            if (trx.EIVBuyerRegTyp != null)
                            {
                                newSO.EIVBuyerRegTyp = newSO.Session.FindObject<vwEIVRegType>(CriteriaOperator.Parse("Code = ?", trx.EIVBuyerRegTyp.Code));
                            }
                            newSO.EIVBuyerSSTRegNum = trx.EIVBuyerSSTRegNum;
                            newSO.EIVBuyerEmail = trx.EIVBuyerEmail;
                            newSO.EIVBuyerContact = trx.EIVBuyerContact;
                            newSO.EIVAddressLine1B = trx.EIVAddressLine1B;
                            newSO.EIVAddressLine2B = trx.EIVAddressLine2B;
                            newSO.EIVAddressLine3B = trx.EIVAddressLine3B;
                            newSO.EIVPostalZoneB = trx.EIVPostalZoneB;
                            newSO.EIVCityNameB = trx.EIVCityNameB;
                            if (trx.EIVStateB != null)
                            {
                                newSO.EIVStateB = newSO.Session.FindObject<vwState>(CriteriaOperator.Parse("Code = ?", trx.EIVStateB.Code));
                            }
                            if (trx.EIVCountryB != null)
                            {
                                newSO.EIVCountryB = newSO.Session.FindObject<vwCountry>(CriteriaOperator.Parse("Code = ?", trx.EIVCountryB.Code));
                            }
                            //Recipient
                            newSO.EIVShippingName = trx.EIVShippingName;
                            newSO.EIVShippingTin = trx.EIVShippingTin;
                            newSO.EIVShippingRegNum = trx.EIVShippingRegNum;
                            if (trx.EIVShippingRegTyp != null)
                            {
                                newSO.EIVShippingRegTyp = newSO.Session.FindObject<vwEIVRegType>(CriteriaOperator.Parse("Code = ?", trx.EIVShippingRegTyp.Code));
                            }
                            newSO.EIVAddressLine1S = trx.EIVAddressLine1S;
                            newSO.EIVAddressLine2S = trx.EIVAddressLine2S;
                            newSO.EIVAddressLine3S = trx.EIVAddressLine3S;
                            newSO.EIVPostalZoneS = trx.EIVPostalZoneS;
                            newSO.EIVCityNameS = trx.EIVCityNameS;
                            if (trx.EIVStateS != null)
                            {
                                newSO.EIVStateS = newSO.Session.FindObject<vwState>(CriteriaOperator.Parse("Code = ?", trx.EIVStateS.Code));
                            }
                            if (trx.EIVCountryS != null)
                            {
                                newSO.EIVCountryS = newSO.Session.FindObject<vwCountry>(CriteriaOperator.Parse("Code = ?", trx.EIVCountryS.Code));
                            }
                            // End ver 1.0.18

                            foreach (SalesQuotationDetails dtl in trx.SalesQuotationDetails)
                            {
                                SalesOrderDetails newsodetails = salesos.CreateObject<SalesOrderDetails>();

                                newsodetails.ItemCode = newsodetails.Session.GetObjectByKey<vwItemMasters>(dtl.ItemCode.ItemCode);
                                newsodetails.ItemDesc = dtl.ItemDesc;
                                newsodetails.Model = dtl.Model;
                                newsodetails.CatalogNo = dtl.CatalogNo;
                                // Start ver 1.0.18
                                if (dtl.EIVClassification != null)
                                {
                                    newsodetails.EIVClassification = newsodetails.Session.FindObject<vwEIVClass>(CriteriaOperator.Parse("Code = ?", dtl.EIVClassification.Code));
                                }
                                // End ver 1.0.18
                                if (dtl.Location != null)
                                {
                                    newsodetails.Location = newsodetails.Session.GetObjectByKey<vwWarehouse>(dtl.Location.WarehouseCode);
                                }
                                newsodetails.Quantity = dtl.Quantity;
                                newsodetails.Price = dtl.Price;
                                newsodetails.AdjustedPrice = dtl.AdjustedPrice;
                                newsodetails.BaseDoc = trx.DocNum;
                                newsodetails.BaseId = dtl.Oid.ToString();
                                newSO.SalesOrderDetails.Add(newsodetails);
                            }

                            salesos.CommitChanges();
                            #endregion
                        }
                    }
                    cmdsq.Dispose();
                    conn.Close();

                    #endregion
                }

                temp = ConfigurationManager.AppSettings["RecreateDO"].ToString().ToUpper();
                if (temp == "Y" || temp == "YES" || temp == "TRUE" || temp == "1")
                {
                    WriteLog("[INFO]", "--Recreate DO Start--");

                    #region Recreate DO

                    string allload = null;
                    XPObjectSpace persistentObjectSpace = (XPObjectSpace)ObjectSpaceProvider.CreateObjectSpace();
                    SelectedData sprocData = persistentObjectSpace.Session.ExecuteSproc("RegenerateDO");
                
                    if (sprocData.ResultSet.Count() > 0)
                    {
                        if (sprocData.ResultSet[0].Rows.Count() > 0)
                        {
                            foreach (SelectStatementResultRow row in sprocData.ResultSet[0].Rows)
                            {
                                if (allload == null)
                                {
                                    allload = row.Values[0].ToString();
                                }
                                else
                                {
                                    allload = allload + "," + row.Values[0].ToString();
                                }
                            }
                        }
                    }

                    if (allload != null)
                    {
                        string[] loadnum = allload.Split(',');

                        foreach (string dtlloadnum in loadnum)
                        {
                            string getso = "EXEC GenerateDO '" + dtlloadnum + "'";
                            if (conn.State == ConnectionState.Open)
                            {
                                conn.Close();
                            }
                            conn.Open();
                            SqlCommand cmdgendo = new SqlCommand(getso, conn);
                            SqlDataReader readerDO = cmdgendo.ExecuteReader();
                            while (readerDO.Read())
                            {
                                IObjectSpace soos = ObjectSpaceProvider.CreateObjectSpace();
                                SalesOrder so = soos.FindObject<SalesOrder>(CriteriaOperator.Parse("DocNum = ?", readerDO.GetString(0)));

                                if (so != null)
                                {
                                    IObjectSpace doos = ObjectSpaceProvider.CreateObjectSpace();
                                    DeliveryOrder delivery = doos.FindObject<DeliveryOrder>(CriteriaOperator.Parse("LoadingNo = ? " +
                                        "and SONo = ?", dtlloadnum, so.DocNum));

                                    if (delivery == null)
                                    {
                                        IObjectSpace loados = ObjectSpaceProvider.CreateObjectSpace();
                                        Load currload = loados.FindObject<Load>(CriteriaOperator.Parse("DocNum = ?", dtlloadnum));

                                        string picklistnum = null;
                                        IObjectSpace deliveryos = ObjectSpaceProvider.CreateObjectSpace();
                                        DeliveryOrder newdelivery = deliveryos.CreateObject<DeliveryOrder>();

                                        GeneralControllers genCon = new GeneralControllers();
                                        string docprefix = GetDocPrefix();
                                        newdelivery.DocNum = genCon.GenerateDocNum(DocTypeList.DO, deliveryos, TransferType.NA, 0, docprefix);
                                        newdelivery.Customer = newdelivery.Session.GetObjectByKey<vwBusniessPartner>(so.Customer.BPCode);
                                        newdelivery.CustomerName = so.CustomerName;
                                        newdelivery.Status = DocStatus.Submitted;
                                        // Start ver 1.0.8.1
                                        newdelivery.Priority = newdelivery.Session.GetObjectByKey<PriorityType>(so.Priority.Oid);
                                        // End ver 1.0.8.1
                                        // Start ver 1.0.10
                                        newdelivery.Remarks = so.Remarks;
                                        // End ver 1.0.10
                                        // Start ver 1.0.18
                                        // Buyer
                                        if (so.EIVConsolidate != null)
                                        {
                                            newdelivery.EIVConsolidate = newdelivery.Session.FindObject<vwYesNo>(CriteriaOperator.Parse("Code = ?", so.EIVConsolidate.Code));
                                        }
                                        if (so.EIVType != null)
                                        {
                                            newdelivery.EIVType = newdelivery.Session.FindObject<vwEIVType>(CriteriaOperator.Parse("Code = ?", so.EIVType.Code));
                                        }
                                        if (so.EIVFreqSync != null)
                                        {
                                            newdelivery.EIVFreqSync = newdelivery.Session.FindObject<vwEIVFreqSync>(CriteriaOperator.Parse("Code = ?", so.EIVFreqSync.Code));
                                        }
                                        newdelivery.EIVBuyerName = so.CustomerName;
                                        newdelivery.EIVBuyerTIN = so.EIVBuyerTIN;
                                        newdelivery.EIVBuyerRegNum = so.EIVBuyerRegNum;
                                        if (so.EIVBuyerRegTyp != null)
                                        {
                                            newdelivery.EIVBuyerRegTyp = newdelivery.Session.FindObject<vwEIVRegType>(CriteriaOperator.Parse("Code = ?", so.EIVBuyerRegTyp.Code));
                                        }
                                        newdelivery.EIVBuyerSSTRegNum = so.EIVBuyerSSTRegNum;
                                        newdelivery.EIVBuyerEmail = so.EIVBuyerEmail;
                                        newdelivery.EIVBuyerContact = so.EIVBuyerContact;
                                        newdelivery.EIVAddressLine1B = so.EIVAddressLine1B;
                                        newdelivery.EIVAddressLine2B = so.EIVAddressLine2B;
                                        newdelivery.EIVAddressLine3B = so.EIVAddressLine3B;
                                        newdelivery.EIVPostalZoneB = so.EIVPostalZoneB;
                                        newdelivery.EIVCityNameB = so.EIVCityNameB;
                                        if (so.EIVStateB != null)
                                        {
                                            newdelivery.EIVStateB = newdelivery.Session.FindObject<vwState>(CriteriaOperator.Parse("Code = ?", so.EIVStateB.Code));
                                        }
                                        if (so.EIVCountryB != null)
                                        {
                                            newdelivery.EIVCountryB = newdelivery.Session.FindObject<vwCountry>(CriteriaOperator.Parse("Code = ?", so.EIVCountryB.Code));
                                        }
                                        //Recipient
                                        newdelivery.EIVShippingName = so.EIVShippingName;
                                        newdelivery.EIVShippingTin = so.EIVShippingTin;
                                        newdelivery.EIVShippingRegNum = so.EIVShippingRegNum;
                                        if (so.EIVShippingRegTyp != null)
                                        {
                                            newdelivery.EIVShippingRegTyp = newdelivery.Session.FindObject<vwEIVRegType>(CriteriaOperator.Parse("Code = ?", so.EIVShippingRegTyp.Code));
                                        }
                                        newdelivery.EIVAddressLine1S = so.EIVAddressLine1S;
                                        newdelivery.EIVAddressLine2S = so.EIVAddressLine2S;
                                        newdelivery.EIVAddressLine3S = so.EIVAddressLine3S;
                                        newdelivery.EIVPostalZoneS = so.EIVPostalZoneS;
                                        newdelivery.EIVCityNameS = so.EIVCityNameS;
                                        if (so.EIVStateS != null)
                                        {
                                            newdelivery.EIVStateS = newdelivery.Session.FindObject<vwState>(CriteriaOperator.Parse("Code = ?", so.EIVStateS.Code));
                                        }
                                        if (so.EIVCountryS != null)
                                        {
                                            newdelivery.EIVCountryS = newdelivery.Session.FindObject<vwCountry>(CriteriaOperator.Parse("Code = ?", so.EIVCountryS.Code));
                                        }
                                        // End ver 1.0.18

                                        string[] packlistnum = currload.PackListNo.Replace(" ", "").Split(',');
                                        foreach (string dtlpack in packlistnum)
                                        {
                                            if (dtlpack != null)
                                            {
                                                foreach (LoadDetails dtlload in currload.LoadDetails)
                                                {
                                                    if (dtlload.PackList == dtlpack)
                                                    {
                                                        PackList pl = deliveryos.FindObject<PackList>(CriteriaOperator.Parse("DocNum = ?", dtlpack));

                                                        newdelivery.CustomerGroup = pl.CustomerGroup;

                                                        foreach (PackListDetails dtlpackdetail in pl.PackListDetails)
                                                        {
                                                            if (dtlload.Bundle.BundleID == dtlpackdetail.Bundle.BundleID)
                                                            {
                                                                string picklistoid = null;
                                                                // Start ver 1.0.22
                                                                string SOBaseID = null;
                                                                // End ver 1.0.22
                                                                bool pickitem = false;

                                                                PickList picklist = deliveryos.FindObject<PickList>(CriteriaOperator.Parse("DocNum = ?", dtlpackdetail.PickListNo));

                                                                foreach (PickListDetailsActual dtlactual in picklist.PickListDetailsActual)
                                                                {
                                                                    if (dtlpackdetail.BaseId == dtlactual.Oid.ToString())
                                                                    {
                                                                        picklistoid = dtlactual.PickListDetailOid.ToString();
                                                                        // Start ver 1.0.22
                                                                        SOBaseID = dtlactual.SOBaseId.ToString();
                                                                        // End ver 1.0.22

                                                                        if (dtlactual.SOBaseDoc == so.DocNum)
                                                                        {
                                                                            pickitem = true;
                                                                        }
                                                                        break;
                                                                    }
                                                                }

                                                                foreach (DeliveryOrderDetails dtldelivery in newdelivery.DeliveryOrderDetails)
                                                                {
                                                                    // Start ver 1.0.22
                                                                    //if (dtldelivery.PackListLine == picklistoid)
                                                                    if (dtldelivery.SOBaseID == SOBaseID)
                                                                    // End ver 1.0.22
                                                                    {
                                                                        dtldelivery.Quantity = dtldelivery.Quantity + dtlpackdetail.Quantity;
                                                                        pickitem = false;
                                                                        break;
                                                                    }
                                                                }

                                                                if (pickitem == true)
                                                                {
                                                                    if (dtlpackdetail.Quantity > 0)
                                                                    {
                                                                        DeliveryOrderDetails newdeliveryitem = deliveryos.CreateObject<DeliveryOrderDetails>();

                                                                        newdeliveryitem.ItemCode = newdeliveryitem.Session.GetObjectByKey<vwItemMasters>(dtlpackdetail.ItemCode.ItemCode);
                                                                        newdeliveryitem.Quantity = dtlpackdetail.Quantity;
                                                                        newdeliveryitem.PackListLine = picklistoid;

                                                                        if (dtlload.Bin != null)
                                                                        {
                                                                            newdeliveryitem.Warehouse = newdeliveryitem.Session.GetObjectByKey<vwWarehouse>(dtlload.Bin.Warehouse);
                                                                            newdeliveryitem.Bin = newdeliveryitem.Session.GetObjectByKey<vwBin>(dtlload.Bin.BinCode);
                                                                        }

                                                                        //GET SO
                                                                        foreach (PickListDetails dtlpick in picklist.PickListDetails)
                                                                        {
                                                                            if (dtlpick.Oid.ToString() == picklistoid)
                                                                            {
                                                                                foreach (SalesOrderDetails dtlsales in so.SalesOrderDetails)
                                                                                {
                                                                                    if (dtlsales.ItemCode.ItemCode == dtlpackdetail.ItemCode.ItemCode &&
                                                                                        dtlsales.Oid.ToString() == dtlpick.SOBaseId)
                                                                                    {
                                                                                        newdeliveryitem.Price = dtlsales.AdjustedPrice;
                                                                                        // Start ver 1.0.18
                                                                                        if (dtlsales.EIVClassification != null)
                                                                                        {
                                                                                            newdeliveryitem.EIVClassification = newdeliveryitem.Session.FindObject<vwEIVClass>
                                                                                                (CriteriaOperator.Parse("Code = ?", dtlsales.EIVClassification.Code));
                                                                                        }
                                                                                        // End ver 1.0.18
                                                                                    }
                                                                                }

                                                                                newdeliveryitem.SOBaseID = dtlpick.SOBaseId;
                                                                                newdelivery.CustomerGroup = picklist.CustomerGroup;
                                                                            }
                                                                        }

                                                                        newdeliveryitem.BaseDoc = dtlloadnum;
                                                                        newdeliveryitem.BaseId = dtlload.Oid.ToString();
                                                                        newdeliveryitem.SODocNum = so.DocNum;
                                                                        //newdeliveryitem.SOBaseID = dtlpick.SOBaseId;
                                                                        newdeliveryitem.PickListDocNum = dtlpackdetail.PickListNo;

                                                                        newdelivery.DeliveryOrderDetails.Add(newdeliveryitem);
                                                                    }

                                                                    picklistnum = dtlpackdetail.PickListNo;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        // Start ver 1.0.8.1
                                        string dupno = null;
                                        string dupso = null;
                                        foreach (DeliveryOrderDetails dtl in newdelivery.DeliveryOrderDetails)
                                        {
                                            if (dupno != dtl.BaseDoc)
                                            {
                                                if (newdelivery.LoadingNo == null)
                                                {
                                                    newdelivery.LoadingNo = dtl.BaseDoc;
                                                }
                                                else
                                                {
                                                    newdelivery.LoadingNo = newdelivery.LoadingNo + ", " + dtl.BaseDoc;
                                                }

                                                dupno = dtl.BaseDoc;
                                            }

                                            if (dupso != dtl.SODocNum)
                                            {
                                                if (newdelivery.SONo == null)
                                                {
                                                    newdelivery.SONo = dtl.SODocNum;
                                                }
                                                else
                                                {
                                                    newdelivery.SONo = newdelivery.SONo + ", " + dtl.SODocNum;
                                                }

                                                dupso = dtl.SODocNum;
                                            }

                                            // Start ver 1.0.10
                                            if (newdelivery.Warehouse == null)
                                            {
                                                newdelivery.Warehouse = newdelivery.Session.GetObjectByKey<vwWarehouse>(dtl.Warehouse.WarehouseCode);
                                            }
                                            // End ver 1.0.10
                                        }
                                        // End ver 1.0.8.1

                                        deliveryos.CommitChanges();
                                    }
                                }
                            }
                            conn.Close();
                        }
                    }

                #endregion
                }
                // End ver 1.0.13
            }
            catch (Exception ex)
            {
                WriteLog("[Error]", "Message:" + ex.Message);
            }

        // End Post ======================================================================================
        EndApplication:
            return;
        }

        private void WriteLog(string lvl, string str)
        {
            FileStream fileStream = null;

            string filePath = "C:\\" + ConfigurationManager.AppSettings["LogFileName"].ToString() + "\\";
            filePath = filePath + "[" + "Posting Status" + "] Log_" + System.DateTime.Today.ToString("yyyyMMdd") + "." + "txt";

            FileInfo fileInfo = new FileInfo(filePath);
            DirectoryInfo dirInfo = new DirectoryInfo(fileInfo.DirectoryName);
            if (!dirInfo.Exists) dirInfo.Create();

            if (!fileInfo.Exists)
            {
                fileStream = fileInfo.Create();
            }
            else
            {
                fileStream = new FileStream(filePath, FileMode.Append);
            }

            StreamWriter log = new StreamWriter(fileStream);
            string status = lvl.ToString().Replace("[Log]", "");

            //For Portal_UpdateStatus_Log
            log.WriteLine("{0}{1}", status, str.ToString());

            log.Close();
        }

        // Start ver 1.0.13
        public string GetDocPrefix()
        {
            string prefix = null;

            string getcompany = "SELECT CompanyPrefix FROM [" + ConfigurationManager.AppSettings.Get("CommonTable").ToString() + "]..ODBC WHERE " +
                "DBName = '" + conn1.Database + "'";
            if (conn1.State == ConnectionState.Open)
            {
                conn1.Close();
            }
            conn1.Open();
            SqlCommand cmd = new SqlCommand(getcompany, conn1);
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                prefix = reader.GetString(0);
            }
            conn1.Close();

            return prefix;
        }
        // End ver 1.0.13

        public int PostSOtoSAP(SalesOrder oTargetDoc, IObjectSpaceProvider ObjectSpaceProvider, SAPCompany sap)
        {
            // return 0 = post nothing
            // return -1 = posting error
            // return 1 = posting successful
            try
            {
                if (!oTargetDoc.Sap)
                {
                    IObjectSpace fos = ObjectSpaceProvider.CreateObjectSpace();
                    bool found = false;
                    DateTime postdate = DateTime.Now;

                    Guid g;
                    // Create and display the value of two GUIDs.
                    g = Guid.NewGuid();

                    int sapempid = 0;
                    SAPbobsCOM.Documents oDoc = null;

                    oDoc = (SAPbobsCOM.Documents)sap.oCom.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oOrders);

                    oDoc.DocType = BoDocumentTypes.dDocument_Items;
                    oDoc.CardCode = oTargetDoc.Customer.BPCode;
                    oDoc.CardName = oTargetDoc.CustomerName;
                    oDoc.DocDate = oTargetDoc.PostingDate;
                    oDoc.DocDueDate = oTargetDoc.DeliveryDate;
                    oDoc.TaxDate = oTargetDoc.DocDate;
                    oDoc.Comments = oTargetDoc.Remarks;
                    oDoc.UserFields.Fields.Item("U_PortalDocNum").Value = oTargetDoc.DocNum;
                    if (oTargetDoc.Transporter != null)
                    {
                        oDoc.UserFields.Fields.Item("U_Transporter").Value = oTargetDoc.Transporter.TransporterID;
                    }
                    if (oTargetDoc.Priority != null)
                    {
                        oDoc.UserFields.Fields.Item("U_Priority").Value = oTargetDoc.Priority.PriorityName;
                    }
                    if (oTargetDoc.ContactPerson != null)
                    {
                        oDoc.SalesPersonCode = oTargetDoc.ContactPerson.SlpCode;
                    }
                    if (oTargetDoc.BillingAddress != null)
                    {
                        oDoc.PayToCode = oTargetDoc.BillingAddress.AddressKey;
                    }
                    // Start ver 1.0.18
                    //if (oTargetDoc.BillingAddressfield != null)
                    //{
                    //    oDoc.Address = oTargetDoc.BillingAddressfield;
                    //}
                    // End ver 1.0.18
                    if (oTargetDoc.ShippingAddress != null)
                    {
                        oDoc.ShipToCode = oTargetDoc.ShippingAddress.AddressKey;
                    }
                    // Start ver 1.0.18
                    //if (oTargetDoc.ShippingAddressfield != null)
                    //{
                    //    oDoc.Address2 = oTargetDoc.ShippingAddressfield;
                    //}
                    // End ver 1.0.18

                    if (oTargetDoc.Series != null)
                    {
                        oDoc.Series = int.Parse(oTargetDoc.Series.Series);
                    }

                    // Start ver 1.0.18
                    // Buyer
                    if (oTargetDoc.EIVConsolidate != null)
                    {
                        if (oTargetDoc.EIVConsolidate.Code == "Y")
                        {
                            oDoc.UserFields.Fields.Item("U_EIV_Consolidate").Value = "N";
                        }
                        else
                        {
                            oDoc.UserFields.Fields.Item("U_EIV_Consolidate").Value = "Y";
                        }
                    }
                    if (oTargetDoc.EIVType != null)
                    {
                        oDoc.UserFields.Fields.Item("U_EIV_InvoiceType").Value = oTargetDoc.EIVType.Code;
                    }
                    if (oTargetDoc.EIVFreqSync != null)
                    {
                        oDoc.UserFields.Fields.Item("U_EIV_FreqSync").Value = oTargetDoc.EIVFreqSync.Code;
                    }
                    oDoc.UserFields.Fields.Item("U_EIV_BuyerName").Value = oTargetDoc.CustomerName == null ? "" : oTargetDoc.CustomerName;
                    oDoc.UserFields.Fields.Item("U_EIV_BuyerTin").Value = oTargetDoc.EIVBuyerTIN == null ? "" : oTargetDoc.EIVBuyerTIN;
                    oDoc.UserFields.Fields.Item("U_EIV_BuyerRegNum").Value = oTargetDoc.EIVBuyerRegNum == null ? "" : oTargetDoc.EIVBuyerRegNum;
                    if (oTargetDoc.EIVBuyerRegTyp != null)
                    {
                        oDoc.UserFields.Fields.Item("U_EIV_BuyerRegTyp").Value = oTargetDoc.EIVBuyerRegTyp.Code;
                    }
                    oDoc.UserFields.Fields.Item("U_EIV_BuyerSSTRegNum").Value = oTargetDoc.EIVBuyerSSTRegNum == null ? "" : oTargetDoc.EIVBuyerSSTRegNum;
                    oDoc.UserFields.Fields.Item("U_EIV_BuyerEmail").Value = oTargetDoc.EIVBuyerEmail == null ? "" : oTargetDoc.EIVBuyerEmail;
                    oDoc.UserFields.Fields.Item("U_EIV_BuyerContact").Value = oTargetDoc.EIVBuyerContact == null ? "" : oTargetDoc.EIVBuyerContact;

                    oDoc.AddressExtension.BillToStreet = oTargetDoc.EIVAddressLine1B == null ? "" : oTargetDoc.EIVAddressLine1B;
                    oDoc.AddressExtension.BillToBlock = oTargetDoc.EIVAddressLine2B == null ? "" : oTargetDoc.EIVAddressLine2B;
                    oDoc.AddressExtension.BillToCity = oTargetDoc.EIVAddressLine3B == null ? "" : oTargetDoc.EIVAddressLine3B;
                    oDoc.AddressExtension.BillToCounty = oTargetDoc.EIVCityNameB == null ? "" : oTargetDoc.EIVCityNameB;
                    oDoc.AddressExtension.BillToZipCode = oTargetDoc.EIVPostalZoneB == null ? "" : oTargetDoc.EIVPostalZoneB;

                    oDoc.AddressExtension.UserFields.Fields.Item("U_EIV_AddressLine1B").Value = oTargetDoc.EIVAddressLine1B == null ? "" : oTargetDoc.EIVAddressLine1B;
                    oDoc.AddressExtension.UserFields.Fields.Item("U_EIV_AddressLine2B").Value = oTargetDoc.EIVAddressLine2B == null ? "" : oTargetDoc.EIVAddressLine2B;
                    oDoc.AddressExtension.UserFields.Fields.Item("U_EIV_AddressLine3B").Value = oTargetDoc.EIVAddressLine3B == null ? "" : oTargetDoc.EIVAddressLine3B;
                    oDoc.AddressExtension.UserFields.Fields.Item("U_EIV_PostalZoneB").Value = oTargetDoc.EIVPostalZoneB == null ? "" : oTargetDoc.EIVPostalZoneB;
                    oDoc.AddressExtension.UserFields.Fields.Item("U_EIV_CityNameB").Value = oTargetDoc.EIVCityNameB == null ? "" : oTargetDoc.EIVCityNameB;
                    if (oTargetDoc.EIVStateB != null)
                    {
                        oDoc.AddressExtension.BillToState = oTargetDoc.EIVStateB.Code;
                    }
                    if (oTargetDoc.EIVCountryB != null)
                    {
                        oDoc.AddressExtension.BillToCountry = oTargetDoc.EIVCountryB.Code;
                    }

                    // Recipient
                    oDoc.UserFields.Fields.Item("U_EIV_ShippingName").Value = oTargetDoc.EIVShippingName == null ? "" : oTargetDoc.EIVShippingName;
                    oDoc.UserFields.Fields.Item("U_EIV_ShippingTin").Value = oTargetDoc.EIVShippingTin == null ? "" : oTargetDoc.EIVShippingTin;
                    oDoc.UserFields.Fields.Item("U_EIV_ShippingRegNum").Value = oTargetDoc.EIVShippingRegNum == null ? "" : oTargetDoc.EIVShippingRegNum;
                    if (oTargetDoc.EIVShippingRegTyp != null)
                    {
                        oDoc.UserFields.Fields.Item("U_EIV_ShippingRegTyp").Value = oTargetDoc.EIVShippingRegTyp.Code;
                    }
                    oDoc.AddressExtension.ShipToStreet = oTargetDoc.EIVAddressLine1S == null ? "" : oTargetDoc.EIVAddressLine1S;
                    oDoc.AddressExtension.ShipToBlock = oTargetDoc.EIVAddressLine2S == null ? "" : oTargetDoc.EIVAddressLine2S;
                    oDoc.AddressExtension.ShipToCity = oTargetDoc.EIVAddressLine3S == null ? "" : oTargetDoc.EIVAddressLine3S;
                    oDoc.AddressExtension.ShipToCounty = oTargetDoc.EIVCityNameS == null ? "" : oTargetDoc.EIVCityNameS;
                    oDoc.AddressExtension.ShipToZipCode = oTargetDoc.EIVPostalZoneS == null ? "" : oTargetDoc.EIVPostalZoneS;

                    oDoc.AddressExtension.UserFields.Fields.Item("U_EIV_AddressLine1S").Value = oTargetDoc.EIVAddressLine1S == null ? "" : oTargetDoc.EIVAddressLine1S;
                    oDoc.AddressExtension.UserFields.Fields.Item("U_EIV_AddressLine2S").Value = oTargetDoc.EIVAddressLine2S == null ? "" : oTargetDoc.EIVAddressLine2S;
                    oDoc.AddressExtension.UserFields.Fields.Item("U_EIV_AddressLine3S").Value = oTargetDoc.EIVAddressLine3S == null ? "" : oTargetDoc.EIVAddressLine3S;
                    oDoc.AddressExtension.UserFields.Fields.Item("U_EIV_PostalZoneS").Value = oTargetDoc.EIVPostalZoneS == null ? "" : oTargetDoc.EIVPostalZoneS;
                    oDoc.AddressExtension.UserFields.Fields.Item("U_EIV_CityNameS").Value = oTargetDoc.EIVCityNameS == null ? "" : oTargetDoc.EIVCityNameS;
                    if (oTargetDoc.EIVStateS != null)
                    {
                        oDoc.AddressExtension.ShipToState = oTargetDoc.EIVStateS.Code;
                    }
                    if (oTargetDoc.EIVCountryS != null)
                    {
                        oDoc.AddressExtension.ShipToCountry = oTargetDoc.EIVCountryS.Code;
                    }
                    // End ver 1.0.18

                    if (sapempid > 0)
                        oDoc.DocumentsOwner = sapempid;

                    int cnt = 0;
                    foreach (SalesOrderDetails dtl in oTargetDoc.SalesOrderDetails)
                    {
                        //if (dtl.LineAmount > 0)
                        //{
                        cnt++;
                        if (cnt == 1)
                        {
                        }
                        else
                        {
                            oDoc.Lines.Add();
                            oDoc.Lines.SetCurrentLine(oDoc.Lines.Count - 1);
                        }

                        //oDoc.Lines.VatGroup = dtl.Tax.BoCode;
                        //oDoc.Lines.TaxTotal = (double)dtl.TaxAmount;
                        oDoc.Lines.WarehouseCode = dtl.Location.WarehouseCode;
                        oDoc.Lines.UserFields.Fields.Item("U_PortalLineOid").Value = dtl.Oid.ToString();
                        // Start ver 1.0.18
                        if (dtl.EIVClassification != null)
                        {
                            oDoc.Lines.UserFields.Fields.Item("U_EIV_Classification").Value = dtl.EIVClassification.Code;
                        }
                        // End ver 1.0.18

                        oDoc.Lines.ItemCode = dtl.ItemCode.ItemCode;
                        oDoc.Lines.ItemDetails = dtl.ItemDesc;
                        oDoc.Lines.Quantity = (double)dtl.Quantity;
                        oDoc.Lines.UnitPrice = (double)dtl.AdjustedPrice;
                    }

                    int rc = oDoc.Add();
                    if (rc != 0)
                    {
                        string temp = sap.oCom.GetLastErrorDescription();
                        if (sap.oCom.InTransaction)
                        {
                            sap.oCom.EndTransaction(BoWfTransOpt.wf_RollBack);
                        }

                        IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                        SalesOrder obj = osupdate.GetObjectByKey<SalesOrder>(oTargetDoc.Oid);

                        SalesOrderDocStatus ds = osupdate.CreateObject<SalesOrderDocStatus>();
                        ds.CreateUser = osupdate.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                        ds.CreateDate = DateTime.Now;
                        ds.DocStatus = DocStatus.PendPost;
                        ds.DocRemarks = "SAP Error:" + temp;
                        obj.SalesOrderDocStatus.Add(ds);

                        osupdate.CommitChanges();

                        WriteLog("[Error]", "Message: SO Posting :" + oTargetDoc + "-" + temp);
                        return -1;
                    }
                    return 1;
                }
                return 0;
            }
            catch (Exception ex)
            {
                IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                SalesOrder obj = osupdate.GetObjectByKey<SalesOrder>(oTargetDoc.Oid);

                SalesOrderDocStatus ds = osupdate.CreateObject<SalesOrderDocStatus>();
                ds.CreateUser = osupdate.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                ds.CreateDate = DateTime.Now;
                ds.DocStatus = DocStatus.PendPost;
                ds.DocRemarks = "SAP Error:" + ex.Message;
                obj.SalesOrderDocStatus.Add(ds);

                osupdate.CommitChanges();

                WriteLog("[Error]", "Message: SO Posting :" + oTargetDoc + "-" + ex.Message);
                return -1;
            }
        }

        public int PostPOtoSAP(PurchaseOrders oTargetDoc, IObjectSpaceProvider ObjectSpaceProvider, SAPCompany sap)
        {
            // return 0 = post nothing
            // return -1 = posting error
            // return 1 = posting successful
            try
            {
                if (!oTargetDoc.Sap)
                {
                    IObjectSpace fos = ObjectSpaceProvider.CreateObjectSpace();
                    bool found = false;
                    DateTime postdate = DateTime.Now;

                    Guid g;
                    // Create and display the value of two GUIDs.
                    g = Guid.NewGuid();

                    if (oTargetDoc.PurchaseOrderAttachment != null && oTargetDoc.PurchaseOrderAttachment.Count > 0)
                    {
                        foreach (PurchaseOrderAttachment obj in oTargetDoc.PurchaseOrderAttachment)
                        {
                            string fullpath = ConfigurationManager.AppSettings["B1AttachmentPath"].ToString() + g.ToString() + obj.File.FileName;
                            using (System.IO.FileStream fs = System.IO.File.OpenWrite(fullpath))
                            {
                                obj.File.SaveToStream(fs);
                            }
                        }
                    }

                    int sapempid = 0;
                    SAPbobsCOM.Documents oDoc = null;

                    oDoc = (SAPbobsCOM.Documents)sap.oCom.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oPurchaseOrders);

                    oDoc.DocType = BoDocumentTypes.dDocument_Items;
                    oDoc.CardCode = oTargetDoc.Supplier.BPCode;
                    oDoc.CardName = oTargetDoc.SupplierName;
                    oDoc.DocDate = oTargetDoc.PostingDate;
                    oDoc.DocDueDate = oTargetDoc.DeliveryDate;
                    //oDoc.TaxDate = oTargetDoc.DocDate;
                    oDoc.Comments = oTargetDoc.Remarks;
                    oDoc.UserFields.Fields.Item("U_PortalDocNum").Value = oTargetDoc.DocNum;

                    if (oTargetDoc.Series != null)
                    {
                        oDoc.Series = int.Parse(oTargetDoc.Series.Series);
                    }

                    if (sapempid > 0)
                        oDoc.DocumentsOwner = sapempid;

                    int cnt = 0;
                    foreach (PurchaseOrderDetails dtl in oTargetDoc.PurchaseOrderDetails)
                    {
                        //if (dtl.LineAmount > 0)
                        //{
                        cnt++;
                        if (cnt == 1)
                        {
                        }
                        else
                        {
                            oDoc.Lines.Add();
                            oDoc.Lines.SetCurrentLine(oDoc.Lines.Count - 1);
                        }

                        //oDoc.Lines.VatGroup = dtl.Tax.BoCode;
                        //oDoc.Lines.TaxTotal = (double)dtl.TaxAmount;
                        oDoc.Lines.WarehouseCode = dtl.Location.WarehouseCode;
                        oDoc.Lines.UserFields.Fields.Item("U_PortalLineOid").Value = dtl.Oid.ToString();
                        // Start ver 1.0.12
                        if (dtl.FOC == true)
                        {
                            oDoc.Lines.UserFields.Fields.Item("U_FOC").Value = "Y";
                        }
                        else
                        {
                            oDoc.Lines.UserFields.Fields.Item("U_FOC").Value = "N";
                        }
                        // End ver 1.0.12

                        oDoc.Lines.ItemCode = dtl.ItemCode.ItemCode;
                        oDoc.Lines.ItemDetails = dtl.ItemDesc;
                        oDoc.Lines.Quantity = (double)dtl.Quantity;
                        oDoc.Lines.UnitPrice = (double)dtl.AdjustedPrice;
                        if (dtl.BaseDoc != null)
                        {
                            oDoc.Lines.UserFields.Fields.Item("U_BaseDocNum").Value = dtl.BaseDoc;
                        }

                    }
                    if (oTargetDoc.PurchaseOrderAttachment != null && oTargetDoc.PurchaseOrderAttachment.Count > 0)
                    {
                        cnt = 0;
                        SAPbobsCOM.Attachments2 oAtt = (SAPbobsCOM.Attachments2)sap.oCom.GetBusinessObject(BoObjectTypes.oAttachments2);
                        foreach (PurchaseOrderAttachment dtl in oTargetDoc.PurchaseOrderAttachment)
                        {

                            cnt++;
                            if (cnt == 1)
                            {
                                if (oAtt.Lines.Count == 0)
                                    oAtt.Lines.Add();
                            }
                            else
                                oAtt.Lines.Add();

                            string attfile = "";
                            string[] fexe = dtl.File.FileName.Split('.');
                            if (fexe.Length <= 2)
                                attfile = fexe[0];
                            else
                            {
                                for (int x = 0; x < fexe.Length - 1; x++)
                                {
                                    if (attfile == "")
                                        attfile = fexe[x];
                                    else
                                        attfile += "." + fexe[x];
                                }
                            }
                            oAtt.Lines.FileName = g.ToString() + attfile;
                            if (fexe.Length > 1)
                                oAtt.Lines.FileExtension = fexe[fexe.Length - 1];
                            string path = ConfigurationManager.AppSettings["B1AttachmentPath"].ToString();
                            path = path.Replace("\\\\", "\\");
                            path = path.Substring(0, path.Length - 1);
                            oAtt.Lines.SourcePath = path;
                            oAtt.Lines.Override = SAPbobsCOM.BoYesNoEnum.tYES;
                        }
                        int iAttEntry = -1;
                        if (oAtt.Add() == 0)
                        {
                            iAttEntry = int.Parse(sap.oCom.GetNewObjectKey());
                        }
                        else
                        {
                            string temp = sap.oCom.GetLastErrorDescription();
                            if (sap.oCom.InTransaction)
                            {
                                sap.oCom.EndTransaction(BoWfTransOpt.wf_RollBack);
                            }

                            IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                            PurchaseOrders obj = osupdate.GetObjectByKey<PurchaseOrders>(oTargetDoc.Oid);

                            PurchaseOrderDocTrail ds = osupdate.CreateObject<PurchaseOrderDocTrail>();
                            ds.CreateUser = osupdate.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                            ds.CreateDate = DateTime.Now;
                            ds.DocStatus = DocStatus.PendPost;
                            ds.DocRemarks = "SAP Error:" + temp;
                            obj.PurchaseOrderDocTrail.Add(ds);

                            osupdate.CommitChanges();

                            WriteLog("[Error]", "Message: PO Attachement Error :" + oTargetDoc + "-" + temp);
                            return -1;
                        }
                        oDoc.AttachmentEntry = iAttEntry;
                    }

                    int rc = oDoc.Add();
                    if (rc != 0)
                    {
                        string temp = sap.oCom.GetLastErrorDescription();
                        if (sap.oCom.InTransaction)
                        {
                            sap.oCom.EndTransaction(BoWfTransOpt.wf_RollBack);
                        }

                        IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                        PurchaseOrders obj = osupdate.GetObjectByKey<PurchaseOrders>(oTargetDoc.Oid);

                        PurchaseOrderDocTrail ds = osupdate.CreateObject<PurchaseOrderDocTrail>();
                        ds.CreateUser = osupdate.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                        ds.CreateDate = DateTime.Now;
                        ds.DocStatus = DocStatus.PendPost;
                        ds.DocRemarks = "SAP Error:" + temp;
                        obj.PurchaseOrderDocTrail.Add(ds);

                        osupdate.CommitChanges();

                        WriteLog("[Error]", "Message: PO Posting :" + oTargetDoc + "-" + temp);
                        return -1;
                    }
                    return 1;
                }
                return 0;
            }
            catch (Exception ex)
            {
                IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                PurchaseOrders obj = osupdate.GetObjectByKey<PurchaseOrders>(oTargetDoc.Oid);

                PurchaseOrderDocTrail ds = osupdate.CreateObject<PurchaseOrderDocTrail>();
                ds.CreateUser = osupdate.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                ds.CreateDate = DateTime.Now;
                ds.DocStatus = DocStatus.PendPost;
                ds.DocRemarks = "SAP Error:" + ex.Message;
                obj.PurchaseOrderDocTrail.Add(ds);

                osupdate.CommitChanges();

                WriteLog("[Error]", "Message: PO Posting :" + oTargetDoc + "-" + ex.Message);
                return -1;
            }
        }

        public int PostGRNtoSAP(GRN oTargetDoc, IObjectSpaceProvider ObjectSpaceProvider, SAPCompany sap)
        {
            // return 0 = post nothing
            // return -1 = posting error
            // return 1 = posting successful
            try
            {
                if (!oTargetDoc.Sap)
                {
                    bool found = false;
                    string asndoc = null;
                    string dupasndoc = null;
                    DateTime postdate = DateTime.Now;
                    IObjectSpace fos = ObjectSpaceProvider.CreateObjectSpace();

                    Guid g;
                    // Create and display the value of two GUIDs.
                    g = Guid.NewGuid();

                    int sapempid = 0;

                    SAPbobsCOM.Documents oDoc = null;
                    SAPbobsCOM.Recordset rs = null;

                    rs = (SAPbobsCOM.Recordset)sap.oCom.GetBusinessObject(SAPbobsCOM.BoObjectTypes.BoRecordset);
                    oDoc = (SAPbobsCOM.Documents)sap.oCom.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oPurchaseDeliveryNotes);

                    oDoc.DocType = BoDocumentTypes.dDocument_Items;
                    oDoc.CardCode = oTargetDoc.Supplier.BPCode;
                    oDoc.CardName = oTargetDoc.SupplierName;
                    // Start ver 1.0.19
                    //oDoc.DocDate = oTargetDoc.DocDate;
                    oDoc.DocDate = DateTime.Today;
                    oDoc.TaxDate = oTargetDoc.DocDate;
                    // End ver 1.0.19
                    //oDoc.Comments = oTargetDoc.Remarks;
                    oDoc.UserFields.Fields.Item("U_PortalDocNum").Value = oTargetDoc.DocNum;
                    if (oTargetDoc.InvoiceNo != null)
                    {
                        oDoc.NumAtCard = oTargetDoc.InvoiceNo;
                    }

                    if (sapempid > 0)
                        oDoc.DocumentsOwner = sapempid;

                    int cnt = 0;
                    foreach (GRNDetails dtl in oTargetDoc.GRNDetails)
                    {
                        if (dtl.Received > 0)
                        {
                            cnt++;
                            if (cnt == 1)
                            {
                            }
                            else
                            {
                                //oDoc.Lines.BatchNumbers.Add();
                                //oDoc.Lines.BatchNumbers.SetCurrentLine(oDoc.Lines.Count - 1);
                                oDoc.Lines.Add();
                                oDoc.Lines.SetCurrentLine(oDoc.Lines.Count - 1);
                            }

                            //oDoc.Lines.TaxCode = dtl.Tax.BoCode;
                            //oDoc.Lines.TaxTotal = (double)dtl.TaxAmount;
                            oDoc.Lines.WarehouseCode = dtl.Location.WarehouseCode;
                            oDoc.Lines.UserFields.Fields.Item("U_PortalLineOid").Value = dtl.Oid.ToString();

                            oDoc.Lines.ItemCode = dtl.ItemCode.ItemCode;
                            oDoc.Lines.ItemDescription = dtl.ItemDesc;
                            oDoc.Lines.Quantity = (double)dtl.Received;// * (double)link.Packsize;
                            if (dtl.DiscrepancyReason != null)
                            {
                                oDoc.Lines.UserFields.Fields.Item("U_DiscreReason").Value = dtl.DiscrepancyReason;
                            }
                            if (dtl.DefBin != null)
                            {
                                oDoc.Lines.BinAllocations.BinAbsEntry = dtl.DefBin.AbsEntry;
                                oDoc.Lines.BinAllocations.Quantity = (double)dtl.Received;
                                //oDoc.Lines.BinAllocations.Add();
                            }

                            //oDoc.Lines.UnitPrice = (double)dtl.UnitPrice;// / oDoc.Lines.Quantity;

                            if (dtl.BaseDoc != null)
                            {
                                oDoc.Lines.BaseType = 22;
                                oDoc.Lines.BaseEntry = int.Parse(dtl.BaseDoc);//Docentry
                                oDoc.Lines.BaseLine = int.Parse(dtl.BaseId);//line no

                                string po = "SELECT DocNum FROM [" + ConfigurationManager.AppSettings["CompanyDB"].ToString() +
                                    "]..OPOR WHERE DocEntry = " + int.Parse(dtl.BaseDoc);

                                rs.DoQuery(po);

                                if (rs.RecordCount > 0)
                                {
                                    oDoc.Comments = rs.Fields.Item("DocNum").Value.ToString();
                                }
                            }

                            if (dtl.ASNBaseDoc != null)
                            {
                                oDoc.Lines.BaseType = 22;
                                oDoc.Lines.BaseEntry = int.Parse(dtl.ASNPOBaseDoc);//Docentry
                                oDoc.Lines.BaseLine = int.Parse(dtl.ASNPOBaseId);//line no

                                string po = "SELECT DocNum FROM [" + ConfigurationManager.AppSettings["CompanyDB"].ToString() +
                                    "]..OPOR WHERE DocEntry = " + int.Parse(dtl.ASNPOBaseDoc);

                                rs.DoQuery(po);

                                if (rs.RecordCount > 0)
                                {
                                    oDoc.Comments = rs.Fields.Item("DocNum").Value.ToString();
                                }

                                if (dupasndoc != dtl.ASNBaseDoc)
                                {
                                    if (asndoc != null)
                                    {
                                        asndoc = asndoc + ", " + dtl.ASNBaseDoc;
                                    }
                                    else
                                    {
                                        asndoc = dtl.ASNBaseDoc;
                                    }

                                    dupasndoc = dtl.ASNBaseDoc;
                                }
                            }
                        }
                    }

                    if (asndoc != null)
                    {
                        int countchar = 0;
                        foreach (char c in asndoc)
                        {
                            countchar++;
                        }
                        if (countchar >= 99)
                        {
                            oDoc.UserFields.Fields.Item("U_PortalASNNum").Value = asndoc.Substring(0, 99).ToString();
                        }
                        else
                        {
                            oDoc.UserFields.Fields.Item("U_PortalASNNum").Value = asndoc;
                        }
                    }

                    int rc = oDoc.Add();
                    if (rc != 0)
                    {
                        string temp = sap.oCom.GetLastErrorDescription();
                        if (sap.oCom.InTransaction)
                        {
                            sap.oCom.EndTransaction(BoWfTransOpt.wf_RollBack);
                        }

                        IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                        GRN obj = osupdate.GetObjectByKey<GRN>(oTargetDoc.Oid);

                        GRNDocTrail ds = osupdate.CreateObject<GRNDocTrail>();
                        ds.CreateUser = osupdate.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                        ds.CreateDate = DateTime.Now;
                        ds.DocStatus = DocStatus.PendPost;
                        ds.DocRemarks = "SAP Error:" + temp;
                        obj.GRNDocTrail.Add(ds);

                        osupdate.CommitChanges();

                        WriteLog("[Error]", "Message: GRN Posting :" + oTargetDoc + "-" + temp);

                        return -1;
                    }
                    return 1;
                }
                return 0;
            }
            catch (Exception ex)
            {
                IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                GRN obj = osupdate.GetObjectByKey<GRN>(oTargetDoc.Oid);

                GRNDocTrail ds = osupdate.CreateObject<GRNDocTrail>();
                ds.CreateUser = osupdate.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                ds.CreateDate = DateTime.Now;
                ds.DocStatus = DocStatus.PendPost;
                ds.DocRemarks = "SAP Error:" + ex.Message;
                obj.GRNDocTrail.Add(ds);

                osupdate.CommitChanges();

                WriteLog("[Error]", "Message: GRN Posting :" + oTargetDoc + "-" + ex.Message);
                return -1;
            }
        }

        public int PostPReturntoSAP(PurchaseReturns oTargetDoc, IObjectSpaceProvider ObjectSpaceProvider, SAPCompany sap)
        {
            // return 0 = post nothing
            // return -1 = posting error
            // return 1 = posting successful
            try
            {
                if (!oTargetDoc.Sap)
                {
                    bool found = false;
                    DateTime postdate = DateTime.Now;

                    Guid g;
                    // Create and display the value of two GUIDs.
                    g = Guid.NewGuid();

                    int sapempid = 0;

                    SAPbobsCOM.Documents oDoc = null;

                    oDoc = (SAPbobsCOM.Documents)sap.oCom.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oPurchaseReturns);

                    oDoc.DocType = BoDocumentTypes.dDocument_Items;
                    oDoc.CardCode = oTargetDoc.Supplier.BPCode;
                    oDoc.CardName = oTargetDoc.SupplierName;
                    oDoc.DocDate = oTargetDoc.DocDate;
                    oDoc.Comments = oTargetDoc.Remarks;
                    oDoc.UserFields.Fields.Item("U_PortalDocNum").Value = oTargetDoc.DocNum;

                    if (sapempid > 0)
                        oDoc.DocumentsOwner = sapempid;

                    int cnt = 0;
                    foreach (PurchaseReturnDetails dtl in oTargetDoc.PurchaseReturnDetails)
                    {
                        //if (dtl.Total > 0)
                        //{
                        cnt++;
                        if (cnt == 1)
                        {
                        }
                        else
                        {
                            oDoc.Lines.Add();
                            oDoc.Lines.SetCurrentLine(oDoc.Lines.Count - 1);
                        }

                        //oDoc.Lines.TaxCode = dtl.Tax.BoCode;
                        //oDoc.Lines.TaxTotal = (double)dtl.TaxAmount;
                        oDoc.Lines.WarehouseCode = dtl.Warehouse.WarehouseCode;
                        oDoc.Lines.UserFields.Fields.Item("U_PortalLineOid").Value = dtl.Oid.ToString();
                        oDoc.Lines.ItemCode = dtl.ItemCode.ItemCode;
                        oDoc.Lines.ItemDescription = dtl.ItemDesc;
                        oDoc.Lines.Quantity = (double)dtl.Quantity;
                        oDoc.Lines.UnitPrice = (double)dtl.Price;
                        if (dtl.PODocNum != null)
                        {
                            oDoc.Lines.UserFields.Fields.Item("U_BaseDocNum").Value = dtl.PODocNum;
                        }

                        if (dtl.Bin != null)
                        {
                            oDoc.Lines.BinAllocations.BinAbsEntry = dtl.Bin.AbsEntry;
                            oDoc.Lines.BinAllocations.Quantity = (double)dtl.Quantity;
                        }

                        // Start ver 1.0.7
                        if (oTargetDoc.GRPOCorrection == true)
                        {
                            if (dtl.GRNBaseDoc != null)
                            {
                                oDoc.Lines.BaseType = 20;
                                oDoc.Lines.BaseEntry = int.Parse(dtl.GRNBaseDoc);
                                oDoc.Lines.BaseLine = int.Parse(dtl.GRNBaseId);
                            }
                        }
                        // End ver 1.0.7
                        //}
                    }

                    int rc = oDoc.Add();
                    if (rc != 0)
                    {
                        string temp = sap.oCom.GetLastErrorDescription();
                        if (sap.oCom.InTransaction)
                        {
                            sap.oCom.EndTransaction(BoWfTransOpt.wf_RollBack);
                        }

                        IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                        PurchaseReturns obj = osupdate.GetObjectByKey<PurchaseReturns>(oTargetDoc.Oid);

                        PurchaseReturnDocTrail ds = osupdate.CreateObject<PurchaseReturnDocTrail>();
                        ds.CreateUser = osupdate.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                        ds.CreateDate = DateTime.Now;
                        ds.DocStatus = DocStatus.PendPost;
                        ds.DocRemarks = "SAP Error:" + temp;
                        obj.PurchaseReturnDocTrail.Add(ds);

                        osupdate.CommitChanges();

                        WriteLog("[Error]", "Message: Purchase Return Posting :" + oTargetDoc + "-" + temp);

                        return -1;
                    }
                    return 1;
                }
                return 0;
            }
            catch (Exception ex)
            {
                IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                PurchaseReturns obj = osupdate.GetObjectByKey<PurchaseReturns>(oTargetDoc.Oid);

                PurchaseReturnDocTrail ds = osupdate.CreateObject<PurchaseReturnDocTrail>();
                ds.CreateUser = osupdate.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                ds.CreateDate = DateTime.Now;
                ds.DocStatus = DocStatus.PendPost;
                ds.DocRemarks = "SAP Error:" + ex.Message;
                obj.PurchaseReturnDocTrail.Add(ds);

                osupdate.CommitChanges();

                WriteLog("[Error]", "Message: Purchase Return Posting :" + oTargetDoc + "-" + ex.Message);

                return -1;
            }
        }

        public int PostSReturntoSAP(SalesReturns oTargetDoc, IObjectSpaceProvider ObjectSpaceProvider, SAPCompany sap)
        {
            // return 0 = post nothing
            // return -1 = posting error
            // return 1 = posting successful
            try
            {
                if (!oTargetDoc.Sap)
                {
                    bool found = false;
                    DateTime postdate = DateTime.Now;

                    Guid g;
                    // Create and display the value of two GUIDs.
                    g = Guid.NewGuid();

                    int sapempid = 0;

                    SAPbobsCOM.Documents oDoc = null;

                    oDoc = (SAPbobsCOM.Documents)sap.oCom.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oReturns);

                    oDoc.DocType = BoDocumentTypes.dDocument_Items;
                    oDoc.CardCode = oTargetDoc.Customer.BPCode;
                    oDoc.CardName = oTargetDoc.CustomerName;
                    oDoc.DocDate = oTargetDoc.DocDate;
                    oDoc.Comments = oTargetDoc.Remarks;
                    oDoc.NumAtCard = oTargetDoc.Reference;
                    oDoc.UserFields.Fields.Item("U_PortalDocNum").Value = oTargetDoc.DocNum;
                    // Start ver 1.0.18
                    if (oTargetDoc.BillingAddress != null)
                    {
                        oDoc.PayToCode = oTargetDoc.BillingAddress.AddressKey;
                    }
                    if (oTargetDoc.ShippingAddress != null)
                    {
                        oDoc.ShipToCode = oTargetDoc.ShippingAddress.AddressKey;
                    }
                    // End ver 1.0.18

                    // Start ver 1.0.18
                    // Buyer
                    if (oTargetDoc.EIVConsolidate != null)
                    {
                        if (oTargetDoc.EIVConsolidate.Code == "Y")
                        {
                            oDoc.UserFields.Fields.Item("U_EIV_Consolidate").Value = "N";
                        }
                        else
                        {
                            oDoc.UserFields.Fields.Item("U_EIV_Consolidate").Value = "Y";
                        }
                    }
                    if (oTargetDoc.EIVType != null)
                    {
                        oDoc.UserFields.Fields.Item("U_EIV_InvoiceType").Value = oTargetDoc.EIVType.Code;
                    }
                    if (oTargetDoc.EIVFreqSync != null)
                    {
                        oDoc.UserFields.Fields.Item("U_EIV_FreqSync").Value = oTargetDoc.EIVFreqSync.Code;
                    }
                    oDoc.UserFields.Fields.Item("U_EIV_BuyerName").Value = oTargetDoc.CustomerName == null ? "" : oTargetDoc.CustomerName;
                    oDoc.UserFields.Fields.Item("U_EIV_BuyerTin").Value = oTargetDoc.EIVBuyerTIN == null ? "" : oTargetDoc.EIVBuyerTIN;
                    oDoc.UserFields.Fields.Item("U_EIV_BuyerRegNum").Value = oTargetDoc.EIVBuyerRegNum == null ? "" : oTargetDoc.EIVBuyerRegNum;
                    if (oTargetDoc.EIVBuyerRegTyp != null)
                    {
                        oDoc.UserFields.Fields.Item("U_EIV_BuyerRegTyp").Value = oTargetDoc.EIVBuyerRegTyp.Code;
                    }
                    oDoc.UserFields.Fields.Item("U_EIV_BuyerSSTRegNum").Value = oTargetDoc.EIVBuyerSSTRegNum == null ? "" : oTargetDoc.EIVBuyerSSTRegNum;
                    oDoc.UserFields.Fields.Item("U_EIV_BuyerEmail").Value = oTargetDoc.EIVBuyerEmail == null ? "" : oTargetDoc.EIVBuyerEmail;
                    oDoc.UserFields.Fields.Item("U_EIV_BuyerContact").Value = oTargetDoc.EIVBuyerContact == null ? "" : oTargetDoc.EIVBuyerContact;

                    oDoc.AddressExtension.BillToStreet = oTargetDoc.EIVAddressLine1B == null ? "" : oTargetDoc.EIVAddressLine1B;
                    oDoc.AddressExtension.BillToBlock = oTargetDoc.EIVAddressLine2B == null ? "" : oTargetDoc.EIVAddressLine2B;
                    oDoc.AddressExtension.BillToCity = oTargetDoc.EIVAddressLine3B == null ? "" : oTargetDoc.EIVAddressLine3B;
                    oDoc.AddressExtension.BillToCounty = oTargetDoc.EIVCityNameB == null ? "" : oTargetDoc.EIVCityNameB;
                    oDoc.AddressExtension.BillToZipCode = oTargetDoc.EIVPostalZoneB == null ? "" : oTargetDoc.EIVPostalZoneB;

                    oDoc.AddressExtension.UserFields.Fields.Item("U_EIV_AddressLine1B").Value = oTargetDoc.EIVAddressLine1B == null ? "" : oTargetDoc.EIVAddressLine1B;
                    oDoc.AddressExtension.UserFields.Fields.Item("U_EIV_AddressLine2B").Value = oTargetDoc.EIVAddressLine2B == null ? "" : oTargetDoc.EIVAddressLine2B;
                    oDoc.AddressExtension.UserFields.Fields.Item("U_EIV_AddressLine3B").Value = oTargetDoc.EIVAddressLine3B == null ? "" : oTargetDoc.EIVAddressLine3B;
                    oDoc.AddressExtension.UserFields.Fields.Item("U_EIV_PostalZoneB").Value = oTargetDoc.EIVPostalZoneB == null ? "" : oTargetDoc.EIVPostalZoneB;
                    oDoc.AddressExtension.UserFields.Fields.Item("U_EIV_CityNameB").Value = oTargetDoc.EIVCityNameB == null ? "" : oTargetDoc.EIVCityNameB;
                    if (oTargetDoc.EIVStateB != null)
                    {
                        oDoc.AddressExtension.BillToState = oTargetDoc.EIVStateB.Code;
                    }
                    if (oTargetDoc.EIVCountryB != null)
                    {
                        oDoc.AddressExtension.BillToCountry = oTargetDoc.EIVCountryB.Code;
                    }

                    // Recipient
                    oDoc.UserFields.Fields.Item("U_EIV_ShippingName").Value = oTargetDoc.EIVShippingName == null ? "" : oTargetDoc.EIVShippingName;
                    oDoc.UserFields.Fields.Item("U_EIV_ShippingTin").Value = oTargetDoc.EIVShippingTin == null ? "" : oTargetDoc.EIVShippingTin;
                    oDoc.UserFields.Fields.Item("U_EIV_ShippingRegNum").Value = oTargetDoc.EIVShippingRegNum == null ? "" : oTargetDoc.EIVShippingRegNum;
                    if (oTargetDoc.EIVShippingRegTyp != null)
                    {
                        oDoc.UserFields.Fields.Item("U_EIV_ShippingRegTyp").Value = oTargetDoc.EIVShippingRegTyp.Code;
                    }

                    oDoc.AddressExtension.ShipToStreet = oTargetDoc.EIVAddressLine1S == null ? "" : oTargetDoc.EIVAddressLine1S;
                    oDoc.AddressExtension.ShipToBlock = oTargetDoc.EIVAddressLine2S == null ? "" : oTargetDoc.EIVAddressLine2S;
                    oDoc.AddressExtension.ShipToCity = oTargetDoc.EIVAddressLine3S == null ? "" : oTargetDoc.EIVAddressLine3S;
                    oDoc.AddressExtension.ShipToCounty = oTargetDoc.EIVCityNameS == null ? "" : oTargetDoc.EIVCityNameS;
                    oDoc.AddressExtension.ShipToZipCode = oTargetDoc.EIVPostalZoneS == null ? "" : oTargetDoc.EIVPostalZoneS;

                    oDoc.AddressExtension.UserFields.Fields.Item("U_EIV_AddressLine1S").Value = oTargetDoc.EIVAddressLine1S == null ? "" : oTargetDoc.EIVAddressLine1S;
                    oDoc.AddressExtension.UserFields.Fields.Item("U_EIV_AddressLine2S").Value = oTargetDoc.EIVAddressLine2S == null ? "" : oTargetDoc.EIVAddressLine2S;
                    oDoc.AddressExtension.UserFields.Fields.Item("U_EIV_AddressLine3S").Value = oTargetDoc.EIVAddressLine3S == null ? "" : oTargetDoc.EIVAddressLine3S;
                    oDoc.AddressExtension.UserFields.Fields.Item("U_EIV_PostalZoneS").Value = oTargetDoc.EIVPostalZoneS == null ? "" : oTargetDoc.EIVPostalZoneS;
                    oDoc.AddressExtension.UserFields.Fields.Item("U_EIV_CityNameS").Value = oTargetDoc.EIVCityNameS == null ? "" : oTargetDoc.EIVCityNameS;
                    if (oTargetDoc.EIVStateS != null)
                    {
                        oDoc.AddressExtension.ShipToState = oTargetDoc.EIVStateS.Code;
                    }
                    if (oTargetDoc.EIVCountryS != null)
                    {
                        oDoc.AddressExtension.ShipToCountry = oTargetDoc.EIVCountryS.Code;
                    }
                    // End ver 1.0.18

                    if (sapempid > 0)
                        oDoc.DocumentsOwner = sapempid;

                    int cnt = 0;
                    foreach (SalesReturnDetails dtl in oTargetDoc.SalesReturnDetails)
                    {
                        //if (dtl.Total > 0)
                        //{
                        cnt++;
                        if (cnt == 1)
                        {
                        }
                        else
                        {
                            oDoc.Lines.Add();
                            oDoc.Lines.SetCurrentLine(oDoc.Lines.Count - 1);
                        }

                        //oDoc.Lines.TaxCode = dtl.Tax.BoCode;
                        //oDoc.Lines.TaxTotal = (double)dtl.TaxAmount;
                        oDoc.Lines.WarehouseCode = dtl.Warehouse.WarehouseCode;
                        oDoc.Lines.UserFields.Fields.Item("U_PortalLineOid").Value = dtl.Oid.ToString();
                        oDoc.Lines.ItemCode = dtl.ItemCode.ItemCode;
                        oDoc.Lines.ItemDescription = dtl.ItemDesc;
                        oDoc.Lines.Quantity = (double)dtl.RtnQuantity;
                        oDoc.Lines.UnitPrice = (double)dtl.Price;
                        if (dtl.ReasonCode != null)
                        {
                            oDoc.Lines.UserFields.Fields.Item("U_SalReturnReason").Value = dtl.ReasonCode.ReasonCode;
                        }
                        if (dtl.UnitCost != 0)
                        {
                            oDoc.Lines.EnableReturnCost = BoYesNoEnum.tYES;
                            oDoc.Lines.ReturnCost = (double)dtl.UnitCost;
                        }

                        if (dtl.InvoiceDoc != null)
                        {
                            oDoc.Lines.UserFields.Fields.Item("U_BaseDocNum").Value = dtl.InvoiceDoc;
                        }
                        if (dtl.Bin != null)
                        {
                            oDoc.Lines.BinAllocations.BinAbsEntry = dtl.Bin.AbsEntry;
                            oDoc.Lines.BinAllocations.Quantity = (double)dtl.RtnQuantity;
                        }
                        // Start ver 1.0.18
                        if (dtl.EIVClassification != null)
                        {
                            oDoc.Lines.UserFields.Fields.Item("U_EIV_Classification").Value = dtl.EIVClassification.Code;
                        }
                        // End ver 1.0.18

                        IObjectSpace osreq = ObjectSpaceProvider.CreateObjectSpace();
                        SalesReturnRequests srr = osreq.FindObject<SalesReturnRequests>(CriteriaOperator.Parse("DocNum = ?", dtl.BaseDoc));

                        foreach (SalesReturnRequestDetails srrdetail in srr.SalesReturnRequestDetails)
                        {
                            if (srrdetail.BaseDoc != null)
                            {
                                oDoc.DocumentReferences.ReferencedDocEntry = int.Parse(srrdetail.BaseDoc);
                                oDoc.DocumentReferences.ReferencedObjectType = ReferencedObjectTypeEnum.rot_SalesInvoice;
                                oDoc.DocumentReferences.Remark = oTargetDoc.DocNum;
                            }
                        }
                        //}
                    }

                    int rc = oDoc.Add();
                    if (rc != 0)
                    {
                        string temp = sap.oCom.GetLastErrorDescription();
                        if (sap.oCom.InTransaction)
                        {
                            sap.oCom.EndTransaction(BoWfTransOpt.wf_RollBack);
                        }

                        IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                        SalesReturns obj = osupdate.GetObjectByKey<SalesReturns>(oTargetDoc.Oid);

                        SalesReturnDocTrail ds = osupdate.CreateObject<SalesReturnDocTrail>();
                        ds.CreateUser = osupdate.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                        ds.CreateDate = DateTime.Now;
                        ds.DocStatus = DocStatus.PendPost;
                        ds.DocRemarks = "SAP Error:" + temp;
                        obj.SalesReturnDocTrail.Add(ds);

                        osupdate.CommitChanges();

                        WriteLog("[Error]", "Message: Sales Return Posting :" + oTargetDoc + "-" + temp);

                        return -1;
                    }
                    return 1;
                }
                return 0;
            }
            catch (Exception ex)
            {
                IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                SalesReturns obj = osupdate.GetObjectByKey<SalesReturns>(oTargetDoc.Oid);

                SalesReturnDocTrail ds = osupdate.CreateObject<SalesReturnDocTrail>();
                ds.CreateUser = osupdate.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                ds.CreateDate = DateTime.Now;
                ds.DocStatus = DocStatus.PendPost;
                ds.DocRemarks = "SAP Error:" + ex.Message;
                obj.SalesReturnDocTrail.Add(ds);

                osupdate.CommitChanges();

                WriteLog("[Error]", "Message: Sales Return Posting :" + oTargetDoc + "-" + ex.Message);

                return -1;
            }
        }

        public int PostWTIntoSAP(WarehouseTransfers oTargetDoc, IObjectSpaceProvider ObjectSpaceProvider, SAPCompany sap)
        {
            // return 0 = post nothing
            // return -1 = posting error
            // return 1 = posting successful
            try
            {
                if (!oTargetDoc.Sap)
                {
                    IObjectSpace fos = ObjectSpaceProvider.CreateObjectSpace();
                    Guid g;
                    // Create and display the value of two GUIDs.
                    g = Guid.NewGuid();

                    if (oTargetDoc.WarehouseTransferAttachment != null && oTargetDoc.WarehouseTransferAttachment.Count > 0)
                    {
                        foreach (WarehouseTransferAttachment obj in oTargetDoc.WarehouseTransferAttachment)
                        {
                            string fullpath = ConfigurationManager.AppSettings["B1AttachmentPath"].ToString() + g.ToString() + obj.File.FileName;
                            using (System.IO.FileStream fs = System.IO.File.OpenWrite(fullpath))
                            {
                                obj.File.SaveToStream(fs);
                            }
                        }
                    }

                    SAPbobsCOM.StockTransfer oDoc = null;

                    oDoc = (SAPbobsCOM.StockTransfer)sap.oCom.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oStockTransfer);

                    if (oTargetDoc.Supplier != null)
                    {
                        oDoc.CardCode = oTargetDoc.Supplier.BPCode;
                        oDoc.CardName = oTargetDoc.Supplier.BPName;
                    }
                    oDoc.DocDate = oTargetDoc.TransferDate;
                    //oDoc.TaxDate = oTargetDoc.DeliveryDate;
                    oDoc.Comments = oTargetDoc.Remarks;
                    oDoc.FromWarehouse = oTargetDoc.FromWarehouse.WarehouseCode;
                    oDoc.ToWarehouse = oTargetDoc.ToWarehouse.WarehouseCode;
                    oDoc.UserFields.Fields.Item("U_PortalDocNum").Value = oTargetDoc.DocNum;

                    int cnt = 0;
                    foreach (WarehouseTransferDetails dtl in oTargetDoc.WarehouseTransferDetails)
                    {
                        if (dtl.Quantity > 0)
                        {
                            cnt++;
                            if (cnt == 1)
                            {
                            }
                            else
                            {
                                oDoc.Lines.Add();
                                oDoc.Lines.SetCurrentLine(oDoc.Lines.Count - 1);
                            }

                            oDoc.Lines.ItemCode = dtl.ItemCode.ItemCode;
                            oDoc.Lines.ItemDescription = dtl.ItemDesc;
                            oDoc.Lines.Quantity = (double)dtl.Quantity;
                            oDoc.Lines.FromWarehouseCode = oTargetDoc.FromWarehouse.WarehouseCode;
                            oDoc.Lines.WarehouseCode = oTargetDoc.ToWarehouse.WarehouseCode;
                            oDoc.Lines.UserFields.Fields.Item("U_PortalLineOid").Value = dtl.Oid.ToString();

                            if (dtl.FromBin != null)
                            {
                                oDoc.Lines.BinAllocations.BinAbsEntry = dtl.FromBin.AbsEntry;
                                oDoc.Lines.BinAllocations.Quantity = (double)dtl.Quantity;
                                oDoc.Lines.BinAllocations.BinActionType = SAPbobsCOM.BinActionTypeEnum.batFromWarehouse;
                            }

                            if (dtl.ToBin != null)
                            {
                                oDoc.Lines.BinAllocations.Add();
                                oDoc.Lines.BinAllocations.BinAbsEntry = dtl.ToBin.AbsEntry;
                                oDoc.Lines.BinAllocations.Quantity = (double)dtl.Quantity;
                                oDoc.Lines.BinAllocations.BinActionType = SAPbobsCOM.BinActionTypeEnum.batToWarehouse;
                            }
                        }
                    }
                    if (oTargetDoc.WarehouseTransferAttachment != null && oTargetDoc.WarehouseTransferAttachment.Count > 0)
                    {
                        cnt = 0;
                        SAPbobsCOM.Attachments2 oAtt = (SAPbobsCOM.Attachments2)sap.oCom.GetBusinessObject(BoObjectTypes.oAttachments2);
                        foreach (WarehouseTransferAttachment dtl in oTargetDoc.WarehouseTransferAttachment)
                        {

                            cnt++;
                            if (cnt == 1)
                            {
                                if (oAtt.Lines.Count == 0)
                                    oAtt.Lines.Add();
                            }
                            else
                                oAtt.Lines.Add();

                            string attfile = "";
                            string[] fexe = dtl.File.FileName.Split('.');
                            if (fexe.Length <= 2)
                                attfile = fexe[0];
                            else
                            {
                                for (int x = 0; x < fexe.Length - 1; x++)
                                {
                                    if (attfile == "")
                                        attfile = fexe[x];
                                    else
                                        attfile += "." + fexe[x];
                                }
                            }
                            oAtt.Lines.FileName = g.ToString() + attfile;
                            if (fexe.Length > 1)
                                oAtt.Lines.FileExtension = fexe[fexe.Length - 1];
                            string path = ConfigurationManager.AppSettings["B1AttachmentPath"].ToString();
                            path = path.Replace("\\\\", "\\");
                            path = path.Substring(0, path.Length - 1);
                            oAtt.Lines.SourcePath = path;
                            oAtt.Lines.Override = SAPbobsCOM.BoYesNoEnum.tYES;
                        }
                        int iAttEntry = -1;
                        if (oAtt.Add() == 0)
                        {
                            iAttEntry = int.Parse(sap.oCom.GetNewObjectKey());
                        }
                        else
                        {
                            string temp = sap.oCom.GetLastErrorDescription();
                            if (sap.oCom.InTransaction)
                            {
                                sap.oCom.EndTransaction(BoWfTransOpt.wf_RollBack);
                            }

                            IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                            WarehouseTransfers obj = osupdate.GetObjectByKey<WarehouseTransfers>(oTargetDoc.Oid);

                            WarehouseTransfersDocTrail ds = osupdate.CreateObject<WarehouseTransfersDocTrail>();
                            ds.CreateUser = osupdate.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                            ds.CreateDate = DateTime.Now;
                            ds.DocStatus = DocStatus.PendPost;
                            ds.DocRemarks = "SAP Error:" + temp;
                            obj.WarehouseTransfersDocTrail.Add(ds);

                            osupdate.CommitChanges();

                            WriteLog("[Error]", "Message: Warehoouse Transfer Attachement Error :" + oTargetDoc + "-" + temp);

                            return -1;
                        }
                        oDoc.AttachmentEntry = iAttEntry;
                    }

                    int rc = oDoc.Add();
                    if (rc != 0)
                    {
                        string temp = sap.oCom.GetLastErrorDescription();
                        if (sap.oCom.InTransaction)
                        {
                            sap.oCom.EndTransaction(BoWfTransOpt.wf_RollBack);
                        }

                        IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                        WarehouseTransfers obj = osupdate.GetObjectByKey<WarehouseTransfers>(oTargetDoc.Oid);

                        WarehouseTransfersDocTrail ds = osupdate.CreateObject<WarehouseTransfersDocTrail>();
                        ds.CreateUser = osupdate.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                        ds.CreateDate = DateTime.Now;
                        ds.DocStatus = DocStatus.PendPost;
                        ds.DocRemarks = "SAP Error:" + temp;
                        obj.WarehouseTransfersDocTrail.Add(ds);

                        osupdate.CommitChanges();

                        WriteLog("[Error]", "Message: Warehouse Transfer Posting :" + oTargetDoc + "-" + temp);

                        return -1;
                    }
                    return 1;
                }
                return 0;
            }
            catch (Exception ex)
            {
                IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                WarehouseTransfers obj = osupdate.GetObjectByKey<WarehouseTransfers>(oTargetDoc.Oid);

                WarehouseTransfersDocTrail ds = osupdate.CreateObject<WarehouseTransfersDocTrail>();
                ds.CreateUser = osupdate.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                ds.CreateDate = DateTime.Now;
                ds.DocStatus = DocStatus.PendPost;
                ds.DocRemarks = "SAP Error:" + ex.Message;
                obj.WarehouseTransfersDocTrail.Add(ds);

                osupdate.CommitChanges();

                WriteLog("[Error]", "Message: Warehouse Transfer Posting :" + oTargetDoc + "-" + ex.Message);

                return -1;
            }
        }

        public int PostSAOuttoSAP(StockAdjustments oTargetDoc, IObjectSpaceProvider ObjectSpaceProvider, SAPCompany sap)
        {
            // return 0 = post nothing
            // return -1 = posting error
            // return 1 = posting successful
            try
            {
                if (!oTargetDoc.Sap)
                {
                    IObjectSpace fos = ObjectSpaceProvider.CreateObjectSpace();
                    bool found = false;

                    DateTime postdate = DateTime.Now;

                    foreach (StockAdjustmentDetails dtl in oTargetDoc.StockAdjustmentDetails)
                    {
                        found = true;
                    }
                    if (!found) return 0;

                    Guid g;
                    // Create and display the value of two GUIDs.
                    g = Guid.NewGuid();

                    if (oTargetDoc.StockAdjustmentAttactment != null && oTargetDoc.StockAdjustmentAttactment.Count > 0)
                    {
                        foreach (StockAdjustmentAttactment obj in oTargetDoc.StockAdjustmentAttactment)
                        {
                            string fullpath = ConfigurationManager.AppSettings["B1AttachmentPath"].ToString() + g.ToString() + obj.File.FileName;
                            using (System.IO.FileStream fs = System.IO.File.OpenWrite(fullpath))
                            {
                                obj.File.SaveToStream(fs);
                            }
                        }
                    }

                    int sapempid = 0;

                    SAPbobsCOM.Documents oDoc = null;

                    oDoc = (SAPbobsCOM.Documents)sap.oCom.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInventoryGenExit);

                    oDoc.DocType = BoDocumentTypes.dDocument_Items;
                    oDoc.DocDate = postdate;

                    if (sapempid > 0)
                        oDoc.DocumentsOwner = sapempid;
                    oDoc.DocDate = oTargetDoc.AdjDate;
                    oDoc.TaxDate = oTargetDoc.DocDate;
                    oDoc.Comments = oTargetDoc.Remarks;
                    oDoc.UserFields.Fields.Item("U_PortalDocNum").Value = oTargetDoc.DocNum;

                    int cnt = 0;
                    foreach (StockAdjustmentDetails dtl in oTargetDoc.StockAdjustmentDetails)
                    {
                        if (dtl.Quantity < 0)
                        {
                            cnt++;
                            if (cnt == 1)
                            {
                            }
                            else
                            {
                                //oDoc.Lines.BatchNumbers.Add();
                                //oDoc.Lines.BatchNumbers.SetCurrentLine(oDoc.Lines.Count - 1);
                                oDoc.Lines.Add();
                                oDoc.Lines.SetCurrentLine(oDoc.Lines.Count - 1);
                            }

                            oDoc.Lines.WarehouseCode = dtl.Warehouse.WarehouseCode;
                            oDoc.Lines.ItemCode = dtl.ItemCode.ItemCode;
                            if (oTargetDoc.ReasonCode.GLAcc != null)
                            {
                                oDoc.Lines.AccountCode = oTargetDoc.ReasonCode.GLAcc;
                            }
                            oDoc.Lines.ItemDescription = dtl.ItemDesc;
                            oDoc.Lines.Quantity = (double)(dtl.Quantity - dtl.Quantity - dtl.Quantity);
                            oDoc.Lines.UserFields.Fields.Item("U_PortalLineOid").Value = dtl.Oid.ToString();

                            if (dtl.Bin != null)
                            {
                                oDoc.Lines.BinAllocations.BinAbsEntry = dtl.Bin.AbsEntry;
                                oDoc.Lines.BinAllocations.Quantity = (double)(dtl.Quantity - dtl.Quantity - dtl.Quantity);
                            }
                        }
                    }
                    if (oTargetDoc.StockAdjustmentAttactment != null && oTargetDoc.StockAdjustmentAttactment.Count > 0)
                    {
                        cnt = 0;
                        SAPbobsCOM.Attachments2 oAtt = (SAPbobsCOM.Attachments2)sap.oCom.GetBusinessObject(BoObjectTypes.oAttachments2);
                        foreach (StockAdjustmentAttactment dtl in oTargetDoc.StockAdjustmentAttactment)
                        {

                            cnt++;
                            if (cnt == 1)
                            {
                                if (oAtt.Lines.Count == 0)
                                    oAtt.Lines.Add();
                            }
                            else
                                oAtt.Lines.Add();

                            string attfile = "";
                            string[] fexe = dtl.File.FileName.Split('.');
                            if (fexe.Length <= 2)
                                attfile = fexe[0];
                            else
                            {
                                for (int x = 0; x < fexe.Length - 1; x++)
                                {
                                    if (attfile == "")
                                        attfile = fexe[x];
                                    else
                                        attfile += "." + fexe[x];
                                }
                            }
                            oAtt.Lines.FileName = g.ToString() + attfile;
                            if (fexe.Length > 1)
                                oAtt.Lines.FileExtension = fexe[fexe.Length - 1];
                            string path = ConfigurationManager.AppSettings["B1AttachmentPath"].ToString();
                            path = path.Replace("\\\\", "\\");
                            path = path.Substring(0, path.Length - 1);
                            oAtt.Lines.SourcePath = path;
                            oAtt.Lines.Override = SAPbobsCOM.BoYesNoEnum.tYES;
                        }
                        int iAttEntry = -1;
                        if (oAtt.Add() == 0)
                        {
                            iAttEntry = int.Parse(sap.oCom.GetNewObjectKey());
                        }
                        else
                        {
                            string temp = sap.oCom.GetLastErrorDescription();
                            if (sap.oCom.InTransaction)
                            {
                                sap.oCom.EndTransaction(BoWfTransOpt.wf_RollBack);
                            }

                            IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                            StockAdjustments obj = osupdate.GetObjectByKey<StockAdjustments>(oTargetDoc.Oid);

                            StockAdjustmentDocTrail ds = osupdate.CreateObject<StockAdjustmentDocTrail>();
                            ds.CreateUser = osupdate.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                            ds.CreateDate = DateTime.Now;
                            ds.DocStatus = DocStatus.PendPost;
                            ds.DocRemarks = "SAP Error:" + temp;
                            obj.StockAdjustmentDocTrail.Add(ds);

                            osupdate.CommitChanges();

                            WriteLog("[Error]", "Message: Stock Adjustment(Issue) Attachement Error :" + oTargetDoc + "-" + temp);

                            return -1;
                        }
                        oDoc.AttachmentEntry = iAttEntry;
                    }

                    int rc = oDoc.Add();
                    if (rc != 0)
                    {
                        string temp = sap.oCom.GetLastErrorDescription();
                        if (sap.oCom.InTransaction)
                        {
                            sap.oCom.EndTransaction(BoWfTransOpt.wf_RollBack);
                        }

                        IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                        StockAdjustments obj = osupdate.GetObjectByKey<StockAdjustments>(oTargetDoc.Oid);

                        StockAdjustmentDocTrail ds = osupdate.CreateObject<StockAdjustmentDocTrail>();
                        ds.CreateUser = osupdate.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                        ds.CreateDate = DateTime.Now;
                        ds.DocStatus = DocStatus.PendPost;
                        ds.DocRemarks = "SAP Error:" + temp;
                        obj.StockAdjustmentDocTrail.Add(ds);

                        osupdate.CommitChanges();

                        WriteLog("[Error]", "Message: Stock Adjustment(Issue) Posting :" + oTargetDoc + "-" + temp);

                        return -1;
                    }
                    return 1;
                }
                return 0;
            }
            catch (Exception ex)
            {
                IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                StockAdjustments obj = osupdate.GetObjectByKey<StockAdjustments>(oTargetDoc.Oid);

                StockAdjustmentDocTrail ds = osupdate.CreateObject<StockAdjustmentDocTrail>();
                ds.CreateUser = osupdate.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                ds.CreateDate = DateTime.Now;
                ds.DocStatus = DocStatus.PendPost;
                ds.DocRemarks = "SAP Error:" + ex.Message;
                obj.StockAdjustmentDocTrail.Add(ds);

                osupdate.CommitChanges();

                WriteLog("[Error]", "Message: Stock Adjustment(Issue) Posting :" + oTargetDoc + "-" + ex.Message);

                return -1;
            }
        }

        public int PostSAINtoSAP(StockAdjustments oTargetDoc, IObjectSpaceProvider ObjectSpaceProvider, SAPCompany sap)
        {
            // return 0 = post nothing
            // return -1 = posting error
            // return 1 = posting successful
            try
            {
                if (!oTargetDoc.Sap)
                {
                    IObjectSpace fos = ObjectSpaceProvider.CreateObjectSpace();
                    bool found = false;
                    DateTime postdate = DateTime.Now;

                    foreach (StockAdjustmentDetails dtl in oTargetDoc.StockAdjustmentDetails)
                    {
                        found = true;
                    }
                    if (!found) return 0;

                    Guid g;
                    // Create and display the value of two GUIDs.
                    g = Guid.NewGuid();

                    if (oTargetDoc.StockAdjustmentAttactment != null && oTargetDoc.StockAdjustmentAttactment.Count > 0)
                    {
                        foreach (StockAdjustmentAttactment obj in oTargetDoc.StockAdjustmentAttactment)
                        {
                            string fullpath = ConfigurationManager.AppSettings["B1AttachmentPath"].ToString() + g.ToString() + obj.File.FileName;
                            using (System.IO.FileStream fs = System.IO.File.OpenWrite(fullpath))
                            {
                                obj.File.SaveToStream(fs);
                            }
                        }
                    }

                    int sapempid = 0;

                    SAPbobsCOM.Documents oDoc = null;

                    oDoc = (SAPbobsCOM.Documents)sap.oCom.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInventoryGenEntry);

                    oDoc.DocType = BoDocumentTypes.dDocument_Items;
                    oDoc.DocDate = postdate;

                    if (sapempid > 0)
                        oDoc.DocumentsOwner = sapempid;
                    oDoc.DocDate = oTargetDoc.AdjDate;
                    oDoc.TaxDate = oTargetDoc.DocDate;
                    oDoc.Comments = oTargetDoc.Remarks;
                    oDoc.UserFields.Fields.Item("U_PortalDocNum").Value = oTargetDoc.DocNum;

                    int cnt = 0;
                    foreach (StockAdjustmentDetails dtl in oTargetDoc.StockAdjustmentDetails)
                    {
                        if (dtl.Quantity > 0)
                        {
                            cnt++;
                            if (cnt == 1)
                            {
                            }
                            else
                            {
                                //oDoc.Lines.BatchNumbers.Add();
                                //oDoc.Lines.BatchNumbers.SetCurrentLine(oDoc.Lines.Count - 1);
                                oDoc.Lines.Add();
                                oDoc.Lines.SetCurrentLine(oDoc.Lines.Count - 1);
                            }

                            oDoc.Lines.WarehouseCode = dtl.Warehouse.WarehouseCode;

                            oDoc.Lines.ItemCode = dtl.ItemCode.ItemCode;
                            oDoc.Lines.ItemDescription = dtl.ItemDesc;
                            oDoc.Lines.Quantity = (double)dtl.Quantity;
                            if (oTargetDoc.ReasonCode.GLAcc != null)
                            {
                                oDoc.Lines.AccountCode = oTargetDoc.ReasonCode.GLAcc;
                            }
                            if (dtl.CostType == AdjustmnetCost.Zero)
                            {
                                oDoc.Lines.UnitPrice = 0.01;
                            }
                            else
                            {
                                oDoc.Lines.UnitPrice = (double)dtl.Price;
                            }
                            oDoc.Lines.UserFields.Fields.Item("U_PortalLineOid").Value = dtl.Oid.ToString();

                            if (dtl.Bin != null)
                            {
                                oDoc.Lines.BinAllocations.BinAbsEntry = dtl.Bin.AbsEntry;
                                oDoc.Lines.BinAllocations.Quantity = (double)dtl.Quantity;
                            }
                        }
                    }
                    if (oTargetDoc.StockAdjustmentAttactment != null && oTargetDoc.StockAdjustmentAttactment.Count > 0)
                    {
                        cnt = 0;
                        SAPbobsCOM.Attachments2 oAtt = (SAPbobsCOM.Attachments2)sap.oCom.GetBusinessObject(BoObjectTypes.oAttachments2);
                        foreach (StockAdjustmentAttactment dtl in oTargetDoc.StockAdjustmentAttactment)
                        {

                            cnt++;
                            if (cnt == 1)
                            {
                                if (oAtt.Lines.Count == 0)
                                    oAtt.Lines.Add();
                            }
                            else
                                oAtt.Lines.Add();

                            string attfile = "";
                            string[] fexe = dtl.File.FileName.Split('.');
                            if (fexe.Length <= 2)
                                attfile = fexe[0];
                            else
                            {
                                for (int x = 0; x < fexe.Length - 1; x++)
                                {
                                    if (attfile == "")
                                        attfile = fexe[x];
                                    else
                                        attfile += "." + fexe[x];
                                }
                            }
                            oAtt.Lines.FileName = g.ToString() + attfile;
                            if (fexe.Length > 1)
                                oAtt.Lines.FileExtension = fexe[fexe.Length - 1];
                            string path = ConfigurationManager.AppSettings["B1AttachmentPath"].ToString();
                            path = path.Replace("\\\\", "\\");
                            path = path.Substring(0, path.Length - 1);
                            oAtt.Lines.SourcePath = path;
                            oAtt.Lines.Override = SAPbobsCOM.BoYesNoEnum.tYES;
                        }
                        int iAttEntry = -1;
                        if (oAtt.Add() == 0)
                        {
                            iAttEntry = int.Parse(sap.oCom.GetNewObjectKey());
                        }
                        else
                        {
                            string temp = sap.oCom.GetLastErrorDescription();
                            if (sap.oCom.InTransaction)
                            {
                                sap.oCom.EndTransaction(BoWfTransOpt.wf_RollBack);
                            }

                            IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                            StockAdjustments obj = osupdate.GetObjectByKey<StockAdjustments>(oTargetDoc.Oid);

                            StockAdjustmentDocTrail ds = osupdate.CreateObject<StockAdjustmentDocTrail>();
                            ds.CreateUser = osupdate.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                            ds.CreateDate = DateTime.Now;
                            ds.DocStatus = DocStatus.PendPost;
                            ds.DocRemarks = "SAP Error:" + temp;
                            obj.StockAdjustmentDocTrail.Add(ds);

                            osupdate.CommitChanges();

                            WriteLog("[Error]", "Message: Stock Adjustment(Receipt) Attachement Error :" + oTargetDoc + "-" + temp);

                            return -1;
                        }
                        oDoc.AttachmentEntry = iAttEntry;
                    }

                    int rc = oDoc.Add();
                    if (rc != 0)
                    {
                        string temp = sap.oCom.GetLastErrorDescription();
                        if (sap.oCom.InTransaction)
                        {
                            sap.oCom.EndTransaction(BoWfTransOpt.wf_RollBack);
                        }

                        IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                        StockAdjustments obj = osupdate.GetObjectByKey<StockAdjustments>(oTargetDoc.Oid);

                        StockAdjustmentDocTrail ds = osupdate.CreateObject<StockAdjustmentDocTrail>();
                        ds.CreateUser = osupdate.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                        ds.CreateDate = DateTime.Now;
                        ds.DocStatus = DocStatus.PendPost;
                        ds.DocRemarks = "SAP Error:" + temp;
                        obj.StockAdjustmentDocTrail.Add(ds);

                        osupdate.CommitChanges();

                        WriteLog("[Error]", "Message: Stock Adjustment(Receipt) Posting :" + oTargetDoc + "-" + temp);

                        return -1;
                    }
                    return 1;
                }
                return 0;
            }
            catch (Exception ex)
            {
                IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                StockAdjustments obj = osupdate.GetObjectByKey<StockAdjustments>(oTargetDoc.Oid);

                StockAdjustmentDocTrail ds = osupdate.CreateObject<StockAdjustmentDocTrail>();
                ds.CreateUser = osupdate.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                ds.CreateDate = DateTime.Now;
                ds.DocStatus = DocStatus.PendPost;
                ds.DocRemarks = "SAP Error:" + ex.Message;
                obj.StockAdjustmentDocTrail.Add(ds);

                osupdate.CommitChanges();

                WriteLog("[Error]", "Message: Stock Adjustment(Receipt) Posting :" + oTargetDoc + "-" + ex.Message);

                return -1;
            }
        }

        public int PostCNtoSAP(SalesRefundRequests oTargetDoc, IObjectSpaceProvider ObjectSpaceProvider, SAPCompany sap)
        {
            // return 0 = post nothing
            // return -1 = posting error
            // return 1 = posting successful
            try
            {
                if (!oTargetDoc.Sap)
                {
                    IObjectSpace fos = ObjectSpaceProvider.CreateObjectSpace();
                    Guid g;
                    // Create and display the value of two GUIDs.
                    g = Guid.NewGuid();

                    SAPbobsCOM.Documents oDoc = null;

                    oDoc = (SAPbobsCOM.Documents)sap.oCom.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oCreditNotes);

                    oDoc.CardCode = oTargetDoc.Customer.BPCode;
                    oDoc.CardName = oTargetDoc.CustomerName;
                    oDoc.DocDate = oTargetDoc.PostingDate;
                    oDoc.Comments = oTargetDoc.Remarks;
                    oDoc.UserFields.Fields.Item("U_PortalDocNum").Value = oTargetDoc.DocNum;
                    if (oTargetDoc.Reference != null)
                    {
                        oDoc.NumAtCard = oTargetDoc.Reference;
                    }
                    if (oTargetDoc.ContactPerson != null)
                    {
                        oDoc.SalesPersonCode = oTargetDoc.ContactPerson.SlpCode;
                    }
                    // Start ver 1.0.18
                    vwBillingAddress BillingAddress = fos.FindObject<vwBillingAddress>(CriteriaOperator.Parse("AddressKey = ? and CardCode = ?"
                        , oTargetDoc.Customer.BillToDef, oTargetDoc.Customer.BPCode));
                    vwShippingAddress ShippingAddress = fos.FindObject<vwShippingAddress>(CriteriaOperator.Parse("AddressKey = ? and CardCode = ?"
                        , oTargetDoc.Customer.ShipToDef, oTargetDoc.Customer.BPCode));

                    if (BillingAddress != null)
                    {
                        oDoc.PayToCode = BillingAddress.AddressKey;
                    }
                    if (ShippingAddress != null)
                    {
                        oDoc.ShipToCode = ShippingAddress.AddressKey;
                    }
                    // End ver 1.0.18

                    // Start ver 1.0.18
                    // Buyer
                    if (oTargetDoc.EIVConsolidate != null)
                    {
                        if (oTargetDoc.EIVConsolidate.Code == "Y")
                        {
                            oDoc.UserFields.Fields.Item("U_EIV_Consolidate").Value = "N";
                        }
                        else
                        {
                            oDoc.UserFields.Fields.Item("U_EIV_Consolidate").Value = "Y";
                        }
                    }
                    if (oTargetDoc.EIVType != null)
                    {
                        oDoc.UserFields.Fields.Item("U_EIV_InvoiceType").Value = oTargetDoc.EIVType.Code;
                    }
                    if (oTargetDoc.EIVFreqSync != null)
                    {
                        oDoc.UserFields.Fields.Item("U_EIV_FreqSync").Value = oTargetDoc.EIVFreqSync.Code;
                    }
                    oDoc.UserFields.Fields.Item("U_EIV_BuyerName").Value = oTargetDoc.CustomerName == null ? "" : oTargetDoc.CustomerName;
                    oDoc.UserFields.Fields.Item("U_EIV_BuyerTin").Value = oTargetDoc.EIVBuyerTIN == null ? "" : oTargetDoc.EIVBuyerTIN;
                    oDoc.UserFields.Fields.Item("U_EIV_BuyerRegNum").Value = oTargetDoc.EIVBuyerRegNum == null ? "" : oTargetDoc.EIVBuyerRegNum;
                    if (oTargetDoc.EIVBuyerRegTyp != null)
                    {
                        oDoc.UserFields.Fields.Item("U_EIV_BuyerRegTyp").Value = oTargetDoc.EIVBuyerRegTyp.Code;
                    }
                    oDoc.UserFields.Fields.Item("U_EIV_BuyerSSTRegNum").Value = oTargetDoc.EIVBuyerSSTRegNum == null ? "" : oTargetDoc.EIVBuyerSSTRegNum;
                    oDoc.UserFields.Fields.Item("U_EIV_BuyerContact").Value = oTargetDoc.EIVBuyerContact == null ? "" : oTargetDoc.EIVBuyerContact;

                    oDoc.AddressExtension.BillToStreet = oTargetDoc.EIVAddressLine1B == null ? "" : oTargetDoc.EIVAddressLine1B;
                    oDoc.AddressExtension.BillToBlock = oTargetDoc.EIVAddressLine2B == null ? "" : oTargetDoc.EIVAddressLine2B;
                    oDoc.AddressExtension.BillToCity = oTargetDoc.EIVAddressLine3B == null ? "" : oTargetDoc.EIVAddressLine3B;
                    oDoc.AddressExtension.BillToCounty = oTargetDoc.EIVCityNameB == null ? "" : oTargetDoc.EIVCityNameB;
                    oDoc.AddressExtension.BillToZipCode = oTargetDoc.EIVPostalZoneB == null ? "" : oTargetDoc.EIVPostalZoneB;

                    oDoc.AddressExtension.UserFields.Fields.Item("U_EIV_AddressLine1B").Value = oTargetDoc.EIVAddressLine1B == null ? "" : oTargetDoc.EIVAddressLine1B;
                    oDoc.AddressExtension.UserFields.Fields.Item("U_EIV_AddressLine2B").Value = oTargetDoc.EIVAddressLine2B == null ? "" : oTargetDoc.EIVAddressLine2B;
                    oDoc.AddressExtension.UserFields.Fields.Item("U_EIV_AddressLine3B").Value = oTargetDoc.EIVAddressLine3B == null ? "" : oTargetDoc.EIVAddressLine3B;
                    oDoc.AddressExtension.UserFields.Fields.Item("U_EIV_PostalZoneB").Value = oTargetDoc.EIVPostalZoneB == null ? "" : oTargetDoc.EIVPostalZoneB;
                    oDoc.AddressExtension.UserFields.Fields.Item("U_EIV_CityNameB").Value = oTargetDoc.EIVCityNameB == null ? "" : oTargetDoc.EIVCityNameB;
                    if (oTargetDoc.EIVStateB != null)
                    {
                        oDoc.AddressExtension.BillToState = oTargetDoc.EIVStateB.Code;
                    }
                    if (oTargetDoc.EIVCountryB != null)
                    {
                        oDoc.AddressExtension.BillToCountry = oTargetDoc.EIVCountryB.Code;
                    }

                    // Recipient
                    oDoc.UserFields.Fields.Item("U_EIV_ShippingName").Value = oTargetDoc.EIVShippingName == null ? "" : oTargetDoc.EIVShippingName;
                    oDoc.UserFields.Fields.Item("U_EIV_ShippingTin").Value = oTargetDoc.EIVShippingTin == null ? "" : oTargetDoc.EIVShippingTin;
                    oDoc.UserFields.Fields.Item("U_EIV_ShippingRegNum").Value = oTargetDoc.EIVShippingRegNum == null ? "" : oTargetDoc.EIVShippingRegNum;
                    if (oTargetDoc.EIVShippingRegTyp != null)
                    {
                        oDoc.UserFields.Fields.Item("U_EIV_ShippingRegTyp").Value = oTargetDoc.EIVShippingRegTyp.Code;
                    }

                    oDoc.AddressExtension.ShipToStreet = oTargetDoc.EIVAddressLine1S == null ? "" : oTargetDoc.EIVAddressLine1S;
                    oDoc.AddressExtension.ShipToBlock = oTargetDoc.EIVAddressLine2S == null ? "" : oTargetDoc.EIVAddressLine2S;
                    oDoc.AddressExtension.ShipToCity = oTargetDoc.EIVAddressLine3S == null ? "" : oTargetDoc.EIVAddressLine3S;
                    oDoc.AddressExtension.ShipToCounty = oTargetDoc.EIVCityNameS == null ? "" : oTargetDoc.EIVCityNameS;
                    oDoc.AddressExtension.ShipToZipCode = oTargetDoc.EIVPostalZoneS == null ? "" : oTargetDoc.EIVPostalZoneS;

                    oDoc.AddressExtension.UserFields.Fields.Item("U_EIV_AddressLine1S").Value = oTargetDoc.EIVAddressLine1S == null ? "" : oTargetDoc.EIVAddressLine1S;
                    oDoc.AddressExtension.UserFields.Fields.Item("U_EIV_AddressLine2S").Value = oTargetDoc.EIVAddressLine2S == null ? "" : oTargetDoc.EIVAddressLine2S;
                    oDoc.AddressExtension.UserFields.Fields.Item("U_EIV_AddressLine3S").Value = oTargetDoc.EIVAddressLine3S == null ? "" : oTargetDoc.EIVAddressLine3S;
                    oDoc.AddressExtension.UserFields.Fields.Item("U_EIV_PostalZoneS").Value = oTargetDoc.EIVPostalZoneS == null ? "" : oTargetDoc.EIVPostalZoneS;
                    oDoc.AddressExtension.UserFields.Fields.Item("U_EIV_CityNameS").Value = oTargetDoc.EIVCityNameS == null ? "" : oTargetDoc.EIVCityNameS;
                    if (oTargetDoc.EIVStateS != null)
                    {
                        oDoc.AddressExtension.ShipToState = oTargetDoc.EIVStateS.Code;
                    }
                    if (oTargetDoc.EIVCountryS != null)
                    {
                        oDoc.AddressExtension.ShipToCountry = oTargetDoc.EIVCountryS.Code;
                    }
                    // End ver 1.0.18

                    int cnt = 0;
                    foreach (SalesRefundReqDetails dtl in oTargetDoc.SalesRefundReqDetails)
                    {
                        cnt++;
                        if (cnt == 1)
                        {
                        }
                        else
                        {
                            oDoc.Lines.Add();
                            oDoc.Lines.SetCurrentLine(oDoc.Lines.Count - 1);
                        }

                        oDoc.Lines.ItemCode = dtl.ItemCode.ItemCode;
                        oDoc.Lines.ItemDescription = dtl.ItemDesc;
                        oDoc.Lines.Quantity = (double)dtl.Quantity;
                        oDoc.Lines.UnitPrice = (double)dtl.Price;
                        oDoc.Lines.WithoutInventoryMovement = BoYesNoEnum.tYES;
                        if (dtl.Warehouse != null)
                        {
                            oDoc.Lines.WarehouseCode = dtl.Warehouse.WarehouseCode;
                        }
                        oDoc.Lines.UserFields.Fields.Item("U_PortalLineOid").Value = dtl.Oid.ToString();
                        if (dtl.ReasonCode != null)
                        {
                            oDoc.Lines.UserFields.Fields.Item("U_SalReturnReason").Value = dtl.ReasonCode.ReasonCode;
                        }
                        // Start ver 1.0.18
                        if (dtl.EIVClassification != null)
                        {
                            oDoc.Lines.UserFields.Fields.Item("U_EIV_Classification").Value = dtl.EIVClassification.Code;
                        }
                        // End ver 1.0.18

                        if (dtl.Bin != null)
                        {
                            oDoc.Lines.BinAllocations.BinAbsEntry = dtl.Bin.AbsEntry;
                            oDoc.Lines.BinAllocations.Quantity = (double)dtl.Quantity;
                        }
                    }

                    int rc = oDoc.Add();
                    if (rc != 0)
                    {
                        string temp = sap.oCom.GetLastErrorDescription();
                        if (sap.oCom.InTransaction)
                        {
                            sap.oCom.EndTransaction(BoWfTransOpt.wf_RollBack);
                        }

                        IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                        SalesRefundRequests obj = osupdate.GetObjectByKey<SalesRefundRequests>(oTargetDoc.Oid);

                        SalesRefundReqDocTrail ds = osupdate.CreateObject<SalesRefundReqDocTrail>();
                        ds.CreateUser = osupdate.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                        ds.CreateDate = DateTime.Now;
                        ds.DocStatus = DocStatus.PendPost;
                        ds.DocRemarks = "SAP Error:" + temp;
                        obj.SalesRefundReqDocTrail.Add(ds);

                        osupdate.CommitChanges();

                        WriteLog("[Error]", "Message: Sales Refund Posting :" + oTargetDoc + "-" + temp);

                        return -1;
                    }
                    return 1;
                }
                return 0;
            }
            catch (Exception ex)
            {
                IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                SalesRefundRequests obj = osupdate.GetObjectByKey<SalesRefundRequests>(oTargetDoc.Oid);

                SalesRefundReqDocTrail ds = osupdate.CreateObject<SalesRefundReqDocTrail>();
                ds.CreateUser = osupdate.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                ds.CreateDate = DateTime.Now;
                ds.DocStatus = DocStatus.PendPost;
                ds.DocRemarks = "SAP Error:" + ex.Message;
                obj.SalesRefundReqDocTrail.Add(ds);

                osupdate.CommitChanges();

                WriteLog("[Error]", "Message: Sales Refund Posting :" + oTargetDoc + "-" + ex.Message);

                return -1;
            }
        }

        public int PostDPtoSAP(SalesOrderCollection oTargetDoc, string sodocnum, IObjectSpaceProvider ObjectSpaceProvider, SAPCompany sap)
        {
            // return 0 = post nothing
            // return -1 = posting error
            // return 1 = posting successful
            try
            {
                IObjectSpace os = ObjectSpaceProvider.CreateObjectSpace();
                SalesOrder so = os.FindObject<SalesOrder>(CriteriaOperator.Parse("DocNum = ?", sodocnum));

                if (so != null)
                {
                    if (so.SAPDocNum != null)
                    {
                        Guid g;
                        // Create and display the value of two GUIDs.
                        g = Guid.NewGuid();

                        SAPbobsCOM.Documents oDoc = null;

                        oDoc = (SAPbobsCOM.Documents)sap.oCom.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oDownPayments);

                        oDoc.CardCode = so.Customer.BPCode;
                        oDoc.CardName = so.CustomerName;
                        oDoc.DocDate = oTargetDoc.DocDate;
                        oDoc.UserFields.Fields.Item("U_PortalDocNum").Value = oTargetDoc.DocNum;
                        oDoc.UserFields.Fields.Item("U_SoDocNumber").Value = so.DocNum;
                        oDoc.DownPaymentType = DownPaymentTypeEnum.dptInvoice;
                        // Start ver 1.0.18
                        //if (so.BillingAddressfield != null)
                        //{
                        //    oDoc.Address = so.BillingAddressfield;
                        //}
                        // End ver 1.0.18
                        if (oTargetDoc.Remarks != null)
                        {
                            oDoc.Comments = oTargetDoc.Remarks;
                        }
                        // Start ver 1.0.18
                        if (so.BillingAddress != null)
                        {
                            oDoc.PayToCode = so.BillingAddress.AddressKey;
                        }
                        if (so.ShippingAddress != null)
                        {
                            oDoc.ShipToCode = so.ShippingAddress.AddressKey;
                        }
                        // End ver 1.0.18

                        // Start ver 1.0.18
                        // Buyer
                        if (so.EIVConsolidate != null)
                        {
                            if (so.EIVConsolidate.Code == "Y")
                            {
                                oDoc.UserFields.Fields.Item("U_EIV_Consolidate").Value = "N";
                            }
                            else
                            {
                                oDoc.UserFields.Fields.Item("U_EIV_Consolidate").Value = "Y";
                            }
                        }
                        if (so.EIVType != null)
                        {
                            oDoc.UserFields.Fields.Item("U_EIV_InvoiceType").Value = so.EIVType.Code;
                        }
                        if (so.EIVFreqSync != null)
                        {
                            oDoc.UserFields.Fields.Item("U_EIV_FreqSync").Value = so.EIVFreqSync.Code;
                        }
                        oDoc.UserFields.Fields.Item("U_EIV_BuyerName").Value = so.CustomerName == null ? "" : so.CustomerName;
                        oDoc.UserFields.Fields.Item("U_EIV_BuyerTin").Value = so.EIVBuyerTIN == null ? "" : so.EIVBuyerTIN;
                        oDoc.UserFields.Fields.Item("U_EIV_BuyerRegNum").Value = so.EIVBuyerRegNum == null ? "" : so.EIVBuyerRegNum;
                        if (so.EIVBuyerRegTyp != null)
                        {
                            oDoc.UserFields.Fields.Item("U_EIV_BuyerRegTyp").Value = so.EIVBuyerRegTyp.Code;
                        }
                        oDoc.UserFields.Fields.Item("U_EIV_BuyerSSTRegNum").Value = so.EIVBuyerSSTRegNum == null ? "" : so.EIVBuyerSSTRegNum;
                        oDoc.UserFields.Fields.Item("U_EIV_BuyerEmail").Value = so.EIVBuyerEmail == null ? "" : so.EIVBuyerEmail;
                        oDoc.UserFields.Fields.Item("U_EIV_BuyerContact").Value = so.EIVBuyerContact == null ? "" : so.EIVBuyerContact;

                        oDoc.AddressExtension.BillToStreet = so.EIVAddressLine1B == null ? "" : so.EIVAddressLine1B;
                        oDoc.AddressExtension.BillToBlock = so.EIVAddressLine2B == null ? "" : so.EIVAddressLine2B;
                        oDoc.AddressExtension.BillToCity = so.EIVAddressLine3B == null ? "" : so.EIVAddressLine3B;
                        oDoc.AddressExtension.BillToCounty = so.EIVCityNameB == null ? "" : so.EIVCityNameB;
                        oDoc.AddressExtension.BillToZipCode = so.EIVPostalZoneB == null ? "" : so.EIVPostalZoneB;

                        oDoc.AddressExtension.UserFields.Fields.Item("U_EIV_AddressLine1B").Value = so.EIVAddressLine1B == null ? "" : so.EIVAddressLine1B;
                        oDoc.AddressExtension.UserFields.Fields.Item("U_EIV_AddressLine2B").Value = so.EIVAddressLine2B == null ? "" : so.EIVAddressLine2B;
                        oDoc.AddressExtension.UserFields.Fields.Item("U_EIV_AddressLine3B").Value = so.EIVAddressLine3B == null ? "" : so.EIVAddressLine3B;
                        oDoc.AddressExtension.UserFields.Fields.Item("U_EIV_PostalZoneB").Value = so.EIVPostalZoneB == null ? "" : so.EIVPostalZoneB;
                        oDoc.AddressExtension.UserFields.Fields.Item("U_EIV_CityNameB").Value = so.EIVCityNameB == null ? "" : so.EIVCityNameB;
                        if (so.EIVStateB != null)
                        {
                            oDoc.AddressExtension.BillToState = so.EIVStateB.Code;
                        }
                        if (so.EIVCountryB != null)
                        {
                            oDoc.AddressExtension.BillToCountry = so.EIVCountryB.Code;
                        }

                        // Recipient
                        oDoc.UserFields.Fields.Item("U_EIV_ShippingName").Value = so.EIVShippingName == null ? "" : so.EIVShippingName;
                        oDoc.UserFields.Fields.Item("U_EIV_ShippingTin").Value = so.EIVShippingTin == null ? "" : so.EIVShippingTin;
                        oDoc.UserFields.Fields.Item("U_EIV_ShippingRegNum").Value = so.EIVShippingRegNum == null ? "" : so.EIVShippingRegNum;
                        if (so.EIVShippingRegTyp != null)
                        {
                            oDoc.UserFields.Fields.Item("U_EIV_ShippingRegTyp").Value = so.EIVShippingRegTyp.Code;
                        }

                        oDoc.AddressExtension.ShipToStreet = so.EIVAddressLine1S == null ? "" : so.EIVAddressLine1S;
                        oDoc.AddressExtension.ShipToBlock = so.EIVAddressLine2S == null ? "" : so.EIVAddressLine2S;
                        oDoc.AddressExtension.ShipToCity = so.EIVAddressLine3S == null ? "" : so.EIVAddressLine3S;
                        oDoc.AddressExtension.ShipToCounty = so.EIVCityNameS == null ? "" : so.EIVCityNameS;
                        oDoc.AddressExtension.ShipToZipCode = so.EIVPostalZoneS == null ? "" : so.EIVPostalZoneS;

                        oDoc.AddressExtension.UserFields.Fields.Item("U_EIV_AddressLine1S").Value = so.EIVAddressLine1S == null ? "" : so.EIVAddressLine1S;
                        oDoc.AddressExtension.UserFields.Fields.Item("U_EIV_AddressLine2S").Value = so.EIVAddressLine2S == null ? "" : so.EIVAddressLine2S;
                        oDoc.AddressExtension.UserFields.Fields.Item("U_EIV_AddressLine3S").Value = so.EIVAddressLine3S == null ? "" : so.EIVAddressLine3S;
                        oDoc.AddressExtension.UserFields.Fields.Item("U_EIV_PostalZoneS").Value = so.EIVPostalZoneS == null ? "" : so.EIVPostalZoneS;
                        oDoc.AddressExtension.UserFields.Fields.Item("U_EIV_CityNameS").Value = so.EIVCityNameS == null ? "" : so.EIVCityNameS;
                        if (so.EIVStateS != null)
                        {
                            oDoc.AddressExtension.ShipToState = so.EIVStateS.Code;
                        }
                        if (so.EIVCountryS != null)
                        {
                            oDoc.AddressExtension.ShipToCountry = so.EIVCountryS.Code;
                        }
                        // End ver 1.0.18

                        int cnt = 0;
                        foreach (SalesOrderDetails dtl in so.SalesOrderDetails)
                        {
                            cnt++;
                            if (cnt == 1)
                            {
                            }
                            else
                            {
                                oDoc.Lines.Add();
                                oDoc.Lines.SetCurrentLine(oDoc.Lines.Count - 1);
                            }

                            oDoc.Lines.ItemCode = dtl.ItemCode.ItemCode;
                            oDoc.Lines.ItemDescription = dtl.ItemDesc;
                            oDoc.Lines.Quantity = (double)dtl.Quantity;
                            oDoc.Lines.UnitPrice = (double)dtl.AdjustedPrice;
                            //if (oTargetDoc.PaymentType != null)
                            //{
                            //    oDoc.Lines.AccountCode = oTargetDoc.PaymentType.GLAccount;
                            //}
                            if (dtl.Location != null)
                            {
                                oDoc.Lines.WarehouseCode = dtl.Location.WarehouseCode;
                            }
                            oDoc.Lines.UserFields.Fields.Item("U_PortalLineOid").Value = dtl.Oid.ToString();
                            // Start ver 1.0.18
                            if (dtl.EIVClassification != null)
                            {
                                oDoc.Lines.UserFields.Fields.Item("U_EIV_Classification").Value = dtl.EIVClassification.Code;
                            }
                            // End ver 1.0.18

                            if (dtl.SAPDocEntry != 0)
                            {
                                oDoc.Lines.BaseType = 17;
                                oDoc.Lines.BaseEntry = dtl.SAPDocEntry;
                                oDoc.Lines.BaseLine = dtl.SAPBaseLine;
                            }
                        }

                        // Start ver 1.0.10
                        if (oTargetDoc.ReturnAmt > 0)
                        {
                            oDoc.DocTotal = (double)(oTargetDoc.Total - oTargetDoc.ReturnAmt);
                        }
                        // End ver 1.0.10

                        int rc = oDoc.Add();
                        if (rc != 0)
                        {
                            string temp = sap.oCom.GetLastErrorDescription();
                            if (sap.oCom.InTransaction)
                            {
                                sap.oCom.EndTransaction(BoWfTransOpt.wf_RollBack);
                            }

                            IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                            SalesOrderCollection obj = osupdate.GetObjectByKey<SalesOrderCollection>(oTargetDoc.Oid);

                            SalesOrderCollectionDocStatus ds = osupdate.CreateObject<SalesOrderCollectionDocStatus>();
                            ds.CreateUser = osupdate.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                            ds.CreateDate = DateTime.Now;
                            ds.DocStatus = DocStatus.PendPost;
                            ds.DocRemarks = "SAP Error:" + temp;
                            obj.SalesOrderCollectionDocStatus.Add(ds);

                            osupdate.CommitChanges();

                            WriteLog("[Error]", "Message: AR Downpayment Posting :" + oTargetDoc + "-" + temp);

                            return -1;
                        }
                        return 1;
                    }
                    else
                    {
                        return 2;
                    }
                }
                return 0;
            }
            catch (Exception ex)
            {
                IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                SalesOrderCollection obj = osupdate.GetObjectByKey<SalesOrderCollection>(oTargetDoc.Oid);

                SalesOrderCollectionDocStatus ds = osupdate.CreateObject<SalesOrderCollectionDocStatus>();
                ds.CreateUser = osupdate.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                ds.CreateDate = DateTime.Now;
                ds.DocStatus = DocStatus.PendPost;
                ds.DocRemarks = "SAP Error:" + ex.Message;
                obj.SalesOrderCollectionDocStatus.Add(ds);

                osupdate.CommitChanges();

                WriteLog("[Error]", "Message: AR Downpayment Posting :" + oTargetDoc + "-" + ex.Message);

                return -1;
            }
        }

        public int PostDPPaymenttoSAP(SalesOrderCollection oTargetDoc, SalesOrderCollectionDetails detail, string sodocnum,
            IObjectSpaceProvider ObjectSpaceProvider, SAPCompany sap, int DPDocEntry)
        {
            // return 0 = post nothing
            // return -1 = posting error
            // return 1 = posting successful
            try
            {
                //foreach (SalesOrderCollectionDetails payment in oTargetDoc.SalesOrderCollectionDetails)
                //{
                if (detail.SalesOrder == sodocnum)
                {
                    IObjectSpace os = ObjectSpaceProvider.CreateObjectSpace();
                    SalesOrder so = os.FindObject<SalesOrder>(CriteriaOperator.Parse("DocNum = ?", sodocnum));

                    if (so != null)
                    {
                        Guid g;
                        // Create and display the value of two GUIDs.
                        g = Guid.NewGuid();

                        SAPbobsCOM.Payments oDoc = null;

                        oDoc = (SAPbobsCOM.Payments)sap.oCom.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oIncomingPayments);

                        oDoc.CardCode = so.Customer.BPCode;
                        oDoc.CardName = so.CustomerName;
                        oDoc.DocDate = oTargetDoc.DocDate;
                        oDoc.UserFields.Fields.Item("U_PortalDocNum").Value = oTargetDoc.DocNum;
                        if (so.BillingAddressfield != null)
                        {
                            oDoc.Address = so.BillingAddressfield;
                        }

                        // Start ver 1.0.10
                        //if (detail.PaymentAmount > 0)
                        if (detail.PaymentAmount - oTargetDoc.ReturnAmt > 0)
                        // End ver 1.0.10
                        {
                            if (oTargetDoc.PaymentType.PaymentMean == "CASH")
                            {
                                // Start ver 1.0.10
                                //oDoc.CashSum += Convert.ToDouble(detail.PaymentAmount);
                                oDoc.CashSum += Convert.ToDouble(detail.PaymentAmount - oTargetDoc.ReturnAmt);
                                // End ver 1.0.10
                                if (detail.GLAccount != null)
                                {
                                    // Start ver 1.0.8
                                    //oDoc.CashAccount = detail.GLAccount.AcctCode;
                                    oDoc.CashAccount = oTargetDoc.PaymentType.GLAccount;
                                    // End ver 1.0.8
                                }
                            }

                            if (oTargetDoc.PaymentType.PaymentMean == "CCARD")
                            {
                                // Start ver 1.0.10
                                //oDoc.CreditCards.CreditSum += Convert.ToDouble(detail.PaymentAmount);
                                oDoc.CreditCards.CreditSum += Convert.ToDouble(detail.PaymentAmount - oTargetDoc.ReturnAmt);
                                // End ver 1.0.10
                                // Start ver 1.0.8
                                //oDoc.CreditCards.CreditAcct = detail.GLAccount.AcctCode;
                                oDoc.CreditCards.CreditAcct = oTargetDoc.PaymentType.GLAccount;
                                // End ver 1.0.8
                                oDoc.CreditCards.PaymentMethodCode = oTargetDoc.PaymentType.CCardPayMethodCode;

                                //oDoc.CreditCards.VoucherNum = oTargetDoc.ReferenceNum;
                                int countchar = 0;
                                if (oTargetDoc.ReferenceNum != null)
                                {
                                    foreach (char c in oTargetDoc.ReferenceNum)
                                    {
                                        countchar++;
                                    }
                                    if (countchar >= 19)
                                    {
                                        oDoc.CreditCards.VoucherNum = oTargetDoc.ReferenceNum.Substring(0, 19).ToString();
                                    }
                                    else
                                    {
                                        oDoc.CreditCards.VoucherNum = oTargetDoc.ReferenceNum;
                                    }
                                }

                                oDoc.CreditCards.CardValidUntil = DateTime.Parse("01/12/" + DateTime.Today.Year);
                                oDoc.CreditCards.CreditCardNumber = oTargetDoc.CreditCardNum;
                                oDoc.CreditCards.CreditCard = oTargetDoc.PaymentType.CCardPayMethodCode;
                            }

                            if (oTargetDoc.PaymentType.PaymentMean == "TRANSFER")
                            {
                                // Start ver 1.0.10
                                //oDoc.TransferSum += Convert.ToDouble(detail.PaymentAmount);
                                oDoc.TransferSum += Convert.ToDouble(detail.PaymentAmount - oTargetDoc.ReturnAmt);
                                // End ver 1.0.10
                                // Start ver 1.0.8
                                //oDoc.TransferAccount = detail.GLAccount.AcctCode;
                                oDoc.TransferAccount = oTargetDoc.PaymentType.GLAccount;
                                // End ver 1.0.8

                                //oDoc.TransferReference = oTargetDoc.ReferenceNum;
                                int countchar = 0;
                                if (oTargetDoc.ReferenceNum != null)
                                {
                                    foreach (char c in oTargetDoc.ReferenceNum)
                                    {
                                        countchar++;
                                    }
                                    if (countchar >= 24)
                                    {
                                        oDoc.TransferReference = oTargetDoc.ReferenceNum.Substring(0, 24).ToString();
                                    }
                                    else
                                    {
                                        oDoc.TransferReference = oTargetDoc.ReferenceNum;
                                    }
                                }
                            }

                            if (oTargetDoc.PaymentType.PaymentMean == "CHEQUE")
                            {
                                // Start ver 1.0.10
                                //oDoc.Checks.CheckSum += Convert.ToDouble(detail.PaymentAmount);
                                oDoc.Checks.CheckSum += Convert.ToDouble(detail.PaymentAmount - oTargetDoc.ReturnAmt);
                                // End ver 1.0.10
                                // Start ver 1.0.8
                                //oDoc.Checks.CheckAccount = detail.GLAccount.AcctCode;
                                oDoc.Checks.CheckAccount = oTargetDoc.PaymentType.GLAccount;
                                // End ver 1.0.8
                                oDoc.Checks.BankCode = oTargetDoc.ChequeBank.BankCode;
                                oDoc.Checks.CheckNumber = int.Parse(oTargetDoc.CheckNum);
                            }
                        }

                        oDoc.Invoices.DocEntry = DPDocEntry;
                        oDoc.Invoices.InvoiceType = BoRcptInvTypes.it_DownPayment;
                        oDoc.Invoices.SumApplied = Convert.ToDouble(detail.Total);
                        oDoc.Invoices.Add();

                        int rc = oDoc.Add();
                        if (rc != 0)
                        {
                            string temp = sap.oCom.GetLastErrorDescription();
                            if (sap.oCom.InTransaction)
                            {
                                sap.oCom.EndTransaction(BoWfTransOpt.wf_RollBack);
                            }

                            IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                            SalesOrderCollection obj = osupdate.GetObjectByKey<SalesOrderCollection>(oTargetDoc.Oid);

                            SalesOrderCollectionDocStatus ds = osupdate.CreateObject<SalesOrderCollectionDocStatus>();
                            ds.CreateUser = osupdate.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                            ds.CreateDate = DateTime.Now;
                            ds.DocStatus = DocStatus.PendPost;
                            ds.DocRemarks = "SAP Error:" + temp;
                            obj.SalesOrderCollectionDocStatus.Add(ds);

                            osupdate.CommitChanges();

                            WriteLog("[Error]", "Message: AR Downpayment Payment Posting :" + oTargetDoc + "-" + temp);

                            return -1;
                        }
                        return 1;
                    }
                }
                return 0;
                //}
                //return 0;
            }
            catch (Exception ex)
            {
                IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                SalesOrderCollection obj = osupdate.GetObjectByKey<SalesOrderCollection>(oTargetDoc.Oid);

                SalesOrderCollectionDocStatus ds = osupdate.CreateObject<SalesOrderCollectionDocStatus>();
                ds.CreateUser = osupdate.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                ds.CreateDate = DateTime.Now;
                ds.DocStatus = DocStatus.PendPost;
                ds.DocRemarks = "SAP Error:" + ex.Message;
                obj.SalesOrderCollectionDocStatus.Add(ds);

                osupdate.CommitChanges();

                WriteLog("[Error]", "Message: AR Downpayment Payment Posting :" + oTargetDoc + "-" + ex.Message);

                return -1;
            }
        }

        public int PostDOtoSAP(DeliveryOrder oTargetDoc, IObjectSpaceProvider ObjectSpaceProvider, SAPCompany sap)
        {
            // return 0 = post nothing
            // return -1 = posting error
            // return 1 = posting successful
            try
            {
                if (!oTargetDoc.Sap)
                {
                    IObjectSpace bos = ObjectSpaceProvider.CreateObjectSpace();
                    // Start ver 1.0.10
                    string sodocnum = null;
                    // End ver 1.0.10
                    Guid g;
                    // Create and display the value of two GUIDs.
                    g = Guid.NewGuid();

                    SAPbobsCOM.Documents oDoc = null;

                    oDoc = (SAPbobsCOM.Documents)sap.oCom.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInvoices);

                    oDoc.CardCode = oTargetDoc.Customer.BPCode;
                    oDoc.CardName = oTargetDoc.CustomerName;
                    oDoc.DocDate = DateTime.Now;
                    oDoc.Comments = oTargetDoc.Remarks;
                    oDoc.UserFields.Fields.Item("U_PortalDocNum").Value = oTargetDoc.DocNum;
                    // Start ver 1.0.18
                    vwBillingAddress BillingAddress = bos.FindObject<vwBillingAddress>(CriteriaOperator.Parse("AddressKey = ? and CardCode = ?"
                        , oTargetDoc.Customer.BillToDef, oTargetDoc.Customer.BPCode));
                    vwShippingAddress ShippingAddress = bos.FindObject<vwShippingAddress>(CriteriaOperator.Parse("AddressKey = ? and CardCode = ?"
                        , oTargetDoc.Customer.ShipToDef, oTargetDoc.Customer.BPCode));

                    if (BillingAddress != null)
                    {
                        oDoc.PayToCode = BillingAddress.AddressKey;
                    }
                    if (ShippingAddress != null)
                    {
                        oDoc.ShipToCode = ShippingAddress.AddressKey;
                    }
                    // End ver 1.0.18

                    // Start ver 1.0.18
                    // Buyer
                    if (oTargetDoc.EIVConsolidate != null)
                    {
                        if (oTargetDoc.EIVConsolidate.Code == "Y")
                        {
                            oDoc.UserFields.Fields.Item("U_EIV_Consolidate").Value = "N";
                        }
                        else
                        {
                            oDoc.UserFields.Fields.Item("U_EIV_Consolidate").Value = "Y";
                        }
                    }
                    if (oTargetDoc.EIVType != null)
                    {
                        oDoc.UserFields.Fields.Item("U_EIV_InvoiceType").Value = oTargetDoc.EIVType.Code;
                    }
                    if (oTargetDoc.EIVFreqSync != null)
                    {
                        oDoc.UserFields.Fields.Item("U_EIV_FreqSync").Value = oTargetDoc.EIVFreqSync.Code;
                    }
                    oDoc.UserFields.Fields.Item("U_EIV_BuyerName").Value = oTargetDoc.CustomerName == null ? "" : oTargetDoc.CustomerName;
                    oDoc.UserFields.Fields.Item("U_EIV_BuyerTin").Value = oTargetDoc.EIVBuyerTIN == null ? "" : oTargetDoc.EIVBuyerTIN;
                    oDoc.UserFields.Fields.Item("U_EIV_BuyerRegNum").Value = oTargetDoc.EIVBuyerRegNum == null ? "" : oTargetDoc.EIVBuyerRegNum;
                    if (oTargetDoc.EIVBuyerRegTyp != null)
                    {
                        oDoc.UserFields.Fields.Item("U_EIV_BuyerRegTyp").Value = oTargetDoc.EIVBuyerRegTyp.Code;
                    }
                    oDoc.UserFields.Fields.Item("U_EIV_BuyerSSTRegNum").Value = oTargetDoc.EIVBuyerSSTRegNum == null ? "" : oTargetDoc.EIVBuyerSSTRegNum;
                    oDoc.UserFields.Fields.Item("U_EIV_BuyerEmail").Value = oTargetDoc.EIVBuyerEmail == null ? "" : oTargetDoc.EIVBuyerEmail;
                    oDoc.UserFields.Fields.Item("U_EIV_BuyerContact").Value = oTargetDoc.EIVBuyerContact == null ? "" : oTargetDoc.EIVBuyerContact;

                    oDoc.AddressExtension.BillToStreet = oTargetDoc.EIVAddressLine1B == null ? "" : oTargetDoc.EIVAddressLine1B;
                    oDoc.AddressExtension.BillToBlock = oTargetDoc.EIVAddressLine2B == null ? "" : oTargetDoc.EIVAddressLine2B;
                    oDoc.AddressExtension.BillToCity = oTargetDoc.EIVAddressLine3B == null ? "" : oTargetDoc.EIVAddressLine3B;
                    oDoc.AddressExtension.BillToCounty = oTargetDoc.EIVCityNameB == null ? "" : oTargetDoc.EIVCityNameB;
                    oDoc.AddressExtension.BillToZipCode = oTargetDoc.EIVPostalZoneB == null ? "" : oTargetDoc.EIVPostalZoneB;

                    oDoc.AddressExtension.UserFields.Fields.Item("U_EIV_AddressLine1B").Value = oTargetDoc.EIVAddressLine1B == null ? "" : oTargetDoc.EIVAddressLine1B;
                    oDoc.AddressExtension.UserFields.Fields.Item("U_EIV_AddressLine2B").Value = oTargetDoc.EIVAddressLine2B == null ? "" : oTargetDoc.EIVAddressLine2B;
                    oDoc.AddressExtension.UserFields.Fields.Item("U_EIV_AddressLine3B").Value = oTargetDoc.EIVAddressLine3B == null ? "" : oTargetDoc.EIVAddressLine3B;
                    oDoc.AddressExtension.UserFields.Fields.Item("U_EIV_PostalZoneB").Value = oTargetDoc.EIVPostalZoneB == null ? "" : oTargetDoc.EIVPostalZoneB;
                    oDoc.AddressExtension.UserFields.Fields.Item("U_EIV_CityNameB").Value = oTargetDoc.EIVCityNameB == null ? "" : oTargetDoc.EIVCityNameB;
                    if (oTargetDoc.EIVStateB != null)
                    {
                        oDoc.AddressExtension.BillToState = oTargetDoc.EIVStateB.Code;
                    }
                    if (oTargetDoc.EIVCountryB != null)
                    {
                        oDoc.AddressExtension.BillToCountry = oTargetDoc.EIVCountryB.Code;
                    }

                    // Recipient
                    oDoc.UserFields.Fields.Item("U_EIV_ShippingName").Value = oTargetDoc.EIVShippingName == null ? "" : oTargetDoc.EIVShippingName;
                    oDoc.UserFields.Fields.Item("U_EIV_ShippingTin").Value = oTargetDoc.EIVShippingTin == null ? "" : oTargetDoc.EIVShippingTin;
                    oDoc.UserFields.Fields.Item("U_EIV_ShippingRegNum").Value = oTargetDoc.EIVShippingRegNum == null ? "" : oTargetDoc.EIVShippingRegNum;
                    if (oTargetDoc.EIVShippingRegTyp != null)
                    {
                        oDoc.UserFields.Fields.Item("U_EIV_ShippingRegTyp").Value = oTargetDoc.EIVShippingRegTyp.Code;
                    }

                    oDoc.AddressExtension.ShipToStreet = oTargetDoc.EIVAddressLine1S == null ? "" : oTargetDoc.EIVAddressLine1S;
                    oDoc.AddressExtension.ShipToBlock = oTargetDoc.EIVAddressLine2S == null ? "" : oTargetDoc.EIVAddressLine2S;
                    oDoc.AddressExtension.ShipToCity = oTargetDoc.EIVAddressLine3S == null ? "" : oTargetDoc.EIVAddressLine3S;
                    oDoc.AddressExtension.ShipToCounty = oTargetDoc.EIVCityNameS == null ? "" : oTargetDoc.EIVCityNameS;
                    oDoc.AddressExtension.ShipToZipCode = oTargetDoc.EIVPostalZoneS == null ? "" : oTargetDoc.EIVPostalZoneS;

                    oDoc.AddressExtension.UserFields.Fields.Item("U_EIV_AddressLine1S").Value = oTargetDoc.EIVAddressLine1S == null ? "" : oTargetDoc.EIVAddressLine1S;
                    oDoc.AddressExtension.UserFields.Fields.Item("U_EIV_AddressLine2S").Value = oTargetDoc.EIVAddressLine2S == null ? "" : oTargetDoc.EIVAddressLine2S;
                    oDoc.AddressExtension.UserFields.Fields.Item("U_EIV_AddressLine3S").Value = oTargetDoc.EIVAddressLine3S == null ? "" : oTargetDoc.EIVAddressLine3S;
                    oDoc.AddressExtension.UserFields.Fields.Item("U_EIV_PostalZoneS").Value = oTargetDoc.EIVPostalZoneS == null ? "" : oTargetDoc.EIVPostalZoneS;
                    oDoc.AddressExtension.UserFields.Fields.Item("U_EIV_CityNameS").Value = oTargetDoc.EIVCityNameS == null ? "" : oTargetDoc.EIVCityNameS;
                    if (oTargetDoc.EIVStateS != null)
                    {
                        oDoc.AddressExtension.ShipToState = oTargetDoc.EIVStateS.Code;
                    }
                    if (oTargetDoc.EIVCountryS != null)
                    {
                        oDoc.AddressExtension.ShipToCountry = oTargetDoc.EIVCountryS.Code;
                    }
                    // End ver 1.0.18

                    int cnt = 0;
                    foreach (DeliveryOrderDetails dtl in oTargetDoc.DeliveryOrderDetails)
                    {
                        cnt++;
                        if (cnt == 1)
                        {
                        }
                        else
                        {
                            oDoc.Lines.Add();
                            oDoc.Lines.SetCurrentLine(oDoc.Lines.Count - 1);
                        }

                        oDoc.Lines.ItemCode = dtl.ItemCode.ItemCode;
                        oDoc.Lines.ItemDescription = dtl.ItemDesc;
                        oDoc.Lines.Quantity = (double)dtl.Quantity;
                        oDoc.Lines.UnitPrice = (double)dtl.Price;
                        if (dtl.Warehouse != null)
                        {
                            oDoc.Lines.WarehouseCode = dtl.Warehouse.WarehouseCode;
                        }
                        oDoc.Lines.UserFields.Fields.Item("U_PortalLineOid").Value = dtl.Oid.ToString();
                        // Start ver 1.0.18
                        if (dtl.EIVClassification != null)
                        {
                            oDoc.Lines.UserFields.Fields.Item("U_EIV_Classification").Value = dtl.EIVClassification.Code;
                        }
                        // End ver 1.0.18

                        if (dtl.Bin != null)
                        {
                            oDoc.Lines.BinAllocations.BinAbsEntry = dtl.Bin.AbsEntry;
                            oDoc.Lines.BinAllocations.Quantity = (double)dtl.Quantity;
                        }

                        string getdoDocentry = "SELECT T1.DocEntry, T1.LineNum, T1.U_PortalLineOid " +
                         "From [" + ConfigurationManager.AppSettings["CompanyDB"].ToString() + "]..ODLN T0 " +
                         "INNER join [" + ConfigurationManager.AppSettings["CompanyDB"].ToString() + "]..DLN1 T1 on T0.DocEntry = T1.DocEntry " +
                         "WHERE U_PortalDocNum = '" + oTargetDoc.DocNum + "'";
                        if (conn.State == ConnectionState.Open)
                        {
                            conn.Close();
                        }
                        conn.Open();
                        SqlCommand cmd1 = new SqlCommand(getdoDocentry, conn);
                        SqlDataReader reader1 = cmd1.ExecuteReader();
                        while (reader1.Read())
                        {
                            if (reader1.GetString(2) == dtl.Oid.ToString())
                            {
                                oDoc.Lines.BaseType = 15;
                                oDoc.Lines.BaseEntry = reader1.GetInt32(0);
                                oDoc.Lines.BaseLine = reader1.GetInt32(1);
                            }
                        }
                        conn.Close();

                        IObjectSpace os = ObjectSpaceProvider.CreateObjectSpace();
                        SalesOrder so = os.FindObject<SalesOrder>(CriteriaOperator.Parse("DocNum = ?", dtl.SODocNum));

                        // Start ver 1.0.10
                        sodocnum = so.DocNum;
                        // End ver 1.0.10

                        if (so.Series.SeriesName == "Cash")
                        {
                            IObjectSpace fos = ObjectSpaceProvider.CreateObjectSpace();
                            vwSeries series = fos.FindObject<vwSeries>(CriteriaOperator.Parse("SeriesName = ? and ObjectCode = ?",
                                "Cash", "13"));

                            if (series != null)
                            {
                                oDoc.Series = int.Parse(series.Series);
                            }
                        }
                        else
                        {
                            IObjectSpace fos = ObjectSpaceProvider.CreateObjectSpace();
                            vwSeries series = fos.FindObject<vwSeries>(CriteriaOperator.Parse("SeriesName = ? and ObjectCode = ?",
                                "Term", "13"));

                            if (series != null)
                            {
                                oDoc.Series = int.Parse(series.Series);
                            }
                        }
                    }

                    // Start ver 1.0.10
                    //string getdpDocentry = "SELECT T0.DocEntry, T0.DocTotal FROM [" + ConfigurationManager.AppSettings["CompanyDB"].ToString() + "]..ODPI T0 " +
                    //      "LEFT JOIN DeliveryOrderDetails T1 on T0.U_SoDocNumber = T1.SODocNum COLLATE DATABASE_DEFAULT " +
                    //      "WHERE T1.DeliveryOrder = " + oTargetDoc.Oid + " " +
                    //      "GROUP BY T0.DocEntry, T0.DocTotal";
                    string getdpDocentry = "SELECT T0.DocEntry, T0.DocTotal, T0.DocTotal - SUM(ISNULL(T2.Total, 0)) as Balance " +
                        "FROM [" + ConfigurationManager.AppSettings["CompanyDB"].ToString() + "]..ODPI T0 " +
                        "LEFT JOIN DeliveryOrder T1 on T1.SONo = T0.U_SoDocNumber COLLATE DATABASE_DEFAULT " +
                        "AND T1.GCRecord is null and T1.SapINV = 1 " +
                        "LEFT JOIN DeliveryOrderDetails T2 on T1.OID = T2.DeliveryOrder and T2.GCRecord is null " +
                        "WHERE T0.U_SoDocNumber = '" + sodocnum + "' " +
                        "GROUP BY T0.DocEntry, T0.DocTotal";
                    // End ver 1.0.10
                    if (conn.State == ConnectionState.Open)
                    {
                        conn.Close();
                    }
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(getdpDocentry, conn);
                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        // Start ver 1.0.10
                        if (reader.GetDecimal(2) > 0)
                        {
                            double drawamt = (double)reader.GetDecimal(2) - (double)oTargetDoc.DeliveryOrderDetails.Sum(s => s.Total);
                        // End ver 1.0.10
                            SAPbobsCOM.DownPaymentsToDraw dpm = oDoc.DownPaymentsToDraw;
                            dpm.DocEntry = reader.GetInt32(0);
                            // Start ver 1.0.10
                            //dpm.AmountToDraw = (double)oTargetDoc.DeliveryOrderDetails.Sum(s => s.Total);
                            //dpm.AmountToDraw = (double)reader.GetDecimal(1);
                            if (drawamt <= 0)
                            {
                                dpm.AmountToDraw = (double)reader.GetDecimal(2);
                            }
                            else
                            {
                                dpm.AmountToDraw = (double)oTargetDoc.DeliveryOrderDetails.Sum(s => s.Total);
                            }
                            // End ver 1.0.10
                            dpm.Add();
                        // Start ver 1.0.10
                        }
                        // End ver 1.0.10
                    }
                    conn.Close();

                    int rc = oDoc.Add();
                    if (rc != 0)
                    {
                        string temp = sap.oCom.GetLastErrorDescription();
                        if (sap.oCom.InTransaction)
                        {
                            sap.oCom.EndTransaction(BoWfTransOpt.wf_RollBack);
                        }

                        IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                        DeliveryOrder obj = osupdate.GetObjectByKey<DeliveryOrder>(oTargetDoc.Oid);

                        DeliveryOrderDocTrail ds = osupdate.CreateObject<DeliveryOrderDocTrail>();
                        ds.CreateUser = osupdate.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                        ds.CreateDate = DateTime.Now;
                        ds.DocStatus = DocStatus.PendPost;
                        ds.DocRemarks = "SAP Error:" + temp;
                        obj.DeliveryOrderDocTrail.Add(ds);

                        osupdate.CommitChanges();

                        WriteLog("[Error]", "Message: Delivery Order Posting :" + oTargetDoc + "-" + temp);

                        return -1;
                    }
                    return 1;
                }
                return 0;
            }
            catch (Exception ex)
            {
                IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                DeliveryOrder obj = osupdate.GetObjectByKey<DeliveryOrder>(oTargetDoc.Oid);

                DeliveryOrderDocTrail ds = osupdate.CreateObject<DeliveryOrderDocTrail>();
                ds.CreateUser = osupdate.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                ds.CreateDate = DateTime.Now;
                ds.DocStatus = DocStatus.PendPost;
                ds.DocRemarks = "SAP Error:" + ex.Message;
                obj.DeliveryOrderDocTrail.Add(ds);

                osupdate.CommitChanges();

                WriteLog("[Error]", "Message: Delivery Order Posting :" + oTargetDoc + "-" + ex.Message);

                return -1;
            }
        }

        public int PostARDOtoSAP(DeliveryOrder oTargetDoc, IObjectSpaceProvider ObjectSpaceProvider, SAPCompany sap)
        {
            // return 0 = post nothing
            // return -1 = posting error
            // return 1 = posting successful
            try
            {
                if (!oTargetDoc.Sap)
                {
                    IObjectSpace fos = ObjectSpaceProvider.CreateObjectSpace();
                    Guid g;
                    // Create and display the value of two GUIDs.
                    g = Guid.NewGuid();

                    SAPbobsCOM.Documents oDoc = null;

                    oDoc = (SAPbobsCOM.Documents)sap.oCom.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oDeliveryNotes);

                    oDoc.CardCode = oTargetDoc.Customer.BPCode;
                    oDoc.CardName = oTargetDoc.CustomerName;
                    oDoc.DocDate = DateTime.Now;
                    oDoc.Comments = oTargetDoc.Remarks;
                    oDoc.UserFields.Fields.Item("U_PortalDocNum").Value = oTargetDoc.DocNum;
                    // Start ver 1.0.18
                    vwBillingAddress BillingAddress = fos.FindObject<vwBillingAddress>(CriteriaOperator.Parse("AddressKey = ? and CardCode = ?"
                        , oTargetDoc.Customer.BillToDef, oTargetDoc.Customer.BPCode));
                    vwShippingAddress ShippingAddress = fos.FindObject<vwShippingAddress>(CriteriaOperator.Parse("AddressKey = ? and CardCode = ?"
                        , oTargetDoc.Customer.ShipToDef, oTargetDoc.Customer.BPCode));

                    if (BillingAddress != null)
                    {
                        oDoc.PayToCode = BillingAddress.AddressKey;
                    }
                    if (ShippingAddress != null)
                    {
                        oDoc.ShipToCode = ShippingAddress.AddressKey;
                    }
                    // End ver 1.0.18

                    // Start ver 1.0.18
                    // Buyer
                    if (oTargetDoc.EIVConsolidate != null)
                    {
                        if (oTargetDoc.EIVConsolidate.Code == "Y")
                        {
                            oDoc.UserFields.Fields.Item("U_EIV_Consolidate").Value = "N";
                        }
                        else
                        {
                            oDoc.UserFields.Fields.Item("U_EIV_Consolidate").Value = "Y";
                        }
                    }
                    if (oTargetDoc.EIVType != null)
                    {
                        oDoc.UserFields.Fields.Item("U_EIV_InvoiceType").Value = oTargetDoc.EIVType.Code;
                    }
                    if (oTargetDoc.EIVFreqSync != null)
                    {
                        oDoc.UserFields.Fields.Item("U_EIV_FreqSync").Value = oTargetDoc.EIVFreqSync.Code;
                    }
                    oDoc.UserFields.Fields.Item("U_EIV_BuyerName").Value = oTargetDoc.CustomerName == null ? "" : oTargetDoc.CustomerName;
                    oDoc.UserFields.Fields.Item("U_EIV_BuyerTin").Value = oTargetDoc.EIVBuyerTIN == null ? "" : oTargetDoc.EIVBuyerTIN;
                    oDoc.UserFields.Fields.Item("U_EIV_BuyerRegNum").Value = oTargetDoc.EIVBuyerRegNum == null ? "" : oTargetDoc.EIVBuyerRegNum;
                    if (oTargetDoc.EIVBuyerRegTyp != null)
                    {
                        oDoc.UserFields.Fields.Item("U_EIV_BuyerRegTyp").Value = oTargetDoc.EIVBuyerRegTyp.Code;
                    }
                    oDoc.UserFields.Fields.Item("U_EIV_BuyerSSTRegNum").Value = oTargetDoc.EIVBuyerSSTRegNum == null ? "" : oTargetDoc.EIVBuyerSSTRegNum;
                    oDoc.UserFields.Fields.Item("U_EIV_BuyerEmail").Value = oTargetDoc.EIVBuyerEmail == null ? "" : oTargetDoc.EIVBuyerEmail;
                    oDoc.UserFields.Fields.Item("U_EIV_BuyerContact").Value = oTargetDoc.EIVBuyerContact == null ? "" : oTargetDoc.EIVBuyerContact;

                    oDoc.AddressExtension.BillToStreet = oTargetDoc.EIVAddressLine1B == null ? "" : oTargetDoc.EIVAddressLine1B;
                    oDoc.AddressExtension.BillToBlock = oTargetDoc.EIVAddressLine2B == null ? "" : oTargetDoc.EIVAddressLine2B;
                    oDoc.AddressExtension.BillToCity = oTargetDoc.EIVAddressLine3B == null ? "" : oTargetDoc.EIVAddressLine3B;
                    oDoc.AddressExtension.BillToCounty = oTargetDoc.EIVCityNameB == null ? "" : oTargetDoc.EIVCityNameB;
                    oDoc.AddressExtension.BillToZipCode = oTargetDoc.EIVPostalZoneB == null ? "" : oTargetDoc.EIVPostalZoneB;

                    oDoc.AddressExtension.UserFields.Fields.Item("U_EIV_AddressLine1B").Value = oTargetDoc.EIVAddressLine1B == null ? "" : oTargetDoc.EIVAddressLine1B;
                    oDoc.AddressExtension.UserFields.Fields.Item("U_EIV_AddressLine2B").Value = oTargetDoc.EIVAddressLine2B == null ? "" : oTargetDoc.EIVAddressLine2B;
                    oDoc.AddressExtension.UserFields.Fields.Item("U_EIV_AddressLine3B").Value = oTargetDoc.EIVAddressLine3B == null ? "" : oTargetDoc.EIVAddressLine3B;
                    oDoc.AddressExtension.UserFields.Fields.Item("U_EIV_PostalZoneB").Value = oTargetDoc.EIVPostalZoneB == null ? "" : oTargetDoc.EIVPostalZoneB;
                    oDoc.AddressExtension.UserFields.Fields.Item("U_EIV_CityNameB").Value = oTargetDoc.EIVCityNameB == null ? "" : oTargetDoc.EIVCityNameB;
                    if (oTargetDoc.EIVStateB != null)
                    {
                        oDoc.AddressExtension.BillToState = oTargetDoc.EIVStateB.Code;
                    }
                    if (oTargetDoc.EIVCountryB != null)
                    {
                        oDoc.AddressExtension.BillToCountry = oTargetDoc.EIVCountryB.Code;
                    }

                    // Recipient
                    oDoc.UserFields.Fields.Item("U_EIV_ShippingName").Value = oTargetDoc.EIVShippingName == null ? "" : oTargetDoc.EIVShippingName;
                    oDoc.UserFields.Fields.Item("U_EIV_ShippingTin").Value = oTargetDoc.EIVShippingTin == null ? "" : oTargetDoc.EIVShippingTin;
                    oDoc.UserFields.Fields.Item("U_EIV_ShippingRegNum").Value = oTargetDoc.EIVShippingRegNum == null ? "" : oTargetDoc.EIVShippingRegNum;
                    if (oTargetDoc.EIVShippingRegTyp != null)
                    {
                        oDoc.UserFields.Fields.Item("U_EIV_ShippingRegTyp").Value = oTargetDoc.EIVShippingRegTyp.Code;
                    }

                    oDoc.AddressExtension.ShipToStreet = oTargetDoc.EIVAddressLine1S == null ? "" : oTargetDoc.EIVAddressLine1S;
                    oDoc.AddressExtension.ShipToBlock = oTargetDoc.EIVAddressLine2S == null ? "" : oTargetDoc.EIVAddressLine2S;
                    oDoc.AddressExtension.ShipToCity = oTargetDoc.EIVAddressLine3S == null ? "" : oTargetDoc.EIVAddressLine3S;
                    oDoc.AddressExtension.ShipToCounty = oTargetDoc.EIVCityNameS == null ? "" : oTargetDoc.EIVCityNameS;
                    oDoc.AddressExtension.ShipToZipCode = oTargetDoc.EIVPostalZoneS == null ? "" : oTargetDoc.EIVPostalZoneS;

                    oDoc.AddressExtension.UserFields.Fields.Item("U_EIV_AddressLine1S").Value = oTargetDoc.EIVAddressLine1S == null ? "" : oTargetDoc.EIVAddressLine1S;
                    oDoc.AddressExtension.UserFields.Fields.Item("U_EIV_AddressLine2S").Value = oTargetDoc.EIVAddressLine2S == null ? "" : oTargetDoc.EIVAddressLine2S;
                    oDoc.AddressExtension.UserFields.Fields.Item("U_EIV_AddressLine3S").Value = oTargetDoc.EIVAddressLine3S == null ? "" : oTargetDoc.EIVAddressLine3S;
                    oDoc.AddressExtension.UserFields.Fields.Item("U_EIV_PostalZoneS").Value = oTargetDoc.EIVPostalZoneS == null ? "" : oTargetDoc.EIVPostalZoneS;
                    oDoc.AddressExtension.UserFields.Fields.Item("U_EIV_CityNameS").Value = oTargetDoc.EIVCityNameS == null ? "" : oTargetDoc.EIVCityNameS;
                    if (oTargetDoc.EIVStateS != null)
                    {
                        oDoc.AddressExtension.ShipToState = oTargetDoc.EIVStateS.Code;
                    }
                    if (oTargetDoc.EIVCountryS != null)
                    {
                        oDoc.AddressExtension.ShipToCountry = oTargetDoc.EIVCountryS.Code;
                    }
                    // End ver 1.0.18

                    int cnt = 0;
                    foreach (DeliveryOrderDetails dtl in oTargetDoc.DeliveryOrderDetails)
                    {
                        cnt++;
                        if (cnt == 1)
                        {
                        }
                        else
                        {
                            oDoc.Lines.Add();
                            oDoc.Lines.SetCurrentLine(oDoc.Lines.Count - 1);
                        }

                        oDoc.Lines.ItemCode = dtl.ItemCode.ItemCode;
                        oDoc.Lines.ItemDescription = dtl.ItemDesc;
                        oDoc.Lines.Quantity = (double)dtl.Quantity;
                        oDoc.Lines.UnitPrice = (double)dtl.Price;
                        if (dtl.Warehouse != null)
                        {
                            oDoc.Lines.WarehouseCode = dtl.Warehouse.WarehouseCode;
                        }
                        oDoc.Lines.UserFields.Fields.Item("U_PortalLineOid").Value = dtl.Oid.ToString();
                        // Start ver 1.0.18
                        if (dtl.EIVClassification != null)
                        {
                            oDoc.Lines.UserFields.Fields.Item("U_EIV_Classification").Value = dtl.EIVClassification.Code;
                        }
                        // End ver 1.0.18

                        if (dtl.Bin != null)
                        {
                            oDoc.Lines.BinAllocations.BinAbsEntry = dtl.Bin.AbsEntry;
                            oDoc.Lines.BinAllocations.Quantity = (double)dtl.Quantity;
                        }

                        IObjectSpace os = ObjectSpaceProvider.CreateObjectSpace();
                        SalesOrder so = os.FindObject<SalesOrder>(CriteriaOperator.Parse("DocNum = ?", dtl.SODocNum));

                        foreach (SalesOrderDetails dtlsales in so.SalesOrderDetails)
                        {
                            if (dtlsales.SAPDocEntry != 0 && dtlsales.Oid.ToString() == dtl.SOBaseID)
                            {
                                oDoc.Lines.BaseType = 17;
                                oDoc.Lines.BaseEntry = dtlsales.SAPDocEntry;
                                oDoc.Lines.BaseLine = dtlsales.SAPBaseLine;
                            }
                        }
                    }

                    int rc = oDoc.Add();
                    if (rc != 0)
                    {
                        string temp = sap.oCom.GetLastErrorDescription();
                        if (sap.oCom.InTransaction)
                        {
                            sap.oCom.EndTransaction(BoWfTransOpt.wf_RollBack);
                        }

                        IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                        DeliveryOrder obj = osupdate.GetObjectByKey<DeliveryOrder>(oTargetDoc.Oid);

                        DeliveryOrderDocTrail ds = osupdate.CreateObject<DeliveryOrderDocTrail>();
                        ds.CreateUser = osupdate.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                        ds.CreateDate = DateTime.Now;
                        ds.DocStatus = DocStatus.PendPost;
                        ds.DocRemarks = "SAP Error:" + temp;
                        obj.DeliveryOrderDocTrail.Add(ds);

                        osupdate.CommitChanges();

                        WriteLog("[Error]", "Message: Delivery Order(DO) Posting :" + oTargetDoc + "-" + temp);

                        return -1;
                    }
                    return 1;
                }
                return 0;
            }
            catch (Exception ex)
            {
                IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                DeliveryOrder obj = osupdate.GetObjectByKey<DeliveryOrder>(oTargetDoc.Oid);

                DeliveryOrderDocTrail ds = osupdate.CreateObject<DeliveryOrderDocTrail>();
                ds.CreateUser = osupdate.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                ds.CreateDate = DateTime.Now;
                ds.DocStatus = DocStatus.PendPost;
                ds.DocRemarks = "SAP Error:" + ex.Message;
                obj.DeliveryOrderDocTrail.Add(ds);

                osupdate.CommitChanges();

                WriteLog("[Error]", "Message: Delivery Order(DO) Posting :" + oTargetDoc + "-" + ex.Message);

                return -1;
            }
        }

        public int PostPLtoSAP(PickList oTargetDoc, string warehouse, IObjectSpaceProvider ObjectSpaceProvider, SAPCompany sap)
        {
            // return 0 = post nothing
            // return -1 = posting error
            // return 1 = posting successful
            try
            {
                if (oTargetDoc.Sap)
                    return 0;

                IObjectSpace fos = ObjectSpaceProvider.CreateObjectSpace();
                Guid g;
                // Create and display the value of two GUIDs.
                g = Guid.NewGuid();

                if (oTargetDoc.PickListAttachment != null && oTargetDoc.PickListAttachment.Count > 0)
                {
                    foreach (PickListAttachment obj in oTargetDoc.PickListAttachment)
                    {
                        string fullpath = ConfigurationManager.AppSettings["B1AttachmentPath"].ToString() + g.ToString() + obj.File.FileName;
                        using (System.IO.FileStream fs = System.IO.File.OpenWrite(fullpath))
                        {
                            obj.File.SaveToStream(fs);
                        }
                    }
                }

                SAPbobsCOM.StockTransfer oDoc = null;

                oDoc = (SAPbobsCOM.StockTransfer)sap.oCom.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oStockTransfer);

                //oDoc.CardCode = oTargetDoc.Vendor.BoCode;
                //oDoc.CardName = oTargetDoc.Vendor.BoName;
                // Start ver 1.0.22
                //oDoc.DocDate = oTargetDoc.DocDate;
                //oDoc.TaxDate = oTargetDoc.DeliveryDate;
                oDoc.DocDate = DateTime.Today;
                oDoc.TaxDate = DateTime.Today;
                // End ver 1.0.22
                oDoc.Comments = oTargetDoc.Remarks;
                oDoc.FromWarehouse = warehouse;
                oDoc.ToWarehouse = warehouse;
                oDoc.UserFields.Fields.Item("U_PortalDocNum").Value = oTargetDoc.DocNum;

                int cnt = 0;
                int itemcount = 0;
                foreach (PickListDetailsActual dtl in oTargetDoc.PickListDetailsActual)
                {
                    if (dtl.Warehouse.WarehouseCode == warehouse)
                    {
                        if (dtl.FromBin.BinCode != dtl.ToBin.BinCode)
                        {
                            cnt++;
                            itemcount++;
                            if (cnt == 1)
                            {
                            }
                            else
                            {
                                oDoc.Lines.Add();
                                oDoc.Lines.SetCurrentLine(oDoc.Lines.Count - 1);
                            }

                            oDoc.Lines.FromWarehouseCode = dtl.Warehouse.WarehouseCode;
                            oDoc.Lines.WarehouseCode = dtl.Warehouse.WarehouseCode;

                            oDoc.Lines.ItemCode = dtl.ItemCode.ItemCode;
                            oDoc.Lines.ItemDescription = dtl.ItemCode.ItemName;
                            oDoc.Lines.Quantity = (double)dtl.PickQty;
                            oDoc.Lines.UserFields.Fields.Item("U_PortalLineOid").Value = dtl.Oid.ToString();

                            if (dtl.FromBin != null)
                            {
                                oDoc.Lines.BinAllocations.BinAbsEntry = dtl.FromBin.AbsEntry;
                                oDoc.Lines.BinAllocations.Quantity = (double)dtl.PickQty;
                                oDoc.Lines.BinAllocations.BinActionType = SAPbobsCOM.BinActionTypeEnum.batFromWarehouse;
                            }

                            if (dtl.ToBin != null)
                            {
                                oDoc.Lines.BinAllocations.Add();
                                oDoc.Lines.BinAllocations.BinAbsEntry = dtl.ToBin.AbsEntry;
                                oDoc.Lines.BinAllocations.Quantity = (double)dtl.PickQty;
                                oDoc.Lines.BinAllocations.BinActionType = SAPbobsCOM.BinActionTypeEnum.batToWarehouse;
                            }
                        }
                    }
                }
                if (oTargetDoc.PickListAttachment != null && oTargetDoc.PickListAttachment.Count > 0)
                {
                    cnt = 0;
                    SAPbobsCOM.Attachments2 oAtt = (SAPbobsCOM.Attachments2)sap.oCom.GetBusinessObject(BoObjectTypes.oAttachments2);
                    foreach (PickListAttachment dtl in oTargetDoc.PickListAttachment)
                    {

                        cnt++;
                        if (cnt == 1)
                        {
                            if (oAtt.Lines.Count == 0)
                                oAtt.Lines.Add();
                        }
                        else
                            oAtt.Lines.Add();

                        string attfile = "";
                        string[] fexe = dtl.File.FileName.Split('.');
                        if (fexe.Length <= 2)
                            attfile = fexe[0];
                        else
                        {
                            for (int x = 0; x < fexe.Length - 1; x++)
                            {
                                if (attfile == "")
                                    attfile = fexe[x];
                                else
                                    attfile += "." + fexe[x];
                            }
                        }
                        oAtt.Lines.FileName = g.ToString() + attfile;
                        if (fexe.Length > 1)
                            oAtt.Lines.FileExtension = fexe[fexe.Length - 1];
                        string path = ConfigurationManager.AppSettings["B1AttachmentPath"].ToString();
                        path = path.Replace("\\\\", "\\");
                        path = path.Substring(0, path.Length - 1);
                        oAtt.Lines.SourcePath = path;
                        oAtt.Lines.Override = SAPbobsCOM.BoYesNoEnum.tYES;
                    }
                    int iAttEntry = -1;
                    if (oAtt.Add() == 0)
                    {
                        iAttEntry = int.Parse(sap.oCom.GetNewObjectKey());
                    }
                    else
                    {
                        string temp = sap.oCom.GetLastErrorDescription();
                        if (sap.oCom.InTransaction)
                        {
                            sap.oCom.EndTransaction(BoWfTransOpt.wf_RollBack);
                        }

                        IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                        PickList obj = osupdate.GetObjectByKey<PickList>(oTargetDoc.Oid);

                        PickListDocTrail ds = osupdate.CreateObject<PickListDocTrail>();
                        ds.CreateUser = osupdate.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                        ds.CreateDate = DateTime.Now;
                        ds.DocRemarks = "SAP Error:" + temp;
                        obj.PickListDocTrail.Add(ds);

                        osupdate.CommitChanges();

                        WriteLog("[Error]", "Message: Pick List Attachement Error :" + oTargetDoc + "-" + temp);

                        return -1;
                    }
                    oDoc.AttachmentEntry = iAttEntry;
                }

                if (itemcount < 1)
                {
                    return 2;
                }

                int rc = oDoc.Add();
                if (rc != 0)
                {
                    string temp = sap.oCom.GetLastErrorDescription();
                    if (sap.oCom.InTransaction)
                    {
                        sap.oCom.EndTransaction(BoWfTransOpt.wf_RollBack);
                    }

                    IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                    PickList obj = osupdate.GetObjectByKey<PickList>(oTargetDoc.Oid);

                    PickListDocTrail ds = osupdate.CreateObject<PickListDocTrail>();
                    ds.CreateUser = osupdate.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                    ds.CreateDate = DateTime.Now;
                    ds.DocRemarks = "SAP Error:" + temp;
                    obj.PickListDocTrail.Add(ds);

                    osupdate.CommitChanges();

                    WriteLog("[Error]", "Message: Pick List Posting :" + oTargetDoc + "-" + temp);

                    return -1;
                }
                return 1;
            }
            catch (Exception ex)
            {
                IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                PickList obj = osupdate.GetObjectByKey<PickList>(oTargetDoc.Oid);

                PickListDocTrail ds = osupdate.CreateObject<PickListDocTrail>();
                ds.CreateUser = osupdate.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                ds.CreateDate = DateTime.Now;
                ds.DocRemarks = "SAP Error:" + ex.Message;
                obj.PickListDocTrail.Add(ds);

                osupdate.CommitChanges();

                WriteLog("[Error]", "Message: Pick List Posting :" + oTargetDoc + "-" + ex.Message);

                return -1;
            }
        }

        // Start ver 1.0.7
        public int PostCNCanceltoSAP(ARDownpaymentCancel oTargetDoc, IObjectSpaceProvider ObjectSpaceProvider, SAPCompany sap)
        {
            // return 0 = post nothing
            // return -1 = posting error
            // return 1 = posting successful
            try
            {
                if (!oTargetDoc.Sap)
                {
                    IObjectSpace fos = ObjectSpaceProvider.CreateObjectSpace();
                    Guid g;
                    // Create and display the value of two GUIDs.
                    g = Guid.NewGuid();

                    SAPbobsCOM.Documents oDoc = null;

                    oDoc = (SAPbobsCOM.Documents)sap.oCom.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oCreditNotes);

                    oDoc.CardCode = oTargetDoc.Customer.BPCode;
                    oDoc.CardName = oTargetDoc.CustomerName;
                    oDoc.DocDate = oTargetDoc.PostingDate;
                    oDoc.Comments = oTargetDoc.Remarks;
                    oDoc.UserFields.Fields.Item("U_PortalDocNum").Value = oTargetDoc.DocNum;
                    if (oTargetDoc.Reference != null)
                    {
                        oDoc.NumAtCard = oTargetDoc.Reference;
                    }
                    if (oTargetDoc.ContactPerson != null)
                    {
                        oDoc.SalesPersonCode = oTargetDoc.ContactPerson.SlpCode;
                    }

                    int cnt = 0;
                    foreach (ARDownpaymentCancelDetails dtl in oTargetDoc.ARDownpaymentCancelDetails)
                    {
                        cnt++;
                        if (cnt == 1)
                        {
                        }
                        else
                        {
                            oDoc.Lines.Add();
                            oDoc.Lines.SetCurrentLine(oDoc.Lines.Count - 1);
                        }

                        oDoc.Lines.ItemCode = dtl.ItemCode.ItemCode;
                        oDoc.Lines.ItemDescription = dtl.ItemDesc;
                        oDoc.Lines.Quantity = (double)dtl.Quantity;
                        oDoc.Lines.UnitPrice = (double)dtl.Price;
                        //oDoc.Lines.WithoutInventoryMovement = BoYesNoEnum.tYES;
                        if (dtl.Warehouse != null)
                        {
                            oDoc.Lines.WarehouseCode = dtl.Warehouse.WarehouseCode;
                        }
                        oDoc.Lines.UserFields.Fields.Item("U_PortalLineOid").Value = dtl.Oid.ToString();

                        if (dtl.Bin != null)
                        {
                            oDoc.Lines.BinAllocations.BinAbsEntry = dtl.Bin.AbsEntry;
                            oDoc.Lines.BinAllocations.Quantity = (double)dtl.Quantity;
                        }

                        if (dtl.BaseDoc != null)
                        {
                            oDoc.Lines.BaseType = 203;
                            oDoc.Lines.BaseEntry = int.Parse(dtl.BaseDoc);
                            oDoc.Lines.BaseLine = int.Parse(dtl.BaseId);
                        }
                    }

                    int rc = oDoc.Add();
                    if (rc != 0)
                    {
                        string temp = sap.oCom.GetLastErrorDescription();
                        if (sap.oCom.InTransaction)
                        {
                            sap.oCom.EndTransaction(BoWfTransOpt.wf_RollBack);
                        }

                        IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                        SalesRefundRequests obj = osupdate.GetObjectByKey<SalesRefundRequests>(oTargetDoc.Oid);

                        SalesRefundReqDocTrail ds = osupdate.CreateObject<SalesRefundReqDocTrail>();
                        ds.CreateUser = osupdate.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                        ds.CreateDate = DateTime.Now;
                        ds.DocStatus = DocStatus.PendPost;
                        ds.DocRemarks = "SAP Error:" + temp;
                        obj.SalesRefundReqDocTrail.Add(ds);

                        osupdate.CommitChanges();

                        WriteLog("[Error]", "Message: Sales Refund Posting :" + oTargetDoc + "-" + temp);

                        return -1;
                    }
                    return 1;
                }
                return 0;
            }
            catch (Exception ex)
            {
                IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                SalesRefundRequests obj = osupdate.GetObjectByKey<SalesRefundRequests>(oTargetDoc.Oid);

                SalesRefundReqDocTrail ds = osupdate.CreateObject<SalesRefundReqDocTrail>();
                ds.CreateUser = osupdate.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                ds.CreateDate = DateTime.Now;
                ds.DocStatus = DocStatus.PendPost;
                ds.DocRemarks = "SAP Error:" + ex.Message;
                obj.SalesRefundReqDocTrail.Add(ds);

                osupdate.CommitChanges();

                WriteLog("[Error]", "Message: Sales Refund Posting :" + oTargetDoc + "-" + ex.Message);

                return -1;
            }
        }

        public int PostCancelPaymenttoSAP(ARDownpaymentCancel doc, int oTargetDoc, IObjectSpaceProvider ObjectSpaceProvider, SAPCompany sap)
        {
            // return 0 = post nothing
            // return -1 = posting error
            // return 1 = posting successful
            try
            {
                Guid g;
                // Create and display the value of two GUIDs.
                g = Guid.NewGuid();

                SAPbobsCOM.Payments oDoc = null;

                oDoc = (SAPbobsCOM.Payments)sap.oCom.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oIncomingPayments);
                oDoc.GetByKey(oTargetDoc);

                if (oDoc.Cancelled == BoYesNoEnum.tNO)
                {
                    int rc = oDoc.Cancel();
                    if (rc != 0)
                    {
                        {
                            string temp = sap.oCom.GetLastErrorDescription();
                            if (sap.oCom.InTransaction)
                            {
                                sap.oCom.EndTransaction(BoWfTransOpt.wf_RollBack);
                            }

                            IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                            ARDownpaymentCancel obj = osupdate.GetObjectByKey<ARDownpaymentCancel>(doc.Oid);

                            ARDownpaymentCancellationDocTrail ds = osupdate.CreateObject<ARDownpaymentCancellationDocTrail>();
                            ds.CreateUser = osupdate.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                            ds.CreateDate = DateTime.Now;
                            ds.DocStatus = DocStatus.PendPost;
                            ds.DocRemarks = "SAP Error:" + temp;
                            obj.ARDownpaymentCancellationDocTrail.Add(ds);

                            osupdate.CommitChanges();

                            WriteLog("[Error]", "Message: AR Downpayment Cancel :" + oTargetDoc + "-" + temp);

                            return -1;
                        }
                    }
                    return 1;
                }
                return 2;
            }
            catch (Exception ex)
            {
                IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                ARDownpaymentCancel obj = osupdate.GetObjectByKey<ARDownpaymentCancel>(doc.Oid);

                ARDownpaymentCancellationDocTrail ds = osupdate.CreateObject<ARDownpaymentCancellationDocTrail>();
                ds.CreateUser = osupdate.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                ds.CreateDate = DateTime.Now;
                ds.DocStatus = DocStatus.PendPost;
                ds.DocRemarks = "SAP Error:" + ex.Message;
                obj.ARDownpaymentCancellationDocTrail.Add(ds);

                osupdate.CommitChanges();

                WriteLog("[Error]", "Message: AR Downpayment Cancel :" + oTargetDoc + "-" + ex.Message);

                return -1;
            }
        }
        // End ver 1.0.7

        // Start ver 1.0.9
        public int CancelSOtoSAP(SalesOrder oTargetDoc, int docentry, IObjectSpaceProvider ObjectSpaceProvider, SAPCompany sap)
        {
            // return 0 = post nothing
            // return -1 = posting error
            // return 1 = posting successful
            try
            {
                Guid g;
                // Create and display the value of two GUIDs.
                g = Guid.NewGuid();

                SAPbobsCOM.Documents oDoc = null;

                oDoc = (SAPbobsCOM.Documents)sap.oCom.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oOrders);
                oDoc.GetByKey(docentry);

                if (oDoc.Cancelled == BoYesNoEnum.tNO)
                {
                    int rc = oDoc.Cancel();
                    if (rc != 0)
                    {
                        {
                            string temp = sap.oCom.GetLastErrorDescription();
                            if (sap.oCom.InTransaction)
                            {
                                sap.oCom.EndTransaction(BoWfTransOpt.wf_RollBack);
                            }

                            IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                            SalesOrder obj = osupdate.GetObjectByKey<SalesOrder>(oTargetDoc.Oid);

                            SalesOrderDocStatus ds = osupdate.CreateObject<SalesOrderDocStatus>();
                            ds.CreateUser = osupdate.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                            ds.CreateDate = DateTime.Now;
                            ds.DocStatus = DocStatus.Open;
                            ds.DocRemarks = "SAP Error:" + temp;
                            obj.SalesOrderDocStatus.Add(ds);

                            osupdate.CommitChanges();

                            WriteLog("[Error]", "Message: Sales Order Cancel :" + oTargetDoc + "-" + temp);

                            return -1;
                        }
                    }
                    return 1;
                }
                return 2;
            }
            catch (Exception ex)
            {
                IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                SalesOrder obj = osupdate.GetObjectByKey<SalesOrder>(oTargetDoc.Oid);

                SalesOrderDocStatus ds = osupdate.CreateObject<SalesOrderDocStatus>();
                ds.CreateUser = osupdate.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                ds.CreateDate = DateTime.Now;
                ds.DocStatus = DocStatus.Open;
                ds.DocRemarks = "SAP Error:" + ex.Message;
                obj.SalesOrderDocStatus.Add(ds);

                osupdate.CommitChanges();

                WriteLog("[Error]", "Message: Sales Order Cancel :" + oTargetDoc + "-" + ex.Message);

                return -1;
            }
        }
        public int ClosedSOtoSAP(SalesOrder oTargetDoc, int docentry, IObjectSpaceProvider ObjectSpaceProvider, SAPCompany sap)
        {
            // return 0 = post nothing
            // return -1 = posting error
            // return 1 = posting successful
            try
            {
                Guid g;
                // Create and display the value of two GUIDs.
                g = Guid.NewGuid();

                SAPbobsCOM.Documents oDoc = null;

                oDoc = (SAPbobsCOM.Documents)sap.oCom.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oOrders);
                oDoc.GetByKey(docentry);

                if (oDoc.DocumentStatus != BoStatus.bost_Close)
                {
                    int rc = oDoc.Close();
                    if (rc != 0)
                    {
                        {
                            string temp = sap.oCom.GetLastErrorDescription();
                            if (sap.oCom.InTransaction)
                            {
                                sap.oCom.EndTransaction(BoWfTransOpt.wf_RollBack);
                            }

                            IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                            SalesOrder obj = osupdate.GetObjectByKey<SalesOrder>(oTargetDoc.Oid);

                            SalesOrderDocStatus ds = osupdate.CreateObject<SalesOrderDocStatus>();
                            ds.CreateUser = osupdate.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                            ds.CreateDate = DateTime.Now;
                            ds.DocStatus = DocStatus.Open;
                            ds.DocRemarks = "SAP Error:" + temp;
                            obj.SalesOrderDocStatus.Add(ds);

                            osupdate.CommitChanges();

                            WriteLog("[Error]", "Message: Sales Order Close :" + oTargetDoc + "-" + temp);

                            return -1;
                        }
                    }
                    return 1;
                }
                return 2;
            }
            catch (Exception ex)
            {
                IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                SalesOrder obj = osupdate.GetObjectByKey<SalesOrder>(oTargetDoc.Oid);

                SalesOrderDocStatus ds = osupdate.CreateObject<SalesOrderDocStatus>();
                ds.CreateUser = osupdate.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                ds.CreateDate = DateTime.Now;
                ds.DocStatus = DocStatus.Open;
                ds.DocRemarks = "SAP Error:" + ex.Message;
                obj.SalesOrderDocStatus.Add(ds);

                osupdate.CommitChanges();

                WriteLog("[Error]", "Message: Sales Order Close :" + oTargetDoc + "-" + ex.Message);

                return -1;
            }
        }
        // End ver 1.0.9

        // Start ver 1.0.8.1
        public int UpdNonPersistent(IObjectSpaceProvider ObjectSpaceProvider)
        {
            IObjectSpace lso = ObjectSpaceProvider.CreateObjectSpace();

            //// Sales Order Collection
            //IList<SalesOrderCollection> soclist = lso.GetObjects<SalesOrderCollection>
            //    (CriteriaOperator.Parse("IsNull(SONumber)"));

            //foreach (SalesOrderCollection dtlsoc in soclist)
            //{
            //    IObjectSpace socos = ObjectSpaceProvider.CreateObjectSpace();
            //    SalesOrderCollection socobj = socos.GetObjectByKey<SalesOrderCollection>(dtlsoc.Oid);

            //    string dupso = null;
            //    foreach (SalesOrderCollectionDetails dtl in socobj.SalesOrderCollectionDetails)
            //    {
            //        if (dupso != dtl.SalesOrder)
            //        {
            //            if (socobj.SONumber == null)
            //            {
            //                socobj.SONumber = dtl.SalesOrder;
            //            }
            //            else
            //            {
            //                socobj.SONumber = socobj.SONumber + ", " + dtl.SalesOrder;
            //            }

            //            dupso = dtl.SalesOrder;
            //        }
            //    }

            //    socos.CommitChanges();
            //}

            //// Pick List
            //IList<PickList> pllist = lso.GetObjects<PickList>
            //    (CriteriaOperator.Parse("IsNull(Customer)"));

            //foreach (PickList dtlpl in pllist)
            //{
            //    IObjectSpace plos = ObjectSpaceProvider.CreateObjectSpace();
            //    PickList plobj = plos.GetObjectByKey<PickList>(dtlpl.Oid);

            //    string dupso = null;
            //    foreach (PickListDetails dtl in plobj.PickListDetails)
            //    {
            //        if (plobj.Customer == null)
            //        {
            //            plobj.Customer = dtl.Customer.BPCode;
            //        }
            //        if (plobj.CustomerName == null)
            //        {
            //            plobj.CustomerName = dtl.Customer.BPName;
            //        }

            //        if (dupso != dtl.SOBaseDoc)
            //        {
            //            if (plobj.SONumber == null)
            //            {
            //                plobj.SONumber = dtl.SOBaseDoc;
            //            }
            //            else
            //            {
            //                plobj.SONumber = plobj.SONumber + ", " + dtl.SOBaseDoc;
            //            }

            //            dupso = dtl.SOBaseDoc;
            //        }
            //    }

            //    string sodeliverydate = null;
            //    if (plobj.PickListDetails.Count() > 0)
            //    {
            //        sodeliverydate = plobj.PickListDetails.Where(x => x.SOBaseDoc != null).OrderBy(c => c.SODeliveryDate).Min().SODeliveryDate.Date.ToString();
            //    }

            //    if (sodeliverydate != null)
            //    {
            //        plobj.SODeliveryDate =  sodeliverydate.Substring(0, 10);
            //    }

            //    if (plobj.PickListDetails.Count() > 0)
            //    {
            //        plobj.Priority = plobj.PickListDetails.Where(x => x.SOBaseDoc != null).OrderBy(c => c.Priority).Max().Priority;
            //    }

            //    plos.CommitChanges();
            //}

            //// Pack List
            //IList<PackList> packlist = lso.GetObjects<PackList>
            //    (CriteriaOperator.Parse("IsNull(SONumber)"));

            //foreach (PackList dtlpack in packlist)
            //{
            //    IObjectSpace packos = ObjectSpaceProvider.CreateObjectSpace();
            //    PackList packobj = packos.GetObjectByKey<PackList>(dtlpack.Oid);

            //    string duppl = null;
            //    string dupso = null;
            //    string dupcustomer = null;
            //    foreach (PackListDetails dtl in packobj.PackListDetails)
            //    {
            //        if (duppl != dtl.PickListNo)
            //        {
            //            PickList picklist = packos.FindObject<PickList>(CriteriaOperator.Parse("DocNum = ?", dtl.PickListNo));

            //            if (picklist != null)
            //            {
            //                foreach (PickListDetails dtl2 in picklist.PickListDetails)
            //                {
            //                    if (dupso != dtl2.SOBaseDoc)
            //                    {
            //                        if (packobj.SONumber == null)
            //                        {
            //                            packobj.SONumber = dtl2.SOBaseDoc;
            //                        }
            //                        else
            //                        {
            //                            packobj.SONumber = packobj.SONumber + ", " + dtl2.SOBaseDoc;
            //                        }

            //                        SalesOrder salesorder = packos.FindObject<SalesOrder>(CriteriaOperator.Parse("DocNum = ?", dtl2.SOBaseDoc));

            //                        if (salesorder != null)
            //                        {
            //                            if (packobj.SAPSONo == null)
            //                            {
            //                                packobj.SAPSONo = salesorder.SAPDocNum;
            //                            }
            //                            else
            //                            {
            //                                packobj.SAPSONo = packobj.SAPSONo + ", " + salesorder.SAPDocNum;
            //                            }
            //                        }

            //                        dupso = dtl2.SOBaseDoc;
            //                    }

            //                    if (dupcustomer != dtl2.Customer.BPName)
            //                    {
            //                        if (packobj.Customer == null)
            //                        {
            //                            packobj.Customer = dtl2.Customer.BPName;
            //                        }
            //                        else
            //                        {
            //                            packobj.Customer = packobj.Customer + ", " + dtl2.Customer.BPName;
            //                        }

            //                        dupcustomer = dtl2.Customer.BPName;
            //                    }
            //                }

            //                if (picklist != null)
            //                {
            //                    if (packobj.Priority == null)
            //                    {
            //                        packobj.Priority = picklist.PickListDetails.Where(x => x.SOBaseDoc != null).OrderBy(c => c.Priority).Max().Priority;
            //                    }
            //                }
            //            }

            //            if (packobj.PickListNo == null)
            //            {
            //                packobj.PickListNo = dtl.PickListNo;
            //            }
            //            else
            //            {
            //                packobj.PickListNo = packobj.PickListNo + ", " + dtl.PickListNo;
            //            }

            //            duppl = dtl.PickListNo;
            //        }
            //    }

            //    packos.CommitChanges();
            //}

            //// Load
            //IList<Load> loadlist = lso.GetObjects<Load>
            //    (CriteriaOperator.Parse("IsNull(PackListNo)"));

            //foreach (Load dtlload in loadlist)
            //{
            //    IObjectSpace loados = ObjectSpaceProvider.CreateObjectSpace();
            //    Load loadobj = loados.GetObjectByKey<Load>(dtlload.Oid);

            //    string duppack = null;
            //    foreach (LoadDetails dtl in loadobj.LoadDetails)
            //    {
            //        if (duppack != dtl.BaseDoc)
            //        {
            //            if (loadobj.PackListNo == null)
            //            {
            //                loadobj.PackListNo = dtl.BaseDoc;
            //            }
            //            else
            //            {
            //                loadobj.PackListNo = loadobj.PackListNo + ", " + dtl.BaseDoc;
            //            }

            //            duppack = dtl.BaseDoc;
            //        }

            //        PackList pack = loados.FindObject<PackList>(CriteriaOperator.Parse("DocNum = ?", dtl.PackList));

            //        if (pack != null)
            //        {
            //            if (loadobj.SONumber == null)
            //            {
            //                loadobj.SONumber = pack.SONumber;
            //            }

            //            if (loadobj.Priority == null)
            //            {
            //                loadobj.Priority = pack.Priority;
            //            }
            //        }
            //    }

            //    loados.CommitChanges();
            //}

            //// Delivery Orders
            //IList<DeliveryOrder> deliverylist = lso.GetObjects<DeliveryOrder>
            //    (CriteriaOperator.Parse("IsNull(SONo)"));

            //foreach (DeliveryOrder dtldelivery in deliverylist)
            //{
            //    IObjectSpace deliveryos = ObjectSpaceProvider.CreateObjectSpace();
            //    DeliveryOrder deliveryobj = deliveryos.GetObjectByKey<DeliveryOrder>(dtldelivery.Oid);

            //    string dupno = null;
            //    string dupso = null;
            //    foreach (DeliveryOrderDetails dtl in deliveryobj.DeliveryOrderDetails)
            //    {
            //        if (dupno != dtl.BaseDoc)
            //        {
            //            if (deliveryobj.LoadingNo == null)
            //            {
            //                deliveryobj.LoadingNo = dtl.BaseDoc;
            //            }
            //            else
            //            {
            //                deliveryobj.LoadingNo = deliveryobj.LoadingNo + ", " + dtl.BaseDoc;
            //            }

            //            dupno = dtl.BaseDoc;
            //        }

            //        if (dupso != dtl.SODocNum)
            //        {
            //            if (deliveryobj.SONo == null)
            //            {
            //                deliveryobj.SONo = dtl.SODocNum;
            //            }
            //            else
            //            {
            //                deliveryobj.SONo = deliveryobj.SONo + ", " + dtl.SODocNum;
            //            }

            //            dupso = dtl.SODocNum;
            //        }

            //        if (deliveryobj.Priority == null)
            //        {
            //            SalesOrder salesorder = deliveryos.FindObject<SalesOrder>(CriteriaOperator.Parse("DocNum = ?", dtl.SODocNum));

            //            if (salesorder != null)
            //            {
            //                deliveryobj.Priority = salesorder.Priority;
            //            }
            //        }
            //    }

            //    deliveryos.CommitChanges();
            //}

            //// ASN
            //IList<ASN> asnlist = lso.GetObjects<ASN>
            //    (CriteriaOperator.Parse("IsNull(PONo)"));

            //foreach (ASN dtlasn in asnlist)
            //{
            //    IObjectSpace asnos = ObjectSpaceProvider.CreateObjectSpace();
            //    ASN asnobj = asnos.GetObjectByKey<ASN>(dtlasn.Oid);

            //    string duppo = null;
            //    foreach (ASNDetails dtl in asnobj.ASNDetails)
            //    {
            //        if (duppo != dtl.PORefNo)
            //        {
            //            if (asnobj.PONo == null)
            //            {
            //                asnobj.PONo = dtl.PORefNo;
            //            }
            //            else
            //            {
            //                asnobj.PONo = asnobj.PONo + ", " + dtl.PORefNo;
            //            }

            //            duppo = dtl.PORefNo;
            //        }
            //    }

            //    asnos.CommitChanges();
            //}

            //// GRN
            //IList<GRN> grnlist = lso.GetObjects<GRN>
            //    (CriteriaOperator.Parse("IsNull(SAPPONo)"));

            //foreach (GRN dtlgrn in grnlist)
            //{
            //    IObjectSpace grnos = ObjectSpaceProvider.CreateObjectSpace();
            //    GRN grnobj = grnos.GetObjectByKey<GRN>(dtlgrn.Oid);

            //    // Start ver 1.0.8.1
            //    string duppo = null;
            //    string dupporef = null;
            //    string dupasn = null;
            //    // End ver 1.0.8.1
            //    foreach (GRNDetails dtl2 in grnobj.GRNDetails)
            //    {
            //        if (dtl2.PONo != null)
            //        {
            //            if (duppo != dtl2.PONo)
            //            {
            //                if (grnobj.SAPPONo == null)
            //                {
            //                    grnobj.SAPPONo = dtl2.PONo;
            //                }
            //                else
            //                {
            //                    grnobj.SAPPONo = grnobj.SAPPONo + ", " + dtl2.PONo;
            //                }

            //                duppo = dtl2.PONo;
            //            }
            //        }

            //        if (dtl2.PORefNo != null)
            //        {
            //            if (dupporef != dtl2.PORefNo)
            //            {
            //                if (grnobj.PortalPONo == null)
            //                {
            //                    grnobj.PortalPONo = dtl2.PORefNo;
            //                }
            //                else
            //                {
            //                    grnobj.PortalPONo = grnobj.PortalPONo + ", " + dtl2.PORefNo;
            //                }

            //                dupporef = dtl2.PORefNo;
            //            }
            //        }

            //        if (dtl2.ASNBaseDoc != null)
            //        {
            //            if (dupasn != dtl2.ASNBaseDoc)
            //            {
            //                if (grnobj.ASNNo == null)
            //                {
            //                    grnobj.ASNNo = dtl2.ASNBaseDoc;
            //                }
            //                else
            //                {
            //                    grnobj.ASNNo = grnobj.ASNNo + ", " + dtl2.ASNBaseDoc;
            //                }

            //                dupasn = dtl2.ASNBaseDoc;
            //            }
            //        }
            //    }

            //    grnos.CommitChanges();
            //}

            // Load
            IList<Load> loadlist = lso.GetObjects<Load>
                (CriteriaOperator.Parse("IsNull(Warehouse)"));

            foreach (Load dtlload in loadlist)
            {
                IObjectSpace loados = ObjectSpaceProvider.CreateObjectSpace();
                Load loadobj = loados.GetObjectByKey<Load>(dtlload.Oid);

                foreach (LoadDetails dtl in loadobj.LoadDetails)
                {

                    PackList pack = loados.FindObject<PackList>(CriteriaOperator.Parse("DocNum = ?", dtl.PackList));

                    if (pack != null)
                    {
                        if (loadobj.Warehouse == null)
                        {
                            loadobj.Warehouse = loadobj.Session.GetObjectByKey<vwWarehouse>(pack.Warehouse.WarehouseCode);
                        }
                    }
                }

                loados.CommitChanges();
            }

            return 0;
        }
        // End ver 1.0.8.1

        // Start ver 1.0.12
        public int PostSCOuttoSAP(StockCountConfirm oTargetDoc, IObjectSpaceProvider ObjectSpaceProvider, SAPCompany sap)
        {
            // return 0 = post nothing
            // return -1 = posting error
            // return 1 = posting successful
            try
            {
                if (!oTargetDoc.Sap)
                {
                    IObjectSpace fos = ObjectSpaceProvider.CreateObjectSpace();
                    bool found = false;

                    DateTime postdate = DateTime.Now;

                    foreach (StockCountConfirmDetails dtl in oTargetDoc.StockCountConfirmDetails)
                    {
                        found = true;
                    }
                    if (!found) return 0;

                    Guid g;
                    // Create and display the value of two GUIDs.
                    g = Guid.NewGuid();

                    int sapempid = 0;

                    SAPbobsCOM.Documents oDoc = null;

                    oDoc = (SAPbobsCOM.Documents)sap.oCom.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInventoryGenExit);

                    oDoc.DocType = BoDocumentTypes.dDocument_Items;
                    oDoc.DocDate = postdate;

                    if (sapempid > 0)
                        oDoc.DocumentsOwner = sapempid;
                    oDoc.DocDate = oTargetDoc.StockCountDate;
                    oDoc.TaxDate = DateTime.Today;
                    oDoc.Comments = oTargetDoc.Remarks;
                    oDoc.UserFields.Fields.Item("U_PortalDocNum").Value = oTargetDoc.DocNum;

                    // Start ver 1.0.13
                    //string getitem = "SELECT T1.ItemCode, T1.Quantity -  ISNULL(SUM(T2.Qty), 0), T1.OID From StockCountConfirm T0 " +
                    //   "INNER JOIN StockCountConfirmDetails T1 on T0.OID = T1.StockCountConfirm AND T1.GCRecord is null " +
                    //   "LEFT JOIN " +
                    //   "( " +
                    //   "SELECT " +
                    //   "T2.ItemCode, T7.ItemName, T6.BinCode, " +
                    //   "SUM(CASE WHEN T2.ActionType in ('1','19') THEN T4.Quantity " +
                    //   "WHEN T2.ActionType in ('2', '20') THEN - T4.Quantity ELSE 0 END) as [Qty], T2.DocDate " +
                    //   "FROM [" + ConfigurationManager.AppSettings["CompanyDB"].ToString() + "]..OILM T2 " +
                    //   "INNER JOIN [" + ConfigurationManager.AppSettings["CompanyDB"].ToString() + "]..OBTL T4 ON(T2.MessageID = T4.MessageID) " +
                    //   "INNER JOIN [" + ConfigurationManager.AppSettings["CompanyDB"].ToString() + "]..OBIN T6 ON(T4.BinAbs = T6.AbsEntry) " +
                    //   "INNER JOIN [" + ConfigurationManager.AppSettings["CompanyDB"].ToString() + "]..OITM T7 ON(T2.ItemCode = T7.ItemCode) " +
                    //   "GROUP BY T2.ItemCode,T7.ItemName, T6.BinCode, T2.DocDate)  " +
                    //   "T2 on T1.ItemCode = T2.ItemCode COLLATE DATABASE_DEFAULT and T1.Bin = T2.BinCode COLLATE DATABASE_DEFAULT " +
                    //   "and T2.DocDate <= T0.StockCountDate " +
                    //   "LEFT JOIN [" + ConfigurationManager.AppSettings["CompanyDB"].ToString() + "]..OITW T3 with(nolock) on " +
                    //   "T3.ItemCode = T1.ItemCode COLLATE DATABASE_DEFAULT " +
                    //   "AND T3.WhsCode = T0.Warehouse COLLATE DATABASE_DEFAULT " +
                    //   "WHERE T0.DocNum = '" + oTargetDoc.DocNum + "' " +
                    //   "GROUP BY T1.ItemCode, T1.Quantity, T1.OID";
                    //if (conn.State == ConnectionState.Open)
                    //{
                    //    conn.Close();
                    //}
                    //conn.Open();
                    //SqlCommand cmd = new SqlCommand(getitem, conn);
                    //SqlDataReader reader = cmd.ExecuteReader();
                    // End ver 1.0.13

                    int cnt = 0;

                    // Start ver 1.0.13
                    //while (reader.Read())
                    //{
                    //    if (reader.GetDecimal(1) < 0)
                    //    {
                    //        foreach (StockCountConfirmDetails dtl in oTargetDoc.StockCountConfirmDetails)
                    //        {
                    //            if (dtl.Oid == reader.GetInt32(2))
                    //            {
                    //                cnt++;
                    //                if (cnt == 1)
                    //                {
                    //                }
                    //                else
                    //                {
                    //                    //oDoc.Lines.BatchNumbers.Add();
                    //                    //oDoc.Lines.BatchNumbers.SetCurrentLine(oDoc.Lines.Count - 1);
                    //                    oDoc.Lines.Add();
                    //                    oDoc.Lines.SetCurrentLine(oDoc.Lines.Count - 1);
                    //                }

                    //                oDoc.Lines.WarehouseCode = dtl.Warehouse.WarehouseCode;
                    //                oDoc.Lines.ItemCode = dtl.ItemCode.ItemCode;

                    //                vwStockCountGL glacc = fos.FindObject<vwStockCountGL>(CriteriaOperator.Parse("ItmsGrpNam = ?", dtl.ItemCode.Model));
                    //                if (glacc != null)
                    //                {
                    //                    oDoc.Lines.AccountCode = glacc.GLAccount;
                    //                }

                    //                oDoc.Lines.ItemDescription = dtl.ItemDesc;
                    //                oDoc.Lines.Quantity = (double)(reader.GetDecimal(1) - reader.GetDecimal(1) - reader.GetDecimal(1));
                    //                oDoc.Lines.UserFields.Fields.Item("U_PortalLineOid").Value = dtl.Oid.ToString();

                    //                if (dtl.Bin != null)
                    //                {
                    //                    oDoc.Lines.BinAllocations.BinAbsEntry = dtl.Bin.AbsEntry;
                    //                    oDoc.Lines.BinAllocations.Quantity = (double)(reader.GetDecimal(1) - reader.GetDecimal(1) - reader.GetDecimal(1));
                    //                }

                    //                break;
                    //            }
                    //        }
                    //    }
                    //}
                    //conn.Close();
                    // End ver 1.0.13

                    // Start ver 1.0.13
                    foreach (StockCountConfirmDetails dtl in oTargetDoc.StockCountConfirmDetails)
                    {
                        if (dtl.Quantity < 0)
                        {
                            cnt++;
                            if (cnt == 1)
                            {
                            }
                            else
                            {
                                //oDoc.Lines.BatchNumbers.Add();
                                //oDoc.Lines.BatchNumbers.SetCurrentLine(oDoc.Lines.Count - 1);
                                oDoc.Lines.Add();
                                oDoc.Lines.SetCurrentLine(oDoc.Lines.Count - 1);
                            }

                            oDoc.Lines.WarehouseCode = dtl.Warehouse.WarehouseCode;
                            oDoc.Lines.ItemCode = dtl.ItemCode.ItemCode;

                            vwStockCountGL glacc = fos.FindObject<vwStockCountGL>(CriteriaOperator.Parse("ItmsGrpNam = ?", dtl.ItemCode.Model));
                            if (glacc != null)
                            {
                                oDoc.Lines.AccountCode = glacc.GLAccount;
                            }

                            oDoc.Lines.ItemDescription = dtl.ItemDesc;
                            oDoc.Lines.Quantity = (double)(dtl.Quantity - dtl.Quantity - dtl.Quantity);
                            oDoc.Lines.UserFields.Fields.Item("U_PortalLineOid").Value = dtl.Oid.ToString();

                            if (dtl.Bin != null)
                            {
                                oDoc.Lines.BinAllocations.BinAbsEntry = dtl.Bin.AbsEntry;
                                oDoc.Lines.BinAllocations.Quantity = (double)(dtl.Quantity - dtl.Quantity - dtl.Quantity);
                            }
                        }
                    }
                    // End ver 1.0.13

                    if (cnt <= 0)
                    {
                        return 2;
                    }

                    int rc = oDoc.Add();
                    if (rc != 0)
                    {
                        string temp = sap.oCom.GetLastErrorDescription();
                        if (sap.oCom.InTransaction)
                        {
                            sap.oCom.EndTransaction(BoWfTransOpt.wf_RollBack);
                        }

                        IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                        StockCountConfirm obj = osupdate.GetObjectByKey<StockCountConfirm>(oTargetDoc.Oid);

                        StockCountConfirmDocTrail ds = osupdate.CreateObject<StockCountConfirmDocTrail>();
                        ds.CreateUser = osupdate.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                        ds.CreateDate = DateTime.Now;
                        ds.DocStatus = DocStatus.Submitted;
                        ds.DocRemarks = "SAP Error:" + temp;
                        obj.StockCountConfirmDocTrail.Add(ds);

                        osupdate.CommitChanges();

                        WriteLog("[Error]", "Message: Stock Count(Issue) Posting :" + oTargetDoc + "-" + temp);

                        return -1;
                    }
                    return 1;
                }
                return 0;
            }
            catch (Exception ex)
            {
                IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                StockCountConfirm obj = osupdate.GetObjectByKey<StockCountConfirm>(oTargetDoc.Oid);

                StockCountConfirmDocTrail ds = osupdate.CreateObject<StockCountConfirmDocTrail>();
                ds.CreateUser = osupdate.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                ds.CreateDate = DateTime.Now;
                ds.DocStatus = DocStatus.Submitted;
                ds.DocRemarks = "SAP Error:" + ex.Message;
                obj.StockCountConfirmDocTrail.Add(ds);

                osupdate.CommitChanges();

                WriteLog("[Error]", "Message: Stock Count(Issue) Posting :" + oTargetDoc + "-" + ex.Message);

                return -1;
            }
        }

        public int PostSCINtoSAP(StockCountConfirm oTargetDoc, IObjectSpaceProvider ObjectSpaceProvider, SAPCompany sap)
        {
            // return 0 = post nothing
            // return -1 = posting error
            // return 1 = posting successful
            try
            {
                if (!oTargetDoc.Sap)
                {
                    IObjectSpace fos = ObjectSpaceProvider.CreateObjectSpace();
                    bool found = false;
                    DateTime postdate = DateTime.Now;

                    foreach (StockCountConfirmDetails dtl in oTargetDoc.StockCountConfirmDetails)
                    {
                        found = true;
                    }
                    if (!found) return 0;

                    Guid g;
                    // Create and display the value of two GUIDs.
                    g = Guid.NewGuid();

                    int sapempid = 0;

                    SAPbobsCOM.Documents oDoc = null;

                    oDoc = (SAPbobsCOM.Documents)sap.oCom.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oInventoryGenEntry);

                    oDoc.DocType = BoDocumentTypes.dDocument_Items;
                    oDoc.DocDate = postdate;

                    if (sapempid > 0)
                        oDoc.DocumentsOwner = sapempid;
                    oDoc.DocDate = oTargetDoc.StockCountDate;
                    oDoc.TaxDate = DateTime.Now; ;
                    oDoc.Comments = oTargetDoc.Remarks;
                    oDoc.UserFields.Fields.Item("U_PortalDocNum").Value = oTargetDoc.DocNum;

                    // Start ver 1.0.13
                    //string getitem = "SELECT T1.ItemCode, T1.Quantity -  ISNULL(SUM(T2.Qty), 0), T1.OID, ISNULL(T3.AvgPrice, 0) From StockCountConfirm T0 " +
                    //    "INNER JOIN StockCountConfirmDetails T1 on T0.OID = T1.StockCountConfirm AND T1.GCRecord is null " +
                    //    "LEFT JOIN " +
                    //    "( " +
                    //    "SELECT " +
                    //    "T2.ItemCode, T7.ItemName, T6.BinCode, " +
                    //    "SUM(CASE WHEN T2.ActionType in ('1','19') THEN T4.Quantity " +
                    //    "WHEN T2.ActionType in ('2', '20') THEN - T4.Quantity ELSE 0 END) as [Qty], T2.DocDate " +
                    //    "FROM [" + ConfigurationManager.AppSettings["CompanyDB"].ToString() + "]..OILM T2 " +
                    //    "INNER JOIN [" + ConfigurationManager.AppSettings["CompanyDB"].ToString() + "]..OBTL T4 ON(T2.MessageID = T4.MessageID) " +
                    //    "INNER JOIN [" + ConfigurationManager.AppSettings["CompanyDB"].ToString() + "]..OBIN T6 ON(T4.BinAbs = T6.AbsEntry) " +
                    //    "INNER JOIN [" + ConfigurationManager.AppSettings["CompanyDB"].ToString() + "]..OITM T7 ON(T2.ItemCode = T7.ItemCode) " +
                    //    "GROUP BY T2.ItemCode,T7.ItemName, T6.BinCode, T2.DocDate)  " +
                    //    "T2 on T1.ItemCode = T2.ItemCode COLLATE DATABASE_DEFAULT and T1.Bin = T2.BinCode COLLATE DATABASE_DEFAULT " +
                    //    "and T2.DocDate <= T0.StockCountDate " +
                    //    "LEFT JOIN [" + ConfigurationManager.AppSettings["CompanyDB"].ToString() + "]..OITW T3 with(nolock) on " +
                    //    "T3.ItemCode = T1.ItemCode COLLATE DATABASE_DEFAULT " +
                    //    "AND T3.WhsCode = T0.Warehouse COLLATE DATABASE_DEFAULT " +
                    //    "WHERE T0.DocNum = '" + oTargetDoc.DocNum + "' " +
                    //    "GROUP BY T1.ItemCode, T1.Quantity, T1.OID, T3.AvgPrice";
                    //if (conn.State == ConnectionState.Open)
                    //{
                    //    conn.Close();
                    //}
                    //conn.Open();
                    //SqlCommand cmd = new SqlCommand(getitem, conn);
                    //SqlDataReader reader = cmd.ExecuteReader();
                    // End ver 1.0.13

                    int cnt = 0;

                    // Start ver 1.0.13
                    //while (reader.Read())
                    //{
                    //    if (reader.GetDecimal(1) > 0)
                    //    {
                    //        foreach (StockCountConfirmDetails dtl in oTargetDoc.StockCountConfirmDetails)
                    //        {
                    //            if (dtl.Oid == reader.GetInt32(2))
                    //            {
                    //                cnt++;
                    //                if (cnt == 1)
                    //                {
                    //                }
                    //                else
                    //                {
                    //                    //oDoc.Lines.BatchNumbers.Add();
                    //                    //oDoc.Lines.BatchNumbers.SetCurrentLine(oDoc.Lines.Count - 1);
                    //                    oDoc.Lines.Add();
                    //                    oDoc.Lines.SetCurrentLine(oDoc.Lines.Count - 1);
                    //                }

                    //                oDoc.Lines.WarehouseCode = dtl.Warehouse.WarehouseCode;

                    //                oDoc.Lines.ItemCode = dtl.ItemCode.ItemCode;
                    //                oDoc.Lines.ItemDescription = dtl.ItemDesc;
                    //                oDoc.Lines.Quantity = (double)reader.GetDecimal(1);

                    //                vwStockCountGL glacc = fos.FindObject<vwStockCountGL>(CriteriaOperator.Parse("ItmsGrpNam = ?", dtl.ItemCode.Model));
                    //                if (glacc != null)
                    //                {
                    //                    oDoc.Lines.AccountCode = glacc.GLAccount;
                    //                }

                    //                if (reader.GetDecimal(3) <= 0)
                    //                {
                    //                    oDoc.Lines.UnitPrice = 0.01;
                    //                }
                    //                else
                    //                {
                    //                    oDoc.Lines.UnitPrice = (double)reader.GetDecimal(3);
                    //                }
                    //                oDoc.Lines.UserFields.Fields.Item("U_PortalLineOid").Value = dtl.Oid.ToString();

                    //                if (dtl.Bin != null)
                    //                {
                    //                    oDoc.Lines.BinAllocations.BinAbsEntry = dtl.Bin.AbsEntry;
                    //                    oDoc.Lines.BinAllocations.Quantity = (double)reader.GetDecimal(1);
                    //                }

                    //                break;
                    //            }
                    //        }
                    //    }
                    //}

                    //conn.Close();
                    // End ver 1.0.13

                    // Start ver 1.0.13
                    foreach (StockCountConfirmDetails dtl in oTargetDoc.StockCountConfirmDetails)
                    {
                        if (dtl.Quantity > 0)
                        {
                            cnt++;
                            if (cnt == 1)
                            {
                            }
                            else
                            {
                                //oDoc.Lines.BatchNumbers.Add();
                                //oDoc.Lines.BatchNumbers.SetCurrentLine(oDoc.Lines.Count - 1);
                                oDoc.Lines.Add();
                                oDoc.Lines.SetCurrentLine(oDoc.Lines.Count - 1);
                            }

                            oDoc.Lines.WarehouseCode = dtl.Warehouse.WarehouseCode;

                            oDoc.Lines.ItemCode = dtl.ItemCode.ItemCode;
                            oDoc.Lines.ItemDescription = dtl.ItemDesc;
                            oDoc.Lines.Quantity = (double)dtl.Quantity;

                            vwStockCountGL glacc = fos.FindObject<vwStockCountGL>(CriteriaOperator.Parse("ItmsGrpNam = ?", dtl.ItemCode.Model));
                            if (glacc != null)
                            {
                                oDoc.Lines.AccountCode = glacc.GLAccount;
                            }

                            string getavg = "SELECT T1.ItemCode, T1.OID, ISNULL(T3.AvgPrice, 0) From StockCountConfirm T0 " +
                                "INNER JOIN StockCountConfirmDetails T1 on T0.OID = T1.StockCountConfirm AND T1.GCRecord is null " +
                                "LEFT JOIN [STL_SAP_LIVE]..OITW T3 with (nolock) on T3.ItemCode = T1.ItemCode COLLATE DATABASE_DEFAULT " +
                                "AND T3.WhsCode = T0.Warehouse COLLATE DATABASE_DEFAULT " +
                                "WHERE T0.DocNum = '" + oTargetDoc.DocNum + "' " +
                                "GROUP BY T1.ItemCode, T1.OID, T3.AvgPrice";
                            if (conn.State == ConnectionState.Open)
                            {
                                conn.Close();
                            }
                            conn.Open();
                            SqlCommand cmd = new SqlCommand(getavg, conn);
                            SqlDataReader reader = cmd.ExecuteReader();
                            while (reader.Read())
                            {
                                if (reader.GetInt32(1) == dtl.Oid)
                                {
                                    if (reader.GetDecimal(2) <= 0)
                                    {
                                        oDoc.Lines.UnitPrice = 0.01;
                                    }
                                    else
                                    {
                                        oDoc.Lines.UnitPrice = (double)reader.GetDecimal(2);
                                    }
                                }
                            }

                            oDoc.Lines.UserFields.Fields.Item("U_PortalLineOid").Value = dtl.Oid.ToString();

                            if (dtl.Bin != null)
                            {
                                oDoc.Lines.BinAllocations.BinAbsEntry = dtl.Bin.AbsEntry;
                                oDoc.Lines.BinAllocations.Quantity = (double)dtl.Quantity;
                            }
                        }
                    }
                    // End ver 1.0.13

                    if (cnt <= 0)
                    {
                        return 2;
                    }

                    int rc = oDoc.Add();
                    if (rc != 0)
                    {
                        string temp = sap.oCom.GetLastErrorDescription();
                        if (sap.oCom.InTransaction)
                        {
                            sap.oCom.EndTransaction(BoWfTransOpt.wf_RollBack);
                        }

                        IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                        StockCountConfirm obj = osupdate.GetObjectByKey<StockCountConfirm>(oTargetDoc.Oid);

                        StockCountConfirmDocTrail ds = osupdate.CreateObject<StockCountConfirmDocTrail>();
                        ds.CreateUser = osupdate.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                        ds.CreateDate = DateTime.Now;
                        ds.DocStatus = DocStatus.Submitted;
                        ds.DocRemarks = "SAP Error:" + temp;
                        obj.StockCountConfirmDocTrail.Add(ds);

                        osupdate.CommitChanges();

                        WriteLog("[Error]", "Message: Stock Count(Receipt) Posting :" + oTargetDoc + "-" + temp);

                        return -1;
                    }
                    return 1;
                }
                return 0;
            }
            catch (Exception ex)
            {
                IObjectSpace osupdate = ObjectSpaceProvider.CreateObjectSpace();
                StockCountConfirm obj = osupdate.GetObjectByKey<StockCountConfirm>(oTargetDoc.Oid);

                StockCountConfirmDocTrail ds = osupdate.CreateObject<StockCountConfirmDocTrail>();
                ds.CreateUser = osupdate.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                ds.CreateDate = DateTime.Now;
                ds.DocStatus = DocStatus.Submitted;
                ds.DocRemarks = "SAP Error:" + ex.Message;
                obj.StockCountConfirmDocTrail.Add(ds);

                osupdate.CommitChanges();

                WriteLog("[Error]", "Message: Stock Count(Receipt) Posting :" + oTargetDoc + "-" + ex.Message);

                return -1;
            }
        }
        // End ver 1.0.12
    }
}
