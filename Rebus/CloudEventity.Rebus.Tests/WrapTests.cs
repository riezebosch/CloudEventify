using CloudEventify;
using CloudEventify.Rebus;
using Rebus.Messages;
using System;
using System.Collections.Generic;
using Xunit;

namespace CloudEventity.Rebus.Tests
{
    public class WrapTests
    {
        [Fact]
        public void EnvelopeWithConfiguredUri()
        {
            var sut = new Wrap(new TypesMapper().Map<string>("my-custom-event"), new Uri("test:uri"));
            var res = sut.Envelope(new Message(
                new Dictionary<string, string> { 
                    { "rbs2-msg-id", Guid.NewGuid().ToString() },
                    { "rbs2-senttime", DateTime.UtcNow.ToString() }
                }, "somestringdata"));
            Assert.NotNull(res);
            Assert.Equal(new Uri("test:uri"), res.Source);
        }

        [Fact]
        public void EnvelopeWithRequiredRebusHeaderMessageId()
        {
            var sut = new Wrap(new TypesMapper().Map<string>("my-custom-event"), new Uri("test:uri"));
            var fact = Guid.NewGuid().ToString();
            var res = sut.Envelope(new Message(
                new Dictionary<string, string> {
                    { "rbs2-msg-id", fact},
                    { "rbs2-senttime", new DateTimeOffset(2007, 10, 31, 21, 0, 0, new TimeSpan(-8, 0, 0)).ToString() }
                }, "somestringdata"));
            Assert.NotNull(res.Time);
            Assert.Equal(fact, res.Id);
        }

        [Fact]
        public void EnvelopeWithRquiredRebusHeaderSenttime()
        {
            var sut = new Wrap(new TypesMapper().Map<string>("my-custom-event"), new Uri("test:uri"));
            //Note you lose some precision due to serialization format
            var fact = new DateTimeOffset(2007, 10, 31, 21, 0, 0, new TimeSpan(-8, 0, 0));
            var res = sut.Envelope(new Message(
                new Dictionary<string, string> {
                    { "rbs2-msg-id", Guid.NewGuid().ToString() },
                    { "rbs2-senttime", fact.ToString() }
                }, "somestringdata"));
            Assert.NotNull(res.Time);
            Assert.Equal(fact, res.Time);
        }

        internal record SomeDataObject
        {
            public string? StringValue1 { get; init; }
            public decimal DecimalValue1 { get; init; }
            public DateTime DateTimeValue1 { get; init; }

            public static SomeDataObject GetRandom()
            {
                return new SomeDataObject()
                {
                    StringValue1 = "",
                    DecimalValue1 = 54.32M,
                    DateTimeValue1 = DateTime.UtcNow
                };
            }
        }

        [Fact]
        public void EnvelopeWithDataObjectFromBody()
        {
            var sut = new Wrap(new TypesMapper().Map<SomeDataObject>("myDataObjectTopic"), new Uri("test:uri"));
            var fact = SomeDataObject.GetRandom();
            var res = sut.Envelope(new Message(
                new Dictionary<string, string> {
                    { "rbs2-msg-id", Guid.NewGuid().ToString() },
                    { "rbs2-senttime", new DateTimeOffset(2007, 10, 31, 21, 0, 0, new TimeSpan(-8, 0, 0)).ToString() }
                }, fact));
            Assert.NotNull(res.Data);
            Assert.Equal(fact, res.Data);
        }

        [Fact]
        public void EnvelopeWithMappedType()
        {
            var fact = new TypesMapper().Map<SomeDataObject>("someDataObject553").Map<string>("stringObject");
            var sut = new Wrap(fact, new Uri("test:uri"));
            var res = sut.Envelope(new Message(
                new Dictionary<string, string> {
                        { "rbs2-msg-id", Guid.NewGuid().ToString() },
                        { "rbs2-senttime", new DateTimeOffset(2007, 10, 31, 21, 0, 0, new TimeSpan(-8, 0, 0)).ToString() }
            }, SomeDataObject.GetRandom()));
            Assert.NotNull(res.Type);
            Assert.Equal(fact[res.Data!.GetType()], res.Type);
        }

}
}
