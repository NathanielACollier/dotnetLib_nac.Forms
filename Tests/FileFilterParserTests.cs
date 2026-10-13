using Microsoft.VisualStudio.TestTools.UnitTesting;
using nac.Forms.lib;

namespace Tests
{
    [TestClass]
    public class FileFilterParserTests
    {
        [TestMethod]
        public void NullOrEmptyFilterReturnsNoEntries()
        {
            Assert.AreEqual(0, FileFilterParser.Parse(null).Count);
            Assert.AreEqual(0, FileFilterParser.Parse("").Count);
            Assert.AreEqual(0, FileFilterParser.Parse("   ").Count);
        }

        [TestMethod]
        public void SingleFilterSimple()
        {
            var entries = FileFilterParser.Parse("pfx|(*.pfx)");

            Assert.AreEqual(1, entries.Count);
            Assert.AreEqual("pfx", entries[0].Name);
            CollectionAssert.AreEqual(new[] { "*.pfx" }, entries[0].Patterns);
        }

        [TestMethod]
        public void FilterWithMultiplePatterns()
        {
            var entries = FileFilterParser.Parse("Certificates|(*.pfx;*.crt;*.cer)");

            Assert.AreEqual(1, entries.Count);
            Assert.AreEqual("Certificates", entries[0].Name);
            CollectionAssert.AreEqual(new[] { "*.pfx", "*.crt", "*.cer" }, entries[0].Patterns);
        }

        [TestMethod]
        public void MultipleFiltersSeparatedByDoublePipe()
        {
            var entries = FileFilterParser.Parse("pfx|(*.pfx)||PEM|(*.pem;*.key)");

            Assert.AreEqual(2, entries.Count);

            Assert.AreEqual("pfx", entries[0].Name);
            CollectionAssert.AreEqual(new[] { "*.pfx" }, entries[0].Patterns);

            Assert.AreEqual("PEM", entries[1].Name);
            CollectionAssert.AreEqual(new[] { "*.pem", "*.key" }, entries[1].Patterns);
        }

        [TestMethod]
        public void WhitespaceIsIgnored()
        {
            var entries = FileFilterParser.Parse("  pfx | ( *.pfx ; *.p12 )  ");

            Assert.AreEqual(1, entries.Count);
            Assert.AreEqual("pfx", entries[0].Name);
            CollectionAssert.AreEqual(new[] { "*.pfx", "*.p12" }, entries[0].Patterns);
        }

        [TestMethod]
        public void ParenthesesAreOptional()
        {
            var entries = FileFilterParser.Parse("pfx|*.pfx;*.p12");

            Assert.AreEqual(1, entries.Count);
            CollectionAssert.AreEqual(new[] { "*.pfx", "*.p12" }, entries[0].Patterns);
        }

        [TestMethod]
        public void FilterWithoutANameUsesThePatternsAsTheName()
        {
            var entries = FileFilterParser.Parse("(*.pfx;*.p12)");

            Assert.AreEqual(1, entries.Count);
            Assert.AreEqual("*.pfx; *.p12", entries[0].Name);
            CollectionAssert.AreEqual(new[] { "*.pfx", "*.p12" }, entries[0].Patterns);
        }

        [TestMethod]
        public void BareExtensionsBecomeWildcards()
        {
            var entries = FileFilterParser.Parse("Certificates|pfx;p12;cer");

            Assert.AreEqual(1, entries.Count);
            CollectionAssert.AreEqual(new[] { "*.pfx", "*.p12", "*.cer" }, entries[0].Patterns);
        }

        [TestMethod]
        public void FullFileNamePatternsAreLeftAlone()
        {
            var entries = FileFilterParser.Parse("Reports|report.txt");

            Assert.AreEqual(1, entries.Count);
            CollectionAssert.AreEqual(new[] { "report.txt" }, entries[0].Patterns);
        }

        [TestMethod]
        public void EntryWithoutUsablePatternsIsDropped()
        {
            var entries = FileFilterParser.Parse("pfx|()||PEM|(*.pem)");

            Assert.AreEqual(1, entries.Count);
            Assert.AreEqual("PEM", entries[0].Name);
            CollectionAssert.AreEqual(new[] { "*.pem" }, entries[0].Patterns);
        }
    }
}
