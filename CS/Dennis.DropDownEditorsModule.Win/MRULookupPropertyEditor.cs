using System;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Win.Editors;
using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;

namespace Dennis.DropDownEditorsModule.Win {
    [PropertyEditor(typeof(IXPSimpleObject))]
    public class MRULookupPropertyEditor : DXPropertyEditor, IComplexPropertyEditor {
        protected bool collectionLoadedCore = false;
        private string valueStringCore = string.Empty;
        private ObjectSpace objectSpaceCore = null;
        private XafApplication applicationCore = null;
        private MRUEdit controlCore = null;

        public MRULookupPropertyEditor(Type objectType, DictionaryNode info): base(objectType, info) {}
        public new MRUEdit Control {
            get {
                return controlCore;
            }
        }
        protected ObjectSpace ObjectSpace {
            get { return objectSpaceCore; }
        }
        protected XafApplication Application {
            get { return applicationCore; }
        }
        protected IMemberInfo DefaultMember {
            get { return MemberInfo.MemberTypeInfo.DefaultMember; }
        }
        protected bool ShouldLoadCollection {
            get { return (!Object.ReferenceEquals(CurrentObject, null)) && (!this.ReadOnly) && (!collectionLoadedCore); }
        }
        protected override object GetControlValueCore() {
            if (ShouldLoadCollection) {
                foreach (IXPSimpleObject obj in objectSpaceCore.CreateCollection(MemberInfo.MemberTypeInfo.Type, null, new SortingCollection(new SortProperty[] { new SortProperty("Name", DevExpress.Xpo.DB.SortingDirection.Ascending) }))) {
                    Control.Properties.Items.Add(DefaultMember.GetValue(obj));
                }
                collectionLoadedCore = true;
            }
            return base.GetControlValueCore();
        }
        protected override object CreateControlCore() {
            controlCore = new MRUEdit();
            controlCore.Properties.DropDownRows = 10;
            controlCore.Validated += new EventHandler(OnControlValidated);
            controlCore.EditValueChanged += new EventHandler(OnControlEditValueChanged);
            controlCore.Properties.TextEditStyle = ReadOnly.ResultValue ? TextEditStyles.DisableTextEditor : TextEditStyles.Standard;
            return controlCore;
        }
        private void OnControlValidated(object sender, EventArgs e) {
            WriteValueCore();
        }
        private void OnControlEditValueChanged(object sender, EventArgs e) {
            valueStringCore = (Control.EditValue != null) ? Control.EditValue.ToString() : null;
        }
        protected override void WriteValueCore() {
            if (!ReadOnly.ResultValue) {
                Type objectType = MemberInfo.MemberTypeInfo.Type;
                IXPSimpleObject obj = null;
                if (!String.IsNullOrEmpty(valueStringCore)) {
                    obj = objectSpaceCore.FindObject(objectType, new BinaryOperator(DefaultMember.Name, valueStringCore)) as IXPSimpleObject;
                    if (obj == null) {
                        obj = objectSpaceCore.CreateObject(objectType) as IXPSimpleObject;
                        DefaultMember.SetValue(obj, valueStringCore);
                    }
                }
                PropertyValue = obj;
            }
        }
        protected override void ReadValueCore() {
            if (PropertyValue != null) {
                valueStringCore = DefaultMember.GetValue(PropertyValue) as string;
            }
            Control.EditValue = valueStringCore;
        }
        protected override void Dispose(bool disposing) {
            if (disposing) {
                controlCore.EditValueChanged -= new EventHandler(OnControlEditValueChanged);
            }
            base.Dispose(disposing);
        }
        void IComplexPropertyEditor.Setup(ObjectSpace objectSpace, XafApplication application) {
            objectSpaceCore = objectSpace;
            applicationCore = application;
        }
    }
}