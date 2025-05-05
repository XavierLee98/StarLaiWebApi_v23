using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Xpo;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using DevExpress.XtraPrinting;
using StarLaiPortal.Module.BusinessObjects.Setup;
using StarLaiPortal.Module.BusinessObjects.View;
using StarLaiPortal.Module.Controllers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using static System.Net.Mime.MediaTypeNames;
using DevExpress.Xpo.DB.Helpers;
using DevExpress.Persistent.Base.Security;
using System.Runtime.Remoting.Lifetime;
using DevExpress.Web.Internal.XmlProcessor;

// 2023-07-28 block submit if no address for OC and OS ver 1.0.7
// 2023-12-01 change to action for create SO button ver 1.0.13
// 2024-01-30 Add import update button ver 1.0.14
// 2024-04-01 Enhance performance ver 1.0.15
// 2024-04-01 filter customer by U_blockSales ver 1.0.15
// 2024-04-04 - remove stockbalance view - ver 1.0.15
// 2024-06-01 - hide the priority if inactive - ver 1.0.17
// 2024-06-12 - e-invoice - ver 1.0.18
// 2024-10-09 - new enhancement - ver 1.0.21
// 2025-01-23 - new enhancement - ver 1.0.22

namespace StarLaiPortal.Module.BusinessObjects.Sales_Quotation
{
    [DefaultClassOptions]
    [XafDisplayName("Sales Quotation")]
    [NavigationItem("Sales Quotation")]
    [DefaultProperty("DocNum")]
    [Appearance("HideNew", AppearanceItemType = "Action", TargetItems = "New", Context = "SalesQuotation_ListView_Approval", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide)]
    [Appearance("HideNew1", AppearanceItemType = "Action", TargetItems = "New", Criteria = "(AppStatus in (2))", Context = "SalesQuotation_DetailView_Approval", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide)]
    
    [Appearance("HideEdit", AppearanceItemType.Action, "True", TargetItems = "SwitchToEditMode; Edit", Criteria = "not (Status in (0))", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideEdit1", AppearanceItemType.Action, "True", TargetItems = "SwitchToEditMode; Edit", Criteria = "(AppStatus in (2))", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    
    [Appearance("HideDelete", AppearanceItemType.Action, "True", TargetItems = "Delete", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSubmit", AppearanceItemType.Action, "True", TargetItems = "CreateSalesOrder", Criteria = "not (Status in (0))", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSubmit1", AppearanceItemType.Action, "True", TargetItems = "CreateSalesOrder", Criteria = "(AppStatus in (2))", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    // Start ver 1.0.13
    [Appearance("HideSubmitAction", AppearanceItemType.Action, "True", TargetItems = "CreateSalesOrderAction", Criteria = "not (Status in (0))", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSubmitAction1", AppearanceItemType.Action, "True", TargetItems = "CreateSalesOrderAction", Criteria = "(AppStatus in (2))", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    // End ver 1.0.13

    [Appearance("HideCancel", AppearanceItemType.Action, "True", TargetItems = "CancelSalesOrder", Criteria = "not (Status in (0))", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideCancel1", AppearanceItemType.Action, "True", TargetItems = "CancelSalesOrder", Criteria = "(AppStatus in (2))", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]

    [Appearance("HideDup", AppearanceItemType.Action, "True", TargetItems = "DuplicateSQ", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "SalesQuotation_DetailView_Approval")]

    [Appearance("HideItemInq", AppearanceItemType.Action, "True", TargetItems = "InquiryItem", Criteria = "Customer = null or Transporter = null or CustomerName = null " +
        "or ContactNo = null or IsValid9 = 1 or IsValid16 = 1 or IsValid18 = 1 or IsValid12 = 1", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideExportSQ", AppearanceItemType.Action, "True", TargetItems = "ExportSQImport", Criteria = "DocNum = null", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideImportSQ", AppearanceItemType.Action, "True", TargetItems = "ImportSQ", Criteria = "DocNum = null", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    // Start ver 1.0.14
    [Appearance("HideImportUpdateSQ", AppearanceItemType.Action, "True", TargetItems = "ImportUpdateSQ", Criteria = "DocNum = null", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    // End ver 1.0.14

    // Start ver 1.0.18
    [RuleCriteria("EIVSQBilling", DefaultContexts.Save, "IsValid9 = 0", "Please fill in Buyer TIN and Buyer Reg.Num.")]
    //[RuleCriteria("EIVSQShipping", DefaultContexts.Save, "IsValid10 = 0", "Shipping TIN and Shipping Reg. Num. must fill one of them.")]

    [RuleCriteria("EIVSQBillingType", DefaultContexts.Save, "IsValid11 = 0", "Please fill in Buyer Reg. Type.")]
    [RuleCriteria("EIVSQShippingType", DefaultContexts.Save, "IsValid12 = 0", "Please fill in Shipping Reg. Type.")]

    [RuleCriteria("EIVSQBillingState", DefaultContexts.Save, "IsValid13 = 0", "Please fill in Buyer State.")]
    [RuleCriteria("EIVSQShippingState", DefaultContexts.Save, "IsValid14 = 0", "Please fill in Shipping State.")]

    [RuleCriteria("EIVSQEmail", DefaultContexts.Save, "IsValid15 = 0", "Please fill in email address.")]

    [RuleCriteria("EIVSQEIVBMandatory", DefaultContexts.Save, "IsValid16 = 0", "Please fill in EIV mandatory field. (EIV Type / Sync. Freq. / Buyer's Name/ " +
        "Buyer's Address Line 1 / Buyer's Country / Contact No. ")]

    [RuleCriteria("EIVSQEIVBSMandatory", DefaultContexts.Save, "IsValid17 = 0", "Recipient's Address Line 1 / Recipient's City / Recipient's Country")]

    [RuleCriteria("EIVSQEIVSNameMandatory", DefaultContexts.Save, "IsValid18 = 0", "Recipient's name not allow blank.")]
    // End ver 1.0.18

    public class SalesQuotation : XPObject
    { // Inherit from a different class to provide a custom primary key, concurrency and deletion behavior, etc. (https://documentation.devexpress.com/eXpressAppFramework/CustomDocument113146.aspx).
      // Use CodeRush to create XPO classes and properties with a few keystrokes.
      // https://docs.devexpress.com/CodeRushForRoslyn/118557
        public SalesQuotation(Session session)
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
            DeliveryDate = DateTime.Now;

            DocType = DocTypeList.SQ;
            Priority = Session.FindObject<PriorityType>(CriteriaOperator.Parse("PriorityName = ? and IsActive= ?",
                         "Normal", "True"));
            Status = DocStatus.Draft;
            AppStatus = ApprovalStatusType.Not_Applicable;

            // Start ver 1.0.18
            EIVCountryB = Session.FindObject<vwCountry>(CriteriaOperator.Parse("Code = ?", "MY"));
            EIVCountryS = Session.FindObject<vwCountry>(CriteriaOperator.Parse("Code = ?", "MY"));
            // End ver 1.0.18
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
        [Index(301), VisibleInListView(true), VisibleInDetailView(false), VisibleInLookupListView(false)]
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
        [XafDisplayName("Quotation Num")]
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
        [LookupEditorMode(LookupEditorMode.AllItems)]
        // Start ver 1.0.15
        //[DataSourceCriteria("ValidFor = 'Y' and CardType = 'C'")]
        [DataSourceCriteria("ValidFor = 'Y' and CardType = 'C' and U_blockSales = 'N'")]
        // End ver 1.0.15
        [Index(5), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(false)]
        public vwBusniessPartner Customer
        {
            get { return _Customer; }
            set
            {
                SetPropertyValue("Customer", ref _Customer, value);
                if (!IsLoading && value != null)
                {
                    bool update = false;
                    //if (Customer.GroupName != "Trade Debtor - Cash")
                    //{
                    //    CustomerName = Customer.BPName;
                    //}
                    //else
                    //{
                    //    CustomerName = null;
                    //}
                    Balance = Customer.Balance;
                    BillingAddress = Session.FindObject<vwBillingAddress>(CriteriaOperator.Parse("AddressKey = ? and CardCode = ?"
                        , Customer.BillToDef, Customer.BPCode));
                    ShippingAddress = Session.FindObject<vwShippingAddress>(CriteriaOperator.Parse("AddressKey = ? and CardCode = ?"
                        , Customer.ShipToDef, Customer.BPCode));
                    if (Customer.PaymentTerm != null)
                    {
                        PaymentTerm = Session.FindObject<vwPaymentTerm>(CriteriaOperator.Parse("GroupNum = ?", Customer.PaymentTerm.GroupNum));
                    }
                    
                    // Start ver 1.0.21
                    //ContactNo = Customer.Contact;
                    // End ver 1.0.21
                    Currency = Customer.Currency;
                    if (Customer.Transporter != null)
                    {
                        Transporter = Session.FindObject<vwTransporter>(CriteriaOperator.Parse("TransporterID = ?", Customer.Transporter.TransporterID));
                    }
                    if (Customer.SlpCode != null)
                    {
                        ContactPerson = Session.FindObject<vwSalesPerson>(CriteriaOperator.Parse("SlpCode = ?", Customer.SlpCode.SlpCode));
                    }
                    if (Customer.SalesOrderSeries != null)
                    {
                        Series = Session.FindObject<vwSeries>(CriteriaOperator.Parse("Series = ?", Customer.SalesOrderSeries));
                    }

                    // Start ver 1.0.18
                    // Buyer
                    if (Customer.U_EIV_Consolidate == "Y")
                    {
                        EIVConsolidate = Session.FindObject<vwYesNo>(CriteriaOperator.Parse("Code = ?", "N"));
                    }
                    else
                    {
                        EIVConsolidate = Session.FindObject<vwYesNo>(CriteriaOperator.Parse("Code = ?", "Y"));
                    }
                    EIVType = Session.FindObject<vwEIVType>(CriteriaOperator.Parse("Code = ?", Customer.U_EIV_TypeARIV));
                    EIVFreqSync = Session.FindObject<vwEIVFreqSync>(CriteriaOperator.Parse("Code = ?", Customer.U_EIV_FreqARIV));
                    EIVBuyerName = Customer.U_EIV_BuyerName;
                    EIVBuyerTIN = Customer.U_EIV_BuyerTin;
                    EIVBuyerRegNum = Customer.U_EIV_BuyerRegNum;
                    EIVBuyerRegTyp = Session.FindObject<vwEIVRegType>(CriteriaOperator.Parse("Code = ?", Customer.U_EIV_BuyerRegTyp));
                    EIVBuyerSSTRegNum = Customer.U_EIV_BuyerSSTRegNum;
                    EIVBuyerEmail = Customer.U_EIV_BuyerEmail;
                    EIVBuyerContact = Customer.U_EIV_BuyerContact;
                    EIVShippingName = Customer.U_EIV_BuyerName;
                    // End ver 1.0.18

                    // Start ver 1.0.21
                    ContactNo = Customer.Contact;
                    CustomerName = Customer.BPName;
                    // End ver 1.0.21

                    foreach (SalesQuotationDetails dtl in this.SalesQuotationDetails)
                    {
                        vwPriceWithVolumeDiscount tempprice = Session.FindObject<vwPriceWithVolumeDiscount>(CriteriaOperator.Parse(
                        "ItemCode = ? and ListNum = ? and ? >= FromDate and ? <= ToDate and ? >= FromQty and ? <= ToQty",
                        dtl.ItemCode, Customer.ListNum, PostingDate.Date, PostingDate.Date, dtl.Quantity, dtl.Quantity));

                        if (tempprice != null)
                        {
                            dtl.Price = tempprice.Price;
                            dtl.AdjustedPrice = tempprice.Price;
                        }
                        else
                        {
                            vwPrice temppricelist = Session.FindObject<vwPrice>(CriteriaOperator.Parse("ItemCode = ? and PriceList = ?",
                                dtl.ItemCode.ItemCode, Customer.ListNum));

                            if (temppricelist != null)
                            {
                                dtl.Price = temppricelist.Price;
                                dtl.AdjustedPrice = temppricelist.Price;
                            }
                            else
                            {
                                dtl.Price = 0;
                                dtl.AdjustedPrice = 0;
                            }
                        }
                        
                        update = true;
                    }

                    if (update == true && this.DocNum != null)
                    {
                        this.Session.CommitTransaction();
                    }
                }
                else if (!IsLoading && value == null)
                {
                    CustomerName = null;
                    Balance = 0;
                    BillingAddress = null;
                    ShippingAddress = null;
                    PaymentTerm = null;
                    ContactNo = null;
                    Currency = null;
                    Transporter = null;
                    ContactPerson = null;
                    Series = null;

                    // Start ver 1.0.18
                    // Buyer
                    EIVConsolidate = null;
                    EIVType = null;
                    EIVFreqSync = null;
                    EIVBuyerName = null;
                    EIVBuyerTIN = null;
                    EIVBuyerRegNum = null;
                    EIVBuyerRegTyp = null;
                    EIVBuyerSSTRegNum = null;
                    EIVBuyerEmail = null;
                    EIVBuyerContact = null;
                    //Recipient
                    EIVShippingName = null;
                    EIVShippingTin = null;
                    EIVShippingRegNum = null;
                    EIVShippingRegTyp = null;
                    // End ver 1.0.18
                }
            }
        }

        private string _CustomerName;
        [ImmediatePostData]
        [XafDisplayName("Customer Name")]
        [Appearance("CustomerName", Enabled = false, Criteria = "Customer.GroupName != 'Trade Debtor - Cash'")]
        [RuleRequiredField(DefaultContexts.Save)]
        [Index(8), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(false)]
        public string CustomerName
        {
            get { return _CustomerName; }
            set
            {
                SetPropertyValue("CustomerName", ref _CustomerName, value);
                // Start ver 1.0.18
                if (!IsLoading && value != null)
                {
                    EIVBuyerName = CustomerName;
                }
                // End ver 1.0.18
            }
        }

        private vwTransporter _Transporter;
        [NoForeignKey]
        [ImmediatePostData]
        [LookupEditorMode(LookupEditorMode.AllItems)]
        [RuleRequiredField(DefaultContexts.Save)]
        [XafDisplayName("Transporter")]
        [Index(10), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(false)]
        public vwTransporter Transporter
        {
            get { return _Transporter; }
            set
            {
                SetPropertyValue("Transporter", ref _Transporter, value);
            }
        }

        private string _ContactNo;
        [ImmediatePostData]
        [XafDisplayName("Contact No")]
        [RuleRequiredField(DefaultContexts.Save, TargetCriteria = "Customer.GroupName = 'Trade Debtor - Cash'")]
        [Index(13), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public string ContactNo
        {
            get { return _ContactNo; }
            set
            {
                SetPropertyValue("ContactNo", ref _ContactNo, value);
                if (!IsLoading && value != null)
                {
                    if (string.IsNullOrEmpty(EIVBuyerContact))
                    {
                        if (ContactNo.Length > 20)
                        {
                            EIVBuyerContact = ContactNo.Substring(0, 19);
                        }
                        else
                        {
                            EIVBuyerContact = ContactNo;
                        }
                    }
                }
            }
        }

        private vwSalesPerson _ContactPerson;
        [NoForeignKey]
        [XafDisplayName("Salesperson")]
        [LookupEditorMode(LookupEditorMode.AllItems)]
        [DataSourceCriteria("Active = 'Y'")]
        [Index(15), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(false)]
        public vwSalesPerson ContactPerson
        {
            get { return _ContactPerson; }
            set
            {
                SetPropertyValue("ContactPerson", ref _ContactPerson, value);
            }
        }

        private decimal _Balance;
        [XafDisplayName("Credit Balance")]
        [DbType("numeric(18,6)")]
        [ModelDefault("DisplayFormat", "{0:n2}")]
        [Index(18), VisibleInListView(false), VisibleInDetailView(true), VisibleInLookupListView(false)]
        [Appearance("Balance", Enabled = false)]
        public decimal Balance

        {
            get { return _Balance; }
            set
            {
                SetPropertyValue("Balance", ref _Balance, value);
            }
        }

        private vwPaymentTerm _PaymentTerm;
        [NoForeignKey]
        [XafDisplayName("Payment Term")]
        // Start ver 1.0.15
        //[LookupEditorMode(LookupEditorMode.AllItems)]
        // End ver 1.0.15
        [Appearance("PaymentTerm", Enabled = false)]
        [Index(19), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public vwPaymentTerm PaymentTerm
        {
            get { return _PaymentTerm; }
            set
            {
                SetPropertyValue("PaymentTerm", ref _PaymentTerm, value);
            }
        }

        private vwSeries _Series;
        [NoForeignKey]
        // Start ver 1.0.15
        //[ImmediatePostData]
        // End ver 1.0.15
        [LookupEditorMode(LookupEditorMode.AllItems)]
        [DataSourceCriteria("ObjectCode = '17' and (('@this.Customer.GroupName' != 'Trade Debtor - Cash' and SeriesName != 'Cash') " +
            "or ('@this.Customer.GroupName' = 'Trade Debtor - Cash'))")]
        [XafDisplayName("Series")]
        [Index(20), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public vwSeries Series
        {
            get { return _Series; }
            set
            {
                SetPropertyValue("Series", ref _Series, value);
            }
        }

        private PriorityType _Priority;
        [XafDisplayName("Priority")]
        // Start ver 1.0.17
        [ImmediatePostData]
        [DataSourceCriteria("IsActive = 'True'")]
        // End ver 1.0.17
        [Index(23), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public PriorityType Priority
        {
            get { return _Priority; }
            set
            {
                SetPropertyValue("Priority", ref _Priority, value);
            }
        }

        private DateTime _PostingDate;
        // Start ver 1.0.15
        [ImmediatePostData]
        // End ver 1.0.15
        [XafDisplayName("Posting Date")]
        [Index(24), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public DateTime PostingDate
        {
            get { return _PostingDate; }
            set
            {
                SetPropertyValue("PostingDate", ref _PostingDate, value);
                if (!IsLoading)
                {
                    bool update = false;
                    foreach (SalesQuotationDetails dtl in this.SalesQuotationDetails)
                    {
                        vwPriceWithVolumeDiscount tempprice = Session.FindObject<vwPriceWithVolumeDiscount>(CriteriaOperator.Parse(
                        "ItemCode = ? and ListNum = ? and ? >= FromDate and ? <= ToDate and ? >= FromQty and ? <= ToQty",
                        dtl.ItemCode, Customer.ListNum, PostingDate.Date, PostingDate.Date, dtl.Quantity, dtl.Quantity));

                        if (tempprice != null)
                        {
                            dtl.Price = tempprice.Price;
                            dtl.AdjustedPrice = tempprice.Price;
                        }
                        else
                        {
                            vwPrice temppricelist = Session.FindObject<vwPrice>(CriteriaOperator.Parse("ItemCode = ? and PriceList = ?",
                                dtl.ItemCode.ItemCode, Customer.ListNum));

                            if (temppricelist != null)
                            {
                                dtl.Price = temppricelist.Price;
                                dtl.AdjustedPrice = temppricelist.Price;
                            }
                            else
                            {
                                dtl.Price = 0;
                                dtl.AdjustedPrice = 0;
                            }
                        }
                        
                        update = true;
                    }

                    if (update == true && this.DocNum != null)
                    {
                        this.Session.CommitTransaction();
                    }
                }
            }
        }

        private DateTime _DeliveryDate;
        [XafDisplayName("Delivery Date")]
        [Index(25), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public DateTime DeliveryDate
        {
            get { return _DeliveryDate; }
            set
            {
                SetPropertyValue("DeliveryDate", ref _DeliveryDate, value);
            }
        }

        private DateTime _ValidUntil;
        [XafDisplayName("Valid Until")]
        [Index(26), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public DateTime ValidUntil
        {
            get { return _ValidUntil; }
            set
            {
                SetPropertyValue("ValidUntil", ref _ValidUntil, value);
            }
        }

        private DateTime _DocDate;
        [XafDisplayName("DocDate")]
        [Index(27), VisibleInDetailView(false), VisibleInListView(false), VisibleInLookupListView(false)]
        public DateTime DocDate
        {
            get { return _DocDate; }
            set
            {
                SetPropertyValue("_DocDate", ref _DocDate, value);
            }
        }

        private ApprovalStatusType _AppStatus;
        [XafDisplayName("Approval Status")]
        [Appearance("AppStatus", Enabled = false)]
        [Index(28), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(false)]
        public ApprovalStatusType AppStatus
        {
            get { return _AppStatus; }
            set
            {
                SetPropertyValue("AppStatus", ref _AppStatus, value);
            }
        }

        private DocStatus _Status;
        [XafDisplayName("Status")]
        [Appearance("Status", Enabled = false)]
        [Index(29), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(false)]
        public DocStatus Status
        {
            get { return _Status; }
            set
            {
                SetPropertyValue("Status", ref _Status, value);
            }
        }

        private decimal _Total;
        [XafDisplayName("Total")]
        [Appearance("Total", Enabled = false)]
        [DbType("numeric(18,6)")]
        [ModelDefault("DisplayFormat", "{0:n2}")]
        [Index(30), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public decimal Total
        {
            get
            {
                if (Session.IsObjectsSaving != true)
                {
                    decimal rtn = 0;
                    if (SalesQuotationDetails != null)
                        rtn += SalesQuotationDetails.Sum(p => p.Total);

                    return rtn;
                }
                else
                {
                    return _Total;
                }
            }
            set
            {
                SetPropertyValue("Total", ref _Total, value);
            }
        }

        private vwBillingAddress _BillingAddress;
        [NoForeignKey]
        [ImmediatePostData]
        [DataSourceCriteria("CardCode = '@this.Customer.BPCode'")]
        [LookupEditorMode(LookupEditorMode.AllItems)]
        [XafDisplayName("Billing Address")]
        [Index(33), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public vwBillingAddress BillingAddress
        {
            get { return _BillingAddress; }
            set
            {
                SetPropertyValue("BillingAddress", ref _BillingAddress, value);
                if (!IsLoading && value != null)
                {
                    BillingAddressfield = BillingAddress.Address;
                    // Start ver 1.0.18
                    EIVAddressLine1B = BillingAddress.Street;
                    EIVAddressLine2B = BillingAddress.Block;
                    EIVAddressLine3B = BillingAddress.City;
                    EIVPostalZoneB = BillingAddress.ZipCode;
                    EIVCityNameB = BillingAddress.County;
                    EIVStateB = Session.FindObject<vwState>(CriteriaOperator.Parse("Code = ?", BillingAddress.State));
                    EIVCountryB = Session.FindObject<vwCountry>(CriteriaOperator.Parse("Code = ?", BillingAddress.Country));
                    // End ver 1.0.18
                }
                else if (!IsLoading && value == null)
                {
                    BillingAddressfield = null;
                    // Start ver 1.0.18
                    EIVAddressLine1B = null;
                    EIVAddressLine2B = null;
                    EIVAddressLine3B = null;
                    EIVPostalZoneB = null;
                    EIVCityNameB = null;
                    EIVStateB = null;
                    EIVCountryB = Session.FindObject<vwCountry>(CriteriaOperator.Parse("Code = ?", "MY"));
                    // End ver 1.0.18
                }
            }
        }

        private string _BillingAddressfield;
        [XafDisplayName("Billing Address Field")]
        [Size(254)]
        [Index(35), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public string BillingAddressfield
        {
            get { return _BillingAddressfield; }
            set
            {
                SetPropertyValue("BillingAddressfield", ref _BillingAddressfield, value);
            }
        }

        private vwShippingAddress _ShippingAddress;
        [NoForeignKey]
        [ImmediatePostData]
        [DataSourceCriteria("CardCode = '@this.Customer.BPCode'")]
        [LookupEditorMode(LookupEditorMode.AllItems)]
        [XafDisplayName("Shipping Address")]
        [Index(38), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public vwShippingAddress ShippingAddress
        {
            get { return _ShippingAddress; }
            set
            {
                SetPropertyValue("ShippingAddress", ref _ShippingAddress, value);
                if (!IsLoading && value != null)
                {
                    ShippingAddressfield = ShippingAddress.Address;
                    // Start ver 1.0.18
                    EIVAddressLine1S = ShippingAddress.Street;
                    EIVAddressLine2S = ShippingAddress.Block;
                    EIVAddressLine3S = ShippingAddress.City;
                    EIVPostalZoneS = ShippingAddress.ZipCode;
                    EIVCityNameS = ShippingAddress.County;
                    EIVStateS = Session.FindObject<vwState>(CriteriaOperator.Parse("Code = ?", ShippingAddress.State));
                    EIVCountryS = Session.FindObject<vwCountry>(CriteriaOperator.Parse("Code = ?", ShippingAddress.Country));
                    EIVShippingName = ShippingAddress.U_EIV_ShippingName;
                    EIVShippingTin = ShippingAddress.U_EIV_ShippingTin;
                    EIVShippingRegNum = ShippingAddress.U_EIV_ShippingRegNum;
                    EIVShippingRegTyp = Session.FindObject<vwEIVRegType>(CriteriaOperator.Parse("Code = ?", ShippingAddress.U_EIV_ShippingRegTyp));
                    // End ver 1.0.18
                }
                else if (!IsLoading && value == null)
                {
                    ShippingAddressfield = null;
                    // Start ver 1.0.18
                    EIVAddressLine1S = null;
                    EIVAddressLine2S = null;
                    EIVAddressLine3S = null;
                    EIVPostalZoneS = null;
                    EIVCityNameS = null;
                    EIVStateS = null;
                    EIVCountryS = Session.FindObject<vwCountry>(CriteriaOperator.Parse("Code = ?", "MY"));
                    EIVShippingName = null;
                    EIVShippingTin = null;
                    EIVShippingRegNum = null;
                    EIVShippingRegTyp = null;
                    // End ver 1.0.18
                }
            }
        }

        private string _ShippingAddressfield;
        [XafDisplayName("Shipping Address Field")]
        [Size(254)]
        [Index(40), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public string ShippingAddressfield
        {
            get { return _ShippingAddressfield; }
            set
            {
                SetPropertyValue("ShippingAddressfield", ref _ShippingAddressfield, value);
            }
        }

        private string _Remarks;
        [XafDisplayName("Remarks")]
        [Index(43), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public string Remarks
        {
            get { return _Remarks; }
            set
            {
                SetPropertyValue("Remarks", ref _Remarks, value);
            }
        }

        private string _Currency;
        [ImmediatePostData]
        [XafDisplayName("Currency")]
        [Appearance("Currency", Enabled = false)]
        [Index(45), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public string Currency
        {
            get { return _Currency; }
            set
            {
                SetPropertyValue("Currency", ref _Currency, value);
                if (!IsLoading && value != null)
                {
                    vwExchangeRate temprate = Session.FindObject<vwExchangeRate>(CriteriaOperator.Parse("RateDate = ? and Currency = ?",
                          PostingDate.Date, Currency));

                    if (temprate != null)
                    {
                        CurrencyRate = temprate.Rate;
                    }
                    else
                    {
                        if (Currency == "MYR")
                        {
                            CurrencyRate = 1;
                        }
                    }
                }
                else if (!IsLoading && value == null)
                {
                    CurrencyRate = 0;
                }
            }
        }

        private decimal _CurrencyRate;
        // Start ver 1.0.15
        //[ImmediatePostData]
        // End ver 1.0.15
        [DbType("numeric(18,6)")]
        [ModelDefault("DisplayFormat", "{0:n2}")]
        [XafDisplayName("Currency Rate")]
        [Appearance("CurrencyRate", Enabled = false)]
        [Index(48), VisibleInListView(true), VisibleInDetailView(false), VisibleInLookupListView(false)]
        public decimal CurrencyRate
        {
            get { return _CurrencyRate; }
            set
            {
                SetPropertyValue("CurrencyRate", ref _CurrencyRate, value);
            }
        }

        private bool _PriceChange;
        [XafDisplayName("Price Change")]
        [Appearance("PriceChange", Enabled = false)]
        [Index(50), VisibleInDetailView(false), VisibleInListView(true), VisibleInLookupListView(false)]
        public bool PriceChange
        {
            get { return _PriceChange; }
            set
            {
                SetPropertyValue("PriceChange", ref _PriceChange, value);
            }
        }

        private bool _ExceedPrice;
        [XafDisplayName("Exceed Limit")]
        [Appearance("ExceedPrice", Enabled = false)]
        [Index(53), VisibleInDetailView(false), VisibleInListView(true), VisibleInLookupListView(false)]
        public bool ExceedPrice
        {
            get { return _ExceedPrice; }
            set
            {
                SetPropertyValue("ExceedPrice", ref _ExceedPrice, value);
            }
        }

        private bool _ExceedCreditControl;
        [XafDisplayName("Exceed Credit Term")]
        [Appearance("ExceedCreditControl", Enabled = false)]
        [Index(55), VisibleInDetailView(false), VisibleInListView(true), VisibleInLookupListView(false)]
        public bool ExceedCreditControl
        {
            get { return _ExceedCreditControl; }
            set
            {
                SetPropertyValue("ExceedCreditControl", ref _ExceedCreditControl, value);
            }
        }

        private string _Attn;
        [XafDisplayName("Attn")]
        [Index(58), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public string Attn
        {
            get { return _Attn; }
            set
            {
                SetPropertyValue("Attn", ref _Attn, value);
            }
        }

        private string _RefNo;
        [XafDisplayName("Reference")]
        [Index(60), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public string RefNo
        {
            get { return _RefNo; }
            set
            {
                SetPropertyValue("RefNo", ref _RefNo, value);
            }
        }

        // Start ver 1.0.18
        private vwYesNo _EIVConsolidate;
        [NoForeignKey]
        [XafDisplayName("Require E-Invoice")]
        [RuleRequiredField(DefaultContexts.Save)]
        [Appearance("EIVConsolidate", Enabled = false, Criteria = "Customer.GroupName != 'Trade Debtor - Cash'")]
        [Index(70), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
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
        [ImmediatePostData]
        [XafDisplayName("E-Invoice Type")]
        //[RuleRequiredField(DefaultContexts.Save)]
        [Appearance("EIVType", Enabled = false, Criteria = "Customer.GroupName != 'Trade Debtor - Cash'")]
        [Index(71), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
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
        [ImmediatePostData]
        [XafDisplayName("Sync. Freq.")]
        //[RuleRequiredField(DefaultContexts.Save)]
        [Appearance("EIVFreqSync", Enabled = false, Criteria = "Customer.GroupName != 'Trade Debtor - Cash'")]
        [Index(72), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
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
        [Index(73), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public string EIVBuyerName
        {
            get { return _EIVBuyerName; }
            set
            {
                SetPropertyValue("EIVBuyerName", ref _EIVBuyerName, value);
            }
        }

        private string _EIVBuyerTIN;
        [XafDisplayName("Buyer's TIN No*")]
        [ImmediatePostData]
        [Size(14)]
        [Index(74), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public string EIVBuyerTIN
        {
            get { return _EIVBuyerTIN; }
            set
            {
                SetPropertyValue("EIVBuyerTIN", ref _EIVBuyerTIN, value);
            }
        }

        private string _EIVBuyerRegNum;
        [XafDisplayName("Registration No.*")]
        [ImmediatePostData]
        [Index(75), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
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
        [ImmediatePostData]
        [XafDisplayName("Registration Type*")]
        [Index(76), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
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
        [Index(77), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public string EIVBuyerSSTRegNum
        {
            get { return _EIVBuyerSSTRegNum; }
            set
            {
                SetPropertyValue("EIVBuyerSSTRegNum", ref _EIVBuyerSSTRegNum, value);
            }
        }

        private string _EIVBuyerEmail;
        [XafDisplayName("E-mail *")]
        [Index(78), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public string EIVBuyerEmail
        {
            get { return _EIVBuyerEmail; }
            set
            {
                SetPropertyValue("EIVBuyerEmail", ref _EIVBuyerEmail, value);
            }
        }

        private string _EIVBuyerContact;
        [XafDisplayName("Contact No.*")]
        [ImmediatePostData]
        [Index(79), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public string EIVBuyerContact
        {
            get { return _EIVBuyerContact; }
            set
            {
                SetPropertyValue("EIVBuyerContact", ref _EIVBuyerContact, value);
            }
        }

        private string _EIVAddressLine1B;
        [XafDisplayName("Buyer's Address Line 1*")]
        //[RuleRequiredField(DefaultContexts.Save)]
        [Index(80), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        [ImmediatePostData]
        public string EIVAddressLine1B
        {
            get { return _EIVAddressLine1B; }
            set
            {
                SetPropertyValue("EIVAddressLine1B", ref _EIVAddressLine1B, value);
                if (!IsLoading)
                {
                    if (EIVStateB != null)
                    {
                        if (EIVAddressLine2B != null && EIVAddressLine3B != null)
                        {
                            BillingAddressfield = EIVAddressLine1B + Environment.NewLine + EIVAddressLine2B + Environment.NewLine + EIVAddressLine3B
                                + Environment.NewLine + EIVPostalZoneB + " " + EIVCityNameB + ", "
                                + EIVStateB.Name;
                        }
                        else if (EIVAddressLine2B != null && EIVAddressLine3B == null)
                        {
                            BillingAddressfield = EIVAddressLine1B + Environment.NewLine + EIVAddressLine2B + Environment.NewLine
                                + EIVPostalZoneB + " " + EIVCityNameB + ", "
                                + EIVStateB.Name;
                        }
                        else if (EIVAddressLine2B == null && EIVAddressLine3B != null)
                        {
                            BillingAddressfield = EIVAddressLine1B + Environment.NewLine + EIVAddressLine3B + Environment.NewLine
                                + EIVPostalZoneB + " " + EIVCityNameB + ", "
                                + EIVStateB.Name;
                        }
                        else
                        {
                            BillingAddressfield = EIVAddressLine1B + Environment.NewLine
                                + EIVPostalZoneB + " " + EIVCityNameB + ", "
                                + EIVStateB.Name;
                        }
                    }
                    else
                    {
                        if (EIVAddressLine2B != null && EIVAddressLine3B != null)
                        {
                            BillingAddressfield = EIVAddressLine1B + Environment.NewLine + EIVAddressLine2B + Environment.NewLine + EIVAddressLine3B
                                + Environment.NewLine + EIVPostalZoneB + " " + EIVCityNameB;
                        }
                        else if (EIVAddressLine2B != null && EIVAddressLine3B == null)
                        {
                            BillingAddressfield = EIVAddressLine1B + Environment.NewLine + EIVAddressLine2B + Environment.NewLine 
                                + EIVPostalZoneB + " " + EIVCityNameB;
                        }
                        else if (EIVAddressLine2B == null && EIVAddressLine3B != null)
                        {
                            BillingAddressfield = EIVAddressLine1B + Environment.NewLine + EIVAddressLine3B + Environment.NewLine
                                + EIVPostalZoneB + " " + EIVCityNameB;
                        }
                        else
                        {
                            BillingAddressfield = EIVAddressLine1B + Environment.NewLine
                                + EIVPostalZoneB + " " + EIVCityNameB;
                        }
                    }
                }
            }
        }

        private string _EIVAddressLine2B;
        [XafDisplayName("Buyer's Address Line 2")]
        [Index(81), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        [ImmediatePostData]
        public string EIVAddressLine2B
        {
            get { return _EIVAddressLine2B; }
            set
            {
                SetPropertyValue("EIVAddressLine2B", ref _EIVAddressLine2B, value);
                if (!IsLoading)
                {
                    if (EIVStateB != null)
                    {
                        if (EIVAddressLine2B != null && EIVAddressLine3B != null)
                        {
                            BillingAddressfield = EIVAddressLine1B + Environment.NewLine + EIVAddressLine2B + Environment.NewLine + EIVAddressLine3B
                                + Environment.NewLine + EIVPostalZoneB + " " + EIVCityNameB + ", "
                                + EIVStateB.Name;
                        }
                        else if (EIVAddressLine2B != null && EIVAddressLine3B == null)
                        {
                            BillingAddressfield = EIVAddressLine1B + Environment.NewLine + EIVAddressLine2B + Environment.NewLine
                                + EIVPostalZoneB + " " + EIVCityNameB + ", "
                                + EIVStateB.Name;
                        }
                        else if (EIVAddressLine2B == null && EIVAddressLine3B != null)
                        {
                            BillingAddressfield = EIVAddressLine1B + Environment.NewLine + EIVAddressLine3B + Environment.NewLine
                                + EIVPostalZoneB + " " + EIVCityNameB + ", "
                                + EIVStateB.Name;
                        }
                        else
                        {
                            BillingAddressfield = EIVAddressLine1B + Environment.NewLine
                                + EIVPostalZoneB + " " + EIVCityNameB + ", "
                                + EIVStateB.Name;
                        }
                    }
                    else
                    {
                        if (EIVAddressLine2B != null && EIVAddressLine3B != null)
                        {
                            BillingAddressfield = EIVAddressLine1B + Environment.NewLine + EIVAddressLine2B + Environment.NewLine + EIVAddressLine3B
                                + Environment.NewLine + EIVPostalZoneB + " " + EIVCityNameB;
                        }
                        else if (EIVAddressLine2B != null && EIVAddressLine3B == null)
                        {
                            BillingAddressfield = EIVAddressLine1B + Environment.NewLine + EIVAddressLine2B + Environment.NewLine
                                + EIVPostalZoneB + " " + EIVCityNameB;
                        }
                        else if (EIVAddressLine2B == null && EIVAddressLine3B != null)
                        {
                            BillingAddressfield = EIVAddressLine1B + Environment.NewLine + EIVAddressLine3B + Environment.NewLine
                                + EIVPostalZoneB + " " + EIVCityNameB;
                        }
                        else
                        {
                            BillingAddressfield = EIVAddressLine1B + Environment.NewLine
                                + EIVPostalZoneB + " " + EIVCityNameB;
                        }
                    }
                }
            }
        }

        private string _EIVAddressLine3B;
        [XafDisplayName("Buyer's Address Line 3")]
        [Index(82), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        [ImmediatePostData]
        public string EIVAddressLine3B
        {
            get { return _EIVAddressLine3B; }
            set
            {
                SetPropertyValue("EIVAddressLine3B", ref _EIVAddressLine3B, value);
                if (!IsLoading)
                {
                    if (EIVStateB != null)
                    {
                        if (EIVAddressLine2B != null && EIVAddressLine3B != null)
                        {
                            BillingAddressfield = EIVAddressLine1B + Environment.NewLine + EIVAddressLine2B + Environment.NewLine + EIVAddressLine3B
                                + Environment.NewLine + EIVPostalZoneB + " " + EIVCityNameB + ", "
                                + EIVStateB.Name;
                        }
                        else if (EIVAddressLine2B != null && EIVAddressLine3B == null)
                        {
                            BillingAddressfield = EIVAddressLine1B + Environment.NewLine + EIVAddressLine2B + Environment.NewLine
                                + EIVPostalZoneB + " " + EIVCityNameB + ", "
                                + EIVStateB.Name;
                        }
                        else if (EIVAddressLine2B == null && EIVAddressLine3B != null)
                        {
                            BillingAddressfield = EIVAddressLine1B + Environment.NewLine + EIVAddressLine3B + Environment.NewLine
                                + EIVPostalZoneB + " " + EIVCityNameB + ", "
                                + EIVStateB.Name;
                        }
                        else
                        {
                            BillingAddressfield = EIVAddressLine1B + Environment.NewLine
                                + EIVPostalZoneB + " " + EIVCityNameB + ", "
                                + EIVStateB.Name;
                        }
                    }
                    else
                    {
                        if (EIVAddressLine2B != null && EIVAddressLine3B != null)
                        {
                            BillingAddressfield = EIVAddressLine1B + Environment.NewLine + EIVAddressLine2B + Environment.NewLine + EIVAddressLine3B
                                + Environment.NewLine + EIVPostalZoneB + " " + EIVCityNameB;
                        }
                        else if (EIVAddressLine2B != null && EIVAddressLine3B == null)
                        {
                            BillingAddressfield = EIVAddressLine1B + Environment.NewLine + EIVAddressLine2B + Environment.NewLine
                                + EIVPostalZoneB + " " + EIVCityNameB;
                        }
                        else if (EIVAddressLine2B == null && EIVAddressLine3B != null)
                        {
                            BillingAddressfield = EIVAddressLine1B + Environment.NewLine + EIVAddressLine3B + Environment.NewLine
                                + EIVPostalZoneB + " " + EIVCityNameB;
                        }
                        else
                        {
                            BillingAddressfield = EIVAddressLine1B + Environment.NewLine
                                + EIVPostalZoneB + " " + EIVCityNameB;
                        }
                    }
                }
            }
        }

        private string _EIVPostalZoneB;
        [ImmediatePostData]
        [XafDisplayName("Buyer's Postcode")]
        [Index(83), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public string EIVPostalZoneB
        {
            get { return _EIVPostalZoneB; }
            set
            {
                SetPropertyValue("EIVPostalZoneB", ref _EIVPostalZoneB, value);
                if (!IsLoading)
                {
                    if (EIVStateB != null)
                    {
                        if (EIVAddressLine2B != null && EIVAddressLine3B != null)
                        {
                            BillingAddressfield = EIVAddressLine1B + Environment.NewLine + EIVAddressLine2B + Environment.NewLine + EIVAddressLine3B
                                + Environment.NewLine + EIVPostalZoneB + " " + EIVCityNameB + ", "
                                + EIVStateB.Name;
                        }
                        else if (EIVAddressLine2B != null && EIVAddressLine3B == null)
                        {
                            BillingAddressfield = EIVAddressLine1B + Environment.NewLine + EIVAddressLine2B + Environment.NewLine
                                + EIVPostalZoneB + " " + EIVCityNameB + ", "
                                + EIVStateB.Name;
                        }
                        else if (EIVAddressLine2B == null && EIVAddressLine3B != null)
                        {
                            BillingAddressfield = EIVAddressLine1B + Environment.NewLine + EIVAddressLine3B + Environment.NewLine
                                + EIVPostalZoneB + " " + EIVCityNameB + ", "
                                + EIVStateB.Name;
                        }
                        else
                        {
                            BillingAddressfield = EIVAddressLine1B + Environment.NewLine
                                + EIVPostalZoneB + " " + EIVCityNameB + ", "
                                + EIVStateB.Name;
                        }
                    }
                    else
                    {
                        if (EIVAddressLine2B != null && EIVAddressLine3B != null)
                        {
                            BillingAddressfield = EIVAddressLine1B + Environment.NewLine + EIVAddressLine2B + Environment.NewLine + EIVAddressLine3B
                                + Environment.NewLine + EIVPostalZoneB + " " + EIVCityNameB;
                        }
                        else if (EIVAddressLine2B != null && EIVAddressLine3B == null)
                        {
                            BillingAddressfield = EIVAddressLine1B + Environment.NewLine + EIVAddressLine2B + Environment.NewLine
                                + EIVPostalZoneB + " " + EIVCityNameB;
                        }
                        else if (EIVAddressLine2B == null && EIVAddressLine3B != null)
                        {
                            BillingAddressfield = EIVAddressLine1B + Environment.NewLine + EIVAddressLine3B + Environment.NewLine
                                + EIVPostalZoneB + " " + EIVCityNameB;
                        }
                        else
                        {
                            BillingAddressfield = EIVAddressLine1B + Environment.NewLine
                                + EIVPostalZoneB + " " + EIVCityNameB;
                        }
                    }
                }
            }
        }

        private string _EIVCityNameB;
        [XafDisplayName("Buyer's City*")]
        //[RuleRequiredField(DefaultContexts.Save)]
        [ImmediatePostData]
        [Index(84), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public string EIVCityNameB
        {
            get { return _EIVCityNameB; }
            set
            {
                SetPropertyValue("EIVCityNameB", ref _EIVCityNameB, value);
                if (!IsLoading)
                {
                    if (EIVStateB != null)
                    {
                        if (EIVAddressLine2B != null && EIVAddressLine3B != null)
                        {
                            BillingAddressfield = EIVAddressLine1B + Environment.NewLine + EIVAddressLine2B + Environment.NewLine + EIVAddressLine3B
                                + Environment.NewLine + EIVPostalZoneB + " " + EIVCityNameB + ", "
                                + EIVStateB.Name;
                        }
                        else if (EIVAddressLine2B != null && EIVAddressLine3B == null)
                        {
                            BillingAddressfield = EIVAddressLine1B + Environment.NewLine + EIVAddressLine2B + Environment.NewLine
                                + EIVPostalZoneB + " " + EIVCityNameB + ", "
                                + EIVStateB.Name;
                        }
                        else if (EIVAddressLine2B == null && EIVAddressLine3B != null)
                        {
                            BillingAddressfield = EIVAddressLine1B + Environment.NewLine + EIVAddressLine3B + Environment.NewLine
                                + EIVPostalZoneB + " " + EIVCityNameB + ", "
                                + EIVStateB.Name;
                        }
                        else
                        {
                            BillingAddressfield = EIVAddressLine1B + Environment.NewLine
                                + EIVPostalZoneB + " " + EIVCityNameB + ", "
                                + EIVStateB.Name;
                        }
                    }
                    else
                    {
                        if (EIVAddressLine2B != null && EIVAddressLine3B != null)
                        {
                            BillingAddressfield = EIVAddressLine1B + Environment.NewLine + EIVAddressLine2B + Environment.NewLine + EIVAddressLine3B
                                + Environment.NewLine + EIVPostalZoneB + " " + EIVCityNameB;
                        }
                        else if (EIVAddressLine2B != null && EIVAddressLine3B == null)
                        {
                            BillingAddressfield = EIVAddressLine1B + Environment.NewLine + EIVAddressLine2B + Environment.NewLine
                                + EIVPostalZoneB + " " + EIVCityNameB;
                        }
                        else if (EIVAddressLine2B == null && EIVAddressLine3B != null)
                        {
                            BillingAddressfield = EIVAddressLine1B + Environment.NewLine + EIVAddressLine3B + Environment.NewLine
                                + EIVPostalZoneB + " " + EIVCityNameB;
                        }
                        else
                        {
                            BillingAddressfield = EIVAddressLine1B + Environment.NewLine
                                + EIVPostalZoneB + " " + EIVCityNameB;
                        }
                    }
                }
            }
        }

        private vwState _EIVStateB;
        [NoForeignKey]
        [ImmediatePostData]
        [XafDisplayName("Buyer's State*")]
        [Index(85), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public vwState EIVStateB
        {
            get { return _EIVStateB; }
            set
            {
                SetPropertyValue("EIVStateB", ref _EIVStateB, value);
                if (!IsLoading)
                {
                    if (EIVStateB != null)
                    {
                        if (EIVAddressLine2B != null && EIVAddressLine3B != null)
                        {
                            BillingAddressfield = EIVAddressLine1B + Environment.NewLine + EIVAddressLine2B + Environment.NewLine + EIVAddressLine3B
                                + Environment.NewLine + EIVPostalZoneB + " " + EIVCityNameB + ", "
                                + EIVStateB.Name;
                        }
                        else if (EIVAddressLine2B != null && EIVAddressLine3B == null)
                        {
                            BillingAddressfield = EIVAddressLine1B + Environment.NewLine + EIVAddressLine2B + Environment.NewLine
                                + EIVPostalZoneB + " " + EIVCityNameB + ", "
                                + EIVStateB.Name;
                        }
                        else if (EIVAddressLine2B == null && EIVAddressLine3B != null)
                        {
                            BillingAddressfield = EIVAddressLine1B + Environment.NewLine + EIVAddressLine3B + Environment.NewLine
                                + EIVPostalZoneB + " " + EIVCityNameB + ", "
                                + EIVStateB.Name;
                        }
                        else
                        {
                            BillingAddressfield = EIVAddressLine1B + Environment.NewLine
                                + EIVPostalZoneB + " " + EIVCityNameB + ", "
                                + EIVStateB.Name;
                        }
                    }
                    else
                    {
                        if (EIVAddressLine2B != null && EIVAddressLine3B != null)
                        {
                            BillingAddressfield = EIVAddressLine1B + Environment.NewLine + EIVAddressLine2B + Environment.NewLine + EIVAddressLine3B
                                + Environment.NewLine + EIVPostalZoneB + " " + EIVCityNameB;
                        }
                        else if (EIVAddressLine2B != null && EIVAddressLine3B == null)
                        {
                            BillingAddressfield = EIVAddressLine1B + Environment.NewLine + EIVAddressLine2B + Environment.NewLine
                                + EIVPostalZoneB + " " + EIVCityNameB;
                        }
                        else if (EIVAddressLine2B == null && EIVAddressLine3B != null)
                        {
                            BillingAddressfield = EIVAddressLine1B + Environment.NewLine + EIVAddressLine3B + Environment.NewLine
                                + EIVPostalZoneB + " " + EIVCityNameB;
                        }
                        else
                        {
                            BillingAddressfield = EIVAddressLine1B + Environment.NewLine
                                + EIVPostalZoneB + " " + EIVCityNameB;
                        }
                    }
                }
            }
        }

        private vwCountry _EIVCountryB;
        [NoForeignKey]
        [ImmediatePostData]
        [XafDisplayName("Buyer's Country")]
        //[RuleRequiredField(DefaultContexts.Save)]
        [Index(86), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
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
        [ImmediatePostData]
        [XafDisplayName("Recipient's Name")]
        [Index(87), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
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
        [Index(88), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
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
        [Index(89), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
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
        [ImmediatePostData]
        [XafDisplayName("Recipient’s Reg. No. Type")]
        [Index(90), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
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
        [Index(91), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        [ImmediatePostData]
        public string EIVAddressLine1S
        {
            get { return _EIVAddressLine1S; }
            set
            {
                SetPropertyValue("EIVAddressLine1S", ref _EIVAddressLine1S, value);
                if (!IsLoading)
                {
                    if (EIVStateS != null)
                    {
                        if (EIVAddressLine2S != null && EIVAddressLine3S != null)
                        {
                            ShippingAddressfield = EIVAddressLine1S + Environment.NewLine + EIVAddressLine2S + Environment.NewLine + EIVAddressLine3S
                                + Environment.NewLine + EIVPostalZoneS + " " + EIVCityNameS + ", "
                                + EIVStateS.Name;
                        }
                        else if (EIVAddressLine2S != null && EIVAddressLine3S == null)
                        {
                            ShippingAddressfield = EIVAddressLine1S + Environment.NewLine + EIVAddressLine2S + Environment.NewLine
                                + EIVPostalZoneS + " " + EIVCityNameS + ", "
                                + EIVStateS.Name;
                        }
                        else if (EIVAddressLine2B == null && EIVAddressLine3B != null)
                        {
                            ShippingAddressfield = EIVAddressLine1S + Environment.NewLine + EIVAddressLine3S + Environment.NewLine
                                + EIVPostalZoneS + " " + EIVCityNameS + ", "
                                + EIVStateS.Name;
                        }
                        else
                        {
                            ShippingAddressfield = EIVAddressLine1S + Environment.NewLine
                                + EIVPostalZoneS + " " + EIVCityNameS + ", "
                                + EIVStateS.Name;
                        }
                    }
                    else
                    {
                        if (EIVAddressLine2S != null && EIVAddressLine3S != null)
                        {
                            ShippingAddressfield = EIVAddressLine1S + Environment.NewLine + EIVAddressLine2S + Environment.NewLine + EIVAddressLine3S
                                + Environment.NewLine + EIVPostalZoneS + " " + EIVCityNameS;
                        }
                        else if (EIVAddressLine2S != null && EIVAddressLine3S == null)
                        {
                            ShippingAddressfield = EIVAddressLine1S + Environment.NewLine + EIVAddressLine2S + Environment.NewLine
                                + EIVPostalZoneS + " " + EIVCityNameS;
                        }
                        else if (EIVAddressLine2B == null && EIVAddressLine3B != null)
                        {
                            ShippingAddressfield = EIVAddressLine1S + Environment.NewLine + EIVAddressLine3S + Environment.NewLine
                                + EIVPostalZoneS + " " + EIVCityNameS;
                        }
                        else
                        {
                            ShippingAddressfield = EIVAddressLine1S + Environment.NewLine
                                + EIVPostalZoneS + " " + EIVCityNameS;
                        }
                    }
                }
            }
        }

        private string _EIVAddressLine2S;
        [XafDisplayName("Recipient's Address Line 2")]
        [Index(92), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        [ImmediatePostData]
        public string EIVAddressLine2S
        {
            get { return _EIVAddressLine2S; }
            set
            {
                SetPropertyValue("EIVAddressLine2S", ref _EIVAddressLine2S, value);
                if (!IsLoading)
                {
                    if (EIVStateS != null)
                    {
                        if (EIVAddressLine2S != null && EIVAddressLine3S != null)
                        {
                            ShippingAddressfield = EIVAddressLine1S + Environment.NewLine + EIVAddressLine2S + Environment.NewLine + EIVAddressLine3S
                                + Environment.NewLine + EIVPostalZoneS + " " + EIVCityNameS + ", "
                                + EIVStateS.Name;
                        }
                        else if (EIVAddressLine2S != null && EIVAddressLine3S == null)
                        {
                            ShippingAddressfield = EIVAddressLine1S + Environment.NewLine + EIVAddressLine2S + Environment.NewLine
                                + EIVPostalZoneS + " " + EIVCityNameS + ", "
                                + EIVStateS.Name;
                        }
                        else if (EIVAddressLine2B == null && EIVAddressLine3B != null)
                        {
                            ShippingAddressfield = EIVAddressLine1S + Environment.NewLine + EIVAddressLine3S + Environment.NewLine
                                + EIVPostalZoneS + " " + EIVCityNameS + ", "
                                + EIVStateS.Name;
                        }
                        else
                        {
                            ShippingAddressfield = EIVAddressLine1S + Environment.NewLine
                                + EIVPostalZoneS + " " + EIVCityNameS + ", "
                                + EIVStateS.Name;
                        }
                    }
                    else
                    {
                        if (EIVAddressLine2S != null && EIVAddressLine3S != null)
                        {
                            ShippingAddressfield = EIVAddressLine1S + Environment.NewLine + EIVAddressLine2S + Environment.NewLine + EIVAddressLine3S
                                + Environment.NewLine + EIVPostalZoneS + " " + EIVCityNameS;
                        }
                        else if (EIVAddressLine2S != null && EIVAddressLine3S == null)
                        {
                            ShippingAddressfield = EIVAddressLine1S + Environment.NewLine + EIVAddressLine2S + Environment.NewLine
                                + EIVPostalZoneS + " " + EIVCityNameS;
                        }
                        else if (EIVAddressLine2B == null && EIVAddressLine3B != null)
                        {
                            ShippingAddressfield = EIVAddressLine1S + Environment.NewLine + EIVAddressLine3S + Environment.NewLine
                                + EIVPostalZoneS + " " + EIVCityNameS;
                        }
                        else
                        {
                            ShippingAddressfield = EIVAddressLine1S + Environment.NewLine
                                + EIVPostalZoneS + " " + EIVCityNameS;
                        }
                    }
                }
            }
        }

        private string _EIVAddressLine3S;
        [XafDisplayName("Recipient's Address Line 3")]
        [Index(93), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        [ImmediatePostData]
        public string EIVAddressLine3S
        {
            get { return _EIVAddressLine3S; }
            set
            {
                SetPropertyValue("EIVAddressLine3S", ref _EIVAddressLine3S, value);
                if (!IsLoading)
                {
                    if (EIVStateS != null)
                    {
                        if (EIVAddressLine2S != null && EIVAddressLine3S != null)
                        {
                            ShippingAddressfield = EIVAddressLine1S + Environment.NewLine + EIVAddressLine2S + Environment.NewLine + EIVAddressLine3S
                                + Environment.NewLine + EIVPostalZoneS + " " + EIVCityNameS + ", "
                                + EIVStateS.Name;
                        }
                        else if (EIVAddressLine2S != null && EIVAddressLine3S == null)
                        {
                            ShippingAddressfield = EIVAddressLine1S + Environment.NewLine + EIVAddressLine2S + Environment.NewLine
                                + EIVPostalZoneS + " " + EIVCityNameS + ", "
                                + EIVStateS.Name;
                        }
                        else if (EIVAddressLine2B == null && EIVAddressLine3B != null)
                        {
                            ShippingAddressfield = EIVAddressLine1S + Environment.NewLine + EIVAddressLine3S + Environment.NewLine
                                + EIVPostalZoneS + " " + EIVCityNameS + ", "
                                + EIVStateS.Name;
                        }
                        else
                        {
                            ShippingAddressfield = EIVAddressLine1S + Environment.NewLine
                                + EIVPostalZoneS + " " + EIVCityNameS + ", "
                                + EIVStateS.Name;
                        }
                    }
                    else
                    {
                        if (EIVAddressLine2S != null && EIVAddressLine3S != null)
                        {
                            ShippingAddressfield = EIVAddressLine1S + Environment.NewLine + EIVAddressLine2S + Environment.NewLine + EIVAddressLine3S
                                + Environment.NewLine + EIVPostalZoneS + " " + EIVCityNameS;
                        }
                        else if (EIVAddressLine2S != null && EIVAddressLine3S == null)
                        {
                            ShippingAddressfield = EIVAddressLine1S + Environment.NewLine + EIVAddressLine2S + Environment.NewLine
                                + EIVPostalZoneS + " " + EIVCityNameS;
                        }
                        else if (EIVAddressLine2B == null && EIVAddressLine3B != null)
                        {
                            ShippingAddressfield = EIVAddressLine1S + Environment.NewLine + EIVAddressLine3S + Environment.NewLine
                                + EIVPostalZoneS + " " + EIVCityNameS;
                        }
                        else
                        {
                            ShippingAddressfield = EIVAddressLine1S + Environment.NewLine
                                + EIVPostalZoneS + " " + EIVCityNameS;
                        }
                    }
                }
            }
        }

        private string _EIVPostalZoneS;
        [XafDisplayName("Recipient's Postcode")]
        [Index(94), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        [ImmediatePostData]
        public string EIVPostalZoneS
        {
            get { return _EIVPostalZoneS; }
            set
            {
                SetPropertyValue("EIVPostalZoneS", ref _EIVPostalZoneS, value);
                if (!IsLoading)
                {
                    if (EIVStateS != null)
                    {
                        if (EIVAddressLine2S != null && EIVAddressLine3S != null)
                        {
                            ShippingAddressfield = EIVAddressLine1S + Environment.NewLine + EIVAddressLine2S + Environment.NewLine + EIVAddressLine3S
                                + Environment.NewLine + EIVPostalZoneS + " " + EIVCityNameS + ", "
                                + EIVStateS.Name;
                        }
                        else if (EIVAddressLine2S != null && EIVAddressLine3S == null)
                        {
                            ShippingAddressfield = EIVAddressLine1S + Environment.NewLine + EIVAddressLine2S + Environment.NewLine
                                + EIVPostalZoneS + " " + EIVCityNameS + ", "
                                + EIVStateS.Name;
                        }
                        else if (EIVAddressLine2B == null && EIVAddressLine3B != null)
                        {
                            ShippingAddressfield = EIVAddressLine1S + Environment.NewLine + EIVAddressLine3S + Environment.NewLine
                                + EIVPostalZoneS + " " + EIVCityNameS + ", "
                                + EIVStateS.Name;
                        }
                        else
                        {
                            ShippingAddressfield = EIVAddressLine1S + Environment.NewLine
                                + EIVPostalZoneS + " " + EIVCityNameS + ", "
                                + EIVStateS.Name;
                        }
                    }
                    else
                    {
                        if (EIVAddressLine2S != null && EIVAddressLine3S != null)
                        {
                            ShippingAddressfield = EIVAddressLine1S + Environment.NewLine + EIVAddressLine2S + Environment.NewLine + EIVAddressLine3S
                                + Environment.NewLine + EIVPostalZoneS + " " + EIVCityNameS;
                        }
                        else if (EIVAddressLine2S != null && EIVAddressLine3S == null)
                        {
                            ShippingAddressfield = EIVAddressLine1S + Environment.NewLine + EIVAddressLine2S + Environment.NewLine
                                + EIVPostalZoneS + " " + EIVCityNameS;
                        }
                        else if (EIVAddressLine2B == null && EIVAddressLine3B != null)
                        {
                            ShippingAddressfield = EIVAddressLine1S + Environment.NewLine + EIVAddressLine3S + Environment.NewLine
                                + EIVPostalZoneS + " " + EIVCityNameS;
                        }
                        else
                        {
                            ShippingAddressfield = EIVAddressLine1S + Environment.NewLine
                                + EIVPostalZoneS + " " + EIVCityNameS;
                        }
                    }
                }
            }
        }

        private string _EIVCityNameS;
        [XafDisplayName("Recipient's City")]
        //[RuleRequiredField(DefaultContexts.Save)]
        [Index(95), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        [ImmediatePostData]
        public string EIVCityNameS
        {
            get { return _EIVCityNameS; }
            set
            {
                SetPropertyValue("EIVCityNameS", ref _EIVCityNameS, value);
                if (!IsLoading)
                {
                    if (EIVStateS != null)
                    {
                        if (EIVAddressLine2S != null && EIVAddressLine3S != null)
                        {
                            ShippingAddressfield = EIVAddressLine1S + Environment.NewLine + EIVAddressLine2S + Environment.NewLine + EIVAddressLine3S
                                + Environment.NewLine + EIVPostalZoneS + " " + EIVCityNameS + ", "
                                + EIVStateS.Name;
                        }
                        else if (EIVAddressLine2S != null && EIVAddressLine3S == null)
                        {
                            ShippingAddressfield = EIVAddressLine1S + Environment.NewLine + EIVAddressLine2S + Environment.NewLine
                                + EIVPostalZoneS + " " + EIVCityNameS + ", "
                                + EIVStateS.Name;
                        }
                        else if (EIVAddressLine2B == null && EIVAddressLine3B != null)
                        {
                            ShippingAddressfield = EIVAddressLine1S + Environment.NewLine + EIVAddressLine3S + Environment.NewLine
                                + EIVPostalZoneS + " " + EIVCityNameS + ", "
                                + EIVStateS.Name;
                        }
                        else
                        {
                            ShippingAddressfield = EIVAddressLine1S + Environment.NewLine
                                + EIVPostalZoneS + " " + EIVCityNameS + ", "
                                + EIVStateS.Name;
                        }
                    }
                    else
                    {
                        if (EIVAddressLine2S != null && EIVAddressLine3S != null)
                        {
                            ShippingAddressfield = EIVAddressLine1S + Environment.NewLine + EIVAddressLine2S + Environment.NewLine + EIVAddressLine3S
                                + Environment.NewLine + EIVPostalZoneS + " " + EIVCityNameS;
                        }
                        else if (EIVAddressLine2S != null && EIVAddressLine3S == null)
                        {
                            ShippingAddressfield = EIVAddressLine1S + Environment.NewLine + EIVAddressLine2S + Environment.NewLine
                                + EIVPostalZoneS + " " + EIVCityNameS;
                        }
                        else if (EIVAddressLine2B == null && EIVAddressLine3B != null)
                        {
                            ShippingAddressfield = EIVAddressLine1S + Environment.NewLine + EIVAddressLine3S + Environment.NewLine
                                + EIVPostalZoneS + " " + EIVCityNameS;
                        }
                        else
                        {
                            ShippingAddressfield = EIVAddressLine1S + Environment.NewLine
                                + EIVPostalZoneS + " " + EIVCityNameS;
                        }
                    }
                }
            }
        }

        private vwState _EIVStateS;
        [NoForeignKey]
        [ImmediatePostData]
        [XafDisplayName("Recipient's State")]
        [Index(96), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public vwState EIVStateS
        {
            get { return _EIVStateS; }
            set
            {
                SetPropertyValue("EIVStateS", ref _EIVStateS, value);
                if (!IsLoading)
                {
                    if (EIVStateS != null)
                    {
                        if (EIVAddressLine2S != null && EIVAddressLine3S != null)
                        {
                            ShippingAddressfield = EIVAddressLine1S + Environment.NewLine + EIVAddressLine2S + Environment.NewLine + EIVAddressLine3S
                                + Environment.NewLine + EIVPostalZoneS + " " + EIVCityNameS + ", "
                                + EIVStateS.Name;
                        }
                        else if (EIVAddressLine2S != null && EIVAddressLine3S == null)
                        {
                            ShippingAddressfield = EIVAddressLine1S + Environment.NewLine + EIVAddressLine2S + Environment.NewLine
                                + EIVPostalZoneS + " " + EIVCityNameS + ", "
                                + EIVStateS.Name;
                        }
                        else if (EIVAddressLine2B == null && EIVAddressLine3B != null)
                        {
                            ShippingAddressfield = EIVAddressLine1S + Environment.NewLine + EIVAddressLine3S + Environment.NewLine
                                + EIVPostalZoneS + " " + EIVCityNameS + ", "
                                + EIVStateS.Name;
                        }
                        else
                        {
                            ShippingAddressfield = EIVAddressLine1S + Environment.NewLine
                                + EIVPostalZoneS + " " + EIVCityNameS + ", "
                                + EIVStateS.Name;
                        }
                    }
                    else
                    {
                        if (EIVAddressLine2S != null && EIVAddressLine3S != null)
                        {
                            ShippingAddressfield = EIVAddressLine1S + Environment.NewLine + EIVAddressLine2S + Environment.NewLine + EIVAddressLine3S
                                + Environment.NewLine + EIVPostalZoneS + " " + EIVCityNameS;
                        }
                        else if (EIVAddressLine2S != null && EIVAddressLine3S == null)
                        {
                            ShippingAddressfield = EIVAddressLine1S + Environment.NewLine + EIVAddressLine2S + Environment.NewLine
                                + EIVPostalZoneS + " " + EIVCityNameS;
                        }
                        else if (EIVAddressLine2B == null && EIVAddressLine3B != null)
                        {
                            ShippingAddressfield = EIVAddressLine1S + Environment.NewLine + EIVAddressLine3S + Environment.NewLine
                                + EIVPostalZoneS + " " + EIVCityNameS;
                        }
                        else
                        {
                            ShippingAddressfield = EIVAddressLine1S + Environment.NewLine
                                + EIVPostalZoneS + " " + EIVCityNameS;
                        }
                    }
                }
            }
        }

        private vwCountry _EIVCountryS;
        [NoForeignKey]
        [XafDisplayName("Recipient's Country")]
        //[RuleRequiredField(DefaultContexts.Save)]
        [Index(97), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public vwCountry EIVCountryS
        {
            get { return _EIVCountryS; }
            set
            {
                SetPropertyValue("EIVCountryS", ref _EIVCountryS, value);
            }
        }
        // End ver 1.0.18

        private string _AppUser;
        [XafDisplayName("AppUser")]
        [Index(110), VisibleInDetailView(false), VisibleInListView(false), VisibleInLookupListView(false)]
        public string AppUser
        {
            get { return _AppUser; }
            set
            {
                SetPropertyValue("AppUser", ref _AppUser, value);
            }
        }

        [Browsable(false)]
        public bool IsNew
        {
            get
            { return Session.IsNewObject(this); }
        }

        [Browsable(false)]
        public bool IsValid
        {
            get
            {
                if (SalesQuotationDetails.GroupBy(x => x.Location).Count() > 1)
                {
                    return true;
                }

                return false;
            }
        }

        [Browsable(false)]
        public bool IsValid1
        {
            get
            {
                foreach (SalesQuotationDetails dtl in this.SalesQuotationDetails)
                {
                    return true;
                }

                return false;
            }
        }

        [Browsable(false)]
        public bool IsValid2
        {
            get
            {
                if (this.Series == null || this.ContactPerson == null)
                {
                    return true;
                }

                if (this.ContactPerson != null)
                {
                    if (this.ContactPerson.SlpCode == -1)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        [Browsable(false)]
        public bool IsValid3
        {
            get
            {
                foreach (SalesQuotationDetails dtl in this.SalesQuotationDetails)
                {
                   if (dtl.Total <= 0)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        [Browsable(false)]
        public bool IsValid4
        {
            get
            {
                foreach (SalesQuotationDetails dtl in this.SalesQuotationDetails)
                {
                    // Start ver 1.0.15
                    //if (dtl.Quantity > (decimal)dtl.Available.InStock)
                    if (dtl.Quantity > dtl.Available)
                    // Start ver 1.0.15
                    {
                        if (this.Series.SeriesName != "BackOrdP" && this.Series.SeriesName != "BackOrdS")
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        [Browsable(false)]
        public bool IsValid5
        {
            get
            {
                if (this.Customer.GroupName != "Trade Debtor - Cash" && this.Series.SeriesName.ToUpper() == "CASH")
                {
                    return true;
                }

                return false;
            }
        }

        [Browsable(false)]
        public bool IsValid6
        {
            get
            {
                if (this.ContactPerson != null)
                {
                    if (this.ContactPerson.Active == "N")
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        // Start ver 1.0.7
        [Browsable(false)]
        public bool IsValid7
        {
            get
            {
                if (this.Transporter.U_Type != "OC" || this.Transporter.U_Type == "OS")
                {
                    // Start ver 1.0.18
                    //if (this.BillingAddressfield == null || this.ShippingAddressfield == null)
                    // Start ver 1.0.22
                    //if (this.EIVAddressLine1B == null || this.EIVAddressLine1S == null)
                    if (string.IsNullOrEmpty(this.EIVAddressLine1B) || string.IsNullOrEmpty(this.EIVAddressLine1S))
                    // End ver 1.0.22
                    // End ver 1.0.18
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        [Browsable(false)]
        public bool IsValid8
        {
            get
            {
                if(this.Priority == null)
                {
                    return true;
                }

                return false;
            }
        }
        // End ver 1.0.7

        // Start ver 1.0.18
        [Browsable(false)]
        public bool IsValid9
        {
            get
            {
                if (this.EIVConsolidate != null)
                {
                    if (this.EIVConsolidate.Code == "Y")
                    {
                        // Start ver 1.0.22
                        //if (this.EIVBuyerTIN == null || this.EIVBuyerRegNum == null)
                        if (string.IsNullOrEmpty(this.EIVBuyerTIN) || string.IsNullOrEmpty(this.EIVBuyerRegNum))
                        // End ver 1.0.22
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        [Browsable(false)]
        public bool IsValid10
        {
            get
            {
                if (this.EIVConsolidate != null)
                {
                    if (this.EIVConsolidate.Code == "Y")
                    {
                        // Start ver 1.0.22
                        //if (this.EIVShippingTin == null && this.EIVShippingRegNum == null)
                        if (string.IsNullOrEmpty(this.EIVShippingTin) && string.IsNullOrEmpty(this.EIVShippingRegNum))
                        // End ver 1.0.22
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        [Browsable(false)]
        public bool IsValid11
        {
            get
            {
                if (this.EIVConsolidate != null)
                {
                    if (this.EIVConsolidate.Code == "Y")
                    {
                        if (this.EIVBuyerRegNum != null && this.EIVBuyerRegTyp == null)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        [Browsable(false)]
        public bool IsValid12
        {
            get
            {
                if (this.EIVConsolidate != null)
                {
                    if (this.EIVConsolidate.Code == "Y")
                    {
                        if (this.EIVShippingRegNum != null && this.EIVShippingRegTyp == null)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        [Browsable(false)]
        public bool IsValid13
        {
            get
            {
                if (this.EIVConsolidate != null)
                {
                    if (this.EIVConsolidate.Code == "Y")
                    {
                        if (this.EIVCountryB != null)
                        {
                            if (this.EIVCountryB.Code == "MY" && this.EIVStateB == null)
                            {
                                return true;
                            }
                        }
                    }
                }

                return false;
            }
        }

        [Browsable(false)]
        public bool IsValid14
        {
            get
            {
                if (this.EIVConsolidate != null)
                {
                    if (this.EIVConsolidate.Code == "Y")
                    {
                        if (this.EIVCountryS != null && (this.EIVShippingTin != null || this.EIVShippingRegNum != null))
                        {
                            if (this.EIVCountryS.Code == "MY" && this.EIVStateS == null)
                            {
                                return true;
                            }
                        }
                    }
                }

                return false;
            }
        }

        [Browsable(false)]
        public bool IsValid15
        {
            get
            {
                if (this.EIVConsolidate != null)
                {
                    if (this.EIVConsolidate.Code == "Y" && string.IsNullOrEmpty(EIVBuyerEmail))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        [Browsable(false)]
        public bool IsValid16
        {
            get
            {
                if (this.EIVConsolidate != null)
                {
                    if (this.EIVConsolidate.Code == "Y")
                    {
                        if (this.EIVType == null || this.EIVFreqSync == null ||
                            // Start ver 1.0.22
                            //this.EIVAddressLine1B == null || this.EIVCityNameB == null || 
                            string.IsNullOrEmpty(this.EIVAddressLine1B) || string.IsNullOrEmpty(this.EIVCityNameB) || 
                            // End ver 1.0.22
                            this.EIVCountryB == null || 
                            // Start ver 1.0.22
                            //this.EIVBuyerContact == null
                            string.IsNullOrEmpty(this.EIVBuyerContact)
                            // End ver 1.0.22
                            )
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        [Browsable(false)]
        public bool IsValid17
        {
            get
            {
                if (this.EIVConsolidate != null)
                {
                    if (this.EIVConsolidate.Code == "Y")
                    {
                        if (this.EIVShippingTin != null || this.EIVShippingRegNum != null)
                        {
                            // Start ver 1.0.22
                            //if (this.EIVAddressLine1S == null || this.EIVCityNameS == null || this.EIVCountryS == null)
                            if (string.IsNullOrEmpty(this.EIVAddressLine1S) || string.IsNullOrEmpty(this.EIVCityNameS) || this.EIVCountryS == null)
                            // End ver 1.0.22
                            {
                                return true;
                            }
                        }
                    }
                }

                return false;
            }
        }

        [Browsable(false)]
        public bool IsValid18
        {
            get
            {
                if (this.ShippingAddress == null)
                {
                    if (string.IsNullOrEmpty(EIVShippingName))
                    {
                        return true;
                    }
                }

                return false;
            }
        }
        // End ver 1.0.18

        [Association("SalesQuotation-SalesQuotationDetails")]
        [XafDisplayName("Items")]
        public XPCollection<SalesQuotationDetails> SalesQuotationDetails
        {
            get { return GetCollection<SalesQuotationDetails>("SalesQuotationDetails"); }
        }

        [Association("SalesQuotation-SalesQuotationDocTrail")]
        [XafDisplayName("Status History")]
        public XPCollection<SalesQuotationDocTrail> SalesQuotationDocTrail
        {
            get { return GetCollection<SalesQuotationDocTrail>("SalesQuotationDocTrail"); }
        }

        [Association("SalesQuotation-SalesQuotationAppStage")]
        [XafDisplayName("Approval Stage")]
        public XPCollection<SalesQuotationAppStage> SalesQuotationAppStage
        {
            get { return GetCollection<SalesQuotationAppStage>("SalesQuotationAppStage"); }
        }

        [Association("SalesQuotation-SalesQuotationAppStatus")]
        [XafDisplayName("Approval Status")]
        public XPCollection<SalesQuotationAppStatus> SalesQuotationAppStatus
        {
            get { return GetCollection<SalesQuotationAppStatus>("SalesQuotationAppStatus"); }
        }

        [Association("SalesQuotation-SalesQuotationAttachment")]
        [XafDisplayName("Attachment")]
        public XPCollection<SalesQuotationAttachment> SalesQuotationAttachment
        {
            get { return GetCollection<SalesQuotationAttachment>("SalesQuotationAttachment"); }
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
                    SalesQuotationDocTrail ds = new SalesQuotationDocTrail(Session);
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
                    this.SalesQuotationDocTrail.Add(ds);
                }
            }
        }
    }
}