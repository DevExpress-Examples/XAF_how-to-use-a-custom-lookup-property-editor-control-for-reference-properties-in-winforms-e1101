using DevExpress.ExpressApp;
using System;
using DevExpress.ExpressApp.Updating;
using DevExpress.Xpo;

namespace WinSolution.Module {
    public class Updater : ModuleUpdater {
        public Updater(ObjectSpace objectSpace, Version currentDBVersion) : base(objectSpace, currentDBVersion) { }
        public override void UpdateDatabaseAfterUpdateSchema() {
            base.UpdateDatabaseAfterUpdateSchema();
            DemoObject obj1 = ObjectSpace.CreateObject<DemoObject>();
            obj1.Name = "DemoObject1";
            DemoObject obj2 = ObjectSpace.CreateObject<DemoObject>();
            obj2.Name = "DemoObject2";
            DemoLookupObject lookupObj1 = ObjectSpace.CreateObject<DemoLookupObject>();
            lookupObj1.Name = "DemoLookupObject1";
            DemoLookupObject lookupObj2 = ObjectSpace.CreateObject<DemoLookupObject>();
            lookupObj2.Name = "DemoLookupObject2";
            obj1.LookupProperty = lookupObj1;
            obj2.LookupProperty = lookupObj2;
            obj1.Save();
            lookupObj1.Save();
            lookupObj2.Save();
        }
    }
}
