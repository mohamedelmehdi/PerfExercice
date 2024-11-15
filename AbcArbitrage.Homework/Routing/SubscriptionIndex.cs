using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AbcArbitrage.Homework.Routing
{
    public class SubscriptionIndex : ISubscriptionIndex
    {
        // List to store clients subscriptions
        private readonly List<(string ClientId, Dictionary<string, PatternNode> MessageDict)> _subscriptions;

        public SubscriptionIndex()
        {
            _subscriptions = new List<(string, Dictionary<string, PatternNode>)>();
        }

        public IEnumerable<string> FindSubscriptionsByClients(MessageTypeId messageTypeId, MessageRoutingContent routingContent)
        {
            var searchPattern = routingContent.Parts?.ToArray() ?? Array.Empty<string>();
            var messageTypeIdValue = messageTypeId.ToString();
            var results = new ConcurrentBag<string>();

            Parallel.ForEach(_subscriptions,kvp =>
            {
                if (kvp.MessageDict.TryGetValue(messageTypeIdValue, out var rootNode))
                {
                    if (rootNode.IsEndOfPattern ||
                    (searchPattern.Length >= rootNode.ShortestDepth && PatternNodeHelper.SearchPatternInTree(searchPattern, rootNode)) )
                    {
                        results.Add(kvp.ClientId);
                    }
                }
            });

            return results;
        }

        public IEnumerable<string> FindSubscriptionsByClients_Unoptimized(MessageTypeId messageTypeId, MessageRoutingContent routingContent)
        {
            var searchPattern = routingContent.Parts?.ToArray() ?? Array.Empty<string>();
            var messageTypeIdValue = messageTypeId.ToString();
            var results = new List<string>();

            foreach (var kvp in _subscriptions)
            {
                if (kvp.MessageDict.TryGetValue(messageTypeIdValue, out var rootNode))
                {
                    // Simple and slow pattern matching
                    if (rootNode.IsEndOfPattern ||
                    (searchPattern.Length >= rootNode.ShortestDepth && PatternNodeHelper.SearchPatternInTree(searchPattern, rootNode)))
                    {
                        results.Add(kvp.ClientId);
                    }
                    {
                        results.Add(kvp.ClientId);
                    }
                }
            }

            return results;
        }


        // Add subscriptions using tree of nodes
        public void AddSubscriptions(IEnumerable<Subscription> subscriptions)
        {
            foreach (var subscription in subscriptions)
            {
                var clientId = subscription.ConsumerId.ToString();
                var messageTypeId = subscription.MessageTypeId.ToString();
                var contentPatternParts = subscription.ContentPattern.Parts?.ToArray() ?? Array.Empty<string>();

                var messageDict = _subscriptions.FirstOrDefault(s => s.ClientId == clientId).MessageDict;

                if (messageDict == null)
                {
                    messageDict = new Dictionary<string, PatternNode>();
                    _subscriptions.Add((clientId, messageDict)); // Add new client subscriptions
                }

                if (!messageDict.TryGetValue(messageTypeId, out var rootNode))
                {
                    rootNode = new PatternNode();
                    messageDict[messageTypeId] = rootNode;
                }

                PatternNodeHelper.AddPatternToTree(contentPatternParts, rootNode);
            }
        }

        // Remove subscriptions from the tree
        public void RemoveSubscriptions(IEnumerable<Subscription> subscriptions)
        {
            foreach (var subscription in subscriptions)
            {
                var clientId = subscription.ConsumerId.ToString();
                var messageTypeId = subscription.MessageTypeId.ToString();
                var contentPatternParts = subscription.ContentPattern.Parts?.ToArray() ?? Array.Empty<string>();

                var index = _subscriptions.FindIndex(s => s.ClientId == clientId);

                if (index >= 0)
                {
                    var messageDict = _subscriptions[index].MessageDict;

                    if (messageDict.TryGetValue(messageTypeId, out var rootNode))
                    {
                        PatternNodeHelper.RemovePatternFromTree(contentPatternParts, rootNode);

                        // Clean up if the root node is empty
                        if (rootNode.Children.Count == 0)
                        {
                            messageDict.Remove(messageTypeId);
                        }

                        // Clean up if the client dictionary is empty
                        if (messageDict.Count == 0)
                        {
                            _subscriptions.RemoveAt(index);
                        }
                    }
                }
            }
        }

        public void RemoveSubscriptionsForConsumer(ClientId consumer)
        {
            var index = _subscriptions.FindIndex(s => s.ClientId == consumer.ToString());
            if (index >= 0)
            {
                _subscriptions.RemoveAt(index);
            }
        }

        // Find subscriptions using the tree
        public IEnumerable<Subscription> FindSubscriptions(MessageTypeId messageTypeId, MessageRoutingContent routingContent)
        {
            var searchPattern = routingContent.Parts?.ToArray() ?? Array.Empty<string>();
            var results = new ConcurrentBag<Subscription>();

            Parallel.ForEach(_subscriptions, kvp =>
            {
                var clientId = kvp.ClientId;
                var messageDict = kvp.MessageDict;

                if (messageDict.TryGetValue(messageTypeId.ToString(), out var rootNode))
                {
                    if (PatternNodeHelper.SearchPatternInTree(searchPattern, rootNode))
                    {
                        var matchedPattern = string.Join(".", searchPattern);
                        results.Add(new Subscription(new ClientId(clientId), messageTypeId, new ContentPattern(matchedPattern.Split('.'))));
                    }
                }
            });

            return results;
        }
    }

}
