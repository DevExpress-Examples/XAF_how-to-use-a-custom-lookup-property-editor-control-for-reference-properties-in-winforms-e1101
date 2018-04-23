Imports Microsoft.VisualBasic
Imports System

Imports DevExpress.Xpo

Imports DevExpress.ExpressApp
Imports DevExpress.Persistent.Base
Imports DevExpress.Persistent.BaseImpl
Imports DevExpress.Persistent.Validation

Namespace WinSolution.Module
	<DefaultClassOptions> _
	Public Class DomainObject1
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
		Private _DomainObject2 As DomainObject2
		<ImmediatePostData> _
		Public Property DomainObject2() As DomainObject2
			Get
				Return _DomainObject2
			End Get
			Set(ByVal value As DomainObject2)
				SetPropertyValue("DomainObject2", _DomainObject2, value)
			End Set
		End Property
	End Class

End Namespace
