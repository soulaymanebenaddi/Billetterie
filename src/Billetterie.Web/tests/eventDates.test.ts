import assert from 'node:assert/strict'
import { spawnSync } from 'node:child_process'
import test from 'node:test'
import { fileURLToPath } from 'node:url'
import { filterEvents } from '../src/utils/eventFilters.ts'
import { formatEventDate, getEventDateKey } from '../src/utils/formatters.ts'
import type { EventListItemDto } from '../src/types/events.ts'

const dateCases = [
  {
    // January 15 at 21:30 in Montréal, January 16 in UTC.
    startsAt: '2026-01-16T02:30:00Z',
    eventDay: '2026-01-15',
    otherDay: '2026-01-16',
    label: '15 janv. 2026',
  },
  {
    // July 16 at 00:30: daylight saving time must use UTC-4, not a fixed UTC-5.
    startsAt: '2026-07-16T04:30:00Z',
    eventDay: '2026-07-16',
    otherDay: '2026-07-15',
    label: '16 juill. 2026',
  },
]

// Each process creates its Intl formatters in the visitor's time zone from the start.
if (process.env.BILLETTERIE_DATE_TEST_CHILD !== '1') {
  test('Event dates stay the same in every visitor time zone', async (t) => {
    for (const visitorTimeZone of ['UTC', 'America/Toronto', 'Europe/Paris', 'Asia/Tokyo']) {
      await t.test(visitorTimeZone, () => {
        const result = spawnSync(process.execPath, [
          '--experimental-strip-types', '--test', fileURLToPath(import.meta.url),
        ], {
          env: { ...process.env, TZ: visitorTimeZone, BILLETTERIE_DATE_TEST_CHILD: '1' },
          encoding: 'utf8',
        })

        assert.equal(result.status, 0, result.error?.message ?? `${result.stdout}\n${result.stderr}`)
      })
    }
  })
}

for (const dateCase of dateCases) {
  test(`Display and search use ${dateCase.eventDay} for ${dateCase.startsAt}`, () => {
    const event: EventListItemDto = {
      id: 'event-1',
      name: 'Concert à Montréal',
      imageUrl: null,
      category: 0,
      startsAt: dateCase.startsAt,
      venueName: 'Salle de démonstration',
      city: 'Montréal',
      startingPrice: 49,
      currency: 'CAD',
    }

    assert.equal(formatEventDate(event.startsAt), dateCase.label)
    assert.equal(getEventDateKey(event.startsAt), dateCase.eventDay)
    assert.deepEqual(filterEvents([event], { name: '', city: '', date: dateCase.eventDay }), [event])
    assert.deepEqual(filterEvents([event], { name: '', city: '', date: dateCase.otherDay }), [])
  })
}

test('Invalid dates preserve the display fallback and do not produce a search date', () => {
  assert.equal(formatEventDate('invalid-date'), 'Date à confirmer')
  assert.equal(getEventDateKey('invalid-date'), '')
})
