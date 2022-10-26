using Rebus.Messages;

namespace CloudEventify.Rebus;

public class RebusHeader2CloudAttributeMap
{
    public readonly IDictionary<string, string> Rebus2CloudMap = new Dictionary<string, string>()
    {
        { Headers.MessageId,             "r2msgid" },
        { Headers.Type,                  "r2msgtype" },
        { Headers.CorrelationId,         "r2corrid" },
        { Headers.InReplyTo,             "r2inreplyto" },
        { Headers.CorrelationSequence,   "r2corrseq" },
        { Headers.ReturnAddress,         "r2returnaddress" },
        { Headers.SenderAddress,         "r2senderaddress" },
        { Headers.RoutingSlipItinerary,  "r2routingitinerary" },
        { Headers.RoutingSlipTravelogue, "r2routingtravelogue" },
        { Headers.ContentType,           "r2contenttype" },
        { Headers.ContentEncoding,       "r2contentencoding" },
        { Headers.ErrorDetails,          "r2errordetails" },
        { Headers.SourceQueue,           "r2sourcequeue" },
        { Headers.DeferredUntil,         "r2deferreduntil" },
        { Headers.DeferredRecipient,     "r2deferredrecipient" },
        { Headers.DeferCount,            "r2defercount" },
        { Headers.TimeToBeReceived,      "r2timetobereceived" },
        { Headers.Express,               "r2express" },
        { Headers.SentTime,              "r2senttime" },
        { Headers.Intent,                "r2intent" },
        { Headers.MessagePayloadAttachmentId, "r2msgattachementid" }
    };

    public string FromCloudAttribute(string cloudAttributeName)
    {
        return Rebus2CloudMap.First(p => p.Value.Equals(cloudAttributeName, StringComparison.OrdinalIgnoreCase)).Key; ;
    }

    public string ToCloudAttributeName(string rebusHeaderName)
    {
        return Rebus2CloudMap.First(p=> p.Key.Equals(rebusHeaderName, StringComparison.Ordinal)).Value;
    }
    
}