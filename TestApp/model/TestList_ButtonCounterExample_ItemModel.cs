

namespace TestApp.model
{
    public class TestList_ButtonCounterExample_ItemModel : nac.ViewModelBase.ViewModelBase {
        public int Counter {
            get { return base.GetValue(()=> this.Counter);}
            set { base.SetValue(() => this.Counter, value);}
        }
        public string Label{
            get { return base.GetValue(() => this.Label);}
            set { base.SetValue(() => this.Label, value);}
        }

    }
    
    
}