using NUnit.Framework;
using JustEnoughVi;

namespace JustEnoughViTests
{
    [TestFixture]
    public class NavigationTests : TextEditorTestBase
    {
        [TestCase("aaa$", "h", "aa$a")]
        [TestCase("aaa$", "hh", "a$aa")]
        [TestCase("aaa$", "2h", "a$aa")]
        [TestCase("aaa$", "3h", "a$aa")]
        [TestCase("aa\naaa$", "3h", "aa\na$aa")]
        public void H_tests(string source, string keys, string expected)
        {
            Test(source, keys, expected, typeof(NormalMode));
        }
    }
}

