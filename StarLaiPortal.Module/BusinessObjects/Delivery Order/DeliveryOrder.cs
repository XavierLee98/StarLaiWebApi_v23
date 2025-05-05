using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using StarLaiPortal.Module.BusinessObjects.Sales_Order;
using StarLaiPortal.Module.BusinessObjects.Setup;
using StarLaiPortal.Module.BusinessObjects.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

// 2023-04-09 - fix speed issue ver 1.0.8.1
// 2023-09-25 - add warehouse field ver 1.0.10
// 2024-05-16 - enhance speed - ver 1.0.15
// 2024-06-12 - e-invoice - ver 1.0.18
// 2024-10-09 - add PrintBy - ver 1.0.21

namespace StarLaiPortal.Module.BusinessObjects.Delivery_Order
{
    [DefaultClassOptions]
    [XafDisplayName("Delivery Order")]
    [NavigationItem("Delivery Order")]
    [DefaultProperty("DocNum")]
    [Appearance("HideNew", AppearanceItemType.Action, "True", TargetItems = "New", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideDelete", AppearanceItemType.Action, "True", TargetItems = "Delete", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideEdit", AppearanceItemType.Action, "True", TargetItems = "SwitchToEditMode; Edit", Criteria = "not (Status in (0))", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSubmit", AppearanceItemType.Action, "True", TargetItems = "SubmitDO", Criteria = "not (Status in (0))", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideCancel", AppearanceItemType.Action, "True", TargetItems = "CancelDO", Criteria = "not (Status in (0))", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    // Start ver 1.0.15
    [Appearance("HideFullTextSearch", AppearanceItemType.Action, "True", TargetItems = "FullTextSearch", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "DeliveryOrder_ListView_ByDate")]
    // End ver 1.0.15

    public class DeliveryOrder : XPObject
    { // Inherit from a different class to provide a custom primary key, concurrency and deletion behavior, etc. (https://documentation.devexpress.com/eXpressAppFramework/CustomDocument113146.aspx).
        // Use CodeRush to create XPO classes and properties with a few keystrokes.
        // https://docs.devexpress.com/CodeRushForRoslyn/118557
        public DeliveryOrder(Session session)
            : base(session)
        {
        }
        public override void AfterConstruction()
        {
            base.AfterConstruction();
            // Place your initialization code here (https://documentation.devexpress.com/eXpressAppFramework/CustomDocument112834.aspx).
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
            DocDate = DateTime.Now;
            PostingDate = DateTime.Now;

            Status = DocStatus.Draft;
            DocType = DocTypeList.DO;
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
        //[ModelDefault("EditMask", "(000)-00"), VisibleInListView(false)]
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
        [XafDisplayName("No.")]
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

        private vwBusniessPartner _Customer;
        [XafDisplayName("Customer")]
        [NoForeignKey]
        [ImmediatePostData]
        // Start ver 1.0.15
        //[LookupEditorMode(LookupEditorMode.AllItems)]
        // End ver 1.0.15
        [DataSourceCriteria("ValidFor = 'Y' and CardType = 'C'")]
        [Appearance("Customer", Enabled = false, Criteria = "not IsNew")]
        [Index(5), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(false)]
        public vwBusniessPartner Customer
        {
            get { return _Customer; }
            set
            {
                SetPropertyValue("Customer", ref _Customer, value);
                if (!IsLoading && value != null)
                {
                    CustomerName = Customer.BPName;
                    CustomerGroup = Customer.GroupName;
                }
                else if (!IsLoading && value == null)
                {
                    CustomerName = null;
                    CustomerGroup = null;
                }
            }
        }

        private string _CustomerName;
        [XafDisplayName("Customer Name")]
        [Appearance("CustomerName", Enabled = false)]
        [Index(8), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(false)]
        public string CustomerName
        {
            get { return _CustomerName; }
            set
            {
                SetPropertyValue("CustomerName", ref _CustomerName, value);
            }
        }

        // Start ver 1.0.10
        private vwWarehouse _Warehouse;
        [XafDisplayName("Warehouse")]
        [NoForeignKey]
        [ImmediatePostData]
        // Start ver 1.0.15
        //[LookupEditorMode(LookupEditorMode.AllItems)]
        // End ver 1.0.15
        [DataSourceCriteria("Inactive = 'N'")]
        [Index(9), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(false)]
        public vwWarehouse Warehouse
        {
            get { return _Warehouse; }
            set
            {
                SetPropertyValue("Warehouse", ref _Warehouse, value);
            }
        }
        // End ver 1.0.10

        private DateTime _DocDate;
        [XafDisplayName("Date")]
        [Index(10), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(false)]
        public DateTime DocDate
        {
            get { return _DocDate; }
            set
            {
                SetPropertyValue("_DocDate", ref _DocDate, value);
            }
        }

        private DateTime _PostingDate;
        [XafDisplayName("Posting Date")]
        [Index(13), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(false)]
        public DateTime PostingDate
        {
            get { return _PostingDate; }
            set
            {
                SetPropertyValue("PostingDate", ref _PostingDate, value);
            }
        }

        private DocStatus _Status;
        [XafDisplayName("Status")]
        [Appearance("Status", Enabled = false)]
        [Index(15), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(false)]
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
        [Index(20), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public string Remarks
        {
            get { return _Remarks; }
            set
            {
                SetPropertyValue("Remarks", ref _Remarks, value);
            }
        }

        // Start ver 1.0.8.1
        //[NonPersistent]
        private string _LoadingNo;
        // End ver 1.0.8.1
        [XafDisplayName("Loading No.")]
        [Index(21), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(false)]
        [Appearance("LoadingNo", Enabled = false)]
        public string LoadingNo
        {
            // Start ver 1.0.8.1
            //get
            //{
            //    string rtn = null;
            //    string dupno = null;
            //    foreach (DeliveryOrderDetails dtl in this.DeliveryOrderDetails)
            //    {
            //        if (dupno != dtl.BaseDoc)
            //        {
            //            if (rtn == null)
            //            {
            //                rtn = dtl.BaseDoc;
            //            }
            //            else
            //            {
            //                rtn = rtn + ", " + dtl.BaseDoc;
            //            }

            //            dupno = dtl.BaseDoc;
            //        }
            //    }

            //    return rtn;
            //}
            get { return _LoadingNo; }
            set
            {
                SetPropertyValue("LoadingNo", ref _LoadingNo, value);
            }
            // End ver 1.0.8.1
        }

        // Start ver 1.0.8.1
        //[NonPersistent]
        private string _SONo;
        // End ver 1.0.8.1
        [XafDisplayName("SO No.")]
        [Index(23), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(false)]
        [Appearance("SONo", Enabled = false)]
        public string SONo
        {
            // Start ver 1.0.8.1
            //get
            //{
            //    string rtn = null;
            //    string dupso = null;
            //    foreach (DeliveryOrderDetails dtl in this.DeliveryOrderDetails)
            //    {
            //        if (dupso != dtl.SODocNum)
            //        {
            //            if (rtn == null)
            //            {
            //                rtn = dtl.SODocNum;
            //            }
            //            else
            //            {
            //                rtn = rtn + ", " + dtl.SODocNum;
            //            }

            //            dupso = dtl.SODocNum;
            //        }
            //    }

            //    return rtn;
            //}
            get { return _SONo; }
            set
            {
                SetPropertyValue("SONo", ref _SONo, value);
            }
            // End ver 1.0.8.1
        }

        // Start ver 1.0.8.1
        //[NonPersistent]
        private PriorityType _Priority;
        // End ver 1.0.8.1
        [XafDisplayName("Priority")]
        [Index(25), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(false)]
        [Appearance("Priority", Enabled = false)]
        public PriorityType Priority
        {
            // Start ver 1.0.8.1
            //get
            //{
            //    PriorityType rtn = null;

            //    foreach (DeliveryOrderDetails dtl in this.DeliveryOrderDetails)
            //    {
            //        SalesOrder salesorder;
            //        salesorder = Session.FindObject<SalesOrder>(CriteriaOperator.Parse("DocNum = ?", dtl.SODocNum));

            //        if (salesorder != null)
            //        {
            //            rtn = salesorder.Priority;
            //        }
            //    }

            //    return rtn;
            //}
            get { return _Priority; }
            set
            {
                SetPropertyValue("Priority", ref _Priority, value);
            }
            // End ver 1.0.8.1
        }

        private string _SAPDocNum;
        [XafDisplayName("SAP AR Inv Num")]
        [Appearance("SAPDocNum", Enabled = false)]
        [Index(30), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(false)]
        public string SAPDocNum
        {
            get { return _SAPDocNum; }
            set
            {
                SetPropertyValue("SAPDocNum", ref _SAPDocNum, value);
            }
        }

        private string _SAPDODocNum;
        [XafDisplayName("SAP AR DO Num")]
        [Appearance("SAPDODocNum", Enabled = false)]
        [Index(31), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(false)]
        public string SAPDODocNum
        {
            get { return _SAPDODocNum; }
            set
            {
                SetPropertyValue("SAPDODocNum", ref _SAPDODocNum, value);
            }
        }

        private string _CustomerGroup;
        [XafDisplayName("Customer Group")]
        [Appearance("CustomerGroup", Enabled = false)]
        [Index(33), VisibleInDetailView(false), VisibleInListView(true), VisibleInLookupListView(false)]
        public string CustomerGroup
        {
            get { return _CustomerGroup; }
            set
            {
                SetPropertyValue("CustomerGroup", ref _CustomerGroup, value);
            }
        }

        private int _DOPrintCount;
        [XafDisplayName("DO Print Count")]
        [Appearance("DOPrintCount", Enabled = false)]
        [Index(35), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public int DOPrintCount
        {
            get { return _DOPrintCount; }
            set
            {
                SetPropertyValue("DOPrintCount", ref _DOPrintCount, value);
            }
        }

        private DateTime _DOPrintDate;
        [XafDisplayName("DO Last Print Date")]
        [ModelDefault("DisplayFormat", "{0: dd/MM/yyyy hh:mm tt}")]
        [Appearance("DOPrintDate", Enabled = false)]
        [Index(38), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(false)]
        public DateTime DOPrintDate
        {
            get { return _DOPrintDate; }
            set
            {
                SetPropertyValue("DOPrintDate", ref _DOPrintDate, value);
            }
        }

        private int _INVPrintCount;
        [XafDisplayName("INV Print Count")]
        [Appearance("INVPrintCount", Enabled = false)]
        [Index(40), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public int INVPrintCount
        {
            get { return _INVPrintCount; }
            set
            {
                SetPropertyValue("INVPrintCount", ref _INVPrintCount, value);
            }
        }

        private DateTime _INVPrintDate;
        [XafDisplayName("INV Last Print Date")]
        [ModelDefault("DisplayFormat", "{0: dd/MM/yyyy hh:mm tt}")]
        [Appearance("INVPrintDate", Enabled = false)]
        [Index(43), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public DateTime INVPrintDate
        {
            get { return _INVPrintDate; }
            set
            {
                SetPropertyValue("INVPrintDate", ref _INVPrintDate, value);
            }
        }

        private int _BundleDOPrintCount;
        [XafDisplayName("Bundle DO Print Count")]
        [Appearance("BundleDOPrintCount", Enabled = false)]
        [Index(40), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public int BundleDOPrintCount
        {
            get { return _BundleDOPrintCount; }
            set
            {
                SetPropertyValue("BundleDOPrintCount", ref _BundleDOPrintCount, value);
            }
        }

        private DateTime _BundleDOPrintDate;
        [XafDisplayName("Bundle DO Last Print Date")]
        [ModelDefault("DisplayFormat", "{0: dd/MM/yyyy hh:mm tt}")]
        [Appearance("BundleDOPrintDate", Enabled = false)]
        [Index(43), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public DateTime BundleDOPrintDate
        {
            get { return _BundleDOPrintDate; }
            set
            {
                SetPropertyValue("BundleDOPrintDate", ref _BundleDOPrintDate, value);
            }
        }

        // Start ver 1.0.21
        private string _DOPrintBy;
        [XafDisplayName("DO Print By")]
        [Appearance("DOPrintBy", Enabled = false)]
        [Index(44), VisibleInDetailView(false), VisibleInListView(false), VisibleInLookupListView(false)]
        public string DOPrintBy
        {
            get { return _DOPrintBy; }
            set
            {
                SetPropertyValue("DOPrintBy", ref _DOPrintBy, value);
            }
        }
        // End ver 1.0.21

        // Start ver 1.0.18
        private vwYesNo _EIVConsolidate;
        [NoForeignKey]
        [XafDisplayName("Require E-Invoice")]
        //[RuleRequiredField(DefaultContexts.Save)]
        [Index(50), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public vwYesNo EIVConsolidate
        {
            get { return _EIVConsolidate; }
            set
            {
                SetPropertyValue("EIVConsolidate", ref _EIVConsolidate, value);
            }
        }

        private vwEIVType _EIVType;
        [NoForeignKey]
        [XafDisplayName("E-Invoice Type")]
        //[RuleRequiredField(DefaultContexts.Save)]
        [Index(51), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public vwEIVType EIVType
        {
            get { return _EIVType; }
            set
            {
                SetPropertyValue("EIVType", ref _EIVType, value);
            }
        }

        private vwEIVFreqSync _EIVFreqSync;
        [NoForeignKey]
        [XafDisplayName("Sync. Freq.")]
        //[RuleRequiredField(DefaultContexts.Save)]
        [Index(52), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public vwEIVFreqSync EIVFreqSync
        {
            get { return _EIVFreqSync; }
            set
            {
                SetPropertyValue("EIVFreqSync", ref _EIVFreqSync, value);
            }
        }

        //Buyer
        private string _EIVBuyerName;
        [XafDisplayName("Buyer's Name")]
        //[RuleRequiredField(DefaultContexts.Save)]
        [Index(53), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public string EIVBuyerName
        {
            get { return _EIVBuyerName; }
            set
            {
                SetPropertyValue("EIVBuyerName", ref _EIVBuyerName, value);
            }
        }

        private string _EIVBuyerTIN;
        [XafDisplayName("Buyer's TIN No")]
        [Index(54), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public string EIVBuyerTIN
        {
            get { return _EIVBuyerTIN; }
            set
            {
                SetPropertyValue("EIVBuyerTIN", ref _EIVBuyerTIN, value);
            }
        }

        private string _EIVBuyerRegNum;
        [XafDisplayName("Registration No.")]
        [Index(55), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public string EIVBuyerRegNum
        {
            get { return _EIVBuyerRegNum; }
            set
            {
                SetPropertyValue("EIVBuyerRegNum", ref _EIVBuyerRegNum, value);
            }
        }

        private vwEIVRegType _EIVBuyerRegTyp;
        [NoForeignKey]
        [XafDisplayName("Registration Type")]
        [Index(56), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public vwEIVRegType EIVBuyerRegTyp
        {
            get { return _EIVBuyerRegTyp; }
            set
            {
                SetPropertyValue("EIVBuyerRegTyp", ref _EIVBuyerRegTyp, value);
            }
        }

        private string _EIVBuyerSSTRegNum;
        [XafDisplayName("SST Registration No.")]
        [Index(57), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public string EIVBuyerSSTRegNum
        {
            get { return _EIVBuyerSSTRegNum; }
            set
            {
                SetPropertyValue("EIVBuyerSSTRegNum", ref _EIVBuyerSSTRegNum, value);
            }
        }

        private string _EIVBuyerEmail;
        [XafDisplayName("E-mail ")]
        [Index(58), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public string EIVBuyerEmail
        {
            get { return _EIVBuyerEmail; }
            set
            {
                SetPropertyValue("EIVBuyerEmail", ref _EIVBuyerEmail, value);
            }
        }

        private string _EIVBuyerContact;
        [XafDisplayName("Contact No.")]
        //[RuleRequiredField(DefaultContexts.Save)]
        [Index(59), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public string EIVBuyerContact
        {
            get { return _EIVBuyerContact; }
            set
            {
                SetPropertyValue("EIVBuyerContact", ref _EIVBuyerContact, value);
            }
        }

        private string _EIVAddressLine1B;
        [XafDisplayName("Buyer's Address Line 1")]
        //[RuleRequiredField(DefaultContexts.Save)]
        [Index(60), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public string EIVAddressLine1B
        {
            get { return _EIVAddressLine1B; }
            set
            {
                SetPropertyValue("EIVAddressLine1B", ref _EIVAddressLine1B, value);
            }
        }

        private string _EIVAddressLine2B;
        [XafDisplayName("Buyer's Address Line 2")]
        [Index(61), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public string EIVAddressLine2B
        {
            get { return _EIVAddressLine2B; }
            set
            {
                SetPropertyValue("EIVAddressLine2B", ref _EIVAddressLine2B, value);
            }
        }

        private string _EIVAddressLine3B;
        [XafDisplayName("Buyer's Address Line 3")]
        [Index(62), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public string EIVAddressLine3B
        {
            get { return _EIVAddressLine3B; }
            set
            {
                SetPropertyValue("EIVAddressLine3B", ref _EIVAddressLine3B, value);
            }
        }

        private string _EIVPostalZoneB;
        [XafDisplayName("Buyer's Postcode")]
        [Index(63), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public string EIVPostalZoneB
        {
            get { return _EIVPostalZoneB; }
            set
            {
                SetPropertyValue("EIVPostalZoneB", ref _EIVPostalZoneB, value);
            }
        }

        private string _EIVCityNameB;
        [XafDisplayName("Buyer's City")]
        //[RuleRequiredField(DefaultContexts.Save)]
        [Index(64), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public string EIVCityNameB
        {
            get { return _EIVCityNameB; }
            set
            {
                SetPropertyValue("EIVCityNameB", ref _EIVCityNameB, value);
            }
        }

        private vwState _EIVStateB;
        [NoForeignKey]
        [XafDisplayName("Buyer's State")]
        [Index(65), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public vwState EIVStateB
        {
            get { return _EIVStateB; }
            set
            {
                SetPropertyValue("EIVStateB", ref _EIVStateB, value);
            }
        }

        private vwCountry _EIVCountryB;
        [NoForeignKey]
        [XafDisplayName("Buyer's Country")]
        //[RuleRequiredField(DefaultContexts.Save)]
        [Index(66), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public vwCountry EIVCountryB
        {
            get { return _EIVCountryB; }
            set
            {
                SetPropertyValue("EIVCountryB", ref _EIVCountryB, value);
            }
        }

        //Recipient
        private string _EIVShippingName;
        [XafDisplayName("Recipient's Name")]
        //[RuleRequiredField(DefaultContexts.Save)]
        [Index(67), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public string EIVShippingName
        {
            get { return _EIVShippingName; }
            set
            {
                SetPropertyValue("EIVShippingName", ref _EIVShippingName, value);
            }
        }

        private string _EIVShippingTin;
        [XafDisplayName("Recipient's TIN")]
        [Index(68), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public string EIVShippingTin
        {
            get { return _EIVShippingTin; }
            set
            {
                SetPropertyValue("EIVShippingTin", ref _EIVShippingTin, value);
            }
        }

        private string _EIVShippingRegNum;
        [XafDisplayName("Recipient’s Registration No.")]
        [Index(69), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public string EIVShippingRegNum
        {
            get { return _EIVShippingRegNum; }
            set
            {
                SetPropertyValue("EIVShippingRegNum", ref _EIVShippingRegNum, value);
            }
        }

        private vwEIVRegType _EIVShippingRegTyp;
        [NoForeignKey]
        [XafDisplayName("Recipient’s Reg. No. Type")]
        [Index(70), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public vwEIVRegType EIVShippingRegTyp
        {
            get { return _EIVShippingRegTyp; }
            set
            {
                SetPropertyValue("EIVShippingRegTyp", ref _EIVShippingRegTyp, value);
            }
        }

        private string _EIVAddressLine1S;
        [XafDisplayName("Recipient's Address Line 1")]
        //[RuleRequiredField(DefaultContexts.Save)]
        [Index(71), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public string EIVAddressLine1S
        {
            get { return _EIVAddressLine1S; }
            set
            {
                SetPropertyValue("EIVAddressLine1S", ref _EIVAddressLine1S, value);
            }
        }

        private string _EIVAddressLine2S;
        [XafDisplayName("Recipient's Address Line 2")]
        [Index(72), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public string EIVAddressLine2S
        {
            get { return _EIVAddressLine2S; }
            set
            {
                SetPropertyValue("EIVAddressLine2S", ref _EIVAddressLine2S, value);
            }
        }

        private string _EIVAddressLine3S;
        [XafDisplayName("Recipient's Address Line 3")]
        [Index(73), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public string EIVAddressLine3S
        {
            get { return _EIVAddressLine3S; }
            set
            {
                SetPropertyValue("EIVAddressLine3S", ref _EIVAddressLine3S, value);
            }
        }

        private string _EIVPostalZoneS;
        [XafDisplayName("Recipient's Postcode")]
        [Index(74), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public string EIVPostalZoneS
        {
            get { return _EIVPostalZoneS; }
            set
            {
                SetPropertyValue("EIVPostalZoneS", ref _EIVPostalZoneS, value);
            }
        }

        private string _EIVCityNameS;
        [XafDisplayName("Recipient's City")]
        //[RuleRequiredField(DefaultContexts.Save)]
        [Index(75), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public string EIVCityNameS
        {
            get { return _EIVCityNameS; }
            set
            {
                SetPropertyValue("EIVCityNameS", ref _EIVCityNameS, value);
            }
        }

        private vwState _EIVStateS;
        [NoForeignKey]
        [XafDisplayName("Recipient's State")]
        [Index(76), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public vwState EIVStateS
        {
            get { return _EIVStateS; }
            set
            {
                SetPropertyValue("EIVStateS", ref _EIVStateS, value);
            }
        }

        private vwCountry _EIVCountryS;
        [NoForeignKey]
        [XafDisplayName("Recipient's Country")]
        //[RuleRequiredField(DefaultContexts.Save)]
        [Index(77), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public vwCountry EIVCountryS
        {
            get { return _EIVCountryS; }
            set
            {
                SetPropertyValue("EIVCountryS", ref _EIVCountryS, value);
            }
        }
        // End ver 1.0.18

        private bool _Sap;
        [XafDisplayName("Sap")]
        [Index(80), VisibleInDetailView(false), VisibleInListView(false), VisibleInLookupListView(false)]
        public bool Sap
        {
            get { return _Sap; }
            set
            {
                SetPropertyValue("Sap", ref _Sap, value);
            }
        }

        private bool _SapDO;
        [XafDisplayName("SapDO")]
        [Index(81), VisibleInDetailView(false), VisibleInListView(false), VisibleInLookupListView(false)]
        public bool SapDO
        {
            get { return _SapDO; }
            set
            {
                SetPropertyValue("SapDO", ref _SapDO, value);
            }
        }

        private bool _SapINV;
        [XafDisplayName("SapINV")]
        [Index(82), VisibleInDetailView(false), VisibleInListView(false), VisibleInLookupListView(false)]
        public bool SapINV
        {
            get { return _SapINV; }
            set
            {
                SetPropertyValue("SapINV", ref _SapINV, value);
            }
        }

        [Browsable(false)]
        public bool IsNew
        {
            get
            { return Session.IsNewObject(this); }
        }

        [Association("DeliveryOrder-DeliveryOrderDetails")]
        [XafDisplayName("Content")]
        public XPCollection<DeliveryOrderDetails> DeliveryOrderDetails
        {
            get { return GetCollection<DeliveryOrderDetails>("DeliveryOrderDetails"); }
        }

        [Association("DeliveryOrder-DeliveryOrderDocTrail")]
        [XafDisplayName("Status History")]
        public XPCollection<DeliveryOrderDocTrail> DeliveryOrderDocTrail
        {
            get { return GetCollection<DeliveryOrderDocTrail>("DeliveryOrderDocTrail"); }
        }

        private XPCollection<AuditDataItemPersistent> auditTrail;
        public XPCollection<AuditDataItemPersistent> AuditTrail
        {
            get
            {
                if (auditTrail == null)
                {
                    auditTrail = AuditedObjectWeakReference.GetAuditTrail(Session, this);
                }
                return auditTrail;
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

                if (Session.IsNewObject(this))
                {
                    DeliveryOrderDocTrail ds = new DeliveryOrderDocTrail(Session);
                    ds.DocStatus = DocStatus.Draft;
                    ds.DocRemarks = "";
                    if (user != null)
                    {
                        ds.CreateUser = Session.GetObjectByKey<ApplicationUser>(user.Oid);
                        ds.UpdateUser = Session.GetObjectByKey<ApplicationUser>(user.Oid);
                    }
                    else
                    {
                        ds.CreateUser = Session.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                        ds.UpdateUser = Session.GetObjectByKey<ApplicationUser>(Guid.Parse("100348B5-290E-47DF-9355-557C7E2C56D3"));
                    }
                    ds.CreateDate = DateTime.Now;
                    ds.UpdateDate = DateTime.Now;
                    this.DeliveryOrderDocTrail.Add(ds);
                }
            }
        }
    }
}