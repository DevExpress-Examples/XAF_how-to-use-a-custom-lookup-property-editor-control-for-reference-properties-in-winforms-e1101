Imports Microsoft.VisualBasic
Imports System
Imports DevExpress.Utils
Imports System.ComponentModel
Imports DevExpress.ExpressApp
Imports DevExpress.Accessibility
Imports DevExpress.ExpressApp.Utils
Imports DevExpress.ExpressApp.Model
Imports DevExpress.ExpressApp.Editors
Imports DevExpress.ExpressApp.Win.Core
Imports DevExpress.XtraEditors.Drawing
Imports DevExpress.XtraEditors.Controls
Imports DevExpress.XtraEditors.ViewInfo
Imports DevExpress.XtraEditors.Repository
Imports DevExpress.ExpressApp.Win.Editors
Imports DevExpress.ExpressApp.Localization
Imports DevExpress.XtraEditors.Registrator

Namespace Dennis.Editors.Win
	<PropertyEditor(GetType(Object), EditorAliases.LookupPropertyEditor)> _
	Public Class LookupPropertyEditorEx
		Inherits DXPropertyEditor
		Implements IComplexPropertyEditor
		Private Const AddButtonTag As String = "AddButtonTag"
		Private Const MinusButtonTag As String = "MinusButtonTag"
		Private lookup As LookUpEditEx
		Private newObjectView As View
		Public Shadows ReadOnly Property Control() As LookUpEditEx
			Get
				Return lookup
			End Get
		End Property
		Private helper_Renamed As LookupEditorHelper
		Public Sub New(ByVal objectType As Type, ByVal item As IModelMemberViewItem)
			MyBase.New(objectType, item)
		End Sub
		Public ReadOnly Property Helper() As LookupEditorHelper
			Get
				Return helper_Renamed
			End Get
		End Property
		Protected Overrides Function CreateControlCore() As Object
			lookup = New LookUpEditEx()
			Return lookup
		End Function
		Protected Overrides Function CreateRepositoryItem() As RepositoryItem
			Return New RepositoryItemLookUpEditEx()
		End Function
		Protected Overrides Sub SetupRepositoryItem(ByVal item As RepositoryItem)
			MyBase.SetupRepositoryItem(item)
			Dim properties As RepositoryItemLookUpEditEx = CType(item, RepositoryItemLookUpEditEx)
			properties.Init(DisplayFormat, helper_Renamed)
			properties.ShowHeader = False
			'properties.DropDownRows = helper.SmallCollectionItemCount;
			AddHandler properties.QueryPopUp, AddressOf properties_QueryPopUp
			AddHandler properties.ButtonClick, AddressOf properties_ButtonClick
			Dim addButton As New EditorButton(ButtonPredefines.Plus)
			addButton.Tag = AddButtonTag
			addButton.Enabled = AllowEdit.ResultValue
			Dim minusButton As New EditorButton(ButtonPredefines.Minus)
			minusButton.Tag = MinusButtonTag
			minusButton.Enabled = AllowEdit.ResultValue
			properties.Buttons.Add(addButton)
			properties.Buttons.Add(minusButton)
			properties.SearchMode = SearchMode.OnlyInPopup
			properties.NullText = CaptionHelper.NullValueText
			If AllowNull Then
				properties.AllowNullInput = DefaultBoolean.True
			End If
			properties.TextEditStyle = TextEditStyles.DisableTextEditor
		End Sub
        Private Sub Setup(ByVal objectSpace As IObjectSpace, ByVal application As XafApplication) Implements IComplexPropertyEditor.Setup
            If helper_Renamed Is Nothing Then
                helper_Renamed = New LookupEditorHelper(application, objectSpace, MemberInfo.MemberTypeInfo, Model)
            End If
            helper_Renamed.SetObjectSpace(objectSpace)
        End Sub
		Protected Overrides Overloads Sub Dispose(ByVal disposing As Boolean)
			Try
				If lookup IsNot Nothing Then
					RemoveHandler lookup.QueryPopUp, AddressOf properties_QueryPopUp
				End If
			Finally
				MyBase.Dispose(disposing)
			End Try
		End Sub
		Private Sub properties_QueryPopUp(ByVal sender As Object, ByVal e As CancelEventArgs)
			If lookup Is Nothing Then
				lookup = CType(sender, LookUpEditEx)
			End If
			lookup.Properties.BeginUpdate()
			lookup.Properties.DataSource = lookup.Properties.Helper.CreateCollectionSource(lookup.FindEditingObject()).List
			lookup.Properties.EndUpdate()
		End Sub
		Private Sub properties_ButtonClick(ByVal sender As Object, ByVal e As ButtonPressedEventArgs)
			Dim tag As String = Convert.ToString(e.Button.Tag)
			If tag = MinusButtonTag Then
				ClearCurrentObject()
			End If
			If tag = AddButtonTag Then
				AddNewObject()
			End If
		End Sub
		Private Sub ClearCurrentObject()
			lookup.EditValue = Nothing
		End Sub
		Private Sub AddNewObject()
			Dim svp As New ShowViewParameters()
            Dim newObjectViewObjectSpace As IObjectSpace = helper_Renamed.Application.CreateObjectSpace()
			Dim newObject As Object = newObjectViewObjectSpace.CreateObject(helper_Renamed.LookupObjectTypeInfo.Type)
			newObjectView = helper_Renamed.Application.CreateDetailView(newObjectViewObjectSpace, newObject, True)
			AddHandler newObjectView.Closed, AddressOf newObjectView_Closed
			svp.CreatedView = newObjectView
			AddHandler newObjectViewObjectSpace.Committed, AddressOf newObjectViewObjectSpace_Committed
			svp.TargetWindow = TargetWindow.NewModalWindow
			svp.Context = TemplateContext.View
			helper_Renamed.Application.ShowViewStrategy.ShowView(svp, New ShowViewSource(Nothing, Nothing))
		End Sub
		Private Sub newObjectView_Closed(ByVal sender As Object, ByVal e As EventArgs)
			RemoveHandler newObjectView.Closed, AddressOf newObjectView_Closed
			RemoveHandler newObjectView.ObjectSpace.Committed, AddressOf newObjectViewObjectSpace_Committed
		End Sub
		Private Sub newObjectViewObjectSpace_Committed(ByVal sender As Object, ByVal e As EventArgs)
			lookup.EditValue = helper_Renamed.ObjectSpace.GetObject(newObjectView.CurrentObject)
		End Sub
		Public Overrides Sub Refresh()
			MyBase.Refresh()
			If Control IsNot Nothing Then
				Control.UpdateDisplayText()
			End If
		End Sub
	End Class
	<ToolboxItem(False)> _
	Public Class LookUpEditEx
		Inherits DevExpress.XtraEditors.LookUpEdit
		Implements IGridInplaceEdit
		Private gridEditingObject_Renamed As Object
		Private isValidating As Boolean
        Shared Sub New()
            RepositoryItemLookUpEditEx.Register()
        End Sub
		Public Sub New()
			AddHandler MyBase.DataBindings.CollectionChanged, AddressOf DataBindings_CollectionChanged
		End Sub
		Public Overrides ReadOnly Property EditorTypeName() As String
			Get
				Return RepositoryItemLookUpEditEx.EditorName
			End Get
		End Property
		Public Shadows ReadOnly Property Properties() As RepositoryItemLookUpEditEx
			Get
				Return CType(MyBase.Properties, RepositoryItemLookUpEditEx)
			End Get
		End Property
		Public Overrides Property EditValue() As Object
			Get
				Return MyBase.EditValue
			End Get
			Set(ByVal value As Object)
				If value IsNot DBNull.Value AndAlso value IsNot Nothing Then
					If (Not Properties.Helper.LookupObjectType.IsInstanceOfType(value)) Then
						If Properties.ThrowExceptionOnInvalidLookUpEditValueType Then
							Throw New InvalidCastException(SystemExceptionLocalizer.GetExceptionMessage(ExceptionId.UnableToCast, value.GetType(), Properties.Helper.LookupObjectType))
						Else
							MyBase.EditValue = Nothing
							Return
						End If
					End If
				End If
				MyBase.EditValue = value
			End Set
		End Property
		Public Function FindEditingObject() As Object
			Return BindingHelper.FindEditingObject(Me)
		End Function
		Private Sub OnEditingObjectChanged()
			If FindEditingObject() Is Nothing AndAlso EditValue IsNot Nothing Then
				EditValue = Nothing
			End If
		End Sub
		Private Sub DataBindings_CollectionChanged(ByVal sender As Object, ByVal e As CollectionChangeEventArgs)
			OnEditingObjectChanged()
		End Sub
		Public Shadows Sub UpdateDisplayText()
			MyBase.UpdateDisplayText()
			MyBase.Refresh()
		End Sub
		Protected Overrides Sub OnValidating(ByVal e As CancelEventArgs)
			isValidating = True
			Try
				MyBase.OnValidating(e)
			Finally
				isValidating = False
			End Try
		End Sub
		Public Overrides Property IsModified() As Boolean
			Get
				Return MyBase.IsModified
			End Get
			Set(ByVal value As Boolean)
				If (Not isValidating) Then
					MyBase.IsModified = value
				End If
			End Set
		End Property

		#Region "IGridInplaceEdit Members"
        Private ReadOnly Property DataBindings() As System.Windows.Forms.ControlBindingsCollection Implements IGridInplaceEdit.DataBindings
            Get
                Return MyBase.DataBindings
            End Get
        End Property
        Private Property GridEditingObject() As Object Implements IGridInplaceEdit.GridEditingObject
            Get
                Return gridEditingObject_Renamed
            End Get
            Set(ByVal value As Object)
                If gridEditingObject_Renamed IsNot value Then
                    gridEditingObject_Renamed = value
                    OnEditingObjectChanged()
                End If
            End Set
        End Property
		#End Region
	End Class
	Public Class RepositoryItemLookUpEditEx
		Inherits DevExpress.XtraEditors.Repository.RepositoryItemLookUpEdit
		Implements ILookupEditRepositoryItem
		Friend Const EditorName As String = "LookUpEditEx"
		Private helper_Renamed As LookupEditorHelper
		Shared Sub New()
			Register()
		End Sub
		Public Sub New()
			TextEditStyle = TextEditStyles.DisableTextEditor
			ExportMode = ExportMode.DisplayText
		End Sub
		Public Shared Sub Register()
			If (Not EditorRegistrationInfo.Default.Editors.Contains(EditorName)) Then
				EditorRegistrationInfo.Default.Editors.Add(New EditorClassInfo(EditorName, GetType(LookUpEditEx), GetType(RepositoryItemLookUpEditEx), GetType(LookUpEditViewInfo), New ButtonEditPainter(), True, EditImageIndexes.LookUpEdit, GetType(PopupEditAccessible)))
			End If
		End Sub
		Public Overrides ReadOnly Property EditorTypeName() As String
			Get
				Return EditorName
			End Get
		End Property
		Public Shadows ReadOnly Property OwnerEdit() As LookUpEditEx
			Get
				Return CType(MyBase.OwnerEdit, LookUpEditEx)
			End Get
		End Property
		Public ReadOnly Property Helper() As LookupEditorHelper
			Get
				Return helper_Renamed
			End Get
		End Property
        Public Sub Init(ByVal displayFormat As String, ByVal helper As LookupEditorHelper)
            Me.helper_Renamed = helper
            BeginUpdate()
            Me.DisplayFormat.FormatString = displayFormat
            Me.DisplayFormat.FormatType = FormatType.Custom
            EditFormat.FormatString = displayFormat
            EditFormat.FormatType = FormatType.Custom
            EndUpdate()
        End Sub
		Public Overrides Sub Assign(ByVal item As RepositoryItem)
			Dim source As RepositoryItemLookUpEditEx = CType(item, RepositoryItemLookUpEditEx)
			Try
				MyBase.Assign(source)
			Catch
			End Try
			helper_Renamed = source.helper
			ThrowExceptionOnInvalidLookUpEditValueType = source.ThrowExceptionOnInvalidLookUpEditValueType
		End Sub
		Public Overrides Function GetDisplayText(ByVal format As FormatInfo, ByVal editValue As Object) As String
			Dim result As String = NullText
			If helper_Renamed IsNot Nothing Then
				result = helper_Renamed.GetDisplayText(editValue, NullText, format.FormatString)
			End If
			Return result
		End Function

		#Region "ILookupEditRepositoryItem Members"
		Private ReadOnly Property LookupObjectType() As Type Implements ILookupEditRepositoryItem.LookupObjectType
			Get
				Return helper_Renamed.LookupObjectType
			End Get
		End Property
		Private ReadOnly Property DisplayMember() As String Implements ILookupEditRepositoryItem.DisplayMember
			Get
				If helper_Renamed.DisplayMember IsNot Nothing Then
					Return helper_Renamed.DisplayMember.Name
				Else
					Return String.Empty
				End If
			End Get
		End Property
#End Region
	End Class
End Namespace