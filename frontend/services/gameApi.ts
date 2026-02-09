import type { GameState } from '../types'

const API_BASE = '/api/Game'

async function request<T>(path: string, options?: RequestInit): Promise<T> {
  const res = await fetch(`${API_BASE}${path}`, {
    headers: { 'Content-Type': 'application/json', ...options?.headers },
    ...options,
  })
  if (!res.ok) {
    const err = await res.json().catch(() => ({}))
    throw new Error(
      (err as Record<string, string>).error ||
      (err as Record<string, string>).errorMessage ||
      res.statusText
    )
  }
  return res.json() as Promise<T>
}

export interface NewGameResponse {
  gameId: string
  gameState: GameState
}

/** Create a new game with number 1 randomly placed */
export function createNewGame(playerUsername: string, level: number = 1): Promise<NewGameResponse> {
  return request<NewGameResponse>('/new', {
    method: 'POST',
    body: JSON.stringify({ playerUsername, level }),
  })
}

export function getGameState(gameId: string): Promise<GameState> {
  return request<GameState>(`/${gameId}`)
}

export interface PlaceNumberResponse {
  success: boolean
  isValid: boolean
  gameState?: GameState
  errorMessage?: string
}

/** Place the next number at (row, col).
 *  Returns the response body even on 400 (invalid placement) so the UI can
 *  show the updated board state and a meaningful error. */
export async function placeNumber(gameId: string, row: number, col: number): Promise<PlaceNumberResponse> {
  const res = await fetch(`${API_BASE}/place`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ gameId, row, col }),
  })
  // Both 200 and 400 return a PlaceNumberResponse body
  const body = await res.json() as PlaceNumberResponse
  return body
}

/** US5: Undo last `count` moves (default 1, can undo as many as wanted) */
export function undo(gameId: string, count: number = 1): Promise<{ gameState: GameState }> {
  return request<{ gameState: GameState }>('/undo', {
    method: 'POST',
    body: JSON.stringify({ gameId, count }),
  })
}

/** US4: Clear board. keepOneInPlace=true keeps 1 in same cell; false = random re-allocate. Level 2: outer ring only. */
export function clearBoard(gameId: string, keepOneInPlace: boolean = true): Promise<{ gameState: GameState }> {
  return request<{ gameState: GameState }>('/clear', {
    method: 'POST',
    body: JSON.stringify({ gameId, keepOneInPlace }),
  })
}

/** US8: Expand to Level 2 after Level 1 is won */
export function expandToLevel2(gameId: string): Promise<{ gameState: GameState }> {
  return request<{ gameState: GameState }>('/expand-level2', {
    method: 'POST',
    body: JSON.stringify({ gameId }),
  })
}
