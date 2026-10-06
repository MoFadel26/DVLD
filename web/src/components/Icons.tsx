import type { SVGProps } from 'react'

// One stroke family for the whole app: 24px grid, 1.75 stroke, round joins.
const paths = {
  route: (
    <>
      <path d="M12 21V11" />
      <path d="M12 11 6.5 5.5M12 11l5.5-5.5" />
      <path d="M6.5 9.5v-4h4M17.5 9.5v-4h-4" />
    </>
  ),
  person: (
    <>
      <circle cx="12" cy="7.5" r="3.5" />
      <path d="M4.5 20.5c.8-4 3.8-6 7.5-6s6.7 2 7.5 6" />
    </>
  ),
  form: (
    <>
      <rect x="5" y="3.5" width="14" height="17.5" rx="1.5" />
      <path d="M8.5 8.5h7M8.5 12h7M8.5 15.5h4" />
    </>
  ),
  card: (
    <>
      <rect x="2.5" y="5.5" width="19" height="13" rx="1.5" />
      <circle cx="8" cy="11" r="2" />
      <path d="M5 15.5c.6-1.4 1.6-2 3-2s2.4.6 3 2M13.5 10h5M13.5 13h3.5" />
    </>
  ),
  classes: (
    <>
      <rect x="3.5" y="3.5" width="7" height="7" rx="1" />
      <rect x="13.5" y="3.5" width="7" height="7" rx="1" />
      <rect x="3.5" y="13.5" width="7" height="7" rx="1" />
      <rect x="13.5" y="13.5" width="7" height="7" rx="1" />
    </>
  ),
  apply: (
    <>
      <rect x="5" y="3.5" width="14" height="17.5" rx="1.5" />
      <path d="m8.5 12 2.3 2.3 4.7-4.8" />
    </>
  ),
  eye: (
    <>
      <path d="M2.5 12S6 5.5 12 5.5 21.5 12 21.5 12 18 18.5 12 18.5 2.5 12 2.5 12Z" />
      <circle cx="12" cy="12" r="3" />
    </>
  ),
  book: (
    <>
      <path d="M12 6.5C10.2 5 7.6 4.5 3.5 4.5v14c4.1 0 6.7.5 8.5 2 1.8-1.5 4.4-2 8.5-2v-14c-4.1 0-6.7.5-8.5 2Z" />
      <path d="M12 6.5v14" />
    </>
  ),
  wheel: (
    <>
      <circle cx="12" cy="12" r="8.5" />
      <circle cx="12" cy="12" r="2" />
      <path d="M3.8 10.5c2.7-.7 5.4-.7 8.2-.7s5.5 0 8.2.7M10.3 13.6 7 19.5M13.7 13.6l3.3 5.9" />
    </>
  ),
  arrowUp: (
    <>
      <path d="M12 20V5" />
      <path d="m6 11 6-6 6 6" />
    </>
  ),
  arrowNext: (
    <>
      <path d="M4 12h15" />
      <path d="m13 6 6 6-6 6" />
    </>
  ),
  check: <path d="m5 12.5 4.5 4.5L19 7.5" />,
  close: <path d="M6 6l12 12M18 6 6 18" />,
  plus: <path d="M12 5v14M5 12h14" />,
  search: (
    <>
      <circle cx="10.5" cy="10.5" r="6" />
      <path d="m15 15 5.5 5.5" />
    </>
  ),
  prohibit: (
    <>
      <circle cx="12" cy="12" r="8.5" />
      <path d="M6 6l12 12" />
    </>
  ),
  warning: (
    <>
      <path d="M12 3.5 21.5 20h-19L12 3.5Z" />
      <path d="M12 10v4.5M12 17.2v.1" />
    </>
  ),
  globe: (
    <>
      <circle cx="12" cy="12" r="8.5" />
      <path d="M3.5 12h17M12 3.5c2.4 2.4 3.5 5.2 3.5 8.5s-1.1 6.1-3.5 8.5c-2.4-2.4-3.5-5.2-3.5-8.5S9.6 5.9 12 3.5Z" />
    </>
  ),
  menu: <path d="M4 7h16M4 12h16M4 17h16" />,
  home: (
    <>
      <path d="M4 10.5 12 4l8 6.5V20a1 1 0 0 1-1 1h-4.5v-6h-5v6H5a1 1 0 0 1-1-1v-9.5Z" />
    </>
  ),
  sun: (
    <>
      <circle cx="12" cy="12" r="4" />
      <path d="M12 2.5v2M12 19.5v2M4.6 4.6l1.4 1.4M18 18l1.4 1.4M2.5 12h2M19.5 12h2M4.6 19.4 6 18M18 6l1.4-1.4" />
    </>
  ),
  moon: <path d="M20 14.5A8 8 0 0 1 9.5 4a8 8 0 1 0 10.5 10.5Z" />,
  logout: (
    <>
      <path d="M14 4h4a2 2 0 0 1 2 2v12a2 2 0 0 1-2 2h-4" />
      <path d="M10 16l-4-4 4-4M6 12h10" />
    </>
  ),
  alert: (
    <>
      <circle cx="12" cy="12" r="9" />
      <path d="M12 7.5v5.5M12 16.4v.1" />
    </>
  ),
  // Vehicle pictograms for the seven license classes.
  motorcycle: (
    <>
      <circle cx="5.5" cy="16" r="3" />
      <circle cx="18.5" cy="16" r="3" />
      <path d="M5.5 16 9 11h5l2.5 5M14 11l-1.5-3H10M16.5 16l-1-5.5 2-1" />
    </>
  ),
  heavyMotorcycle: (
    <>
      <circle cx="5" cy="16" r="3.5" />
      <circle cx="19" cy="16" r="3.5" />
      <path d="M5 16 8 10.5h7.5L19 16M8 10.5 9.5 8h4M15.5 10.5l1-3h2.5M11 13.5h3" />
    </>
  ),
  car: (
    <>
      <path d="M3 16.5v-3.2l2-4.3c.3-.6.9-1 1.6-1h10.8c.7 0 1.3.4 1.6 1l2 4.3v3.2" />
      <path d="M3 13.3h18M2.5 16.5h19" />
      <circle cx="7" cy="17.5" r="1.7" />
      <circle cx="17" cy="17.5" r="1.7" />
    </>
  ),
  taxi: (
    <>
      <path d="M3 16.5v-3.2l2-4.3c.3-.6.9-1 1.6-1h10.8c.7 0 1.3.4 1.6 1l2 4.3v3.2" />
      <path d="M3 13.3h18M2.5 16.5h19M9.5 8V5.5h5V8" />
      <circle cx="7" cy="17.5" r="1.7" />
      <circle cx="17" cy="17.5" r="1.7" />
    </>
  ),
  tractor: (
    <>
      <circle cx="7" cy="15.5" r="4" />
      <circle cx="18.5" cy="17" r="2.5" />
      <path d="M7 11.5V5h5l1.5 6.5H21v5.5M11 15.5h5M17 11.5V8" />
    </>
  ),
  bus: (
    <>
      <rect x="4" y="3.5" width="16" height="14" rx="2" />
      <path d="M4 11h16M8 21v-3.5M16 21v-3.5M7.5 14.5h.1M16.5 14.5h.1" />
    </>
  ),
  truck: (
    <>
      <path d="M2.5 16.5v-10h11v10M13.5 9.5h4l3.5 3.5v3.5h-7.5" />
      <path d="M2.5 16.5h3M9.5 16.5h6" />
      <circle cx="7.5" cy="17" r="1.8" />
      <circle cx="17.5" cy="17" r="1.8" />
    </>
  ),
} as const

export type IconName = keyof typeof paths


type Props = SVGProps<SVGSVGElement> & { name: IconName; size?: number; label?: string }

export function Icon({ name, size = 20, label, className, ...rest }: Props) {
  return (
    <svg
      viewBox="0 0 24 24"
      width={size}
      height={size}
      fill="none"
      stroke="currentColor"
      strokeWidth={1.75}
      strokeLinecap="round"
      strokeLinejoin="round"
      className={['icon', className].filter(Boolean).join(' ')}
      role={label ? 'img' : undefined}
      aria-label={label}
      aria-hidden={label ? undefined : true}
      focusable="false"
      {...rest}
    >
      {paths[name]}
    </svg>
  )
}
