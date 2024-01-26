namespace MCTS;

public enum Player { NONE, ONE, TWO, TIE }
public static class PlayerExtension
{
    public static string ToStr(this Player player)
    {
        return player switch
        {
            Player.ONE => "O",
            Player.TWO => "X",
            Player.TIE => "TIE",
            _ => " ",
        };
    }
    public static Player Opposite(this Player player)
    {
        if (player == Player.NONE)
            return Player.NONE;
        return player == Player.ONE ? Player.TWO : Player.ONE;
    }
    public static bool IsFull(this Player[,] board)
    {
        foreach (var piece in board)
            if (piece == Player.NONE)
                return false;
        return true;
    }
    public static string Log(this Player[,] board)
    {
        string log = "";
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
                log += board[i, j].ToStr();
            log += Environment.NewLine;
        }
        return log;
    }
}

/* Store information of a play */
public class Play(int row, int col)
{
    public readonly int row = row;
    public readonly int col = col;
}

/* Store information of a game state */
public class State(List<Play> history, Player[,] board, Player player)
{
    public List<Play> history = history;
    public readonly Player player = player;
    public Player[,] board = board;

    public bool IsPlayer(Player player)
    {
        return this.player == player;
    }
}

public class Game
{
    /* Generate and return the initial game state */
    public static State Start(Player player)
    {
        Player[,] newBoard = new Player[3, 3];
        return new State([], newBoard, player);
    }

    /* Return the current player's legal plays from given state */
    public static List<Play> LegalPlays(State state)
    {
        List<Play> legalPlays = [];

        for (int row = 0; row < 3; row++)
            for (int col = 0; col < 3; col++)
                if (state.board[row, col] == Player.NONE)
                    legalPlays.Add(new Play(row, col));

        return legalPlays;
    }

    /* Advance the given state and return it */
    public static State NextState(State state, Play play)
    {
        // copy history to new list & push play to it
        List<Play> newHistory = new(state.history) { play };
        // copy board to new array
        Player[,] newBoard = (Player[,])state.board.Clone();
        // apply the play on new board
        newBoard[play.row, play.col] = state.player;
        // create new state of new history & new board, player is changed since this is next turn
        return new State(newHistory, newBoard, state.player.Opposite());
    }

    /* Check and return the winner of the game */
    public static Player Winner(State state)
    {
        for (int i = 0; i < 3; i++)
        {
            if (state.board[i, 0] != Player.NONE &&
                state.board[i, 0] == state.board[i, 1] &&
                state.board[i, 0] == state.board[i, 2])
                return state.board[i, 0];

            if (state.board[0, i] != Player.NONE &&
                state.board[0, i] == state.board[1, i] &&
                state.board[0, i] == state.board[2, i])
                return state.board[0, i];
        }

        if (state.board[1, 1] == Player.NONE)
            return Player.NONE;

        if (state.board[1, 1] == state.board[0, 0] &&
            state.board[1, 1] == state.board[2, 2])
            return state.board[1, 1];

        if (state.board[1, 1] == state.board[0, 2] &&
            state.board[1, 1] == state.board[2, 0])
            return state.board[1, 1];

        if (state.board.IsFull())
            return Player.TIE;

        return Player.NONE;
    }
}