using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;

namespace AbcArbitrage.Homework.Routing
{
    [ShortRunJob]
    public class MessageRouterBenchmarks
    {
        private readonly MessageRouter _router;

        public MessageRouterBenchmarks()
        {
            var subscriptionIndex = BuildSubscriptionIndex();
            _router = new MessageRouter(subscriptionIndex);
        }

        private static SubscriptionIndex BuildSubscriptionIndex()
        {
            var subscriptionIndex = new SubscriptionIndex();
            var baseTypeName = typeof(RoutableMessage0).FullName!.TrimEnd('0');

            //var subscriptions = from clientIndex in Enumerable.Range(0, 30)
            //                    let clientId = new ClientId($"Client.{clientIndex}")
            //                    from typeIndex in Enumerable.Range(0, 10)
            //                    let messageTypeId = new MessageTypeId($"{baseTypeName}{typeIndex}")
            //                    from contentIndex in Enumerable.Range(0, 4_000)
            //                    select new Subscription(clientId, messageTypeId, new ContentPattern(contentIndex.ToString()));
            var subscriptions = from clientIndex in Enumerable.Range(0, 1000)
                                let clientId = new ClientId($"Client.{clientIndex}")
                                from typeIndex in Enumerable.Range(0, 10)
                                let messageTypeId = new MessageTypeId($"{baseTypeName}{typeIndex}")
                                from contentIndex in Enumerable.Range(0, 100)
                                select new Subscription(clientId, messageTypeId, new ContentPattern(contentIndex.ToString()));
            subscriptionIndex.AddSubscriptions(subscriptions);
            return subscriptionIndex;
        }

        [Benchmark]
        public List<ClientId> GetConsumers() => _router.GetConsumers(new RoutableMessage0 { Id = 999, Value = 1234m }).ToList();

        [Benchmark]
        public List<ClientId> GetConsumers_Unoptimized() => _router.GetConsumers_Unoptimized(new RoutableMessage0 { Id = 999, Value = 1234m }).ToList();

        public class RoutableMessage0 : IRoutableMessage
        {
            public int Id { get; set; }
            public decimal Value { get; set; }

            public MessageRoutingContent GetContent() => new(Id.ToString(), Value.ToString());
        }
    }
}
