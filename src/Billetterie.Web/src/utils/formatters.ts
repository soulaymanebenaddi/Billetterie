const eventDateFormatter = new Intl.DateTimeFormat('fr-CA', {
  day: 'numeric',
  month: 'short',
  year: 'numeric',
})

// The API does not provide the venue's time zone; use the browser's local date.
export function formatEventDate(startsAt: string): string {
  const date = new Date(startsAt)

  return Number.isNaN(date.getTime()) ? 'Date à confirmer' : eventDateFormatter.format(date)
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
