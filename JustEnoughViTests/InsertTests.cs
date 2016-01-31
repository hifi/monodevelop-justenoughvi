using JustEnoughVi;
using NUnit.Framework;

namespace JustEnoughViTests
{
    [TestFixture]
    public class InsertTests : TextEditorTestBase
    {
        [Test]
        public void Should_insert_text()
        {
            Test("abcd$", "i", "abc$d", typeof(InsertMode));
        }

        [Test]
        public void Should_append_text()
        {
            Test("abcd$", "a", "abcd$", typeof(InsertMode));
        }
    }
}

