using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Web.Editors.ASPx;
using DevExpress.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace StarLaiPortal.Module.Web.Editors
{
    [PropertyEditor(typeof(int), true)]
    public partial class IntPropertyEditor : ASPxIntPropertyEditor
    {
        public IntPropertyEditor(Type objectType, IModelMemberViewItem model) : base(objectType, model) { }

        protected override void SetupControl(WebControl control)
        {
            base.SetupControl(control);

            var spinEditor = control as ASPxSpinEdit;
            if (spinEditor == null) return;
            //spinEditor.HorizontalAlign = HorizontalAlign.Right;
            //spinEditor.SpinButtons.ShowIncrementButtons = false;
            spinEditor.AllowMouseWheel = false;
            spinEditor.SelectInputTextOnClick = true;
            //spinEditor.DecimalPlaces = 2;
            spinEditor.NumberType = SpinEditNumberType.Integer;
        }
    }
}
