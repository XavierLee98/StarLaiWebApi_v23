using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web.UI;
using DevExpress.ExpressApp.Templates;
using DevExpress.ExpressApp.Web;
using DevExpress.ExpressApp.Web.Templates;
using DevExpress.ExpressApp.Web.Templates.ActionContainers;

public partial class Default : BaseXafPage {

    const string LastActivityTag = "LastActivity";
    int ActivityTimeout = int.Parse(ConfigurationManager.AppSettings["ActivityTimeoutSec"].ToString());

    protected override ContextActionsMenu CreateContextActionsMenu() {
        return new ContextActionsMenu(this, "Edit", "RecordEdit", "ObjectsCreation", "ListView", "Reports");
    }
    public override Control InnerContentPlaceHolder {
        get {
            return Content;
        }
    }

    protected DateTime LastActivity
    {
        get
        {
            if (Session[LastActivityTag] != null)
            {
                return (DateTime)Session[LastActivityTag];
            }
            else
            {
                return DateTime.Now;
            }
        }
        set { Session[LastActivityTag] = value; }
    }
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        if (System.Web.HttpContext.Current.Request.Form["__CALLBACKID"] != ASPxCallback1.ClientID)
        {
            LastActivity = DateTime.Now;
        }
    }
    protected void ASPxCallback1_Callback(object source, DevExpress.Web.CallbackEventArgs e)
    {
        if (DevExpress.ExpressApp.Web.WebWindow.CurrentRequestWindow is DevExpress.ExpressApp.Web.PopupWindow) return;
        if (DateTime.Now.Subtract(LastActivity).TotalSeconds > ActivityTimeout)
        {
            WebApplication.Instance.LogOff();
        }
    }
}