using FluentAssertions;
using Rebus.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace CloudEventify.Rebus.Tests
{
    public class HeaderMapTests
    {
        [Fact]
        public void Should_Contain_All_Known_Rebus_Headers()
        {
            var map = HeaderMap.Instance;

            Assert.NotNull(map);

            map.Forward.Keys.Should().Contain(Headers.ContentEncoding);
            map.Forward.Keys.Should().Contain(Headers.ContentType);
            map.Forward.Keys.Should().Contain(Headers.CorrelationId);
            map.Forward.Keys.Should().Contain(Headers.CorrelationSequence);
            map.Forward.Keys.Should().Contain(Headers.DeferCount);
            map.Forward.Keys.Should().Contain(Headers.DeferredRecipient);
            map.Forward.Keys.Should().Contain(Headers.DeferredUntil);
            map.Forward.Keys.Should().Contain(Headers.ErrorDetails);
            map.Forward.Keys.Should().Contain(Headers.Express);
            map.Forward.Keys.Should().Contain(Headers.InReplyTo);
            map.Forward.Keys.Should().Contain(Headers.Intent);
            map.Forward.Keys.Should().Contain(Headers.MessageId);
            map.Forward.Keys.Should().Contain(Headers.MessagePayloadAttachmentId);
            map.Forward.Keys.Should().Contain(Headers.ReturnAddress);
            map.Forward.Keys.Should().Contain(Headers.RoutingSlipItinerary);
            map.Forward.Keys.Should().Contain(Headers.RoutingSlipTravelogue);
            map.Forward.Keys.Should().Contain(Headers.SenderAddress);
            map.Forward.Keys.Should().Contain(Headers.SentTime);
            map.Forward.Keys.Should().Contain(Headers.SourceQueue);
            map.Forward.Keys.Should().Contain(Headers.TimeToBeReceived);
            map.Forward.Keys.Should().Contain(Headers.Type);

            map.Forward.Keys.Distinct().Count().Should().Be(21);
            map.Forward.Values.Distinct().Count().Should().Be(21);

            map.Forward[Headers.ContentEncoding].Should().Be("r2contentencoding");
            map.Forward[Headers.ContentType].Should().Be("r2contenttype");
            map.Forward[Headers.CorrelationId].Should().Be("r2corrid");
            map.Forward[Headers.CorrelationSequence].Should().Be("r2corrseq");
            map.Forward[Headers.DeferCount].Should().Be("r2defercount");
            map.Forward[Headers.DeferredRecipient].Should().Be("r2deferredrecipient");
            map.Forward[Headers.DeferredUntil].Should().Be("r2deferreduntil");
            map.Forward[Headers.ErrorDetails].Should().Be("r2errordetails");
            map.Forward[Headers.Express].Should().Be("r2express");
            map.Forward[Headers.InReplyTo].Should().Be("r2inreplyto");
            map.Forward[Headers.Intent].Should().Be("r2intent");
            map.Forward[Headers.MessageId].Should().Be("r2msgid");
            map.Forward[Headers.MessagePayloadAttachmentId].Should().Be("r2msgattachementid");
            map.Forward[Headers.ReturnAddress].Should().Be("r2returnaddress");
            map.Forward[Headers.RoutingSlipItinerary].Should().Be("r2routingitinerary");
            map.Forward[Headers.RoutingSlipTravelogue].Should().Be("r2routingtravelogue");
            map.Forward[Headers.SenderAddress].Should().Be("r2senderaddress");
            map.Forward[Headers.SentTime].Should().Be("r2senttime");
            map.Forward[Headers.SourceQueue].Should().Be("r2sourcequeue");
            map.Forward[Headers.TimeToBeReceived].Should().Be("r2timetobereceived");
            map.Forward[Headers.Type].Should().Be("r2msgtype");
        }

        [Fact]
        public void Should_be_Singleton()
        {
            var map = HeaderMap.Instance;
            var map2 = HeaderMap.Instance;
            HeaderMap.ReferenceEquals(map, map2);
            map2.Should().BeEquivalentTo(map);
        }
    }
}
