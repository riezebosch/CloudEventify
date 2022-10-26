using FluentAssertions;
using Xunit;

namespace CloudEventity.Tests;

public class MapperTests
{
    [Fact]
    public void Subject()
    {
        var map = new Mapper()
            .Map<UserLoggedIn>("user/loggedIn", m => m with {
                Subject = x => $"u/l/{x.Id}"
            });
            

        map[typeof(UserLoggedIn)].Subject(new UserLoggedIn("1234")).Should().Be("u/l/1234");
    }

    private record UserLoggedIn(string Id);
}