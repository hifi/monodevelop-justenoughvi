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

        [TestCase("a$aa", "l", "aa$a")]
        [TestCase("a$aa", "ll", "aaa$")]
        [TestCase("aa$a", "2l", "aaa$")]
        [TestCase("a$aa", "3l", "aaa$")]
        [TestCase("a$aa\naa", "3l", "aaa$\naa")]
        public void L_tests(string source, string keys, string expected)
        {
            Test(source, keys, expected, typeof(NormalMode));
        }
    }
}

