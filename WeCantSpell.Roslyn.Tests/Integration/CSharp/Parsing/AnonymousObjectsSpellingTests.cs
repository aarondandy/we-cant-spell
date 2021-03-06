﻿using System.Threading.Tasks;
using FluentAssertions;
using WeCantSpell.Roslyn.Tests.Utilities;
using Xunit;

namespace WeCantSpell.Roslyn.Tests.Integration.CSharp.Parsing
{
    public class AnonymousObjectsSpellingTests : CSharpParsingTestBase
    {
        public static object[][] can_find_mistakes_in_anonymous_members_data => new[]
        {
            new object[] { "Count", 204 },
            new object[] { "Distance", 232 },
            new object[] { "Nested", 265 },
            new object[] { "Value", 318 }
        };

        [Theory, MemberData(nameof(can_find_mistakes_in_anonymous_members_data))]
        public async Task can_find_mistakes_in_anonymous_members(string expectedWord, int expectedStart)
        {
            var expectedEnd = expectedStart + expectedWord.Length;

            var analyzer = new SpellingAnalyzerCSharp(new WrongWordChecker(expectedWord));
            var project = await ReadCodeFileAsProjectAsync("AnonymousObjects.SimpleExamples.csx");

            var diagnostics = await GetDiagnosticsAsync(project, analyzer);

            diagnostics.Should().ContainSingle()
                .Subject.Should()
                .HaveId("SP3110")
                .And.HaveLocation(expectedStart, expectedEnd, "AnonymousObjects.SimpleExamples.csx")
                .And.HaveMessageContaining(expectedWord);
        }
    }
}
