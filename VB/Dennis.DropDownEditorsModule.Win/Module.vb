Imports Microsoft.VisualBasic
Imports System
Imports System.Collections.Generic
Imports DevExpress.ExpressApp
Imports System.ComponentModel
Imports System.Drawing

Namespace Dennis.DropDownEditorsModule.Win
	<ToolboxItem(True), Description("Dennis: provides alternative editors for lookup properties in Windows Forms applications."), Browsable(True), EditorBrowsable(EditorBrowsableState.Always), ToolboxBitmap(GetType(DropDownEditorsWindowsFormsModule), "Dennis.DropDownEditorsModule.Win.Resourses.DennisModule.bmp")> _
	Public NotInheritable Partial Class DropDownEditorsWindowsFormsModule
		Inherits ModuleBase
		Public Sub New()
			InitializeComponent()
		End Sub
	End Class
End Namespace
