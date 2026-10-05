import type { EventListItemDto, EventSearchFilters } from '../types/events'

function normalizeSearch(value: string): string {
  return value.trim().normalize('NFD').replace(/\p{M}/gu, '').toLocaleLowerCase('fr-CA')
}

function getLocalDate(startsAt: string): string {
  const date = new Date(startsAt)
  if (Number.isNaN(date.getTime())) return ''

  // Match the local day displayed on the card, rather than the UTC day.
  const year = date.getFullYear()
  const month = String(date.getMonth() + 1).padStart(2, '0')
  const day = String(date.getDate()).padStart(2, '0')
  return `${year}-${month}-${day}`
}

export function filterEvents(
  events: EventListItemDto[],
  filters: EventSearchFilters,
): EventListItemDto[] {
  const name = normalizeSearch(filters.name)

  return events.filter((event) => {
    const matchesName = normalizeSearch(event.name).includes(name)
    const matchesDate = filters.date === '' || getLocalDate(event.startsAt) === filters.date
    const matchesCity = filters.city === '' || event.city === filters.city

    return matchesName && matchesDate && matchesCity
  })
}
