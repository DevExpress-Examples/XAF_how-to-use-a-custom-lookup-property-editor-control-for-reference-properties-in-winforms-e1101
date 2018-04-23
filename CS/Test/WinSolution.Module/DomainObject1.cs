using System;

using DevExpress.Xpo;

using DevExpress.ExpressApp;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Persistent.Validation;

namespace WinSolution.Module {
    [DefaultClassOptions]
    public class DomainObject1 : BaseObject {
        public DomainObject1(Session session) : base(session) { }
        private string _Name;
        public string Name {
            get { return _Name; }
            set { SetPropertyValue("Name", ref _Name, value); }
        }
        private DomainObject2 _DomainObject2;
        [ImmediatePostData]
        public DomainObject2 DomainObject2 {
            get { return _DomainObject2; }
            set { SetPropertyValue("DomainObject2", ref _DomainObject2, value); }
        }
    }

}
