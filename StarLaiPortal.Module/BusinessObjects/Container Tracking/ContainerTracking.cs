using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using DevExpress.XtraPrinting.Native;
using DevExpress.XtraRichEdit.Fields;
using StarLaiPortal.Module.BusinessObjects.Inquiry_View;
using StarLaiPortal.Module.BusinessObjects.Sales_Quotation;
using StarLaiPortal.Module.BusinessObjects.Setup;
using StarLaiPortal.Module.BusinessObjects.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.Remoting.Contexts;
using System.Text;

namespace StarLaiPortal.Module.BusinessObjects.Container_Tracking
{
    [XafDisplayName("Container Tracking")]
    [NavigationItem("Container Tracking")]
    [DefaultProperty("DocNum")]
    [Appearance("HideDelete", AppearanceItemType.Action, "True", TargetItems = "Delete", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    public class ContainerTracking : XPObject
    { 
        public ContainerTracking(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();

            ApplicationUser user = (ApplicationUser)SecuritySystem.CurrentUser;
            if (user != null)
            {
                CreateUser = Session.GetObjectByKey<ApplicationUser>(user.Oid);
            }
            else
            {
                CreateUser = Session.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
            }
            CreateDate = DateTime.Now;

            DocType = DocTypeList.CT;
            Status = DocStatus.New;
        }

        private ApplicationUser _CreateUser;
        [XafDisplayName("Create User")]
        //[ModelDefault("EditMask", "(000)-00"), VisibleInListView(false)]
        [Appearance("CreateUser", Enabled = false)]
        [Index(300), VisibleInListView(false), VisibleInDetailView(false), VisibleInLookupListView(false)]
        public ApplicationUser CreateUser
        {
            get { return _CreateUser; }
            set
            {
                SetPropertyValue("CreateUser", ref _CreateUser, value);
            }
        }

        private DateTime? _CreateDate;
        [XafDisplayName("Create Date")]
        [ModelDefault("DisplayFormat", "{0: dd/MM/yyyy hh:mm tt}")]
        [Index(301), VisibleInListView(false), VisibleInDetailView(false), VisibleInLookupListView(false)]
        [Appearance("CreateDate", Enabled = false)]
        public DateTime? CreateDate
        {
            get { return _CreateDate; }
            set
            {
                SetPropertyValue("CreateDate", ref _CreateDate, value);
            }
        }

        private ApplicationUser _UpdateUser;
        [XafDisplayName("Update User"), ToolTip("Enter Text")]
        [Appearance("UpdateUser", Enabled = false)]
        [Index(302), VisibleInListView(false), VisibleInDetailView(false), VisibleInLookupListView(false)]
        public ApplicationUser UpdateUser
        {
            get { return _UpdateUser; }
            set
            {
                SetPropertyValue("UpdateUser", ref _UpdateUser, value);
            }
        }

        private DateTime? _UpdateDate;
        [Index(303), VisibleInListView(false), VisibleInDetailView(false), VisibleInLookupListView(false)]
        [Appearance("UpdateDate", Enabled = false)]
        public DateTime? UpdateDate
        {
            get { return _UpdateDate; }
            set
            {
                SetPropertyValue("UpdateDate", ref _UpdateDate, value);
            }
        }

        private DocTypeList _DocType;
        [Appearance("DocType", Enabled = false, Criteria = "not IsNew")]
        [Index(304), VisibleInListView(false), VisibleInDetailView(false), VisibleInLookupListView(false)]
        public virtual DocTypeList DocType
        {
            get { return _DocType; }
            set
            {
                SetPropertyValue("DocType", ref _DocType, value);
            }
        }

        private string _DocNum;
        [XafDisplayName("No")]
        [Appearance("DocNum", Enabled = false)]
        [Index(3), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(false)]
        public string DocNum
        {
            get { return _DocNum; }
            set
            {
                SetPropertyValue("DocNum", ref _DocNum, value);
            }
        }

        private vwBusniessPartner _Supplier;
        [XafDisplayName("Supplier Code")]
        [NoForeignKey]
        [ImmediatePostData]
        [LookupEditorMode(LookupEditorMode.AllItems)]
        [DataSourceCriteria("ValidFor = 'Y' and CardType = 'S'")]
        [RuleRequiredField(DefaultContexts.Save)]
        [Index(5), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(false)]
        public vwBusniessPartner Supplier
        {
            get { return _Supplier; }
            set
            {
                SetPropertyValue("Supplier", ref _Supplier, value);
                if (!IsLoading && value != null)
                {
                    SupplierName = Supplier.BPName;
                }
                else if (!IsLoading && value == null)
                {
                    SupplierName = null;
                }
            }
        }

        private string _SupplierName;
        [ImmediatePostData]
        [XafDisplayName("Supplier Name")]
        [RuleRequiredField(DefaultContexts.Save)]
        [Index(8), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(false)]
        public string SupplierName
        {
            get { return _SupplierName; }
            set
            {
                SetPropertyValue("SupplierName", ref _SupplierName, value);
            }
        }

        private DateTime _ESTDeparture;
        [XafDisplayName("EST. Departure From Country of Origin")]
        [Index(10), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(false)]
        public DateTime ESTDeparture
        {
            get { return _ESTDeparture; }
            set
            {
                SetPropertyValue("ESTDeparture", ref _ESTDeparture, value);
            }
        }

        private DateTime _ESTArrival;
        [XafDisplayName("EST. Time Arrival at Local Port")]
        [Index(13), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(false)]
        public DateTime ESTArrival
        {
            get { return _ESTArrival; }
            set
            {
                SetPropertyValue("ESTArrival", ref _ESTArrival, value);
            }
        }

        private string _ContainerNo;
        [XafDisplayName("Container No")]
        [Index(15), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(false)]
        public string ContainerNo
        {
            get { return _ContainerNo; }
            set
            {
                SetPropertyValue("ContainerNo", ref _ContainerNo, value);
            }
        }

        private string _ShipmentInvoiceNo;
        [XafDisplayName("Shipment Invoice No")]
        [Index(18), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(false)]
        public string ShipmentInvoiceNo
        {
            get { return _ShipmentInvoiceNo; }
            set
            {
                SetPropertyValue("ShipmentInvoiceNo", ref _ShipmentInvoiceNo, value);
            }
        }

        private string _Forwarded;
        [XafDisplayName("Forwarded")]
        [Index(20), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public string Forwarded
        {
            get { return _Forwarded; }
            set
            {
                SetPropertyValue("Forwarded", ref _Forwarded, value);
            }
        }

        private string _ContainerType;
        [NoForeignKey]
        [XafDisplayName("Container Type")]
        [Index(23), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public string ContainerType
        {
            get { return _ContainerType; }
            set
            {
                SetPropertyValue("ContainerType", ref _ContainerType, value);
            }
        }

        private DocStatus _Status;
        [XafDisplayName("Status")]
        [Appearance("Status", Enabled = false)]
        [Index(25), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(false)]
        public DocStatus Status
        {
            get { return _Status; }
            set
            {
                SetPropertyValue("Status", ref _Status, value);
            }
        }

        private string _Remarks;
        [XafDisplayName("Remarks")]
        [Index(28), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public string Remarks
        {
            get { return _Remarks; }
            set
            {
                SetPropertyValue("Remarks", ref _Remarks, value);
            }
        }

        // Purchase Dept
        private string _PurBillOfLandingType;
        [NoForeignKey]
        [ImmediatePostData]
        [XafDisplayName("Pur Bill Of Landing Type")]
        [Index(30), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        [Appearance("PurBillOfLandingType", Enabled = false, Criteria = "PurchaseDeptStatus = 1")]
        public string PurBillOfLandingType
        {
            get { return _PurBillOfLandingType; }
            set
            {
                SetPropertyValue("PurBillOfLandingType", ref _PurBillOfLandingType, value);
                if (!IsLoading && value != null)
                {
                    AccBillOfLandingType = PurBillOfLandingType;
                }
                else if (!IsLoading && value == null)
                {
                    AccBillOfLandingType = null;
                }
            }
        }

        private bool _PurCertificateOfOrigin;
        [ImmediatePostData]
        [XafDisplayName("Pur Certificate Of Origin")]
        [Index(33), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        [Appearance("PurCertificateOfOrigin", Enabled = false, Criteria = "PurchaseDeptStatus = 1")]
        public bool PurCertificateOfOrigin
        {
            get { return _PurCertificateOfOrigin; }
            set
            {
                SetPropertyValue("PurCertificateOfOrigin", ref _PurCertificateOfOrigin, value);
                if (!IsLoading)
                {
                    AccCertificateOfOrigin = PurCertificateOfOrigin;
                }
            }
        }

        private DateTime _SoftcopyBLRec;
        [ImmediatePostData]
        [XafDisplayName("Softcopy BL Rec. From Supplier")]
        [Index(35), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        [ModelDefault("DisplayFormat", "{0: dd/MM/yyyy hh:mm tt}")]
        [ModelDefault("EditMask", "MM/dd/yyyy hh:mm")]
        [Appearance("SoftcopyBLRec", Enabled = false, Criteria = "PurchaseDeptStatus = 1")]
        public DateTime SoftcopyBLRec
        {
            get { return _SoftcopyBLRec; }
            set
            {
                SetPropertyValue("SoftcopyBLRec", ref _SoftcopyBLRec, value);
                if (!IsLoading)
                {
                    SoftcopyBLRecFrmPur = SoftcopyBLRec;
                }
            }
        }

        private DateTime _SoftcopyPLRec;
        [ImmediatePostData]
        [XafDisplayName("Softcopy Packing List Rec. From Supplier")]
        [Index(38), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        [ModelDefault("DisplayFormat", "{0: dd/MM/yyyy hh:mm tt}")]
        [ModelDefault("EditMask", "MM/dd/yyyy hh:mm")]
        [Appearance("SoftcopyPLRec", Enabled = false, Criteria = "PurchaseDeptStatus = 1")]
        public DateTime SoftcopyPLRec
        {
            get { return _SoftcopyPLRec; }
            set
            {
                SetPropertyValue("SoftcopyPLRec", ref _SoftcopyPLRec, value);
                if (!IsLoading)
                {
                    SoftcopyPLRecFrmPur = SoftcopyPLRec;
                }
            }
        }

        private DateTime _SoftcopyInvoice;
        [ImmediatePostData]
        [XafDisplayName("Softcopy Invoice Rec. From Supplier")]
        [Index(40), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        [ModelDefault("DisplayFormat", "{0: dd/MM/yyyy hh:mm tt}")]
        [ModelDefault("EditMask", "MM/dd/yyyy hh:mm")]
        [Appearance("SoftcopyInvoice", Enabled = false, Criteria = "PurchaseDeptStatus = 1")]
        public DateTime SoftcopyInvoice
        {
            get { return _SoftcopyInvoice; }
            set
            {
                SetPropertyValue("SoftcopyInvoice", ref _SoftcopyInvoice, value);
                if (!IsLoading)
                {
                    SoftcopyInvoiceFrmPur = SoftcopyInvoice;
                }
            }
        }

        private vwPaymentTerm _PurCreditorPaymentTerm;
        [NoForeignKey]
        [ImmediatePostData]
        [XafDisplayName("Pur Creditor Payment Term")]
        [Index(43), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        [Appearance("PurCreditorPaymentTerm", Enabled = false, Criteria = "PurchaseDeptStatus = 1")]
        public vwPaymentTerm PurCreditorPaymentTerm
        {
            get { return _PurCreditorPaymentTerm; }
            set
            {
                SetPropertyValue("PurCreditorPaymentTerm", ref _PurCreditorPaymentTerm, value);
                if (!IsLoading && value != null)
                {
                    AccCreditorPaymentTerm = Session.FindObject<vwPaymentTerm>(CriteriaOperator.Parse("GroupNum = ?", PurCreditorPaymentTerm.GroupNum));
                }
                else if (!IsLoading && value == null)
                {
                    AccCreditorPaymentTerm = null;
                }
            }
        }

        private string _PurTradeTerms;
        [NoForeignKey]
        [ImmediatePostData]
        [XafDisplayName("Pur Trade Terms")]
        [Index(45), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        [Appearance("PurTradeTerms", Enabled = false, Criteria = "PurchaseDeptStatus = 1")]
        public string PurTradeTerms
        {
            get { return _PurTradeTerms; }
            set
            {
                SetPropertyValue("PurTradeTerms", ref _PurTradeTerms, value);
                if (!IsLoading && value != null)
                {
                    AccTradeTerms = PurTradeTerms;
                }
                else if (!IsLoading && value == null)
                {
                    AccTradeTerms = null;
                }
            }
        }

        private ContainerStatus _PurchaseDeptStatus;
        [ImmediatePostData]
        [XafDisplayName("Purchase Dept. Status")]
        [Index(48), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public ContainerStatus PurchaseDeptStatus
        {
            get { return _PurchaseDeptStatus; }
            set
            {
                SetPropertyValue("PurchaseDeptStatus", ref _PurchaseDeptStatus, value);
            }
        }

        // Account Dept
        private string _AccBillOfLandingType;
        [NoForeignKey]
        [XafDisplayName("Acc Bill Of Landing Type")]
        [Index(50), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        [Appearance("AccBillOfLandingType", Enabled = false, Criteria = "AccDeptStatus = 1")]
        public string AccBillOfLandingType
        {
            get { return _AccBillOfLandingType; }
            set
            {
                SetPropertyValue("AccBillOfLandingType", ref _AccBillOfLandingType, value);
            }
        }

        private bool _AccCertificateOfOrigin;
        [XafDisplayName("Acc Certificate Of Origin")]
        [Index(53), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        [Appearance("AccCertificateOfOrigin", Enabled = false, Criteria = "AccDeptStatus = 1")]
        public bool AccCertificateOfOrigin
        {
            get { return _AccCertificateOfOrigin; }
            set
            {
                SetPropertyValue("AccCertificateOfOrigin", ref _AccCertificateOfOrigin, value);
            }
        }

        private DateTime _SoftcopyBLRecFrmPur;
        [ImmediatePostData]
        [XafDisplayName("Softcopy BL Rec. From Pur. Dept.")]
        [Index(55), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        [ModelDefault("DisplayFormat", "{0: dd/MM/yyyy hh:mm tt}")]
        [ModelDefault("EditMask", "MM/dd/yyyy hh:mm")]
        [Appearance("SoftcopyBLRecFrmPur", Enabled = false, Criteria = "AccDeptStatus = 1")]
        public DateTime SoftcopyBLRecFrmPur
        {
            get { return _SoftcopyBLRecFrmPur; }
            set
            {
                SetPropertyValue("SoftcopyBLRecFrmPur", ref _SoftcopyBLRecFrmPur, value);
                if (!IsLoading)
                {
                    if (SoftcopyBLRecFrmPur.Date.ToString("MM/dd/yyyy") != "01/01/0001" && BLRecvDate.Date.ToString("MM/dd/yyyy") != "01/01/0001")
                    {
                        PendingDocDays = (SoftcopyBLRecFrmPur.Date - BLRecvDate.Date).Days;
                    }
                    else
                    {
                        PendingDocDays = 0;
                    }
                }
            }
        }

        private DateTime _SoftcopyPLRecFrmPur;
        [XafDisplayName("Softcopy Packing List Rec. From Pur. Dept.")]
        [Index(58), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        [ModelDefault("DisplayFormat", "{0: dd/MM/yyyy hh:mm tt}")]
        [ModelDefault("EditMask", "MM/dd/yyyy hh:mm")]
        [Appearance("SoftcopyPLRecFrmPur", Enabled = false, Criteria = "AccDeptStatus = 1")]
        public DateTime SoftcopyPLRecFrmPur
        {
            get { return _SoftcopyPLRecFrmPur; }
            set
            {
                SetPropertyValue("SoftcopyPLRecFrmPur", ref _SoftcopyPLRecFrmPur, value);
            }
        }

        private DateTime _SoftcopyInvoiceFrmPur;
        [XafDisplayName("Softcopy Invoice Rec. From Pur. Dept.")]
        [Index(60), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        [ModelDefault("DisplayFormat", "{0: dd/MM/yyyy hh:mm tt}")]
        [ModelDefault("EditMask", "MM/dd/yyyy hh:mm")]
        [Appearance("SoftcopyInvoiceFrmPur", Enabled = false, Criteria = "AccDeptStatus = 1")]
        public DateTime SoftcopyInvoiceFrmPur
        {
            get { return _SoftcopyInvoiceFrmPur; }
            set
            {
                SetPropertyValue("SoftcopyInvoiceFrmPur", ref _SoftcopyInvoiceFrmPur, value);
            }
        }

        private vwPaymentTerm _AccCreditorPaymentTerm;
        [NoForeignKey]
        [XafDisplayName("Acc Creditor Payment Term")]
        [Index(63), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        [Appearance("AccCreditorPaymentTerm", Enabled = false, Criteria = "AccDeptStatus = 1")]
        public vwPaymentTerm AccCreditorPaymentTerm
        {
            get { return _AccCreditorPaymentTerm; }
            set
            {
                SetPropertyValue("AccCreditorPaymentTerm", ref _AccCreditorPaymentTerm, value);
            }
        }

        private string _AccTradeTerms;
        [NoForeignKey]
        [XafDisplayName("Acc Trade Terms")]
        [Index(65), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        [Appearance("AccTradeTerms", Enabled = false, Criteria = "AccDeptStatus = 1")]
        public string AccTradeTerms
        {
            get { return _AccTradeTerms; }
            set
            {
                SetPropertyValue("AccTradeTerms", ref _AccTradeTerms, value);
            }
        }

        private ContainerStatus _AccDeptStatus;
        [ImmediatePostData]
        [XafDisplayName("Account Dept. Status")]
        [Index(68), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public ContainerStatus AccDeptStatus
        {
            get { return _AccDeptStatus; }
            set
            {
                SetPropertyValue("AccDeptStatus", ref _AccDeptStatus, value);
            }
        }

        private string _ShippingLine;
        [XafDisplayName("Shipping Line")]
        [Index(70), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        [Appearance("ShippingLine", Enabled = false, Criteria = "AccDeptStatus = 1")]
        public string ShippingLine
        {
            get { return _ShippingLine; }
            set
            {
                SetPropertyValue("ShippingLine", ref _ShippingLine, value);
            }
        }

        private DateTime _AccStakeOnDateTime;
        [ImmediatePostData]
        [XafDisplayName("Acc Stake On Date Time")]
        [Index(73), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        [ModelDefault("DisplayFormat", "{0: dd/MM/yyyy hh:mm tt}")]
        [ModelDefault("EditMask", "MM/dd/yyyy hh:mm")]
        [Appearance("AccStakeOnDateTime", Enabled = false, Criteria = "AccDeptStatus = 1")]
        public DateTime AccStakeOnDateTime
        {
            get { return _AccStakeOnDateTime; }
            set
            {
                SetPropertyValue("AccStakeOnDateTime", ref _AccStakeOnDateTime, value);
                if (!IsLoading)
                {
                    WhsStakeOnDateTime = AccStakeOnDateTime;
                }
            }
        }

        private int _AccStorageFreeDays;
        [ImmediatePostData]
        [XafDisplayName("Acc Storage Free Days")]
        [Index(75), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        [Appearance("AccStorageFreeDays", Enabled = false, Criteria = "AccDeptStatus = 1")]
        public int AccStorageFreeDays
        {
            get { return _AccStorageFreeDays; }
            set
            {
                SetPropertyValue("AccStorageFreeDays", ref _AccStorageFreeDays, value);
                if (!IsLoading)
                {
                    WhsStorageFreeDays = AccStorageFreeDays;
                }
            }
        }

        private int _AccDemmurrageFreeDays;
        [ImmediatePostData]
        [XafDisplayName("Acc Demmurrage Free Days")]
        [Index(78), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        [Appearance("AccDemmurrageFreeDays", Enabled = false, Criteria = "AccDeptStatus = 1")]
        public int AccDemmurrageFreeDays
        {
            get { return _AccDemmurrageFreeDays; }
            set
            {
                SetPropertyValue("AccDemmurrageFreeDays", ref _AccDemmurrageFreeDays, value);
                if (!IsLoading)
                {
                    WhsDemmurrageFreeDays = AccDemmurrageFreeDays;
                }
            }
        }

        private int _AccDetentionFreeDays;
        [ImmediatePostData]
        [XafDisplayName("Acc Detention Free Days")]
        [Index(80), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        [Appearance("AccDetentionFreeDays", Enabled = false, Criteria = "AccDeptStatus = 1")]
        public int AccDetentionFreeDays
        {
            get { return _AccDetentionFreeDays; }
            set
            {
                SetPropertyValue("AccDetentionFreeDays", ref _AccDetentionFreeDays, value);
                if (!IsLoading)
                {
                    WhsDetentionFreeDays = AccDetentionFreeDays;
                }
            }
        }

        private string _AccRemarks;
        [XafDisplayName("Acc Remarks")]
        [Index(83), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        [Appearance("AccRemarks", Enabled = false, Criteria = "AccDeptStatus = 1")]
        public string AccRemarks
        {
            get { return _AccRemarks; }
            set
            {
                SetPropertyValue("AccRemarks", ref _AccRemarks, value);
            }
        }

        private DateTime _BLRecvDate;
        [ImmediatePostData]
        [XafDisplayName("BL Recv. Date")]
        [Index(85), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        [Appearance("BLRecvDate", Enabled = false, Criteria = "AccDeptStatus = 1")]
        public DateTime BLRecvDate
        {
            get { return _BLRecvDate; }
            set
            {
                SetPropertyValue("BLRecvDate", ref _BLRecvDate, value);
                if (!IsLoading)
                {
                    if (SoftcopyBLRecFrmPur.Date.ToString("MM/dd/yyyy") != "01/01/0001" && BLRecvDate.Date.ToString("MM/dd/yyyy") != "01/01/0001")
                    {
                        PendingDocDays = (SoftcopyBLRecFrmPur.Date - BLRecvDate.Date).Days;
                    }
                    else
                    {
                        PendingDocDays = 0;
                    }
                }
            }
        }

        private int _PendingDocDays;
        [XafDisplayName("Pending Doc. Days")]
        [Index(88), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        [Appearance("PendingDocDays", Enabled = false)]
        public int PendingDocDays
        {
            get { return _PendingDocDays; }
            set
            {
                SetPropertyValue("PendingDocDays", ref _PendingDocDays, value);
            }
        }

        private DateTime _InvSettDate;
        [XafDisplayName("Invoice Sett. Date")]
        [Index(90), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        [Appearance("InvSettDate", Enabled = false, Criteria = "AccDeptStatus = 1")]
        public DateTime InvSettDate
        {
            get { return _InvSettDate; }
            set
            {
                SetPropertyValue("InvSettDate", ref _InvSettDate, value);
            }
        }

        private DateTime _ForwardedSubmission;
        [XafDisplayName("Forwarded Submission DateTime")]
        [Index(93), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        [ModelDefault("DisplayFormat", "{0: dd/MM/yyyy hh:mm tt}")]
        [ModelDefault("EditMask", "MM/dd/yyyy hh:mm")]
        [Appearance("ForwardedSubmission", Enabled = false, Criteria = "AccDeptStatus = 1")]
        public DateTime ForwardedSubmission
        {
            get { return _ForwardedSubmission; }
            set
            {
                SetPropertyValue("ForwardedSubmission", ref _ForwardedSubmission, value);
            }
        }

        private DateTime _DutyDraftFrmForwarded;
        [XafDisplayName("Duty Draft From Forwarded")]
        [Index(95), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        [ModelDefault("DisplayFormat", "{0: dd/MM/yyyy hh:mm tt}")]
        [ModelDefault("EditMask", "MM/dd/yyyy hh:mm")]
        [Appearance("DutyDraftFrmForwarded", Enabled = false, Criteria = "AccDeptStatus = 1")]
        public DateTime DutyDraftFrmForwarded
        {
            get { return _DutyDraftFrmForwarded; }
            set
            {
                SetPropertyValue("DutyDraftFrmForwarded", ref _DutyDraftFrmForwarded, value);
            }
        }

        private DateTime _DutySettDate;
        [XafDisplayName("Duty Settlement Date")]
        [Index(98), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        [ModelDefault("DisplayFormat", "{0: dd/MM/yyyy hh:mm tt}")]
        [ModelDefault("EditMask", "MM/dd/yyyy hh:mm")]
        [Appearance("DutySettDate", Enabled = false, Criteria = "AccDeptStatus = 1")]
        public DateTime DutySettDate
        {
            get { return _DutySettDate; }
            set
            {
                SetPropertyValue("DutySettDate", ref _DutySettDate, value);
            }
        }

        private DateTime _ArrivePortDate;
        [XafDisplayName("Arrive Port Date")]
        [Index(100), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        [Appearance("ArrivePortDate", Enabled = false, Criteria = "AccDeptStatus = 1")]
        public DateTime ArrivePortDate
        {
            get { return _ArrivePortDate; }
            set
            {
                SetPropertyValue("ArrivePortDate", ref _ArrivePortDate, value);
            }
        }

        private DateTime _RecvGatePassDate;
        [XafDisplayName("Receive Gate Pass Date")]
        [Index(103), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        [Appearance("RecvGatePassDate", Enabled = false, Criteria = "AccDeptStatus = 1")]
        public DateTime RecvGatePassDate
        {
            get { return _RecvGatePassDate; }
            set
            {
                SetPropertyValue("RecvGatePassDate", ref _RecvGatePassDate, value);
            }
        }

        // Warehouse Dept
        private DateTime _WhsStakeOnDateTime;
        [ImmediatePostData]
        [XafDisplayName("Whs Stake On Date Time")]
        [Index(105), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        [ModelDefault("DisplayFormat", "{0: dd/MM/yyyy hh:mm tt}")]
        [ModelDefault("EditMask", "MM/dd/yyyy hh:mm")]
        [Appearance("WhsStakeOnDateTime", Enabled = false, Criteria = "WhsDeptStatus = 1")]
        public DateTime WhsStakeOnDateTime
        {
            get { return _WhsStakeOnDateTime; }
            set
            {
                SetPropertyValue("WhsStakeOnDateTime", ref _WhsStakeOnDateTime, value);
                if (!IsLoading)
                {
                    StorageFreeDue = WhsStakeOnDateTime.AddDays(WhsStorageFreeDays);
                    DemmurrageFreeDue = WhsStakeOnDateTime.AddDays(WhsDemmurrageFreeDays);
                }
            }
        }

        private int _WhsStorageFreeDays;
        [ImmediatePostData]
        [XafDisplayName("Whs Storage Free Days")]
        [Index(108), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        [Appearance("WhsStorageFreeDays", Enabled = false, Criteria = "WhsDeptStatus = 1")]
        public int WhsStorageFreeDays
        {
            get { return _WhsStorageFreeDays; }
            set
            {
                SetPropertyValue("WhsStorageFreeDays", ref _WhsStorageFreeDays, value);
                if (!IsLoading)
                {
                    StorageFreeDue = WhsStakeOnDateTime.AddDays(WhsStorageFreeDays);
                }
            }
        }

        private int _WhsDemmurrageFreeDays;
        [ImmediatePostData]
        [XafDisplayName("Whs Demmurrage Free Days")]
        [Index(110), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        [Appearance("WhsDemmurrageFreeDays", Enabled = false, Criteria = "WhsDeptStatus = 1")]
        public int WhsDemmurrageFreeDays
        {
            get { return _WhsDemmurrageFreeDays; }
            set
            {
                SetPropertyValue("WhsDemmurrageFreeDays", ref _WhsDemmurrageFreeDays, value);
                if (!IsLoading)
                {
                    DemmurrageFreeDue = WhsStakeOnDateTime.AddDays(WhsDemmurrageFreeDays);
                    if (ActualPullOutDateTime.Date.ToString("MM/dd/yyyy") != "01/01/0001" && ActualReturnBack.Date.ToString("MM/dd/yyyy") != "01/01/0001")
                    {
                        DetentionFreeDelay = WhsDemmurrageFreeDays - ((ActualPullOutDateTime - ActualReturnBack).Days);
                    }
                    else
                    {
                        DetentionFreeDelay = WhsDemmurrageFreeDays - 0;
                    }
                }
            }
        }

        private int _WhsDetentionFreeDays;
        [XafDisplayName("Whs Detention Free Days")]
        [Index(113), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        [Appearance("WhsDetentionFreeDays", Enabled = false, Criteria = "WhsDeptStatus = 1")]
        public int WhsDetentionFreeDays
        {
            get { return _WhsDetentionFreeDays; }
            set
            {
                SetPropertyValue("WhsDetentionFreeDays", ref _WhsDetentionFreeDays, value);
            }
        }

        private DateTime _ReqPullOutDateTime;
        [ImmediatePostData]
        [XafDisplayName("Request Pull Out From Port")]
        [Index(115), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        [ModelDefault("DisplayFormat", "{0: dd/MM/yyyy hh:mm tt}")]
        [ModelDefault("EditMask", "MM/dd/yyyy hh:mm")]
        [Appearance("ReqPullOutDateTime", Enabled = false, Criteria = "WhsDeptStatus = 1")]
        public DateTime ReqPullOutDateTime
        {
            get { return _ReqPullOutDateTime; }
            set
            {
                SetPropertyValue("ReqPullOutDateTime", ref _ReqPullOutDateTime, value);
                if (!IsLoading)
                {
                    if (ActualPullOutDateTime.Date.ToString("MM/dd/yyyy") != "01/01/0001" && ReqPullOutDateTime.Date.ToString("MM/dd/yyyy") != "01/01/0001")
                    {
                        ForwardedFrmPort = (ActualPullOutDateTime - ReqPullOutDateTime).Days;
                    }
                    else
                    {
                        ForwardedFrmPort = 0;
                    }
                }
            }
        }

        private DateTime _ActualPullOutDateTime;
        [ImmediatePostData]
        [XafDisplayName("Actual Pull Out From Port/Arrive Whs")]
        [Index(118), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        [ModelDefault("DisplayFormat", "{0: dd/MM/yyyy hh:mm tt}")]
        [ModelDefault("EditMask", "MM/dd/yyyy hh:mm")]
        [Appearance("ActualPullOutDateTime", Enabled = false, Criteria = "WhsDeptStatus = 1")]
        public DateTime ActualPullOutDateTime
        {
            get { return _ActualPullOutDateTime; }
            set
            {
                SetPropertyValue("ActualPullOutDateTime", ref _ActualPullOutDateTime, value);
                if (!IsLoading)
                {
                    if (ActualPullOutDateTime.Date.ToString("MM/dd/yyyy") != "01/01/0001" && ActualReturnBack.Date.ToString("MM/dd/yyyy") != "01/01/0001")
                    {
                        DetentionFreeDelay = WhsDemmurrageFreeDays - ((ActualPullOutDateTime - ActualReturnBack).Days);
                    }
                    else
                    {
                        DetentionFreeDelay = WhsDemmurrageFreeDays - 0;
                    }

                    if (ActualPullOutDateTime.Date.ToString("MM/dd/yyyy") != "01/01/0001" && ReqPullOutDateTime.Date.ToString("MM/dd/yyyy") != "01/01/0001")
                    {
                        ForwardedFrmPort = (ActualPullOutDateTime - ReqPullOutDateTime).Days;
                    }
                    else
                    {
                        ForwardedFrmPort = 0;
                    }

                    if (ReqReturnBack.Date.ToString("MM/dd/yyyy") != "01/01/0001" && ActualPullOutDateTime.Date.ToString("MM/dd/yyyy") != "01/01/0001")
                    {
                        WhsUnload = (ReqReturnBack - ActualPullOutDateTime).Days;
                    }
                    else
                    {
                        WhsUnload = 0;
                    }
                }
            }
        }

        private DateTime _ReqReturnBack;
        [ImmediatePostData]
        [XafDisplayName("Request Return Back to Port")]
        [Index(120), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        [ModelDefault("DisplayFormat", "{0: dd/MM/yyyy hh:mm tt}")]
        [ModelDefault("EditMask", "MM/dd/yyyy hh:mm")]
        [Appearance("ReqReturnBack", Enabled = false, Criteria = "WhsDeptStatus = 1")]
        public DateTime ReqReturnBack
        {
            get { return _ReqReturnBack; }
            set
            {
                SetPropertyValue("ReqReturnBack", ref _ReqReturnBack, value);
                if (!IsLoading)
                {
                    if (ReqReturnBack.Date.ToString("MM/dd/yyyy") != "01/01/0001" && ActualPullOutDateTime.Date.ToString("MM/dd/yyyy") != "01/01/0001")
                    {
                        WhsUnload = (ReqReturnBack - ActualPullOutDateTime).Days;
                    }
                    else
                    {
                        WhsUnload = 0;
                    }

                    if (ActualReturnBack.Date.ToString("MM/dd/yyyy") != "01/01/0001" && ReqReturnBack.Date.ToString("MM/dd/yyyy") != "01/01/0001")
                    {
                        ForwardedToPort = (ActualReturnBack - ReqReturnBack).Days;
                    }
                    else
                    {
                        ForwardedToPort = 0;
                    }

                    if (GRPOReturnBack.Date.ToString("MM/dd/yyyy") != "01/01/0001" && ReqReturnBack.Date.ToString("MM/dd/yyyy") != "01/01/0001")
                    {
                        WhsProcessing = (GRPOReturnBack - ReqReturnBack).Days;
                    }
                    else
                    {
                        WhsProcessing = 0;
                    }
                }
            }
        }

        private DateTime _ActualReturnBack;
        [ImmediatePostData]
        [XafDisplayName("Actual Return Back to Port")]
        [Index(123), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        [ModelDefault("DisplayFormat", "{0: dd/MM/yyyy hh:mm tt}")]
        [ModelDefault("EditMask", "MM/dd/yyyy hh:mm")]
        [Appearance("ActualReturnBack", Enabled = false, Criteria = "WhsDeptStatus = 1")]
        public DateTime ActualReturnBack
        {
            get { return _ActualReturnBack; }
            set
            {
                SetPropertyValue("ActualReturnBack", ref _ActualReturnBack, value);
                if (!IsLoading)
                {
                    if (ActualPullOutDateTime.Date.ToString("MM/dd/yyyy") != "01/01/0001" && ActualReturnBack.Date.ToString("MM/dd/yyyy") != "01/01/0001")
                    {
                        DetentionFreeDelay = WhsDemmurrageFreeDays - ((ActualPullOutDateTime - ActualReturnBack).Days);
                    }
                    else
                    {
                        DetentionFreeDelay = WhsDemmurrageFreeDays - 0;
                    }

                    if (ActualReturnBack.Date.ToString("MM/dd/yyyy") != "01/01/0001" && ReqReturnBack.Date.ToString("MM/dd/yyyy") != "01/01/0001")
                    {
                        ForwardedToPort = (ActualReturnBack - ReqReturnBack).Days;
                    }
                    else
                    {
                        ForwardedToPort = 0;
                    }
                }
            }
        }

        private DateTime _GRPOReturnBack;
        [ImmediatePostData]
        [XafDisplayName("GRPO Completion Date")]
        [Index(125), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        [Appearance("GRPOReturnBack", Enabled = false, Criteria = "WhsDeptStatus = 1")]
        public DateTime GRPOReturnBack
        {
            get { return _GRPOReturnBack; }
            set
            {
                SetPropertyValue("GRPOReturnBack", ref _GRPOReturnBack, value);
                if (!IsLoading)
                {
                    if (GRPOReturnBack.Date.ToString("MM/dd/yyyy") != "01/01/0001" && ReqReturnBack.Date.ToString("MM/dd/yyyy") != "01/01/0001")
                    {
                        WhsProcessing = (GRPOReturnBack - ReqReturnBack).Days;
                    }
                    else
                    {
                        WhsProcessing = 0;
                    }
                }
            }
        }

        private string _WhsRemarks;
        [XafDisplayName("Whs Remarks")]
        [Index(130), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        [Appearance("WhsRemarks", Enabled = false, Criteria = "WhsDeptStatus = 1")]
        public string WhsRemarks
        {
            get { return _WhsRemarks; }
            set
            {
                SetPropertyValue("WhsRemarks", ref _WhsRemarks, value);
            }
        }

        private ContainerStatus _WhsDeptStatus;
        [ImmediatePostData]
        [XafDisplayName("Whs Dept. Status")]
        [Index(135), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public ContainerStatus WhsDeptStatus
        {
            get { return _WhsDeptStatus; }
            set
            {
                SetPropertyValue("WhsDeptStatus", ref _WhsDeptStatus, value);
            }
        }

        private DateTime _StorageFreeDue;
        [XafDisplayName("Storage Free Due Date Time")]
        [Index(140), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        [ModelDefault("DisplayFormat", "{0: dd/MM/yyyy hh:mm tt}")]
        [ModelDefault("EditMask", "MM/dd/yyyy hh:mm")]
        [Appearance("StorageFreeDue", Enabled = false)]
        public DateTime StorageFreeDue
        {
            get { return _StorageFreeDue; }
            set
            {
                SetPropertyValue("StorageFreeDue", ref _StorageFreeDue, value);
            }
        }

        private DateTime _DemmurrageFreeDue;
        [XafDisplayName("Demmurrage Free Due Date Time")]
        [Index(143), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        [ModelDefault("DisplayFormat", "{0: dd/MM/yyyy hh:mm tt}")]
        [ModelDefault("EditMask", "MM/dd/yyyy hh:mm")]
        [Appearance("DemmurrageFreeDue", Enabled = false)]
        public DateTime DemmurrageFreeDue
        {
            get { return _DemmurrageFreeDue; }
            set
            {
                SetPropertyValue("DemmurrageFreeDue", ref _DemmurrageFreeDue, value);
            }
        }

        private vwWarehouse _Warehouse;
        [NoForeignKey]
        [XafDisplayName("Warehouse")]
        [Index(145), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        [Appearance("Warehouse", Enabled = false, Criteria = "WhsDeptStatus = 1")]
        public vwWarehouse Warehouse
        {
            get { return _Warehouse; }
            set
            {
                SetPropertyValue("Warehouse", ref _Warehouse, value);
            }
        }

        private int _DetentionFreeDelay;
        [XafDisplayName("Detention Free Delay Days")]
        [Index(148), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        [Appearance("DetentionFreeDelay", Enabled = false)]
        public int DetentionFreeDelay
        {
            get { return _DetentionFreeDelay; }
            set
            {
                SetPropertyValue("DetentionFreeDelay", ref _DetentionFreeDelay, value);
            }
        }

        private int _ForwardedFrmPort;
        [XafDisplayName("Forwarded From Port Duration")]
        [Index(150), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        [Appearance("ForwardedFrmPort", Enabled = false)]
        public int ForwardedFrmPort
        {
            get { return _ForwardedFrmPort; }
            set
            {
                SetPropertyValue("ForwardedFrmPort", ref _ForwardedFrmPort, value);
            }
        }

        private int _WhsUnload;
        [XafDisplayName("Whs Unload Duration")]
        [Index(153), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        [Appearance("WhsUnload", Enabled = false)]
        public int WhsUnload
        {
            get { return _WhsUnload; }
            set
            {
                SetPropertyValue("WhsUnload", ref _WhsUnload, value);
            }
        }

        private int _ForwardedToPort;
        [XafDisplayName("Forwarded To Port Duration")]
        [Index(155), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        [Appearance("ForwardedToPort", Enabled = false)]
        public int ForwardedToPort
        {
            get { return _ForwardedToPort; }
            set
            {
                SetPropertyValue("ForwardedToPort", ref _ForwardedToPort, value);
            }
        }

        private int _WhsProcessing;
        [XafDisplayName("Whs Processing Duration")]
        [Index(158), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        [Appearance("WhsProcessing", Enabled = false)]
        public int WhsProcessing
        {
            get { return _WhsProcessing; }
            set
            {
                SetPropertyValue("WhsProcessing", ref _WhsProcessing, value);
            }
        }

        protected override void OnSaving()
        {
            base.OnSaving();
            if (!(Session is NestedUnitOfWork)
                && (Session.DataLayer != null)
                    && (Session.ObjectLayer is SimpleObjectLayer)
                        )
            {
                ApplicationUser user = (ApplicationUser)SecuritySystem.CurrentUser;
                if (user != null)
                {
                    UpdateUser = Session.GetObjectByKey<ApplicationUser>(user.Oid);
                }
                else
                {
                    UpdateUser = Session.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                }
                UpdateDate = DateTime.Now;
            }
        }
    }
}