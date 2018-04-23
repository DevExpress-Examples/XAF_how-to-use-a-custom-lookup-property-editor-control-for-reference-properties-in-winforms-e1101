using System;
using DevExpress.Utils;
using System.Collections;
using System.ComponentModel;
using DevExpress.ExpressApp;
using DevExpress.Accessibility;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Utils;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.Win.Core;
using DevExpress.XtraEditors.Drawing;
using DevExpress.XtraEditors.ViewInfo;
using DevExpress.XtraEditors.Controls;
using DevExpress.ExpressApp.Win.Editors;
using DevExpress.XtraEditors.Repository;
using DevExpress.ExpressApp.Localization;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.XtraEditors.Registrator;

namespace Dennis.Editors.Win {
    [PropertyEditor(typeof(object), EditorAliases.LookupPropertyEditor)]
    public class LookupPropertyEditorEx : DXPropertyEditor, IComplexPropertyEditor {
        private const string AddButtonTag = "AddButtonTag";
        private const string MinusButtonTag = "MinusButtonTag";
        private LookUpEditEx lookup;
        private View lookupObjectView;
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
            properties.Enter += properties_Enter;
            properties.ButtonClick += properties_ButtonClick;

            EditorButton addButton = new EditorButton(ButtonPredefines.Plus);
            addButton.Tag = AddButtonTag;
            addButton.Enabled = AllowEdit.ResultValue;
            properties.Buttons.Add(addButton);

            EditorButton minusButton = new EditorButton(ButtonPredefines.Minus);
            minusButton.Tag = MinusButtonTag;
            minusButton.Enabled = AllowEdit.ResultValue;
            properties.Buttons.Add(minusButton);
        }
        private void properties_Enter(object sender, EventArgs e) {
            if (lookup == null)
                lookup = (LookUpEditEx)sender;
            InitializeDataSource();
        }
        protected virtual void InitializeDataSource() {
            if (lookup != null && lookup.Properties != null && lookup.Properties.Helper != null) {
                lookup.Properties.DataSource = lookup.Properties.Helper.CreateCollectionSource(lookup.FindEditingObject()).List;
            }
        }
        void IComplexPropertyEditor.Setup(IObjectSpace objectSpace, XafApplication application) {
            if (helper == null)
                helper = new LookupEditorHelper(application, objectSpace, MemberInfo.MemberTypeInfo, Model);
            helper.SetObjectSpace(objectSpace);
            helper.ObjectSpace.Reloaded += ObjectSpace_Reloaded;
        }
        private void ObjectSpace_Reloaded(object sender, EventArgs e) {
            InitializeDataSource();
        }
        protected override void Dispose(bool disposing) {
            try {
                if (disposing) {
                    if (lookup != null && lookup.Properties != null)
                        lookup.Properties.Enter -= properties_Enter;
                    if (helper != null && helper.ObjectSpace != null)
                        helper.ObjectSpace.Reloaded -= ObjectSpace_Reloaded;
                }
            } finally {
                base.Dispose(disposing);
            }
        }
        private void properties_ButtonClick(object sender, ButtonPressedEventArgs e) {
            string tag = Convert.ToString(e.Button.Tag);
            if (tag == MinusButtonTag)
                ClearCurrentObject();
            if (tag == AddButtonTag)
                AddNewObject();
        }
        protected virtual void OpenCurrentObject() {
            ShowViewParameters svp = new ShowViewParameters();
            IObjectSpace openObjectViewObjectSpace = helper.Application.CreateObjectSpace();
            object targetObject = openObjectViewObjectSpace.GetObject(lookup.EditValue);
            if (targetObject != null) {
                openObjectViewObjectSpace.Committed += openObjectViewObjectSpace_Committed;
                openObjectViewObjectSpace.Disposed += openObjectViewObjectSpace_Disposed;
                lookupObjectView = helper.Application.CreateDetailView(openObjectViewObjectSpace, targetObject, true);
                svp.CreatedView = lookupObjectView;
                helper.Application.ShowViewStrategy.ShowView(svp, new ShowViewSource(null, null));
            }
        }
        private void openObjectViewObjectSpace_Disposed(object sender, EventArgs e) {
            IObjectSpace os = (IObjectSpace)sender;
            os.Disposed -= openObjectViewObjectSpace_Disposed;
            os.Committed -= openObjectViewObjectSpace_Committed;
        }
        private void openObjectViewObjectSpace_Committed(object sender, EventArgs e) {
            if (lookupObjectView != null)
                lookup.EditValue = helper.ObjectSpace.GetObject(lookupObjectView.CurrentObject);
        }
        protected virtual void ClearCurrentObject() {
            lookup.EditValue = null;
        }
        protected virtual void AddNewObject() {
            ShowViewParameters svp = new ShowViewParameters();
            IObjectSpace newObjectViewObjectSpace = helper.Application.CreateObjectSpace();
            object newObject = newObjectViewObjectSpace.CreateObject(helper.LookupObjectTypeInfo.Type);
            lookupObjectView = helper.Application.CreateDetailView(newObjectViewObjectSpace, newObject, true);
            svp.CreatedView = lookupObjectView;
            newObjectViewObjectSpace.Committed += newObjectViewObjectSpace_Committed;
            newObjectViewObjectSpace.Disposed += newObjectViewObjectSpace_Disposed;
            svp.TargetWindow = TargetWindow.NewModalWindow;
            svp.Context = TemplateContext.PopupWindow;
            svp.Controllers.Add(helper.Application.CreateController<DialogController>());
            helper.Application.ShowViewStrategy.ShowView(svp, new ShowViewSource(null, null));
        }
        private void newObjectViewObjectSpace_Disposed(object sender, EventArgs e) {
            IObjectSpace os = (IObjectSpace)sender;
            os.Disposed -= newObjectViewObjectSpace_Disposed;
            os.Committed -= newObjectViewObjectSpace_Committed;
        }
        private void newObjectViewObjectSpace_Committed(object sender, EventArgs e) {
            lookup.EditValue = helper.ObjectSpace.GetObject(lookupObjectView.CurrentObject);
            if (lookup.Properties.DataSource != null)
                ((IList)lookup.Properties.DataSource).Add(lookup.EditValue);
        }
        public override void Refresh() {
            base.Refresh();
            if (lookup != null)
                lookup.UpdateDisplayText();
        }
    }
    [ToolboxItem(false)]
    public class LookUpEditEx : DevExpress.XtraEditors.LookUpEdit, IGridInplaceEdit {
        private object gridEditingObject;
        static LookUpEditEx() { RepositoryItemLookUpEditEx.Register(); }
        public LookUpEditEx() {
            base.DataBindings.CollectionChanged += DataBindings_CollectionChanged;
        }
        protected override void Dispose(bool disposing) {
            if (disposing)
                base.DataBindings.CollectionChanged -= DataBindings_CollectionChanged;
            base.Dispose(disposing);
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
        public RepositoryItemLookUpEditEx() { }
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
            TextEditStyle = TextEditStyles.Standard;
            ExportMode = ExportMode.DisplayText;
            DisplayMember = ((ILookupEditRepositoryItem)this).DisplayMember;
            ValueMember = null;
            ShowHeader = false;
            DropDownRows = helper.SmallCollectionItemCount;
            SearchMode = SearchMode.AutoFilter;
            NullText = CaptionHelper.NullValueText;
            AllowNullInput = DefaultBoolean.True;

            EndUpdate();
        }
        public override string GetDisplayText(FormatInfo format, object editValue) {
            string result = base.GetDisplayText(format, editValue);
            if (string.IsNullOrEmpty(result) && editValue != null && helper != null)
                result = helper.GetDisplayText(editValue, NullText, format.FormatString);
            return result;
        }
        public override void Assign(RepositoryItem item) {
            RepositoryItemLookUpEditEx source = (RepositoryItemLookUpEditEx)item;
            try {
                base.Assign(source);
            } catch { }
            helper = source.helper;
            ThrowExceptionOnInvalidLookUpEditValueType = source.ThrowExceptionOnInvalidLookUpEditValueType;
        }
        #region ILookupEditRepositoryItem Members
        Type ILookupEditRepositoryItem.LookupObjectType {
            get { return helper.LookupObjectType; }
        }
        string ILookupEditRepositoryItem.DisplayMember {
            get { return helper.DisplayMember != null ? helper.DisplayMember.Name : string.Empty; }
        }
        #endregion
    }
}