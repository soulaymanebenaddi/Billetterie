import type { EventListItemDto } from '../types/events'

const apiBaseUrl = (import.meta.env.VITE_API_BASE_URL?.trim() || '/api').replace(/\/+$/, '')

/** Fetch the event catalog. The optional signal allows the caller to cancel the request. */
export async function getEvents(signal?: AbortSignal): Promise<EventListItemDto[]> {
  const response = await fetch(`${apiBaseUrl}/events`, {
    headers: { Accept: 'application/json' },
    signal,
  })

  if (!response.ok) {
    throw new Error(`Impossible de charger les événements (HTTP ${response.status}).`)
  }

  return (await response.json()) as EventListItemDto[]
}
