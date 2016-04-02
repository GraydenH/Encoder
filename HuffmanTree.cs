using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Node {
    public int Symbol { get; set; }
    public int Frequency { get; set; }
    public Node Right { get; set; }
    public Node Left { get; set; }

    public List<bool> Traverse(int symbol, List<bool> data) {
        // Leaf
        if (Right == null && Left == null) {
            if (symbol.Equals(Symbol)) {
                return data;
            } else {
                return null;
            }
        } else {
            List<bool> left = null;
            List<bool> right = null;

            if (Left != null) {
                List<bool> leftPath = new List<bool>();
                leftPath.AddRange(data);
                leftPath.Add(false);

                left = Left.Traverse(symbol, leftPath);
            }

            if (Right != null) {
                List<bool> rightPath = new List<bool>();
                rightPath.AddRange(data);
                rightPath.Add(true);
                right = Right.Traverse(symbol, rightPath);
            }

            if (left != null) {
                return left;
            } else {
                return right;
            }
        }
    }
}

public class HuffmanTree {
    private List<Node> nodes = new List<Node>();
    public Node Root { get; set; }
    public Dictionary<int, int> Frequencies = new Dictionary<int, int>();

    public void Build(List<int> source) {
        for (int i = 0; i < source.Count; i++) {
            if (!Frequencies.ContainsKey(source[i])) {
                Frequencies.Add(source[i], 0);
            }

            Frequencies[source[i]]++;
        }

        foreach (KeyValuePair<int, int> symbol in Frequencies) {
            nodes.Add(new Node() { Symbol = symbol.Key, Frequency = symbol.Value });
        }

        while (nodes.Count > 1) {
            List<Node> orderedNodes = nodes.OrderBy(node => node.Frequency).ToList();

            if (orderedNodes.Count >= 2) {
                // Take first two items
                List<Node> taken = orderedNodes.Take(2).ToList();

                // Create a parent node by combining the frequencies
                Node parent = new Node() {
                    Symbol = '*',
                    Frequency = taken[0].Frequency + taken[1].Frequency,
                    Left = taken[0],
                    Right = taken[1]
                };

                nodes.Remove(taken[0]);
                nodes.Remove(taken[1]);
                nodes.Add(parent);
            }

            Root = nodes.FirstOrDefault();

        }

    }

    public BitArray Encode(List<int> source) {
        List<bool> encodedSource = new List<bool>();

        for (int i = 0; i < source.Count; i++) {
            List<bool> encodedSymbol = Root.Traverse(source[i], new List<bool>());
            encodedSource.AddRange(encodedSymbol);
        }

        BitArray bits = new BitArray(encodedSource.ToArray());

        return bits;
    }

    public List<int> Decode(BitArray bits) {
        Node current = Root;
        List<int> decoded = new List<int>();

        foreach (bool bit in bits) {
            if (bit) {
                if (current.Right != null) {
                    current = current.Right;
                }
            } else {
                if (current.Left != null) {
                    current = current.Left;
                }
            }

            if (IsLeaf(current)) {
                decoded.Add(current.Symbol);
                current = Root;
            }
        }

        return decoded;
    }

    public bool IsLeaf(Node node) {
        return (node.Left == null && node.Right == null);
    }

}