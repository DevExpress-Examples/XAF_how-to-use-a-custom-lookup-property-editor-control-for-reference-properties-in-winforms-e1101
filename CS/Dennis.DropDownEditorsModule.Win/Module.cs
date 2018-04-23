using System;
using System.Collections.Generic;
using DevExpress.ExpressApp;
using System.ComponentModel;
using System.Drawing;

namespace Dennis.DropDownEditorsModule.Win {
    [ToolboxItem(true)]
    [Description("Dennis: provides alternative editors for lookup properties in Windows Forms applications.")]
    [Browsable(true)]
    [EditorBrowsable(EditorBrowsableState.Always)]
    [ToolboxBitmap(typeof(DropDownEditorsWindowsFormsModule), "Dennis.DropDownEditorsModule.Win.Resourses.DennisModule.bmp")]
    public sealed partial class DropDownEditorsWindowsFormsModule : ModuleBase {
        public DropDownEditorsWindowsFormsModule() {
            InitializeComponent();
        }
    }
}
