import { useState } from 'react'
import type { EventListItemDto } from '../../types/events'
import { formatEventDate, formatStartingPrice } from '../../utils/formatters'
import { Icon } from '../ui/Icon'
import './EventCard.css'

type EventCardProps = {
  event: EventListItemDto
}

export function EventCard({ event }: EventCardProps) {
  const [failedImageUrl, setFailedImageUrl] = useState<string | null>(null)
  const imageUrl = event.imageUrl?.trim() || null
  const hasImage = imageUrl !== null && imageUrl !== failedImageUrl

  return (
    <article className="event-card">
      <div className="event-card__image-frame">
        {hasImage ? (
          <img
            className="event-card__image"
            src={imageUrl}
            alt={`Affiche de ${event.name}`}
            loading="lazy"
            decoding="async"
            onError={() => setFailedImageUrl(imageUrl)}
          />
        ) : (
          <div className="event-card__image-placeholder">
            <Icon name="ticket" />
            <span>Affiche à venir</span>
          </div>
        )}
      </div>

      <div className="event-card__content">
        <h3 className="event-card__title">{event.name}</h3>
        <p className="event-card__city">{event.city}</p>
        <p className="event-card__date">
          <time dateTime={event.startsAt}>{formatEventDate(event.startsAt)}</time>
        </p>
        <p className="event-card__price">
          {formatStartingPrice(event.startingPrice, event.currency)}
        </p>
      </div>

      <button
        className="button button--secondary event-card__button"
        type="button"
        title="La page de détails sera bientôt disponible"
        disabled
      >
        Voir l’événement
        <span className="sr-only"> — bientôt disponible</span>
      </button>
    </article>
  )
}
