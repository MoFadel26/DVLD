import { Route, Routes } from 'react-router-dom'
import { Shell } from './components/Shell'
import { Applications } from './pages/Applications'
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
        <Route path="*" element={<Overview />} />
      </Route>
    </Routes>
  )
}
