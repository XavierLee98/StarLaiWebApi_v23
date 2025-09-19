using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Web.Editors.ASPx;
using DevExpress.ExpressApp.Win.Editors;
using DevExpress.Utils;
using DevExpress.Web;
using DevExpress.XtraEditors.Repository;
using System;
using System.Web.UI.WebControls;

namespace StarLaiPortal.Module.Web.Editors
{
    [PropertyEditor(typeof(DateTime), true)]
    public partial class CustomDateTimeEditor : ASPxDateTimePropertyEditor
    {
        public CustomDateTimeEditor(Type objectType, IModelMemberViewItem model) : base(objectType, model) { }

        protected override void SetupControl(WebControl control)
        {
            base.SetupControl(control);

            if (ViewEditMode == ViewEditMode.Edit)
            {
                ASPxDateEdit dateEdit = (ASPxDateEdit)control;
                dateEdit.TimeSectionProperties.Visible = true;
                dateEdit.UseMaskBehavior = true;
            }
        }
    }
}
