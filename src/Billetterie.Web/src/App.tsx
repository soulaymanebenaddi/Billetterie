import { Route, Routes } from 'react-router'
import HomePage from './pages/HomePage'
import EventDetailsPage from './pages/EventDetailsPage'
import './App.css'

function App() {
  return (
    <Routes>
      <Route path="/" element={<HomePage />} />
      <Route path="/events/:id" element={<EventDetailsPage />} />
      <Route path="*" element={<main><h1>Page introuvable</h1></main>} />
    </Routes>
  )
}

export default App
