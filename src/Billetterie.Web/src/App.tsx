import { useEffect, useState } from 'react'
import { getEvents } from './api/events'
import { EventGallery } from './components/events/EventGallery'
import { Hero } from './components/home/Hero'
import { SiteHeader } from './components/layout/SiteHeader'
import type { EventListItemDto } from './types/events'
import './App.css'

function App() {
  const [events, setEvents] = useState<EventListItemDto[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

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
          setError('Impossible de charger les événements. Veuillez réessayer plus tard.')
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
  }, [])

  return (
    <div id="accueil" className="app">
      <SiteHeader />
      <main>
        <Hero />
        <section className="event-catalog" aria-labelledby="events-title" aria-busy={isLoading}>
          <div className="container">
            <h2 id="events-title" className="sr-only">Événements à venir</h2>

            {isLoading && (
              <p className="event-catalog__status text-muted" role="status">
                Chargement des événements…
              </p>
            )}

            {!isLoading && error !== null && (
              <p className="event-catalog__status" role="alert">{error}</p>
            )}

            {!isLoading && error === null && events.length === 0 && (
              <p className="event-catalog__status text-muted">
                Aucun événement à venir pour le moment.
              </p>
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
