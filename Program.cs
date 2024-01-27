using MCTS;

const int TotalGame = 1;
int player1Wins = 0;
int player2Wins = 0;
int ties = 0;
int gameCount = TotalGame;

Player first = Player.ONE;

UCT mcts = new();
State state = Game.Init(first);

while (gameCount-- > 0)
{
    // UCT mcts = new();

    state = Game.Init(first);
    Player winner = Game.CheckWinner(state);
    while (winner == Player.NONE)
    {
        mcts.RunSearch(state, 3000);

        Console.Write(state.HistoryToStr());
        Console.Write(mcts.GetStats(state));

        Play play = mcts.GetBestPlay(state, UCTPolicy.MaxPlay);
        state = Game.GetNextState(state, play);
        winner = Game.CheckWinner(state);
    }
    if (winner == Player.ONE)
        player1Wins++;
    else if (winner == Player.TWO)
        player2Wins++;
    else
        ties++;

    first = first.Opposite();
}

Console.Write(state.HistoryToStr());
// state = Game.Init(first);
// Console.WriteLine(state.Hash());
Console.Write(mcts.GetStats(state));
Console.WriteLine(player1Wins);
Console.WriteLine(player2Wins);
Console.WriteLine(ties);
Console.WriteLine(mcts.nodes.Count);
