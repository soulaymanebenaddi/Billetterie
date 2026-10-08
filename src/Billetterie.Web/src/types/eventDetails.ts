import type { EventCategory } from './events'

export type EventDetailsDto = {
    id: string
    name: string
    description: string | null
    imageUrl: string | null
    category: EventCategory
    startsAt: string
    endsAt: string
    venueName: string
    address: string
    city: string
    venueSpaceName: string
    startingPrice: number | null
    currency: string | null
}

