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
    public readonly int idx = idx; // 0 ~ 8
    public string Hash() => idx.ToString();
}

/* Store information of a game state */
public class State(List<Play> history, Player[] board, Player player)
{
    public readonly List<Play> history = history;
    public readonly Player player = player;
    public readonly Player[] board = board;

    public string Hash()
    {
        string hash = player.ToStr();
        foreach (var p in history)
            hash += p.Hash();
        return hash;
    }

    public string HistoryToStr()
    {
        List<Player[]> boards = [new Player[9]];
        int last = 0;
        Player player = history.Count % 2 == 1 ? this.player.Opposite() : this.player;
        foreach (var p in history)
        {
            Player[] board = (Player[])boards[last].Clone();
            board[p.idx] = player;
            player = player.Opposite();
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
    /* Generate and return the initial game state */
    public static State Init(Player player) => new([], new Player[9], player);

    /* Return the current player's legal plays from given state */
    public static List<Play> GetLegalPlays(State state)
    {
        List<Play> legalPlays = [];

        int none = 0;

        for (int id = 0; id < 9; id++)
            if (state.board[id] == Player.NONE)
            {
                legalPlays.Add(new Play(id));
                none++;
            }

        // cut some symmetric
        if (none == 9)
            return [new(0), new(1), new(4)];
        if (none == 8 && state.board[4] != Player.NONE)
            return [new(0), new(1)];

        return legalPlays;
    }

    /* Advance the given state and return it */
    public static State GetNextState(State currentState, Play play)
    {
        // copy history to new list & push play to it
        List<Play> newHistory = new(currentState.history) { play };
        // copy board to new array
        Player[] newBoard = (Player[])currentState.board.Clone();
        // apply the play on new board
        newBoard[play.idx] = currentState.player;
        // create new state of new history & new board, player is changed since this is next turn
        return new State(newHistory, newBoard, currentState.player.Opposite());
    }

    /* Check and return the winner of the game */
    public static Player CheckWinner(State state)
    {
        List<List<int>> checks = [
            [0, 1, 2],
            [3, 4, 5],
            [6, 7, 8],
            [0, 3, 6],
            [1, 4, 7],
            [2, 5, 8],
            [0, 4, 8],
            [2, 4, 6]
        ];

        for (var i = 0; i < checks.Count; i++)
        {
            var check = checks[i];
            List<Player> checkArr = [];
            for (var j = 0; j < check.Count; j++)
                checkArr.Add(state.board[check[j]]);

            bool every(Player id)
            {
                foreach (var p in checkArr)
                    if (p != id)
                        return false;
                return true;
            }

            if (every(Player.ONE))
                return Player.ONE;
            if (every(Player.TWO))
                return Player.TWO;
        }

        if (state.board.IsFull())
            return Player.TIE;

        return Player.NONE;
    }
}
