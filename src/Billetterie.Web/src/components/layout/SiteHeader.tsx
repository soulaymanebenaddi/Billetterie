import { Icon } from '../ui/Icon'
import './SiteHeader.css'

export function SiteHeader() {
  return (
    <header className="site-header section--dark">
      <div className="container site-header__inner">
        <nav className="site-nav" aria-label="Navigation principale">
          <a className="site-nav__link" href="/" aria-current="page">
            Accueil
          </a>
          <span className="site-nav__link" aria-disabled="true" title="Bientôt disponible">
            Événements
            <span className="sr-only"> — bientôt disponible</span>
          </span>
          <span className="site-nav__link" aria-disabled="true" title="Bientôt disponible">
            Comment ça marche
            <span className="sr-only"> — bientôt disponible</span>
          </span>
        </nav>

        <a className="site-brand" href="/" aria-label="Billetterie — Accueil">
          <Icon name="ticket" className="site-brand__icon" />
          <span>Billetterie</span>
        </a>

        <div className="site-header__actions">
          <button
            className="site-header__cart"
            type="button"
            aria-label="Panier — bientôt disponible"
            title="Panier bientôt disponible"
            disabled
          >
            <Icon name="cart" />
          </button>
          <button
            className="button button--light site-header__account"
            type="button"
            aria-label="Connexion et inscription — bientôt disponible"
            title="Connexion et inscription bientôt disponibles"
            disabled
          >
            <Icon name="user" />
            <span className="site-header__account-label">Connexion / Inscription</span>
          </button>
        </div>
      </div>
    </header>
  )
}
