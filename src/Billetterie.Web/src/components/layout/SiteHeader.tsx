import { Link, useLocation } from 'react-router'
import { Icon } from '../ui/Icon'
import './SiteHeader.css'

export function SiteHeader() {
  const isHome = useLocation().pathname === '/'

  return (
    <header className="site-header section--dark">
      <div className="container site-header__inner">
        <nav className="site-nav" aria-label="Navigation principale">
          <Link className="site-nav__link" to="/" aria-current={isHome ? 'page' : undefined}>
            Accueil
          </Link>
          <a className="site-nav__link" href={isHome ? '#evenements' : '/#evenements'}>
            Événements
          </a>
          <a className="site-nav__link" href={isHome ? '#comment-ca-marche' : '/#comment-ca-marche'}>
            Comment ça marche
          </a>
        </nav>

        <Link className="site-brand" to="/" aria-label="Billetterie — Accueil">
          <Icon name="ticket" className="site-brand__icon" />
          <span>Billetterie</span>
        </Link>

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
