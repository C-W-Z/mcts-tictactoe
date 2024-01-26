using MCTS;

Player first = Player.ONE;
State state = Game.Init(first);
Player winner = Game.CheckWinner(state);

UCT mcts = new();

while (winner == Player.NONE)
{
    mcts.RunSearch(state);
    Play play = mcts.GetBestPlay(state);
    state = Game.GetNextState(state, play);
    winner = Game.CheckWinner(state);
}

Console.WriteLine(state.board.Log());
Console.WriteLine(winner.ToStr());
