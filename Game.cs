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
public readonly struct Play(int idx)
{
    public readonly int idx = idx; // 棋要下在哪裡：0 ~ 8
    public string ToStr() => idx.ToString();
}

/* Store information of a game state */
public class State(List<Play> history, Player[] board, Player player)
{
    public readonly List<Play> history = history;
    public readonly Player player = player; // 現在換誰下棋
    public readonly Player[] board = board; // 現在要下棋的人面對的局面

    public string Hash()
    {
        string hash = player.ToStr();
        foreach (var p in history)
            hash += p.ToStr();
        return hash;
    }

    public string HistoryToStr()
    {
        List<Player[]> boards = [new Player[9]];
        int last = 0;
        Player player = history.Count % 2 == 1 ? this.player.Opponent() : this.player;
        foreach (var p in history)
        {
            Player[] board = (Player[])boards[last].Clone();
            board[p.idx] = player;
            player = player.Opponent();
            boards.Add(board);
            last++;
        }
        string[] line = ["|", "|", "|"];
        boards.RemoveAt(0);
        foreach (var b in boards)
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                    line[i] += b[i * 3 + j].ToStr();
                line[i] += '|';
            }
        string res = "";
        for (int i = 0; i < 3; i++)
            res += line[i] + Environment.NewLine;
        return res;
    }
}

public class Game
{
    /* Generate a new empty board and assign firstMover as the first mover */
    public static State Init(Player firstMover) => new([], new Player[9], firstMover);

    /* Return the current player's legal plays from given state */
    public static List<Play> GetLegalPlays(State state)
    {
        List<Play> legalPlays = [];

        for (int id = 0; id < 9; id++)
            if (state.board[id] == Player.NONE)
            {
                legalPlays.Add(new Play(id));
            }

        return legalPlays;
    }

    /* Apply newPlay on currentState & return that state */
    public static State GetNextState(State currentState, Play newPlay)
    {
        // copy history to new list & push play to it
        List<Play> newHistory = new(currentState.history) { newPlay };
        // copy board to new array
        Player[] newBoard = (Player[])currentState.board.Clone();
        // apply the play on new board
        newBoard[newPlay.idx] = currentState.player;
        // create new state of new history & new board, player is changed since this is next turn
        return new State(newHistory, newBoard, currentState.player.Opponent());
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
