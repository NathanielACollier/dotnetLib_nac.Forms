using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Templates;

namespace nac.Forms
{
    public partial class Form
    {
        
        
        public Form Tabs(params model.TabCreationInfo[] newTabsToCreate)
        {
            var tc = new Avalonia.Controls.TabControl();

            foreach (var tabCreator in newTabsToCreate)
            {
                addTabToTabControl(tc, tabCreator);
            }
            
            AddRowToHost(tc, rowAutoHeight: false);

            return this;
        }

        private void addTabToTabControl(TabControl tc, model.TabCreationInfo tabCreator)
        {
            var items = tc.Items.Cast<object>().ToList();
            var tab = new Avalonia.Controls.TabItem();

            if (tabCreator.PopulateHeader != null)
            {
                // use a template header
                var headerForm = new Form(_parentForm: this);
                tabCreator.PopulateHeader(headerForm);

                tab.Header = headerForm.Host;
            }
            else
            {
                tab.Header = tabCreator.Header;
            }

            var itemForm = new Form(_parentForm: this);
            tabCreator.Populate(itemForm);
            tab.Content = itemForm.Host;
            
            items.Add(tab);
            tc.Items = items;
        }

        public delegate void InitializeTabDelegate(model.TabCreationInfo newTabInfo);


        public Form Tabs(params InitializeTabDelegate[] tabs)
        {
            var tc = new Avalonia.Controls.TabControl();

            foreach (var newTabInitFunction in tabs)
            {
                var tabCreateInfo = new model.TabCreationInfo();
                newTabInitFunction(tabCreateInfo);
                
                addTabToTabControl(tc, tabCreateInfo);
            }
            
            AddRowToHost(tc, rowAutoHeight: false);

            return this;
        }
    }
}