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

        [TestCase("a lon$g sentence", "a", "a lon$g sentence")]
        [TestCase("a lon$g sentence", "A", "a long sentence$")]
        public void A_tests(string source, string keys, string expected)
        {
            Test(source, keys, expected, typeof(InsertMode));
        }
        [Test] //TODO: this inserts the new line in the wrong place
        public void Should_insert_new_line_above()
        {
            var source = @"aaaaa
                           bb$bbb
                           ccccc";
            var expected = @"aaaaa
                
                           bbbbb
                           ccccc";
            Test(source, "O", expected, typeof(InsertMode));
        }

        [Test]
        public void Should_insert_new_line_below()
        {
            var source = @"aaaaa
                           bb$bbb
                           ccccc";
            var expected = @"aaaaa
                           bbbbb

                           ccccc";
            Test(source, "o", expected, typeof(InsertMode));
        }
    }
}

