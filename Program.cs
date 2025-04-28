using System;
using System.Collections.Generic;
using System.Linq;

namespace SocialNetworkAnalysis
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to the Social Network Influence Calculator!");

            // --- Scalability Note ---
            // The current implementation uses hardcoded graph data within the
            // Demo methods. For real-world applications and larger datasets,
            // reading graph data from external files (e.g., text files, databases)
            // is highly recommended for better scalability, flexibility, and maintainability.
            // See the alternative implementation provided previously for an example using file input.
            // --- End Scalability Note ---

            Console.WriteLine("\nChoose the graph type (using hardcoded demo data):");
            Console.WriteLine("1. Unweighted Graph");
            Console.WriteLine("2. Weighted Graph");

            // Basic input validation
            if (int.TryParse(Console.ReadLine(), out int choice))
            {
                if (choice == 1)
                    UnweightedGraphDemo();
                else if (choice == 2)
                    WeightedGraphDemo();
                else
                    Console.WriteLine("Invalid choice.");
            }
            else
            {
                Console.WriteLine("Invalid input. Please enter 1 or 2.");
            }

            Console.WriteLine("\nPress Enter to exit.");
            Console.ReadLine();
        }

        static void UnweightedGraphDemo()
        {
            // Hardcoded graph data (as in the original code)
            var graph = new Dictionary<string, List<string>>
            {
                { "Alicia", new List<string> { "Britney" } },
                { "Britney", new List<string> { "Claire" } },
                { "Claire", new List<string> { "Diana" } },
                { "Diana", new List<string> { "Edward", "Harry" } },
                { "Edward", new List<string> { "Harry", "Gloria", "Fred" } },
                { "Harry", new List<string> { "Gloria" } },
                { "Gloria", new List<string> { "Fred" } },
                { "Fred", new List<string>() }
                // Example of a disconnected node:
                // ,{ "Isolated", new List<string>() }
            };
            // Ensure all nodes mentioned are keys
            var allNodes = new HashSet<string>(graph.Keys);
            foreach (var neighbors in graph.Values)
            {
                foreach (var neighbor in neighbors)
                {
                    allNodes.Add(neighbor);
                }
            }
            foreach (var node in allNodes)
            {
                if (!graph.ContainsKey(node))
                {
                    graph.Add(node, new List<string>());
                }
            }


            Console.WriteLine("\nList 1: edge_list of unweighted social network (Demo Data)");
            Console.WriteLine("Node1 -> Node2");
            int edgeCount = 0;
            foreach (var node in graph.OrderBy(kv => kv.Key)) // Sort for consistent output
            {
                foreach (var neighbor in node.Value.OrderBy(n => n)) // Sort neighbors
                {
                    Console.WriteLine($"{node.Key} -> {neighbor}");
                    edgeCount++;
                }
            }
            Console.WriteLine($"Graph has {graph.Count} nodes and {edgeCount} edges.");


            Console.WriteLine("\nUnweighted Graph Influence Scores (Normalized Closeness Centrality 0-1):");
            Console.WriteLine("----------------------------------------------------");
            var scores = new Dictionary<string, double>();
            foreach (var node in graph.Keys)
            {
                scores[node] = CalculateInfluenceScoreUnweighted(graph, node);
            }

            // Sort scores descending for better readability
            var sortedScores = scores.OrderByDescending(kvp => kvp.Value);
            foreach (var kvp in sortedScores)
            {
                Console.WriteLine($"{kvp.Key,-15}: {kvp.Value:F4}"); // Format score to 4 decimal places
            }
            Console.WriteLine("----------------------------------------------------");
            Console.WriteLine("Note: Score reflects reachability and average distance.");

        }

        // Calculates Normalized Closeness Centrality for Unweighted Graphs
        static double CalculateInfluenceScoreUnweighted(Dictionary<string, List<string>> graph, string startNode)
        {
            int N = graph.Count; // Total number of nodes in the graph
            if (N <= 1) return 0.0; // Score is 0 for single-node or empty graph

            // --- BFS for Shortest Paths ---
            var distances = new Dictionary<string, int>();
            foreach (var node in graph.Keys)
                distances[node] = -1; // Use -1 to indicate unreachability initially

            distances[startNode] = 0;
            var queue = new Queue<string>();
            queue.Enqueue(startNode);
            int reachableCount = 0; // Counts reachable nodes including startNode

            while (queue.Count > 0)
            {
                string current = queue.Dequeue();
                reachableCount++; // Count nodes for which we determine the shortest path

                // Check if the key exists before accessing neighbours - important if graph was built dynamically
                if (!graph.ContainsKey(current)) continue;

                foreach (var neighbor in graph[current])
                {
                    // Ensure neighbor key exists before checking distance
                    if (distances.ContainsKey(neighbor) && distances[neighbor] == -1) // If neighbor hasn't been visited yet
                    {
                        distances[neighbor] = distances[current] + 1;
                        queue.Enqueue(neighbor);
                    }
                }
            }

            // --- Calculate Normalized Closeness ---
            // Sum distances only to reachable nodes (excluding start node)
            // d > 0 ensures we don't count the distance to the start node itself
            double sumDistances = distances.Values.Where(d => d > 0).Sum();
            int n = reachableCount; // Number of nodes reachable from startNode (including itself)

            // --- Edge Case Handling ---
            // If n <= 1, the node only reaches itself (or the graph is just this node). Score is 0.
            // If sumDistances is 0, it means the node only reached itself (n=1) or only nodes with distance 0 (not possible here). Score is 0.
            // If the graph is disconnected, 'n' will be the size of the connected component containing startNode.
            // The normalization factor (n-1)/(N-1) accounts for the proportion of the graph reached.
            // ---
            if (n <= 1 || sumDistances == 0)
            {
                return 0.0;
            }

            // Closeness formula: (Number of reachable nodes - 1) / Sum of distances to reachable nodes
            double closeness = (double)(n - 1) / sumDistances;

            // Normalization factor: (Number of reachable nodes - 1) / (Total nodes in graph - 1)
            // This scales the score based on the fraction of the network that is reachable.
            double normalizationFactor = (double)(n - 1) / (N - 1);

            // Normalized Score (should be between 0 and 1)
            double score = closeness * normalizationFactor;

            // Clamp score just in case of potential floating point inaccuracies
            return Math.Max(0.0, Math.Min(1.0, score));
        }

        static void WeightedGraphDemo()
        {
            // Hardcoded graph data (as in the original code)
            var graph = new Dictionary<string, List<(string Neighbor, int Weight)>>
            {
                { "A", new List<(string, int)> { ("B", 1), ("C", 1), ("E", 5) } },
                { "B", new List<(string, int)> { ("C", 4), ("E", 1), ("G", 1), ("H", 1) } },
                { "C", new List<(string, int)> { ("D", 3), ("E", 1) } },
                { "D", new List<(string, int)> { ("E", 2), ("F", 1), ("G", 5) } },
                { "E", new List<(string, int)> { ("G", 2) } },
                { "F", new List<(string, int)> { ("G", 1) } },
                { "G", new List<(string, int)> { ("H", 2) } },
                { "H", new List<(string, int)> { ("I", 3) } },
                { "I", new List<(string, int)> { ("J", 3) } },
                { "J", new List<(string, int)>() }
                 // Example of a disconnected component:
                 //,{ "K", new List<(string, int)> { ("L", 2)} },
                 //{ "L", new List<(string, int)>() }
            };
            // Ensure all nodes mentioned are keys
            var allNodes = new HashSet<string>(graph.Keys);
            foreach (var neighborsList in graph.Values)
            {
                foreach (var (neighbor, weight) in neighborsList)
                {
                    allNodes.Add(neighbor);
                }
            }
            foreach (var node in allNodes)
            {
                if (!graph.ContainsKey(node))
                {
                    graph.Add(node, new List<(string, int)>());
                }
            }

            Console.WriteLine("\nList 2: edge_list of weighted social network (Demo Data)");
            Console.WriteLine("Node1 -> Node2 (Weight)");
            int edgeCount = 0;
            foreach (var node in graph.OrderBy(kv => kv.Key)) // Sort for consistent output
            {
                foreach (var (neighbor, weight) in node.Value.OrderBy(t => t.Neighbor)) // Sort neighbors
                {
                    Console.WriteLine($"{node.Key} -> {neighbor} ({weight})");
                    edgeCount++;
                }
            }
            Console.WriteLine($"Graph has {graph.Count} nodes and {edgeCount} edges.");


            Console.WriteLine("\nWeighted Graph Influence Scores (Normalized Closeness Centrality 0-1):");
            Console.WriteLine("----------------------------------------------------");
            var scores = new Dictionary<string, double>();
            foreach (var node in graph.Keys)
            {
                scores[node] = CalculateInfluenceScoreWeighted(graph, node);
            }
            // Sort scores descending for better readability
            var sortedScores = scores.OrderByDescending(kvp => kvp.Value);
            foreach (var kvp in sortedScores)
            {
                Console.WriteLine($"{kvp.Key,-15}: {kvp.Value:F4}"); // Format score to 4 decimal places
            }
            Console.WriteLine("----------------------------------------------------");
            Console.WriteLine("Note: Score reflects reachability and average distance (using edge weights).");
        }

        // Calculates Normalized Closeness Centrality for Weighted Graphs
        static double CalculateInfluenceScoreWeighted(Dictionary<string, List<(string Neighbor, int Weight)>> graph, string startNode)
        {
            int N = graph.Count; // Total number of nodes in the graph
            if (N <= 1) return 0.0; // Score is 0 for single-node or empty graph

            // --- Dijkstra's Algorithm for Shortest Paths ---
            var distances = new Dictionary<string, double>(); // Use double for potential large sums
            foreach (var node in graph.Keys)
                distances[node] = double.PositiveInfinity; // Use Infinity for unreachability

            distances[startNode] = 0;
            // Using SortedSet as a basic priority queue (Node: Distance, NodeName)
            // In .NET 6+, PriorityQueue<string, double> is more efficient
            var priorityQueue = new SortedSet<(double Distance, string Node)>(Comparer<(double Distance, string Node)>.Create((a, b) =>
            {
                int distCompare = a.Distance.CompareTo(b.Distance);
                if (distCompare == 0) return a.Node.CompareTo(b.Node); // Tie-breaking using Node name
                return distCompare;
            }));

            priorityQueue.Add((0, startNode));
            var visited = new HashSet<string>(); // Keep track of nodes for which shortest path is finalized
            int reachableCount = 0;

            while (priorityQueue.Count > 0)
            {
                // Extract node with smallest distance
                var (currentDistance, currentNode) = priorityQueue.Min;
                priorityQueue.Remove(priorityQueue.Min);

                // If already visited or unreachable, skip
                // Checking visited prevents cycles and redundant work
                // Checking Infinity handles components processed out of order in some edge cases
                if (visited.Contains(currentNode) || double.IsPositiveInfinity(currentDistance))
                {
                    continue;
                }

                visited.Add(currentNode);
                reachableCount++; // Count finalized reachable nodes

                // Check if the key exists before accessing neighbours
                if (!graph.ContainsKey(currentNode)) continue;

                // Explore neighbors
                foreach (var (neighbor, weight) in graph[currentNode])
                {
                    // Skip if neighbor already finalized
                    if (visited.Contains(neighbor)) continue;

                    // Dijkstra requires positive weights. If weights could be non-positive, this needs adjustment.
                    if (weight <= 0) continue; // Skip non-positive weights as they violate Dijkstra's assumption

                    double newDistance = currentDistance + weight;

                    // If found a shorter path to the neighbor
                    if (newDistance < distances[neighbor])
                    {
                        // If the neighbor was already in the queue with a higher distance,
                        // removing it ensures we process the shorter path first.
                        // Note: Removing from SortedSet can be O(log N)
                        priorityQueue.Remove((distances[neighbor], neighbor));

                        distances[neighbor] = newDistance;
                        priorityQueue.Add((newDistance, neighbor));
                    }
                }
            }

            // --- Calculate Normalized Closeness ---
            // Sum distances only to reachable nodes (excluding start node)
            // Filter out PositiveInfinity and distance to self (which is 0)
            double sumDistances = distances.Values.Where(d => d > 0 && !double.IsPositiveInfinity(d)).Sum();
            int n = reachableCount; // Number of nodes reachable from startNode (including itself)

            // --- Edge Case Handling ---
            // Similar to unweighted: if n <= 1 or sumDistances is 0, score is 0.
            // Disconnected components result in smaller 'n', reducing the score via the (n-1)/(N-1) factor.
            // Weighted distances are used in 'sumDistances'.
            // ---
            if (n <= 1 || sumDistances == 0)
            {
                return 0.0;
            }

            // Closeness formula: (Number of reachable nodes - 1) / Sum of distances to reachable nodes
            double closeness = (double)(n - 1) / sumDistances;

            // Normalization factor: (Number of reachable nodes - 1) / (Total nodes in graph - 1)
            double normalizationFactor = (double)(n - 1) / (N - 1);

            // Normalized Score (should be between 0 and 1)
            double score = closeness * normalizationFactor;

            // Clamp score just in case of potential floating point inaccuracies
            return Math.Max(0.0, Math.Min(1.0, score));
        }
    }
}