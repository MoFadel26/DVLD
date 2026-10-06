import { render, screen } from '@testing-library/react'
import { describe, expect, it } from 'vitest'
import { I18nProvider } from './components/I18nProvider'
import { useI18n } from './i18n'

function Probe() {
  const { t, money, className, country } = useI18n()
  return (
    <ul>
      <li data-testid="vars">{t('people.count', { n: 3 })}</li>
      <li data-testid="money">{money(20)}</li>
      <li data-testid="class">{className(3, 'Class 3 - Standard Driving License (Car License)')}</li>
      <li data-testid="country">{country('Jordan')}</li>
      <li data-testid="unknown-country">{country('Narnia')}</li>
    </ul>
  )
}

function renderIn(lang: 'en' | 'ar') {
  localStorage.setItem('dvld.lang', lang)
  render(
    <I18nProvider>
      <Probe />
    </I18nProvider>,
  )
}

describe('i18n', () => {
  it('fills placeholders and keeps API names in English', () => {
    renderIn('en')
    expect(screen.getByTestId('vars')).toHaveTextContent('3 people')
    expect(screen.getByTestId('money')).toHaveTextContent('$20.00')
    expect(screen.getByTestId('class')).toHaveTextContent('Class 3 - Standard Driving License (Car License)')
    expect(screen.getByTestId('country')).toHaveTextContent('Jordan')
    expect(document.documentElement).toHaveAttribute('dir', 'ltr')
  })

  it('switches to Arabic, right to left, with translated names and the same money format', () => {
    renderIn('ar')
    expect(screen.getByTestId('vars')).toHaveTextContent('3 أشخاص')
    expect(screen.getByTestId('money')).toHaveTextContent('$20.00')
    expect(screen.getByTestId('class')).toHaveTextContent('الفئة 3 - رخصة قيادة عادية (سيارة)')
    expect(screen.getByTestId('country')).toHaveTextContent('الأردن')
    expect(screen.getByTestId('unknown-country')).toHaveTextContent('Narnia')
    expect(document.documentElement).toHaveAttribute('dir', 'rtl')
    expect(document.documentElement).toHaveAttribute('lang', 'ar')
  })
})
