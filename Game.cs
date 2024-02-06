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
    public static Player Opponent(this Player player)
    {
        if (player == Player.NONE || player == Player.TIE)
            return player;
        return player == Player.ONE ? Player.TWO : Player.ONE;
    }
    public static bool IsFull(this Player[] board)
    {
        foreach (var piece in board)
            if (piece == Player.NONE)
                return false;
        return true;
    }
    public static string ToStr(this Player[] board)
    {
        string res = "";
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
                res += board[i * 3 + j].ToStr();
            res += Environment.NewLine;
        }
        return res;
    }
}

/* Store information of a play */
public readonly struct Move(int pos)
{
    public readonly int pos = pos; // 棋要下在哪裡：0 ~ 8
    public string ToStr() => pos.ToString();
}

/* Store information of a game state */
public class State(Player[] board, Player player)
{
    public readonly Player[] board = board; // 現在要下棋的人面對的局面
    public readonly Player player = player; // 現在換誰下棋
}

public class Game
{
    /* Generate a new empty board and assign firstMover as the first mover */
    public static State GetInitState(Player firstMover) => new(new Player[9], firstMover);

    /* Return the current player's legal plays from given state */
    public static List<Move> GetLegalPlays(State state)
    {
        List<Move> legalPlays = [];

        for (int id = 0; id < 9; id++)
            if (state.board[id] == Player.NONE)
                legalPlays.Add(new Move(id));

        return legalPlays;
    }

    /* Apply newPlay on currentState & return that state */
    public static State GetNextState(State currentState, Move newPlay)
    {
        // copy history to new list & push play to it
        // copy board to new array
        Player[] newBoard = (Player[])currentState.board.Clone();
        // apply the play on new board
        newBoard[newPlay.pos] = currentState.player;
        // create new state of new history & new board, player is changed since this is next turn
        return new State(newBoard, currentState.player.Opponent());
    }

    static readonly List<List<int>> checks = [
        [0, 1, 2],
        [3, 4, 5],
        [6, 7, 8],
        [0, 3, 6],
        [1, 4, 7],
        [2, 5, 8],
        [0, 4, 8],
        [2, 4, 6]
    ];

    /* Check and return the winner of the game */
    public static Player CheckWinner(State state)
    {
        bool checkLine(List<int> line, Player id)
        {
            for (int j = 0; j < line.Count; j++)
                if (state.board[line[j]] != id)
                    return false;
            return true;
        }

        for (int i = 0; i < checks.Count; i++)
        {
            if (checkLine(checks[i], Player.ONE))
                return Player.ONE;
            if (checkLine(checks[i], Player.TWO))
                return Player.TWO;
        }

        if (state.board.IsFull())
            return Player.TIE;

        // game is not complete yet
        return Player.NONE;
    }
}
