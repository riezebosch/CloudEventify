﻿using CloudNative.CloudEvents;

namespace CloudEventify.Rebus;

public static class CloudEventExtensions
{
    public static Dictionary<string, string> GetRebusHeaders(this CloudEvent cloudEvent)
    {
        var mapper = HeaderMap.Instance;
        var res = new Dictionary<string, string>();
        foreach (var attrName in mapper.Reverse.Keys)
        {
            var attrib = cloudEvent.ExtensionAttributes.FirstOrDefault(ea => ea.Name.Equals(attrName));
            if (attrib != null)
            {
                var value = cloudEvent[attrib];
                if (value != null)
                {
                    res[mapper.Reverse[attrName]] = (string)value;
                }
            }
        }
        return res;
    }

}