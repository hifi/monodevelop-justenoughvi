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
        [TestCase("p$ublic class NavigationTests : TextEditorTestBase", "2w", "public class N$avigationTests : TextEditorTestBase")] //TODO
        [TestCase("p$ublic class NavigationTests : TextEditorTestBase", "www", "public class NavigationTests :$ TextEditorTestBase")]
        [TestCase("p$ublic class NavigationTests : TextEditorTestBase", "wwww", "public class NavigationTests : T$extEditorTestBase")]
        public void W_tests(string source, string keys, string expected)
        {
            Test(source, keys, expected, typeof(NormalMode));
        }

        [TestCase("p$ublic class NavigationTests : TextEditorTestBase", "e", "public$ class NavigationTests : TextEditorTestBase")]
        [TestCase("p$ublic class NavigationTests : TextEditorTestBase", "ee", "public class$ NavigationTests : TextEditorTestBase")]
        [TestCase("p$ublic class NavigationTests : TextEditorTestBase", "2e", "public class$ NavigationTests : TextEditorTestBase")] 
        [TestCase("p$ublic class NavigationTests : TextEditorTestBase", "eeee", "public class NavigationTests :$ TextEditorTestBase")]
        [TestCase("p$ublic class NavigationTests : TextEditorTestBase", "5e", "public class NavigationTests : TextEditorTestBase$")]
        public void E_tests(string source, string keys, string expected)
        {
            Test(source, keys, expected, typeof(NormalMode));
        }

        [TestCase("   12345$67890", "^", "   1$234567890")]
        [TestCase("   12345$67890", "0", "$   1234567890")]
        [TestCase("abcd$efghijk", "fg", "abcdefg$hijk")]
        [TestCase("abcd$efghijk", "fz", "abcd$efghijk")]
        [TestCase("abcd$efghijk", "Fa", "a$bcdefghijk")]
        [TestCase("abcd$efghijk", "Fz", "abcd$efghijk")]
        [TestCase("123$45678", "f6", "123456$78")] //TODO: Fails because it thinks 6 is a count
        public void MiscTests(string source, string keys, string expected)
        {
            Test(source, keys, expected, typeof(NormalMode));
        }
    }
}

