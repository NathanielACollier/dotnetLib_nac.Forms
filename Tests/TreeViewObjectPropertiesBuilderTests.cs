using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using nac.Forms.lib.ObjectPropertiesView;

namespace Tests
{
    [TestClass]
    public class TreeViewObjectPropertiesBuilderTests
    {
        private static IEnumerable<TreeViewItem> PreOrder(TreeViewItem item)
        {
            yield return item;
            foreach (var child in item.Items.Cast<TreeViewItem>())
            {
                foreach (var deeper in PreOrder(child))
                {
                    yield return deeper;
                }
            }
        }

        private static IEnumerable<TreeViewItem> AllNodes(TreeView tree)
        {
            return tree.Items.Cast<TreeViewItem>().SelectMany(i => PreOrder(i));
        }

        private static List<string> AllHeaders(TreeView tree)
        {
            return AllNodes(tree).Select(i => i.Header?.ToString()).ToList();
        }


        [TestMethod]
        public void DictionaryEntries_RenderAsPropertyNames_NotKeyValuePairTypeNode()
        {
            var tree = new TreeView();

            var dict = new Dictionary<string, string>
            {
                ["Root"] = "Projects",
                ["Shared"] = "Folder"
            };

            TreeViewObjectPropertiesBuilder.BuildTree(tree, "ObjectView", dict,
                expandSettings: new NodeExpandSettings(expandAll: true));

            var headers = AllHeaders(tree);

            // no KeyValuePair`2 type-name wrapper node (the old, ugly rendering)
            CollectionAssert.DoesNotContain(headers, "KeyValuePair`2");

            // the dictionary keys are the plain property-name nodes
            CollectionAssert.Contains(headers, "Root");
            CollectionAssert.Contains(headers, "Shared");
        }


        [TestMethod]
        public void DictionaryEntry_Value_RendersUnderKeyNode()
        {
            var tree = new TreeView();

            var dict = new Dictionary<string, string>
            {
                ["Root"] = "Projects"
            };

            TreeViewObjectPropertiesBuilder.BuildTree(tree, "ObjectView", dict,
                expandSettings: new NodeExpandSettings(expandAll: true));

            var keyNode = AllNodes(tree).FirstOrDefault(i => i.Header?.ToString() == "Root");
            Assert.IsNotNull(keyNode, "expected the dictionary key 'Root' to be a node");

            var childHeaders = keyNode.Items.Cast<TreeViewItem>().Select(c => c.Header?.ToString()).ToList();

            // the value is rendered directly under the key node
            CollectionAssert.Contains(childHeaders, "Projects");
        }


        [TestMethod]
        public void NestedDictionary_KeysAndValues_RenderCleanly()
        {
            var tree = new TreeView();

            var nested = new Dictionary<string, Dictionary<string, string>>
            {
                ["Root"] = new Dictionary<string, string> { ["String"] = "Projects" },
                ["Shared"] = new Dictionary<string, string> { ["Forms"] = "UI" }
            };

            TreeViewObjectPropertiesBuilder.BuildTree(tree, "ObjectView", nested,
                expandSettings: new NodeExpandSettings(expandAll: true));

            var headers = AllHeaders(tree);

            CollectionAssert.DoesNotContain(headers, "KeyValuePair`2");
            CollectionAssert.Contains(headers, "Root");
            CollectionAssert.Contains(headers, "Shared");
            CollectionAssert.Contains(headers, "String");
            CollectionAssert.Contains(headers, "Projects");
        }
    }
}
