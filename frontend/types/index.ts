// Game state types aligned with backend GameStateDto
export interface GameState {
  playerUsername: string
  level: number
  board: number[][]
  currentNumber: number
  /** US3: Next number to place - displayed to player, not auto-placed */
  nextNumberToPlace: number
  startTime: string
  score: number
  lastRow: number | null
  lastCol: number | null
  isValid: boolean
  hasWon: boolean
  timeLimitSeconds: number | null
  elapsedSeconds: number
  timeRemainingSeconds: number
  isOvertime: boolean
}

export interface Move {
  row: number
  col: number
  number: number
  timestamp: string
}

export interface GameLog {
  playerUsername: string
  completedAt: string
  duration: number
  level: number
  score: number | null
  board: number[][]
}
