import { Route, Routes } from 'react-router-dom'
import { Shell } from './components/Shell'
import { ApplicationDetail } from './pages/ApplicationDetail'
import { Applications } from './pages/Applications'
import { DriverLicenses } from './pages/DriverLicenses'
import { LicenseClasses } from './pages/LicenseClasses'
import { LicenseDetail } from './pages/LicenseDetail'
import { Licenses } from './pages/Licenses'
import { NewApplication } from './pages/NewApplication'
import { Overview } from './pages/Overview'
import { People } from './pages/People'
import { PersonDetail } from './pages/PersonDetail'
import { PersonForm } from './pages/PersonForm'

export default function App() {
  return (
    <Routes>
      <Route element={<Shell />}>
        <Route index element={<Overview />} />
        <Route path="people" element={<People />} />
        <Route path="people/new" element={<PersonForm />} />
        <Route path="people/:id" element={<PersonDetail />} />
        <Route path="people/:id/edit" element={<PersonForm />} />
        <Route path="applications" element={<Applications />} />
        <Route path="applications/new" element={<NewApplication />} />
        <Route path="applications/:id" element={<ApplicationDetail />} />
        <Route path="licenses" element={<Licenses />} />
        <Route path="licenses/:id" element={<LicenseDetail />} />
        <Route path="drivers/:id" element={<DriverLicenses />} />
        <Route path="classes" element={<LicenseClasses />} />
        <Route path="*" element={<Overview />} />
      </Route>
    </Routes>
  )
}
