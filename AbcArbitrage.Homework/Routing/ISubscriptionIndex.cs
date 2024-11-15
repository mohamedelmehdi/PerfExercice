using System.Collections.Generic;

namespace AbcArbitrage.Homework.Routing
{
    /// <summary>
    /// Stores subscriptions.
    /// </summary>
    public interface ISubscriptionIndex
    {
        void AddSubscriptions(IEnumerable<Subscription> subscriptions);
        void RemoveSubscriptions(IEnumerable<Subscription> subscriptions);

        void RemoveSubscriptionsForConsumer(ClientId consumer);

        IEnumerable<Subscription> FindSubscriptions(MessageTypeId messageTypeId, MessageRoutingContent routingContent);
        IEnumerable<string> FindSubscriptionsByClients(MessageTypeId messageTypeId, MessageRoutingContent routingContent);
        IEnumerable<string> FindSubscriptionsByClients_Unoptimized(MessageTypeId messageTypeId, MessageRoutingContent routingContent);
        
    }
}
