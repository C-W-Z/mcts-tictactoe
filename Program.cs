using MCTS;

Random rng = new();
State state = Game.Start(Player.ONE);
Player winner = Game.Winner(state);

while (winner == Player.NONE)
{
    List<Play> plays = Game.LegalPlays(state);
    Play play = plays[rng.Next(0, plays.Count-1)];
    state = Game.NextState(state, play);
    winner = Game.Winner(state);
}

Console.Write(state.board.Log());
Console.WriteLine("Winner is {0}", winner.ToStr());
