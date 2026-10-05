import type { EventSearchFilters } from '../../types/events'
import { Icon } from '../ui/Icon'
import './Hero.css'

type HeroProps = {
  filters: EventSearchFilters
  cities: string[]
  canSearch: boolean
  canReset: boolean
  onFiltersChange: (filters: EventSearchFilters) => void
  onSearch: () => void
  onReset: () => void
}

export function Hero({
  filters, cities, canSearch, canReset, onFiltersChange, onSearch, onReset,
}: HeroProps) {
  return (
    <section className="hero section--dark" aria-labelledby="hero-title">
      <div className="container">
        <div className="hero__intro">
          <h1 id="hero-title" className="hero__title">
            Réservez les billets pour vos événements préférés
          </h1>
          <p className="hero__subtitle">
            Trouvez votre prochaine sortie en un seul clic !
          </p>
        </div>

        <form
          className="hero-search"
          aria-label="Rechercher un événement"
          onSubmit={(event) => {
            event.preventDefault()
            if (canSearch) onSearch()
          }}
        >
          <label className="hero-search__field" htmlFor="event-name">
            <Icon name="search" />
            <span className="sr-only">Nom de l’événement</span>
            <input
              id="event-name"
              name="eventName"
              type="search"
              placeholder="Nom d’un événement"
              value={filters.name}
              onChange={(event) => onFiltersChange({ ...filters, name: event.target.value })}
            />
          </label>

          <label className="hero-search__field" htmlFor="event-date">
            <Icon name="calendar" />
            <span className="sr-only">Date de l’événement</span>
            <input
              id="event-date"
              name="eventDate"
              type="date"
              value={filters.date}
              onChange={(event) => onFiltersChange({ ...filters, date: event.target.value })}
            />
          </label>

          <label className="hero-search__field hero-search__field--city" htmlFor="event-city">
            <Icon name="map-pin" />
            <span className="sr-only">Ville</span>
            <select
              id="event-city"
              name="city"
              value={filters.city}
              onChange={(event) => onFiltersChange({ ...filters, city: event.target.value })}
              disabled={cities.length === 0}
            >
              <option value="">Toutes les villes</option>
              {cities.map((city) => <option key={city} value={city}>{city}</option>)}
            </select>
            <Icon name="chevron-down" className="hero-search__chevron" />
          </label>

          <button
            className="button button--primary hero-search__submit"
            type="submit"
            disabled={!canSearch}
          >
            Trouver un événement
          </button>
          <button
            className="button button--light hero-search__reset"
            type="button"
            onClick={onReset}
            disabled={!canReset}
          >
            <Icon name="refresh" />
            Réinitialiser
          </button>
        </form>
      </div>
    </section>
  )
}
