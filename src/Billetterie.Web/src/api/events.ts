import type { EventDetailsDto } from '../types/eventDetails'
import type { EventListItemDto } from '../types/events'
import { ApiError } from './ApiError'

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

export async function getEventDetails(eventId: string, signal?: AbortSignal): Promise<EventDetailsDto> {
  const response = await fetch(`${apiBaseUrl}/events/${encodeURIComponent(eventId)}`, {
    headers: { Accept: 'application/json' },
    signal,
  })

  if (!response.ok) {
    let message = 'Impossible de charger les détails de l’événement.'

    if (response.status === 404) {
      message = 'Événement introuvable.'
    } else if (response.status === 400) {
      message = 'Requête invalide.'
    } else if (response.status >= 500) {
      message = 'Erreur serveur.'
    }

    throw new ApiError(`${message} (HTTP ${response.status}).`, response.status)
  }

  return (await response.json()) as EventDetailsDto
}
