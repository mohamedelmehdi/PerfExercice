using System;
using System.Collections.Generic;

namespace AbcArbitrage.Homework.Routing
{
    public static class PatternNodeHelper
    {
        // Add pattern to tree
        public static void AddPatternToTree(string[] parts, PatternNode root)
        {
            var currentNode = root;  // Start from the root
            int currentDepth = 0;    // Keep track of the current depth

            for (int i = 0; i < parts.Length; i++)
            {
                var part = parts[i];

                // Traverse or add nodes as needed
                if (!currentNode.Children.TryGetValue(part, out var nextNode))
                {
                    nextNode = new PatternNode();
                    currentNode.Children[part] = nextNode;
                }
                currentNode = nextNode;
                currentDepth++;
            }
            // Update the ShortestDepth
            root.ShortestDepth = Math.Min(root.ShortestDepth, currentDepth);
            currentNode.IsEndOfPattern = true;
        }

        // Remove pattern from tree
        public static void RemovePatternFromTree(string[] parts, PatternNode root)
        {
            var currentNode = root;
            var nodeStack = new Stack<PatternNode>();

            foreach (var part in parts)
            {
                if (!currentNode.Children.TryGetValue(part, out var nextNode))
                {
                    return; // Pattern doesn't exist
                }
                nodeStack.Push(currentNode);
                currentNode = nextNode;
            }

            currentNode.IsEndOfPattern = false;

            // Clean up the tree if there are no other patterns under this node
            for (int i = parts.Length - 1; i >= 0 && nodeStack.Count > 0; i--)
            {
                var parentNode = nodeStack.Pop();
                var part = parts[i];

                if (currentNode.Children.Count == 0 && !currentNode.IsEndOfPattern)
                {
                    parentNode.Children.Remove(part);
                    currentNode = parentNode;
                }
                else
                {
                    break;
                }
            }
        }

        // Search pattern matching inside tree
        public static bool SearchPatternInTree(string[] parts, PatternNode root)
        {
            var currentNode = root;
            for (int i = 0; i < parts.Length; i++)
            {
                var part = parts[i];

                // Direct match with the part
                if (currentNode.Children.TryGetValue(part, out var nextNode))
                {
                    currentNode = nextNode;
                }
                // Wildcard match, exit early if it's a match-all
                else if (currentNode.Children.TryGetValue("*", out nextNode))
                {
                    currentNode = nextNode;
                }
                else
                {
                    return false; // No match found
                }

                // Exit if we already reached the end of a valid pattern
                if (currentNode.IsEndOfPattern) return true;
            }

            return currentNode.IsEndOfPattern;
        }

    }
}
