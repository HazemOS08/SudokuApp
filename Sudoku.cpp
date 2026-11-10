
#include <cstdlib>

#include <ctime>
using namespace std;

const int Col = 9;

bool CheckElementsInRow(int M[][Col], int n, int m, int row)
{
    bool seen[10] = {false};
    for (int j = 0; j < m; j++)
    {

        int v = M[row][j];
        if (v == 0)
            continue;

        if (seen[v])
            return true;

        seen[v] = true;
    }

    return false;
}
bool CheckElementsInColumn(int M[][Col], int n, int m, int column)
{
    bool seen[10] = {false};

    for (int i = 0; i < n; i++)
    {

        int v = M[i][column];

        if (v == 0)
            continue;

        if (seen[v])
            return true;

        seen[v] = true;
    }
    return false;
}

bool CheckElementsInSubMatrix(int M[][Col], int n, int m, int subN, int subM)
{
    for (int startX = 0; startX < n; startX += subN)
    {
        for (int startY = 0; startY < m; startY += subM)
        {
            bool seen[10] = {false};
            for (int i = startX; i < startX + subN; i++)
            {
                for (int j = startY; j < startY + subM; j++)
                {
                    int v = M[i][j];
                    if (v == 0)
                        continue;
                    if (seen[v])
                        return true;
                    seen[v] = true;
                }
            }
        }
    }
    return false;
}

bool CheckAll(int M[][Col], int n, int m, int subN, int subM)
{

    // checking all columns

    for (int j = 0; j < m; j++)
        if (CheckElementsInColumn(M, n, m, j))
            return false;

    // checking all rows

    for (int i = 0; i < n; i++)
        if (CheckElementsInRow(M, n, m, i))
            return false;

    // if all rows and columns are checked true then check submatrix

    return !CheckElementsInSubMatrix(M, n, m, subN, subM);

    // everything is OK
}

void initSpace1(int M[][Col], int n, int m)
{
    // 1 to 9 digits
    int digit[9];
    for (int i = 0; i < 9; i++)
        digit[i] = i + 1;

    // random order
    for (int i = 8; i > 0; i--)
    {
        int k = rand() % (i + 1);
        int t = digit[i];
        digit[i] = digit[k];
        digit[k] = t;
    }

    // valid grid
    for (int r = 0; r < 9; r++)
        for (int c = 0; c < 9; c++)
            M[r][c] = digit[(3 * (r % 3) + r / 3 + c) % 9];

    // removing 
    int removed = 0;
    while (removed < 68)
    {
        int p = rand() % 81;
        if (M[p / 9][p % 9] != 0)
        {
            M[p / 9][p % 9] = 0;
            removed++;
        }
    }
}

extern "C"
{
    // generate
    __declspec(dllexport) void GenerateNewBoard(int M[][Col], int n, int m)
    {
        std::srand(static_cast<unsigned>(std::time(0))); // different bord each time
        initSpace1(M, n, m);
    }

    // check if a move is valid
    __declspec(dllexport) bool HasConflict(int M[][Col], int n, int m, int row, int column)
    {
        return (CheckElementsInRow(M, n, m, row) ||
                CheckElementsInColumn(M, n, m, column) ||
                CheckElementsInSubMatrix(M, n, m, 3, 3));
    }

    // check if the entire board is fully solved and valid
    __declspec(dllexport) bool IsBoardFullySolved(int M[][Col], int n, int m)
    {

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < m; j++)
            {
                if (M[i][j] == 0)
                {
                    return false;
                }
            }
        }
        return CheckAll(M, n, m, 3, 3);
    }
}