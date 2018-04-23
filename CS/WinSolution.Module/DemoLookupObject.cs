using System;
using DevExpress.Xpo;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;

namespace WinSolution.Module {
    public class DemoLookupObject : BaseObject {
        public DemoLookupObject(Session session) : base(session) { }
        private string _Name;
        public string Name {
            get { return _Name; }
            set { SetPropertyValue("Name", ref _Name, value); }
        }
    }
}
