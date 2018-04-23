using System;
using DevExpress.ExpressApp.Updating;
using DevExpress.Xpo;

namespace WinSolution.Module {
    public class Updater : ModuleUpdater {
        public Updater(Session session, Version currentDBVersion) : base(session, currentDBVersion) { }
        public override void UpdateDatabaseAfterUpdateSchema() {
            base.UpdateDatabaseAfterUpdateSchema();
            DemoObject obj1 = new DemoObject(Session);
            obj1.Name = "DemoObject1";
            DemoObject obj2 = new DemoObject(Session);
            obj2.Name = "DemoObject2";
            DemoLookupObject lookupObj1 = new DemoLookupObject(Session);
            lookupObj1.Name = "DemoLookupObject1";
            DemoLookupObject lookupObj2 = new DemoLookupObject(Session);
            lookupObj2.Name = "DemoLookupObject2";
            obj1.LookupProperty = lookupObj1;
            obj2.LookupProperty = lookupObj2;
            obj1.Save();
            lookupObj1.Save();
            lookupObj2.Save();
        }
    }
}
