using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CloudEventify.Tests;

public class MapTests
{
    [Fact]
    public void TestForwardAndReverseMapping()
    {
        var sut = new Map<string, int>();
        sut.Add("a", 1);
        sut.Add("b", 2);
        sut.Add("c", 3);

        sut.Forward["a"].Should().Be(1);
        sut.Forward["b"].Should().Be(2);
        sut.Forward["c"].Should().Be(3);

        sut.Reverse[1].Should().Be("a");
        sut.Reverse[2].Should().Be("b");
        sut.Reverse[3].Should().Be("c");
    }

    [Fact]
    public void Test_Throws_with_duplicate_keys()
    {
        var sut = new Map<string, int>();
        sut.Add("a", 1);

        Assert.Throws<ArgumentException>(() =>
                sut.Add("a", 2))
                    .Message.Should().Be("An item with the same key has already been added. Key: a");
    }

    [Fact]
    public void Test_Throws_with_duplicate_Value()
    {
        var sut = new Map<string, int>();
        sut.Add("a", 1);

        Assert.Throws<ArgumentException>(() =>
                sut.Add("b", 1))
                    .Message.Should().Be("An item with the same key has already been added. Key: 1");
    }

    [Fact]
    public void Test_Forward_And_Reverse_Should_be_similar()
    {
        var sut = new Map<string, int>();
        sut.Add("a", 1);
        sut.Add("b", 2);
        sut.Add("c", 3);

        sut.Forward.Keys.Should().BeEquivalentTo(sut.Reverse.Values);
        sut.Forward.Values.Should().BeEquivalentTo(sut.Reverse.Keys);
    }

    [Fact]
    public void MapFromDictionary_Should_Have_Full_Map()
    {
        var dict = new Dictionary<string, int>()
        {
            {"a", 1},
            {"b", 2},
            {"c", 3},
            {"d", 4}
        };

        var map = dict.ToMap();

        map.Forward.Keys.Should().BeEquivalentTo(dict.Keys);
        map.Forward.Values.Should().BeEquivalentTo(dict.Values);

        map.Reverse.Keys.Should().BeEquivalentTo(dict.Values);
        map.Reverse.Values.Should().BeEquivalentTo(dict.Keys);

    }

}
