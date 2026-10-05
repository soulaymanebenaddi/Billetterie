/** Numeric values sent by the API's EventCategory enum. Keep both definitions aligned. */
export type EventCategory =
  | 0 // Concert
  | 1 // Sports
  | 2 // Theatre
  | 3 // Comedy
  | 4 // Conference
  | 5 // Other

export type EventListItemDto = {
  id: string
  name: string
  imageUrl: string | null
  category: EventCategory
  startsAt: string
  venueName: string
  city: string
  startingPrice: number | null
  currency: string | null
}

export type EventSearchFilters = {
  name: string
  date: string
  city: string
}
