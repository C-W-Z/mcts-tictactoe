using MCTS;

Random rng = new();

const int TotalGame = 1000;
int player1Wins = 0;
int player2Wins = 0;
int gameCount = TotalGame;

Player first = Player.ONE;

while (gameCount-- > 0)
{
    // play new game
    State state = Game.Init(first);
    Player winner = Game.CheckWinner(state);
    while (winner == Player.NONE)
    {
        List<Play> plays = Game.GetLegalPlays(state);
        Play play = plays[rng.Next(0, plays.Count-1)];
        state = Game.GetNextState(state, play);
        winner = Game.CheckWinner(state);
    }
    if (winner == Player.ONE)
        player1Wins++;
    else if (winner == Player.TWO)
        player2Wins++;
    // if without this line, Player.One will has win rate about 0.6 since first mover advantage
    first = first.Opposite();
}

Console.WriteLine((float)player1Wins / TotalGame);
Console.WriteLine((float)player2Wins / TotalGame);
