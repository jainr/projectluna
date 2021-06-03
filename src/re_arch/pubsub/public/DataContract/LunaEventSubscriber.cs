using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.PubSub.PublicClient
{
    public class LunaEventSubscriber
    {
        public LunaEventSubscriber()
        {
            this.ExcludedEventTypes = new List<string>();
        }

        public string SubscriberServiceName { get; set; }

        public string SubscriberFunctionName { get; set; }

        public string SubscriberQueueName 
        { 
            get 
            { 
                return string.Format("{0}-{1}", 
                    this.SubscriberServiceName.ToLower(), 
                    this.SubscriberFunctionName.ToLower()); 
            } 
        }

        public List<string> ExcludedEventTypes { get; set; }
    }

}
