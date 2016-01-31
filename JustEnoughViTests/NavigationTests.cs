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
        public void L_tests(string source, string keys, string expected)
        {
            Test(source, keys, expected, typeof(NormalMode));
        }

        [TestCase("p$ublic class NavigationTests : TextEditorTestBase", "w", "public c$lass NavigationTests : TextEditorTestBase")]
        [TestCase("p$ublic class NavigationTests : TextEditorTestBase", "ww", "public class N$avigationTests : TextEditorTestBase")]
        //TODO: [TestCase("p$ublic class NavigationTests : TextEditorTestBase", "2w", "public class N$avigationTests : TextEditorTestBase")]
        [TestCase("p$ublic class NavigationTests : TextEditorTestBase", "www", "public class NavigationTests :$ TextEditorTestBase")]
        [TestCase("p$ublic class NavigationTests : TextEditorTestBase", "wwww", "public class NavigationTests : T$extEditorTestBase")]
        public void W_tests(string source, string keys, string expected)
        {
            Test(source, keys, expected, typeof(NormalMode));
        }
    }
}

