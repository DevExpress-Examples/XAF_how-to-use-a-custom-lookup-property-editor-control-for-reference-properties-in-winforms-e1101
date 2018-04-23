Imports Microsoft.VisualBasic
Imports System
Imports DevExpress.ExpressApp
Imports DevExpress.ExpressApp.Editors
Imports DevExpress.ExpressApp.DC
Imports DevExpress.ExpressApp.Win.Editors
Imports DevExpress.Data.Filtering
Imports DevExpress.Xpo
Imports DevExpress.XtraEditors
Imports DevExpress.XtraEditors.Controls

Namespace Dennis.DropDownEditorsModule.Win
	<PropertyEditor(GetType(IXPSimpleObject))> _
	Public Class MRULookupPropertyEditor
		Inherits DXPropertyEditor
		Implements IComplexPropertyEditor
		Protected collectionLoadedCore As Boolean = False
		Private valueStringCore As String = String.Empty
		Private objectSpaceCore As ObjectSpace = Nothing
		Private applicationCore As XafApplication = Nothing
		Private controlCore As MRUEdit = Nothing

		Public Sub New(ByVal objectType As Type, ByVal info As DictionaryNode)
			MyBase.New(objectType, info)
		End Sub
		Public Shadows ReadOnly Property Control() As MRUEdit
			Get
				Return controlCore
			End Get
		End Property
		Protected ReadOnly Property ObjectSpace() As ObjectSpace
			Get
				Return objectSpaceCore
			End Get
		End Property
		Protected ReadOnly Property Application() As XafApplication
			Get
				Return applicationCore
			End Get
		End Property
		Protected ReadOnly Property DefaultMember() As IMemberInfo
			Get
				Return MemberInfo.MemberTypeInfo.DefaultMember
			End Get
		End Property
		Protected ReadOnly Property ShouldLoadCollection() As Boolean
			Get
				Return ((Not Object.ReferenceEquals(CurrentObject, Nothing))) AndAlso (Me.AllowEdit.ResultValue) AndAlso ((Not collectionLoadedCore))
			End Get
		End Property
		Protected Overrides Function GetControlValueCore() As Object
			If ShouldLoadCollection Then
				For Each obj As IXPSimpleObject In objectSpaceCore.CreateCollection(MemberInfo.MemberTypeInfo.Type, Nothing, New SortingCollection(New SortProperty() { New SortProperty("Name", DevExpress.Xpo.DB.SortingDirection.Ascending) }))
					Control.Properties.Items.Add(DefaultMember.GetValue(obj))
				Next obj
				collectionLoadedCore = True
			End If
			Return MyBase.GetControlValueCore()
		End Function
		Protected Overrides Function CreateControlCore() As Object
			controlCore = New MRUEdit()
			controlCore.Properties.DropDownRows = 10
			AddHandler controlCore.Validated, AddressOf OnControlValidated
			AddHandler controlCore.EditValueChanged, AddressOf OnControlEditValueChanged
			If Me.AllowEdit.ResultValue Then
				controlCore.Properties.TextEditStyle = TextEditStyles.Standard
			Else
				controlCore.Properties.TextEditStyle = TextEditStyles.DisableTextEditor
			End If
			Return controlCore
		End Function
		Private Sub OnControlValidated(ByVal sender As Object, ByVal e As EventArgs)
			WriteValueCore()
		End Sub
		Private Sub OnControlEditValueChanged(ByVal sender As Object, ByVal e As EventArgs)
			If (Control.EditValue IsNot Nothing) Then
				valueStringCore = Control.EditValue.ToString()
			Else
				valueStringCore = Nothing
			End If
		End Sub
		Protected Overrides Sub WriteValueCore()
			If AllowEdit.ResultValue Then
				Dim objectType As Type = MemberInfo.MemberTypeInfo.Type
				Dim obj As IXPSimpleObject = Nothing
				If (Not String.IsNullOrEmpty(valueStringCore)) Then
					obj = TryCast(objectSpaceCore.FindObject(objectType, New BinaryOperator(DefaultMember.Name, valueStringCore)), IXPSimpleObject)
					If obj Is Nothing Then
						obj = TryCast(objectSpaceCore.CreateObject(objectType), IXPSimpleObject)
						DefaultMember.SetValue(obj, valueStringCore)
					End If
				End If
				PropertyValue = obj
			End If
		End Sub
		Protected Overrides Sub ReadValueCore()
			If PropertyValue IsNot Nothing Then
				valueStringCore = TryCast(DefaultMember.GetValue(PropertyValue), String)
			End If
			Control.EditValue = valueStringCore
		End Sub
		Protected Overrides Overloads Sub Dispose(ByVal disposing As Boolean)
			If disposing Then
				RemoveHandler controlCore.EditValueChanged, AddressOf OnControlEditValueChanged
			End If
			MyBase.Dispose(disposing)
		End Sub
		Private Sub Setup(ByVal objectSpace As ObjectSpace, ByVal application As XafApplication) Implements IComplexPropertyEditor.Setup
			objectSpaceCore = objectSpace
			applicationCore = application
		End Sub
	End Class
End Namespace