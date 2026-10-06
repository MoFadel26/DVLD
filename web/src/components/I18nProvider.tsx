import { useEffect, useMemo, useState, type ReactNode } from 'react'
import { ar, classDescriptionsAr, classNamesAr, countryNamesAr, en, I18nContext, type I18n, type Lang } from '../i18n'

const LANG_KEY = 'dvld.lang'

export function I18nProvider({ children }: { children: ReactNode }) {
  const [lang, setLang] = useState<Lang>(() =>
    localStorage.getItem(LANG_KEY) === 'ar' ? 'ar' : 'en',
  )

  useEffect(() => {
    document.documentElement.lang = lang
    document.documentElement.dir = lang === 'ar' ? 'rtl' : 'ltr'
    localStorage.setItem(LANG_KEY, lang)
  }, [lang])

  const value = useMemo<I18n>(() => {
    const dict = lang === 'ar' ? ar : en
    // Latin digits in both languages so ids, national numbers, and fees read the same.
    const locale = lang === 'ar' ? 'ar-JO-u-nu-latn' : 'en-GB'
    const dateFmt = new Intl.DateTimeFormat(locale, { day: 'numeric', month: 'short', year: 'numeric' })
    const dateTimeFmt = new Intl.DateTimeFormat(locale, {
      day: 'numeric',
      month: 'short',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    })
    // Arabic locales spell the currency "US$"; both languages show "$20.00" instead.
    const moneyFmt = new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' })

    return {
      lang,
      setLang,
      t: (key, vars) => {
        let s = dict[key] ?? en[key] ?? key
        if (vars) for (const [k, v] of Object.entries(vars)) s = s.replaceAll(`{${k}}`, String(v))
        return s
      },
      date: (iso) => dateFmt.format(new Date(iso)),
      dateTime: (iso) => dateTimeFmt.format(new Date(iso)),
      money: (n) => moneyFmt.format(n),
      className: (id, fallback) => (lang === 'ar' ? (classNamesAr[id] ?? fallback) : fallback),
      classDescription: (id, fallback) =>
        lang === 'ar' ? (classDescriptionsAr[id] ?? fallback) : fallback,
      country: (name) => (name && lang === 'ar' ? (countryNamesAr[name] ?? name) : (name ?? '')),
    }
  }, [lang])

  return <I18nContext.Provider value={value}>{children}</I18nContext.Provider>
}
