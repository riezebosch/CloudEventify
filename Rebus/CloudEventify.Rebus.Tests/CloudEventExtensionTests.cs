using AutoFixture;
using CloudNative.CloudEvents;
using FluentAssertions;
using Rebus.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static CloudEventify.Rebus.Tests.WrapTests;

namespace CloudEventify.Rebus.Tests;

public class CloudEventExtensionTests
{

    private Dictionary<string,string> GetHeaders()
    {
        return new Dictionary<string, string> {
                { Headers.ContentEncoding, "UTF-8" },
                { Headers.ContentType, "application/json" },
                { Headers.CorrelationId, Guid.NewGuid().ToString() },
                { Headers.CorrelationSequence, Guid.NewGuid().ToString() },
                { Headers.DeferCount, "3" },
                { Headers.DeferredRecipient, "abc" },
                { Headers.DeferredUntil, "123" },
                { Headers.ErrorDetails, "456" },
                { Headers.Express, "789" },
                { Headers.InReplyTo, "sb://something.uri" },
                { Headers.Intent, "p2p" },
                { Headers.MessageId, Guid.NewGuid().ToString() },
                { Headers.MessagePayloadAttachmentId, "45672" },
                { Headers.ReturnAddress, "sb:/replyhere.com" },
                { Headers.RoutingSlipItinerary, "23" },
                { Headers.RoutingSlipTravelogue, "0987654" },
                { Headers.SenderAddress, "1234" },
                { Headers.SourceQueue, "gbfd34" },
                { Headers.TimeToBeReceived, new DateTimeOffset(2007, 11, 1, 21, 0, 0, new TimeSpan(-2, 0, 0)).ToString("O") },
                { Headers.Type, "SomeDataObject" },
                { Headers.SentTime, new DateTimeOffset(2007, 10, 31, 21, 0, 0, new TimeSpan(-8, 0, 0)).ToString("O") }
        };
    }

    private Message GetMessage(Dictionary<string,string> headers)
    {
        var fact = SomeDataObject.GetRandom();
        return new Message(headers, fact);
    }

    private CloudEvent GetCloudEvent(Message msg)
    {
        var sut = new Wrap(new Mapper().Map<SomeDataObject>("myDataObjectTopic"), new Uri("test:uri"));
        var res = sut.Envelope(msg);
        return res;
    }

    [Fact]
    public void RebusHeaders_From_CloudEvent_Should_Contain_all_Rbs_headers()
    {
        var factheaders = GetHeaders();
        var factMsg = GetMessage(factheaders);
        var cloudEvent = GetCloudEvent(factMsg);

        Assert.NotNull(cloudEvent);
        cloudEvent.Id.Should().Be(factheaders[Headers.MessageId]);
        
        var headers = cloudEvent.GetRebusHeaders();
        headers.Should().BeEquivalentTo(factheaders);
        headers.Count.Should().Be(21);
    }

    [Fact]
    public void RebusHeaders_From_CloudEvent_Should_Work_without_custom_headers()
    {
        var factheaders = new Dictionary<string, string>
        {
            { Headers.MessageId, Guid.NewGuid().ToString()}
        };
        var factMsg = GetMessage(factheaders);
        var cloudEvent = GetCloudEvent(factMsg);    


        Assert.NotNull(cloudEvent);
        cloudEvent.Id.Should().Be(factheaders[Headers.MessageId]);

        var headers = cloudEvent.GetRebusHeaders();
        headers.Should().BeEquivalentTo(factheaders);
        headers[Headers.MessageId].Should().Be(factheaders[Headers.MessageId]);
        headers.Count.Should().Be(1);

    }
}
