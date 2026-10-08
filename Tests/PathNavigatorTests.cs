using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using controls = nac.Forms.controls;

namespace Tests
{
    [TestClass]
    public class PathNavigatorTests
    {
        [TestMethod]
        public void JoinPath_AppendsSegmentWithSeparator()
        {
            Assert.AreEqual("Root\\Projects", controls.PathNavigator.JoinPath("Root", "Projects", "\\"));
            Assert.AreEqual("Root", controls.PathNavigator.JoinPath(string.Empty, "Root", "\\"));
            Assert.AreEqual("Root::Shared", controls.PathNavigator.JoinPath("Root", "Shared", "::"));
            Assert.AreEqual("a/b", controls.PathNavigator.JoinPath("a", "b", "/"));

            // nulls / empty are handled gracefully
            Assert.AreEqual("abc", controls.PathNavigator.JoinPath(null, "abc", "/"));
            Assert.AreEqual("abc", controls.PathNavigator.JoinPath("abc", string.Empty, "/"));
            Assert.AreEqual("abc", controls.PathNavigator.JoinPath("abc", null, "/"));
        }


        [TestMethod]
        public void PopTopSegment_RemovesLastSegment()
        {
            Assert.AreEqual("Root\\Projects", controls.PathNavigator.PopTopSegment("Root\\Projects\\Forms", "\\"));
            Assert.AreEqual(string.Empty, controls.PathNavigator.PopTopSegment("Root", "\\"));
            Assert.AreEqual("Root", controls.PathNavigator.PopTopSegment("Root::Shared", "::"));
            Assert.AreEqual(string.Empty, controls.PathNavigator.PopTopSegment("abc", "/"));
            Assert.AreEqual(string.Empty, controls.PathNavigator.PopTopSegment(string.Empty, "\\"));

            // multi-character separators
            Assert.AreEqual("A|B", controls.PathNavigator.PopTopSegment("A|B|C", "|"));
            Assert.AreEqual("A", controls.PathNavigator.PopTopSegment("A::B", "::"));
        }


        [TestMethod]
        public void PathNavigator_RaisesPathChangedWhenPathIsSet()
        {
            var nav = new controls.PathNavigator();
            nav.PathSeparator = "\\";

            string last = null;
            nav.PathChanged += (_s, p) => last = p;

            nav.Path = "Root\\Projects";
            Assert.AreEqual("Root\\Projects", last);

            nav.Path = "Root\\Projects\\Forms";
            Assert.AreEqual("Root\\Projects\\Forms", last);
        }


        [TestMethod]
        public void PathNavigator_DefaultSeparatorIsBackslash()
        {
            var nav = new controls.PathNavigator();
            Assert.AreEqual("\\", nav.PathSeparator);
        }
    }
}
