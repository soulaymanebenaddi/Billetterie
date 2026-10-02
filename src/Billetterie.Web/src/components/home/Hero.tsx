import { Icon } from '../ui/Icon'
import './Hero.css'

export function Hero() {
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
          aria-describedby="search-availability"
          onSubmit={(event) => event.preventDefault()}
        >
          <label className="hero-search__field" htmlFor="event-name">
            <Icon name="search" />
            <span className="sr-only">Nom de l’événement</span>
            <input
              id="event-name"
              name="eventName"
              type="search"
              placeholder="Nom d’un événement"
            />
          </label>

          <label className="hero-search__field" htmlFor="event-date">
            <Icon name="calendar" />
            <span className="sr-only">Date de l’événement</span>
            <input id="event-date" name="eventDate" type="date" />
          </label>

          <label className="hero-search__field" htmlFor="event-city">
            <Icon name="map-pin" />
            <span className="sr-only">Ville — bientôt disponible</span>
            <select id="event-city" name="city" defaultValue="" disabled>
              <option value="">Toutes les villes</option>
            </select>
            <Icon name="chevron-down" className="hero-search__chevron" />
          </label>

          <button className="button button--primary hero-search__submit" type="submit" disabled>
            Trouver un événement
          </button>
        </form>

        <p id="search-availability" className="hero__search-note">
          La recherche sera bientôt disponible.
        </p>
      </div>
    </section>
  )
}
