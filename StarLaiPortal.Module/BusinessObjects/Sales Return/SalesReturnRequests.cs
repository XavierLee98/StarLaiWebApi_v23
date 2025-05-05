using DevExpress.Data;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using StarLaiPortal.Module.BusinessObjects.Setup;
using StarLaiPortal.Module.BusinessObjects.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

// 2023-09-25 change date format ver 1.0.10
// 2023-10-05 add payment method for sales return ver 1.0.10
// 2024-01-29 default payment method credit note ver 1.0.14
// 2024-06-12 - e-invoice - ver 1.0.18
// 2025-01-23 - new enhancement - ver 1.0.22

namespace StarLaiPortal.Module.BusinessObjects.Sales_Return
{
    [DefaultClassOptions]
    [XafDisplayName("Sales Return Request")]
    [NavigationItem("Sales Return")]
    [DefaultProperty("DocNum")]
    [Appearance("HideNew", AppearanceItemType = "Action", TargetItems = "New", Context = "SalesReturnRequests_ListView_Approval", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide)]
    [Appearance("HideNew1", AppearanceItemType = "Action", TargetItems = "New", Criteria = "(AppStatus in (2))", Context = "SalesReturnRequests_DetailView_Approval", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide)]

    [Appearance("HideEdit", AppearanceItemType.Action, "True", TargetItems = "SwitchToEditMode; Edit", Criteria = "not (Status in (0))", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideEdit1", AppearanceItemType.Action, "True", TargetItems = "SwitchToEditMode; Edit", Criteria = "(AppStatus in (2))", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]

    [Appearance("HideDelete", AppearanceItemType.Action, "True", TargetItems = "Delete", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSubmit", AppearanceItemType.Action, "True", TargetItems = "SubmitSRR", Criteria = "not (Status in (0))", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSubmit1", AppearanceItemType.Action, "True", TargetItems = "SubmitSRR", Criteria = "(AppStatus in (2))", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]

    [Appearance("HideCancel", AppearanceItemType.Action, "True", TargetItems = "CancelSRR", Criteria = "not (Status in (0, 1))", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideCancel1", AppearanceItemType.Action, "True", TargetItems = "CancelSRR", Criteria = "(AppStatus in (2))", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]

    [Appearance("HideCopyTo", AppearanceItemType.Action, "True", TargetItems = "SRRCopyToSR", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "SalesReturnRequests_DetailView_Approval")]
    [Appearance("HideCopyTo1", AppearanceItemType.Action, "True", TargetItems = "SRRCopyToSR", Criteria = "(not (Status in (1))) or ((Status in (1)) and (not AppStatus in (0, 1)))", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideCopyTo2", AppearanceItemType.Action, "True", TargetItems = "SRRCopyToSR", Criteria = "CopyTo = 1", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]

    [Appearance("HideItemInq", AppearanceItemType.Action, "True", TargetItems = "SRRInquiryItem", Criteria = "Customer = null or IsValid4 = 1 or IsValid11 = 1 or IsValid7", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]

    // Start ver 1.0.18
    [RuleCriteria("EIVSRRBilling", DefaultContexts.Save, "IsValid4 = 0", "Please fill in Buyer TIN and Buyer Reg. Num.")]
    //[RuleCriteria("EIVSRRShipping", DefaultContexts.Save, "IsValid5 = 0", "Shipping TIN and Shipping Reg. Num. must fill one of them.")]

    [RuleCriteria("EIVSRRBillingType", DefaultContexts.Save, "IsValid6 = 0", "Please fill in Buyer Reg. Type.")]
    [RuleCriteria("EIVSRRShippingType", DefaultContexts.Save, "IsValid7 = 0", "Please fill in Shipping Reg. Type.")]

    [RuleCriteria("EIVSRRBillingState", DefaultContexts.Save, "IsValid8 = 0", "Please fill in Buyer State.")]
    [RuleCriteria("EIVSRRShippingState", DefaultContexts.Save, "IsValid9 = 0", "Please fill in Shipping State.")]

    [RuleCriteria("EIVSRREmail", DefaultContexts.Save, "IsValid10 = 0", "Please fill in email address.")]

    [RuleCriteria("EIVSRREIVBMandatory", DefaultContexts.Save, "IsValid11 = 0", "Please fill in EIV mandatory field. (EIV Type / Sync. Freq. / Buyer's Name/ " +
        "Buyer's Address Line 1 / Buyer's Country / Buyer's City / Contact No.")]

    [RuleCriteria("EIVSRREIVSMandatory", DefaultContexts.Save, "IsValid12 = 0", "Recipient's Address Line 1 / Recipient's City / Recipient's Country")]
    // End ver 1.0.18

    public class SalesReturnRequests : XPObject
    { // Inherit from a different class to provide a custom primary key, concurrency and deletion behavior, etc. (https://documentation.devexpress.com/eXpressAppFramework/CustomDocument113146.aspx).
        // Use CodeRush to create XPO classes and properties with a few keystrokes.
        // https://docs.devexpress.com/CodeRushForRoslyn/118557
        public SalesReturnRequests(Session session)
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
            AppStatus = ApprovalStatusType.Not_Applicable;
            DocType = DocTypeList.SRR;

            // Start ver 1.0.14
            PaymentMethod = SRPaymentMethod.CreditNote;
            // End ver 1.0.14

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
        [Index(301), VisibleInListView(false), VisibleInDetailView(false), VisibleInLookupListView(false)]
        // Start ver 1.0.10
        [ModelDefault("DisplayFormat", "{0: dd/MM/yyyy hh:mm tt}")]
        // End ver 1.0.10
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
        [Index(0), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(false)]
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
        [DataSourceCriteria("ValidFor = 'Y' and CardType = 'C'")]
        [Appearance("Customer", Enabled = false, Criteria = "IsValid")]
        [Index(3), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(false)]
        public vwBusniessPartner Customer
        {
            get { return _Customer; }
            set
            {
                SetPropertyValue("Customer", ref _Customer, value);
                if (!IsLoading && value != null)
                {
                    CustomerName = Customer.BPName;
                    Currency = Customer.Currency;
                    if (Customer.SlpCode != null)
                    {
                        Salesperson = Session.FindObject<vwSalesPerson>(CriteriaOperator.Parse("SlpCode = ?", Customer.SlpCode.SlpCode));
                    }
                    BillingAddress = Session.FindObject<vwBillingAddress>(CriteriaOperator.Parse("AddressKey = ? and CardCode = ?"
                      , Customer.BillToDef, Customer.BPCode));
                    ShippingAddress = Session.FindObject<vwShippingAddress>(CriteriaOperator.Parse("AddressKey = ? and CardCode = ?"
                        , Customer.ShipToDef, Customer.BPCode));

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
                }
                else if (!IsLoading && value == null)
                {
                    BillingAddress = null;
                    ShippingAddress = null;
                    CustomerName = null;
                    Currency = null;
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
        [XafDisplayName("Customer Name")]
        //[Appearance("CustomerName", Enabled = false)]
        [Index(5), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(false)]
        public string CustomerName
        {
            get { return _CustomerName; }
            set
            {
                SetPropertyValue("CustomerName", ref _CustomerName, value);
            }
        }

        private vwSalesPerson _Salesperson;
        [NoForeignKey]
        [XafDisplayName("Salesperson")]
        [LookupEditorMode(LookupEditorMode.AllItems)]
        [DataSourceCriteria("Active = 'Y'")]
        [RuleRequiredField(DefaultContexts.Save)]
        [Index(6), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(false)]
        public vwSalesPerson Salesperson
        {
            get { return _Salesperson; }
            set
            {
                SetPropertyValue("Salesperson", ref _Salesperson, value);
            }
        }

        private vwTransporter _Transporter;
        [NoForeignKey]
        [XafDisplayName("Transporter")]
        [Index(8), VisibleInListView(false), VisibleInDetailView(false), VisibleInLookupListView(false)]
        public vwTransporter Transporter
        {
            get { return _Transporter; }
            set
            {
                SetPropertyValue("Transporter", ref _Transporter, value);
            }
        }

        private vwReasonCode _ReasonCode;
        [NoForeignKey]
        [ImmediatePostData]
        [DataSourceCriteria("Type = 'SalesReturn'")]
        //[RuleRequiredField(DefaultContexts.Save)]
        [XafDisplayName("Reason Code")]
        [Index(9), VisibleInListView(true), VisibleInDetailView(true), VisibleInLookupListView(true)]
        public vwReasonCode ReasonCode
        {
            get { return _ReasonCode; }
            set
            {
                SetPropertyValue("ReasonCode", ref _ReasonCode, value);
            }
        }

        private DateTime _DocDate;
        [XafDisplayName("Doc Date")]
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

        private ApprovalStatusType _AppStatus;
        [XafDisplayName("Approval Status")]
        [Appearance("AppStatus", Enabled = false)]
        [Index(16), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(false)]
        public ApprovalStatusType AppStatus
        {
            get { return _AppStatus; }
            set
            {
                SetPropertyValue("AppStatus", ref _AppStatus, value);
            }
        }

        private vwBillingAddress _BillingAddress;
        [NoForeignKey]
        [ImmediatePostData]
        [LookupEditorMode(LookupEditorMode.AllItems)]
        [DataSourceCriteria("CardCode = '@this.Customer.BPCode'")]
        [XafDisplayName("Billing Address")]
        [Index(18), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
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
        [Index(20), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
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
        [LookupEditorMode(LookupEditorMode.AllItems)]
        [DataSourceCriteria("CardCode = '@this.Customer.BPCode'")]
        [XafDisplayName("Shipping Address")]
        [Index(23), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
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
        [Index(25), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public string ShippingAddressfield
        {
            get { return _ShippingAddressfield; }
            set
            {
                SetPropertyValue("ShippingAddressfield", ref _ShippingAddressfield, value);
            }
        }

        private string _Currency;
        [ImmediatePostData]
        [XafDisplayName("Currency")]
        [Appearance("Currency", Enabled = false)]
        [Index(28), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
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
        [ImmediatePostData]
        [DbType("numeric(18,6)")]
        [ModelDefault("DisplayFormat", "{0:n2}")]
        [XafDisplayName("Currency Rate")]
        [Appearance("CurrencyRate", Enabled = false)]
        [Index(30), VisibleInListView(true), VisibleInDetailView(false), VisibleInLookupListView(false)]
        public decimal CurrencyRate
        {
            get { return _CurrencyRate; }
            set
            {
                SetPropertyValue("CurrencyRate", ref _CurrencyRate, value);
            }
        }

        // Start ver 1.0.10
        private SRPaymentMethod _PaymentMethod;
        [XafDisplayName("Payment Method")]
        [Index(33), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(false)]
        public SRPaymentMethod PaymentMethod
        {
            get { return _PaymentMethod; }
            set
            {
                SetPropertyValue("PaymentMethod", ref _PaymentMethod, value);
            }
        }
        // End ver 1.0.10

        // Start ver 1.0.18
        private vwYesNo _EIVConsolidate;
        [NoForeignKey]
        [XafDisplayName("Require E-Invoice")]
        [RuleRequiredField(DefaultContexts.Save)]
        [Appearance("EIVConsolidate", Enabled = false, Criteria = "Customer.GroupName != 'Trade Debtor - Cash'")]
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
        [ImmediatePostData]
        [XafDisplayName("E-Invoice Type")]
        //[RuleRequiredField(DefaultContexts.Save)]
        [Appearance("EIVType", Enabled = false, Criteria = "Customer.GroupName != 'Trade Debtor - Cash'")]
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
        [ImmediatePostData]
        [XafDisplayName("Sync. Freq.")]
        //[RuleRequiredField(DefaultContexts.Save)]
        [Appearance("EIVFreqSync", Enabled = false, Criteria = "Customer.GroupName != 'Trade Debtor - Cash'")]
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
        [XafDisplayName("Buyer's TIN No*")]
        [ImmediatePostData]
        [Size(14)]
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
        [XafDisplayName("Registration No.*")]
        [ImmediatePostData]
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
        [ImmediatePostData]
        [XafDisplayName("Registration Type*")]
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
        [XafDisplayName("Contact No.*")]
        [ImmediatePostData]
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
        [XafDisplayName("Buyer's Address Line 1*")]
        //[RuleRequiredField(DefaultContexts.Save)]
        [Index(60), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
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
        //[XafDisplayName("Buyer's Address Line 2")]
        [Index(61), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
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
        [Index(62), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
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
        [Index(63), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
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
        [Index(64), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        [ImmediatePostData]
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
        [Index(65), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
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
        [ImmediatePostData]
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
        [Index(72), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
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
        [Index(73), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
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
        [ImmediatePostData]
        [XafDisplayName("Recipient's Postcode")]
        [Index(74), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
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
        [Index(75), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
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
        [Index(76), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
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

        private string _AppUser;
        [XafDisplayName("AppUser")]
        [Index(78), VisibleInDetailView(false), VisibleInListView(false), VisibleInLookupListView(false)]
        public string AppUser
        {
            get { return _AppUser; }
            set
            {
                SetPropertyValue("AppUser", ref _AppUser, value);
            }
        }

        private string _Reference;
        [XafDisplayName("AR Invoice No.")]
        [Index(79), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(false)]
        public string Reference
        {
            get { return _Reference; }
            set
            {
                SetPropertyValue("Reference", ref _Reference, value);
            }
        }

        private string _Remarks;
        [XafDisplayName("Remarks")]
        [Index(80), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public string Remarks
        {
            get { return _Remarks; }
            set
            {
                SetPropertyValue("Remarks", ref _Remarks, value);
            }
        }

        private bool _CopyTo;
        [XafDisplayName("CopyTo")]
        [Index(81), VisibleInDetailView(false), VisibleInListView(false), VisibleInLookupListView(false)]
        public bool CopyTo
        {
            get { return _CopyTo; }
            set
            {
                SetPropertyValue("CopyTo", ref _CopyTo, value);
            }
        }

        [Browsable(false)]
        [ImmediatePostData]
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
                foreach (SalesReturnRequestDetails dtl in this.SalesReturnRequestDetails)
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
                foreach (SalesReturnRequestDetails dtl in this.SalesReturnRequestDetails)
                {
                    if (dtl.ReasonCode == null)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        [Browsable(false)]
        public bool IsValid2
        {
            get
            {
                foreach (SalesReturnRequestDetails dtl in this.SalesReturnRequestDetails)
                {
                    if (dtl.RtnQuantity <= 0)
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
                foreach (SalesReturnRequestDetails dtl in this.SalesReturnRequestDetails)
                {
                    if (dtl.IsValid1 == true)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        // Start ver 1.0.18
        [Browsable(false)]
        public bool IsValid4
        {
            get
            {
                if (this.EIVConsolidate != null)
                {
                    if (this.EIVConsolidate.Code == "Y")
                    {
                        // Start ver 1.0.22
                        //if (this.EIVBuyerTIN == null && this.EIVBuyerRegNum == null)
                        if (string.IsNullOrEmpty(this.EIVBuyerTIN) && string.IsNullOrEmpty(this.EIVBuyerRegNum))
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
        public bool IsValid5
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
        public bool IsValid6
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
        public bool IsValid7
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
        public bool IsValid8
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
        public bool IsValid9
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
        public bool IsValid10
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
        public bool IsValid11
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
        public bool IsValid12
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
        // End ver 1.0.18

        [Association("SalesReturnRequests-SalesReturnRequestDetails")]
        [XafDisplayName("Content")]
        [Appearance("SalesReturnRequestDetails", Enabled = false, Criteria = "IsNew")]
        public XPCollection<SalesReturnRequestDetails> SalesReturnRequestDetails
        {
            get { return GetCollection<SalesReturnRequestDetails>("SalesReturnRequestDetails"); }
        }

        [Association("SalesReturnRequests-SalesReturnRequestDocTrail")]
        [XafDisplayName("Status History")]
        public XPCollection<SalesReturnRequestDocTrail> SalesReturnRequestDocTrail
        {
            get { return GetCollection<SalesReturnRequestDocTrail>("SalesReturnRequestDocTrail"); }
        }

        [Association("SalesReturnRequests-SalesReturnRequestAppStage")]
        [XafDisplayName("Approval Stage")]
        public XPCollection<SalesReturnRequestAppStage> SalesReturnRequestAppStage
        {
            get { return GetCollection<SalesReturnRequestAppStage>("SalesReturnRequestAppStage"); }
        }

        [Association("SalesReturnRequests-SalesReturnRequestAppStatus")]
        [XafDisplayName("Approval Status")]
        public XPCollection<SalesReturnRequestAppStatus> SalesReturnRequestAppStatus
        {
            get { return GetCollection<SalesReturnRequestAppStatus>("SalesReturnRequestAppStatus"); }
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
                    SalesReturnRequestDocTrail ds = new SalesReturnRequestDocTrail(Session);
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
                    this.SalesReturnRequestDocTrail.Add(ds);
                }
            }
        }
    }
}