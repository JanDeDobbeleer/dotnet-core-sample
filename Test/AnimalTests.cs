using Xunit;
using Animals;
using FluentAssertions;

namespace Test
{
    public class AnimalTests
    {
        [Fact]
        public void ElephantMakesTrumpSound()
        {
            var actual = new Elephant().MakeSound();
            actual.Should().Be("trump", "that's the sound an elephant makes");
        }

        [Fact]
        public void FishMakesBlubSound()
        {
            var actual = new Fish().MakeSound();
            actual.Should().Be("blub", "that's the sound a fish makes (in the water that is)");
        }
    }
}
