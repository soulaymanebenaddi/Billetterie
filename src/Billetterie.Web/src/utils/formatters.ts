// The MVP's current venues in Montréal and Québec share this time zone.
const eventTimeZone = 'America/Toronto'

const eventDateFormatter = new Intl.DateTimeFormat('fr-CA', {
  timeZone: eventTimeZone,
  day: 'numeric',
  month: 'short',
  year: 'numeric',
})

const eventDateKeyFormatter = new Intl.DateTimeFormat('fr-CA', {
  timeZone: eventTimeZone,
  year: 'numeric',
  month: '2-digit',
  day: '2-digit',
})

const eventTimeFormatter = new Intl.DateTimeFormat('fr-CA', {
  timeZone: eventTimeZone,
  hour: '2-digit',
  minute: '2-digit',
  hourCycle: 'h23',
})

export function formatEventTime(value: string): string {
  const date = new Date(value)
  return Number.isNaN(date.getTime()) ? 'Heure à confirmer' : eventTimeFormatter.format(date)
}

export function formatEventDate(startsAt: string): string {
  const date = new Date(startsAt)

  return Number.isNaN(date.getTime()) ? 'Date à confirmer' : eventDateFormatter.format(date)
}

/** Return the event's day as YYYY-MM-DD to compare it with a date input's value. */
export function getEventDateKey(startsAt: string): string {
  const date = new Date(startsAt)
  if (Number.isNaN(date.getTime())) return ''

  const parts = eventDateKeyFormatter.formatToParts(date)
  const year = parts.find((part) => part.type === 'year')?.value
  const month = parts.find((part) => part.type === 'month')?.value
  const day = parts.find((part) => part.type === 'day')?.value

  return year && month && day ? `${year}-${month}-${day}` : ''
}

export function formatStartingPrice(amount: number | null, currency: string | null): string {
  if (amount === null || currency === null) {
    return 'Tarif à venir'
  }

  const price = new Intl.NumberFormat('fr-CA', {
    style: 'currency',
    currency,
    currencyDisplay: 'code',
  }).format(amount)

  return `À partir de ${price}`
}
