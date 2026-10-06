import { useCallback, useEffect, useState } from 'react'

export type User = {
  userId: number
  username: string
}

export type Session = {
  token: string
  expiresAt: string
  user: User
}

const SESSION_KEY = 'dvld.session'

/** Fired when the API rejects the stored token, so the app can return to the sign-in page. */
export const SESSION_EXPIRED_EVENT = 'dvld:session-expired'

export function getSession(): Session | null {
  try {
    const session = JSON.parse(localStorage.getItem(SESSION_KEY) ?? 'null') as Session | null
    return session && new Date(session.expiresAt) > new Date() ? session : null
  } catch {
    return null
  }
}

export function setSession(session: Session | null) {
  if (session) localStorage.setItem(SESSION_KEY, JSON.stringify(session))
  else localStorage.removeItem(SESSION_KEY)
}

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
  testResult: 'Pass' | 'Fail' | null
  resultNotes: string | null
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

export type Driver = {
  driverId: number
  personId: number
  fullName: string
  nationalNo: string
  createdDate: string
  licenseCount: number
  activeLicenseCount: number
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
  const headers: Record<string, string> = {}
  if (body !== undefined) headers['Content-Type'] = 'application/json'
  const session = getSession()
  if (session) headers.Authorization = `Bearer ${session.token}`

  let res: Response
  try {
    res = await fetch(`/api${path}`, {
      method,
      headers,
      body: body === undefined ? undefined : JSON.stringify(body),
    })
  } catch {
    throw new ApiError(0, 'Network', '')
  }

  if (res.status === 401 && path !== '/auth/login') {
    setSession(null)
    window.dispatchEvent(new Event(SESSION_EXPIRED_EVENT))
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
  login: (username: string, password: string) => request<Session>('POST', '/auth/login', { username, password }),
  logout: () => request<void>('POST', '/auth/logout'),

  countries: () => request<Country[]>('GET', '/countries'),
  people: () => request<Person[]>('GET', '/people'),
  person: (id: number) => request<Person>('GET', `/people/${id}`),
  personByNationalNo: (no: string) =>
    request<Person>('GET', `/people/by-national-no/${encodeURIComponent(no)}`),
  createPerson: (input: PersonInput & { nationalNo: string }) =>
    request<Person>('POST', '/people', input),
  updatePerson: (id: number, input: PersonInput) => request<Person>('PUT', `/people/${id}`, input),
  deletePerson: (id: number) => request<void>('DELETE', `/people/${id}`),
  personLicenses: (id: number) => request<License[]>('GET', `/people/${id}/licenses`),

  drivers: () => request<Driver[]>('GET', '/drivers'),
  driver: (id: number) => request<Driver>('GET', `/drivers/${id}`),

  applications: () => request<LocalApplication[]>('GET', '/applications/local-license'),
  application: (id: number) => request<LocalApplication>('GET', `/applications/local-license/${id}`),
  createApplication: (personId: number, licenseClassId: number) =>
    request<LocalApplication>('POST', '/applications/local-license', {
      applicantPersonId: personId,
      licenseClassId,
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
    }),
  takeTest: (testType: TestType, appointmentId: number, pass: boolean, notes: string) =>
    request<TestResult>('POST', `/tests/${testType}/take`, {
      testAppointmentId: appointmentId,
      testResult: pass ? 1 : 0,
      notes: notes || null,
    }),

  licenseClasses: () => request<LicenseClass[]>('GET', '/license-classes'),

  licenses: () => request<License[]>('GET', '/licenses'),
  license: (id: number) => request<License>('GET', `/licenses/${id}`),
  driverLicenses: (driverId: number) => request<License[]>('GET', `/licenses/driver/${driverId}`),
  issueFirstTime: (localAppId: number, notes: string) =>
    request<License>('POST', '/licenses/issue-first-time', {
      localDrivingLicenseApplicationId: localAppId,
      notes: notes || null,
    }),
  renew: (licenseId: number, notes: string) =>
    request<License>('POST', '/licenses/renew', {
      licenseId,
      notes: notes || null,
    }),
  replaceLost: (licenseId: number) =>
    request<License>('POST', '/licenses/replace-lost', { licenseId }),
  replaceDamaged: (licenseId: number) =>
    request<License>('POST', '/licenses/replace-damaged', {
      licenseId,
    }),
  detain: (licenseId: number, fineFees: number) =>
    request<DetainedLicense>('POST', '/licenses/detain', {
      licenseId,
      fineFees,
    }),
  release: (licenseId: number) =>
    request<DetainedLicense>('POST', '/licenses/release', {
      licenseId,
    }),
  international: (localLicenseId: number) =>
    request<InternationalLicense>('POST', '/licenses/international', {
      localLicenseId,
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
