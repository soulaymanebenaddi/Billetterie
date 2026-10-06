import { Icon } from '../ui/Icon'
import type { IconName } from '../ui/Icon'
import './PurchaseSteps.css'

type PurchaseStep = {
  icon: IconName
  title: string
  description: string
}

const steps: PurchaseStep[] = [
  {
    icon: 'search',
    title: 'Trouvez un événement',
    description: 'Explorez le catalogue et choisissez votre prochaine sortie.',
  },
  {
    icon: 'seat',
    title: 'Choisissez votre place',
    description: 'Consultez les sièges disponibles et sélectionnez celui qui vous convient.',
  },
  {
    icon: 'credit-card',
    title: 'Confirmez votre achat',
    description: 'Connectez-vous ou créez un compte, puis effectuez votre paiement.',
  },
  {
    icon: 'download',
    title: 'Retrouvez votre billet',
    description: 'Après confirmation du paiement, votre billet avec code QR est dans votre compte.',
  },
]

export function PurchaseSteps() {
  return (
    <section
      id="comment-ca-marche"
      className="purchase-steps section section--muted"
      aria-labelledby="purchase-steps-title"
      tabIndex={-1}
    >
      <div className="container purchase-steps__layout">
        <div className="purchase-steps__intro">
          <h2 id="purchase-steps-title">Votre billet en 4 étapes</h2>
          <p className="text-muted">De l’envie de sortir au billet en main, suivez le guide.</p>
          <a className="button button--primary" href="#evenements">
            Explorer les événements
            <Icon name="arrow-right" />
          </a>
        </div>
        <div className="purchase-steps__journey">
          <svg className="purchase-steps__path" viewBox="0 0 800 240" preserveAspectRatio="none" aria-hidden="true" focusable="false">
            <path d="M0 120C0 265 200 265 200 120S400 -25 400 120S600 265 600 120S800 -25 800 120" />
            <circle cx="0" cy="120" r="4" />
            <circle cx="800" cy="120" r="4" />
          </svg>
          <ol className="purchase-steps__list" role="list">
            {steps.map((step, index) => (
              <li className="purchase-steps__item" key={step.icon}>
                <div className="purchase-steps__visual">
                  <Icon name={step.icon} />
                  <span className="purchase-steps__number" aria-hidden="true">{index + 1}</span>
                </div>
                <h3>{step.title}</h3>
                <p className="text-muted">{step.description}</p>
              </li>
            ))}
          </ol>
        </div>
      </div>
    </section>
  )
}
