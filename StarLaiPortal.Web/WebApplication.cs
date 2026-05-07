using System;
using DevExpress.ExpressApp;
using System.ComponentModel;
using DevExpress.ExpressApp.Web;
using System.Collections.Generic;
using DevExpress.ExpressApp.Xpo;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.Security.ClientServer;
using StarLaiPortal.Module.BusinessObjects;
using System.Configuration;
using DevExpress.XtraCharts.Native;
using System.Data.SqlClient;
using System.Data;
using System.Web;
using System.Net.Http;

// 2024-04-04 - add login new loginpage - ver 1.0.15
// 2025-11-04 - Add Security control - ver 1.0.26

namespace StarLaiPortal.Web {
    // For more typical usage scenarios, be sure to check out https://docs.devexpress.com/eXpressAppFramework/DevExpress.ExpressApp.Web.WebApplication
    public partial class StarLaiPortalAspNetApplication : WebApplication {
        private DevExpress.ExpressApp.SystemModule.SystemModule module1;
        private DevExpress.ExpressApp.Web.SystemModule.SystemAspNetModule module2;
        private StarLaiPortal.Module.StarLaiPortalModule module3;
        private StarLaiPortal.Module.Web.StarLaiPortalAspNetModule module4;
        private DevExpress.ExpressApp.Security.SecurityModule securityModule1;
        private DevExpress.ExpressApp.Security.SecurityStrategyComplex securityStrategyComplex1;
        private DevExpress.ExpressApp.Security.AuthenticationStandard authenticationStandard1;
        private DevExpress.ExpressApp.AuditTrail.AuditTrailModule auditTrailModule;
        private DevExpress.ExpressApp.Objects.BusinessClassLibraryCustomizationModule objectsModule;
        private DevExpress.ExpressApp.ConditionalAppearance.ConditionalAppearanceModule conditionalAppearanceModule;
        private DevExpress.ExpressApp.FileAttachments.Web.FileAttachmentsAspNetModule fileAttachmentsAspNetModule;
        private DevExpress.ExpressApp.ReportsV2.ReportsModuleV2 reportsModuleV2;
        private DevExpress.ExpressApp.ReportsV2.Web.ReportsAspNetModuleV2 reportsAspNetModuleV2;
        private DevExpress.ExpressApp.Validation.ValidationModule validationModule;
        private DevExpress.ExpressApp.Validation.Web.ValidationAspNetModule validationAspNetModule;

        #region Default XAF configuration options (https://www.devexpress.com/kb=T501418)
        static StarLaiPortalAspNetApplication()
        {
            EnableMultipleBrowserTabsSupport = true;
            DevExpress.ExpressApp.Web.Editors.ASPx.ASPxGridListEditor.AllowFilterControlHierarchy = true;
            DevExpress.ExpressApp.Web.Editors.ASPx.ASPxGridListEditor.MaxFilterControlHierarchyDepth = 3;
            DevExpress.ExpressApp.Web.Editors.ASPx.ASPxCriteriaPropertyEditor.AllowFilterControlHierarchyDefault = true;
            DevExpress.ExpressApp.Web.Editors.ASPx.ASPxCriteriaPropertyEditor.MaxHierarchyDepthDefault = 3;
            DevExpress.Persistent.Base.PasswordCryptographer.EnableRfc2898 = true;
            DevExpress.Persistent.Base.PasswordCryptographer.SupportLegacySha512 = false;
        }
        private void InitializeDefaults()
        {
            LinkNewObjectToParentImmediately = false;
            OptimizedControllersCreation = true;
        }
        #endregion

        public StarLaiPortalAspNetApplication() {
            InitializeComponent();
            InitializeDefaults();

            // Start ver 1.0.15
            this.LoggedOff += Application_LoggedOff;
            // End ver 1.0.15
            // Start ver 1.0.26
            this.LoggedOn += Application_LoggedOn;
            this.LoggingOff += Application_LoggingOff;
            // End ver 1.0.26
        }

        protected override void OnLoggingOn(LogonEventArgs args)
        {
            base.OnLoggingOn(args);

            // Start ver 1.0.15
            ((IDatabaseNameParameter)args.LogonParameters).DatabaseName = ConfigurationManager.AppSettings["PortalDB"].ToString();
            // End ver 1.0.15

            if (String.IsNullOrEmpty(((IDatabaseNameParameter)args.LogonParameters).DatabaseName))
                throw new InvalidOperationException("Please select Company.");
            MSSqlServerChangeDatabaseHelper.UpdateDatabaseName(this, ((IDatabaseNameParameter)args.LogonParameters).DatabaseName);
        }

        //protected override IViewUrlManager CreateViewUrlManager() {
        //    return new ViewUrlManager();
        //}
        protected override void CreateDefaultObjectSpaceProvider(CreateCustomObjectSpaceProviderEventArgs args) {
            args.ObjectSpaceProvider = new SecuredObjectSpaceProvider((SecurityStrategyComplex)Security, GetDataStoreProvider(args.ConnectionString, args.Connection), false);
            args.ObjectSpaceProviders.Add(new NonPersistentObjectSpaceProvider(TypesInfo, null));
            #region Allow Store Proc
            ((SecuredObjectSpaceProvider)args.ObjectSpaceProvider).AllowICommandChannelDoWithSecurityContext = true;
            #endregion
        }
        private IXpoDataStoreProvider GetDataStoreProvider(string connectionString, System.Data.IDbConnection connection) {
            System.Web.HttpApplicationState application = (System.Web.HttpContext.Current != null) ? System.Web.HttpContext.Current.Application : null;
            IXpoDataStoreProvider dataStoreProvider = null;
            if(application != null && application["DataStoreProvider"] != null) {
                dataStoreProvider = application["DataStoreProvider"] as IXpoDataStoreProvider;
            }
            else {
                dataStoreProvider = XPObjectSpaceProvider.GetDataStoreProvider(connectionString, connection, false);
                if(application != null) {
                    application["DataStoreProvider"] = dataStoreProvider;
                }
            }
			return dataStoreProvider;
        }
        private void StarLaiPortalAspNetApplication_DatabaseVersionMismatch(object sender, DevExpress.ExpressApp.DatabaseVersionMismatchEventArgs e) {
#if EASYTEST
            e.Updater.Update();
            e.Handled = true;
#else
            if(System.Diagnostics.Debugger.IsAttached) {
                e.Updater.Update();
                e.Handled = true;
            }
            else {
                string message = "Connection Error";

                if (e.CompatibilityError != null && e.CompatibilityError.Exception != null) {
                    message += "\r\n\r\nInner exception: " + e.CompatibilityError.Exception.Message;
                }
                throw new InvalidOperationException(message);
            }
#endif
        }
        private void InitializeComponent() {
            this.module1 = new DevExpress.ExpressApp.SystemModule.SystemModule();
            this.module2 = new DevExpress.ExpressApp.Web.SystemModule.SystemAspNetModule();
            this.module3 = new StarLaiPortal.Module.StarLaiPortalModule();
            this.module4 = new StarLaiPortal.Module.Web.StarLaiPortalAspNetModule();
            this.securityModule1 = new DevExpress.ExpressApp.Security.SecurityModule();
            this.securityStrategyComplex1 = new DevExpress.ExpressApp.Security.SecurityStrategyComplex();
            this.securityStrategyComplex1.SupportNavigationPermissionsForTypes = false;
            this.authenticationStandard1 = new DevExpress.ExpressApp.Security.AuthenticationStandard();
            this.auditTrailModule = new DevExpress.ExpressApp.AuditTrail.AuditTrailModule();
            this.objectsModule = new DevExpress.ExpressApp.Objects.BusinessClassLibraryCustomizationModule();
            this.conditionalAppearanceModule = new DevExpress.ExpressApp.ConditionalAppearance.ConditionalAppearanceModule();
            this.fileAttachmentsAspNetModule = new DevExpress.ExpressApp.FileAttachments.Web.FileAttachmentsAspNetModule();
            this.reportsModuleV2 = new DevExpress.ExpressApp.ReportsV2.ReportsModuleV2();
            this.reportsAspNetModuleV2 = new DevExpress.ExpressApp.ReportsV2.Web.ReportsAspNetModuleV2();
            this.validationModule = new DevExpress.ExpressApp.Validation.ValidationModule();
            this.validationAspNetModule = new DevExpress.ExpressApp.Validation.Web.ValidationAspNetModule();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            // 
            // securityStrategyComplex1
            // 
            this.securityStrategyComplex1.Authentication = this.authenticationStandard1;
            this.securityStrategyComplex1.RoleType = typeof(DevExpress.Persistent.BaseImpl.PermissionPolicy.PermissionPolicyRole);
            // ApplicationUser descends from PermissionPolicyUser and supports OAuth authentication. For more information, refer to the following help topic: https://docs.devexpress.com/eXpressAppFramework/402197
            // If your application uses PermissionPolicyUser or a custom user type, set the UserType property as follows:
            this.securityStrategyComplex1.UserType = typeof(StarLaiPortal.Module.BusinessObjects.ApplicationUser);
            // 
            // securityModule1
            // 
            this.securityModule1.UserType = typeof(StarLaiPortal.Module.BusinessObjects.ApplicationUser);
            // 
            // authenticationStandard1
            // 
            this.authenticationStandard1.LogonParametersType = typeof(StarLaiPortal.Module.BusinessObjects.CustomLogonParametersForStandardAuthentication);
            // ApplicationUserLoginInfo is only necessary for applications that use the ApplicationUser user type.
            // Comment out the following line if using PermissionPolicyUser or a custom user type.
            this.authenticationStandard1.UserLoginInfoType = typeof(StarLaiPortal.Module.BusinessObjects.ApplicationUserLoginInfo);
            //
            // auditTrailModule
            //
            this.auditTrailModule.AuditDataItemPersistentType = typeof(DevExpress.Persistent.BaseImpl.AuditDataItemPersistent);
            //
            // reportsModuleV2
            //
            this.reportsModuleV2.EnableInplaceReports = true;
            this.reportsModuleV2.ReportDataType = typeof(DevExpress.Persistent.BaseImpl.ReportDataV2);
            this.reportsModuleV2.ShowAdditionalNavigation = true;
            //
            // reportsAspNetModuleV2
            //
            this.reportsAspNetModuleV2.ShowFormatSpecificExportActions = true;
            this.reportsAspNetModuleV2.ReportViewerType = DevExpress.ExpressApp.ReportsV2.Web.ReportViewerTypes.HTML5;
            this.reportsModuleV2.ReportStoreMode = DevExpress.ExpressApp.ReportsV2.ReportStoreModes.XML;
            //
            // validationModule
            //
            this.validationModule.AllowValidationDetailsAccess = false;
            // 
            // StarLaiPortalAspNetApplication
            // 
            this.ApplicationName = "StarLaiPortal";
            this.CheckCompatibilityType = DevExpress.ExpressApp.CheckCompatibilityType.DatabaseSchema;
            this.Modules.Add(this.module1);
            this.Modules.Add(this.module2);
            this.Modules.Add(this.module3);
            this.Modules.Add(this.module4);
            this.Modules.Add(this.securityModule1);
            this.Security = this.securityStrategyComplex1;
            this.Modules.Add(this.auditTrailModule);
            this.Modules.Add(this.objectsModule);
            this.Modules.Add(this.conditionalAppearanceModule);
            this.Modules.Add(this.fileAttachmentsAspNetModule);
            this.Modules.Add(this.reportsModuleV2);
            this.Modules.Add(this.reportsAspNetModuleV2);
            this.Modules.Add(this.validationModule);
            this.Modules.Add(this.validationAspNetModule);
            this.DatabaseVersionMismatch += new System.EventHandler<DevExpress.ExpressApp.DatabaseVersionMismatchEventArgs>(this.StarLaiPortalAspNetApplication_DatabaseVersionMismatch);
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();

        }

        protected override bool OnLogonFailed(object logonParameters, Exception e)
        {
             // Start ver 1.0.26
            string query = "";
            string error = "";
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ToString());

            query = "SELECT ISNULL(AccLocked, 0) FROM [" + ConfigurationManager.AppSettings["PortalDB"].ToString() + "]..PermissionPolicyUser " +
                "WHERE UserName = '" + logonParameters.GetType().GetProperty("UserName").GetValue(logonParameters) + "'";
            if (conn.State == ConnectionState.Open)
            {
                conn.Close();
            }
            conn.Open();
            SqlCommand cmdsecurity = new SqlCommand(query, conn);
            SqlDataReader readersecurity = cmdsecurity.ExecuteReader();
            while (readersecurity.Read())
            {
                if (readersecurity.GetBoolean(0) == true)
                {
                    if (error == "")
                    {
                        error = "Your user ID has been locked. Please contact the system administrator to have it unlocked.";
                    }
                }
            }
            cmdsecurity.Dispose();
            conn.Close();

            if (e.Message != null)
            {
                if (logonParameters.GetType().GetProperty("UserName").GetValue(logonParameters).ToString().ToUpper() != "ADMIN")
                {
                    query = "UPDATE T0 SET T0.LoginFailedCount = ISNULL(T0.LoginFailedCount, 0) + 1, " +
                        "T0.AccLocked = CASE WHEN ISNULL(T0.LoginFailedCount, 0) + 1 >= T1.FailedLoginCount THEN 1 ELSE 0 END " +
                        "FROM [" + ConfigurationManager.AppSettings["PortalDB"].ToString() + "]..PermissionPolicyUser T0 " +
                        "INNER JOIN [" + ConfigurationManager.AppSettings["PortalDB"].ToString() + "]..SecurityControl T1 on T1.OID = 1 " +
                        "WHERE T0.UserName = '" + logonParameters.GetType().GetProperty("UserName").GetValue(logonParameters) + "'";
                    if (conn.State == ConnectionState.Open)
                    {
                        conn.Close();
                    }
                    conn.Open();
                    SqlCommand cmdUpd = new SqlCommand(query, conn);
                    SqlDataReader readerUpd = cmdUpd.ExecuteReader();
                    cmdUpd.Dispose();
                    conn.Close();

                    if (error == "")
                    {
                        error = e.Message;
                    }
                }

                if (error == "")
                {
                    error = e.Message;
                }
            }

            if (error != "")
            {
                query = "INSERT INTO [" + ConfigurationManager.AppSettings["PortalDB"].ToString() + "]..LoginHistory VALUES " +
                    "(GETDATE(), '" + logonParameters.GetType().GetProperty("UserName").GetValue(logonParameters) + "', '', 'Fail', 'Login')";
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
                conn.Open();
                SqlCommand cmdinsHis = new SqlCommand(query, conn);
                SqlDataReader readerinsHis = cmdinsHis.ExecuteReader();
                cmdinsHis.Dispose();
                conn.Close();

                throw new UserFriendlyException(error);
            }
            // End ver 1.0.26

            return false;
        }

        // Start ver 1.0.15
        private void Application_LoggedOff(object sender, EventArgs e)
        {
            Redirect(ConfigurationManager.AppSettings["CommonUrl"].ToString());
        }
        // End ver 1.0.15

        // Start ver 1.0.26
        private void Application_LoggedOn(object sender, EventArgs e)
        {
            string query = "";
            string FromTime = "";
            string ToTime = "";
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ToString());
            ApplicationUser user = (ApplicationUser)SecuritySystem.CurrentUser;

            if (!user.IsUserInRole("Administrators"))
            {
                query = "SELECT AccessRestrictedFrom, AccessRestrictedTo " +
                    "FROM [" + ConfigurationManager.AppSettings["PortalDB"].ToString() + "]..SecurityControl " +
                    "WHERE OID = 1 AND GCRecord is null";
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
                conn.Open();
                SqlCommand cmdsecurity = new SqlCommand(query, conn);
                SqlDataReader readersecurity = cmdsecurity.ExecuteReader();
                while (readersecurity.Read())
                {
                    FromTime = readersecurity.GetString(0);
                    ToTime = readersecurity.GetString(1);
                }
                cmdsecurity.Dispose();
                conn.Close();

                if (!(DateTime.Now.TimeOfDay < TimeSpan.Parse(ToTime) && DateTime.Now.TimeOfDay > TimeSpan.Parse(FromTime)))
                {
                    throw new UserFriendlyException("Access Restricted. Operation portal is available only between " + DateTime.Parse(FromTime).ToString("hh:mm") + " AM " +
                        "and " + DateTime.Parse(ToTime).ToString("hh:mm") + " PM.");
                }

                if (user.AccLocked == true)
                {
                    throw new UserFriendlyException("Your user ID has been locked. Please contact the system administrator to have it unlocked.");
                }
            }

            query = "INSERT INTO [" + ConfigurationManager.AppSettings["PortalDB"].ToString() + "]..LoginHistory VALUES " +
                "(GETDATE(), '" + user.UserName + "', '" + user.Staff.StaffName + "', 'Success', 'Login')";
            if (conn.State == ConnectionState.Open)
            {
                conn.Close();
            }
            conn.Open();
            SqlCommand cmdinsHis = new SqlCommand(query, conn);
            SqlDataReader readerinsHis = cmdinsHis.ExecuteReader();
            cmdinsHis.Dispose();
            conn.Close();

            query = "UPDATE [" + ConfigurationManager.AppSettings["PortalDB"].ToString() + "]..PermissionPolicyUser set LoginFailedCount = 0 " +
                "WHERE UserName = '" + user.UserName + "'";
            if (conn.State == ConnectionState.Open)
            {
                conn.Close();
            }
            conn.Open();
            SqlCommand cmdupdsucc = new SqlCommand(query, conn);
            SqlDataReader readupdsucc = cmdupdsucc.ExecuteReader();
            cmdupdsucc.Dispose();
            conn.Close();
        }

        private void Application_LoggingOff(object sender, LoggingOffEventArgs e)
        {
            string query = "";
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnectionString"].ToString());
            ApplicationUser user = (ApplicationUser)SecuritySystem.CurrentUser;

            if (user != null)
            {
                query = "INSERT INTO [" + ConfigurationManager.AppSettings["PortalDB"].ToString() + "]..LoginHistory VALUES " +
                    "(GETDATE(), '" + user.UserName + "', '" + user.Staff.StaffName + "', 'Success', 'Logout')";
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
                conn.Open();
                SqlCommand cmdinsHis = new SqlCommand(query, conn);
                SqlDataReader readerinsHis = cmdinsHis.ExecuteReader();
                cmdinsHis.Dispose();
                conn.Close();
            }
        }
        // End ver 1.0.26
    }
}
