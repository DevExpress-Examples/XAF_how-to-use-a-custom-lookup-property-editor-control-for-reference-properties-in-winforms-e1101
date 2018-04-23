Imports Microsoft.VisualBasic
Imports System
Imports DevExpress.ExpressApp.Updating
Imports DevExpress.Xpo

Namespace WinSolution.Module
	Public Class Updater
		Inherits ModuleUpdater
		Public Sub New(ByVal session As Session, ByVal currentDBVersion As Version)
			MyBase.New(session, currentDBVersion)
		End Sub
		Public Overrides Sub UpdateDatabaseAfterUpdateSchema()
			MyBase.UpdateDatabaseAfterUpdateSchema()
			Dim obj1 As New DemoObject(Session)
			obj1.Name = "DemoObject1"
			Dim obj2 As New DemoObject(Session)
			obj2.Name = "DemoObject2"
			Dim lookupObj1 As New DemoLookupObject(Session)
			lookupObj1.Name = "DemoLookupObject1"
			Dim lookupObj2 As New DemoLookupObject(Session)
			lookupObj2.Name = "DemoLookupObject2"
			obj1.LookupProperty = lookupObj1
			obj2.LookupProperty = lookupObj2
			obj1.Save()
			lookupObj1.Save()
			lookupObj2.Save()
		End Sub
	End Class
End Namespace
