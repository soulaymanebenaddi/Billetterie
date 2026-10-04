import type { EventListItemDto } from '../../types/events'
import { EventCard } from './EventCard'
import './EventGallery.css'

type EventGalleryProps = {
  events: EventListItemDto[]
}

export function EventGallery({ events }: EventGalleryProps) {
  return (
    <>
      <div
        className="event-gallery"
        role="region"
        aria-label="Galerie des événements à venir, défilement horizontal"
        tabIndex={0}
      >
        <ul className="event-gallery__list">
          {events.map((event) => (
            <li key={event.id} className="event-gallery__item">
              <EventCard event={event} />
            </li>
          ))}
        </ul>
      </div>
    </>
  )
}
