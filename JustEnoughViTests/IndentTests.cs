using NUnit.Framework;
using JustEnoughVi;

namespace JustEnoughViTests
{
    [TestFixture]
    public class IndentTests : TextEditorTestBase
    {
        [TestCase("aaa$", ">>", "    aaa$")]
        [TestCase("    aaa$", "<<", "aaa$")]
        [TestCase(" aaa$", "<<", "aaa$")]
        public void Indent_tests(string source, string keys, string expected)
        {
            Test(source, keys, expected, typeof(NormalMode));
        }
    }
}
