// Copyright (C) Abc Arbitrage Asset Management - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited
// Proprietary and confidential
// Written by Olivier Coanet <o.coanet@abc-arbitrage.com>, 2020-10-06

using AbcArbitrage.Homework.Routing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace AbcArbitrage.Homework.Routing
{
    public class MessageQueue
    {
        // A dictionary to hold the queues for each client
        private readonly ConcurrentDictionary<ClientId, SortedDictionary<MessagePriority, Queue<IMessage>>> _clientQueues
            = new ConcurrentDictionary<ClientId, SortedDictionary<MessagePriority, Queue<IMessage>>>();
        public void EnqueueForClient(ClientId clientId, IMessage message, MessagePriority priority = MessagePriority.Normal)
        {
            var priorityQueue = _clientQueues.GetOrAdd(clientId, _ => new SortedDictionary<MessagePriority, Queue<IMessage>>());

            // Lock the priority queue to ensure thread safety when adding a message
            lock (priorityQueue)
            {
                // Get or add the queue for the specific priority
                if (!priorityQueue.TryGetValue(priority, out var queue))
                {
                    queue = new Queue<IMessage>();
                    priorityQueue[priority] = queue; // Initialize the queue if it doesn't exist
                }

                queue.Enqueue(message);
            }
        }

        public bool TryDequeueForClient(ClientId clientId, [MaybeNullWhen(false)] out IMessage message)
        {
            message = null;

            // Try to get the client's priority queue
            if (_clientQueues.TryGetValue(clientId, out var priorityQueue))
            {
                lock (priorityQueue) // Lock the priority queue for safe access
                {
                    // Iterate over priorities in descending order
                    foreach (var priority in priorityQueue.Keys.OrderByDescending(p => p))
                    {
                        if (priorityQueue.TryGetValue(priority, out var queue) && queue.Count > 0)
                        {
                            // Dequeue the message
                            message = queue.Dequeue();

                            // Remove the queue if it's empty
                            if (queue.Count == 0)
                            {
                                priorityQueue.Remove(priority); // Remove empty queue
                            }

                            // Remove the client entry if there are no queues left
                            if (priorityQueue.Count == 0)
                            {
                                _clientQueues.TryRemove(clientId, out _);
                            }

                            return true; // Message dequeued successfully
                        }
                    }
                }
            }

            return false; // No message to dequeue
        }
    }
}
