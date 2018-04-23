using System;
using DevExpress.Xpo;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.ExpressApp;

namespace WinSolution.Module {
    [DefaultClassOptions]
    [DefaultListViewOptions(true, NewItemRowPosition.Top)]
    public class DemoObject : BaseObject {
        public DemoObject(Session session) : base(session) { }
        private string _Name;
        public string Name {
            get { return _Name; }
            set { SetPropertyValue("Name", ref _Name, value); }
        }
        private DemoLookupObject _LookupProperty;
        [ImmediatePostData]
        [VisibleInListView(true)]
        public DemoLookupObject LookupProperty {
            get { return _LookupProperty; }
            set { SetPropertyValue("LookupProperty", ref _LookupProperty, value); }
        }
    }
}
