import { useCallback, useEffect, useState } from 'react'

// There is no authentication in the API; every write is attributed to this user.
export const CURRENT_USER_ID = 1

export type Person = {
  personId: number
  nationalNo: string
  firstName: string
  secondName: string
  thirdName: string | null
  lastName: string
  fullName: string
  dateOfBirth: string
  age: number
  gender: 'Male' | 'Female'
  address: string
  phone: string
  email: string
  nationalityCountryId: number
  countryName: string | null
  imagePath: string | null
}

export type Country = {
  countryId: number
  countryName: string
}

export type PersonInput = {
  firstName: string
  secondName: string
  thirdName: string | null
  lastName: string
  dateOfBirth: string
  gender: 0 | 1
  address: string
  phone: string
  email: string
  nationalityCountryId: number
  imagePath: string | null
}

export type ApplicationStatus = 'New' | 'Cancelled' | 'Completed'

export type LocalApplication = {
  localDrivingLicenseApplicationId: number
  applicationId: number
  applicantPersonId: number
  applicantFullName: string
  nationalNo: string
  applicationDate: string
  licenseClassId: number
  className: string
  passedTestCount: number
  applicationStatus: ApplicationStatus
  paidFees: number
}

export type LicenseClass = {
  licenseClassId: number
  className: string
  classDescription: string
  minimumAllowedAge: number
  validityLength: number
  classFees: number
}

export type TestType = 1 | 2 | 3

export type TestAppointment = {
  testAppointmentId: number
  localDrivingLicenseApplicationId: number
  testTypeTitle: string
  appointmentDate: string
  paidFees: number
  isLocked: boolean
  createdByUserId: number
  retakeTestApplicationId: number | null
}

export type TestResult = {
  testId: number
  testAppointmentId: number
  testResult: 'Pass' | 'Fail'
  notes: string | null
  createdDate: string
}

export type License = {
  licenseId: number
  applicationId: number
  driverId: number
  personId: number
  driverFullName: string
  nationalNo: string
  licenseClassId: number
  className: string
  issueDate: string
  expirationDate: string
  notes: string | null
  paidFees: number
  isActive: boolean
  issueReason: 'FirstTime' | 'Renew' | 'ReplacementForDamaged' | 'ReplacementForLost'
  isDetained: boolean
  createdByUserId: number
}

export type DetainedLicense = {
  detainId: number
  licenseId: number
  detainDate: string
  fineFees: number
  isReleased: boolean
  releaseDate: string | null
}

export type InternationalLicense = {
  internationalLicenseId: number
  applicationId: number
  driverId: number
  issuedUsingLocalLicenseId: number
  issueDate: string
  expirationDate: string
  isActive: boolean
}

/** An RFC 7807 problem returned by the API's exception middleware. */
export class ApiError extends Error {
  status: number
  title: string
  detail: string

  constructor(status: number, title: string, detail: string) {
    super(detail || title)
    this.status = status
    this.title = title
    this.detail = detail
  }
}

async function request<T>(method: string, path: string, body?: unknown): Promise<T> {
  let res: Response
  try {
    res = await fetch(`/api${path}`, {
      method,
      headers: body === undefined ? undefined : { 'Content-Type': 'application/json' },
      body: body === undefined ? undefined : JSON.stringify(body),
    })
  } catch {
    throw new ApiError(0, 'Network', '')
  }

  if (!res.ok) {
    let title = res.statusText
    let detail = ''
    try {
      const problem = await res.json()
      title = problem.title ?? title
      detail = problem.detail ?? ''
    } catch {
      // Not a problem-details body (for example the dev proxy failing to reach the API).
    }
    throw new ApiError(res.status, res.status >= 500 && !detail ? 'Network' : title, detail)
  }

  if (res.status === 204) return undefined as T
  return res.json() as Promise<T>
}

export const api = {
  countries: () => request<Country[]>('GET', '/countries'),
  people: () => request<Person[]>('GET', '/people'),
  person: (id: number) => request<Person>('GET', `/people/${id}`),
  personByNationalNo: (no: string) =>
    request<Person>('GET', `/people/by-national-no/${encodeURIComponent(no)}`),
  createPerson: (input: PersonInput & { nationalNo: string }) =>
    request<Person>('POST', '/people', input),
  updatePerson: (id: number, input: PersonInput) => request<Person>('PUT', `/people/${id}`, input),
  deletePerson: (id: number) => request<void>('DELETE', `/people/${id}`),

  applications: () => request<LocalApplication[]>('GET', '/applications/local-license'),
  application: (id: number) => request<LocalApplication>('GET', `/applications/local-license/${id}`),
  createApplication: (personId: number, licenseClassId: number) =>
    request<LocalApplication>('POST', '/applications/local-license', {
      applicantPersonId: personId,
      licenseClassId,
      createdByUserId: CURRENT_USER_ID,
    }),
  cancelApplication: (applicationId: number) =>
    request<void>('PUT', `/applications/${applicationId}/cancel`),

  appointments: (localAppId: number, testType: TestType) =>
    request<TestAppointment[]>('GET', `/tests/appointments/${localAppId}/${testType}`),
  scheduleTest: (localAppId: number, testType: TestType, appointmentDate: string) =>
    request<TestAppointment>('POST', '/tests/appointments', {
      localDrivingLicenseApplicationId: localAppId,
      testType,
      appointmentDate,
      createdByUserId: CURRENT_USER_ID,
    }),
  takeTest: (testType: TestType, appointmentId: number, pass: boolean, notes: string) =>
    request<TestResult>('POST', `/tests/${testType}/take`, {
      testAppointmentId: appointmentId,
      testResult: pass ? 1 : 0,
      notes: notes || null,
      createdByUserId: CURRENT_USER_ID,
    }),

  licenseClasses: () => request<LicenseClass[]>('GET', '/license-classes'),

  license: (id: number) => request<License>('GET', `/licenses/${id}`),
  driverLicenses: (driverId: number) => request<License[]>('GET', `/licenses/driver/${driverId}`),
  issueFirstTime: (localAppId: number, notes: string) =>
    request<License>('POST', '/licenses/issue-first-time', {
      localDrivingLicenseApplicationId: localAppId,
      notes: notes || null,
      createdByUserId: CURRENT_USER_ID,
    }),
  renew: (licenseId: number, notes: string) =>
    request<License>('POST', '/licenses/renew', {
      licenseId,
      notes: notes || null,
      createdByUserId: CURRENT_USER_ID,
    }),
  replaceLost: (licenseId: number) =>
    request<License>('POST', '/licenses/replace-lost', { licenseId, createdByUserId: CURRENT_USER_ID }),
  replaceDamaged: (licenseId: number) =>
    request<License>('POST', '/licenses/replace-damaged', {
      licenseId,
      createdByUserId: CURRENT_USER_ID,
    }),
  detain: (licenseId: number, fineFees: number) =>
    request<DetainedLicense>('POST', '/licenses/detain', {
      licenseId,
      fineFees,
      createdByUserId: CURRENT_USER_ID,
    }),
  release: (licenseId: number) =>
    request<DetainedLicense>('POST', '/licenses/release', {
      licenseId,
      releasedByUserId: CURRENT_USER_ID,
    }),
  international: (localLicenseId: number) =>
    request<InternationalLicense>('POST', '/licenses/international', {
      localLicenseId,
      createdByUserId: CURRENT_USER_ID,
    }),
}

export type Loadable<T> = {
  data: T | undefined
  error: ApiError | undefined
  loading: boolean
  reload: () => void
}

/** Runs `load` on mount and whenever `deps` change; `reload` runs it again. */
export function useApi<T>(load: () => Promise<T>, deps: unknown[]): Loadable<T> {
  const [data, setData] = useState<T>()
  const [error, setError] = useState<ApiError>()
  const [loading, setLoading] = useState(true)
  const [tick, setTick] = useState(0)

  // eslint-disable-next-line react-hooks/exhaustive-deps
  const run = useCallback(load, deps)

  useEffect(() => {
    let live = true
    setLoading(true)
    run()
      .then((value) => {
        if (!live) return
        setData(value)
        setError(undefined)
      })
      .catch((err: unknown) => {
        if (!live) return
        setError(err instanceof ApiError ? err : new ApiError(0, 'Network', String(err)))
      })
      .finally(() => live && setLoading(false))
    return () => {
      live = false
    }
  }, [run, tick])

  return { data, error, loading, reload: () => setTick((n) => n + 1) }
}

// The licensing API has no endpoint that lists licenses, so license ids this
// browser has seen are remembered locally to make them findable again.
const RECENT_KEY = 'dvld.recentLicenses'

export function rememberLicense(id: number) {
  const ids = recentLicenseIds().filter((x) => x !== id)
  localStorage.setItem(RECENT_KEY, JSON.stringify([id, ...ids].slice(0, 12)))
}

export function recentLicenseIds(): number[] {
  try {
    const parsed = JSON.parse(localStorage.getItem(RECENT_KEY) ?? '[]')
    return Array.isArray(parsed) ? parsed.filter((x) => Number.isInteger(x)) : []
  } catch {
    return []
  }
}

export function forgetLicense(id: number) {
  localStorage.setItem(RECENT_KEY, JSON.stringify(recentLicenseIds().filter((x) => x !== id)))
}
