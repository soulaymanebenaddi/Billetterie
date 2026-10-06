import { useEffect, useState } from 'react'
import { getEvents } from './api/events'
import { EventGallery } from './components/events/EventGallery'
import { Hero } from './components/home/Hero'
import { Benefits } from './components/home/Benefits'
import { PurchaseSteps } from './components/home/PurchaseSteps'
import { SiteHeader } from './components/layout/SiteHeader'
import { Icon } from './components/ui/Icon'
import type { EventListItemDto, EventSearchFilters } from './types/events'
import { filterEvents } from './utils/eventFilters'
import './App.css'

const emptyFilters: EventSearchFilters = { name: '', date: '', city: '' }

function App() {
  const [events, setEvents] = useState<EventListItemDto[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [retryCount, setRetryCount] = useState(0)
  const [draftFilters, setDraftFilters] = useState<EventSearchFilters>(emptyFilters)
  const [appliedFilters, setAppliedFilters] = useState<EventSearchFilters>(emptyFilters)

  const filteredEvents = filterEvents(events, appliedFilters)
  const cities = [...new Set(events.map((event) => event.city))]
    .sort((first, second) => first.localeCompare(second, 'fr-CA'))
  const canSearch = !isLoading && error === null && events.length > 0
  const canReset = Object.values(draftFilters).some((value) => value !== '')
    || Object.values(appliedFilters).some((value) => value !== '')
  const showStatus = isLoading || error !== null || filteredEvents.length === 0

  function resetFilters() {
    setDraftFilters(emptyFilters)
    setAppliedFilters(emptyFilters)
  }

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
        <Hero
          filters={draftFilters}
          cities={cities}
          canSearch={canSearch}
          canReset={canReset}
          onFiltersChange={setDraftFilters}
          onSearch={() => setAppliedFilters({ ...draftFilters })}
          onReset={resetFilters}
        />
        <section
          id="evenements"
          className={`event-catalog${showStatus ? ' event-catalog--status' : ''}`}
          aria-labelledby="events-title"
          aria-busy={isLoading}
          tabIndex={-1}
        >
          <div className="container">
            <h2 id="events-title" className="sr-only">Événements à venir</h2>

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

            {!isLoading && error === null && events.length > 0 && filteredEvents.length === 0 && (
              <div className="event-catalog__status">
                <div className="event-catalog__status-header" role="status">
                  <div className="event-catalog__status-icon"><Icon name="search" /></div>
                  <h3>Aucun événement ne correspond à votre recherche</h3>
                  <p className="event-catalog__status-description">
                    Essayez d’autres critères ou réinitialisez les filtres pour voir tous les événements.
                  </p>
                </div>
                <button
                  className="button button--light event-catalog__status-action"
                  type="button"
                  onClick={resetFilters}
                >
                  <Icon name="refresh" />
                  Réinitialiser les filtres
                </button>
              </div>
            )}
          </div>

          {!isLoading && error === null && filteredEvents.length > 0 && (
            <>
              <p className="sr-only" role="status">
                {filteredEvents.length} événement{filteredEvents.length > 1 ? 's' : ''} trouvé{filteredEvents.length > 1 ? 's' : ''}.
              </p>
              <EventGallery events={filteredEvents} />
            </>
          )}
        </section>
        <Benefits />
        <PurchaseSteps />
      </main>
    </div>
  )
}

export default App
