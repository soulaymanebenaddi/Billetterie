import { useEffect, useState } from 'react'
import { getEvents } from './api/events'
import { EventGallery } from './components/events/EventGallery'
import { Hero } from './components/home/Hero'
import { SiteHeader } from './components/layout/SiteHeader'
import { Icon } from './components/ui/Icon'
import type { EventListItemDto } from './types/events'
import './App.css'

function App() {
  const [events, setEvents] = useState<EventListItemDto[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [retryCount, setRetryCount] = useState(0)

  function reloadEvents() {
    if (isLoading) return

    setError(null)
    setIsLoading(true)
    setRetryCount((count) => count + 1)
  }

  useEffect(() => {
    const controller = new AbortController()

    async function loadEvents() {
      try {
        const data = await getEvents(controller.signal)

        if (!controller.signal.aborted) {
          setEvents(data)
        }
      } catch {
        // An intentional cancellation should not display an error.
        if (!controller.signal.aborted) {
          setError('Les événements n’ont pas pu être récupérés. Réessayez dans un instant.')
        }
      } finally {
        if (!controller.signal.aborted) {
          setIsLoading(false)
        }
      }
    }

    loadEvents()

    // Cleanup also runs during StrictMode's development checks.
    return () => controller.abort()
  }, [retryCount])

  return (
    <div id="accueil" className="app">
      <SiteHeader />
      <main>
        <Hero />
        <section className="event-catalog" aria-labelledby="events-title" aria-busy={isLoading}>
          <div className="container">

            {isLoading && (
              <div className="event-catalog__status">
                <div className="event-catalog__status-header" role="status">
                  <div className="event-catalog__status-icon"><Icon name="refresh" /></div>
                  <h3>Chargement des événements…</h3>
                  <p className="event-catalog__status-description">
                    Nous préparons les informations pour vous.
                  </p>
                </div>
              </div>
            )}

            {!isLoading && error !== null && (
              <div className="event-catalog__status">
                <div className="event-catalog__status-header" role="alert">
                  <div className="event-catalog__status-icon"><Icon name="alert-circle" /></div>
                  <h3>Le catalogue est indisponible</h3>
                  <p className="event-catalog__status-description">{error}</p>
                </div>
                <button
                  className="button button--light event-catalog__status-action"
                  type="button"
                  onClick={reloadEvents}
                >
                  <Icon name="refresh" />
                  Réessayer
                </button>
              </div>
            )}

            {!isLoading && error === null && events.length === 0 && (
              <div className="event-catalog__status">
                <div className="event-catalog__status-header" role="status">
                  <div className="event-catalog__status-icon"><Icon name="calendar" /></div>
                  <h3>Aucun événement à venir</h3>
                  <p className="event-catalog__status-description">
                    De nouveaux événements seront affichés ici dès leur publication.
                  </p>
                </div>
                <button
                  className="button button--light event-catalog__status-action"
                  type="button"
                  onClick={reloadEvents}
                >
                  <Icon name="refresh" />
                  Actualiser
                </button>
              </div>
            )}
          </div>

          {!isLoading && error === null && events.length > 0 && (
            <EventGallery events={events} />
          )}
        </section>
      </main>
    </div>
  )
}

export default App
