using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.ConditionalAppearance;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo;
using StarLaiPortal.Module.BusinessObjects.View;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

// 2023-09-25 change date format ver 1.0.10
// 2024-04-19 set salesperson field to mandatory ver 1.0.15
// 2024-06-12 e-invoice - ver 1.0.18
// 2025-01-23 new enhancement - ver 1.0.22

namespace StarLaiPortal.Module.BusinessObjects.Sales_Refund
{
    [DefaultClassOptions]
    [XafDisplayName("Manual Credit Note Request")]
    [NavigationItem("Manual Credit Note Request")]
    [DefaultProperty("DocNum")]
    [Appearance("HideNew", AppearanceItemType = "Action", TargetItems = "New", Context = "SalesRefundRequests_ListView_Approval", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide)]
    [Appearance("HideNew1", AppearanceItemType = "Action", TargetItems = "New", Criteria = "(AppStatus in (2))", Context = "SalesRefundRequests_DetailView_Approval", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide)]

    [Appearance("HideEdit", AppearanceItemType.Action, "True", TargetItems = "SwitchToEditMode; Edit", Criteria = "not (Status in (0))", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideEdit1", AppearanceItemType.Action, "True", TargetItems = "SwitchToEditMode; Edit", Criteria = "(AppStatus in (2))", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]

    [Appearance("HideDelete", AppearanceItemType.Action, "True", TargetItems = "Delete", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSubmit", AppearanceItemType.Action, "True", TargetItems = "SubmitSFR", Criteria = "not (Status in (0))", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideSubmit1", AppearanceItemType.Action, "True", TargetItems = "SubmitSFR", Criteria = "(AppStatus in (2))", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]

    [Appearance("HideCancel", AppearanceItemType.Action, "True", TargetItems = "CancelSFR", Criteria = "not (Status in (0))", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideCancel1", AppearanceItemType.Action, "True", TargetItems = "CancelSFR", Criteria = "(AppStatus in (2))", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]

    [Appearance("HideCopyTo", AppearanceItemType.Action, "True", TargetItems = "SFRCopyToSF", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "SalesRefundRequests_DetailView_Approval")]
    [Appearance("HideCopyTo1", AppearanceItemType.Action, "True", TargetItems = "SFRCopyToSF", Criteria = "(not (Status in (1))) or ((Status in (1)) and (not AppStatus in (0, 1)))", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]
    [Appearance("HideCopyTo2", AppearanceItemType.Action, "True", TargetItems = "SFRCopyToSF", Criteria = "CopyTo = 1", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]

    [Appearance("HideItemInq", AppearanceItemType.Action, "True", TargetItems = "SFRInquiryItem", Criteria = "Customer = null or Reference = null or IsValid3 = 1 or IsValid10 = 1", Visibility = DevExpress.ExpressApp.Editors.ViewItemVisibility.Hide, Context = "Any")]

    // Start ver 1.0.18
    [RuleCriteria("EIVSFRBilling", DefaultContexts.Save, "IsValid3 = 0", "Please fill in Buyer TIN and Buyer Reg. Num.")]
    //[RuleCriteria("EIVSFRShipping", DefaultContexts.Save, "IsValid4 = 0", "Shipping TIN and Shipping Reg. Num. must fill one of them.")]

    [RuleCriteria("EIVSFRBillingType", DefaultContexts.Save, "IsValid5 = 0", "Please fill in Buyer Reg. Type.")]
    [RuleCriteria("EIVSFRShippingType", DefaultContexts.Save, "IsValid6 = 0", "Please fill in Shipping Reg. Type.")]

    [RuleCriteria("EIVSFRBillingState", DefaultContexts.Save, "IsValid7 = 0", "Please fill in Buyer State.")]
    [RuleCriteria("EIVSFRShippingState", DefaultContexts.Save, "IsValid8 = 0", "Please fill in Shipping State.")]

    //[RuleCriteria("EIVSFREmail", DefaultContexts.Save, "IsValid9 = 0", "Please fill in email address.")]

    [RuleCriteria("EIVSFREIVBMandatory", DefaultContexts.Save, "IsValid10 = 0", "Please fill in EIV mandatory field. (EIV Type / Sync. Freq. / Buyer's Name/ " +
        "Buyer's Address Line 1 / Buyer's Country / Buyer's City / Contact No.")]

    [RuleCriteria("EIVSFREIVSMandatory", DefaultContexts.Save, "IsValid11 = 0", "Recipient's Address Line 1 / Recipient's City / Recipient's Country")]
    // End ver 1.0.18

    public class SalesRefundRequests : XPObject
    { // Inherit from a different class to provide a custom primary key, concurrency and deletion behavior, etc. (https://documentation.devexpress.com/eXpressAppFramework/CustomDocument113146.aspx).
        // Use CodeRush to create XPO classes and properties with a few keystrokes.
        // https://docs.devexpress.com/CodeRushForRoslyn/118557
        public SalesRefundRequests(Session session)
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
            DocType = DocTypeList.SRF;

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
        [LookupEditorMode(LookupEditorMode.AllItems)]
        [DataSourceCriteria("ValidFor = 'Y' and CardType = 'C'")]
        [Appearance("Customer", Enabled = false, Criteria = "not IsNew")]
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
                    if (Customer.SlpCode != null)
                    {
                        ContactPerson = Session.FindObject<vwSalesPerson>(CriteriaOperator.Parse("SlpCode = ?", Customer.SlpCode.SlpCode));
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

                    vwBillingAddress BillingAddress = Session.FindObject<vwBillingAddress>(CriteriaOperator.Parse("AddressKey = ? and CardCode = ?"
                        , Customer.BillToDef, Customer.BPCode));
                    vwShippingAddress ShippingAddress = Session.FindObject<vwShippingAddress>(CriteriaOperator.Parse("AddressKey = ? and CardCode = ?"
                        , Customer.ShipToDef, Customer.BPCode));

                    if (BillingAddress != null)
                    {
                        EIVAddressLine1B = BillingAddress.Street;
                        EIVAddressLine2B = BillingAddress.Block;
                        EIVAddressLine3B = BillingAddress.City;
                        EIVPostalZoneB = BillingAddress.ZipCode;
                        EIVCityNameB = BillingAddress.County;
                        EIVStateB = Session.FindObject<vwState>(CriteriaOperator.Parse("Code = ?", BillingAddress.State));
                        EIVCountryB = Session.FindObject<vwCountry>(CriteriaOperator.Parse("Code = ?", BillingAddress.Country));
                    }
                    else
                    {
                        EIVCountryB = Session.FindObject<vwCountry>(CriteriaOperator.Parse("Code = ?", "MY"));
                    }
                    if (ShippingAddress != null)
                    {
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
                    }
                    else
                    {
                        EIVCountryS = Session.FindObject<vwCountry>(CriteriaOperator.Parse("Code = ?", "MY"));
                    }
                    // End ver 1.0.18
                }
                else if (!IsLoading && value == null)
                {
                    CustomerName = null;
                    ContactPerson = null;
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

                    EIVAddressLine1B = null;
                    EIVAddressLine2B = null;
                    EIVPostalZoneB = null;
                    EIVCityNameB = null;
                    EIVStateB = null;
                    EIVCountryB = Session.FindObject<vwCountry>(CriteriaOperator.Parse("Code = ?", "MY"));
                    EIVAddressLine1S = null;
                    EIVAddressLine2S = null;
                    EIVPostalZoneS = null;
                    EIVCityNameS = null;
                    EIVStateS = null;
                    EIVCountryS = Session.FindObject<vwCountry>(CriteriaOperator.Parse("Code = ?", "MY"));
                    // End ver 1.0.18
                }
            }
        }

        private string _CustomerName;
        [XafDisplayName("Customer Name")]
        [Appearance("CustomerName", Enabled = false)]
        [Index(5), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(false)]
        public string CustomerName
        {
            get { return _CustomerName; }
            set
            {
                SetPropertyValue("CustomerName", ref _CustomerName, value);
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

        private vwSalesPerson _ContactPerson;
        [NoForeignKey]
        [XafDisplayName("Salesperson")]
        [LookupEditorMode(LookupEditorMode.AllItems)]
        [DataSourceCriteria("Active = 'Y'")]
        // Start ver 1.0.15
        [RuleRequiredField(DefaultContexts.Save)]
        // End ver 1.0.15
        [Index(14), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(false)]
        public vwSalesPerson ContactPerson
        {
            get { return _ContactPerson; }
            set
            {
                SetPropertyValue("ContactPerson", ref _ContactPerson, value);
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

        private string _SAPDocNum;
        [XafDisplayName("SAP CN No.")]
        [Appearance("SAPDocNum", Enabled = false)]
        [Index(18), VisibleInDetailView(true), VisibleInListView(true), VisibleInLookupListView(false)]
        public string SAPDocNum
        {
            get { return _SAPDocNum; }
            set
            {
                SetPropertyValue("SAPDocNum", ref _SAPDocNum, value);
            }
        }

        // Start ver 1.0.18
        private vwYesNo _EIVConsolidate;
        [NoForeignKey]
        [XafDisplayName("Require E-Invoice")]
        [RuleRequiredField(DefaultContexts.Save)]
        [Appearance("EIVConsolidate", Enabled = false, Criteria = "Customer.GroupName != 'Trade Debtor - Cash'")]
        [Index(30), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
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
        [Appearance("EIVType", Enabled = false, Criteria = "Customer.GroupName != 'Trade Debtor - Cash'")]
        [Index(31), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
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
        [Appearance("EIVFreqSync", Enabled = false, Criteria = "Customer.GroupName != 'Trade Debtor - Cash'")]
        [Index(32), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
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
        [Index(33), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
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
        [Index(34), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
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
        [Index(35), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
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
        [Index(36), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
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
        [Index(37), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
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
        [Index(38), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
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
        [Index(39), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
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
        [ImmediatePostData]
        //[RuleRequiredField(DefaultContexts.Save)]
        [Index(40), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
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
        [Index(41), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
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
        [Index(42), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
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
        [Index(43), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
        public string EIVPostalZoneB
        {
            get { return _EIVPostalZoneB; }
            set
            {
                SetPropertyValue("EIVPostalZoneB", ref _EIVPostalZoneB, value);
            }
        }

        private string _EIVCityNameB;
        [XafDisplayName("Buyer's City*")]
        //[RuleRequiredField(DefaultContexts.Save)]
        [ImmediatePostData]
        [Index(44), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
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
        [ImmediatePostData]
        [XafDisplayName("Buyer's State*")]
        [Index(45), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
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
        [Index(46), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
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
        [Index(47), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
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
        [Index(48), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
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
        [Index(49), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
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
        [Index(50), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
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
        [Index(51), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
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
        [Index(52), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
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
        [Index(53), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
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
        [Index(54), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
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
        [Index(55), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
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
        [Index(56), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
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
        [Index(57), VisibleInDetailView(true), VisibleInListView(false), VisibleInLookupListView(false)]
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
        [ImmediatePostData]
        [XafDisplayName("AR Invoice No.")]
        [RuleRequiredField(DefaultContexts.Save)]
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

        private bool _Sap;
        [XafDisplayName("Sap")]
        [Index(82), VisibleInDetailView(false), VisibleInListView(false), VisibleInLookupListView(false)]
        public bool Sap
        {
            get { return _Sap; }
            set
            {
                SetPropertyValue("Sap", ref _Sap, value);
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
                foreach (SalesRefundReqDetails dtl in this.SalesRefundReqDetails)
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
                if (this.ContactPerson.Active == "N")
                {
                    return true;
                }

                return false;
            }
        }

        // Start ver 1.0.18
        [Browsable(false)]
        public bool IsValid3
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
        public bool IsValid4
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
        public bool IsValid5
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
        public bool IsValid6
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
        public bool IsValid7
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
        public bool IsValid8
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
        public bool IsValid9
        {
            get
            {
                if (this.EIVConsolidate != null)
                {
                    // Start ver 1.0.22
                    //if (this.EIVConsolidate.Code == "Y" && this.EIVBuyerEmail == null)
                    if (this.EIVConsolidate.Code == "Y" && string.IsNullOrEmpty(this.EIVBuyerEmail))
                    // End ver 1.0.22
                    {
                        return true;
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
        public bool IsValid11
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

        [Association("SalesRefundRequests-SalesRefundReqDetails")]
        [XafDisplayName("Content")]
        public XPCollection<SalesRefundReqDetails> SalesRefundReqDetails
        {
            get { return GetCollection<SalesRefundReqDetails>("SalesRefundReqDetails"); }
        }

        [Association("SalesRefundRequests-SalesRefundReqDocTrail")]
        [XafDisplayName("Status History")]
        public XPCollection<SalesRefundReqDocTrail> SalesRefundReqDocTrail
        {
            get { return GetCollection<SalesRefundReqDocTrail>("SalesRefundReqDocTrail"); }
        }

        [Association("SalesRefundRequests-SalesRefundReqAppStage")]
        [XafDisplayName("Approval Stage")]
        public XPCollection<SalesRefundReqAppStage> SalesRefundReqAppStage
        {
            get { return GetCollection<SalesRefundReqAppStage>("SalesRefundReqAppStage"); }
        }

        [Association("SalesRefundRequests-SalesRefundReqAppStatus")]
        [XafDisplayName("Approval Status")]
        public XPCollection<SalesRefundReqAppStatus> SalesRefundReqAppStatus
        {
            get { return GetCollection<SalesRefundReqAppStatus>("SalesRefundReqAppStatus"); }
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
                    SalesRefundReqDocTrail ds = new SalesRefundReqDocTrail(Session);
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
                    this.SalesRefundReqDocTrail.Add(ds);
                }
            }
        }
    }
}