Imports DevExpress.ExpressApp
Imports DevExpress.ExpressApp.Updating

Namespace WinSolution.Module
	Public Class Updater
		Inherits ModuleUpdater
		Public Sub New(ByVal objectSpace As IObjectSpace, ByVal currentDBVersion As Version)
			MyBase.New(objectSpace, currentDBVersion)
		End Sub
		Public Overrides Sub UpdateDatabaseAfterUpdateSchema()
			MyBase.UpdateDatabaseAfterUpdateSchema()
			Dim obj1 As DemoObject = ObjectSpace.CreateObject(Of DemoObject)()
			obj1.Name = "DemoObject1"
			Dim obj2 As DemoObject = ObjectSpace.CreateObject(Of DemoObject)()
			obj2.Name = "DemoObject2"
			Dim lookupObj1 As DemoLookupObject = ObjectSpace.CreateObject(Of DemoLookupObject)()
			lookupObj1.Name = "DemoLookupObject1"
			Dim lookupObj2 As DemoLookupObject = ObjectSpace.CreateObject(Of DemoLookupObject)()
			lookupObj2.Name = "DemoLookupObject2"
			obj1.LookupProperty = lookupObj1
			obj2.LookupProperty = lookupObj2
			obj1.Save()
			lookupObj1.Save()
			lookupObj2.Save()
		End Sub
	End Class
End Namespace
