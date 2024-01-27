namespace MCTS;

/* Monte Carlo Search Tree Node (using UCB1) */
class Node(Node? parent, Play? parentPlay, State state)
{
    public readonly State state = state; // 這個node代表的state: 現在的局面和現在是誰要下下一步棋
    public readonly Play? parentPlayerPlay = parentPlay; // (上個人下的)上一步棋是什麼
    public int parentPlayerWins = 0; // 這個Node(含)以下的模擬中，上個人贏了幾次
    public int totalPlays = 0; // 這個Node(含)以下模擬了幾次
    public readonly Node? parent = parent;
    public readonly List<Node> children = [];

    public bool IsLeaf => children.Count == 0;

    public double UCB1(double UCB1ExploreParam)
    {
        if (parent == null) // no needs to explore root node since it is the current actual game state
            return 0;
        if (totalPlays == 0) // if this node has not explored yet
            return double.MaxValue;
        if (parentPlayerWins < 0)
            return parentPlayerWins;
        double exploit = (double)parentPlayerWins / totalPlays;
        double explore = Math.Sqrt(UCB1ExploreParam * Math.Log(parent.totalPlays) / totalPlays);
        return exploit + explore;
    }

    public Node FindMaxUCB1Child(double UCB1ExploreParam)
    {
        Random rng = new();
        Node res = children[rng.Next(0, children.Count - 1)];
        double maxUCB = res.UCB1(UCB1ExploreParam);
        foreach (var n in children)
        {
            double newUCB = n.UCB1(UCB1ExploreParam);
            if (newUCB > maxUCB)
            {
                res = n;
                maxUCB = newUCB;
            }
        }
        return res;
    }

    public Node GetRandomChild()
    {
        Random rng = new(Guid.NewGuid().GetHashCode());
        return children[rng.Next(0, children.Count - 1)];
    }
}

public enum Policy { WinRate, MaxPlay }

/* Monte Carlo Search Tree (using UCB1) */
class UCT(Node root, double UCB1ExploreParam = 2)
{
    // The square of the bias parameter in the UCB1 algorithm
    readonly double UCB1ExploreParam = UCB1ExploreParam;
    readonly Random rng = new(Guid.NewGuid().GetHashCode());
    readonly Node root = root;

    public Play GetBestPlay(Policy policy)
    {
        Play? bestPlay = null;
        double max = double.MinValue;
        foreach (var child in root.children)
        {
            if (policy == Policy.MaxPlay && child.totalPlays > max)
            {
                bestPlay = child.parentPlayerPlay;
                max = child.totalPlays;
            }
            else if (policy == Policy.WinRate)
            {
                double rate = (double)child.parentPlayerWins / child.totalPlays;
                if (rate > max)
                {
                    bestPlay = child.parentPlayerPlay;
                    max = rate;
                }
            }

            // Console.WriteLine(child.parentPlayerPlay?.ToStr() + ":" + child.parentPlayerWins + "/" + child.totalPlays);
        }
        if (bestPlay == null)
            throw new Exception("Play not found");
        return (Play)bestPlay;
    }

    public void Search(int iteration)
    {
        while (iteration-- > 0)
            Iterate();
    }

    void Iterate()
    {
        Node leaf = Select(root);
        Player winner = Game.CheckWinner(leaf.state);
        if (winner == Player.NONE)
        {
            Expand(leaf);
            leaf = leaf.GetRandomChild();
        }
        winner = Rollout(leaf);
        Backpropogate(leaf, winner);
    }

    /* Select a leaf node with max UCB1 value */
    Node Select(Node root)
    {
        while (!root.IsLeaf)
            root = root.FindMaxUCB1Child(UCB1ExploreParam);
        return root;
    }

    /* Create all possible child of the node */
    static void Expand(Node leaf)
    {
        if (!leaf.IsLeaf)
            return;
        List<Play> possiblePlays = Game.GetLegalPlays(leaf.state);
        foreach (var play in possiblePlays)
        {
            State stateAfterPlay = Game.GetNextState(leaf.state, play);
            leaf.children.Add(new Node(leaf, play, stateAfterPlay));
        }
    }

    Player Rollout(Node leafNode)
    {
        State state = leafNode.state;
        Player winner = Game.CheckWinner(state);

        if (winner == leafNode.state.player &&
            winner == root.state.player.Opponent())
        {
            /* Here means that root player(leafNode.parent.state.player)
               will instantly lose if it choose this play(leafNode.parentPlayerPlay),
               so this node should not be choose afterward. */
            leafNode.parentPlayerWins = -2;
        }
        if (leafNode.parent != null &&
            winner == leafNode.parent.state.player &&
            winner == root.state.player.Opponent())
        {
            /* leafNode represent the result of one of possible plays of
               leafNode.parent.state.player, 
               and this result is instantly making root player lose,
               so leafNode.parent.parentPlayerPlay should not be selected afterward,
               since if leafNode.parent.state.player is smart enough,
               it will catch the chance. */
            leafNode.parent.parentPlayerWins = -1;
        }

        /* Randomly play until game complete */
        while (winner == Player.NONE)
        {
            List<Play> possiblePlays = Game.GetLegalPlays(state);
            Play play = possiblePlays[rng.Next(0, possiblePlays.Count - 1)];

            state = Game.GetNextState(state, play);
            winner = Game.CheckWinner(state);
        }

        return winner;
    }

    static void Backpropogate(Node leafNode, Player winner)
    {
        Node? node = leafNode;
        while (node != null)
        {
            if (node.state.player.Opponent() == winner)
                node.parentPlayerWins++;
            node.totalPlays++;

            node = node.parent;
        }
    }
}

public static class MCTS
{
    /* Search the next play by MCTS */
    public static Play Search(State currentState, int iteration, Policy policy = Policy.MaxPlay, double UCB1ExploreParam = 2)
    {
        UCT tree = new(new Node(null, null, currentState), UCB1ExploreParam);
        tree.Search(iteration);
        return tree.GetBestPlay(policy);
    }
}
