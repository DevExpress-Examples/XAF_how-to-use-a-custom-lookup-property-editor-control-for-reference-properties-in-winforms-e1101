Imports DevExpress.Xpo
Imports DevExpress.Persistent.Base
Imports DevExpress.Persistent.BaseImpl
Imports DevExpress.ExpressApp

Namespace WinSolution.Module
	<DefaultClassOptions, DefaultListViewOptions(True, NewItemRowPosition.Top)> _
	Public Class DemoObject
		Inherits BaseObject
		Public Sub New(ByVal session As Session)
			MyBase.New(session)
		End Sub
		Private _Name As String
		Public Property Name() As String
			Get
				Return _Name
			End Get
			Set(ByVal value As String)
				SetPropertyValue("Name", _Name, value)
			End Set
		End Property
		Private _LookupProperty As DemoLookupObject
		<ImmediatePostData, VisibleInListView(True)> _
		Public Property LookupProperty() As DemoLookupObject
			Get
				Return _LookupProperty
			End Get
			Set(ByVal value As DemoLookupObject)
				SetPropertyValue("LookupProperty", _LookupProperty, value)
			End Set
		End Property
	End Class
End Namespace
