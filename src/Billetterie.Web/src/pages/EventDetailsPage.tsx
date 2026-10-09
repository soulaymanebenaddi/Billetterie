import { useEffect, useRef, useState } from 'react'
import { Link, useParams } from 'react-router'
import { ApiError } from '../api/ApiError'
import { getEventDetails } from '../api/events'
import { SiteHeader } from '../components/layout/SiteHeader'
import { Icon } from '../components/ui/Icon'
import type { EventDetailsDto } from '../types/eventDetails'
import type { EventCategory } from '../types/events'
import { formatEventDate, formatEventTime, formatStartingPrice } from '../utils/formatters'
import './EventDetailsPage.css'

type DetailsState =
  | { status: 'loading' }
  | { status: 'ready'; event: EventDetailsDto }
  | { status: 'error'; kind: 'not-found' | 'invalid-id' | 'unavailable' }

const categoryLabels: Record<EventCategory, string> = {
  0: 'Concert',
  1: 'Sport',
  2: 'Théâtre',
  3: 'Humour',
  4: 'Conférence',
  5: 'Autre événement',
}

const errorContent = {
  'not-found': {
    title: 'Événement introuvable',
    description: 'Cet événement n’existe pas ou n’est plus accessible au public.',
  },
  'invalid-id': {
    title: 'Lien d’événement invalide',
    description: 'L’adresse utilisée ne permet pas d’identifier un événement. Retrouvez-le dans le catalogue.',
  },
  unavailable: {
    title: 'Les détails sont indisponibles',
    description: 'Les informations n’ont pas pu être récupérées. Réessayez dans un instant.',
  },
}

function EventDetailsContent({ eventId }: { eventId: string }) {
  const [state, setState] = useState<DetailsState>({ status: 'loading' })
  const [retryCount, setRetryCount] = useState(0)
  const [failedImageUrl, setFailedImageUrl] = useState<string | null>(null)

  useEffect(() => {
    const controller = new AbortController()

    async function loadEvent() {
      try {
        const event = await getEventDetails(eventId, controller.signal)
        if (!controller.signal.aborted) setState({ status: 'ready', event })
      } catch (error) {
        if (controller.signal.aborted) return

        const kind = error instanceof ApiError && error.status === 404
          ? 'not-found'
          : error instanceof ApiError && error.status === 400
            ? 'invalid-id'
            : 'unavailable'
        setState({ status: 'error', kind })
      }
    }

    loadEvent()
    return () => controller.abort()
  }, [eventId, retryCount])

  const pageTitle = state.status === 'ready'
    ? state.event.name
    : state.status === 'error' ? errorContent[state.kind].title : 'Détails de l’événement'

  useEffect(() => {
    document.title = `${pageTitle} - Billetterie`
    return () => { document.title = 'Billetterie' }
  }, [pageTitle])

  function retry() {
    setState({ status: 'loading' })
    setRetryCount((count) => count + 1)
  }

  if (state.status === 'loading') {
    return (
      <section className="event-details__loading" aria-busy="true" aria-labelledby="details-loading-title">
        <div role="status">
          <h1 id="details-loading-title">Chargement de l’événement…</h1>
          <p className="text-muted">Nous préparons les informations pour vous.</p>
        </div>
        <div className="event-details__skeleton" aria-hidden="true" />
      </section>
    )
  }

  if (state.status === 'error') {
    const content = errorContent[state.kind]
    return (
      <section className="event-details__status" aria-labelledby="details-error-title">
        <div className="event-details__status-icon"><Icon name="alert-circle" /></div>
        <div role="alert">
          <h1 id="details-error-title">{content.title}</h1>
          <p className="text-muted">{content.description}</p>
        </div>
        {state.kind === 'unavailable' && (
          <button className="button button--primary" type="button" onClick={retry}>
            <Icon name="refresh" /> Réessayer
          </button>
        )}
      </section>
    )
  }

  const { event } = state
  const imageUrl = event.imageUrl?.trim() || null
  const hasImage = imageUrl !== null && imageUrl !== failedImageUrl
  const description = event.description?.trim()

  return (
    <article className="event-details__article" aria-labelledby="event-details-title">
      <header className="event-details__heading">
        <h1 id="event-details-title">{event.name}</h1>
      </header>

      <div className="event-details__image-frame">
        {hasImage ? (
          <img
            className="event-details__image"
            src={imageUrl}
            alt={`Affiche de ${event.name}`}
            decoding="async"
            onError={() => setFailedImageUrl(imageUrl)}
          />
        ) : (
          <div className="event-details__image-placeholder">
            <Icon name="ticket" />
            <span>Affiche à venir</span>
          </div>
        )}
      </div>

      <div className="event-details__summary">
        <section className="event-details__info" aria-labelledby="event-schedule-title">
          <Icon name="calendar" />
          <div>
            <h2 id="event-schedule-title">Date et horaires</h2>
            <dl className="event-details__schedule">
              <div>
                <dt>Début</dt>
                <dd><time dateTime={event.startsAt}>{formatEventDate(event.startsAt)} · {formatEventTime(event.startsAt)}</time></dd>
              </div>
              <div>
                <dt>Fin</dt>
                <dd><time dateTime={event.endsAt}>{formatEventDate(event.endsAt)} · {formatEventTime(event.endsAt)}</time></dd>
              </div>
            </dl>
          </div>
        </section>

        <section className="event-details__info" aria-labelledby="event-venue-title">
          <Icon name="map-pin" />
          <div>
            <h2 id="event-venue-title">Lieu</h2>
            <p className="event-details__venue-name">{event.venueName}</p>
            <p className="text-muted">{event.venueSpaceName}</p>
            <address>{event.address}<br />{event.city}</address>
          </div>
        </section>

        <section className="event-details__price" aria-labelledby="event-price-title">
          <h2 id="event-price-title">Tarif des billets</h2>
          <p>{formatStartingPrice(event.startingPrice, event.currency)}</p>
        </section>
      </div>

        <section className="event-details__description" aria-labelledby="event-description-title">
          <h2 id="event-description-title">À propos de l’événement</h2>
          <p> <strong>Catégorie :</strong> {categoryLabels[event.category]}</p>
          {description && <p>{description}</p>}
        </section>
    </article>
  )
}

function EventDetailsPage() {
  const { id = '' } = useParams<{ id: string }>()
  const mainRef = useRef<HTMLElement>(null)

  useEffect(() => {
    window.scrollTo(0, 0)
    mainRef.current?.focus({ preventScroll: true })
  }, [id])

  return (
    <div className="app">
      <SiteHeader />
      <main className="event-details container" ref={mainRef} tabIndex={-1}>
        <Link className="event-details__back" to="/">
          <Icon name="arrow-right" /> Retour aux événements
        </Link>
        <ol className="event-details__steps" aria-label="Parcours d’achat" role="list">
          {['Choisir sa place', 'Paiement', 'Obtenir son billet'].map((label, index) => (
            <li key={label}>
              <span className="event-details__step-number" aria-hidden="true">{index + 1}</span>
              <span>{label}</span>
            </li>
          ))}
        </ol>
        {/* A new identifier resets loading and image state before displaying another event. */}
        <EventDetailsContent key={id} eventId={id} />
      </main>
    </div>
  )
}

export default EventDetailsPage
