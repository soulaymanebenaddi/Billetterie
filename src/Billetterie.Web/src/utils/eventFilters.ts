import type { EventListItemDto, EventSearchFilters } from '../types/events'
import { getEventDateKey } from './formatters.ts'

function normalizeSearch(value: string): string {
  return value.trim().normalize('NFD').replace(/\p{M}/gu, '').toLocaleLowerCase('fr-CA')
}

export function filterEvents(
  events: EventListItemDto[],
  filters: EventSearchFilters,
): EventListItemDto[] {
  const name = normalizeSearch(filters.name)

  return events.filter((event) => {
    const matchesName = normalizeSearch(event.name).includes(name)
    const matchesDate = filters.date === '' || getEventDateKey(event.startsAt) === filters.date
    const matchesCity = filters.city === '' || event.city === filters.city

    return matchesName && matchesDate && matchesCity
  })
}
