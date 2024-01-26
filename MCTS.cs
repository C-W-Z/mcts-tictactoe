namespace MCTS;

/* Monte Carlo Search Tree Node (using UCB1) */
public class Node
{
    public Play? play;
    public State state;
    public int wins;
    public int plays;
    public Node? parent;
    public Dictionary<Play, Node?> children;

    public Node(Play? play, State state, Node? parent, List<Play> unexpandedPlays)
    {
        this.play = play;
        this.state = state;
        this.parent = parent;
        this.wins = 0;
        this.plays = 0;
        this.children = [];
        foreach (var p in unexpandedPlays)
            children.Add(p, null);
    }

    public Node GetChildNode(Play play)
    {
        if (children.TryGetValue(play, out Node? node))
        {
            if (node == null)
                throw new Exception("Such child is not expanded.");
            return node;
        }
        throw new Exception("No such play.");
    }

    public Node Expand(Play play, State state, List<Play> unexpandedPlays)
    {
        Node child = new(play, state, this, unexpandedPlays);
        children[play] = child; // will throw KeyNotFoundException if play is not found
        return child;
    }

    public List<Play> GetAllPlays()
    {
        List<Play> allPlays = [];
        foreach (var key in children.Keys)
            allPlays.Add(key);
        return allPlays;
    }

    public List<Play> GetUnexpandedPlays()
    {
        List<Play> unexpandedPlays = [];
        foreach (var keyNVal in children)
            if (keyNVal.Value == null)
                unexpandedPlays.Add(keyNVal.Key);
        return unexpandedPlays;
    }

    public bool IsFullyExpanded()
    {
        foreach (var val in children.Values)
            if (val == null)
                return false;
        return true;
    }

    public bool IsLeaf => children.Count == 0;

    public double UCB1(double UCB1ExploreParam)
    {
        if (parent == null || parent.plays == 0 || plays == 0)
            return 0;
        return (double)wins / plays + Math.Sqrt(UCB1ExploreParam * Math.Log10(parent.plays) / plays);
    }
}

public enum UCTPolicy { Random, WinRate, MaxPlay }

/* Monte Carlo Search Tree (using UCB1) */
public class UCT(int UCB1ExploreParam = 2)
{
    // The square of the bias parameter in the UCB1 algorithm
    readonly int UCB1ExploreParam = UCB1ExploreParam;
    readonly Dictionary<State, Node> nodes = [];

    readonly Random rng = new();

    public void MakeNode(State state)
    {
        if (nodes.ContainsKey(state))
            return;
        List<Play> unexpandedPlays = Game.GetLegalPlays(state);
        Node node = new(null, state, null, unexpandedPlays);
        nodes.Add(state, node);
    }

    public void RunSearch(State state, int time = 1000)
    {
        MakeNode(state);

        while (time-- > 0)
        {
            Node node = Select(state);
            Player winner = Game.CheckWinner(node.state);
            if (!node.IsLeaf && winner == Player.NONE) {
                node = Expand(node);
                winner = Simulate(node);
            }
            Backpropagate(node, winner);
        }
    }

    public Play GetBestPlay(State state, UCTPolicy policy = UCTPolicy.MaxPlay)
    {
        MakeNode(state);

        // If not all children are expanded, not enough information
        if (!nodes[state].IsFullyExpanded())
            throw new Exception("Not enough information!");

        Node node = nodes[state];
        List<Play> allPlays = node.GetAllPlays();

        Play bestPlay = allPlays[rng.Next(0, allPlays.Count - 1)];

        if (policy == UCTPolicy.Random)
            return bestPlay;

        double max = double.MinValue;
        foreach (var p in allPlays)
        {
            Node child = node.GetChildNode(p);

            if (policy == UCTPolicy.WinRate)
            {
                double winrate = (double)child.wins / child.plays;
                if (winrate > max)
                {
                    bestPlay = p;
                    max = winrate;
                }
            }
            else if (policy == UCTPolicy.MaxPlay && child.plays > max)
            {
                bestPlay = p;
                max = child.plays;
            }
        }

        return bestPlay;
    }

    public Node Select(State state)
    {
        Node node = nodes[state];
        while (!node.IsLeaf && node.IsFullyExpanded())
        {
            List<Play> plays = node.GetAllPlays();
            Play bestPlay = plays[rng.Next(0, plays.Count - 1)];;
            double bestUCB1 = double.MinValue;
            foreach (var p in plays)
            {
                Node? child = node.children[p] ?? throw new Exception("Child not expanded");
                double UCB1 = child.UCB1(UCB1ExploreParam);
                if (UCB1 > bestUCB1)
                {
                    bestPlay = p;
                    bestUCB1 = UCB1;
                }
            }
            node = node.GetChildNode(bestPlay);
        }
        return node;
    }

    public Node Expand(Node node)
    {
        List<Play> plays = node.GetUnexpandedPlays();
        Play play = plays[rng.Next(0, plays.Count - 1)];

        State childState = Game.GetNextState(node.state, play);
        List<Play> childUnexpandedPlays = Game.GetLegalPlays(childState);
        Node childNode = node.Expand(play, childState, childUnexpandedPlays);
        nodes.Add(childState, childNode);

        return childNode;
    }

    public Player Simulate(Node node)
    {
        State state = node.state;
        Player winner = Game.CheckWinner(state);

        while (winner == Player.NONE)
        {
            List<Play> plays = Game.GetLegalPlays(state);
            Play play = plays[rng.Next(0, plays.Count-1)];
            state = Game.GetNextState(state, play);
            winner = Game.CheckWinner(state);
        }

        return winner;
    }

    public static void Backpropagate(Node? node, Player winner)
    {
        while (node != null) {
            node.plays++;
            // Parent's choice
            if (node.state.player == winner.Opposite())
                node.wins++;
            node = node.parent;
        }
    }
}
