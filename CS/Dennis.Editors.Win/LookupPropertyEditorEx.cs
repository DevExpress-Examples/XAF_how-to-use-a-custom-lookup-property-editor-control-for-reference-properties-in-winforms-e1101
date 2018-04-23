using System;
using DevExpress.Utils;
using System.ComponentModel;
using DevExpress.ExpressApp;
using DevExpress.Accessibility;
using DevExpress.ExpressApp.Utils;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Win.Core;
using DevExpress.XtraEditors.Drawing;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.ViewInfo;
using DevExpress.XtraEditors.Repository;
using DevExpress.ExpressApp.Win.Editors;
using DevExpress.ExpressApp.Localization;
using DevExpress.XtraEditors.Registrator;

namespace Dennis.Editors.Win {
    [PropertyEditor(typeof(object), EditorAliases.LookupPropertyEditor)]
    public class LookupPropertyEditorEx : DXPropertyEditor, IComplexPropertyEditor {
        private const string AddButtonTag = "AddButtonTag";
        private const string MinusButtonTag = "MinusButtonTag";
        private LookUpEditEx lookup;
        private View newObjectView;
        public new LookUpEditEx Control { get { return lookup; } }
        private LookupEditorHelper helper;
        public LookupPropertyEditorEx(Type objectType, IModelMemberViewItem item) : base(objectType, item) { }
        public LookupEditorHelper Helper {
            get { return helper; }
        }
        protected override object CreateControlCore() {
            lookup = new LookUpEditEx();
            return lookup;
        }
        protected override RepositoryItem CreateRepositoryItem() {
            return new RepositoryItemLookUpEditEx();
        }
        protected override void SetupRepositoryItem(RepositoryItem item) {
            base.SetupRepositoryItem(item);
            RepositoryItemLookUpEditEx properties = (RepositoryItemLookUpEditEx)item;
            properties.Init(DisplayFormat, helper);
            properties.ShowHeader = false;
            //properties.DropDownRows = helper.SmallCollectionItemCount;
            properties.QueryPopUp += properties_QueryPopUp;
            properties.ButtonClick += properties_ButtonClick;
            EditorButton addButton = new EditorButton(ButtonPredefines.Plus);
            addButton.Tag = AddButtonTag;
            addButton.Enabled = AllowEdit.ResultValue;
            EditorButton minusButton = new EditorButton(ButtonPredefines.Minus);
            minusButton.Tag = MinusButtonTag;
            minusButton.Enabled = AllowEdit.ResultValue;
            properties.Buttons.Add(addButton);
            properties.Buttons.Add(minusButton);
            properties.SearchMode = SearchMode.OnlyInPopup;
            properties.NullText = CaptionHelper.NullValueText;
            if (AllowNull)
                properties.AllowNullInput = DefaultBoolean.True;
            properties.TextEditStyle = TextEditStyles.DisableTextEditor;
        }
        void IComplexPropertyEditor.Setup(ObjectSpace objectSpace, XafApplication application) {
            if (helper == null)
                helper = new LookupEditorHelper(application, objectSpace, MemberInfo.MemberTypeInfo, Model);
            helper.SetObjectSpace(objectSpace);
        }
        protected override void Dispose(bool disposing) {
            try {
                if (lookup != null)
                    lookup.QueryPopUp -= properties_QueryPopUp;
            } finally {
                base.Dispose(disposing);
            }
        }
        private void properties_QueryPopUp(object sender, CancelEventArgs e) {
            if (lookup == null)
                lookup = (LookUpEditEx)sender;
            lookup.Properties.BeginUpdate();
            lookup.Properties.DataSource = lookup.Properties.Helper.CreateCollectionSource(lookup.FindEditingObject()).List;
            lookup.Properties.EndUpdate();
        }
        private void properties_ButtonClick(object sender, ButtonPressedEventArgs e) {
            string tag = Convert.ToString(e.Button.Tag);
            if (tag == MinusButtonTag)
                ClearCurrentObject();
            if (tag == AddButtonTag)
                AddNewObject();
        }
        private void ClearCurrentObject() {
            lookup.EditValue = null;
        }
        private void AddNewObject() {
            ShowViewParameters svp = new ShowViewParameters();
            ObjectSpace newObjectViewObjectSpace = helper.Application.CreateObjectSpace();
            object newObject = newObjectViewObjectSpace.CreateObject(helper.LookupObjectTypeInfo.Type);
            newObjectView = helper.Application.CreateDetailView(newObjectViewObjectSpace, newObject, true);
            newObjectView.Closed += newObjectView_Closed;
            svp.CreatedView = newObjectView;
            newObjectViewObjectSpace.Committed += newObjectViewObjectSpace_Committed;
            svp.TargetWindow = TargetWindow.NewModalWindow;
            svp.Context = TemplateContext.View;
            helper.Application.ShowViewStrategy.ShowView(svp, new ShowViewSource(null, null));
        }
        private void newObjectView_Closed(object sender, EventArgs e) {
            newObjectView.Closed -= newObjectView_Closed;
            newObjectView.ObjectSpace.Committed -= newObjectViewObjectSpace_Committed;
        }
        private void newObjectViewObjectSpace_Committed(object sender, EventArgs e) {
            lookup.EditValue = helper.ObjectSpace.GetObject(newObjectView.CurrentObject);
        }
        public override void Refresh() {
            base.Refresh();
            if (Control != null)
                Control.UpdateDisplayText();
        }
    }
    [ToolboxItem(false)]
    public class LookUpEditEx : DevExpress.XtraEditors.LookUpEdit, IGridInplaceEdit {
        private object gridEditingObject;
        private bool isValidating;
        public LookUpEditEx() {
            base.DataBindings.CollectionChanged += DataBindings_CollectionChanged;
        }
        public override string EditorTypeName {
            get { return RepositoryItemLookUpEditEx.EditorName; }
        }
        public new RepositoryItemLookUpEditEx Properties {
            get { return (RepositoryItemLookUpEditEx)base.Properties; }
        }
        public override object EditValue {
            get { return base.EditValue; }
            set {
                if (value != DBNull.Value && value != null) {
                    if (!Properties.Helper.LookupObjectType.IsInstanceOfType(value)) {
                        if (Properties.ThrowExceptionOnInvalidLookUpEditValueType) {
                            throw new InvalidCastException(SystemExceptionLocalizer.GetExceptionMessage(ExceptionId.UnableToCast,
                                value.GetType(),
                                Properties.Helper.LookupObjectType));
                        }
                        else {
                            base.EditValue = null;
                            return;
                        }
                    }
                }
                base.EditValue = value;
            }
        }
        public object FindEditingObject() {
            return BindingHelper.FindEditingObject(this);
        }
        private void OnEditingObjectChanged() {
            if (FindEditingObject() == null && EditValue != null)
                EditValue = null;
        }
        private void DataBindings_CollectionChanged(object sender, CollectionChangeEventArgs e) {
            OnEditingObjectChanged();
        }
        public new void UpdateDisplayText() {
            base.UpdateDisplayText();
            base.Refresh();
        }
        protected override void OnValidating(CancelEventArgs e) {
            isValidating = true;
            try {
                base.OnValidating(e);
            } finally {
                isValidating = false;
            }
        }
        public override bool IsModified {
            get { return base.IsModified; }
            set {
                if (!isValidating)
                    base.IsModified = value;
            }
        }

        #region IGridInplaceEdit Members
        System.Windows.Forms.ControlBindingsCollection IGridInplaceEdit.DataBindings {
            get { return base.DataBindings; }
        }
        object IGridInplaceEdit.GridEditingObject {
            get { return gridEditingObject; }
            set {
                if (gridEditingObject != value) {
                    gridEditingObject = value;
                    OnEditingObjectChanged();
                }
            }
        }
        #endregion
    }
    public class RepositoryItemLookUpEditEx : DevExpress.XtraEditors.Repository.RepositoryItemLookUpEdit, ILookupEditRepositoryItem {
        internal const string EditorName = "LookUpEditEx";
        private LookupEditorHelper helper;
        static RepositoryItemLookUpEditEx() {
            Register();
        }
        public RepositoryItemLookUpEditEx() {
            TextEditStyle = TextEditStyles.DisableTextEditor;
            ExportMode = ExportMode.DisplayText;
        }
        public static void Register() {
            if (!EditorRegistrationInfo.Default.Editors.Contains(EditorName)) {
                EditorRegistrationInfo.Default.Editors.Add(new EditorClassInfo(EditorName, typeof(LookUpEditEx),
                    typeof(RepositoryItemLookUpEditEx), typeof(LookUpEditViewInfo),
                    new ButtonEditPainter(), true, EditImageIndexes.LookUpEdit, typeof(PopupEditAccessible)));
            }
        }
        public override string EditorTypeName { get { return EditorName; } }
        public new LookUpEditEx OwnerEdit {
            get { return (LookUpEditEx)base.OwnerEdit; }
        }
        public LookupEditorHelper Helper {
            get { return helper; }
        }
        public void Init(string displayFormat, LookupEditorHelper helper) {
            this.helper = helper;
            BeginUpdate();
            DisplayFormat.FormatString = displayFormat;
            DisplayFormat.FormatType = FormatType.Custom;
            EditFormat.FormatString = displayFormat;
            EditFormat.FormatType = FormatType.Custom;
            EndUpdate();
        }
        public override void Assign(RepositoryItem item) {
            RepositoryItemLookUpEditEx source = (RepositoryItemLookUpEditEx)item;
            try {
                base.Assign(source);
            } catch { }
            helper = source.helper;
            ThrowExceptionOnInvalidLookUpEditValueType = source.ThrowExceptionOnInvalidLookUpEditValueType;
        }
        public override string GetDisplayText(FormatInfo format, object editValue) {
            string result = NullText;
            if (helper != null)
                result = helper.GetDisplayText(editValue, NullText, format.FormatString);
            return result;
        }

        #region ILookupEditRepositoryItem Members
        Type ILookupEditRepositoryItem.LookupObjectType {
            get { return helper.LookupObjectType; }
        }
        string ILookupEditRepositoryItem.DisplayMember {
            get { return helper.DisplayMember != null ? helper.DisplayMember.Name : string.Empty; }
        }
        bool ILookupEditRepositoryItem.IsFilterByValueSupported {
            get { return true; }
        }
        #endregion
    }
}