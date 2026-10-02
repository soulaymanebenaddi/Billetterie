import type { ReactNode } from 'react'

type IconName = 'ticket' | 'cart' | 'user' | 'search' | 'calendar' | 'map-pin' | 'chevron-down'

type IconProps = {
  name: IconName
  className?: string
}

const iconPaths: Record<IconName, ReactNode> = {
  ticket: (
    <>
      <path d="M4 4h16v5a3 3 0 0 0 0 6v5H4v-5a3 3 0 0 0 0-6V4Z" />
      <path d="M14 4v3m0 3v4m0 3v3" />
    </>
  ),
  cart: (
    <>
      <path d="M2 3h3l3 13h11l3-10H6" />
      <circle cx="9" cy="21" r="1" />
      <circle cx="19" cy="21" r="1" />
    </>
  ),
  user: (
    <>
      <circle cx="12" cy="7" r="4" />
      <path d="M4 21v-2a8 8 0 0 1 16 0v2" />
    </>
  ),
  search: (
    <>
      <circle cx="10.5" cy="10.5" r="6.5" />
      <path d="m16 16 5 5" />
    </>
  ),
  calendar: (
    <>
      <rect x="3" y="5" width="18" height="16" rx="2" />
      <path d="M7 3v4m10-4v4M3 11h18M7 15h2m3 0h2m-7 3h2" />
    </>
  ),
  'map-pin': (
    <>
      <path d="M20 10c0 6-8 12-8 12S4 16 4 10a8 8 0 1 1 16 0Z" />
      <circle cx="12" cy="10" r="3" />
    </>
  ),
  'chevron-down': <path d="m6 9 6 6 6-6" />,
}

export function Icon({ name, className }: IconProps) {
  return (
    <svg
      className={className}
      width="24"
      height="24"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="1.75"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
      focusable="false"
    >
      {iconPaths[name]}
    </svg>
  )
}
