using MCTS;

const int TotalGame = 1000;
int Player1Wins = 0;
int Player2Wins = 0;
int Tie = 0;
int game = TotalGame;

Player first = Player.ONE;

while (game-- > 0)
{
    State state = Game.Init(first);
    Player winner = Game.CheckWinner(state);

    while (winner == Player.NONE)
    {
        // Console.Write(state.board.ToStr());

        // Console.WriteLine();

        // Console.WriteLine("Player: {0}", state.player.ToStr());
        Play play = MCTS.MCTS.Search(state, 2000, Policy.WinRate);
        // Console.WriteLine("Choose: {0}", play.ToStr());

        state = Game.GetNextState(state, play);
        winner = Game.CheckWinner(state);

        // Console.WriteLine();
    }

    // Console.Write(state.board.ToStr());
    // Console.WriteLine();
    // Console.WriteLine("Winner: {0}", winner.ToStr());

    if (winner == Player.ONE)
        Player1Wins++;
    else if (winner == Player.TWO)
        Player2Wins++;
    else
        Tie++;
}

Console.WriteLine("Player O Wins: {0}", Player1Wins);
Console.WriteLine("Player X Wins: {0}", Player2Wins);
Console.WriteLine("Ties: {0}", Tie);
