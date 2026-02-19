---
name: Implement Level 3 Gameplay
overview: Implement Level 3 functionality that begins after Level 2 completion. The level erases inner 5x5 grid (except number 1) and requires players to fill numbers 2-25 following intersection and diagonal placement rules with undo capability.
todos:
  - id: backend-gamelogic
    content: Implement Level 3 placement validation and expansion in GameLogic.cs
    status: pending
  - id: backend-service
    content: Add Level 3 support in GameStateService.cs (CreateNewGame, ExpandToLevel3)
    status: pending
  - id: backend-api
    content: Add expand-level3 endpoint in GameController.cs and DTOs
    status: pending
  - id: frontend-api
    content: Add expandToLevel3 function in gameApi.ts
    status: pending
  - id: frontend-select-level
    content: Add Level 3 card to SelectLevelPage.tsx with unlock logic
    status: pending
  - id: frontend-game-page
    content: Add Level 3 expansion handler and UI updates in GamePage.tsx
    status: pending
  - id: testing
    content: Test Level 3 gameplay, validation rules, and expansion flow
    status: pending
isProject: false
---

# Level 3 Implementation Plan

## Overview

Level 3 begins when Level 2 is completed. It uses the same 7x7 board but erases the inner 5x5 grid (except for the cell containing number 1), requiring the player to fill numbers 2-25 back into the inner grid following new placement rules.

## Key Requirements Analysis

Based on the clarifications:

- **Yellow corners**: The 4 outer corners `[0,0]`, `[0,6]`, `[6,0]`, `[6,6]`
- **Outer ring visibility**: Numbers 2-25 remain visible on the outer ring
- **Diagonal cells**: All 13 cells on main diagonal or anti-diagonal of the 7x7 board
- **Intersection rule**: Must place at the UNIQUE intersection cell determined by the outer ring position of the current number. The placement cell maps directly to the inner grid position corresponding to the outer ring location.
- **Win condition**: All 25 numbers successfully placed in inner 5x5 grid

## Architecture Changes

### Backend Structure

The game will support three levels (1, 2, and 3) with the same 7x7 board structure:

- **Level 1**: 5x5 grid, numbers 1-25
- **Level 2**: 7x7 grid, outer ring placement (numbers 2-25)
- **Level 3**: 7x7 grid, inner 5x5 placement (numbers 2-25) with outer ring visible as reference

## Detailed Level 3 Rule Logic

### Intersection Rule - Detailed Explanation

The intersection rule works by mapping outer ring positions to specific inner grid cells:

**Mapping Logic:**

```
Outer Ring Position → Inner Grid Cell (in 7x7 coordinates)

Top edge [0, c] where c ∈ [1,5]    → [1, c]   (first inner row)
Bottom edge [6, c] where c ∈ [1,5] → [5, c]   (last inner row)
Left edge [r, 0] where r ∈ [1,5]   → [r, 1]   (first inner col)
Right edge [r, 6] where r ∈ [1,5]  → [r, 5]   (last inner col)

Yellow Corners (special cases):
[0,0] → [1,1]   top-left corner → top-left inner cell
[0,6] → [1,5]   top-right corner → top-right inner cell
[6,0] → [5,1]   bottom-left corner → bottom-left inner cell
[6,6] → [5,5]   bottom-right corner → bottom-right inner cell
```

**Example Scenarios:**

1. **Number 10 is at position [0,3]** (top edge, column 3):
  - Maps to inner cell `[1,3]`
  - Player must place 10 at `[1,3]` ONLY
2. **Number 15 is at position [3,6]** (right edge, row 3):
  - Maps to inner cell `[3,5]`
  - Player must place 15 at `[3,5]` ONLY
3. **Number 8 is at position [0,0]** (yellow corner - top-left):
  - Maps to inner cell `[1,1]`
  - Additional diagonal constraint: `[1,1]` must be on a diagonal
  - `[1,1]` IS on main diagonal (row==col), so it's valid
4. **Number 12 appears at BOTH [0,4] and [2,6]** (multiple positions):
  - `[0,4]` maps to `[1,4]`
  - `[2,6]` maps to `[2,5]`
  - Player can place 12 at EITHER `[1,4]` OR `[2,5]`

### Diagonal Rule for Yellow Corners

When a number appears in ANY yellow corner position `[0,0]`, `[0,6]`, `[6,0]`, or `[6,6]`:

- The mapped inner cell(s) must ALSO satisfy the diagonal constraint
- The cell must be one of the 13 diagonal cells in the 7x7 board

**13 Diagonal Cells of 7x7 board:**

- Main diagonal: `[0,0]`, `[1,1]`, `[2,2]`, `[3,3]`, `[4,4]`, `[5,5]`, `[6,6]`
- Anti-diagonal: `[0,6]`, `[1,5]`, `[2,4]`, `[3,3]`, `[4,2]`, `[5,1]`, `[6,0]`
- Note: `[3,3]` appears in both

**Inner 5x5 cells that are also on diagonals (coordinates in 7x7 grid):**

- `[1,1]`, `[2,2]`, `[3,3]`, `[4,4]`, `[5,5]` (main diagonal)
- `[1,5]`, `[2,4]`, `[3,3]`, `[4,2]`, `[5,1]` (anti-diagonal)

**Example with diagonal constraint:**

- Number 5 is at `[6,6]` (yellow corner - bottom-right)
- Maps to `[5,5]`
- Check if `[5,5]` is on a diagonal: YES (main diagonal)
- Placement is valid

## Implementation Steps

### 1. Backend Core Logic ([backend/GameLogic.cs](backend/GameLogic.cs))

**Add Level 3 validation method** `PlaceNumberLevel3(int row, int col)`:

Key validation rules:

1. Must be in inner 5x5 grid (`IsInnerCell(row, col)`)
2. Cell must be empty (`_board7[row, col] == 0`)
3. **Intersection Rule**: The placement cell must be the UNIQUE intersection determined by where the current number appears on the outer ring:
  - Find all positions of `currentNumber` on the outer ring
  - For each outer ring position, map it to the corresponding inner cell:
    - If at `[0, col]` (top edge) → maps to inner cell `[1, col]`
    - If at `[6, col]` (bottom edge) → maps to inner cell `[5, col]`
    - If at `[row, 0]` (left edge) → maps to inner cell `[row, 1]`
    - If at `[row, 6]` (right edge) → maps to inner cell `[row, 5]`
  - The placement `(row, col)` must match one of these valid mapped positions
4. **Diagonal Rule**: If the number appears in a yellow corner `[0,0]`, `[0,6]`, `[6,0]`, or `[6,6]`, the placement must ALSO be on one of the 13 cells that lie on either main diagonal or anti-diagonal of the 7x7 board (this is an ADDITIONAL constraint on top of rule 3)

**Helper methods to add**:

- `IsOnDiagonal7x7(int row, int col)`: Check if cell is on main diagonal (row == col) OR anti-diagonal (row + col == 6) of the 7x7 board
- `IsYellowCorner(int row, int col)`: Check if position is `[0,0]`, `[0,6]`, `[6,0]`, or `[6,6]`
- `GetValidInnerPositionsLevel3(int number)`: Returns list of valid inner 5x5 positions where the number can be placed, based on where it appears on outer ring and applying the diagonal rule for yellow corners

**Update existing methods**:

- `PlaceNumber()`: Add case for `_level == 3` calling `PlaceNumberLevel3()`
- `HasWon()`: For Level 3, check if `currentNumber > 25` (all numbers placed)
- `GetLevel()`: Already returns `_level`, no changes needed

**Add expansion method** `ExpandToLevel3()`:

```csharp
public bool ExpandToLevel3()
{
    if (_level != 2 || currentNumber != 26)
        return false;
    
    // Keep outer ring, erase inner 5x5 except the cell with number 1
    int oneRow = -1, oneCol = -1;
    for (int r = 1; r <= 5; r++)
    {
        for (int c = 1; c <= 5; c++)
        {
            if (_board7[r, c] == 1)
            {
                oneRow = r;
                oneCol = c;
            }
            else
            {
                _board7[r, c] = 0; // Erase all except 1
            }
        }
    }
    
    _level = 3;
    currentNumber = 2;
    lastRow = oneRow;
    lastCol = oneCol;
    return true;
}
```

**Add direct Level 3 initialization** `InitializeLevel3Game(Random rng)`:

- Create 7x7 board
- Fill outer ring with numbers 2-25 in a valid pattern
- Place number 1 randomly in inner 5x5
- Clear rest of inner 5x5

**Update `ClearBoard()`**: For Level 3, clear inner 5x5 except number 1, reset to currentNumber = 2

### 2. Backend Service Layer ([backend/Services/GameStateService.cs](backend/Services/GameStateService.cs))

**Update `CreateNewGame()` method**:

- Add support for `level == 3` parameter
- Call `InitializeLevel3Game()` when level 3 is selected
- Find and store position of number 1

**Add expansion method** `ExpandToLevel3(string gameId)`:

```csharp
public bool ExpandToLevel3(string gameId)
{
    var session = GetSession(gameId);
    if (session == null || session.Level != 2 || !session.GameLogic.HasWon())
        return false;
    if (!session.GameLogic.ExpandToLevel3())
        return false;
    session.Level = 3;
    session.MoveHistory.Clear(); // Clear history for fresh start
    return true;
}
```

**No changes needed** to:

- `PlaceNumber()`: Already delegates to `GameLogic.PlaceNumber()`
- `Undo()`: Already works with any level
- `GetGameState()`: Already converts board correctly

### 3. Backend API Controller ([backend/Controllers/GameController.cs](backend/Controllers/GameController.cs))

**Add new endpoint** `POST /api/Game/expand-level3`:

```csharp
[HttpPost("expand-level3")]
public IActionResult ExpandToLevel3([FromBody] ExpandLevel3Request request)
{
    var success = _gameStateService.ExpandToLevel3(request.GameId);
    if (!success)
    {
        return BadRequest(new { Error = "Cannot expand to Level 3 (Level 2 must be completed first)" });
    }
    
    var gameState = _gameStateService.GetGameState(request.GameId);
    return Ok(new { GameState = gameState });
}
```

**Add DTO** in [backend/Models/GameStateDto.cs](backend/Models/GameStateDto.cs):

```csharp
public class ExpandLevel3Request
{
    public string GameId { get; set; } = string.Empty;
}
```

### 4. Frontend API Service ([frontend/services/gameApi.ts](frontend/services/gameApi.ts))

**Add function**:

```typescript
export async function expandToLevel3(gameId: string): Promise<{ gameState: GameState }> {
  const res = await fetch(`${API_BASE}/expand-level3`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ gameId })
  });
  if (!res.ok) throw new Error('Failed to expand to Level 3');
  return res.json();
}
```

### 5. Frontend Level Selection ([frontend/pages/SelectLevelPage.tsx](frontend/pages/SelectLevelPage.tsx))

**Add Level 3 card**:

- Check `localStorage` for Level 2 completion status
- Show "Level 3" card with lock icon if Level 2 not completed
- Add 7x7 badge and "Advanced" difficulty indicator
- Handle level 3 selection by calling `api.createNewGame(username, 3)`

### 6. Frontend Game Page ([frontend/pages/GamePage.tsx](frontend/pages/GamePage.tsx))

**Add expansion handler**:

```typescript
const handleExpandLevel3 = useCallback(async () => {
  if (!gameId || busy || !gameState || gameState.level !== 2 || !gameState.hasWon) return;
  setBusy(true);
  try {
    const res = await api.expandToLevel3(gameId);
    if (res.gameState) { 
      setGameState(res.gameState); 
      msg("Welcome to Level 3! Solve the puzzle using the outer ring numbers.");
    }
  } catch { msg("Expand failed", "err"); }
  finally { setBusy(false); }
}, [gameId, gameState, busy]);
```

**Update UI elements**:

- Add "Expand to Level 3" button that appears when Level 2 is won
- Update win message for Level 2 to mention Level 3 expansion
- Add visual distinction for Level 3 (could use different color scheme)
- Update progress calculation for Level 3 (2-25 = 24 numbers)
- Save Level 2 completion to localStorage when won
- Show "Level 3 - 7x7" in header when level is 3

**Cell rendering logic**:

- For Level 3, highlight outer ring cells differently (read-only, reference numbers)
- Inner cells should be interactive (except cell with 1)
- Consider adding visual hints for valid placements based on intersection rule

### 7. Frontend TypeScript Types ([frontend/types/index.ts](frontend/types/index.ts))

**Verify `GameState` interface** supports level 3:

```typescript
export interface GameState {
  level: number; // Should support 1, 2, and 3
  board: number[][];
  currentNumber: number;
  nextNumberToPlace: number;
  score: number;
  hasWon: boolean;
  lastRow: number | null;
  lastCol: number | null;
  playerUsername: string;
  startTime: string;
}
```

No changes needed if interface already uses `number` for level.

## Visual Design Considerations

### Level 3 Board Appearance

- **Outer ring cells**: Display with accent color, slightly muted (read-only reference)
- **Inner empty cells**: Standard interactive appearance
- **Cell with 1**: Highlighted with primary color (cannot be moved)
- **Last placed cell**: Border highlight as in other levels

### User Experience Enhancements

- **Visual feedback**: When hovering over an empty inner cell, could show which outer ring numbers would make it valid
- **Error messages**: Clear explanations when placement violates intersection or diagonal rules
- **Tutorial/Help**: Brief explanation of Level 3 rules accessible from game page

## Testing Considerations

### Backend Logic Tests

1. Verify `PlaceNumberLevel3()` correctly validates intersection rule
2. Verify diagonal rule for yellow corner numbers
3. Test `ExpandToLevel3()` preserves outer ring and number 1 only
4. Test undo functionality works for Level 3
5. Test clear board maintains number 1 and outer ring

### Frontend Tests

1. Level 3 unlocks only after Level 2 completion
2. Expansion from Level 2 to Level 3 works correctly
3. UI correctly displays read-only outer ring vs interactive inner grid
4. Win condition triggers when all 25 numbers placed

## Implementation Sequence

To implement efficiently, follow this order:

1. Backend `GameLogic.cs` - core validation and expansion methods
2. Backend `GameStateService.cs` - service layer support
3. Backend `GameController.cs` - API endpoint
4. Frontend `gameApi.ts` - API client function
5. Frontend `SelectLevelPage.tsx` - add Level 3 option
6. Frontend `GamePage.tsx` - expansion handler and UI updates

## Edge Cases to Handle

1. **Dead ends**: Player may need to undo multiple moves when no valid placement exists
2. **Number 1 position**: Ensure it doesn't get erased during expansion or clear
3. **Outer ring integrity**: Never allow modification of outer ring in Level 3
4. **State persistence**: Undo stack should work correctly across all moves
5. **Multiple numbers in corners**: Some numbers may appear in multiple positions on outer ring

## Migration Notes

- No database migrations needed (using in-memory state)
- No breaking changes to existing API contracts
- Level 1 and Level 2 functionality remains unchanged
- Backward compatible with existing game sessions

