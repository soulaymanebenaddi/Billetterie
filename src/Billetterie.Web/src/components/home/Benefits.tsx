import { Icon } from '../ui/Icon'
import type { IconName } from '../ui/Icon'
import './Benefits.css'

type Benefit = {
  icon: IconName
  title: string
  description: string
}

const benefits: Benefit[] = [
  {
    icon: 'calendar',
    title: 'Des sorties pour tous',
    description: 'Concerts, théâtre, sport ou humour : trouvez l’événement qui vous ressemble.',
  },
  {
    icon: 'seat',
    title: 'Votre place, votre choix',
    description: 'Découvrez les places disponibles et choisissez où vivre votre événement.',
  },
  {
    icon: 'credit-card',
    title: 'Un parcours en ligne',
    description: 'De la découverte de votre événement au paiement, tout se passe au même endroit.',
  },
  {
    icon: 'ticket',
    title: 'Vos billets numériques',
    description: 'Retrouvez vos billets dans votre compte et présentez leur code QR à l’entrée.',
  },
]

export function Benefits() {
  return (
    <section className="benefits section" aria-labelledby="benefits-title">
      <div className="container">
        <div className="benefits__intro">
          <h2 id="benefits-title">Nos avantages</h2>
          <p className="text-muted">Tout pour préparer votre prochaine sortie.</p>
        </div>
        <ul className="benefits__list" role="list">
          {benefits.map((benefit) => (
            <li className="benefits__item" key={benefit.icon}>
              <div className={`benefits__visual benefits__visual--${benefit.icon}`}>
                <Icon name={benefit.icon} />
              </div>
              <h3>{benefit.title}</h3>
              <p className="text-muted">{benefit.description}</p>
            </li>
          ))}
        </ul>
      </div>
    </section>
  )
}
