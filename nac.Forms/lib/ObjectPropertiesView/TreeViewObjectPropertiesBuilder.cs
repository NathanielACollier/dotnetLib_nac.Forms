using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Avalonia.Controls;
using Avalonia.Media;
using nac.Forms.lib.Extensions;

namespace nac.Forms.lib.ObjectPropertiesView;

public static class TreeViewObjectPropertiesBuilder
{
    private static List<object> VisitedReferences = new List<object>();
    
    public static void BuildTree(TreeView tree, string rootNodeHeader, object sourceObject,
        NodeExpandSettings expandSettings = null)
    {
        if (expandSettings == null)
        {
            expandSettings = new NodeExpandSettings();
        }

        VisitedReferences.Clear(); // clear out the hashing that detects recursive properties
        clearTree(tree);

        TreeViewItem root = new TreeViewItem { Header = rootNodeHeader };

        BuildTree(root, sourceObject, expandSettings);

        root.IsExpanded = expandSettings.ExpandRoot;
        
        addItemToTree(tree, root);
    }

    private static void clearTree(TreeView tree)
    {
        tree.Items.Set( new List<TreeViewItem>());
    }

    private static void addItemToTree(TreeView tree, TreeViewItem newItem)
    {
        var items = new List<object>();
        foreach (var existingItem in tree.Items)
        {
            items.Add(existingItem);
        }
        items.Add(newItem);

        tree.Items.Set( items);
    }

    private static void addItemToTreeItem(TreeViewItem parent, TreeViewItem newItem)
    {
        var items = new List<object>();
        foreach (var existingItem in parent.Items)
        {
            items.Add(existingItem);
        }
        items.Add(newItem);
        parent.Items.Set(items);
    }


    private static void HandleCollectionItem(TreeViewItem parentTreeNode, int index, object entryObj,
        NodeExpandSettings expandSettings)
    {
        if (entryObj != null)
        {
            TreeViewItem arrayParentItem = new TreeViewItem
                { Header = entryObj.GetType().Name, FontWeight = FontWeight.Bold };

            BuildTree(arrayParentItem, entryObj, expandSettings);

            arrayParentItem.IsExpanded = expandSettings.ExpandCollections;
            addItemToTreeItem(parentTreeNode, arrayParentItem);
        }
        else
        {
            TreeViewItem nullItem = new TreeViewItem
            {
                Header = string.Format("Index[{0}]=NULL", index), FontWeight = FontWeight.Bold,
                Foreground = Brushes.Red
            };
            nullItem.IsExpanded = expandSettings.ExpandNullItem;
            addItemToTreeItem(parentTreeNode, nullItem);
        }
    }


    private static void HandleCollection(TreeViewItem parentTreeNode, ICollection collection,
        NodeExpandSettings expandSettings)
    {
        int index = 0;


        foreach (object itemValue in collection)
        {
            HandleCollectionItem(parentTreeNode, index, itemValue, expandSettings);
            ++index;
        }
    }

    public static bool isCollection(object o)
    {
        return typeof(ICollection).IsAssignableFrom(o.GetType())
               || typeof(ICollection<>).IsAssignableFrom(o.GetType());
    }


    public static bool isXMLLinqObject(object o)
    {
        return (o is XAttribute) ||
               (o is XElement) ||
               (o is XDocument);
    }


    private static void HandleXMLLinqObject(TreeViewItem parentItem, object sourceObject,
        NodeExpandSettings expandSettings)
    {
        if (sourceObject is XAttribute)
        {
            XAttribute attr = sourceObject as XAttribute;

            var attrItem = new TreeViewItem
                { Header = attr.Name.LocalName, IsExpanded = expandSettings.ExpandPropertyName };
            var attrValueItem = new TreeViewItem { Header = attr.Value, IsExpanded = expandSettings.ExpandXML };
            
            addItemToTreeItem(attrItem, attrValueItem);
            
            addItemToTreeItem(parentItem, attrItem);
        }
        else if (sourceObject is XElement)
        {
            XElement element = sourceObject as XElement;

            TreeViewItem elementItem = new TreeViewItem
                { Header = element.Name.LocalName, IsExpanded = expandSettings.ExpandXML };

            foreach (XAttribute attr in element.Attributes())
            {
                HandleXMLLinqObject(elementItem, attr, expandSettings);
            }

            foreach (XElement subElement in element.Elements())
            {
                HandleXMLLinqObject(elementItem, subElement, expandSettings);
            }

            if (!string.IsNullOrEmpty(element.Value))
            {
                var valueDisplayItem = new TreeViewItem
                    { Header = "Value", IsExpanded = expandSettings.ExpandPropertyName };
                var valueItem = new TreeViewItem { Header = element.Value, IsExpanded = expandSettings.ExpandXML };
                
                addItemToTreeItem(valueDisplayItem, valueItem);
                addItemToTreeItem(elementItem, valueDisplayItem);
            }
            
            addItemToTreeItem(parentItem, elementItem);
        }
        else if (sourceObject is XDocument)
        {
            XDocument doc = sourceObject as XDocument;

            HandleXMLLinqObject(parentItem, doc.Root, expandSettings);
        }
    }
    
    
    private static bool isPrimativeType(Type t)
    {
        // KeyValuePair from Dictionary IsValueType = True && IsGenericType = True
        //    + There may be other structure types that look like int,bool,string,etc...
        // + This section of code is to identify the objects that do not have properties like int,bool,string,double,etc...
        // what if the object that is sent in is a primitive???
        return t.IsPrimitive ||
               (t.IsValueType && !t.IsGenericType) ||
               t.Name == "String";
    }

    private static bool IsKeyValuePair(object o)
    {
        Type type = o.GetType();
        if (type.IsGenericType)
        {
            return type.GetGenericTypeDefinition() != null
                ? type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>)
                : false;
        }

        return false;
    }


    private static object GetNamedPropValue(object sourceObject, string propertyName)
    {
        var properties = sourceObject.GetType().GetProperties();
        var prop = properties.Single(p => string.Equals(p.Name, propertyName));

        return prop.GetValue(sourceObject, null);
    }


    private static void BuildTree(TreeViewItem parentItem, object sourceObject, NodeExpandSettings expandSettings)
    {
        // this is to prevent recursive references (If we see an item twice we don't keep following it down)
        if (sourceObject.GetType().IsClass)
        {
            if (VisitedReferences.Contains(sourceObject))
            {
                return; // we've already seen this object so get out
            }

            VisitedReferences.Add(sourceObject); // should store a reference to the tree node of the property
        }


        if (isPrimativeType(sourceObject.GetType()))
        {
            //TreeViewItem propNameItem = new TreeViewItem { Header = sourceObject.GetType().Name, FontWeight = FontWeights.Bold };
            TreeViewItem valueItem = new TreeViewItem
            {
                Header = sourceObject, IsExpanded = expandSettings.ExpandPrimativeValues,
                FontWeight = FontWeight.Normal
            };
            
            addItemToTreeItem(parentItem, valueItem);

            return;
        }

        if (IsKeyValuePair(sourceObject))
        {
            var key = GetNamedPropValue(sourceObject, "Key");
            var val = GetNamedPropValue(sourceObject, "Value");
            var dictEntryNode = new TreeViewItem
                { Header = key, FontWeight = FontWeight.Bold, IsExpanded = expandSettings.ExpandPropertyName };
            BuildTree(dictEntryNode, val, expandSettings);
            addItemToTreeItem(parentItem, dictEntryNode);
            return;
        }

        if (isCollection(sourceObject))
        {
            // if a collection got sent in as a source object we need to make a tree node called collection and then add stuff to it
            //   This shouldn't happen I don't think through recursion, it should only be possible with the first call to the function
            TreeViewItem collectionItem = new TreeViewItem
            {
                Header = "Collection", FontWeight = FontWeight.Normal, IsExpanded = expandSettings.ExpandCollections
            };

            HandleCollection(collectionItem, (ICollection)sourceObject, expandSettings);
            
            addItemToTreeItem(parentItem, collectionItem);

            return;
        }

        if (isXMLLinqObject(sourceObject))
        {
            TreeViewItem xmlStructure = new TreeViewItem
                { Header = "XML", FontWeight = FontWeight.Normal, IsExpanded = expandSettings.ExpandXML };

            HandleXMLLinqObject(xmlStructure, sourceObject, expandSettings);
            
            addItemToTreeItem(parentItem, xmlStructure);

            return;
        }


        foreach (PropertyInfo propInfo in sourceObject.GetType().GetProperties())
        {
            TreeViewItem propNameItem = new TreeViewItem { Header = propInfo.Name, FontWeight = FontWeight.Bold };
            propNameItem.IsExpanded = expandSettings.ExpandPropertyName;

            object propValue = propInfo.GetValue(sourceObject, null);

            if (propValue != null)
            {
                BuildTree(propNameItem, propValue, expandSettings);
            }
            
            addItemToTreeItem(parentItem, propNameItem);
        }

        foreach (FieldInfo fieldInfo in sourceObject.GetType().GetFields())
        {
            TreeViewItem fieldNameItem = new TreeViewItem { Header = fieldInfo.Name, FontWeight = FontWeight.Bold };
            fieldNameItem.IsExpanded = expandSettings.ExpandPropertyName;

            object propValue = fieldInfo.GetValue(sourceObject);

            if (propValue != null)
            {
                BuildTree(fieldNameItem, propValue, expandSettings);
            }
            
            addItemToTreeItem(parentItem, fieldNameItem);
        }
    }
}